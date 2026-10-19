using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SlimDX.DirectInput;

/// <summary>
/// Physical keyboard state captured by the window layer. DirectInput has no
/// cross-platform backend, so this fills the gap for the game's KeyboardCapture:
/// the window publishes press/release events, KeyboardState only reads them.
/// </summary>
public static class KeyboardInput
{
    private static readonly ConcurrentDictionary<Key, bool> Pressed = new();

    public static void Set(Key key, bool isDown)
    {
        if (isDown) Pressed[key] = true;
        else Pressed.TryRemove(key, out _);
    }

    public static bool IsPressed(Key key) => Pressed.ContainsKey(key);

    /// <summary>Copy of the keys currently held, used to build a frame snapshot.</summary>
    public static System.Collections.Generic.List<Key> Snapshot()
        => new(Pressed.Keys);

    public static void Clear() => Pressed.Clear();
}

/// <summary>Maps Silk.NET key codes onto the DirectInput subset the game uses.</summary>
public static class KeyMap
{
    public static Key Translate(Silk.NET.Input.Key key) => key switch
    {
        Silk.NET.Input.Key.Escape => Key.Escape,
        Silk.NET.Input.Key.Enter => Key.Return,
        Silk.NET.Input.Key.KeypadEnter => Key.Return,
        Silk.NET.Input.Key.Space => Key.Space,
        Silk.NET.Input.Key.Tab => Key.Tab,
        Silk.NET.Input.Key.Backspace => Key.Back,
        Silk.NET.Input.Key.Insert => Key.Insert,
        Silk.NET.Input.Key.Delete => Key.Delete,
        Silk.NET.Input.Key.Home => Key.Home,
        Silk.NET.Input.Key.End => Key.End,
        Silk.NET.Input.Key.PageUp => Key.PageUp,
        Silk.NET.Input.Key.PageDown => Key.PageDown,
        Silk.NET.Input.Key.Left => Key.LeftArrow,
        Silk.NET.Input.Key.Right => Key.RightArrow,
        Silk.NET.Input.Key.Up => Key.UpArrow,
        Silk.NET.Input.Key.Down => Key.DownArrow,
        Silk.NET.Input.Key.ShiftLeft => Key.LeftShift,
        Silk.NET.Input.Key.ShiftRight => Key.RightShift,
        Silk.NET.Input.Key.ControlLeft => Key.LeftControl,
        Silk.NET.Input.Key.ControlRight => Key.RightControl,
        Silk.NET.Input.Key.AltLeft => Key.LeftAlt,
        Silk.NET.Input.Key.AltRight => Key.RightAlt,
        Silk.NET.Input.Key.Minus => Key.Minus,
        Silk.NET.Input.Key.Equal => Key.Equals,
        Silk.NET.Input.Key.Comma => Key.Comma,
        Silk.NET.Input.Key.Period => Key.Period,
        Silk.NET.Input.Key.Slash => Key.Slash,
        Silk.NET.Input.Key.Semicolon => Key.Semicolon,
        Silk.NET.Input.Key.Apostrophe => Key.Apostrophe,
        Silk.NET.Input.Key.GraveAccent => Key.Grave,
        Silk.NET.Input.Key.BackSlash => Key.Backslash,
        Silk.NET.Input.Key.LeftBracket => Key.LeftBracket,
        Silk.NET.Input.Key.RightBracket => Key.RightBracket,
        Silk.NET.Input.Key.CapsLock => Key.CapsLock,
        Silk.NET.Input.Key.NumLock => Key.NumLock,
        Silk.NET.Input.Key.ScrollLock => Key.ScrollLock,
        Silk.NET.Input.Key.PrintScreen => Key.PrintScreen,
        Silk.NET.Input.Key.Pause => Key.Pause,
        Silk.NET.Input.Key.F1 => Key.F1, Silk.NET.Input.Key.F2 => Key.F2,
        Silk.NET.Input.Key.F3 => Key.F3, Silk.NET.Input.Key.F4 => Key.F4,
        Silk.NET.Input.Key.F5 => Key.F5, Silk.NET.Input.Key.F6 => Key.F6,
        Silk.NET.Input.Key.F7 => Key.F7, Silk.NET.Input.Key.F8 => Key.F8,
        Silk.NET.Input.Key.F9 => Key.F9, Silk.NET.Input.Key.F10 => Key.F10,
        Silk.NET.Input.Key.F11 => Key.F11, Silk.NET.Input.Key.F12 => Key.F12,
        Silk.NET.Input.Key.Number0 => Key.D0, Silk.NET.Input.Key.Number1 => Key.D1,
        Silk.NET.Input.Key.Number2 => Key.D2, Silk.NET.Input.Key.Number3 => Key.D3,
        Silk.NET.Input.Key.Number4 => Key.D4, Silk.NET.Input.Key.Number5 => Key.D5,
        Silk.NET.Input.Key.Number6 => Key.D6, Silk.NET.Input.Key.Number7 => Key.D7,
        Silk.NET.Input.Key.Number8 => Key.D8, Silk.NET.Input.Key.Number9 => Key.D9,
        Silk.NET.Input.Key.Keypad0 => Key.Numpad0, Silk.NET.Input.Key.Keypad1 => Key.Numpad1,
        Silk.NET.Input.Key.Keypad2 => Key.Numpad2, Silk.NET.Input.Key.Keypad3 => Key.Numpad3,
        Silk.NET.Input.Key.Keypad4 => Key.Numpad4, Silk.NET.Input.Key.Keypad5 => Key.Numpad5,
        Silk.NET.Input.Key.Keypad6 => Key.Numpad6, Silk.NET.Input.Key.Keypad7 => Key.Numpad7,
        Silk.NET.Input.Key.Keypad8 => Key.Numpad8, Silk.NET.Input.Key.Keypad9 => Key.Numpad9,
        Silk.NET.Input.Key.KeypadDecimal => Key.NumpadPeriod,
        Silk.NET.Input.Key.KeypadSubtract => Key.NumpadMinus,
        Silk.NET.Input.Key.KeypadAdd => Key.NumpadPlus,
        Silk.NET.Input.Key.A => Key.A, Silk.NET.Input.Key.B => Key.B,
        Silk.NET.Input.Key.C => Key.C, Silk.NET.Input.Key.D => Key.D,
        Silk.NET.Input.Key.E => Key.E, Silk.NET.Input.Key.F => Key.F,
        Silk.NET.Input.Key.G => Key.G, Silk.NET.Input.Key.H => Key.H,
        Silk.NET.Input.Key.I => Key.I, Silk.NET.Input.Key.J => Key.J,
        Silk.NET.Input.Key.K => Key.K, Silk.NET.Input.Key.L => Key.L,
        Silk.NET.Input.Key.M => Key.M, Silk.NET.Input.Key.N => Key.N,
        Silk.NET.Input.Key.O => Key.O, Silk.NET.Input.Key.P => Key.P,
        Silk.NET.Input.Key.Q => Key.Q, Silk.NET.Input.Key.R => Key.R,
        Silk.NET.Input.Key.S => Key.S, Silk.NET.Input.Key.T => Key.T,
        Silk.NET.Input.Key.U => Key.U, Silk.NET.Input.Key.V => Key.V,
        Silk.NET.Input.Key.W => Key.W, Silk.NET.Input.Key.X => Key.X,
        Silk.NET.Input.Key.Y => Key.Y, Silk.NET.Input.Key.Z => Key.Z,
        _ => Key.Unknown,
    };
}
