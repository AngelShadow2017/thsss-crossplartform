using System.Diagnostics;
using System.Reflection;
using Shooting;
using Silk.NET.Windowing;
using THSSS.Graphics;

if (args.Length != 1) throw new ArgumentException("Pass an isolated game data directory.");
Environment.SetEnvironmentVariable("THSSS_DATA_ROOT", args[0]);
Environment.SetEnvironmentVariable("THSSS_INPUT", "");
Environment.SetEnvironmentVariable("ALSOFT_DRIVERS", "null");
var flags = BindingFlags.Static | BindingFlags.NonPublic;
IWindow window = null;
int checks = 0, phase = 0, settle = 0;
var sizes = new[] { (800,600), (1000,750), (1280,960), (640,480), (1000,650), (800,600) };
Stopwatch minimized = null;
Exception failure = null;
int settledGeneration = -1;
var timeout = Stopwatch.StartNew();
var fullscreen = typeof(Game_Main).GetMethod("ToggleFullScreen", BindingFlags.Instance | BindingFlags.NonPublic)!;
var observer = Task.Run(() =>
{
    while ((window = (IWindow)typeof(Shooting.Program).GetField("_window",flags)!.GetValue(null)) == null)
    {
        if (timeout.Elapsed.TotalSeconds > 30) throw new TimeoutException("Game window was not created.");
        Thread.Sleep(1);
    }
    window.Update += _ =>
    {
        if (timeout.Elapsed.TotalSeconds > 75)
        {
            failure ??= new TimeoutException($"Resize fixture stalled at phase {phase}.");
            window.Close();
        }
        if (minimized?.ElapsedMilliseconds > 400) { window.WindowState = WindowState.Normal; minimized = null; }
    };
    window.Render += _ =>
    {
        try
        {
            var game = (Game_Main)typeof(Shooting.Program).GetField("_game",flags)!.GetValue(null);
            if (game == null || window.IsClosing) return;
            int frame = (int)typeof(Shooting.Program).GetField("_frame",flags)!.GetValue(null);
            if (frame < 500 || minimized != null) return;
            if (phase < sizes.Length)
            {
                var requested = sizes[phase];
                if (settle++ == 0) { window.Size = new(requested.Item1,requested.Item2); return; }
                if (settle < 12) return;
                if (game.DeviceMain.RenderBackend is THSSS.Graphics.Vulkan.VulkanRenderBackend vk)
                {
                    if (settle == 12) settledGeneration = vk.SwapchainGeneration;
                    if (settle < 24) return;
                    if (vk.SwapchainGeneration != settledGeneration) throw new Exception("Swapchain keeps rebuilding after window size settled.");
                }
                var expected = game.Form_Main.AllowAspectCorrection
                    ? (requested.Item1 / 4 * 4, requested.Item1 / 4 * 3) : requested;
                if (window.Size.X != expected.Item1 || window.Size.Y != expected.Item2)
                    throw new Exception($"Window did not settle at {expected}: {window.Size}; state={window.WindowState}");
                CheckGameFrame(game, window);
                ResizePixels.Check(game,window.FramebufferSize.X,window.FramebufferSize.Y);
                Console.WriteLine($"PASS resize {window.Size} framebuffer={window.FramebufferSize} backend={game.DeviceMain.RenderBackend.Kind}"); checks++;
                phase++;settle=0;
                if (phase == 3) { minimized = Stopwatch.StartNew(); window.WindowState = WindowState.Minimized; }
                return;
            }
            if (phase == sizes.Length)
            {
                fullscreen.Invoke(game,null);
                phase++;settle=0;
                return;
            }
            if (phase == sizes.Length + 1)
            {
                if (++settle < 24) return;
                CheckGameFrame(game, window);
                ResizePixels.Check(game,window.FramebufferSize.X,window.FramebufferSize.Y);
                Console.WriteLine($"PASS game fullscreen path state={window.WindowState} framebuffer={window.FramebufferSize}");checks++;
                fullscreen.Invoke(game,null);
                phase++;settle=0;
                return;
            }
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
Shooting.Program.Main(Array.Empty<string>());
observer.Wait();
if(failure != null) throw failure;
if(checks != 8) throw new Exception($"Resize checks did not finish ({checks}).");

static void CheckGameFrame(Game_Main game, IWindow window)
{
    var fb=window.FramebufferSize;
    if (game.DeviceMain.FramebufferWidth != fb.X || game.DeviceMain.FramebufferHeight != fb.Y)
        throw new Exception("Device retained stale framebuffer dimensions.");
    if(game.DeviceMain.GameWidth!=640 || game.DeviceMain.GameHeight!=480)
        throw new Exception("Resize changed game coordinates.");
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
