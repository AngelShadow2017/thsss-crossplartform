// SlimDX.Multimedia compatibility: WAV parsing for the OpenAL-backed audio engine.
using System;
using System.IO;

namespace SlimDX.Multimedia
{
    public enum WaveFormatTag : short
    {
        Pcm = 1,
        Adpcm = 2,
        IeeeFloat = 3,
        Extensible = unchecked((short)0xFFFE),
        WMAudio2 = 0x0161,
    }

    public class WaveFormat
    {
        public short FormatTag { get; set; } = (short)WaveFormatTag.Pcm;
        public short Channels { get; set; } = 1;
        public int SamplesPerSecond { get; set; } = 44100;
        public int AverageBytesPerSecond { get; set; }
        public short BlockAlign { get; set; }
        public short BitsPerSample { get; set; } = 16;
        public short Size { get; set; }

        /// <summary>SlimDX spells BlockAlign as BlockAlignment.</summary>
        public short BlockAlignment
        {
            get => BlockAlign;
            set => BlockAlign = value;
        }

        public WaveFormat() { }

        public WaveFormat(int sampleRate, int bitsPerSample, int channels)
        {
            SamplesPerSecond = sampleRate;
            BitsPerSample = (short)bitsPerSample;
            Channels = (short)channels;
            BlockAlign = (short)(channels * bitsPerSample / 8);
            AverageBytesPerSecond = sampleRate * BlockAlign;
        }
    }

    public class WaveFormatExtensible : WaveFormat
    {
        public short ValidBitsPerSample { get; set; }

        /// <summary>Speaker configuration bits (WAVEFORMATEXTENSIBLE dwChannelMask).</summary>
        public Speakers SpeakerMask
        {
            get => (Speakers)ChannelMask;
            set => ChannelMask = (int)value;
        }

        public int ChannelMask { get; set; }
        public Guid SubFormat { get; set; }
    }

    [Flags]
    public enum Speakers : uint
    {
        FrontLeft = 0x1,
        FrontRight = 0x2,
        FrontCenter = 0x4,
        LowFrequency = 0x8,
        BackLeft = 0x10,
        BackRight = 0x20,
        FrontLeftOfCenter = 0x40,
        FrontRightOfCenter = 0x80,
        BackCenter = 0x100,
        SideLeft = 0x200,
        SideRight = 0x400,
    }

    /// <summary>
    /// Reads the RIFF/WAVE payload and exposes the PCM bytes plus its format, which is
    /// what the original game used to hand a finished buffer to XAudio2.
    /// </summary>
    public class WaveStream : IDisposable
    {
        private readonly MemoryStream _data;

        public WaveStream(Stream stream) : this(ReadBytes(stream)) { }

        private static byte[] ReadBytes(Stream stream)
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        }

        private WaveStream(byte[] bytes)
        {
            if (bytes.Length < 12 || bytes[0] != 'R' || bytes[1] != 'I' || bytes[2] != 'F' || bytes[3] != 'F')
                throw new InvalidDataException("Not a RIFF file");

            int position = 12;
            while (position + 8 <= bytes.Length)
            {
                string chunkId = System.Text.Encoding.ASCII.GetString(bytes, position, 4);
                int chunkSize = BitConverter.ToInt32(bytes, position + 4);
                position += 8;
                if (chunkSize < 0 || position + chunkSize > bytes.Length) break;

                if (chunkId == "fmt ")
                {
                    Format = new WaveFormat
                    {
                        FormatTag = BitConverter.ToInt16(bytes, position),
                        Channels = BitConverter.ToInt16(bytes, position + 2),
                        SamplesPerSecond = BitConverter.ToInt32(bytes, position + 4),
                        AverageBytesPerSecond = BitConverter.ToInt32(bytes, position + 8),
                        BlockAlign = BitConverter.ToInt16(bytes, position + 12),
                        BitsPerSample = BitConverter.ToInt16(bytes, position + 14),
                    };
                }
                else if (chunkId == "data")
                {
                    Pcm = new byte[chunkSize];
                    Array.Copy(bytes, position, Pcm, 0, chunkSize);
                }
                position += chunkSize + (chunkSize & 1);
            }

            Format ??= new WaveFormat();
            Pcm ??= Array.Empty<byte>();
            _data = new MemoryStream(Pcm, writable: false);
        }

        public WaveStream(string filename) : this(File.ReadAllBytes(filename)) { }

        public WaveFormat Format { get; private set; } = new();

        /// <summary>Raw PCM samples, or empty when the file carried no data chunk.</summary>
        public byte[] Pcm { get; private set; } = Array.Empty<byte>();

        public long Length => Pcm.LongLength;

        public long Position { get => _data.Position; set => _data.Position = value; }

        public void Dispose() => _data.Dispose();

        public static implicit operator IntPtr(WaveStream stream) => IntPtr.Zero;
    }
}
