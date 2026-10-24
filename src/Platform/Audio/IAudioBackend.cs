using System;

namespace THSSS.Audio;

internal interface IAudioBackend : IDisposable
{
    string Name { get; }
    IAudioSource CreateSource(int sampleRate);
}

internal interface IAudioSource : IDisposable
{
    void Submit(float[] stereoSamples, int sampleRate);
    int ProcessedBuffers { get; }
    void UnqueueProcessed();
    int SampleOffset { get; }
    bool IsPlaying { get; }
    void Play();
    void Pause();
    void Clear();
    void SetGain(float gain);
}
