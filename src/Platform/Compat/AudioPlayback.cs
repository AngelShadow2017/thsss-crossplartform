using System;
using SlimDX.Multimedia;

namespace SlimDX.XAudio2
{
    internal sealed class AudioPlayback
    {
        internal readonly float[] Samples;
        internal readonly int Channels;
        internal readonly int PlayBegin;
        internal readonly int PlayEnd;
        internal readonly int LoopBegin;
        internal readonly int LoopEnd;
        internal readonly int LoopCount;
        internal readonly long Length;
        internal readonly bool EndOfStream;
        internal long Uploaded;
        internal long Consumed;
        internal bool Started;

        internal AudioPlayback(AudioBuffer buffer, WaveFormat format)
        {
            byte[] pcm = buffer.Pcm ?? throw new ArgumentException("AudioData must be a WaveStream.", nameof(buffer));
            int bytes = buffer.AudioBytes == 0 ? pcm.Length : buffer.AudioBytes;
            Channels = format.Channels;
            EndOfStream = (buffer.Flags & BufferFlags.EndOfStream) != 0;
            int sampleBytes = format.BitsPerSample / 8;
            if (bytes <= 0 || bytes > pcm.Length || bytes % (sampleBytes * Channels) != 0)
                throw new ArgumentException("AudioBytes must contain complete PCM frames.", nameof(buffer));
            int frames = bytes / (sampleBytes * Channels);
            PlayBegin = buffer.PlayBegin;
            PlayEnd = buffer.PlayLength == 0 ? frames : checked(PlayBegin + buffer.PlayLength);
            LoopBegin = buffer.LoopBegin;
            LoopEnd = buffer.LoopLength == 0 ? PlayEnd : checked(LoopBegin + buffer.LoopLength);
            LoopCount = buffer.LoopCount;
            if (PlayBegin < 0 || PlayBegin >= PlayEnd || PlayEnd > frames || buffer.PlayLength < 0 ||
                LoopCount < 0 || LoopCount > 255 || buffer.LoopLength < 0 ||
                (LoopCount != 0 && (LoopBegin < 0 || LoopBegin >= LoopEnd || LoopEnd > PlayEnd || PlayBegin >= LoopEnd)))
                throw new ArgumentOutOfRangeException(nameof(buffer), "Invalid playback or loop sample region.");
            Length = LoopCount == 255 ? long.MaxValue : PlayEnd - PlayBegin + (long)LoopCount * (LoopEnd - LoopBegin);
            Samples = new float[bytes / sampleBytes];
            for (int i = 0; i < Samples.Length; i++)
                Samples[i] = Decode(pcm, i * sampleBytes, format);
        }

        private static float Decode(byte[] pcm, int offset, WaveFormat format)
        {
            if (format.FormatTag == (short)WaveFormatTag.IeeeFloat)
            {
                float value = BitConverter.ToSingle(pcm, offset);
                if (!float.IsFinite(value)) throw new ArgumentException("PCM float samples must be finite.");
                return value;
            }
            return format.BitsPerSample switch
            {
                8 => (pcm[offset] - 128) / 128f,
                16 => BitConverter.ToInt16(pcm, offset) / 32768f,
                24 => ((pcm[offset] | pcm[offset + 1] << 8 | pcm[offset + 2] << 16) << 8 >> 8) / 8388608f,
                32 => BitConverter.ToInt32(pcm, offset) / 2147483648f,
                _ => throw new NotSupportedException("Unsupported PCM sample size.")
            };
        }

        internal int FrameAt(long position)
        {
            if (LoopCount == 0 || position < LoopEnd - PlayBegin) return PlayBegin + (int)position;
            long repeated = position - (LoopEnd - PlayBegin);
            int loopLength = LoopEnd - LoopBegin;
            if (LoopCount == 255 || repeated < (long)LoopCount * loopLength)
                return LoopBegin + (int)(repeated % loopLength);
            return LoopEnd + (int)(repeated - (long)LoopCount * loopLength);
        }

        internal float[] Render(long position, int frames, float[] matrix)
        {
            var output = new float[frames * 2];
            for (int i = 0; i < frames; i++)
            {
                int source = FrameAt(position + i) * Channels;
                for (int destination = 0; destination < 2; destination++)
                    for (int channel = 0; channel < Channels; channel++)
                        output[i * 2 + destination] += Samples[source + channel] * matrix[destination * Channels + channel];
            }
            return output;
        }
    }
}
