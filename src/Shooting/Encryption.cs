using System;
using System.Security.Cryptography;
using System.Text;

namespace Shooting;

internal class Encryption
{
    public static string Encrypt(string plaintext) => Encrypt(plaintext, LegacySaveEncoding.Utf8);

    internal static string Encrypt(string plaintext, Encoding encoding)
    {
        using DES des = CreateCipher();
        using ICryptoTransform encryptor = des.CreateEncryptor();
        byte[] bytes = encoding.GetBytes(plaintext);
        return Convert.ToHexString(encryptor.TransformFinalBlock(bytes, 0, bytes.Length));
    }

    public static string Decrypt(string ciphertext) => Decrypt(ciphertext, out _);

    internal static string Decrypt(string ciphertext, out Encoding encoding)
    {
        byte[] bytes = Convert.FromHexString(ciphertext);
        using DES des = CreateCipher();
        using ICryptoTransform decryptor = des.CreateDecryptor();
        return LegacySaveEncoding.Decode(decryptor.TransformFinalBlock(bytes, 0, bytes.Length), out encoding);
    }

    private static DES CreateCipher()
    {
        DES des = DES.Create();
        des.Key = Encoding.ASCII.GetBytes("SeiweLL0");
        des.IV = Encoding.ASCII.GetBytes("0SeiweLL");
        return des;
    }
}
