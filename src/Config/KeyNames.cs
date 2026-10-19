using System;
using System.Collections.Generic;

namespace THSSS.Config;

/// <summary>
/// Canonical Setting.INI key names. The game turns physical keys into these names via
/// LegacyKeyMap.Name/Translate, so only names that translate back are offered.
/// </summary>
internal static class KeyNames
{
    public static readonly string[] Values = Build();

    private static string[] Build()
    {
        var names = new List<string>();
        foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ") names.Add(c.ToString());
        for (int i = 0; i <= 9; i++) names.Add("D" + i);
        for (int i = 1; i <= 12; i++) names.Add("F" + i);
        names.AddRange(new[]
        {
            "Up", "Down", "Left", "Right",
            "ShiftKey", "ControlKey", "Menu",
            "Space", "Return", "Escape", "Back", "Tab", "Delete", "Insert",
            "Home", "End", "PageUp", "PageDown",
            "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4",
            "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9",
            "OemSemicolon", "Oemplus", "Oemcomma", "OemMinus", "OemPeriod",
            "OemOpenBrackets", "OemPipe", "OemCloseBrackets", "OemQuotes", "Oemtilde",
        });
        return names.ToArray();
    }

    /// <summary>Index of a stored name, or -1 when the file holds an unknown name.</summary>
    public static int IndexOf(string? name) => string.IsNullOrEmpty(name) ? -1 : Array.IndexOf(Values, name);

    public static string At(int index) => Values[Math.Clamp(index, 0, Values.Length - 1)];
}
