using System;
using System.Security.Cryptography;
using System.Text;

namespace Shooting;

internal class Encryption
{
    public static string Encrypt(string plaintext) => Encrypt(plaintext, LegacySaveEncoding.Utf8);

    internal static string Encrypt(string plaintext, Encoding encoding)
    {
        byte[] bytes = encoding.GetBytes(plaintext);
        return Convert.ToHexString(Cipher().Encrypt(bytes));
    }

    public static string Decrypt(string ciphertext) => Decrypt(ciphertext, out _);

    internal static string Decrypt(string ciphertext, out Encoding encoding)
    {
        byte[] bytes = Convert.FromHexString(ciphertext);
        return LegacySaveEncoding.Decode(Cipher().Decrypt(bytes), out encoding);
    }

    private static readonly byte[] Key = Encoding.ASCII.GetBytes("SeiweLL0");
    private static readonly byte[] Iv = Encoding.ASCII.GetBytes("0SeiweLL");

    /// <summary>
    /// Platform DES, or the managed fallback when the OS has no DES provider.
    /// The fallback is opt-in at build time; see THSSS.csproj.
    /// </summary>
    private static SaveCipher Cipher()
    {
        try
        {
            using DES des = DES.Create();
            des.Key = Key;
            des.IV = Iv;
            return new PlatformCipher(des.CreateEncryptor(), des.CreateDecryptor());
        }
        catch (CryptographicException)
        {
#if THSSS_MANAGED_DES_FALLBACK
            return new ManagedCipher();
#else
            throw new PlatformNotSupportedException(
                "This platform has no DES provider (Wine and Proton lack bcrypt's DES). " +
                "Rebuild with -p:THSSSManagedDesFallback=true to include the managed fallback.");
#endif
        }
    }

    private interface SaveCipher
    {
        byte[] Encrypt(byte[] plaintext);
        byte[] Decrypt(byte[] ciphertext);
    }

    private sealed class PlatformCipher : SaveCipher
    {
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;
        public PlatformCipher(ICryptoTransform encryptor, ICryptoTransform decryptor)
        { _encryptor = encryptor; _decryptor = decryptor; }
        public byte[] Encrypt(byte[] plaintext) => _encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
        public byte[] Decrypt(byte[] ciphertext) => _decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }

#if THSSS_MANAGED_DES_FALLBACK
    private sealed class ManagedCipher : SaveCipher
    {
        public byte[] Encrypt(byte[] plaintext) => ManagedDes.EncryptCbcPkcs7(plaintext, Key, Iv);
        public byte[] Decrypt(byte[] ciphertext) => ManagedDes.DecryptCbcPkcs7(ciphertext, Key, Iv);
    }
#endif
}
