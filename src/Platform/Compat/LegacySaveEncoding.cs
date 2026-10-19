using System;
using System.IO;
using System.Text;

namespace Shooting;

internal static class LegacySaveEncoding
{
    internal static readonly Encoding Utf8 = new UTF8Encoding(false, true);
    private static readonly Encoding Chinese = CreateChineseEncoding();

    private static Encoding CreateChineseEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }

    internal static string Decode(byte[] bytes, out Encoding encoding)
    {
        encoding = Utf8;
        try { return Utf8.GetString(bytes); }
        catch (DecoderFallbackException)
        {
            encoding = Chinese;
            string text = Chinese.GetString(bytes);
            if (!bytes.AsSpan().SequenceEqual(Chinese.GetBytes(text)))
                throw new InvalidDataException("Save text cannot be decoded without changing bytes.");
            return text;
        }
    }

    internal static void WriteAtomic(string path, byte[] bytes)
    {
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(true);
            }
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
