using System.Diagnostics;
using System.Reflection;
using Shooting;
using Silk.NET.Windowing;
using THSSS.Graphics;

// Window resize fixture. It does not fake the window: it boots the real game loop and
// drives it from the outside through Silk events plus the game's own ToggleFullScreen,
// asserting after every transition that the device still reports the framebuffer the
// window has and that the menu is still on screen.
//
// Why the phase counter: a fixture that dies (or hangs) before finishing must not look
// like a pass. Every phase is counted, the end requires the exact expected count, and a
// wall-clock timeout reports the phase it stalled in.
if (args.Length != 1) throw new ArgumentException("Pass an isolated game data directory.");
// An isolated data root keeps the run from touching the player's save/config; empty input
// and a null audio driver keep the loop deterministic and headless.
Environment.SetEnvironmentVariable("THSSS_DATA_ROOT", args[0]);
Environment.SetEnvironmentVariable("THSSS_INPUT", "");
Environment.SetEnvironmentVariable("ALSOFT_DRIVERS", "null");
var flags = BindingFlags.Static | BindingFlags.NonPublic;
IWindow window = null;
int checks = 0, phase = 0, settle = 0;
// Size sequence walks through up/down scaling; it returns to the starting size to prove
// the earlier geometry is recoverable.
var sizes = new[] { (800,600), (1000,750), (1280,960), (640,480), (1000,650), (800,600) };
Stopwatch minimized = null;
Exception failure = null;
int settledGeneration = -1;
var timeout = Stopwatch.StartNew();
// The game's own fullscreen toggle is what users hit (F11/alt-enter path), not a raw
// WindowState assignment, so its side effects are covered too.
var fullscreen = typeof(Game_Main).GetMethod("ToggleFullScreen", BindingFlags.Instance | BindingFlags.NonPublic)!;
var observer = Task.Run(() =>
{
    // The window only exists once the game loop has started; poll for it with its own
    // deadline so a failed startup reports instead of blocking forever.
    while ((window = (IWindow)typeof(Shooting.Program).GetField("_window",flags)!.GetValue(null)) == null)
    {
        if (timeout.Elapsed.TotalSeconds > 30) throw new TimeoutException("Game window was not created.");
        Thread.Sleep(1);
    }
    window.Update += _ =>
    {
        // Hard stall guard: the whole fixture must finish in 75s, and the message names the
        // phase so a hang is diagnosable rather than a bare timeout.
        if (timeout.Elapsed.TotalSeconds > 75)
        {
            failure ??= new TimeoutException($"Resize fixture stalled at phase {phase}.");
            window.Close();
        }
        // A minimized window gets no presents, so the restore is driven from Update, which
        // still runs; 400 ms leaves the compositor time to report the iconified state.
        if (minimized?.ElapsedMilliseconds > 400) { window.WindowState = WindowState.Normal; minimized = null; }
    };
    window.Render += _ =>
    {
        try
        {
            var game = (Game_Main)typeof(Shooting.Program).GetField("_game",flags)!.GetValue(null);
            if (game == null || window.IsClosing) return;
            // The menu needs a few hundred frames to finish loading before resizes start;
            // the fixture measures rendering, not boot time.
            int frame = (int)typeof(Shooting.Program).GetField("_frame",flags)!.GetValue(null);
            if (frame < 500 || minimized != null) return;
            if (phase < sizes.Length)
            {
                // Each size gets a settle window: one frame to request, then frames to let the
                // backend and game finish reacting before anything is asserted.
                var requested = sizes[phase];
                if (settle++ == 0) { window.Size = new(requested.Item1,requested.Item2); return; }
                if (settle < 12) return;
                // Vulkan rebuilds its swapchain asynchronously; the generation must stop
                // changing, otherwise the resize is fighting an endless rebuild loop.
                if (game.DeviceMain.RenderBackend is THSSS.Graphics.Vulkan.VulkanRenderBackend vk)
                {
                    if (settle == 12) settledGeneration = vk.SwapchainGeneration;
                    if (settle < 24) return;
                    if (vk.SwapchainGeneration != settledGeneration) throw new Exception("Swapchain keeps rebuilding after window size settled.");
                }
                // Aspect correction is a game setting: when enabled the window may legitimately
                // land on a 4:3 geometry instead of the requested one.
                var expected = game.Form_Main.AllowAspectCorrection
                    ? (requested.Item1 / 4 * 4, requested.Item1 / 4 * 3) : requested;
                if (window.Size.X != expected.Item1 || window.Size.Y != expected.Item2)
                    throw new Exception($"Window did not settle at {expected}: {window.Size}; state={window.WindowState}");
                CheckGameFrame(game, window);
                ResizePixels.Check(game,window.FramebufferSize.X,window.FramebufferSize.Y);
                Console.WriteLine($"PASS resize {window.Size} framebuffer={window.FramebufferSize} backend={game.DeviceMain.RenderBackend.Kind}"); checks++;
                phase++;settle=0;
                // Minimize once the mid-sequence size is in place, so restore has to recover
                // from the iconified state rather than a fresh resize.
                if (phase == 3) { minimized = Stopwatch.StartNew(); window.WindowState = WindowState.Minimized; }
                return;
            }
            if (phase == sizes.Length)
            {
                // Entering fullscreen goes through the game's own toggle, not WindowState.
                fullscreen.Invoke(game,null);
                phase++;settle=0;
                return;
            }
            if (phase == sizes.Length + 1)
            {
                // Fullscreen has no single-frame outcome: give it the same 24-frame settle and
                // then verify the game survived the mode switch.
                if (++settle < 24) return;
                CheckGameFrame(game, window);
                ResizePixels.Check(game,window.FramebufferSize.X,window.FramebufferSize.Y);
                Console.WriteLine($"PASS game fullscreen path state={window.WindowState} framebuffer={window.FramebufferSize}");checks++;
                fullscreen.Invoke(game,null);
                phase++;settle=0;
                return;
            }
            // Final phase: back to windowed after minimize plus fullscreen; only now is the full
            // recovery path proven, so the loop may close.
            if (++settle < 24) return;
            CheckGameFrame(game, window);
            ResizePixels.Check(game,window.FramebufferSize.X,window.FramebufferSize.Y);
            checks++;
            Console.WriteLine($"PASS resize/minimize/fullscreen recovery: {checks} checks");
            window.Close();
        }
        catch(Exception ex) { failure=ex;window.Close(); }
    };
});
// The fixture runs the real entry point on this thread; the observer task watches the loop.
Shooting.Program.Main(Array.Empty<string>());
observer.Wait();
if(failure != null) throw failure;
// Guards against a phase machine that closed early: all eight transitions must have reported.
if(checks != 8) throw new Exception($"Resize checks did not finish ({checks}).");

// Shared post-condition for every phase: the device state and what is actually on screen
// must both follow the window, and game coordinates must stay the original 640x480.
static void CheckGameFrame(Game_Main game, IWindow window)
{
    var fb=window.FramebufferSize;
    if (game.DeviceMain.FramebufferWidth != fb.X || game.DeviceMain.FramebufferHeight != fb.Y)
        throw new Exception("Device retained stale framebuffer dimensions.");
    if(game.DeviceMain.GameWidth!=640 || game.DeviceMain.GameHeight!=480)
        throw new Exception("Resize changed game coordinates.");
    // A readback of the framebuffer centre proves the menu is still rendered, not merely that
    // the dimensions were updated. Bindings are restored afterwards so later draws are unaffected.
    var backend=game.DeviceMain.RenderBackend;
    var draw=backend.CurrentDrawFramebuffer;var read=backend.CurrentReadFramebuffer;
    try
    {
        backend.RestoreFramebufferBindings(draw,null);
        var pixel=backend.ReadPixels(fb.X/2,fb.Y/2,1,1);
        if(pixel[0]<50 || pixel[1]<50 || pixel[2]<50)
            throw new Exception("Menu disappeared: "+string.Join(',',pixel));
    }
    finally { backend.RestoreFramebufferBindings(draw,read); }
}
