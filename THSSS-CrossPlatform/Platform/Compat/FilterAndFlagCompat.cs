// Supplemental XAudio2 enums used by the game's audio players.
using System;

namespace SlimDX.XAudio2
{
    [Flags]
    public enum PlayFlags
    {
        None = 0,
        UseFilter = 1,
    }

    [Flags]
    public enum VoiceFlags
    {
        None = 0,
        UseFilter = 1,
        Music = 2,
    }

    /// <summary>XAudio2 built-in filter kinds.</summary>
    public enum FilterType
    {
        LowPassFilter = 0,
        BandPassFilter = 1,
        HighPassFilter = 2,
    }

    [Flags]
    public enum BufferFlags
    {
        None = 0,
        EndOfStream = 0x0040,
    }

    public struct FilterParameters
    {
        public FilterType Type;
        public float Frequency;
        public float OneOverQ;
    }

    public struct Volume
    {
        public float Value;
        public Volume(float value) { Value = value; }
        public static implicit operator Volume(float value) => new(value);
        public static implicit operator float(Volume volume) => volume.Value;
    }
}
