// Byte-for-byte parity between the opt-in managed DES and the platform provider.
// Only meaningful where the platform has DES (Linux/macOS/Windows), so Wine is excluded.
using System.Security.Cryptography;
using System.Text;
using Shooting;

byte[] key = Encoding.ASCII.GetBytes("SeiweLL0");
byte[] iv = Encoding.ASCII.GetBytes("0SeiweLL");
string[] samples = { "", "a", "hello world", "东方夏夜祭 THSSS", "score=1234567\nstage=All" };
int matched = 0;
foreach (string sample in samples)
{
    byte[] plain = Encoding.UTF8.GetBytes(sample);
    using DES des = DES.Create();
    des.Key = key;
    des.IV = iv;
    byte[] expected = des.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);
    byte[] managed = ManagedDes.EncryptCbcPkcs7(plain, key, iv);
    if (!expected.AsSpan().SequenceEqual(managed))
    {
        Console.WriteLine($"FAIL encrypt mismatch for '{sample}'");
        Environment.ExitCode = 1;
        return;
    }
    if (!ManagedDes.DecryptCbcPkcs7(managed, key, iv).AsSpan().SequenceEqual(plain))
    {
        Console.WriteLine($"FAIL managed roundtrip for '{sample}'");
        Environment.ExitCode = 1;
        return;
    }
    matched++;
}
byte[] longText = Encoding.UTF8.GetBytes(new string('x', 5000) + "尾部");
using (DES des = DES.Create())
{
    des.Key = key;
    des.IV = iv;
    byte[] cipher = des.CreateEncryptor().TransformFinalBlock(longText, 0, longText.Length);
    if (!ManagedDes.DecryptCbcPkcs7(cipher, key, iv).AsSpan().SequenceEqual(longText))
    {
        Console.WriteLine("FAIL platform ciphertext not readable by managed DES");
        Environment.ExitCode = 1;
        return;
    }
    if (!des.CreateDecryptor().TransformFinalBlock(ManagedDes.EncryptCbcPkcs7(longText, key, iv), 0, ManagedDes.EncryptCbcPkcs7(longText, key, iv).Length).AsSpan().SequenceEqual(longText))
    {
        Console.WriteLine("FAIL managed ciphertext not readable by platform DES");
        Environment.ExitCode = 1;
        return;
    }
}
Console.WriteLine($"PASS ManagedDesCheck: {matched} samples + long cross-encryption match the platform DES");
