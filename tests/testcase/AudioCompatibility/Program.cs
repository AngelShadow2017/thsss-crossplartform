using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX.Multimedia;
using SlimDX.XAudio2;
using Silk.NET.OpenAL;

// Audio compatibility contract: the SlimDX.XAudio2-shaped API the game was written against
// (Voice/State/BufferStart/BufferEnd/looping/SetOutputMatrix) must keep the same observable
// semantics on top of OpenAL. Assertion counters stand in for a test framework: any failed
// assertion throws, and the process exit code is the verdict.
internal static class Program
{
    private static int _assertions;

    // The game builds every WaveFormat at 22050 Hz, so fixtures use the engine rate to keep
    // sample-frame == native-frame arithmetic in the cursor assertions exact.
    private const int Rate = 22050;

    // One process-wide engine, as in the game. Reflection is used deliberately: tests need the
    // same internal Sync lock and AudioEngine singleton as the production transport, because the
    // loopback device below temporarily replaces the current OpenAL context.
    private static readonly XAudio2 Engine = new();
    private static readonly Type EngineType = typeof(XAudio2).Assembly.GetType("SlimDX.XAudio2.AudioEngine", true)!;
    private static readonly object Backend = EngineType.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
    private static readonly object Sync = EngineType.GetField("Sync", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(Backend)!;

    private static void Assert(bool condition, string message)
    {
        _assertions++;
        if (!condition) throw new Exception(message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { _assertions++; return; }
        throw new Exception($"Expected {typeof(T).Name}");
    }

    // Builds a RIFF/WAVE blob in memory so decode tests never depend on files on disk.
    // The fixed sample values 0.25 / -0.125 are exactly representable in every supported bit
    // depth (8, 16, 24, 32 int and 32 float), which lets channel/matrix assertions compare
    // against literal numbers instead of tolerances.
    private static byte[] Wav(int frames, int channels = 1, int bits = 16, short tag = 1)
    {
        using var memory = new MemoryStream();
        using var writer = new BinaryWriter(memory, Encoding.ASCII, true);
        int size = frames * channels * bits / 8;
        writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(size + 36);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
        writer.Write(tag); writer.Write((short)channels); writer.Write(Rate);
        writer.Write(Rate * channels * bits / 8); writer.Write((short)(channels * bits / 8)); writer.Write((short)bits);
        writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(size);
        for (int frame = 0; frame < frames; frame++)
            for (int channel = 0; channel < channels; channel++)
            {
                float sample = (channel == 0 ? 0.25f : -0.125f);
                if (bits == 8) writer.Write((byte)(128 + sample * 128));
                else if (tag == 3) writer.Write(sample);
                else if (bits == 16) writer.Write((short)(sample * 32768));
                else if (bits == 24) { int value = (int)(sample * 8388608); writer.Write((byte)value); writer.Write((byte)(value >> 8)); writer.Write((byte)(value >> 16)); }
                else writer.Write((int)(sample * 2147483648));
            }
        return memory.ToArray();
    }

    private static WaveStream Wave(int frames, int channels = 1) => new(new MemoryStream(Wav(frames, channels)));

    // Reads the OpenAL source id that the voice is currently driving, to prove a real native
    // source exists (rather than a no-op stub) and to query its state directly.
    private static uint Source(SourceVoice voice)
    {
        var output = typeof(SourceVoice).GetField("_output", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(voice)!;
        return (uint)output.GetType().GetField("Id",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(output)!;
    }

    // Constructs the internal AudioPlayback (the decoded-PCM timeline) directly so the
    // frame-mapping rule can be asserted without any device or clock.
    private static object Playback(AudioBuffer buffer, WaveFormat format) => Activator.CreateInstance(
        typeof(SourceVoice).Assembly.GetType("SlimDX.XAudio2.AudioPlayback", true)!,
        BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { buffer, format }, null)!;

    private static void TimelineTests()
    {
        // Contract: FrameAt(logicalFrame) maps a voice output position onto a source PCM frame,
        // honouring PlayBegin/PlayLength first, then LoopBegin/LoopLength, LoopCount times, then
        // the remaining tail. The expected literal is the original ordering: intro (3,4), the
        // region's first pass (5-8), the two LoopCount repeats (5-8 twice), tail (9-16).
        using var wave = Wave(20);
        var buffer = new AudioBuffer { AudioData = wave, PlayBegin = 3, PlayLength = 14, LoopBegin = 5, LoopLength = 4, LoopCount = 2 };
        object playback = Playback(buffer, wave.Format);
        var method = playback.GetType().GetMethod("FrameAt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var actual = Enumerable.Range(0, 22).Select(i => (int)method.Invoke(playback, new object[] { (long)i })!).ToArray();
        int[] expected = { 3,4,5,6,7,8,5,6,7,8,5,6,7,8,9,10,11,12,13,14,15,16 };
        Assert(actual.SequenceEqual(expected), "Finite playback must include intro, two exact repeats and tail.");

        // LoopCount 255 is the original "infinite" sentinel. An absurd frame index must resolve
        // by arithmetic wrap inside the loop region: never by materialising repeated PCM (memory
        // growth) or by overflowing long arithmetic.
        buffer.LoopCount = 255;
        playback = Playback(buffer, wave.Format);
        Assert((int)method.Invoke(playback, new object[] { 10000000006L })! == 5, "Infinite timeline must not allocate repeated PCM or overflow.");

        // Every integer depth plus IEEE float (tag 3) must decode to the same normalised values,
        // because the game ships 8/16-bit assets while 24/32-bit and float reach us via mods.
        for (int bits = 8; bits <= 32; bits += 8)
        {
            using var pcm = new WaveStream(new MemoryStream(Wav(20, 2, bits)));
            var decoded = Playback(new AudioBuffer { AudioData = pcm }, pcm.Format);
            var samples = (float[])decoded.GetType().GetField("Samples", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(decoded)!;
            Assert(samples[0] == 0.25f && samples[1] == -0.125f, $"PCM{bits} decoding");
        }
        using var floating = new WaveStream(new MemoryStream(Wav(20, 2, 32, 3)));
        Assert(Playback(new AudioBuffer { AudioData = floating }, floating.Format) != null, "Float32 decoding");
    }

    private static void OwnershipTests()
    {
        // Contract: opening a WaveStream by path must close the file handle before returning
        // (the game opens hundreds of short SFX during a stage). Holding the file exclusively
        // with FileShare.None is the only proof that no handle leaked; 100 iterations also catch
        // slow handle growth that a single iteration would hide.
        string path = Path.Combine(Path.GetTempPath(), "thsss-audio-" + Guid.NewGuid() + ".wav");
        File.WriteAllBytes(path, Wav(100));
        try
        {
            for (int i = 0; i < 100; i++)
            {
                using var wave = new WaveStream(path);
                using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                Assert(wave.Length == 200, "Filename WaveStream must close its input immediately.");
            }

            // The stream overload is caller-owned and must NOT be disposed by the WaveStream,
            // otherwise the game's cached in-memory assets die on the first voice.
            using var caller = new MemoryStream(Wav(100));
            using (var wave = new WaveStream(caller)) { }
            Assert(caller.CanRead, "Stream overload must leave caller-owned input open.");

            // Failure paths (not just success) must release the handle; a leaked handle on the
            // corrupt-file path is invisible until a retry, so it is asserted explicitly.
            File.WriteAllText(path, "invalid");
            Throws<InvalidDataException>(() => new WaveStream(path));
            using var invalidExclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert(invalidExclusive.Length == 7, "Invalid filename input must also release file handle.");
        }
        finally { File.Delete(path); }
    }

    private static void RealtimeTests()
    {
        // Realtime section: exercises a genuine OpenAL device, not the loopback. It must fail
        // loudly when no audio backend exists (the assertion on the source id below), because a
        // silently skipped realtime test is how transport regressions ship.
        using var wave = Wave(Rate * 2);
        using var voice = new SourceVoice(Engine, wave.Format);
        Assert(Source(voice) != 0, "A real OpenAL context is required, not a skipped audio test.");
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave });
        voice.Start(); Thread.Sleep(140); voice.Stop();

        // Pause must freeze the logical cursor rather than re-queue or rewind it; the sleeps are
        // sized so 140 ms at 22050 Hz comfortably exceeds the 1000-frame threshold even on a
        // slow/software device, and 100 ms exceeds the 500-frame resume threshold.
        long before = voice.State.SamplesPlayed;
        Assert(before > 1000, "OpenAL realtime cursor must advance.");
        Thread.Sleep(100);
        Assert(voice.State.SamplesPlayed == before, "Pause must freeze cursor.");
        voice.Start(); Thread.Sleep(100); voice.Stop();
        long after = voice.State.SamplesPlayed;
        Assert(after > before + 500, "Resume must advance from paused cursor, not restart.");
        Console.WriteLine($"REALTIME cursor: paused={before}, resumed={after}");
        voice.FlushSourceBuffers();
        Assert(voice.State.BuffersQueued == 0, "Flush clears stopped queue.");

        // Infinite-region stress: a 1102-frame buffer (50 ms) looping forever would drain
        // immediately if the transport were one-shot. After 450 ms the logical buffer must still
        // be the single queued one and the cursor must have advanced several buffer lengths; the
        // native state 0x1012 (AL_PLAYING) is the ground truth that OpenAL itself is still playing
        // rather than the managed layer reporting optimism.
        using var shortWave = Wave(1102);
        using var infinite = new SourceVoice(Engine, shortWave.Format);
        infinite.SubmitSourceBuffer(new AudioBuffer { AudioData = shortWave, LoopBegin = 200, LoopLength = 400, LoopCount = 255 });
        infinite.Start(); Thread.Sleep(450);
        Assert(infinite.State.BuffersQueued == 1 && infinite.State.SamplesPlayed > 5000, "Infinite region must survive many original-buffer durations without State polling.");
        Assert(NativeState(infinite) == 0x1012, "Native OpenAL source must still be playing.");
    }

    private static int NativeState(SourceVoice voice)
    {
        using var al = AL.GetApi();
        al.GetSourceProperty(Source(voice), GetSourceInteger.SourceState, out int state);
        return state;
    }

    private static void Main()
    {
        // Order matters only in that the loopback sections swap the current OpenAL context and
        // restore it afterwards; device tests run first so they see the real default device.
        TimelineTests(); OwnershipTests(); RealtimeTests();
        using (var loopback = new Loopback())
        {
            LoopbackTests(loopback);
            RegionAndEndOfStreamTests(loopback);
        }
        Console.WriteLine($"PASS AudioCompatibility: {_assertions} assertions; realtime OpenAL + deterministic OpenAL Soft loopback.");
    }

    private static void LoopbackTests(Loopback loopback)
    {
        // Deterministic section: OpenAL Soft's loopback device renders to a float array, so every
        // numeric assertion here is a sample-accurate arithmetic check. It is NOT a listening
        // test: it proves gain/matrix/loop arithmetic, not perceived loudness or panning.
        using var wave = Wave(2000);
        using var voice = new SourceVoice(Engine, wave.Format);

        // Volume and output matrix are independent settings in XAudio2; a matrix update must
        // never overwrite the configured gain (a regression here silences or boosts all SFX).
        voice.Volume = 0.2f;
        voice.SetOutputMatrix(1, 2, new[] { 1f, 0f });
        Assert((float)voice.Volume == 0.2f, "Matrix must not overwrite configured volume.");

        // PlayLength 1400 with LoopCount 2 x LoopLength 400 == 1400 + 800 = 2200 consumed
        // frames; that exact total is re-asserted after every manual segment below.
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayBegin = 300, PlayLength = 1400, LoopBegin = 500, LoopLength = 400, LoopCount = 2 });
        int ends = 0, starts = 0;
        voice.BufferEnd += (_, _) => ends++;
        voice.BufferStart += (_, _) => starts++;
        voice.Start();

        // Rendering exactly 1000 frames must move the cursor by exactly 1000, i.e. PlayBegin is
        // an offset into the source, not a consumed-frame count.
        float[] output = loopback.Render(voice, 1000);
        Assert(voice.State.SamplesPlayed == 1000, "Loopback cursor counts sample frames including repeats and excluding PlayBegin.");
        CheckChannels(output, 0.05f, 0f, "mono hard left at configured gain");
        voice.Stop();
        loopback.Render(voice, 500);
        Assert(voice.State.SamplesPlayed == 1000, "Loopback pause must retain exact sample cursor.");

        // Changing the matrix while paused must not touch the cursor (the game live-mixes volume
        // fades and channel swaps while BGM is paused for menus).
        voice.SetOutputMatrix(1, 2, new[] { 0f, -2f });
        Assert(voice.State.SamplesPlayed == 1000, "Live matrix change must not alter paused cursor.");
        voice.Start();
        output = loopback.Render(voice, 1000);
        CheckChannels(output, 0f, -0.1f, "live mono right, signed coefficient and independent gain");
        loopback.Render(voice, 500);

        // End of the finite queue: exact repeats plus tail, and the buffer dequeues once.
        Assert(voice.State.SamplesPlayed == 2200 && voice.State.BuffersQueued == 0, "Finite loop must complete exact repeats plus tail and dequeue.");

        // Callbacks are per logical submission, not per streaming chunk or per loop pass;
        // extra callbacks would make the game restart tracks or double-count scores.
        Assert(starts == 1 && ends == 1, "Logical buffer callbacks occur once, not per streaming chunk/loop.");
        voice.Start(); loopback.Render(voice, 200);
        Assert(voice.State.SamplesPlayed == 2200 && ends == 1, "Start on completed queue must not replay old audio.");

        // Queueing two submissions keeps them separate (BuffersQueued==2) and consumes them in
        // submission order: 100+200 = 300 onto the settled 2200 == 2500.
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayBegin = 700, PlayLength = 100 });
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayBegin = 100, PlayLength = 200 });
        Assert(voice.State.BuffersQueued == 2, "Multiple logical submissions remain queued independently.");
        loopback.Render(voice, 500);
        Assert(voice.State.BuffersQueued == 0 && voice.State.SamplesPlayed == 2500 && ends == 3 && starts == 3, "Multiple submissions complete exactly once in order.");

        // Stereo matrix coverage. Source channels are +0.25 / -0.125 at gain 0.4, and each case
        // probes a different rule of the XAudio2 output matrix: swap, signed crossfeed,
        // 3-element diagonal (an original caller passes 3 coefficients for a 2x2 matrix),
        // values above unity (must not clip before the independent gain multiply), mono downmix
        // (sum of both inputs) and mute (gain 0 must win over any coefficient).
        using var stereoWave = Wave(8000, 2);
        using var stereo = new SourceVoice(Engine, stereoWave.Format);
        stereo.Volume = 0.4f;
        stereo.SetOutputMatrix(2, 2, new[] { 0f, 1f, 1f, 0f });
        stereo.SubmitSourceBuffer(new AudioBuffer { AudioData = stereoWave }); stereo.Start();
        CheckChannels(loopback.Render(stereo, 1000), -0.05f, 0.1f, "full stereo matrix swaps channels");
        stereo.SetOutputMatrix(2, 2, new[] { 1f, 1f, 1f, -1f });
        CheckChannels(loopback.Render(stereo, 1000), 0.05f, 0.15f, "full stereo crossfeed with signed coefficient");
        stereo.SetOutputMatrix(2, 2, new[] { 0f, 0.5f, 1f });
        CheckChannels(loopback.Render(stereo, 1000), 0.05f, -0.05f, "original caller three-element stereo diagonal");
        stereo.Volume = 0.1f;
        stereo.SetOutputMatrix(2, 2, new[] { 8f, 0f, 0f, 8f });
        CheckChannels(loopback.Render(stereo, 1000), 0.2f, -0.1f, "matrix above unity must not clip before independent volume");
        stereo.SetOutputMatrix(2, 1, new[] { 1f, 1f });
        CheckChannels(loopback.Render(stereo, 1000), 0.0125f, 0.0125f, "stereo-to-mono matrix must sum both inputs");
        stereo.Volume = 0;
        stereo.SetOutputMatrix(2, 2, new[] { 4f, 0f, 0f, 4f });
        CheckChannels(loopback.Render(stereo, 1000), 0, 0, "matrix must preserve mute");

        // Invalid input must fail fast with the specific exception types the game's error
        // handling distinguishes: malformed matrix, NaN coefficient, NaN gain, a play region that
        // starts at/after the end of the buffer, and a channel count XAudio2 does not support.
        Throws<ArgumentException>(() => stereo.SetOutputMatrix(2, 2, new[] { 1f, 2f }));
        Throws<ArgumentException>(() => stereo.SetOutputMatrix(2, 2, new[] { float.NaN, 0f, 0f, 1f }));
        Throws<ArgumentOutOfRangeException>(() => stereo.Volume = float.NaN);
        Throws<ArgumentOutOfRangeException>(() => stereo.SubmitSourceBuffer(new AudioBuffer { AudioData = stereoWave, PlayBegin = 8000 }));
        Throws<NotSupportedException>(() => new SourceVoice(Engine, new WaveFormat(Rate, 16, 6)));
        Console.WriteLine("LOOPBACK: exact finite cursor=2200; mono/stereo matrix, signed crossfeed, live changes, mute and callbacks verified.");
    }

    private static void RegionAndEndOfStreamTests(Loopback loopback)
    {
        // Rewrites the PCM in place with one distinguishable level per source region, so a probe
        // can prove WHICH source frame was rendered, not merely that something was audible.
        // Levels are 16-bit-exact and unique per region: intro, loop pass, tail.
        using var wave = Wave(2000);
        for (int frame = 0; frame < 2000; frame++)
        {
            short value = frame < 300 ? (short)4096 : frame < 500 ? (short)8192 :
                frame < 700 ? (short)-4096 : frame < 900 ? (short)4096 : (short)12288;
            BitConverter.TryWriteBytes(wave.Pcm.AsSpan(frame * 2, 2), value);
        }
        using var voice = new SourceVoice(Engine, wave.Format);
        voice.SetOutputMatrix(1, 2, new[] { 1f, 0f });
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayBegin = 300, PlayLength = 1400,
            LoopBegin = 500, LoopLength = 400, LoopCount = 2 });
        voice.Start();
        float[] samples = loopback.Render(voice, 3000);

        // Probe positions pin every phase of the timeline: pre-play region, both loop passes,
        // tail, and silence after the queue has ended. If loop bounds are off by one frame these
        // probes land on the neighbouring region's level and fail.
        int[] probes = { 100, 300, 500, 700, 900, 1100, 1300, 1700, 2100, 2950 };
        float[] expected = { 0.25f, -0.125f, 0.125f, -0.125f, 0.125f, -0.125f, 0.125f, 0.375f, 0.375f, 0f };
        for (int i = 0; i < probes.Length; i++)
            Assert(Math.Abs(samples[probes[i] * 2] - expected[i]) < 0.002,
                $"Rendered source region at frame {probes[i]} must be {expected[i]}, got {samples[probes[i] * 2]}.");

        // End-of-stream contract, mirroring XAudio2: a submission made after the voice stopped on
        // completion must stay queued (OpenAL would otherwise swallow it), a stopped queue must
        // not advance the cursor, and an EndOfStream-marked buffer resets SamplesPlayed to 0.
        voice.Stop();
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayLength = 100, Flags = BufferFlags.EndOfStream });
        Assert(voice.State.BuffersQueued == 1, "Submission after stopped completion must remain queued, not be counted processed by OpenAL.");
        loopback.Render(voice, 300);
        Assert(voice.State.BuffersQueued == 1 && voice.State.SamplesPlayed == 2200, "Stopped fresh submission cannot advance cursor.");
        voice.Start(); loopback.Render(voice, 300);
        Assert(voice.State.BuffersQueued == 0 && voice.State.SamplesPlayed == 0, "EndOfStream marker resets SamplesPlayed.");

        // An EOS marker followed by a normal stream counts only the latter: the marker is a
        // separator, not a zero-length sample source.
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayLength = 50, Flags = BufferFlags.EndOfStream });
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayLength = 70 });
        loopback.Render(voice, 200);
        Assert(voice.State.SamplesPlayed == 70, "EOS followed by another queued stream counts only the latter stream.");

        // Infinite region with a non-zero PlayBegin: after 20000 rendered frames the cursor must
        // still be a monotone absolute count (20070), and the native chunk queue must stay
        // bounded (<=4) because refilling an infinite loop must retire old chunks, not accumulate.
        voice.SubmitSourceBuffer(new AudioBuffer { AudioData = wave, PlayBegin = 300, LoopBegin = 500, LoopLength = 400, LoopCount = 255 });
        loopback.Render(voice, 20000);
        Assert(voice.State.BuffersQueued == 1 && voice.State.SamplesPlayed == 20070, "Infinite nonzero-start region must preserve monotonically consumed frames.");
        var chunks = (System.Collections.ICollection)typeof(SourceVoice).GetField("_queued", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(voice)!;
        Assert(chunks.Count <= 4, "Infinite looping must have a bounded native buffer queue.");

        // Pause/resume inside a repeated loop must keep the repetition position: resuming within
        // the region advances exactly the rendered frame count (333), never snaps to a loop start.
        voice.Stop();
        long paused = voice.State.SamplesPlayed;
        loopback.Render(voice, 1000);
        Assert(voice.State.SamplesPlayed == paused, "Pause during repeated loop must preserve exact cursor.");
        voice.Start(); loopback.Render(voice, 333);
        Assert(voice.State.SamplesPlayed == paused + 333, "Resume within repeated loop must preserve repetition position.");

        // Flush must remove running infinite playback entirely (the "stop BGM" path), and Dispose
        // must be idempotent while any post-dispose call is an explicit ObjectDisposedException.
        voice.FlushSourceBuffers();
        Assert(voice.State.BuffersQueued == 0, "Flushing running infinite playback must remove it completely.");
        voice.Dispose(); voice.Dispose();
        Assert(voice.IsDisposed, "Dispose must be idempotent.");
        Throws<ObjectDisposedException>(() => voice.Start());
    }

    // Averages the second half of a rendered block and compares it against expected channel
    // values. Skipping the first half avoids start-up transients from the loopback device's
    // first render call; 0.002 tolerance absorbs float rounding, nothing more.
    private static void CheckChannels(float[] samples, float left, float right, string label)
    {
        double l = 0, r = 0;
        int begin = samples.Length / 2;
        begin -= begin % 2;
        for (int i = begin; i < samples.Length; i += 2) { l += samples[i]; r += samples[i + 1]; }
        int frames = (samples.Length - begin) / 2;
        l /= frames; r /= frames;
        Assert(Math.Abs(l - left) < 0.002 && Math.Abs(r - right) < 0.002,
            $"{label}: expected ({left}, {right}), rendered ({l:F5}, {r:F5})");
    }

    // OpenAL Soft loopback: renders into caller memory with no sound card and no clock, which is
    // what makes the numeric assertions above deterministic. Requires libopenal.so.1 to expose
    // the SOFT loopback extension; absence is a loud failure, never a silent skip.
    private sealed unsafe class Loopback : IDisposable
    {
        private readonly ALContext _alc = ALContext.GetApi();
        private readonly nint _device;
        private readonly Context* _context;
        private readonly Context* _previous;
        [DllImport("libopenal.so.1")] private static extern nint alcLoopbackOpenDeviceSOFT(nint name);
        [DllImport("libopenal.so.1")] private static extern void alcRenderSamplesSOFT(nint device, float* output, int frames);

        // Attributes are ALC_FREQUENCY(0x1007)=22050, ALC_FORMAT_CHANNELS_SOFT(0x1990)=2,
        // ALC_TRUE(0x1501), ALC_FORMAT_TYPE_SOFT(0x1991)=AL_FLOAT(0x1406): stereo float output,
        // so rendered samples are comparable to the decoded PCM values directly.
        internal Loopback()
        {
            lock (Sync)
            {
                _previous = _alc.GetCurrentContext();
                _device = alcLoopbackOpenDeviceSOFT(0);
                Assert(_device != 0, "OpenAL Soft loopback device required.");
                int[] attributes = { 0x1007, Rate, 0x1990, 0x1501, 0x1991, 0x1406, 0 };
                fixed (int* pointer = attributes) _context = _alc.CreateContext((Silk.NET.OpenAL.Device*)_device, pointer);
                Assert(_context != null && _alc.MakeContextCurrent(_context), "Loopback context must initialize stereo float output.");
            }
        }

        // Renders in 128-frame slices and touches voice.State before/after each slice, because
        // the managed layer refills and retires native chunks lazily from that poll. A single
        // giant render call would let a short buffer appear to "end" mid-render and would make
        // the cursor/callback assertions depend on an implementation detail instead of behaviour.
        internal float[] Render(SourceVoice voice, int frames)
        {
            var output = new float[frames * 2];
            lock (Sync)
            {
                for (int position = 0; position < frames; position += 128)
                {
                    int count = Math.Min(128, frames - position);
                    _ = voice.State;
                    fixed (float* pointer = &output[position * 2]) alcRenderSamplesSOFT(_device, pointer, count);
                    _ = voice.State;
                }
            }
            return output;
        }

        // Restores the previously current context so the realtime device keeps working after the
        // loopback sections; the engine's own Sync lock keeps the shared pump from racing this.
        public void Dispose()
        {
            lock (Sync)
            {
                _alc.MakeContextCurrent(_previous);
                _alc.DestroyContext(_context);
                _alc.CloseDevice((Silk.NET.OpenAL.Device*)_device);
                _alc.Dispose();
            }
        }
    }
}
