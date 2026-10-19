using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace THSSS.Config;

/// <summary>
/// GBK-preserving INI reader/writer. The shipped Setting.INI is ANSI (code page 936);
/// unrelated lines, comments and section order stay byte-identical on save.
/// </summary>
internal sealed class IniFile
{
    private static readonly Encoding Gbk = CreateGbk();
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    private readonly List<string> _lines = new();
    private readonly string _path;
    private readonly Encoding _encoding;

    private IniFile(string path, Encoding encoding, string text)
    {
        _path = path;
        _encoding = encoding;
        _lines.AddRange(text.Replace("\r\n", "\n").Split('\n'));
        if (_lines.Count > 0 && _lines[^1].Length == 0) _lines.RemoveAt(_lines.Count - 1);
    }

    public static Encoding CreateGbk()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(936);
    }

    public bool Exists { get; private set; }

    public static IniFile Open(string path)
    {
        if (!File.Exists(path)) return new IniFile(path, Gbk, string.Empty) { Exists = false };
        byte[] bytes = File.ReadAllBytes(path);
        var (encoding, text) = Decode(bytes);
        return new IniFile(path, encoding, text) { Exists = true };
    }

    private static (Encoding Encoding, string Text) Decode(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return (Utf8, Utf8.GetString(bytes, 3, bytes.Length - 3));
        try { return (Utf8, Utf8.GetString(bytes)); }
        catch (DecoderFallbackException) { return (Gbk, Gbk.GetString(bytes)); }
    }

    public string Read(string section, string key, string fallback)
    {
        string? current = null;
        foreach (string line in _lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                current = trimmed[1..^1].Trim();
                continue;
            }
            if (!string.Equals(current, section, StringComparison.OrdinalIgnoreCase)) continue;
            int equals = trimmed.IndexOf('=');
            if (equals <= 0) continue;
            if (!string.Equals(trimmed[..equals].Trim(), key, StringComparison.OrdinalIgnoreCase)) continue;
            return trimmed[(equals + 1)..].Trim();
        }
        return fallback;
    }

    public void Write(string section, string key, string value)
    {
        int sectionStart = -1, sectionEnd = _lines.Count;
        for (int i = 0; i < _lines.Count; i++)
        {
            string trimmed = _lines[i].Trim();
            if (!trimmed.StartsWith('[') || !trimmed.EndsWith(']')) continue;
            if (sectionStart >= 0) { sectionEnd = i; break; }
            if (string.Equals(trimmed[1..^1].Trim(), section, StringComparison.OrdinalIgnoreCase)) sectionStart = i;
        }
        if (sectionStart < 0)
        {
            if (_lines.Count > 0 && _lines[^1].Trim().Length != 0) _lines.Add(string.Empty);
            _lines.Add($"[{section}]");
            _lines.Add($"{key}={value}");
            return;
        }
        for (int i = sectionStart + 1; i < sectionEnd; i++)
        {
            string trimmed = _lines[i].Trim();
            int equals = trimmed.IndexOf('=');
            if (equals <= 0) continue;
            if (!string.Equals(trimmed[..equals].Trim(), key, StringComparison.OrdinalIgnoreCase)) continue;
            _lines[i] = $"{trimmed[..equals].Trim()}={value}";
            return;
        }
        _lines.Insert(sectionEnd, $"{key}={value}");
    }

    /// <summary>Writes a temporary sibling then atomically replaces the target.</summary>
    public void Save()
    {
        string full = Path.GetFullPath(_path);
        string? directory = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        string temp = full + ".tmp";
        File.WriteAllText(temp, string.Join("\r\n", _lines) + "\r\n", _encoding);
        File.Move(temp, full, overwrite: true);
        Exists = true;
    }

    public string EncodingName => _encoding.WebName;
}
