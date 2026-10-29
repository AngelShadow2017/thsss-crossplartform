// Byte-for-byte parity between the opt-in managed DES and the platform provider.
// Only meaningful where the platform has DES (Linux/macOS/Windows), so Wine is excluded.
//
// ManagedDes is the FIPS 46-3 DES/ CBC / PKCS7 implementation used when the platform
// provider is not allowed; this fixture proves the two produce and accept identical
// ciphertext, so a save written by either side is readable by the other.
using System.Security.Cryptography;
using System.Text;
using Shooting;

// Fixed key/IV and a mix of empty, ASCII, multibyte CJK and multi-line payloads: PKCS7 keeps
// an empty input at one full block, and the CJK samples catch any UTF-8/raw-byte mix-up.
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
    // Encrypt must be byte-identical, not merely decryptable: a divergence in padding or IV
    // handling would still round-trip through the same implementation.
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
// A long multi-block payload, longer than any buffer the game normally writes, to cover the
// block loop rather than the single-block special case.
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
    // The reverse direction: the platform provider must also read managed output, otherwise
    // switching the switch back would break existing saves.
    if (!des.CreateDecryptor().TransformFinalBlock(ManagedDes.EncryptCbcPkcs7(longText, key, iv), 0, ManagedDes.EncryptCbcPkcs7(longText, key, iv).Length).AsSpan().SequenceEqual(longText))
    {
        Console.WriteLine("FAIL managed ciphertext not readable by platform DES");
        Environment.ExitCode = 1;
        return;
    }
}
Console.WriteLine($"PASS ManagedDesCheck: {matched} samples + long cross-encryption match the platform DES");
