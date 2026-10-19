using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Shooting;
using Silk.NET.Windowing;
using SlimDX.Windows;

internal static class FrameTiming
{
    private static void Set(Game_Main game, string field, object value) =>
        typeof(Game_Main).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(game, value);

    private static Game_Main Create(bool fixedFps, int interval, int tolerance, bool sync, bool fullscreen)
    {
        var game = (Game_Main)RuntimeHelpers.GetUninitializedObject(typeof(Game_Main));
        Set(game, "firstFrame", true);
        Set(game, "fixedFPS", fixedFps);
        Set(game, "presentInterval", interval);
        Set(game, "delayTolerance", tolerance);
        Set(game, "verticalSync", sync);
        Set(game, "fullWindow", fullscreen);
        return game;
    }

    private static T Bind<T>(Game_Main game, string name) where T : Delegate =>
        (T)typeof(Game_Main).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(T), game);

    private sealed class Clock
    {
        internal long Now = Stopwatch.Frequency;
        internal long StartTime;
        internal int Advances;
        internal readonly List<int> Sleeps = new();
        internal void Start() => StartTime = Now;
        internal void Advance() { StartTime += Stopwatch.Frequency / 60; Advances++; }
        internal void Stall(int milliseconds) => Now += Stopwatch.Frequency * milliseconds / 1000;
        internal void Sleep(int milliseconds) { Sleeps.Add(milliseconds); Stall(milliseconds); }
        internal float Duration()
        {
            // A deterministic 0.01ms cost makes the original spin wait progress.
            Now += Math.Max(1, Stopwatch.Frequency / 100000);
            return 1000f * (Now - StartTime) / Stopwatch.Frequency;
        }
    }

    internal static void Run()
    {
        int scenarios = 0;
        foreach (bool fixedFps in new[] { false, true })
        foreach (int interval in new[] { 1, 2, 3 })
        foreach (int tolerance in new[] { 0, 2, 10 })
        foreach (bool sync in new[] { false, true })
        foreach (bool fullscreen in new[] { false, true })
        foreach (bool splash in new[] { false, true })
        {
            var game = Create(fixedFps, interval, tolerance, sync, fullscreen);
            if (splash) Set(game, "PresentState", RuntimeHelpers.GetUninitializedObject(typeof(Game_Main).Assembly.GetType("Shooting.GameState_SplashScreen", true)));
            var begin = Bind<Action<Action, Action>>(game, "BeginFrameTiming");
            var finish = Bind<Action<Func<float>, Action<int>, Action>>(game, "FinishFrameTiming");
            var actual = new Clock();
            var expected = new Clock();
            var form = new RenderForm_Main();
            var window = Fake.Create<IWindow>();
            form.ConfigurePresentation(fullscreen, sync, interval, gameOwnsPacing: true);
            form.AttachWindow(window);
            game.Form_Main = form;
            Checks.Check(window.FramesPerSecond == 0, "game pacing has no independent render cap");
            Checks.Check(window.VSync == (sync && (interval == 1 || fullscreen)), "original swap-sync selection");
            for (int frame = 0; frame < 30; frame++)
            {
                begin(actual.Start, actual.Advance);
                // Original Game_Main.cs:358-372; Watch advances by truncated frequency/60.
                if (frame == 0 || !fixedFps || splash) expected.Start();
                else for (int i = 0; i < interval; i++) expected.Advance();
                int work = frame == 4 ? 100 : frame == 17 ? 500 : 1;
                actual.Stall(work); expected.Stall(work);
                finish(actual.Duration, actual.Sleep, actual.Advance);
                ReferenceFinish(expected, fixedFps, interval, tolerance, sync, fullscreen);
                Checks.Check(actual.Now == expected.Now && actual.StartTime == expected.StartTime &&
                    actual.Advances == expected.Advances && actual.Sleeps.SequenceEqual(expected.Sleeps),
                    $"original timing parity fixed={fixedFps} interval={interval} tolerance={tolerance} sync={sync} fullscreen={fullscreen} splash={splash} frame={frame}");
            }
            form.DetachWindow();
            scenarios++;
        }
        Console.WriteLine($"Timing reference: {scenarios} scenarios x 30 frames; injected 100/500ms stalls, FixedFPS=0/1, intervals=1/2/3, tolerance=0/2/10, sync/fullscreen/splash matrix");
    }

    private static void ReferenceFinish(Clock clock, bool fixedFps, int interval, int tolerance, bool sync, bool fullscreen)
    {
        // Literal original algorithm, reference/decompiled/Shooting/Game_Main.cs:510-527.
        if (sync && (interval == 1 || fullscreen)) return;
        float num = 16f * interval - clock.Duration();
        if (num > 0f) clock.Sleep((int)num);
        while (16.62f * interval - clock.Duration() > 0f) { }
        if (fixedFps && num < -16.66667f * tolerance)
            for (int i = 0; i < -num / 16.66667f - tolerance; i++) clock.Advance();
    }

    internal static void RealLoop()
    {
        foreach (bool fixedFps in new[] { false, true })
        {
            var options = WindowOptions.Default;
            options.IsVisible = false;
            options.VSync = false;
            using var window = Window.Create(options);
            window.Initialize();
            var form = new RenderForm_Main();
            form.ConfigurePresentation(false, false, 1, gameOwnsPacing: true);
            form.AttachWindow(window);
            var game = Create(fixedFps, 1, 10, false, false);
            game.Form_Main = form;
            var begin = Bind<Action<Action, Action>>(game, "BeginFrameTiming");
            var finish = Bind<Action<Func<float>, Action<int>, Action>>(game, "FinishFrameTiming");
            var timer = new Watch();
            var elapsed = Stopwatch.StartNew();
            var ends = new List<double>();
            int updates = 0;
            window.Update += _ => updates++;
            window.Render += _ =>
            {
                begin(timer.Start, timer.IncreaseOneFrame);
                if (ends.Count == 8) Thread.Sleep(100);
                finish(timer.GetDuration, Thread.Sleep, timer.IncreaseOneFrame);
                ends.Add(elapsed.Elapsed.TotalMilliseconds);
                if (ends.Count == 30) form.Close();
            };
            window.Run();
            var recovery = Enumerable.Range(9, 6).Select(i => ends[i] - ends[i - 1]).ToArray();
            int rapid = recovery.Count(ms => ms < 8);
            Checks.Check(fixedFps ? rapid >= 3 : rapid == 0, $"live GLFW catch-up fixed={fixedFps}: {string.Join(',', recovery)}");
            Checks.Check(updates >= 29 && window.FramesPerSecond == 0, "event/update loop runs between game-paced frames");
            Console.WriteLine($"GLFW game pacing FixedFPS={(fixedFps ? 1 : 0)}: 100ms stall, recovery ms=[{string.Join(",", recovery.Select(ms => ms.ToString("F2")))}], updates={updates}, frames={ends.Count}");
            form.DetachWindow();
        }
    }
}
