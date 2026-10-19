using System;
using System.Collections.Generic;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class BackgroundManager3D
{
	private Light light;

	public bool Enabled3D { get; set; }

	public bool WarpEnabled { get; set; }

	public int WarpColorKey { get; set; }

	public float WarpRadius { get; set; }

	public List<BaseObject> BackgroundList { get; set; }

	public List<BaseParticle3D> BackgroundParticleList { get; set; }

	private VertexDeclaration vertexDecl { get; set; }

	public StageDataPackage StageData { get; private set; }

	public Device DeviceMain => StageData.DeviceMain;

	public IEnvironment Envi { get; set; }

	public BackgroundManager3D(StageDataPackage StageData)
	{
		BackgroundList = new List<BaseObject>();
		BackgroundParticleList = new List<BaseParticle3D>();
		this.StageData = StageData;
		Enabled3D = true;
		vertexDecl = new VertexDeclaration(DeviceMain, TexturedVertex.VertexElements);
		SetupLight();
	}

	private void SetupLight()
	{
		light = default(Light);
		light.Type = LightType.Directional;
		light.Diffuse = Color.White;
		light.Ambient = Color.White;
		light.Direction = new Vector3(0f, 0f, -1f);
	}

	public void Ctrl()
	{
		if (Envi != null)
		{
			Envi.Ctrl();
		}
		foreach (BaseObject x in BackgroundList.ToArray())
		{
			if (x.OutBoundary())
			{
				x.Dispose();
				BackgroundList.Remove(x);
			}
		}
		foreach (BaseObject x in BackgroundList.ToArray())
		{x.Ctrl();
				}
		foreach (BaseParticle3D x in BackgroundParticleList.ToArray())
		{
			if (x.OutBoundary())
			{
				x.Dispose();
				BackgroundParticleList.Remove(x);
			}
		}
		foreach (BaseParticle3D x in BackgroundParticleList.ToArray())
		{x.Ctrl();
				}
		BackgroundParticleList.Sort(new CompareDepth());
		if (WarpEnabled)
		{
			if (WarpRadius < 128f)
			{
				WarpRadius += 4f;
			}
		}
		else if (WarpRadius > 0f)
		{
			WarpRadius -= 4f;
		}
	}

	private void SetParticleRenderPara()
	{
		DeviceMain.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0f, 0f, DeviceMain.Viewport.Height / 2), new Vector3(0f, 0f, 0f), new Vector3(0f, -1f, 0f)));
		DeviceMain.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH((float)Math.PI / 2f, (float)DeviceMain.Viewport.Width / (float)DeviceMain.Viewport.Height, 1f, 1000f));
		DeviceMain.SetRenderState(RenderState.ZEnable, value: false);
		DeviceMain.SetRenderState(RenderState.CullMode, value: true);
		DeviceMain.SetRenderState(RenderState.AlphaBlendEnable, value: true);
		DeviceMain.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
		DeviceMain.SetRenderState(RenderState.FogEnable, value: false);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
		DeviceMain.SetLight(0, light);
		DeviceMain.SetRenderState(RenderState.Lighting, value: true);
		DeviceMain.EnableLight(0, enable: true);
		DeviceMain.VertexDeclaration = vertexDecl;
		DeviceMain.VertexFormat = TexturedVertex.Format;
	}

	public void Show()
	{
		StageData.ScreenTexMan.Begin();
		Viewport viewport = StageData.DeviceMain.Viewport;
		if (!StageData.BoundRect.Equals((object?)new Rectangle(0, 0, 640, 480)))
		{
			DeviceMain.Viewport = new Viewport
			{
				X = StageData.BoundRect.Left - 10,
				Y = StageData.BoundRect.Top - 10,
				Width = StageData.BoundRect.Width + 20,
				Height = StageData.BoundRect.Height + 20,
				MinZ = 0f,
				MaxZ = 1f
			};
		}
		if (Envi != null)
		{
			Envi.SetRenderPara();
		}
		if (Enabled3D)
		{
			foreach (BaseObject x in BackgroundList.ToArray())
		{x.Show();
					}
		}
		SetParticleRenderPara();
		string ModelName = null;
		foreach (BaseParticle3D x in BackgroundParticleList.ToArray())
		{if (x.model != null)
			{
				if (ModelName != x.model.Name)
				{
					x.model.SetModel();
				}
				x.Show();
				ModelName = x.model.Name;
			}
				}
		DeviceMain.Viewport = viewport;
		StageData.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		StageData.Background.Show(Active: false);
		StageData.SpriteMain.End();
		StageData.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
		DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
		StageData.Background.Show(Active: true);
		StageData.SpriteMain.End();
		StageData.ScreenTexMan.End();
		if (StageData.Boss != null)
		{
			StageData.SpriteMain.Begin(SpriteFlags.AlphaBlend);
			DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
			DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
			StageData.EffectMain = StageData.EffectDictionary["Warp"];
			StageData.EffectMain.Technique = "technique1";
			StageData.EffectMain.Begin();
			StageData.EffectMain.SetValue("X", StageData.Boss.Position.X / 640f);
			StageData.EffectMain.SetValue("Y", StageData.Boss.Position.Y / 480f);
			StageData.EffectMain.SetValue("Radius", WarpRadius / 640f);
			StageData.EffectMain.SetValue("Time", (float)StageData.TimeMain / 20f);
			StageData.EffectMain.SetValue("ColorKey", WarpColorKey);
			StageData.EffectMain.BeginPass(0);
			StageData.SpriteMain.Draw2D(StageData.ScreenTexMan.RenderTexture, new Point(0, 0), 0f, new PointF(0f, 0f), Color.White);
			StageData.EffectMain.EndPass();
			StageData.SpriteMain.End();
			StageData.EffectMain.End();
		}
		else
		{
			StageData.SpriteMain.Begin(SpriteFlags.AlphaBlend);
			DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
			DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
			StageData.SpriteMain.Draw2D(StageData.ScreenTexMan.RenderTexture, new Point(0, 0), 0f, new PointF(0f, 0f), Color.White);
			StageData.SpriteMain.End();
		}
	}

	public void Dispose()
	{
		foreach (BaseObject x in BackgroundList.ToArray())
		{x.Dispose();
				}
		foreach (BaseParticle3D x in BackgroundParticleList.ToArray())
		{x.Dispose();
				}
		if (vertexDecl != null)
		{
			vertexDecl.Dispose();
		}
	}

	public void Clear()
	{
		BackgroundList.Clear();
		BackgroundParticleList.Clear();
	}
}
