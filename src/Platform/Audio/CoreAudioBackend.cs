using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if THSSS_MACOS || THSSS_COREAUDIO_TEST
namespace THSSS.Audio;

/// <summary>AudioQueue output; callbacks signal buffer acquisition, not audible completion.</summary>
internal sealed unsafe class CoreAudioBackend:IAudioBackend
{
    private readonly HashSet<Source> _sources=new();private bool _disposed;
    public string Name=>"coreaudio";
    public CoreAudioBackend(){
#if !THSSS_COREAUDIO_TEST
        if(!OperatingSystem.IsMacOS())throw new PlatformNotSupportedException("CoreAudio requires macOS.");
#endif
    }
    public IAudioSource CreateSource(int rate){ObjectDisposedException.ThrowIf(_disposed,this);var s=new Source(this,rate);_sources.Add(s);return s;}
    public void Dispose(){if(_disposed)return;foreach(var s in new List<Source>(_sources))s.Dispose();_disposed=true;}
    private const string Library="/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox";
    [StructLayout(LayoutKind.Sequential)]internal struct StreamDescription{public double SampleRate;public uint FormatId,FormatFlags,BytesPerPacket,FramesPerPacket,BytesPerFrame,ChannelsPerFrame,BitsPerChannel,Reserved;}
    [StructLayout(LayoutKind.Sequential)]internal struct QueueBuffer{public uint Capacity;public nint AudioData;public uint Size;public nint UserData;public uint PacketCapacity;public nint Packets;public uint PacketCount;}
    [StructLayout(LayoutKind.Sequential)]internal struct TimeStamp{public double SampleTime;public ulong HostTime;public double RateScalar;public ulong WordClockTime;public short Subframes,SubframeDivisor;public uint Counter,Type,SmpteFlags;public short Hours,Minutes,Seconds,Frames;public uint Flags,Reserved;}
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]private delegate void OutputCallback(nint context,nint queue,nint buffer);
    [DllImport(Library)]private static extern int AudioQueueNewOutput(ref StreamDescription format,OutputCallback callback,nint context,nint runLoop,nint mode,uint flags,out nint queue);
    [DllImport(Library)]private static extern int AudioQueueAllocateBuffer(nint queue,uint size,out nint buffer);
    [DllImport(Library)]private static extern int AudioQueueEnqueueBuffer(nint queue,nint buffer,uint packets,nint descriptions);
    [DllImport(Library)]private static extern int AudioQueueFreeBuffer(nint queue,nint buffer);
    [DllImport(Library)]private static extern int AudioQueueStart(nint queue,nint time);
    [DllImport(Library)]private static extern int AudioQueuePause(nint queue);
    [DllImport(Library)]private static extern int AudioQueueStop(nint queue,byte immediately);
    [DllImport(Library)]private static extern int AudioQueueDispose(nint queue,byte immediately);
    [DllImport(Library)]private static extern int AudioQueueSetParameter(nint queue,uint parameter,float value);
    [DllImport(Library)]private static extern int AudioQueueGetCurrentTime(nint queue,nint timeline,out TimeStamp time,out byte discontinuity);
    private static void Check(int s,string op){if(s!=0)throw new InvalidOperationException($"CoreAudio {op} failed (OSStatus={s}).");}
    private sealed class Chunk{internal nint Buffer;internal int Frames;internal long End;internal bool Acquired;}
    private sealed class Source:IAudioSource
    {
        private static readonly bool TraceInput = Environment.GetEnvironmentVariable("THSSS_DIAG_INPUT") == "1";
        private readonly CoreAudioBackend _owner;private readonly object _sync=new();
        private readonly List<Chunk> _chunks=new();private readonly OutputCallback _callback;private readonly int _rate;
        private nint _queue;private bool _playing,_disposed;private long _retired,_submitted,_played;private double _epoch;
        internal Source(CoreAudioBackend owner,int rate)
        {
            if(rate<=0)throw new ArgumentOutOfRangeException(nameof(rate));_owner=owner;_rate=rate;_callback=OnOutput;
            var f=new StreamDescription{SampleRate=rate,FormatId=0x6c70636d,FormatFlags=9,BytesPerPacket=8,FramesPerPacket=1,BytesPerFrame=8,ChannelsPerFrame=2,BitsPerChannel=32};
            Check(AudioQueueNewOutput(ref f,_callback,0,0,0,0,out _queue),"NewOutput");
        }
        private void Alive()=>ObjectDisposedException.ThrowIf(_disposed,this);
        private void OnOutput(nint context,nint queue,nint buffer){lock(_sync){if(_disposed)return;foreach(var c in _chunks)if(c.Buffer==buffer){c.Acquired=true;break;}}}
        private long Played()
        {
            if(!_playing)return _played;
            int status=AudioQueueGetCurrentTime(_queue,0,out var ts,out _);
            if(status==0&&(ts.Flags&1)!=0)_played=Math.Clamp(Math.Max(_played,(long)(ts.SampleTime-_epoch)),0,_submitted);
            return _played;
        }
        public void Submit(float[] samples,int rate)
        {
            Alive();if(rate!=_rate||samples.Length==0||samples.Length%2!=0)throw new ArgumentException("Expected stereo frames at source rate.");
            Check(AudioQueueAllocateBuffer(_queue,checked((uint)samples.Length*4),out nint buffer),"AllocateBuffer");
            var c=new Chunk{Buffer=buffer,Frames=samples.Length/2};
            try
            {
                var h=(QueueBuffer*)buffer;Marshal.Copy(samples,0,h->AudioData,samples.Length);h->Size=checked((uint)samples.Length*4);
                lock(_sync){c.End=_submitted+c.Frames;_chunks.Add(c);}
                Check(AudioQueueEnqueueBuffer(_queue,buffer,0,0),"EnqueueBuffer");
                lock(_sync)_submitted=c.End;
            }
            catch{lock(_sync)_chunks.Remove(c);AudioQueueFreeBuffer(_queue,buffer);throw;}
        }
        public int ProcessedBuffers{get{Alive();lock(_sync){long p=Played();int n=0;foreach(var c in _chunks){if(!c.Acquired||c.End>p)break;n++;}return n;}}}
        public void UnqueueProcessed()
        {
            Alive();Chunk c;lock(_sync){if(_chunks.Count==0||!_chunks[0].Acquired||_chunks[0].End>Played())throw new InvalidOperationException("No completed CoreAudio buffer.");c=_chunks[0];_chunks.RemoveAt(0);_retired+=c.Frames;}
            Check(AudioQueueFreeBuffer(_queue,c.Buffer),"FreeBuffer");
        }
        public int SampleOffset{get{Alive();lock(_sync)return checked((int)Math.Max(0,Played()-_retired));}}
        public bool IsPlaying{get{Alive();lock(_sync)return _playing&&Played()<_submitted;}}
        public void Play()
        {
            Alive();
            long start = TraceInput ? Stopwatch.GetTimestamp() : 0;
            Check(AudioQueueStart(_queue,0),"Start");
            lock(_sync)_playing=true;
            TraceTiming("Start", start);
        }
        public void Pause()
        {
            Alive();
            long start = TraceInput ? Stopwatch.GetTimestamp() : 0;
            lock(_sync)Played();
            Check(AudioQueuePause(_queue),"Pause");
            lock(_sync){Played();_playing=false;}
            TraceTiming("Pause", start);
        }
        public void SetGain(float gain){Alive();if(!float.IsFinite(gain)||gain<0)throw new ArgumentOutOfRangeException(nameof(gain));Check(AudioQueueSetParameter(_queue,1,gain),"SetVolume");}
        public void Clear()
        {
            Alive();
            long start = TraceInput ? Stopwatch.GetTimestamp() : 0;
            // AudioQueueStop already calls AudioQueueReset; do not reset it twice.
            Check(AudioQueueStop(_queue,1),"Stop");
            lock(_sync){foreach(var c in _chunks)Check(AudioQueueFreeBuffer(_queue,c.Buffer),"FreeBuffer");_chunks.Clear();_retired=_submitted=_played=0;_playing=false;_epoch=AudioQueueGetCurrentTime(_queue,0,out var ts,out _)==0?ts.SampleTime:0;}
            TraceTiming("Clear", start);
        }
        private static void TraceTiming(string operation, long start)
        {
            if (TraceInput)
                Console.Error.WriteLine($"[AUDIO-TIMING] op={operation} ms={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F2}");
        }
        public void Dispose(){if(_disposed)return;Check(AudioQueueDispose(_queue,1),"Dispose");lock(_sync){_disposed=true;_chunks.Clear();_queue=0;}_owner._sources.Remove(this);GC.KeepAlive(_callback);}
    }
}
#endif
