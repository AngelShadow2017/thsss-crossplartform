using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Shooting;

// Save/replay compatibility contract. Two independent formats are covered:
//   Score.dat  - DES-encrypted, fixed-record text save (player name, histories, 64-bit play time)
//   Replay/*.rpy - key-prefixed replay stream with a locale-formatted metadata block
// Both are read and written with relative paths from the process CWD, exactly like the game, so
// the whole run happens in a private temp directory and the original CWD/culture are restored in
// finally. Culture is swapped deliberately: the replay format must be identical under en-US,
// de-DE and fr-FR.
string originalDirectory = Directory.GetCurrentDirectory();
CultureInfo originalCulture = CultureInfo.CurrentCulture;
string temporary = Path.Combine(Path.GetTempPath(), "thsss-save-compat-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
Directory.SetCurrentDirectory(temporary);
int assertions = 0;
try
{
    // CP936 is the legacy code page the original Windows builds wrote on Chinese/Japanese
    // machines; it is not available on .NET unless the provider is registered. Both decoders use
    // the *throwing* fallbacks on purpose: a silent '?' substitution would corrupt names and this
    // suite would pass while the game loses data.
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    Encoding utf8 = new UTF8Encoding(false, true);
    Encoding cp936 = Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);

    // Seed with real multibyte names (a name, and a history name in a different script) and a
    // play time above int.MaxValue so a 32-bit truncation of the 64-bit field is caught rather
    // than rounding to a plausible-looking value.
    var seed = new PlayerData { PlayerName = "博丽灵梦" };
    seed.S_History[0].PlayerName = "雾隐之蝶";
    seed.C_History[0].PlayingTime = (long)int.MaxValue + 42;
    seed.SaveData();

    // The game's own writer is the oracle: decrypt its output to obtain the canonical plaintext,
    // then re-encrypt fixtures in each legacy encoding. This keeps the fixtures honest even if the
    // writer's field order changes, and proves the loader accepts both encodings.
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

        // Saving an unmodified record must reproduce the fixture byte-for-byte (same encoding,
        // same layout, same padding). Re-serialising with different bytes breaks compatibility
        // with copies of the game that still read the old file.
        loaded.SaveData();
        Check(File.ReadAllBytes("Score.dat").SequenceEqual(fixture), "same-encoding byte roundtrip");
        Check(File.ReadAllBytes("Score.dat.bak").SequenceEqual(fixture), "original backup preserved");

        // After a real edit the new file is readable and the .bak holds the *previous* bytes, so a
        // user can always roll back one save.
        loaded.PlayerName = "Changed";
        loaded.SaveData();
        Check(new PlayerData().PlayerName == "Changed", "atomic replacement readable");
        Check(File.ReadAllBytes("Score.dat.bak").SequenceEqual(fixture), "backup contains previous bytes");

        // CP936 cannot represent an emoji. The write must throw EncoderFallbackException AND leave
        // the existing save untouched — no replacement characters, no half-written file. (UTF-8
        // can represent it, so this is asserted only for 936.)
        if (encoding.CodePage == 936)
        {
            byte[] before = File.ReadAllBytes("Score.dat");
            loaded.PlayerName = "🦋";
            Throws<EncoderFallbackException>(() => loaded.SaveData(), "unrepresentable CP936 edit fails without replacement");
            Check(File.ReadAllBytes("Score.dat").SequenceEqual(before), "encoding failure preserves existing save");
        }
    }

    // Partial-load rollback: the record parse fails late (last field of the last record is not an
    // integer). All previously loaded live state must be discarded — including list identity via
    // ReferenceEquals, because the game holds those same list instances.
    var stable = new PlayerData();
    string previousName = stable.PlayerName;
    var previousScores = stable.S_History;
    var previousSpells = stable.SC_History;
    string broken = plaintext[..(plaintext.LastIndexOf('\t') + 1)] + "not-an-integer\r\n";
    byte[] corruptRecord = Encoding.ASCII.GetBytes(Encrypt(utf8.GetBytes(broken)));
    File.WriteAllBytes("Score.dat", corruptRecord);
    Throws<FormatException>(() => stable.LoadData(), "late record parse failure");
    Check(stable.PlayerName == previousName && ReferenceEquals(previousScores, stable.S_History) && ReferenceEquals(previousSpells, stable.SC_History), "load rollback retains all live state");

    // A failed explicit load must arm the save block: otherwise the very next menu save would
    // overwrite the user's (possibly recoverable) file with defaults.
    stable.SaveData();
    Check(File.ReadAllBytes("Score.dat").SequenceEqual(corruptRecord), "failed explicit load blocks automatic save");

    // Corruption classes the loader must reject without touching the file: non-hex text, an odd
    // hex length, a valid ciphertext of the wrong byte length, truncated plaintext, and extra
    // trailing records. The menu-style save after a failed construction must also be a no-op, and
    // the object must fall back to the original defaults (empty name, "-------" placeholder rows).
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

    // A successful reload must clear the save block again, otherwise the game becomes read-only
    // after the first transient corruption.
    File.WriteAllText("Score.dat", Encrypt(utf8.GetBytes(plaintext)));
    stable.LoadData();
    Check(stable.LoadError == null, "successful reload clears save block");
    stable.SaveData();

    // Write-failure path: making Score.dat.bak a directory forces the backup step of the atomic
    // replacement (write .tmp -> backup old -> move new) to fail. The failure must be an explicit
    // IOException, the destination must still hold its previous bytes, and no .tmp file may be
    // left behind.
    byte[] beforeWriteFailure = File.ReadAllBytes("Score.dat");
    File.Delete("Score.dat.bak");
    Directory.CreateDirectory("Score.dat.bak");
    stable.PlayerName = "Not committed";
    Throws<IOException>(() => stable.SaveData(), "atomic replacement backup failure is explicit");
    Check(File.ReadAllBytes("Score.dat").SequenceEqual(beforeWriteFailure), "failed replacement retains destination bytes");
    Directory.Delete("Score.dat.bak");
    Check(!Directory.GetFiles(temporary, "*.tmp").Any(), "no atomic-write temp files left");

    // Replay format. The first 6 bytes are three little-endian 16-bit words of the raw key stream
    // (0x0123, 0x0ABC) terminated by the 4080 (0x0FF0) sentinel; they must stay unchanged because
    // existing .rpy files and any external tooling depend on that prefix.
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

        // Metadata decimals must be written with '.' regardless of the writer's culture: on de-DE a
        // culture-formatted writer would emit "192,5" and every other client would misparse it.
        Check(utf8.GetString(replayBytes.AsSpan(6)).Contains("192.5\t384.25"), "writer uses invariant decimals under " + culture);

        // ...and the reader must be equally culture-independent: a replay written under one locale
        // has to load with correct name, positions and rate under the other two.
        foreach (string readerCulture in new[] { "en-US", "de-DE", "fr-FR" })
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(readerCulture);
            ReplayInfo info = Replay.ReadTitle("Replay/test.rpy");
            Check(info.PlayerName == "博丽灵梦" && info.MyPlaneData[0].PosX == 192.5f && info.MyPlaneData[0].PosY == 384.25f && info.MyPlaneData[0].Rate == 1.5f, "cross-culture name/position/rate");
        }

        // Key stream round-trip: two written keys followed by the 4080 sentinel, read back exactly.
        // A -257 style "EOF sentinel" leaking into gameplay state is the failure mode guarded here.
        var reader = new Replay();
        reader.LoadRpy("Replay/test.rpy");
        Check(reader.ReadKey() == 0x123 && reader.ReadKey() == 0xABC && reader.ReadKey() == 4080, "valid stream reads exactly 4080");
        reader.Dispose();
        writer.Dispose();
    }

    // A replay that legitimately reached the ending records LastStage=All; that value must not be
    // rejected as "unknown stage" by over-strict validation.
    byte[] canonical = File.ReadAllBytes("Replay/test.rpy");
    string metadata = utf8.GetString(canonical.AsSpan(6));
    File.WriteAllBytes("Replay/completed.rpy",canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("St1\r\n0.0%","All\r\n0.0%"))).ToArray());
    Check(Replay.ReadTitle("Replay/completed.rpy").LastStage=="All","legitimate ending state LastStage=All remains compatible");

    // Legacy files exist that were written with a comma decimal separator (and in CP936 or UTF-8).
    // They must still load, otherwise old players' replays become unreadable after the port.
    foreach (Encoding encoding in new[] { cp936, utf8 })
    {
        File.WriteAllBytes("Replay/legacy.rpy", canonical.Take(6).Concat(encoding.GetBytes(metadata.Replace("192.5", "192,5").Replace("384.25", "384,25").Replace("1.5", "1,5"))).ToArray());
        Check(Replay.ReadTitle("Replay/legacy.rpy").MyPlaneData[0].PosX == 192.5f, "legacy comma decimal and name encoding");
    }

    // The original writer never emitted thousands separators, so a single comma is unambiguously a
    // decimal point: "195,195" and "1,234" must be accepted and compare equal to the '.' reading.
    // (Note the second case is why the parser cannot guess "1,234" means 1234.)
    foreach(string value in new[]{"195,195","1,234"})
    {
        File.WriteAllBytes("Replay/legacy.rpy",canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("192.5",value))).ToArray());
        float expected=float.Parse(value.Replace(',','.'),CultureInfo.InvariantCulture);
        Check(Replay.ReadTitle("Replay/legacy.rpy").MyPlaneData[0].PosX==expected,"legacy writer never groups numbers; accept three-digit comma decimal "+value);
    }

    // Ambiguous or non-finite numbers are rejected rather than coerced: grouped digits, multiple
    // separators, NaN and Infinity would otherwise become coordinates (or poison physics math)
    // silently. Silently picking an interpretation is worse than refusing the file.
    foreach (string value in new[] { "1.234,5", "1,2,3", "NaN", "Infinity" })
    {
        File.WriteAllBytes("Replay/bad.rpy", canonical.Take(6).Concat(utf8.GetBytes(metadata.Replace("192.5", value))).ToArray());
        Throws<InvalidDataException>(() => Replay.ReadTitle("Replay/bad.rpy"), "reject ambiguous/nonfinite number " + value);
    }

    // Header-complete but record-truncated replays (valid start-stage token, 0 or 1 of the two
    // plane records instead of 12+2) must be rejected: an empty MyPlaneData array would crash the
    // load path later, far from the cause.
    foreach(string start in new[]{"StEx","St1","Bs1"})
    foreach(int records in new[]{0,1})
    {
        var lines=metadata.Replace("St1",start).Split("\r\n");
        string incomplete=string.Join("\r\n",lines.Take(12+records));
        File.WriteAllBytes("Replay/bad.rpy",canonical.Take(6).Concat(utf8.GetBytes(incomplete)).ToArray());
        Throws<InvalidDataException>(()=>Replay.ReadTitle("Replay/bad.rpy"),$"header-complete replay missing initial/final records {start}/{records}");
    }

    // Truncated key streams: a partially written key yields only the complete key (never a
    // half-word), and reading past the end throws EndOfStreamException — the original code's
    // -257 magic return must not survive into the port. Truncated metadata is rejected too.
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

    // Metadata-level corruption: cut mid-body, cut before the terminator, an unknown format
    // version ("ver 9.00" instead of "ver 1.00"), and a non-text byte. Unknown versions must fail
    // rather than half-parse, because field meaning may have changed.
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
    // Restore the process CWD and culture even on failure: a failed assertion must not leave the
    // runner in a deleted directory or a German locale for the next suite.
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

// Replay metadata fixture. Two plane records with DataPosition 0 and 2 mirror what the recorder
// writes (one live record plus the final snapshot), and the decimal values are the ones asserted
// for locale independence above.
ReplayInfo Info() => new ReplayInfo
{
    PlayerName = "博丽灵梦", MyPlaneName = "Reimu", WeaponType = "A", Rank = (DifficultLevel)0,
    StartStage = "St1", LastStage = "St1", SlowRate = "0.0%",
    MyPlaneData = new List<MyPlaneInfo> {
        new MyPlaneInfo { Life = 3, Spell = 2, Power = 100, Score = 123456, PosX = 192.5f, PosY = 384.25f, Rate = 1.5f, DataPosition = 0 },
        new MyPlaneInfo { Life = 2, Spell = 2, Power = 100, Score = 123456, PosX = 192.5f, PosY = 384.25f, Rate = 1.5f, DataPosition = 2 }
    }
};

// The save format is DES with the original game's hardcoded key/IV material. BCrypt-based DES is
// required on modern .NET; where the platform cannot provide it the managed fallback is used. The
// key material itself must not be "modernised": it is the compatibility contract with old saves.
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
