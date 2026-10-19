// Cross-platform entry point: one GLFW/OpenGL window replaces the WinForms
// RenderForm + Direct3D9 device the original game created.
using System;
using System.IO;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SlimDX.Direct3D9;
using SlimDX.DirectInput;
using Silk.NET.Input;
using Device = SlimDX.Direct3D9.Device;
using Key = Silk.NET.Input.Key;

namespace Shooting;

public static class Program
{
    private static IWindow _window;
    private static GL _gl;
    private static Device _device;
    private static Game_Main _game;
    private static int _frame;

    [STAThread]
    public static void Main(string[] args)
    {
        // Story scripts, CSVs and setting files are GBK (codepage 936).
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var dataRoot = Environment.GetEnvironmentVariable("THSSS_DATA_ROOT");
        if (string.IsNullOrEmpty(dataRoot)) dataRoot = AppContext.BaseDirectory;
        if (Directory.Exists(dataRoot)) Directory.SetCurrentDirectory(dataRoot);

        var options = WindowOptions.Default;
        options.Title = "THSSS - Touhou Hisou Sousou";
        options.Size = new Silk.NET.Maths.Vector2D<int>(640, 480);
        options.VSync = true;
        options.WindowBorder = WindowBorder.Fixed;
        options.API = GraphicsAPI.Default;
        options.FramesPerSecond = 60;
        options.UpdatesPerSecond = 60;

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Run();
    }

    private static void OnLoad()
    {
        _gl = _window.CreateOpenGL();
        Device.CurrentContext = _gl;
        _device = new Device(_gl, _window.Size.X, _window.Size.Y);

        var input = _window.CreateInput();
        foreach (var keyboard in input.Keyboards)
        {
            keyboard.KeyDown += (_, key, _) => KeyboardInput.Set(KeyMap.Translate(key), true);
            keyboard.KeyUp += (_, key, _) => KeyboardInput.Set(KeyMap.Translate(key), false);
        }

        _game = new Game_Main();
        if (!_game.initSuccess)
            Console.Error.WriteLine("Game_Main initialisation failed; see the diagnostic message.");

        // Verification hook: THSSS_START_STATE=All skips straight to the ending so
        // the dialogue box can be checked without playing through a stage.
        var startState = Environment.GetEnvironmentVariable("THSSS_START_STATE");
        if (_game.initSuccess && !string.IsNullOrEmpty(startState))
            _game.RequestState(startState);
    }

    private static void OnRender(double deltaTime)
    {
        if (_game == null) return;

        // The logical surface is fixed at 640x480; publish the real window size so
        // the sprite batcher can letterbox it exactly like the original window.
        var framebuffer = _window.FramebufferSize;
        if (framebuffer.X > 0 && framebuffer.Y > 0)
        {
            _game.DeviceMain.FramebufferWidth = framebuffer.X;
            _game.DeviceMain.FramebufferHeight = framebuffer.Y;
        }

        _frame++;

        // Verification hook: inject scripted key presses so the input path can be
        // exercised without a human at the keyboard. Format: "frame:key,frame:key".
        InjectScriptedInput(_frame);

        // MainProcess owns update + clear + draw for the whole frame, so it has to run
        // inside the render callback: GLFW presents the buffer right after it returns.
        _game.MainProcess();

        var shotPath = Environment.GetEnvironmentVariable("THSSS_SCREENSHOT");
        if (!string.IsNullOrEmpty(shotPath) && _frame == int.Parse(
                Environment.GetEnvironmentVariable("THSSS_SHOT_FRAME") ?? "120"))
        {
            SaveScreenshot(shotPath);
        }
    }

    private static string[] _scriptedInput;
    private static int _scriptedIndex;

    /// <summary>
    /// Feeds the same KeyboardInput state the real keyboard events write to, at the
    /// frames listed in THSSS_INPUT ("180:Down,200:Enter,..."). Each key is held for
    /// a few frames then released, mimicking a tap.
    /// </summary>
    private static Silk.NET.Input.Key _heldKey;
    private static bool _holdingKey;
    private static int _releaseAtFrame;

    private static void InjectScriptedInput(int frame)
    {
        // A scripted key must be released again: the game only reacts on the
        // rising edge (its "LastZ == 0" style debounce ignores a stuck key).
        if (_holdingKey && frame >= _releaseAtFrame)
        {
            KeyboardInput.Set(KeyMap.Translate(_heldKey), false);
            _holdingKey = false;
        }

        _scriptedInput ??= (Environment.GetEnvironmentVariable("THSSS_INPUT") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (_scriptedIndex >= _scriptedInput.Length) return;

        var parts = _scriptedInput[_scriptedIndex].Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int at)) return;
        if (frame < at) return;

        if (Enum.TryParse<Key>(parts[1], true, out var key))
        {
            KeyboardInput.Set(KeyMap.Translate(key), true);
            _heldKey = key;
            _holdingKey = true;
            _releaseAtFrame = frame + 4;
            Console.WriteLine($"[INPUT] frame={frame} press {key}");
        }
        _scriptedIndex++;
    }

    private static unsafe void SaveScreenshot(string path)
    {
        var size = _window.FramebufferSize;
        int width = size.X, height = size.Y;
        var pixels = new byte[width * height * 4];
        fixed (byte* data = pixels)
        {
            _gl.ReadPixels(0, 0, (uint)width, (uint)height,
                Silk.NET.OpenGL.GLEnum.Rgba, Silk.NET.OpenGL.GLEnum.UnsignedByte, data);
        }
        int nonBlack = 0;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0) nonBlack++;
            pixels[i + 3] = 255;
        }
        // ReadPixels returns rows bottom-up; flip so the PNG matches the screen.
        int stride = width * 4;
        var flipped = new byte[pixels.Length];
        for (int row = 0; row < height; row++)
            System.Buffer.BlockCopy(pixels, (height - 1 - row) * stride, flipped, row * stride, stride);
        using var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(flipped, width, height);
        using var stream = File.Create(path);
        image.Save(stream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        Console.WriteLine($"[SHOT] {path} {width}x{height} nonBlack={nonBlack}/{width * height}");
    }
}
