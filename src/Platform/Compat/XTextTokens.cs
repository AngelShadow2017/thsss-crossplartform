using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SlimDX.Direct3D9;

internal sealed class XTextTokens
{
    private readonly List<string> tokens = new();
    private int position;
    internal bool End => position == tokens.Count;
    internal string Peek => End ? "" : tokens[position];

    internal XTextTokens(string text)
    {
        if (text.Length < 16 || !text.StartsWith("xof ", StringComparison.Ordinal))
            throw new InvalidDataException("Missing DirectX .x header.");
        if (text.Substring(8, 4) != "txt ")
            throw new NotSupportedException("Only text DirectX .x files are supported (not binary/compressed).");
        if (text.Substring(4, 4) != "0303" && text.Substring(4, 4) != "0302")
            throw new NotSupportedException("Unsupported DirectX .x version.");
        if (text.Substring(12, 4) != "0032" && text.Substring(12, 4) != "0064")
            throw new NotSupportedException("Unsupported DirectX .x float size.");
        Tokenize(text, 16);
    }

    private void Tokenize(string text, int start)
    {
        for (int i = start; i < text.Length;)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c) || c == ';' || c == ',') { i++; continue; }
            if (c == '#' || (c == '/' && i + 1 < text.Length && text[i + 1] == '/'))
            {
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                int end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end < 0) throw new InvalidDataException("Unterminated .x comment.");
                i = end + 2;
                continue;
            }
            if (c == '"') { tokens.Add(ReadString(text, ref i)); continue; }
            if (c == '{' || c == '}') { tokens.Add(c.ToString()); i++; continue; }
            int begin = i++;
            while (i < text.Length && !char.IsWhiteSpace(text[i]) && ";,{}\"".IndexOf(text[i]) < 0) i++;
            tokens.Add(text.Substring(begin, i - begin));
        }
    }

    private static string ReadString(string text, ref int i)
    {
        var value = new StringBuilder("\"");
        for (i++; i < text.Length; i++)
        {
            if (text[i] == '"') { i++; return value.ToString(); }
            if (text[i] == '\\' && i + 1 < text.Length && (text[i + 1] == '\\' || text[i + 1] == '"')) i++;
            value.Append(text[i]);
        }
        throw new InvalidDataException("Unterminated .x string.");
    }

    internal string Take()
    {
        if (End) throw new InvalidDataException("Unexpected end of .x file.");
        return tokens[position++];
    }

    internal void Expect(string token)
    {
        if (Take() != token) throw new InvalidDataException($"Expected '{token}' in .x file.");
    }

    internal string OpenObject()
    {
        string name = Peek == "{" ? "" : Take();
        Expect("{");
        return name;
    }

    internal int Integer()
    {
        if (!int.TryParse(Take(), NumberStyles.None, CultureInfo.InvariantCulture, out int value))
            throw new InvalidDataException("Expected a nonnegative integer in .x file.");
        if (value > tokens.Count) throw new InvalidDataException("Declared .x count exceeds file size.");
        return value;
    }

    internal float Number()
    {
        if (!float.TryParse(Take(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) || !float.IsFinite(value))
            throw new InvalidDataException("Expected a finite number in .x file.");
        return value;
    }

    internal string String()
    {
        string value = Take();
        if (!value.StartsWith('"')) throw new InvalidDataException("Expected quoted .x string.");
        return value.Substring(1);
    }

    internal void SkipTemplate()
    {
        OpenObject();
        int depth = 1;
        while (depth > 0)
        {
            string token = Take();
            if (token == "{") depth++;
            if (token == "}") depth--;
        }
    }
}
