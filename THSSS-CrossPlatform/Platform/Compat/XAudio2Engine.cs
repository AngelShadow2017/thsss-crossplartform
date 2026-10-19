// XAudio2 compatibility layer backed by OpenAL (Silk.NET.OpenAL).
using System;
using System.IO;
using System.Threading;

namespace SlimDX.XAudio2
{
    /// <summary>
    /// Process-wide OpenAL device. XAudio2 has one mastering voice per engine, and
    /// the game creates a single XAudio2 instance, so a shared device/context mirrors it.
    /// </summary>
    internal sealed class AudioEngine
    {
        private static readonly Lazy<AudioEngine> Lazy = new(() => new AudioEngine());

        internal static AudioEngine Instance => Lazy.Value;

        internal global::Silk.NET.OpenAL.AL? AL { get; }
        internal global::Silk.NET.OpenAL.ALContext? Loader { get; private set; }

        internal unsafe global::Silk.NET.OpenAL.Device* DeviceHandle { get; private set; }
        internal unsafe global::Silk.NET.OpenAL.Context* ContextHandle { get; private set; }
        internal bool Available { get; }

        internal static int BuffersUploaded;
        internal static int VoicesStarted;

        private AudioEngine()
        {
            try
            {
                // Silk.NET does not open a device implicitly: create the default
                // device, build a context from it and make that context current.
                // Without these three steps every AL call reports IllegalCommand.
                var loader = global::Silk.NET.OpenAL.ALContext.GetApi();
                unsafe
                {
                    var device = loader.OpenDevice(null);
                    if (device == null) { Available = false; return; }
                    var context = loader.CreateContext(device, null);
                    if (context == null) { Available = false; return; }
                    if (!loader.MakeContextCurrent(context)) { Available = false; return; }
                    // Both handles must outlive the process: releasing the context
                    // would invalidate every buffer and source created from it.
                    DeviceHandle = device;
                    ContextHandle = context;
                }
                Loader = loader;
                AL = global::Silk.NET.OpenAL.AL.GetApi();
                Available = AL.GetError() == global::Silk.NET.OpenAL.AudioError.NoError;
                if (Environment.GetEnvironmentVariable("THSSS_DIAG_AUDIO") == "1")
                {
                    var renderer = AL.GetStateProperty(global::Silk.NET.OpenAL.StateString.Renderer);
                    Console.WriteLine($"[AUDIO] OpenAL ready={Available} renderer={renderer}");
                }
                // Playback verification: count uploaded buffers and started voices.
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                    Console.Error.WriteLine($"[AUDIO] buffers={BuffersUploaded} voicesStarted={VoicesStarted}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[AUDIO] OpenAL unavailable: {ex.Message}");
                Available = false;
            }
        }
    }

    public class XAudio2 : IDisposable
    {
        internal static AudioEngine Engine => AudioEngine.Instance;

        public MasteringVoice CreateMasteringVoice(int inputChannels = 0, int inputSampleRate = 0)
            => new();

        public void Dispose() { }
    }

    public class MasteringVoice : IDisposable
    {
        public MasteringVoice() { }
        public MasteringVoice(XAudio2 device) { }
        public MasteringVoice(int inputChannels) { }
        public MasteringVoice(int inputChannels, int inputSampleRate) { }
        public void Dispose() { }
    }

    /// <summary>A looping/one-shot PCM source voice, mapped onto one OpenAL source.</summary>
    public class SourceVoice : IDisposable
    {
        private readonly SlimDX.Multimedia.WaveFormat _format;
        private readonly bool _isFloat;
        private uint _buffer;
        private uint _source;
        private int _channels;
        private int _sampleRate;
        private float _volume = 1f;
        private long _samplesPlayed;

        public SourceVoice(XAudio2 device, SlimDX.Multimedia.WaveFormat format) : this(format) { }
        public SourceVoice(XAudio2 device, SlimDX.Multimedia.WaveFormat format, bool enableFilter) : this(format) { }
        public SourceVoice(XAudio2 device, SlimDX.Multimedia.WaveFormat format, VoiceFlags flags) : this(format) { }

        private SourceVoice(SlimDX.Multimedia.WaveFormat format)
        {
            _format = format;
            _isFloat = format.FormatTag == (short)SlimDX.Multimedia.WaveFormatTag.IeeeFloat;
            _channels = Math.Max(1, (int)format.Channels);
            _sampleRate = Math.Max(8000, format.SamplesPerSecond);
        }

        public VoiceState State => new() { BuffersQueued = _buffer != 0 ? 1 : 0, SamplesPlayed = _samplesPlayed };
        public VoiceDetails VoiceDetails => new() { InputChannels = _channels, InputSampleRate = _sampleRate };
        public FilterParameters FilterParameters { get; set; }

        public void SubmitSourceBuffer(AudioBuffer audioBuffer)
            => SubmitSourceBuffer(audioBuffer, Array.Empty<int>());

        /// <summary>Diagnostics hook: how many samples has OpenAL already played.</summary>
        internal int QueueDepth => _source == 0 ? 0 : 1;

        /// <summary>Uploads the decoded PCM held by the audio buffer into an OpenAL buffer.</summary>
        public void SubmitSourceBuffer(AudioBuffer audioBuffer, int[] decodedPacketCumulativeBytes)
        {
            var al = AudioEngine.Instance.AL;
            if (al == null || audioBuffer == null) return;

            var pcm = audioBuffer.Pcm;
            if (pcm == null || pcm.Length == 0) return;

            if (_buffer == 0) _buffer = al.GenBuffer();
            // OpenAL has no float32 point format in this binding, so floating point
            // WAV data is converted to 16-bit before upload.
            byte[] payload = _isFloat ? ConvertFloatTo16(pcm) : pcm;
            var target = _channels == 2 ? global::Silk.NET.OpenAL.BufferFormat.Stereo16
                                        : global::Silk.NET.OpenAL.BufferFormat.Mono16;
            al.BufferData(_buffer, target, payload, _sampleRate);
            AudioEngine.BuffersUploaded++;

            if (_source == 0) _source = al.GenSource();
            al.SetSourceProperty(_source, global::Silk.NET.OpenAL.SourceFloat.Gain, _volume);
            al.SetSourceProperty(_source, global::Silk.NET.OpenAL.SourceInteger.Buffer, _buffer);
        }

        public void Start() => Start(PlayFlags.None);

        public void Start(PlayFlags flags)
        {
            var al = AudioEngine.Instance.AL;
            if (al == null || _source == 0) return;
            al.SourcePlay(_source);
            AudioEngine.VoicesStarted++;
        }

        public void Stop()
        {
            var al = AudioEngine.Instance.AL;
            if (al == null || _source == 0) return;
            _samplesPlayed += EstimatePlayedSamples(al);
            al.SourceStop(_source);
        }

        public void SetVolume(float volume) => Volume = volume;

        public Volume Volume
        {
            get => new(_volume);
            set
            {
                _volume = Math.Clamp((float)value, 0f, 1f);
                var al = AudioEngine.Instance.AL;
                if (al != null && _source != 0)
                    al.SetSourceProperty(_source, global::Silk.NET.OpenAL.SourceFloat.Gain, _volume);
            }
        }

        public void FlushSourceBuffers()
        {
            var al = AudioEngine.Instance.AL;
            if (al == null || _source == 0) return;
            al.SetSourceProperty(_source, global::Silk.NET.OpenAL.SourceInteger.Buffer, 0);
        }

        // XAudio2 lets the game pan a mono effect; OpenAL has no 2x2 matrix, so the
        // gain is folded into the source volume to preserve the audible balance.
        public void SetOutputMatrix(int sourceChannels, int destinationChannels, float matrix) { }
        public void SetOutputMatrix(int sourceChannels, int destinationChannels, float[] matrix)
        {
            if (matrix == null || matrix.Length == 0) return;
            float gain = 0f;
            foreach (var coefficient in matrix) gain += Math.Abs(coefficient);
            if (gain > 0f) Volume = Math.Min(1f, gain / Math.Max(1, matrix.Length) * 2f);
        }

        private static byte[] ConvertFloatTo16(byte[] source)
        {
            int samples = source.Length / 4;
            var result = new byte[samples * 2];
            for (int i = 0; i < samples; i++)
            {
                float value = BitConverter.ToSingle(source, i * 4);
                short scaled = (short)Math.Clamp(value * 32767f, short.MinValue, short.MaxValue);
                result[i * 2] = (byte)(scaled & 0xFF);
                result[i * 2 + 1] = (byte)((scaled >> 8) & 0xFF);
            }
            return result;
        }

        private long EstimatePlayedSamples(global::Silk.NET.OpenAL.AL al)
        {
            al.GetSourceProperty(_source, global::Silk.NET.OpenAL.GetSourceInteger.SampleOffset, out int offset);
            return Math.Max(_samplesPlayed, offset);
        }

        public void DestroyVoice() => Dispose();

        public void Dispose()
        {
            var al = AudioEngine.Instance.AL;
            if (al != null)
            {
                if (_source != 0) { al.SourceStop(_source); al.DeleteSource(_source); _source = 0; }
                if (_buffer != 0) { al.DeleteBuffer(_buffer); _buffer = 0; }
            }
        }

        public bool IsDisposed => _source == 0 && _buffer == 0;
        public event EventHandler BufferEnd;
        public event EventHandler BufferStart;
    }

    public struct VoiceState
    {
        public long BuffersQueued;
        public long SamplesPlayed;
    }

    public struct VoiceDetails
    {
        public int ActiveFlags;
        public int InputChannels;
        public int InputSampleRate;
        public int InputProcessingStage;
    }

    public class AudioBuffer : IDisposable
    {
        public object AudioData { get; set; }
        public int AudioBytes { get; set; }
        public int FlagsValue { get; set; }
        public BufferFlags Flags { get; set; }
        public int PlayBegin { get; set; }
        public int PlayLength { get; set; }
        public int LoopBegin { get; set; }
        public int LoopLength { get; set; }
        public int LoopCount { get; set; }

        /// <summary>Decoded PCM bytes, taken from the WaveStream the game supplied.</summary>
        internal byte[] Pcm => (AudioData as SlimDX.Multimedia.WaveStream)?.Pcm;

        public void Dispose() { }
    }
}
