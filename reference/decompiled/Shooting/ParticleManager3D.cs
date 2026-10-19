using System;
using System.Collections.Generic;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class ParticleManager3D
{
	private Light light;

	public List<BaseParticle3D> ParticleList { get; set; }

	public VertexDeclaration vertexDecl { get; set; }

	public StageDataPackage StageData { get; private set; }

	public Device DeviceMain => StageData.DeviceMain;

	public ParticleManager3D(StageDataPackage StageData)
	{
		ParticleList = new List<BaseParticle3D>();
		this.StageData = StageData;
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
		ParticleList.ForEach(delegate(BaseParticle3D x)
		{
			if (x.OutBoundary())
			{
				x.Dispose();
				ParticleList.Remove(x);
			}
		});
		ParticleList.ForEach(delegate(BaseParticle3D x)
		{
			x.Ctrl();
		});
		ParticleList.Sort(new CompareDepth());
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
		DeviceMain.Viewport = new Viewport
		{
			X = StageData.BoundRect.Left - 10,
			Y = StageData.BoundRect.Top - 10,
			Width = StageData.BoundRect.Width + 20,
			Height = StageData.BoundRect.Height + 20
		};
		SetParticleRenderPara();
		string ModelName = null;
		ParticleList.ForEach(delegate(BaseParticle3D x)
		{
			if (x.model != null)
			{
				if (ModelName != x.model.Name)
				{
					x.model.SetModel();
				}
				ModelName = x.model.Name;
			}
			else
			{
				ModelName = null;
			}
			x.Show();
		});
		StageData.ScreenTexMan.End();
		DeviceMain.Viewport = viewport;
		StageData.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
		DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
		StageData.SpriteMain.Draw2D(StageData.ScreenTexMan.RenderTexture, new Point(0, 0), 0f, new PointF(0f, 0f), Color.White);
		StageData.SpriteMain.End();
	}

	public void Dispose()
	{
		ParticleList.ForEach(delegate(BaseParticle3D x)
		{
			x.Dispose();
		});
		if (vertexDecl != null)
		{
			vertexDecl.Dispose();
		}
	}

	public void Clear()
	{
		ParticleList.Clear();
	}
}
