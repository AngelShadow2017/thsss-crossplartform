using System;
using System.Windows.Forms;
using SilkKey = Silk.NET.Input.Key;
using SlimDX.DirectInput;

namespace Shooting;

public static class LegacyKeyMap
{
    public static Keys Translate(SilkKey key) => FromDirectInput(KeyMap.Translate(key));
    public static string Name(Keys key) => (int)key == 13 ? "Return" : key.ToString();
    public static Keys FromDirectInput(Key key) => key switch
    {
        Key.Return => Keys.Return,
        Key.LeftShift or Key.RightShift => Keys.ShiftKey,
        Key.LeftControl or Key.RightControl => Keys.ControlKey,
        Key.LeftAlt or Key.RightAlt => Keys.Menu,
        Key.Up => Keys.Up, Key.Down => Keys.Down, Key.Left => Keys.Left, Key.Right => Keys.Right,
        Key.Equals => Keys.Oemplus, Key.Minus => Keys.OemMinus,
        Key.Comma => Keys.Oemcomma, Key.Period => Keys.OemPeriod,
        Key.Slash => Keys.OemQuestion, Key.Semicolon => Keys.OemSemicolon,
        Key.Apostrophe => Keys.OemQuotes, Key.Grave => Keys.Oemtilde,
        Key.Backslash => Keys.OemPipe, Key.LeftBracket => Keys.OemOpenBrackets,
        Key.RightBracket => Keys.OemCloseBrackets, Key.ScrollLock => Keys.Scroll,
        Key.NumpadPeriod => Keys.Decimal, Key.NumpadMinus => Keys.Subtract,
        Key.NumpadPlus => Keys.Add,
        _ => Enum.TryParse<Keys>(key.ToString().Replace("Numpad", "NumPad"), out var result) ? result : Keys.None
    };

    public static KeyClass Capture(MapKey map, KeyboardState state)
    {
        bool Down(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (name == "Enter") name = "Return";
            foreach (Key key in Enum.GetValues<Key>())
                if (state.IsPressed(key) && Name(FromDirectInput(key)) == name) return true;
            return false;
        }
        return new KeyClass
        {
            ArrowLeft = Down(map.Left), ArrowRight = Down(map.Right),
            ArrowUp = Down(map.Up), ArrowDown = Down(map.Down),
            Key_Shift = Down(map.Slow), Key_Z = Down(map.Shoot) || Down("Return"),
            Key_X = Down(map.Bomb), Key_C = Down("R"), Key_Ctrl = Down("ControlKey"),
            Key_ESC = Down("Escape"), Key_plus = Down("Oemplus") || Down("ControlKey"),
            Key_minus = Down("OemMinus") || Down("ShiftKey")
        };
    }
}
