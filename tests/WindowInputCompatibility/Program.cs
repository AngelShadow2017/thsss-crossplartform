using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Shooting;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SlimDX.DirectInput;
using SlimDX.Windows;
using SilkKey = Silk.NET.Input.Key;
using DIKey = SlimDX.DirectInput.Key;

internal static class Checks
{
    private static int _assertions;
    internal static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _assertions++;
    }
    static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--lock-probe")
        {
            try { using var instance = new DataRootInstance(args[1]); return 0; }
            catch (IOException) { return 23; }
        }
        if (args.Length == 2 && args[0] == "--launch-game") { GameSmoke.Launch(args[1]); return Environment.ExitCode; }
        try
        {
            string Bits(int code) => (System.Numerics.BigInteger.One << code).ToString("x");
            Check(!JoystickDevices.HasControllerButtons(Bits(0x110)),"Linux mouse buttons do not constitute a game controller");
            Check(!JoystickDevices.HasControllerButtons(Bits(0x14a)),"Linux touch contact does not constitute a game controller");
            Check(JoystickDevices.HasControllerButtons(Bits(0x130)),"Linux gamepad face button recognized");
            Check(JoystickDevices.HasControllerButtons(Bits(0x120)),"Linux joystick trigger recognized");
            Check(JoystickDevices.HasControllerButtons(Bits(0x220)),"Linux dpad recognized");
            FakeWindowAndInput();
            FrameTiming.Run();
            LockAndDiagnostics();
            if (args.Contains("--glfw")) { RealWindow(); FrameTiming.RealLoop(); }
            if (args.Contains("--game-smoke")) GameSmoke.Run();
            Console.WriteLine($"PASS WindowInputCompatibility: {_assertions} assertions; GLFW={args.Contains("--glfw")}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void Set(object obj, string field, object value) => obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
    static void Bind(Game_Main game, Control form, string name, bool down)
    {
        var method = typeof(Game_Main).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        var handler = (KeyEventHandler)method.CreateDelegate(typeof(KeyEventHandler), game);
        if (down) form.KeyDown += handler; else form.KeyUp += handler;
    }
    static void FakeWindowAndInput()
    {
        var window = Fake.Create<IWindow>();
        var backend = Fake.Of(window);
        backend.Values["Size"] = new Vector2D<int>(640, 480);
        var context = Fake.Create<IGLContext>();
        backend.Values["GLContext"] = context;
        var form = new RenderForm("test", 640, 480);
        form.AttachWindow(window);
        Control control = form;
        Form asForm = form;
        int resize = 0, close = 0;
        control.Resize += (_, _) => resize++;
        asForm.FormClosing += (_, _) => close++;
        backend.OnCall = (name, _) => { if (name == "Close") { backend.Values["IsClosing"] = true; backend.Raise("Closing"); } };
        asForm.ClientSize = new Size(800, 600);
        Check(window.Size == new Vector2D<int>(800, 600), "base Form.ClientSize forwards");
        int sizeCalls = backend.Calls.Count(c => c.Name == "set_Size");
        asForm.ClientSize = new Size(800, 600);
        Check(backend.Calls.Count(c => c.Name == "set_Size") == sizeCalls, "same geometry not rescheduled");
        backend.Raise("Resize", new Vector2D<int>(800, 600));
        Check(resize == 1, "base Control.Resize forwarded");
        form.ConfigurePresentation(true, true, 3);
        Check(window.WindowState == Silk.NET.Windowing.WindowState.Fullscreen && window.VSync, "fullscreen/vsync applied");
        Check(window.FramesPerSecond == 0 && Fake.Of(context).Calls.Last().Args[0].Equals(3), "vsync is sole limiter, interval three");
        form.ConfigurePresentation(false, false, 2);
        Check(window.WindowState == Silk.NET.Windowing.WindowState.Normal && !window.VSync && window.FramesPerSecond == 30, "software interval pacing");
        Check(CursorState.Visible, "windowed cursor visible");
        form.ConfigurePresentation(false,true,2);
        Check(!window.VSync && window.FramesPerSecond==30 && Fake.Of(context).Calls.Last().Args[0].Equals(0),"windowed interval-two ignores VSync and keeps original software pacing");
        form.ConfigurePresentation(false,true,1);
        Check(window.VSync && window.FramesPerSecond==0 && Fake.Of(context).Calls.Last().Args[0].Equals(1),"windowed interval-one follows VSync");
        for (int interval = 1; interval <= 3; interval++)
        {
            form.ConfigurePresentation(false, false, interval);
            Check(window.FramesPerSecond == 60.0 / interval, $"interval {interval} preserves logical/present ratio");
        }
        bool invalid = false;
        try { form.ConfigurePresentation(false, false, 0); } catch (ArgumentOutOfRangeException) { invalid = true; }
        Check(invalid, "invalid interval rejected");

        var keyboard = Fake.Create<IKeyboard>();
        var input = Fake.Create<IInputContext>();
        var inputFake = Fake.Of(input);
        inputFake.Values["Keyboards"] = new[] { keyboard };
        inputFake.Values["Mice"] = Array.Empty<IMouse>();
        inputFake.Values["Joysticks"] = Array.Empty<IJoystick>();
        using var bridge = new WindowInputBridge(form, input);
        var game = (Game_Main)RuntimeHelpers.GetUninitializedObject(typeof(Game_Main));
        var data = (GlobalDataPackage)RuntimeHelpers.GetUninitializedObject(typeof(GlobalDataPackage));
        data.KClass = new KeyClass();
        game.GlobalData = data;
        game.initSuccess = true;
        var map = new MapKey { Left = "A", Right = "D", Up = "W", Down = "S", Slow = "ShiftKey", Shoot = "J", Bomb = "K" };
        Set(game, "mapkey", map);
        Bind(game, control, "Form_Main_KeyDown", true);
        Bind(game, control, "Form_Main_KeyUp", false);
        foreach (var (key, inspect) in new (SilkKey, Func<KeyClass, bool>)[]
        {
            (SilkKey.A, k => k.ArrowLeft), (SilkKey.D, k => k.ArrowRight), (SilkKey.W, k => k.ArrowUp),
            (SilkKey.S, k => k.ArrowDown), (SilkKey.ShiftLeft, k => k.Key_Shift),
            (SilkKey.J, k => k.Key_Z), (SilkKey.K, k => k.Key_X), (SilkKey.Enter, k => k.Key_Z),
            (SilkKey.Escape, k => k.Key_ESC), (SilkKey.Equal, k => k.Key_plus), (SilkKey.Minus, k => k.Key_minus)
        })
        {
            Fake.Of(keyboard).Raise("KeyDown", keyboard, key, 0);
            Check(inspect(data.KClass), $"event mode {key} down reaches original handler");
            var snapshot = new Keyboard().GetCurrentState();
            Check(inspect(LegacyKeyMap.Capture(map, snapshot)), $"snapshot mode configured {key}");
            Fake.Of(keyboard).Raise("KeyUp", keyboard, key, 0);
            Check(!inspect(data.KClass), $"event mode {key} up");
            Check(!new Keyboard().GetCurrentState().IsPressed(KeyMap.Translate(key)), $"snapshot mode {key} released");
        }
        Fake.Of(keyboard).Raise("KeyDown",keyboard,SilkKey.Enter,0);
        Fake.Of(keyboard).Raise("KeyDown",keyboard,SilkKey.KeypadEnter,0);
        Fake.Of(keyboard).Raise("KeyUp",keyboard,SilkKey.Enter,0);
        Check(new Keyboard().GetCurrentState().IsPressed(DIKey.Return) && data.KClass.Key_Z,"Return stays held while keypad Enter remains down");
        Fake.Of(keyboard).Raise("KeyUp",keyboard,SilkKey.KeypadEnter,0);
        Check(!new Keyboard().GetCurrentState().IsPressed(DIKey.Return) && !data.KClass.Key_Z,"Return released only after both physical aliases release");
        Check(LegacyKeyMap.Name(LegacyKeyMap.Translate(SilkKey.Enter)) == "Return", "legacy Return name");
        Check(LegacyKeyMap.Name(LegacyKeyMap.Translate(SilkKey.ShiftRight)) == "ShiftKey", "legacy ShiftKey name");
        foreach (var key in new[] { SilkKey.F8, SilkKey.P, SilkKey.Tab, SilkKey.Backspace, SilkKey.Keypad1, SilkKey.Semicolon, SilkKey.AltLeft })
            Check(LegacyKeyMap.Translate(key) != Keys.None, $"complete legacy mapping {key}");
        var deactivate = typeof(Game_Main).GetMethod("Form_Main_Deactivate", BindingFlags.NonPublic | BindingFlags.Instance);
        var activate = typeof(Game_Main).GetMethod("Form_Main_Activated", BindingFlags.NonPublic | BindingFlags.Instance);
        asForm.Deactivate += (EventHandler)deactivate.CreateDelegate(typeof(EventHandler), game);
        asForm.Activated += (EventHandler)activate.CreateDelegate(typeof(EventHandler), game);
        Fake.Of(keyboard).Raise("KeyDown", keyboard, SilkKey.A, 0);
        backend.Raise("FocusChanged", false);
        Check(!data.KClass.ArrowLeft && KeyboardInput.Snapshot().Count == 0, "focus loss clears event and snapshot keys");
        Check((bool)typeof(Game_Main).GetField("OnPause", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game), "focus loss requests automatic pause");
        Fake.Of(keyboard).Raise("KeyDown", keyboard, SilkKey.J, 0);
        Check(!data.KClass.Key_Z && KeyboardInput.Snapshot().Count == 0, "background input ignored");
        backend.Raise("FocusChanged", true);
        Check(!data.KClass.ArrowLeft && !data.KClass.Key_Z, "focus return no stuck keys");
        Fake.Of(keyboard).Raise("KeyDown", keyboard, SilkKey.ShiftLeft, 0);
        Fake.Of(keyboard).Raise("KeyDown", keyboard, SilkKey.ShiftRight, 0);
        Fake.Of(keyboard).Raise("KeyUp", keyboard, SilkKey.ShiftLeft, 0);
        Check(data.KClass.Key_Shift, "both shift keys release independently");
        Fake.Of(keyboard).Raise("KeyUp", keyboard, SilkKey.ShiftRight, 0);
        Check(!data.KClass.Key_Shift, "last shift release forwarded");
        Joysticks(input, bridge);
        asForm.Close(); asForm.Close();
        Check(window.IsClosing && close == 1, "base Form.Close reaches live window; closing once");
        form.DetachWindow();
    }
    static void Joysticks(IInputContext input, WindowInputBridge bridge)
    {
        var joystick = Fake.Create<IJoystick>();
        var fake = Fake.Of(joystick);
        fake.Values["IsConnected"] = true;
        fake.Values["Index"] = 7;
        fake.Values["Name"] = "fixture joystick";
        fake.Values["Axes"] = new[] { new Axis(0, -0.8f), new Axis(1, 0.9f) };
        fake.Values["Buttons"] = new[] { new Silk.NET.Input.Button(ButtonName.Unknown, 0, true), new Silk.NET.Input.Button(ButtonName.Unknown, 1, false), new Silk.NET.Input.Button(ButtonName.Unknown, 2, true) };
        Fake.Of(input).Values["Joysticks"] = new[] { joystick };
        int changes = 0;
        bridge.DevicesChanged += () => changes++;
        Fake.Of(input).Raise("ConnectionChanged", joystick, true);
        var direct = new DirectInput();
        var devices = direct.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly).ToArray();
        Check(devices.Length == 1 && devices[0].InstanceName == "fixture joystick" && changes == 1, "Silk hot-connect enumerates real source");
        var device = new Joystick(direct, devices[0].InstanceGuid);
        var snapshot = device.GetCurrentState();
        Check(snapshot.X == -800 && snapshot.Y == 900, "axes normalized to original -1000..1000 range");
        Check(snapshot.IsPressed(0) && !snapshot.IsPressed(1) && snapshot.IsPressed(JoystickOffset.Buttons2) && !snapshot.IsPressed(-1) && !snapshot.IsPressed(999), "original indexed button API");
        MapJoyStick.Shoot = 2; MapJoyStick.Slow = 0; MapJoyStick.Bomb = 1;
        MapJoyStick.Skip = 1; MapJoyStick.Pause = 2;
        var capture = new JoystickCapture(IntPtr.Zero);
        var keys = new KeyClass(); capture.UpdateInput(ref keys);
        Check(keys.ArrowLeft && keys.ArrowDown && keys.Key_Z && keys.Key_Shift && keys.Key_ESC && !keys.Key_X, "original JoystickCapture thresholds and configured buttons");
        var keyboard = new KeyClass { Key_X = true, ArrowUp = true };
        var mixed = new KeyClass(); KeyClass.KeyMix(ref mixed, keys, keyboard);
        Check(mixed.Key_Z && mixed.Key_X && mixed.ArrowUp && mixed.ArrowDown, "keyboard/controller mixing");
        bridge.SetFocus(false);
        Check(device.GetCurrentState().X == 0 && !device.GetCurrentState().IsPressed(0), "controller neutral while unfocused");
        bridge.SetFocus(true);
        Fake.Of(input).Values["Joysticks"] = Array.Empty<IJoystick>();
        Fake.Of(input).Raise("ConnectionChanged", joystick, false);
        Check(!direct.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly).Any() && !device.GetCurrentState().IsPressed(0), "disconnect empties enumeration and releases buttons");
        Check(!direct.GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly).Any(), "controller enumeration respects device class");
    }
    static void LockAndDiagnostics()
    {
        string root = Path.Combine(Path.GetTempPath(), "thsss-window-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using (var first = new DataRootInstance(root))
            {
                var start = new ProcessStartInfo("dotnet") { RedirectStandardError = true, RedirectStandardOutput = true };
                start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
                start.ArgumentList.Add("--lock-probe"); start.ArgumentList.Add(root);
                using var process = Process.Start(start);
                Check(process.WaitForExit(10000) && process.ExitCode == 23, "second process same root rejected");
                string other = Path.Combine(root, "other"); Directory.CreateDirectory(other);
                using var independent = new DataRootInstance(other);
                Check(true, "separate roots allowed");
            }
            using var reopened = new DataRootInstance(root);
            Check(true, "root lock released on exit");
            var original = Console.Error; var errors = new StringWriter();
            try { Console.SetError(errors); MessageBox.Show("broken setting", "fixture error"); }
            finally { Console.SetError(original); }
            Check(errors.ToString().Contains("fixture error") && errors.ToString().Contains("broken setting"), "MessageBox errors observable on stderr");
        }
        finally { Directory.Delete(root, true); }
    }
    static void RealWindow()
    {
        var options = WindowOptions.Default;
        options.IsVisible = false; options.Size = new(640, 480); options.VSync = false;
        using var window = Window.Create(options);
        window.Initialize();
        using var input = window.CreateInput();
        var form = new RenderForm("isolated GLFW fixture", 640, 480);
        form.AttachWindow(window);
        using var bridge = new WindowInputBridge(form, input);
        Form asForm = form;
        int closes = 0;
        asForm.FormClosing += (_, _) => closes++;
        asForm.ClientSize = new Size(800, 600);
        window.DoEvents();
        Check(window.Size == new Vector2D<int>(800, 600), "real hidden GLFW size via base Form");
        Check(ReferenceEquals(window, form.Window), "no secondary window");
        form.ConfigurePresentation(false, false, 2);
        Check(!window.VSync && window.FramesPerSecond == 30, "real pacing settings");
        int frames = 0;
        var watch = Stopwatch.StartNew();
        window.Render += _ => { frames++; if (watch.ElapsedMilliseconds >= 400) asForm.Close(); };
        window.Run();
        Check(closes == 1, "real native close invokes compatibility lifecycle once");
        Check(frames >= 7 && frames <= 25, $"one 30Hz limiter observed: {frames} presents/{watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"GLFW pacing: {frames} presents/{watch.ElapsedMilliseconds}ms; keyboards={input.Keyboards.Count}, joysticks={input.Joysticks.Count}");
        form.DetachWindow();
    }
}
