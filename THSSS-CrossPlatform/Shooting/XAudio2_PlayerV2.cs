using System.IO;
using SlimDX.Multimedia;
using SlimDX.XAudio2;

namespace Shooting;

public class XAudio2_PlayerV2
{
	private XAudio2 device;

	private SourceVoice sourceVoice;

	private AudioBuffer buffer;

	private bool xwmaMode;

	public bool loadSuccess { get; private set; }

	public long Position => sourceVoice.State.SamplesPlayed / 44;

	public int Volume
	{
		get
		{
			return (int)(sourceVoice.Volume * 100f);
		}
		set
		{
			if (0 <= value && value <= 100)
			{
				sourceVoice.Volume = (float)value / 100f;
			}
		}
	}

	public XAudio2_PlayerV2(string fileName, XAudio2 device)
	{
		this.device = device;
		loadSuccess = (LoadSound(fileName) ? true : false);
	}

	private bool LoadSound(string fileName)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(fileName);
			if (fileInfo.Extension == ".xwma")
			{
				xWMAStream xWMAStream2 = new xWMAStream(fileName);
				buffer = new AudioBuffer();
				buffer.AudioData = xWMAStream2;
				buffer.AudioBytes = (int)xWMAStream2.Length;
				buffer.Flags = BufferFlags.EndOfStream;
				sourceVoice = new SourceVoice(device, xWMAStream2.Format, VoiceFlags.Music | VoiceFlags.UseFilter);
				xwmaMode = true;
			}
			else if (fileInfo.Extension == ".wav")
			{
				WaveStream waveStream = new WaveStream(fileName);
				buffer = new AudioBuffer();
				buffer.AudioData = waveStream;
				buffer.AudioBytes = (int)waveStream.Length;
				buffer.Flags = BufferFlags.EndOfStream;
				sourceVoice = new SourceVoice(device, waveStream.Format, VoiceFlags.Music | VoiceFlags.UseFilter);
			}
			Volume = 100;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void Play()
	{
		if (loadSuccess)
		{
			Reset();
			sourceVoice.Start(PlayFlags.None);
		}
	}

	public void Play(int Volume)
	{
		int volume = this.Volume;
		this.Volume = Volume;
		Play();
		this.Volume = volume;
	}

	public void Resume()
	{
		if (loadSuccess)
		{
			sourceVoice.Start(PlayFlags.None);
		}
	}

	public void Stop()
	{
		if (!loadSuccess)
		{
			return;
		}
		try
		{
			sourceVoice.Stop();
		}
		catch
		{
		}
	}

	public void Reset()
	{
		if (loadSuccess)
		{
			if (xwmaMode)
			{
				sourceVoice.SubmitSourceBuffer(buffer, ((xWMAStream)buffer.AudioData).DecodedPacketCumulativeBytes);
				return;
			}
			sourceVoice.Stop();
			sourceVoice.FlushSourceBuffers();
			sourceVoice.SubmitSourceBuffer(buffer);
		}
	}

	public void SetLoop(int LoopBegin, int LoopEnd)
	{
		SetLoop(0, 0, 255, LoopBegin, LoopEnd);
	}

	public void SetLoop(int PlayBegin, int PlayLength, int LoopCount, int LoopBegin, int LoopEnd)
	{
		buffer.PlayBegin = PlayBegin;
		buffer.PlayLength = PlayLength;
		buffer.LoopCount = LoopCount;
		buffer.LoopBegin = LoopBegin;
		if (LoopEnd > LoopBegin)
		{
			buffer.LoopLength = LoopEnd - LoopBegin;
		}
	}

	public void FilterON()
	{
		FilterParameters filterParameters = new FilterParameters
		{
			Type = FilterType.HighPassFilter,
			Frequency = 0.5f,
			OneOverQ = 2f
		};
		sourceVoice.FilterParameters = filterParameters;
		sourceVoice.SetOutputMatrix(2, 2, new float[3] { 0f, 4f, 4f });
	}

	public void FilterOFF()
	{
		FilterParameters filterParameters = new FilterParameters
		{
			Type = FilterType.HighPassFilter,
			Frequency = 0f,
			OneOverQ = 2f
		};
		sourceVoice.FilterParameters = filterParameters;
		sourceVoice.SetOutputMatrix(2, 2, new float[3] { 0f, 1f, 1f });
	}

	public void Dispose()
	{
		if (loadSuccess)
		{
			Stop();
			sourceVoice.Dispose();
			(buffer.AudioData as System.IDisposable)?.Dispose();
			buffer.Dispose();
		}
	}
}
