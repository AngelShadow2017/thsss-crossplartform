using System;
using System.Collections.Generic;
using Silk.NET.OpenAL;

namespace THSSS.Audio;

internal sealed unsafe class OpenAlBackend : IAudioBackend
{
  internal AL AL { get; }
  private readonly ALContext _loader;
  private readonly Device* _device;
  private readonly Context* _context;
  private readonly HashSet<Source> _sources = new();
  private bool _disposed;
  public string Name => "openal";
  public OpenAlBackend()
  {
    _loader = ALContext.GetApi();
    _device = _loader.OpenDevice(null);
    if (_device == null) { _loader.Dispose(); throw new InvalidOperationException("OpenAL could not open an audio output device."); }
    _context = _loader.CreateContext(_device, null);
    if (_context == null) { _loader.CloseDevice(_device); _loader.Dispose(); throw new InvalidOperationException("OpenAL could not create its context."); }
    if (!_loader.MakeContextCurrent(_context))
    { _loader.DestroyContext(_context); _loader.CloseDevice(_device); _loader.Dispose(); throw new InvalidOperationException("OpenAL could not activate its context."); }
    try { AL = AL.GetApi(); }
    catch { _loader.MakeContextCurrent(null); _loader.DestroyContext(_context); _loader.CloseDevice(_device); _loader.Dispose(); throw; }
    if (!AL.IsExtensionPresent("AL_EXT_FLOAT32"))
    { Dispose(); throw new NotSupportedException("OpenAL AL_EXT_FLOAT32 is required for independent matrix mixing and gain."); }
  }
  public IAudioSource CreateSource(int sampleRate)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    var source = new Source(this); _sources.Add(source); return source;
  }
  public void Dispose()
  {
    if (_disposed) return;
    foreach (var source in new List<Source>(_sources)) source.Dispose();
    _disposed = true;
    _loader.MakeContextCurrent(null);
    _loader.DestroyContext(_context); _loader.CloseDevice(_device);
    AL.Dispose(); _loader.Dispose();
  }
  private sealed class Source : IAudioSource
  {
    private readonly OpenAlBackend _owner;
    internal readonly uint Id;
    private readonly Queue<uint> _buffers = new();
    private bool _disposed;
    private AL AL => _owner.AL;
    public Source(OpenAlBackend owner)
    { _owner = owner; Id = AL.GenSource(); AL.SetSourceProperty(Id, SourceFloat.MaxGain, 16777216f); Check(); }
    public int ProcessedBuffers { get { AL.GetSourceProperty(Id, GetSourceInteger.BuffersProcessed, out int n); Check(); return n; } }
    public int SampleOffset { get { if (_buffers.Count == 0) return 0; AL.GetSourceProperty(Id, GetSourceInteger.SampleOffset, out int n); Check(); return Math.Max(0, n); } }
    public bool IsPlaying { get { AL.GetSourceProperty(Id, GetSourceInteger.SourceState, out int n); Check(); return n == 0x1012; } }
    public void Submit(float[] samples, int sampleRate)
    {
      if (_buffers.Count == 0) AL.SourceRewind(Id);
      uint buffer = AL.GenBuffer();
      try { AL.BufferData(buffer, (BufferFormat)0x10011, samples, sampleRate); AL.SourceQueueBuffers(Id, new[] { buffer }); Check(); _buffers.Enqueue(buffer); }
      catch { AL.DeleteBuffer(buffer); throw; }
    }
    public void UnqueueProcessed() { AL.SourceUnqueueBuffers(Id, new[] { _buffers.Peek() }); Check(); AL.DeleteBuffer(_buffers.Dequeue()); Check(); }
    public void Play() { AL.SourcePlay(Id); Check(); }
    public void Pause() { AL.SourcePause(Id); Check(); }
    public void SetGain(float gain) { AL.SetSourceProperty(Id, SourceFloat.Gain, gain); Check(); }
    public void Clear()
    {
      AL.SourceStop(Id); AL.SetSourceProperty(Id, SourceInteger.Buffer, 0); AL.SourceRewind(Id);
      while (_buffers.Count > 0) AL.DeleteBuffer(_buffers.Dequeue()); Check();
    }
    private void Check() { var e = AL.GetError(); if (e != AudioError.NoError) throw new InvalidOperationException("OpenAL operation failed: " + e); }
    public void Dispose() { if (_disposed) return; Clear(); AL.DeleteSource(Id); _owner._sources.Remove(this); _disposed = true; }
  }
}
