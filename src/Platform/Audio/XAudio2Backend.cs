using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if THSSS_WINDOWS
namespace THSSS.Audio;

internal sealed unsafe class XAudio2Backend : IAudioBackend
{
    private readonly HashSet<Source> _sources = new();
    private nint _engine;
    private nint _master;
    private nint _comUsage;
    private bool _disposed;
    public string Name => "xaudio2";

    public XAudio2Backend()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("XAudio2 2.9 requires Windows 10/11.");
        try
        {
            // Keep COM's MTA alive without tying disposal to the creating thread.
            Check(CoIncrementMTAUsage(out _comUsage), "CoIncrementMTAUsage");
            Check(XAudio2Create(out _engine, 0, 0xffffffff), "XAudio2Create (xaudio2_9.dll)");
            nint master = 0;
            Check(((delegate* unmanaged[Stdcall]<nint, nint*, uint, uint, uint, nint, nint, int, int>)
                Vtable(_engine)[7])(_engine, &master, 2, 0, 0, 0, 0, 6), "CreateMasteringVoice");
            _master = master;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public IAudioSource CreateSource(int sampleRate)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (sampleRate is < 1000 or > 200000)
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "XAudio2 sample rate must be 1000–200000 Hz.");
        var source = new Source(this, sampleRate);
        try { _sources.Add(source); }
        catch { source.Dispose(); throw; }
        return source;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        while (_sources.Count > 0)
        {
            using var enumerator = _sources.GetEnumerator();
            enumerator.MoveNext();
            enumerator.Current.Dispose();
        }
        DestroyVoice(ref _master);
        if (_engine != 0)
        {
            ((delegate* unmanaged[Stdcall]<nint, uint>)Vtable(_engine)[2])(_engine);
            _engine = 0;
        }
        if (_comUsage != 0)
        {
            int result = CoDecrementMTAUsage(_comUsage);
            _comUsage = 0;
            Check(result, "CoDecrementMTAUsage");
        }
    }

    private nint CreateVoice(int sampleRate)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var format = new WaveFormat
        {
            FormatTag = 3, Channels = 2, SamplesPerSecond = (uint)sampleRate,
            AverageBytesPerSecond = checked((uint)sampleRate * 8), BlockAlign = 8, BitsPerSample = 32
        };
        nint voice = 0;
        Check(((delegate* unmanaged[Stdcall]<nint, nint*, WaveFormat*, uint, float, nint, nint, nint, int>)
            Vtable(_engine)[5])(_engine, &voice, &format, 0, 1f, 0, 0, 0), "CreateSourceVoice");
        return voice;
    }

    private static nint* Vtable(nint instance) => *(nint**)instance;

    private static void DestroyVoice(ref nint voice)
    {
        if (voice == 0) return;
        // IXAudio2Voice is NOT IUnknown: DestroyVoice is slot 18, not Release.
        ((delegate* unmanaged[Stdcall]<nint, void>)Vtable(voice)[18])(voice);
        voice = 0;
    }

    private static void Check(int result, string operation)
    {
        if (result < 0)
            throw new ExternalException($"XAudio2 {operation} failed (HRESULT 0x{result:X8}).", result);
    }

    private sealed class Source : IAudioSource
    {
        private readonly XAudio2Backend _owner;
        private readonly int _sampleRate;
        private readonly Queue<Chunk> _chunks = new();
        private nint _voice;
        private ulong _unqueuedFrames;
        private float _gain = 1f;
        private bool _started;
        private bool _disposed;

        public Source(XAudio2Backend owner, int sampleRate)
        {
            _owner = owner;
            _sampleRate = sampleRate;
            _voice = owner.CreateVoice(sampleRate);
        }

        public int ProcessedBuffers => _chunks.Count - checked((int)GetState().BuffersQueued);

        public int SampleOffset
        {
            get
            {
                VoiceState state = GetState();
                // Include completed chunks until the shared pump explicitly retires them.
                ulong retainedFrames = 0;
                foreach (Chunk chunk in _chunks) retainedFrames += (uint)chunk.Frames;
                ulong offset = state.SamplesPlayed > _unqueuedFrames ? state.SamplesPlayed - _unqueuedFrames : 0;
                return checked((int)Math.Min(Math.Min(offset, retainedFrames), (ulong)int.MaxValue));
            }
        }

        public bool IsPlaying => GetState().BuffersQueued > 0 && _started;

        public void Submit(float[] stereoSamples, int sampleRate)
        {
            EnsureUsable();
            ArgumentNullException.ThrowIfNull(stereoSamples);
            if (sampleRate != _sampleRate)
                throw new ArgumentException("A native XAudio2 source has a fixed sample rate.", nameof(sampleRate));
            if (stereoSamples.Length == 0 || stereoSamples.Length % 2 != 0)
                throw new ArgumentException("XAudio2 requires nonempty, interleaved stereo float32 frames.", nameof(stereoSamples));
            int bytes = checked(stereoSamples.Length * sizeof(float));
            _chunks.EnsureCapacity(_chunks.Count + 1);
            var chunk = new Chunk { Frames = stereoSamples.Length / 2 };
            chunk.Data = Marshal.AllocHGlobal(bytes);
            try
            {
                fixed (float* samples = stereoSamples)
                    Buffer.MemoryCopy(samples, (void*)chunk.Data, bytes, bytes);
                // No END_OF_STREAM: SamplesPlayed stays cumulative across chunks and underruns.
                var buffer = new NativeBuffer { AudioBytes = (uint)bytes, AudioData = chunk.Data };
                Check(((delegate* unmanaged[Stdcall]<nint, NativeBuffer*, nint, int>)
                    Vtable(_voice)[21])(_voice, &buffer, 0), "SubmitSourceBuffer");
            }
            catch { Marshal.FreeHGlobal(chunk.Data); throw; }
            _chunks.Enqueue(chunk);
        }

        public void UnqueueProcessed()
        {
            if (ProcessedBuffers <= 0)
                throw new InvalidOperationException("XAudio2 has no processed buffer to unqueue.");
            Chunk chunk = _chunks.Dequeue();
            _unqueuedFrames += (uint)chunk.Frames;
            Marshal.FreeHGlobal(chunk.Data);
        }

        public void Play()
        {
            EnsureUsable();
            Check(((delegate* unmanaged[Stdcall]<nint, uint, uint, int>)Vtable(_voice)[19])(_voice, 0, 0), "Start");
            _started = true;
        }

        public void Pause()
        {
            EnsureUsable();
            Check(((delegate* unmanaged[Stdcall]<nint, uint, uint, int>)Vtable(_voice)[20])(_voice, 0, 0), "Stop");
            _started = false;
        }

        public void Clear()
        {
            EnsureUsable();
            try
            {
                Pause();
                Check(((delegate* unmanaged[Stdcall]<nint, int>)Vtable(_voice)[22])(_voice), "FlushSourceBuffers");
            }
            finally
            {
                // DestroyVoice waits for native processing; Flush alone need not retire the current buffer.
                ReleaseVoice();
            }
            _voice = _owner.CreateVoice(_sampleRate);
            SetGain(_gain);
        }

        public void SetGain(float gain)
        {
            EnsureUsable();
            if (!float.IsFinite(gain) || gain is < 0 or > 16777216f)
                throw new ArgumentOutOfRangeException(nameof(gain), "XAudio2 gain must be finite and between 0 and 16777216.");
            Check(((delegate* unmanaged[Stdcall]<nint, float, uint, int>)Vtable(_voice)[12])(_voice, gain, 0), "SetVolume");
            _gain = gain;
        }

        private VoiceState GetState()
        {
            EnsureUsable();
            VoiceState state = default;
            ((delegate* unmanaged[Stdcall]<nint, VoiceState*, uint, void>)Vtable(_voice)[25])(_voice, &state, 0);
            return state;
        }

        private void EnsureUsable()
        {
            ObjectDisposedException.ThrowIf(_disposed || _owner._disposed, this);
            if (_voice == 0)
                throw new InvalidOperationException("XAudio2 source is unavailable after a failed voice reset; dispose and recreate it.");
        }

        private void ReleaseVoice()
        {
            DestroyVoice(ref _voice);
            while (_chunks.Count > 0) Marshal.FreeHGlobal(_chunks.Dequeue().Data);
            _unqueuedFrames = 0;
            _started = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            ReleaseVoice();
            _owner._sources.Remove(this);
        }

        private sealed class Chunk
        {
            public nint Data;
            public int Frames;
        }
    }

    // XAudio2 2.9 ABI: xaudio2.h uses #pragma pack(push,1), including VOICE_STATE on x64.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WaveFormat
    {
        public ushort FormatTag, Channels;
        public uint SamplesPerSecond, AverageBytesPerSecond;
        public ushort BlockAlign, BitsPerSample, ExtraSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct NativeBuffer
    {
        public uint Flags, AudioBytes;
        public nint AudioData;
        public uint PlayBegin, PlayLength, LoopBegin, LoopLength, LoopCount;
        public nint Context;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VoiceState
    {
        public nint CurrentBufferContext;
        public uint BuffersQueued;
        public ulong SamplesPlayed;
    }

    [DllImport("xaudio2_9.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int XAudio2Create(out nint engine, uint flags, uint processor);

    [DllImport("ole32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int CoIncrementMTAUsage(out nint cookie);

    [DllImport("ole32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int CoDecrementMTAUsage(nint cookie);
}

#endif
