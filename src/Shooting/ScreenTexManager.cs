using SlimDX.Direct3D9;

namespace Shooting;

public class ScreenTexManager
{
	public ScreenTexture[] ScreenTex { get; private set; }

	private int ScreenLevel { get; set; }

	public Texture RenderTexture => ScreenTex[ScreenLevel].renderTexture;

	/// <summary>True while a caller inside Begin()/End() owns the frame instead of the default one.</summary>
	internal bool HasComposite => ScreenLevel > 0;

	/// <summary>
	/// Texture holding the finished frame while a level is active. Begin() renders into the
	/// level it opens and End() composites that level back, so the finished pixels sit one
	/// level below the one that was just closed.
	/// </summary>
	internal Texture? ActiveComposite => ScreenLevel > 0 ? ScreenTex[ScreenLevel - 1].renderTexture : null;

	public ScreenTexManager(Device DeviceMain)
	{
		ScreenTex = new ScreenTexture[5];
		for (int i = 0; i < ScreenTex.Length; i++)
		{
			ScreenTex[i] = new ScreenTexture(DeviceMain, 640, 480);
		}
	}

	public void Begin()
	{
		ScreenTex[ScreenLevel++].Begin();
	}

	public void End()
	{
		ScreenTex[--ScreenLevel].End();
	}

	public void Reset()
	{
		for (int i = 0; i < ScreenTex.Length; i++)
		{
			if (ScreenTex[i] != null)
			{
				ScreenTex[i].Reset();
			}
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < ScreenTex.Length; i++)
		{
			if (ScreenTex[i] != null)
			{
				ScreenTex[i].Dispose();
			}
		}
	}
}
