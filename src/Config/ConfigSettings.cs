using System;
using System.Globalization;
using System.Linq;

namespace THSSS.Config;

/// <summary>
/// Exactly the Setting.INI keys the original Config.exe reads and writes. Values the
/// original dialog never touches (volume, Clear, renderer flags) are left alone.
/// </summary>
internal sealed class ConfigSettings
{
    public const string FileName = "Setting.INI";

    public static readonly string[] FontChoices = { "宋体", "黑体", "楷体", "仿宋", "新宋体", "幼圆", "隶书" };

    public static readonly string[] ControllerButtons = Enumerable.Range(1, 12).Select(i => "Button" + i).ToArray();

    public string Left = "Left";
    public string Right = "Right";
    public string Up = "Up";
    public string Down = "Down";
    public string Slow = "ShiftKey";
    public string Shoot = "Z";
    public string Bomb = "X";

    public bool VerticalSync;
    public bool FullWindow;
    public int PresentInterval = 1;
    public bool UseDirectInput = true;

    public int JoySkip = 6;
    public int JoyPause = 11;
    public int JoySlow = 7;
    public int JoyShoot = 3;
    public int JoyBomb;

    public string FontType = "楷体";

    public static ConfigSettings Load(IniFile ini)
    {
        var s = new ConfigSettings();
        s.Left = ini.Read("Key", "Left", s.Left);
        s.Right = ini.Read("Key", "Right", s.Right);
        s.Up = ini.Read("Key", "Up", s.Up);
        s.Down = ini.Read("Key", "Down", s.Down);
        s.Slow = ini.Read("Key", "Slow", s.Slow);
        s.Shoot = ini.Read("Key", "Shoot", s.Shoot);
        s.Bomb = ini.Read("Key", "Bomb", s.Bomb);
        s.VerticalSync = ini.Read("DirectX", "VerticalSync", "0") == "1";
        s.FullWindow = ini.Read("DirectX", "FullWindow", "0") == "1";
        s.PresentInterval = Clamp(ParseInt(ini.Read("Mode", "PresentInterval", "1"), 1), 1, 3);
        s.UseDirectInput = ini.Read("DirectInput", "UseDirectInput", "1") == "1";
        s.JoySkip = Clamp(ParseInt(ini.Read("JoyStick", "Skip", "6"), 6), 0, 11);
        s.JoyPause = Clamp(ParseInt(ini.Read("JoyStick", "Pause", "11"), 11), 0, 11);
        s.JoySlow = Clamp(ParseInt(ini.Read("JoyStick", "Slow", "7"), 7), 0, 11);
        s.JoyShoot = Clamp(ParseInt(ini.Read("JoyStick", "Shoot", "3"), 3), 0, 11);
        s.JoyBomb = Clamp(ParseInt(ini.Read("JoyStick", "Bomb", "0"), 0), 0, 11);
        s.FontType = ini.Read("Game", "FontType", s.FontType);
        return s;
    }

    public void Save(IniFile ini)
    {
        ini.Write("Key", "Left", Left);
        ini.Write("Key", "Right", Right);
        ini.Write("Key", "Up", Up);
        ini.Write("Key", "Down", Down);
        ini.Write("Key", "Slow", Slow);
        ini.Write("Key", "Shoot", Shoot);
        ini.Write("Key", "Bomb", Bomb);
        ini.Write("DirectX", "VerticalSync", VerticalSync ? "1" : "0");
        ini.Write("DirectX", "FullWindow", FullWindow ? "1" : "0");
        ini.Write("Mode", "PresentInterval", PresentInterval.ToString(CultureInfo.InvariantCulture));
        ini.Write("DirectInput", "UseDirectInput", UseDirectInput ? "1" : "0");
        ini.Write("JoyStick", "Skip", JoySkip.ToString(CultureInfo.InvariantCulture));
        ini.Write("JoyStick", "Pause", JoyPause.ToString(CultureInfo.InvariantCulture));
        ini.Write("JoyStick", "Slow", JoySlow.ToString(CultureInfo.InvariantCulture));
        ini.Write("JoyStick", "Shoot", JoyShoot.ToString(CultureInfo.InvariantCulture));
        ini.Write("JoyStick", "Bomb", JoyBomb.ToString(CultureInfo.InvariantCulture));
        ini.Write("Game", "FontType", FontType);
    }

    private static int ParseInt(string text, int fallback)
        => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;

    private static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);
}
