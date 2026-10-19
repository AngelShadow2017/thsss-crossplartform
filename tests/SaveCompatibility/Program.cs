using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Shooting;

string originalDirectory = Directory.GetCurrentDirectory();
CultureInfo originalCulture = CultureInfo.CurrentCulture;
string temporary = Path.Combine(Path.GetTempPath(), "thsss-save-compat-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
Directory.SetCurrentDirectory(temporary);
int assertions = 0;
try
{
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    Encoding utf8 = new UTF8Encoding(false, true);
    Encoding cp936 = Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    var seed = new PlayerData { PlayerName = "博丽灵梦" };
    seed.S_History[0].PlayerName = "雾隐之蝶";
    seed.C_History[0].PlayingTime = (long)int.MaxValue + 42;
    seed.SaveData();
    string plaintext = utf8.GetString(Decrypt(File.ReadAllText("Score.dat")));
    foreach (Encoding encoding in new[] { cp936, utf8 })
    {
        byte[] fixture = Encoding.ASCII.GetBytes(Encrypt(encoding.GetBytes(plaintext)));
        File.WriteAllBytes("Score.dat", fixture);
        var loaded = new PlayerData();
        Check(loaded.LoadError == null, encoding.WebName + " loaded");
        Check(loaded.PlayerName == "博丽灵梦", encoding.WebName + " player name");
        Check(loaded.S_History[0].PlayerName == "雾隐之蝶", encoding.WebName + " score name");
        Check(loaded.C_History[0].PlayingTime == (long)int.MaxValue + 42, "64-bit playing time");
        loaded.SaveData();
        Check(File.ReadAllBytes("Score.dat").SequenceEqual(fixture), "same-encoding byte roundtrip");
        Check(File.ReadAllBytes("Score.dat.bak").SequenceEqual(fixture), "original backup preserved");
        loaded.PlayerName = "Changed";
        loaded.SaveData();
        Check(new PlayerData().PlayerName == "Changed", "atomic replacement readable");
        Check(File.ReadAllBytes("Score.dat.bak").SequenceEqual(fixture), "backup contains previous bytes");
        if (encoding.CodePage == 936)
        {
            byte[] before = File.ReadAllBytes("Score.dat");
            loaded.PlayerName = "🦋";
            Throws<EncoderFallbackException>(() => loaded.SaveData(), "unrepresentable CP936 edit fails without replacement");
            Check(File.ReadAllBytes("Score.dat").SequenceEqual(before), "encoding failure preserves existing save");
        }
    }

    var stable = new PlayerData();
    string previousName = stable.PlayerName;
    var previousScores = stable.S_History;
    var previousSpells = stable.SC_History;
    string broken = plaintext[..(plaintext.LastIndexOf('\t') + 1)] + "not-an-integer\r\n";
    byte[] corruptRecord = Encoding.ASCII.GetBytes(Encrypt(utf8.GetBytes(broken)));
    File.WriteAllBytes("Score.dat", corruptRecord);
    Throws<FormatException>(() => stable.LoadData(), "late record parse failure");
    Check(stable.PlayerName == previousName && ReferenceEquals(previousScores, stable.S_History) && ReferenceEquals(previousSpells, stable.SC_History), "load rollback retains all live state");
    stable.SaveData();
    Check(File.ReadAllBytes("Score.dat").SequenceEqual(corruptRecord), "failed explicit load blocks automatic save");
    foreach (byte[] corrupt in new[]
    {
        Encoding.ASCII.GetBytes("bad hex!"), Encoding.ASCII.GetBytes("A"),
        Encoding.ASCII.GetBytes(Encrypt(new byte[] { 0x81 })),
        Encoding.ASCII.GetBytes(Encrypt(utf8.GetBytes(plaintext[..^8]))),
        Encoding.ASCII.GetBytes(Encrypt(utf8.GetBytes(plaintext + "extra\r\n")))
    })
    {
        File.WriteAllBytes("Score.dat", corrupt);
        var failed = new PlayerData();
        Check(failed.LoadError != null, "corrupt constructor reports error");
        failed.SaveData();
        Check(File.ReadAllBytes("Score.dat").SequenceEqual(corrupt), "menu-style save cannot overwrite corrupt bytes");
        Check(failed.PlayerName == "" && failed.S_History[0].PlayerName == "-------", "failed constructor retains defaults");
    }
    File.WriteAllText("Score.dat", Encrypt(utf8.GetBytes(plaintext)));
    stable.LoadData();
    Check(stable.LoadError == null, "successful reload clears save block");
    stable.SaveData();
    byte[] beforeWriteFailure = File.ReadAllBytes("Score.dat");
    File.Delete("Score.dat.bak");
    Directory.CreateDirectory("Score.dat.bak");
    stable.PlayerName = "Not committed";
    Throws<IOException>(() => stable.SaveData(), "atomic replacement backup failure is explicit");
    Check(File.ReadAllBytes("Score.dat").SequenceEqual(beforeWriteFailure), "failed replacement retains destination bytes");
    Directory.Delete("Score.dat.bak");
    Check(!Directory.GetFiles(temporary, "*.tmp").Any(), "no atomic-write temp files left");

    Directory.CreateDirectory("Replay");
    foreach (string culture in new[] { "en-US", "de-DE", "fr-FR" })
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        var writer = new Replay();
        writer.CreatRpy();
        writer.WriteKey(0x123);
        writer.WriteKey(0xABC);
        writer.SaveRpy("Replay/test.rpy", Info());
        byte[] replayBytes = File.ReadAllBytes("Replay/test.rpy");
        Check(replayBytes.Take(6).SequenceEqual(new byte[] { 0x23, 1, 0xBC, 0xA, 0xF0, 0xF }), "key bytes and 4080 sentinel unchanged");
        Check(utf8.GetString(replayBytes.AsSpan(6)).Contains("192.5\t384.25"), "writer uses invariant decimals under " + culture);
        foreach (string readerCulture in new[] { "en-US", "de-DE", "fr-FR" })
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(readerCulture);
            ReplayInfo info = Replay.ReadTitle("Replay/test.rpy");
            Check(info.PlayerName == "博丽灵梦" && info.MyPlaneData[0].PosX == 192.5f && info.MyPlaneData[0].PosY == 384.25f && info.MyPlaneData[0].Rate == 1.5f, "cross-culture name/position/rate");
        }
        var reader = new Replay();
        reader.LoadRpy("Replay/test.rpy");
        Check(reader.ReadKey() == 0x123 && reader.ReadKey() == 0xABC && reader.ReadKey() == 4080, "valid stream reads exactly 4080");
        reader.Dispose();
        writer.Dispose();
    }

    byte[] canonical = File.ReadAllBytes("Replay/test.rpy");
    string metadata = utf8.GetString(canonical.AsSpan(6));
    File.WriteAllBytes("Replay/completed.rpy",canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("St1\r\n0.0%","All\r\n0.0%"))).ToArray());
    Check(Replay.ReadTitle("Replay/completed.rpy").LastStage=="All","legitimate ending state LastStage=All remains compatible");
    foreach (Encoding encoding in new[] { cp936, utf8 })
    {
        File.WriteAllBytes("Replay/legacy.rpy", canonical.Take(6).Concat(encoding.GetBytes(metadata.Replace("192.5", "192,5").Replace("384.25", "384,25").Replace("1.5", "1,5"))).ToArray());
        Check(Replay.ReadTitle("Replay/legacy.rpy").MyPlaneData[0].PosX == 192.5f, "legacy comma decimal and name encoding");
    }
    foreach(string value in new[]{"195,195","1,234"})
    {
        File.WriteAllBytes("Replay/legacy.rpy",canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("192.5",value))).ToArray());
        float expected=float.Parse(value.Replace(',','.'),CultureInfo.InvariantCulture);
        Check(Replay.ReadTitle("Replay/legacy.rpy").MyPlaneData[0].PosX==expected,"legacy writer never groups numbers; accept three-digit comma decimal "+value);
    }
    foreach (string value in new[] { "1.234,5", "1,2,3", "NaN", "Infinity" })
    {
        File.WriteAllBytes("Replay/bad.rpy", canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("192.5", value))).ToArray());
        Throws<InvalidDataException>(() => Replay.ReadTitle("Replay/bad.rpy"), "reject ambiguous/nonfinite number " + value);
    }
    foreach(string start in new[]{"StEx","St1","Bs1"})
    foreach(int records in new[]{0,1})
    {
        var lines=metadata.Replace("St1",start).Split("\r\n");
        string incomplete=string.Join("\r\n",lines.Take(12+records));
        File.WriteAllBytes("Replay/bad.rpy",canonical.Take(6).Concat(utf8.GetBytes(incomplete)).ToArray());
        Throws<InvalidDataException>(()=>Replay.ReadTitle("Replay/bad.rpy"),$"header-complete replay missing initial/final records {start}/{records}");
    }
    foreach (byte[] bytes in new[] { Array.Empty<byte>(), new byte[] { 1 }, new byte[] { 1, 0, 0xF0 } })
    {
        File.WriteAllBytes("Replay/truncated.rpy", bytes);
        var reader = new Replay();
        reader.LoadRpy("Replay/truncated.rpy");
        if (bytes.Length > 1) Check(reader.ReadKey() == 1, "complete key before truncation");
        Throws<EndOfStreamException>(() => reader.ReadKey(), "explicit EOF, never -257");
        reader.Dispose();
        Throws<InvalidDataException>(() => Replay.ReadTitle("Replay/truncated.rpy"), "truncated metadata rejected");
    }
    foreach (byte[] bytes in new[]
    {
        canonical[..20], canonical[..^3], canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("ver 1.00", "ver 9.00"))).ToArray(),
        canonical.Take(6).Concat(new byte[] { 0x81 }).ToArray()
    })
    {
        File.WriteAllBytes("Replay/bad.rpy", bytes);
        Throws<InvalidDataException>(() => Replay.ReadTitle("Replay/bad.rpy"), "bad metadata rejected");
    }
    Console.WriteLine($"PASS SaveCompatibility: {assertions} assertions; isolated DES/CP936/UTF8, rollback/save block/backup, replay locale/key/EOF checks.");
}
finally
{
    CultureInfo.CurrentCulture = originalCulture;
    Directory.SetCurrentDirectory(originalDirectory);
    Directory.Delete(temporary, true);
}

void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    assertions++;
}
void Throws<T>(Action action, string name) where T : Exception
{
    try { action(); }
    catch (T) { assertions++; return; }
    throw new Exception("FAIL: " + name + " did not throw " + typeof(T).Name);
}
ReplayInfo Info() => new ReplayInfo
{
    PlayerName = "博丽灵梦", MyPlaneName = "Reimu", WeaponType = "A", Rank = (DifficultLevel)0,
    StartStage = "St1", LastStage = "St1", SlowRate = "0.0%",
    MyPlaneData = new List<MyPlaneInfo> {
        new MyPlaneInfo { Life = 3, Spell = 2, Power = 100, Score = 123456, PosX = 192.5f, PosY = 384.25f, Rate = 1.5f, DataPosition = 0 },
        new MyPlaneInfo { Life = 2, Spell = 2, Power = 100, Score = 123456, PosX = 192.5f, PosY = 384.25f, Rate = 1.5f, DataPosition = 2 }
    }
};
string Encrypt(byte[] bytes)
{
    using DES des = Cipher();
    using ICryptoTransform transform = des.CreateEncryptor();
    return Convert.ToHexString(transform.TransformFinalBlock(bytes, 0, bytes.Length));
}
byte[] Decrypt(string text)
{
    byte[] bytes = Convert.FromHexString(text);
    using DES des = Cipher();
    using ICryptoTransform transform = des.CreateDecryptor();
    return transform.TransformFinalBlock(bytes, 0, bytes.Length);
}
DES Cipher()
{
    DES des = DES.Create();
    des.Key = Encoding.ASCII.GetBytes("SeiweLL0");
    des.IV = Encoding.ASCII.GetBytes("0SeiweLL");
    return des;
}
