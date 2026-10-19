using System.Diagnostics;
using System.Reflection;
using Shooting;

internal static class GameSmoke
{
    internal static void Launch(string root)
    {
        Environment.SetEnvironmentVariable("THSSS_DATA_ROOT", root);
        Environment.SetEnvironmentVariable("THSSS_WINDOW_HIDDEN", "1");
        Environment.SetEnvironmentVariable("THSSS_START_STATE", "MainMenu");
        Environment.SetEnvironmentVariable("THSSS_INPUT", Environment.GetEnvironmentVariable("THSSS_GAME_SMOKE_KEYS") ?? "480:F8,500:F8,520:F8,540:F8,600:W,700:Enter");
        Environment.SetEnvironmentVariable("ALSOFT_DRIVERS", "null");
        var observe = Task.Run(() =>
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var windowField = typeof(Shooting.Program).GetField("_window", flags);
            Silk.NET.Windowing.IWindow window;
            while ((window = (Silk.NET.Windowing.IWindow)windowField.GetValue(null)) == null) Thread.Sleep(1);
            window.Render += _ =>
            {
                int frame = (int)typeof(Shooting.Program).GetField("_frame", flags).GetValue(null);
                if (frame > 840)
                {
                    Console.Error.WriteLine("Game continued rendering after scripted menu quit.");
                    Environment.ExitCode = 25;
                    window.Close();
                    return;
                }
                if (frame != 601 && frame != 701 && frame != 705) return;
                var game = (Game_Main)typeof(Shooting.Program).GetField("_game", flags).GetValue(null);
                var state = typeof(Game_Main).GetField("PresentState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
                Console.WriteLine($"fixture frame={frame} focus={game.Form_Main.Focused} keys={game.GlobalData.KClass.Key2Hex()} esc={game.GlobalData.KClass.Key_ESC} physical={string.Join(',', SlimDX.DirectInput.KeyboardInput.Snapshot())} state={state.GetType().Name}");
                foreach (var source in JoystickInput.Enumerate(SlimDX.DirectInput.DeviceClass.GameController))
                {
                    var joy = JoystickInput.Read(source.InstanceGuid);
                    Console.WriteLine($"fixture joystick={source.InstanceName} x={joy.X} y={joy.Y} buttons={string.Join(',', joy.Buttons ?? Array.Empty<byte>())}");
                }
            };
        });
        Shooting.Program.Main(Array.Empty<string>());
        observe.Wait();
    }
    internal static void Run()
    {
        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository != null && !File.Exists(Path.Combine(repository.FullName, "src", "THSSS.csproj"))) repository = repository.Parent;
        if (repository == null) throw new DirectoryNotFoundException("Cannot find game assets for smoke fixture.");
        foreach (var scenario in new[] { (Mode:0,Alt:false), (Mode:1,Alt:false), (Mode:1,Alt:true) })
        {
            int mode=scenario.Mode;
            string root = Path.Combine(Path.GetTempPath(), $"thsss-game-window-{mode}-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                foreach (string directory in new[] { "FX", "Model", "Image", "BGM", "Sound", "Story", "CS", "BPic" })
                    Directory.CreateSymbolicLink(Path.Combine(root, directory), Path.Combine(repository.FullName, "game", directory));
                string settings = $"""
                    [DirectX]
                    FullWindow=0
                    AntiAlias=0
                    VerticalSync=0
                    [DirectInput]
                    UseDirectInput={mode}
                    [Mode]
                    Clear=no
                    SingleThreaded=1
                    FixedFPS=0
                    DelayTolerance=10
                    WindowSize=0
                    PresentInterval=1
                    [Key]
                    Left=A
                    Right=D
                    Up=W
                    Down=S
                    Slow=ShiftKey
                    Shoot=J
                    Bomb=K
                    [JoyStick]
                    Skip=0
                    Pause=1
                    Slow=2
                    Shoot=3
                    Bomb=4
                    C=5
                    [Volume]
                    BGMVolume=0
                    SEVolume=0
                    [Game]
                    FontType=DejaVu Sans
                    """;
                File.WriteAllText(Path.Combine(root, "Setting.INI"), settings);
                var start = new ProcessStartInfo("dotnet") { RedirectStandardError = true, RedirectStandardOutput = true };
                start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
                start.ArgumentList.Add("--launch-game"); start.ArgumentList.Add(root);
                if(scenario.Alt) start.Environment["THSSS_GAME_SMOKE_KEYS"]="480:F8,500:F8,520:F8,540:F8,600:W,700:AltLeft,701:F4";
                using var process = Process.Start(start);
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(35000))
                {
                    process.Kill(true); process.WaitForExit();
                    throw new Exception($"UseDirectInput={mode} quit timed out: {stdout.Result}\n{stderr.Result}");
                }
                Checks.Check(process.ExitCode == 0, $"UseDirectInput={mode} game shutdown exit={process.ExitCode}: {stdout.Result}\n{stderr.Result}");
                Checks.Check(stdout.Result.Contains(scenario.Alt ? "frame=701 press F4" : "frame=700 press Enter"), $"UseDirectInput={mode} reached scripted quit (Alt={scenario.Alt}): {stdout.Result}");
                Checks.Check(File.Exists(Path.Combine(root, "Score.dat")), $"UseDirectInput={mode} closing saved isolated Score.dat");
                Checks.Check(stdout.Result.Split("press F8").Length - 1 == 4, $"UseDirectInput={mode} traversed four F8 inputs");
                Console.WriteLine($"Full-game fixture UseDirectInput={mode}: F8 cycle + {(scenario.Alt ? "Alt+F4" : "menu Enter")} quit; isolated Score.dat={new FileInfo(Path.Combine(root, "Score.dat")).Length} bytes; exit=0");
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
