using System;
using System.Collections.Generic;
using System.Threading;
using Silk.NET.OpenAL;
using THSSS.Audio;

namespace SlimDX.XAudio2
{
    internal sealed class AudioEngine
    {
        private static readonly Lazy<AudioEngine> Lazy = new(() => new AudioEngine());
        internal static AudioEngine Instance => Lazy.Value;
        internal IAudioBackend Backend { get; }
        // Kept only for the OpenAL-specific regression probes, never used by shared playback.
        internal AL AL => (Backend as OpenAlBackend)?.AL;
        internal bool Available => Backend != null;
        internal readonly object Sync = new();
        private readonly HashSet<SourceVoice> _voices = new();
        private readonly Timer _pump;
        internal static int BuffersUploaded;
        internal static int VoicesStarted;
        internal void Register(SourceVoice voice) => _voices.Add(voice);
        internal void Unregister(SourceVoice voice) => _voices.Remove(voice);
        private void Pump(object state)
        {
            lock (Sync)
                foreach (var voice in new List<SourceVoice>(_voices)) voice.PumpSafely();
        }
        private AudioEngine()
        {
            Backend = AudioBackendSelection.Create();
            _pump = new Timer(Pump,null,5,5);
            if (Environment.GetEnvironmentVariable("THSSS_DIAG_AUDIO") == "1")
                Console.WriteLine("[AUDIO] backend="+Backend.Name+" ready=True");
            AppDomain.CurrentDomain.ProcessExit += (_,_) =>
            {
                lock(Sync)
                {
                    _pump.Dispose();
                    foreach(var voice in new List<SourceVoice>(_voices))voice.Dispose();
                    Backend.Dispose();
                }
                Console.Error.WriteLine($"[AUDIO] buffers={BuffersUploaded} voicesStarted={VoicesStarted}");
            };
        }
    }
}
