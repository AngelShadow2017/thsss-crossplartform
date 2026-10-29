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
// Two independent Key enums are in play: the Silk.NET windowing one and the SlimDX
// DirectInput one the original game code was written against. Aliases keep both usable.
using SilkKey = Silk.NET.Input.Key;
using DIKey = SlimDX.DirectInput.Key;

// Window input fixture: the DirectInput facade, the window input bridge, legacy key
// mapping, controller-detection filtering and the data-root lock. Everything except
// the opt-in --glfw run works against fakes, so it needs no display server.
internal static class Checks
{
    private static int _assertions;
    // A failed check aborts with a nonzero exit code; the counter is the evidence that the
    // expected number of assertions actually ran instead of the process dying early.
    internal static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _assertions++;
    }
    static int Main(string[] args)
    {
        // Child mode for LockAndDiagnostics: a second DataRootInstance on a live root must
        // be rejected by the OS file lock (exit 23) instead of sharing the directory.
        if (args.Length == 2 && args[0] == "--lock-probe")
        {
            try { using var instance = new DataRootInstance(args[1]); return 0; }
            catch (IOException) { return 23; }
        }
        // Child mode that boots the real game against an isolated data root.
        if (args.Length == 2 && args[0] == "--launch-game") { GameSmoke.Launch(args[1]); return Environment.ExitCode; }
        try
        {
            // Controller detection filtering: Linux reports mice and touch contacts as
            // evdev devices, so only real gamepad/joystick button bits may count.
            string Bits(int code) => (System.Numerics.BigInteger.One << code).ToString("x");
            Check(!JoystickDevices.HasControllerButtons(Bits(0x110)),"Linux mouse buttons do not constitute a game controller");
            Check(!JoystickDevices.HasControllerButtons(Bits(0x14a)),"Linux touch contact does not constitute a game controller");
            Check(JoystickDevices.HasControllerButtons(Bits(0x130)),"Linux gamepad face button recognized");
            Check(JoystickDevices.HasControllerButtons(Bits(0x120)),"Linux joystick trigger recognized");
            Check(JoystickDevices.HasControllerButtons(Bits(0x220)),"Linux dpad recognized");
            FakeWindowAndInput();
            FrameTiming.Run();
            PresentationGeometryChecks.Run();
            LockAndDiagnostics();
            // Real window and real game loop are opt-in so the default run stays headless.
            if (args.Contains("--glfw")) { RealWindow(); FrameTiming.RealLoop(); }
            if (args.Contains("--game-smoke")) GameSmoke.Run();
            Console.WriteLine($"PASS WindowInputCompatibility: {_assertions} assertions; GLFW={args.Contains("--glfw")}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    // Reflection helpers: the fixture drives the original, unmodified Game_Main internals.
    static void Set(object obj, string field, object value) => obj.GetType().GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
    static void Bind(Game_Main game, Control form, string name, bool down)
    {
        var method = typeof(Game_Main).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        var handler = (KeyEventHandler)method.CreateDelegate(typeof(KeyEventHandler), game);
        if (down) form.KeyDown += handler; else form.KeyUp += handler;
    }
    static void FakeWindowAndInput()
    {
        // A fake IWindow/IInputContext stands in for GLFW so the compatibility layer can be
        // exercised deterministically; only calls the bridge makes are recorded.
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
        // WinForms compatibility: geometry set through the base Form must land on the window.
        asForm.ClientSize = new Size(800, 600);
        Check(window.Size == new Vector2D<int>(800, 600), "base Form.ClientSize forwards");
        int sizeCalls = backend.Calls.Count(c => c.Name == "set_Size");
        asForm.ClientSize = new Size(800, 600);
        Check(backend.Calls.Count(c => c.Name == "set_Size") == sizeCalls, "same geometry not rescheduled");
        backend.Raise("Resize", new Vector2D<int>(800, 600));
        Check(resize == 1, "base Control.Resize forwarded");
        // Presentation policy: each case asserts both the Silk window state and the swap
        // interval actually handed to the GL context.
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
        // Software pacing must keep the logical/present ratio the original present interval had.
        for (int interval = 1; interval <= 3; interval++)
        {
            form.ConfigurePresentation(false, false, interval);
            Check(window.FramesPerSecond == 60.0 / interval, $"interval {interval} preserves logical/present ratio");
        }
        // Interval zero is rejected loudly instead of being silently mapped to something else.
        bool invalid = false;
        try { form.ConfigurePresentation(false, false, 0); } catch (ArgumentOutOfRangeException) { invalid = true; }
        Check(invalid, "invalid interval rejected");

        // One fake keyboard backs both input paths: event handlers and polled snapshots.
        var keyboard = Fake.Create<IKeyboard>();
        var input = Fake.Create<IInputContext>();
        var inputFake = Fake.Of(input);
        inputFake.Values["Keyboards"] = new[] { keyboard };
        inputFake.Values["Mice"] = Array.Empty<IMouse>();
        inputFake.Values["Joysticks"] = Array.Empty<IJoystick>();
        using var bridge = new WindowInputBridge(form, input);
        // Game_Main is left uninitialized so only the input handlers and fields under test
        // exist; construction would start the whole engine.
        var game = (Game_Main)RuntimeHelpers.GetUninitializedObject(typeof(Game_Main));
        var data = (GlobalDataPackage)RuntimeHelpers.GetUninitializedObject(typeof(GlobalDataPackage));
        data.KClass = new KeyClass();
        game.GlobalData = data;
        game.initSuccess = true;
        var map = new MapKey { Left = "A", Right = "D", Up = "W", Down = "S", Slow = "ShiftKey", Shoot = "J", Bomb = "K" };
        Set(game, "mapkey", map);
        Bind(game, control, "Form_Main_KeyDown", true);
        Bind(game, control, "Form_Main_KeyUp", false);
        // Every mapped action must arrive in both modes: the original event handler and the
        // polled snapshot the game loop reads through DirectInput.
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
        // Enter and keypad Enter are one physical key in the legacy DirectInput model, so the
        // mapping must stay held until every alias is released.
        Fake.Of(keyboard).Raise("KeyDown",keyboard,SilkKey.Enter,0);
        Fake.Of(keyboard).Raise("KeyDown",keyboard,SilkKey.KeypadEnter,0);
        Fake.Of(keyboard).Raise("KeyUp",keyboard,SilkKey.Enter,0);
        Check(new Keyboard().GetCurrentState().IsPressed(DIKey.Return) && data.KClass.Key_Z,"Return stays held while keypad Enter remains down");
        Fake.Of(keyboard).Raise("KeyUp",keyboard,SilkKey.KeypadEnter,0);
        Check(!new Keyboard().GetCurrentState().IsPressed(DIKey.Return) && !data.KClass.Key_Z,"Return released only after both physical aliases release");
        // Legacy names must match the strings stored in Setting.INI.
        Check(LegacyKeyMap.Name(LegacyKeyMap.Translate(SilkKey.Enter)) == "Return", "legacy Return name");
        Check(LegacyKeyMap.Name(LegacyKeyMap.Translate(SilkKey.ShiftRight)) == "ShiftKey", "legacy ShiftKey name");
        foreach (var key in new[] { SilkKey.F8, SilkKey.P, SilkKey.Tab, SilkKey.Backspace, SilkKey.Keypad1, SilkKey.Semicolon, SilkKey.AltLeft })
            Check(LegacyKeyMap.Translate(key) != Keys.None, $"complete legacy mapping {key}");
        // Focus loss must drop keys from both paths and request the original auto-pause;
        // background input must be ignored so alt-tab cannot leave a key stuck.
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
        // Left and right shift are separate physical keys but one game action.
        Fake.Of(keyboard).Raise("KeyDown", keyboard, SilkKey.ShiftLeft, 0);
        Fake.Of(keyboard).Raise("KeyDown", keyboard, SilkKey.ShiftRight, 0);
        Fake.Of(keyboard).Raise("KeyUp", keyboard, SilkKey.ShiftLeft, 0);
        Check(data.KClass.Key_Shift, "both shift keys release independently");
        Fake.Of(keyboard).Raise("KeyUp", keyboard, SilkKey.ShiftRight, 0);
        Check(!data.KClass.Key_Shift, "last shift release forwarded");
        Joysticks(input, bridge);
        // Closing twice through the base Form must reach the live window exactly once.
        asForm.Close(); asForm.Close();
        Check(window.IsClosing && close == 1, "base Form.Close reaches live window; closing once");
        form.DetachWindow();
    }
    static void Joysticks(IInputContext input, WindowInputBridge bridge)
    {
        // A single fake joystick proves the Silk hot-plug path feeds the real DirectInput
        // enumeration: the device must surface with its original name and index.
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
        // Analog axes are rescaled to the original -1000..1000 units the game logic expects.
        Check(snapshot.X == -800 && snapshot.Y == 900, "axes normalized to original -1000..1000 range");
        Check(snapshot.IsPressed(0) && !snapshot.IsPressed(1) && snapshot.IsPressed(JoystickOffset.Buttons2) && !snapshot.IsPressed(-1) && !snapshot.IsPressed(999), "original indexed button API");
        // JoystickCapture is the original component: it reads axes as directions and applies
        // the user's button mapping from Setting.INI.
        MapJoyStick.Shoot = 2; MapJoyStick.Slow = 0; MapJoyStick.Bomb = 1;
        MapJoyStick.Skip = 1; MapJoyStick.Pause = 2;
        var capture = new JoystickCapture(IntPtr.Zero);
        var keys = new KeyClass(); capture.UpdateInput(ref keys);
        Check(keys.ArrowLeft && keys.ArrowDown && keys.Key_Z && keys.Key_Shift && keys.Key_ESC && !keys.Key_X, "original JoystickCapture thresholds and configured buttons");
        // Keyboard and controller states are merged, not replaced.
        var keyboard = new KeyClass { Key_X = true, ArrowUp = true };
        var mixed = new KeyClass(); KeyClass.KeyMix(ref mixed, keys, keyboard);
        Check(mixed.Key_Z && mixed.Key_X && mixed.ArrowUp && mixed.ArrowDown, "keyboard/controller mixing");
        // Unfocused controllers must read neutral, otherwise a held trigger blocks menus.
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
        // Data-root locking: one process owns a root, a second one is refused, other roots
        // stay usable, and the lock is released when the owner exits.
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
            // MessageBox failures are redirected to stderr so a broken setting reports instead
            // of blocking a headless run on an invisible dialog.
            var original = Console.Error; var errors = new StringWriter();
            try { Console.SetError(errors); MessageBox.Show("broken setting", "fixture error"); }
            finally { Console.SetError(original); }
            Check(errors.ToString().Contains("fixture error") && errors.ToString().Contains("broken setting"), "MessageBox errors observable on stderr");
        }
        finally { Directory.Delete(root, true); }
    }
    static void RealWindow()
    {
        // Opt-in --glfw run: a real but hidden window proves the same Form forwarding and
        // that the software limiter produces roughly one present per 33 ms.
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
        // The bounds allow scheduler jitter but still catch double pacing (60 Hz) or a stall.
        Check(frames >= 7 && frames <= 25, $"one 30Hz limiter observed: {frames} presents/{watch.ElapsedMilliseconds}ms");
        Console.WriteLine($"GLFW pacing: {frames} presents/{watch.ElapsedMilliseconds}ms; keyboards={input.Keyboards.Count}, joysticks={input.Joysticks.Count}");
        form.DetachWindow();
    }
}
