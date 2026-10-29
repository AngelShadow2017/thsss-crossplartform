using System.Text;
using System.Diagnostics;
using SlimDX.Multimedia;
using SlimDX.XAudio2;
using THSSS.Audio;
using System.Reflection;

// Backend compatibility contract: the single shared SlimDX.XAudio2-shaped layer must behave the
// same regardless of which per-OS transport (XAudio2 on Windows, AudioQueue on macOS, OpenAL on
// Linux) sits underneath. Everything here asserts logical voice behaviour; timing waits are
// generous so a software device (null/ALSoft) passes and a real device is not required.
//
// --selection-probe is a separate process mode used by the shell runners: it prints the resolved
// backend and maps the two selection failures to distinct exit codes (2 = unknown name,
// 3 = backend not available on this OS), so the script can assert the error class without
// parsing prose.
if(args.Contains("--selection-probe"))
{
    try { Console.WriteLine(AudioBackendSelection.Resolve(Environment.GetEnvironmentVariable("THSSS_AUDIO_BACKEND"))); }
    catch(ArgumentException ex) { Console.Error.WriteLine(ex.GetType().Name+": "+ex.Message);Environment.ExitCode=2; }
    catch(PlatformNotSupportedException ex) { Console.Error.WriteLine(ex.GetType().Name+": "+ex.Message);Environment.ExitCode=3; }
    return;
}

// Assertion helpers; a failure throws and the process exits non-zero. `checks` is printed at the
// end so a silently-truncated run (e.g. an early return) shows up as a smaller count.
int checks=0;
void Check(bool condition,string message) { if(!condition)throw new Exception(message);checks++; }
void Reject<T>(Action action)where T:Exception { try{action();}catch(T){checks++;return;}throw new Exception("Expected "+typeof(T).Name); }

// Fixture audio: stereo 16-bit 22050 Hz (the game's rate) with +0.125 / -0.0625 per channel.
// Two different, exactly representable channel values make matrix/crossfeed errors identifiable
// instead of merely "some sound came out".
WaveStream Wave(int frames)
{
    var m=new MemoryStream();using(var w=new BinaryWriter(m,Encoding.ASCII,true))
    {
        w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(frames*4+36);w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        w.Write(16);w.Write((short)1);w.Write((short)2);w.Write(22050);w.Write(88200);w.Write((short)4);w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data"));w.Write(frames*4);
        for(int i=0;i<frames;i++){w.Write((short)4096);w.Write((short)-2048);}
    }m.Position=0;return new WaveStream(m);
}

// Real devices advance asynchronously, so completion must be polled, never slept on. The 6 s
// ceiling is far above any legitimate playback here; hitting it means a stalled transport, which
// is exactly the failure this suite exists to report.
void Wait(Func<bool> condition,string label)
{
    var clock=Stopwatch.StartNew();while(!condition()){if(clock.ElapsedMilliseconds>6000)throw new Exception("Timeout: "+label);Thread.Sleep(5);}
}

// Selection contract: "auto" follows the OS default (XAudio2 on Windows, AudioQueue on macOS,
// OpenAL elsewhere); an explicit available name is honoured; an unknown name is a hard
// ArgumentException rather than a silent fallback, and a backend absent from this OS is
// PlatformNotSupportedException rather than "some other backend, quietly".
Check(AudioBackendSelection.Resolve("auto")== (OperatingSystem.IsWindows()?"xaudio2":OperatingSystem.IsMacOS()?"coreaudio":"openal"),"OS default");
Check(AudioBackendSelection.Resolve("openal")=="openal","Explicit OpenAL");
Reject<ArgumentException>(()=>AudioBackendSelection.Resolve("typo"));
if(!OperatingSystem.IsWindows())Reject<PlatformNotSupportedException>(()=>AudioBackendSelection.Resolve("xaudio2"));

// One engine per process, as in the game.
using var engine=new XAudio2();
using var wave=Wave(22050);

// Windows-only section: proves the native XAudio2 2.9 transport keeps a *completed* chunk counted
// in SampleOffset until it is explicitly unqueued. That is the real-API behaviour the shared pump
// depends on; if the transport notified and forget, every cursor assertion below would drift on
// Windows while still passing on software drivers.
if(OperatingSystem.IsWindows())
{
    lock(XAudio2.Engine.Sync)
    {
        using var native=XAudio2.Engine.Backend.CreateSource(22050);
        native.Submit(new float[400],22050);native.Play();
        Wait(()=>native.ProcessedBuffers==1,"native completion before shared pump");
        Check(native.SampleOffset==200,"Completed retained chunk remains counted until explicitly unqueued");
        native.UnqueueProcessed();Check(native.SampleOffset==0,"Retired chunk is no longer offset");
    }
}

// Logical voice contract: PlayLength 900 plus LoopCount 2 x LoopLength 200 == 1300 consumed
// frames, with exactly one start/end callback pair for the whole logical submission (not one per
// streaming chunk and not one per loop pass), because the game uses these to advance BGM.
using(var voice=new SourceVoice(engine,wave.Format))
{
    int starts=0,ends=0;voice.BufferStart+=(_,_)=>starts++;voice.BufferEnd+=(_,_)=>ends++;
    voice.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,PlayBegin=100,PlayLength=900,LoopBegin=200,LoopLength=200,LoopCount=2});
    voice.Start();Wait(()=>voice.State.BuffersQueued==0,"finite loop completion");
    Check(voice.State.SamplesPlayed==1300,"Exact intro+loop+tail samples");Check(starts==1&&ends==1,"One logical callback pair");

    // Infinity (LoopCount 255) plus pause/resume: the cursor must freeze and then continue from
    // the same position. The <4 tolerance only absorbs frames the device consumed between Stop
    // and the cursor read, not a rewind. Resuming must add at least 1000 frames (>45 ms at
    // 22050 Hz) which is why the check compares against stopped+1000 rather than stopped alone.
    voice.Stop();voice.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,LoopBegin=300,LoopLength=600,LoopCount=255});voice.Start();
    Wait(()=>voice.State.SamplesPlayed>3000,"cursor advancement");voice.Stop();var stopped=voice.State.SamplesPlayed;Thread.Sleep(120);
    Check(Math.Abs(voice.State.SamplesPlayed-stopped)<4,"Pause cursor stays fixed");voice.Start();Wait(()=>voice.State.SamplesPlayed>stopped+1000,"resume cursor");

    // Gain and matrix stay independent through SetVolume/SetOutputMatrix (the game fades with
    // SetVolume while panning with the matrix), and invalid coefficients/gain are rejected with
    // the exact exception types callers catch.
    voice.SetVolume(0.4f);voice.SetOutputMatrix(2,2,new[]{1f,0.25f,-0.5f,0.75f});
    Check(Math.Abs((float)voice.Volume-0.4f)<0.0001,"Matrix does not overwrite gain");
    Reject<ArgumentException>(()=>voice.SetOutputMatrix(2,2,new[]{float.NaN,0f,0f,1f}));
    Reject<ArgumentOutOfRangeException>(()=>voice.SetVolume(-1));

    // Flush removes an unterminated infinite loop; EndOfStream resets the logical cursor to 0
    // (the original BGM restart-on-end behaviour). Both are used by the game's stop/restart paths.
    voice.FlushSourceBuffers();Check(voice.State.BuffersQueued==0,"Flush infinite loop");
    voice.Stop();voice.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,PlayLength=100,Flags=BufferFlags.EndOfStream});voice.Start();
    Wait(()=>voice.State.BuffersQueued==0,"EOS");Check(voice.State.SamplesPlayed==0,"EOS resets logical cursor");

    // Dispose must be idempotent; any use afterwards is an explicit ObjectDisposedException
    // (the game disposes voices on scene teardown, sometimes twice via finally blocks).
    voice.Dispose();voice.Dispose();Reject<ObjectDisposedException>(()=>voice.Start());
}

// Two submitted buffers stay independently queued and are consumed in order: 200+300 == 500
// cumulative frames with one BufferEnd per logical submission.
using(var ordered=new SourceVoice(engine,wave.Format))
{
    int ended=0;ordered.BufferEnd+=(_,_)=>ended++;
    ordered.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,PlayLength=200});
    ordered.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,PlayBegin=400,PlayLength=300});
    Check(ordered.State.BuffersQueued==2,"Two logical buffers remain separately queued");
    ordered.Start();Wait(()=>ordered.State.BuffersQueued==0,"ordered buffers");
    Check(ordered.State.SamplesPlayed==500&&ended==2,"Queue ordering and cumulative frames");
}

// Live mixing while playing: 15 rapid matrix/gain updates (including mute) must not disturb the
// queue or the advancing cursor. This is the per-frame fade/pan path the game exercises.
using(var live=new SourceVoice(engine,wave.Format))
{
    live.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,LoopBegin=500,LoopLength=1000,LoopCount=255});live.Start();
    Wait(()=>live.State.SamplesPlayed>1000,"live start");
    for(int i=0;i<15;i++) {live.SetOutputMatrix(2,2,new[]{1f,0f,0f,(i%2)*0.5f});live.SetVolume(i%2==0?0:0.5f);}
    long cursor=live.State.SamplesPlayed;Wait(()=>live.State.SamplesPlayed>cursor+1000,"live matrix update keeps cursor advancing");
    Check(live.State.BuffersQueued==1,"Live matrix/gain updates preserve logical loop");
    live.Dispose();
}

// 12 voices started concurrently: contention on the shared pump/Sync must not drop, stall or
// double-complete any of them (the game can run BGM plus a burst of SFX simultaneously).
var voices=Enumerable.Range(0,12).Select(_=>new SourceVoice(engine,wave.Format)).ToArray();
try
{
    foreach(var v in voices){v.SubmitSourceBuffer(new AudioBuffer{AudioData=wave,PlayLength=1000});v.Start();}
    Wait(()=>voices.All(v=>v.State.BuffersQueued==0),"concurrent voices");Check(true,"12 concurrent voices complete");
}finally{foreach(var v in voices)v.Dispose();}

// Transport-failure contract: when the native source throws, the pump must fail the voice so the
// next State read throws InvalidOperationException instead of lying, and Dispose must not try to
// recreate the native source or call Clear (which would throw during cleanup). FaultSource records
// exactly those two forbidden calls so a "helpful" recovery path is caught rather than tolerated.
using(var broken=new SourceVoice(engine,wave.Format))
{
    var field=typeof(SourceVoice).GetField("_output",BindingFlags.NonPublic|BindingFlags.Instance)!;
    ((IAudioSource)field.GetValue(broken)!).Dispose();
    var failing=new FaultSource();field.SetValue(broken,failing);
    lock(XAudio2.Engine.Sync)
        typeof(SourceVoice).GetMethod("PumpSafely",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(broken,null);
    Reject<InvalidOperationException>(()=>{_ = broken.State;});
    broken.Dispose();broken.Dispose();
    Check(failing.Disposals==1&&failing.Clears==0,"Dispose never recreates a native source or calls reset");
}

// Optional real-asset block: when THSSS_AUDIO_ASSET_ROOT points at game data, open an actual BGM
// and SFX file and verify decode+loop+pause/resume against the real formats rather than synthetic
// WAVs. Thresholds are fractions of the file's own sample rate (1/4 s, 1/8 s) so slow machines
// still pass while a stalled cursor cannot.
string assets=Environment.GetEnvironmentVariable("THSSS_AUDIO_ASSET_ROOT");
if(!string.IsNullOrEmpty(assets))
{
    string bgm=Directory.GetFiles(Path.Combine(assets,"BGM"),"*.wav")[0];
    string sound=Directory.GetFiles(Path.Combine(assets,"Sound"),"*.wav",SearchOption.AllDirectories)[0];
    using var music=new WaveStream(bgm);using var effect=new WaveStream(sound);
    using var m=new SourceVoice(engine,music.Format);using var s=new SourceVoice(engine,effect.Format);
    m.SubmitSourceBuffer(new AudioBuffer{AudioData=music,LoopCount=255});m.Start();
    s.SubmitSourceBuffer(new AudioBuffer{AudioData=effect});s.Start();
    Wait(()=>m.State.SamplesPlayed>music.Format.SamplesPerSecond/4,"real BGM advances");
    m.Stop();long paused=m.State.SamplesPlayed;Thread.Sleep(100);
    Check(m.State.SamplesPlayed==paused,"Actual game BGM pause");m.Start();
    Wait(()=>m.State.SamplesPlayed>paused+music.Format.SamplesPerSecond/8,"actual game BGM resume");
    Check(true,"Actual BGM and SFX decoded/queued concurrently");
    Console.WriteLine("PASS real game BGM/SFX assets and pause/resume");
}

// The resolution is printed with the pass line so a run log records which transport actually
// produced these results (a green run against the wrong backend proves nothing).
Console.WriteLine($"PASS AudioBackendCompatibility: {checks} checks backend={AudioBackendSelection.Resolve(Environment.GetEnvironmentVariable("THSSS_AUDIO_BACKEND"))}");

// A deliberately failing transport. It records Disposals/Clears so the test can assert that
// dispose-time code did not silently recreate or reset the broken source.
sealed class FaultSource : IAudioSource
{
    public int Disposals,Clears;
    public int ProcessedBuffers=>throw new InvalidOperationException("original transport failure");
    public int SampleOffset=>0;
    public bool IsPlaying=>false;
    public void Submit(float[] samples,int rate)=>throw new Exception();
    public void UnqueueProcessed()=>throw new Exception();
    public void Play()=>throw new Exception();
    public void Pause()=>throw new InvalidOperationException("secondary pause failure");
    public void Clear(){Clears++;throw new InvalidOperationException("reset must not be called during disposal");}
    public void SetGain(float gain){}
    public void Dispose()=>Disposals++;
}
