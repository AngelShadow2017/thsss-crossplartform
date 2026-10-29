using System.Reflection;
using System.Runtime.InteropServices;
using THSSS.Audio;

// CoreAudio backend contract, verified without Apple hardware. The production backend imports
// symbols from "AudioToolbox"; here we load a small C software shim (audioqueue_shim.c) built by
// the test runner and register a DllImportResolver that redirects those imports to it. This
// exercises the real managed backend code path (struct ABI, queue state machine, callback
// ordering) against a software driver. It is NOT Apple's AudioToolbox and NOT a runtime test on a
// Mac: a pass here proves layout and logic, not that macOS plays sound.
if(args.Length!=1)throw new ArgumentException("Pass AudioQueue software-driver library.");
nint library=NativeLibrary.Load(Path.GetFullPath(args[0]));
NativeLibrary.SetDllImportResolver(typeof(CoreAudioBackend).Assembly,(name,_,_)=>name.EndsWith("AudioToolbox")?library:0);

// Shim entry points that stand in for AudioQueue callbacks. They are Cdecl because the C shim
// invokes them through the C calling convention; a StdCall mismatch would corrupt the stack on
// x86 and silently misbehave nowhere else.
var complete=Marshal.GetDelegateForFunctionPointer<Complete>(NativeLibrary.GetExport(library,"shim_complete"));
var prefetch=Marshal.GetDelegateForFunctionPointer<Complete>(NativeLibrary.GetExport(library,"shim_prefetch"));
var advance=Marshal.GetDelegateForFunctionPointer<Advance>(NativeLibrary.GetExport(library,"shim_advance"));
int checks=0;
void Check(bool condition,string label){if(!condition)throw new Exception(label);checks++;}

// ABI checks. These structs are passed by pointer to the C API, so their sizes and field offsets
// are part of the interop contract with the Apple headers: AudioStreamBasicDescription is 40
// bytes, AudioTimeStamp is 64, and QueueBuffer's AudioData pointer must sit at offset 8 (after the
// 4-byte byte-capacity field plus 64-bit alignment). Adding, reordering or "tidying" a field
// keeps C# compiling and makes the framework read garbage at runtime, so it is asserted here.
Check(Marshal.SizeOf<CoreAudioBackend.StreamDescription>()==40,"ASBD ABI");
Check(Marshal.SizeOf<CoreAudioBackend.TimeStamp>()==64,"timestamp ABI");
Check(Marshal.OffsetOf<CoreAudioBackend.QueueBuffer>("AudioData").ToInt32()==8,"buffer pointer ABI");

// Set up one source with two chunks (400 and 600 frames) at half gain, mirroring a BGM voice
// with a queued second chunk.
using var backend=new CoreAudioBackend();using var source=backend.CreateSource(22050);
nint queue=(nint)source.GetType().GetField("_queue",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(source)!;
source.Submit(new float[400],22050);source.Submit(new float[600],22050);source.SetGain(.5f);source.Play();

// A prefetch callback means "give me the next buffer", not "this audio was heard". Counting it as
// completion would advance the logical timeline ahead of real playback and desynchronise the game.
prefetch(queue);Check(source.SampleOffset==0&&source.ProcessedBuffers==0,"prefetch callback is not audible completion");

// Partial advance: the device consumed 70 frames of the first 400-frame chunk, so the offset is 70
// and nothing has completed yet.
advance(queue,70);Check(source.SampleOffset==70,"partial cursor");

// Pause must freeze the offset even though the shim keeps advancing its clock; otherwise a paused
// BGM would keep "playing" in the logical timeline.
source.Pause();advance(queue,20);Check(source.SampleOffset==70,"paused cursor frozen");
// Completion of the first chunk: one processed buffer, and its full 200 frames are retained in
// the offset until explicitly unqueued (200+70=270) — the XAudio2 2.9 behaviour the shared layer
// relies on. UnqueueProcessed then subtracts that chunk exactly once (270-200=70).
source.Play();complete(queue);Check(source.ProcessedBuffers==1,"timeline completion");
Check(source.SampleOffset==270,"retained completed chunk counted");
source.UnqueueProcessed();Check(source.SampleOffset==70,"retirement subtracts once");

// Second completion retires the remaining chunk; after unqueueing, the queue is drained: offset 0,
// not playing. A stale "playing" flag here is how BGM loops re-trigger by themselves.
complete(queue);Check(source.ProcessedBuffers==1,"second completion");source.UnqueueProcessed();
Check(source.SampleOffset==0&&!source.IsPlaying,"queue drained");

// Clear is the stop/reset path: it must empty the queue and reset the cursor and playing flag.
source.Submit(new float[100],22050);source.Play();source.Clear();
Check(source.ProcessedBuffers==0&&source.SampleOffset==0&&!source.IsPlaying,"reset empties queue");

// A fresh playback after a reset starts a new timeline: completion of the 100-frame chunk reports
// 50 frames of offset, not a continuation of the previous count.
source.Submit(new float[100],22050);source.Play();complete(queue);Check(source.SampleOffset==50,"new playback after reset");

// Dispose twice (scene teardown sometimes hits both finally blocks); afterward every call must be
// ObjectDisposedException rather than a use-after-free on the native queue handle.
source.Dispose();source.Dispose();
try{source.Play();throw new Exception("disposed source accepted");}catch(ObjectDisposedException){checks++;}
backend.Dispose();backend.Dispose();

// The trailing note is part of the report: software-driver verification must never be read as
// Apple runtime or speaker validation.
Console.WriteLine($"PASS CoreAudioCompatibility: {checks} native ABI/queue software-driver checks; not Apple runtime");
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]delegate void Complete(nint queue);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]delegate void Advance(nint queue,int frames);
