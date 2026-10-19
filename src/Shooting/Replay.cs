using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Shooting;

public class Replay
{
    private FileStream ReplayData { get; set; }
    private string FileName { get; set; }

    public long DataPosition
    {
        get => ReplayData.Position;
        set => ReplayData.Position = value;
    }

    public bool CanRead => ReplayData.CanRead;

    public static ReplayInfo ReadTitle(string fileName)
    {
        byte[] bytes = File.ReadAllBytes(fileName);
        byte[] marker = Encoding.ASCII.GetBytes("\r\nReplayInformation\r\n");
        int end = -1;
        for (int i = 0; i + 2 + marker.Length <= bytes.Length; i += 2)
        {
            if (bytes[i] == 240 && bytes[i + 1] == 15)
            {
                end = i;
                break;
            }
        }
        if (end < 0 || !bytes.AsSpan(end + 2).StartsWith(marker))
            throw new InvalidDataException("Replay end marker or metadata header is missing.");
        string title = LegacySaveEncoding.Decode(bytes.AsSpan(end + 2 + marker.Length).ToArray(), out _);
        string[] lines = title.Replace("\r\n", "\n").Split('\n');
        if (lines.Length < 10 || lines[0] != "ver 1.00")
            throw new InvalidDataException("Replay metadata is truncated or has an unsupported version.");
        var info = new ReplayInfo
        {
            Version = lines[0], PlayerName = lines[1], Date = lines[2], Time = lines[3],
            MyPlaneName = lines[4], WeaponType = lines[5],
            Rank = (DifficultLevel)int.Parse(lines[6], CultureInfo.InvariantCulture),
            StartStage = lines[7], LastStage = lines[8], SlowRate = lines[9]
        };
        for (int i = 10; i < lines.Length; i++)
        {
            if (i == lines.Length - 1 && lines[i] == "") break;
            MyPlaneInfo plane = ParsePlane(lines[i]);
            if (plane.DataPosition < 0 || plane.DataPosition > end || plane.DataPosition % 2 != 0)
                throw new InvalidDataException("Replay stage position is outside the key stream.");
            info.MyPlaneData.Add(plane);
        }
        bool StageName(string stage) => stage == "StEx" || stage.Length == 3
            && (stage.StartsWith("St",StringComparison.Ordinal) || stage.StartsWith("Bs",StringComparison.Ordinal))
            && stage[2] is >= '1' and <= '6';
        if (!StageName(info.StartStage) || (info.LastStage != "All" && !StageName(info.LastStage)) || info.MyPlaneData.Count < 2)
            throw new InvalidDataException("Replay needs valid stage identifiers and initial/final stage records.");
        return info;
    }

    private static MyPlaneInfo ParsePlane(string line)
    {
        string[] fields = line.Split('\t');
        if (fields.Length != 15) throw new InvalidDataException("Replay stage record must have 15 fields.");
        return new MyPlaneInfo
        {
            Life = int.Parse(fields[0], CultureInfo.InvariantCulture),
            Spell = int.Parse(fields[1], CultureInfo.InvariantCulture),
            Power = int.Parse(fields[2], CultureInfo.InvariantCulture),
            Score = long.Parse(fields[3], CultureInfo.InvariantCulture),
            Graze = int.Parse(fields[4], CultureInfo.InvariantCulture),
            PosX = ParseDecimal(fields[5]), PosY = ParseDecimal(fields[6]),
            LifeChip = int.Parse(fields[7], CultureInfo.InvariantCulture),
            SpellChip = int.Parse(fields[8], CultureInfo.InvariantCulture),
            LifeUpCount = int.Parse(fields[9], CultureInfo.InvariantCulture),
            StarPoint = int.Parse(fields[10], CultureInfo.InvariantCulture),
            HighItemScore = int.Parse(fields[11], CultureInfo.InvariantCulture),
            Rate = ParseDecimal(fields[12]),
            LastColor = (EnchantmentType)int.Parse(fields[13], CultureInfo.InvariantCulture),
            DataPosition = long.Parse(fields[14], CultureInfo.InvariantCulture)
        };
    }

    private static float ParseDecimal(string value)
    {
        int comma = value.IndexOf(',');
        if (comma >= 0)
        {
            if (value.Contains('.') || value.LastIndexOf(',') != comma)
                throw new InvalidDataException("Replay number has ambiguous decimal/group separators.");
            value = value.Replace(',', '.');
        }
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number) || !float.IsFinite(number))
            throw new InvalidDataException("Replay number is invalid or non-finite.");
        return number;
    }

    public void WriteKey(int KeyValue)
    {
        ReplayData.WriteByte((byte)(KeyValue & 0xFF));
        ReplayData.WriteByte((byte)((KeyValue >> 8) & 0xFF));
    }

    public int ReadKey()
    {
        int low = ReplayData.ReadByte();
        int high = ReplayData.ReadByte();
        if (low < 0 || high < 0) throw new EndOfStreamException("Replay key stream ended before a complete key/end marker.");
        return low | (high << 8);
    }

    public void CreatRpy()
    {
        ReplayData = new FileStream("Replay/AutoSave.rpy", FileMode.Create);
    }

    public void SaveRpy(string fileName, ReplayInfo repInfo)
    {
        if (ReplayData.CanRead)
        {
            var title = new StringBuilder("\r\nReplayInformation\r\nver 1.00\r\n");
            title.Append(repInfo.PlayerName).Append("\r\n")
                .Append(DateTime.Now.ToString("yy'/'MM'/'dd", CultureInfo.InvariantCulture)).Append("\r\n")
                .Append(DateTime.Now.ToShortTimeString()).Append("\r\n")
                .Append(repInfo.MyPlaneName).Append("\r\n").Append(repInfo.WeaponType).Append("\r\n")
                .Append(((int)repInfo.Rank).ToString(CultureInfo.InvariantCulture)).Append("\r\n")
                .Append(repInfo.StartStage).Append("\r\n").Append(repInfo.LastStage).Append("\r\n").Append(repInfo.SlowRate);
            foreach (MyPlaneInfo plane in SlimDX.Compat.ListCompat.LegacyItems(repInfo.MyPlaneData))
                title.Append("\r\n").Append(FormatPlane(plane));
            byte[] metadata = LegacySaveEncoding.Utf8.GetBytes(title.ToString());
            WriteKey(4080);
            ReplayData.Write(metadata);
            ReplayData.Flush(true);
            ReplayData.Close();
        }
        if (fileName != "Replay/AutoSave.rpy")
            LegacySaveEncoding.WriteAtomic(fileName, File.ReadAllBytes("Replay/AutoSave.rpy"));
    }

    private static string FormatPlane(MyPlaneInfo plane)
    {
        return string.Join("\t", Array.ConvertAll(new IFormattable[]
        {
            plane.Life, plane.Spell, plane.Power, plane.Score, plane.Graze, plane.PosX, plane.PosY,
            plane.LifeChip, plane.SpellChip, plane.LifeUpCount, plane.StarPoint, plane.HighItemScore,
            plane.Rate, (int)plane.LastColor, plane.DataPosition
        }, value => value.ToString(null, CultureInfo.InvariantCulture)));
    }

    public void LoadRpy(string fileName)
    {
        FileStream stream = File.OpenRead(fileName);
        ReplayData?.Dispose();
        ReplayData = stream;
        FileName = fileName;
    }

    public void ReloadRpy() => LoadRpy(FileName);
    public void CloseRpy() => ReplayData?.Close();
    public void Dispose() => ReplayData?.Dispose();
}
