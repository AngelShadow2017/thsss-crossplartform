using System;
using System.Collections.Generic;
using System.Threading;
using Silk.NET.OpenAL;

namespace SlimDX.XAudio2
{
    internal sealed class AudioEngine
    {
        private static readonly Lazy<AudioEngine> Lazy = new(() => new AudioEngine());
        internal static AudioEngine Instance => Lazy.Value;
        internal AL AL { get; }
        internal ALContext Loader { get; private set; }
        internal unsafe Device* DeviceHandle { get; private set; }
        internal unsafe Context* ContextHandle { get; private set; }
        internal bool Available { get; }
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
            _pump = new Timer(Pump, null, 5, 5);
            try
            {
                var loader = ALContext.GetApi();
                unsafe
                {
                    var device = loader.OpenDevice(null);
                    if (device == null) return;
                    var context = loader.CreateContext(device, null);
                    if (context == null) { loader.CloseDevice(device); return; }
                    if (!loader.MakeContextCurrent(context))
                    {
                        loader.DestroyContext(context);
                        loader.CloseDevice(device);
                        return;
                    }
                    DeviceHandle = device;
                    ContextHandle = context;
                }
                Loader = loader;
                AL = AL.GetApi();
                Available = AL.GetError() == AudioError.NoError;
                if (Environment.GetEnvironmentVariable("THSSS_DIAG_AUDIO") == "1")
                {
                    var renderer = AL.GetStateProperty(StateString.Renderer);
                    Console.WriteLine($"[AUDIO] OpenAL ready={Available} renderer={renderer}");
                }
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
}
