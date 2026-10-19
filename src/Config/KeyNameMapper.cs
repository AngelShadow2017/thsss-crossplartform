using System;
using Avalonia.Input;

namespace THSSS.Config;

/// <summary>
/// Maps a physical key to the name the game's KeyboardCapture understands. Unnamed
/// keys (media keys, modifiers pressed alone) are ignored so a stray click cannot
/// write garbage into Setting.INI.
/// </summary>
internal static class KeyNameMapper
{
    public static string? ToGameName(Key key) => key switch
    {
        Key.Return or Key.Enter => "Return",
        Key.LeftShift or Key.RightShift => "ShiftKey",
        Key.LeftCtrl or Key.RightCtrl => "ControlKey",
        Key.LeftAlt or Key.RightAlt => "Menu",
        Key.OemPlus or Key.Add => "Oemplus",
        Key.OemMinus or Key.Subtract => "OemMinus",
        Key.OemComma => "Oemcomma",
        Key.OemPeriod or Key.Decimal => "OemPeriod",
        Key.OemQuestion => "OemQuestion",
        Key.OemSemicolon => "OemSemicolon",
        Key.OemQuotes => "OemQuotes",
        Key.OemTilde => "Oemtilde",
        Key.OemPipe => "OemPipe",
        Key.OemOpenBrackets => "OemOpenBrackets",
        Key.OemCloseBrackets => "OemCloseBrackets",
        Key.Space => "Space",
        Key.Escape => "Escape",
        Key.Back => "Back",
        Key.Tab => "Tab",
        Key.Delete => "Delete",
        Key.Insert => "Insert",
        Key.Home => "Home",
        Key.End => "End",
        Key.PageUp => "PageUp",
        Key.PageDown => "PageDown",
        Key.Up => "Up",
        Key.Down => "Down",
        Key.Left => "Left",
        Key.Right => "Right",
        Key.NumPad0 or Key.NumPad1 or Key.NumPad2 or Key.NumPad3 or Key.NumPad4
            or Key.NumPad5 or Key.NumPad6 or Key.NumPad7 or Key.NumPad8 or Key.NumPad9
            => "NumPad" + key.ToString()[^1],
        >= Key.A and <= Key.Z => key.ToString(),
        >= Key.D0 and <= Key.D9 => key.ToString(),
        >= Key.F1 and <= Key.F12 => key.ToString(),
        _ => null,
    };
}
