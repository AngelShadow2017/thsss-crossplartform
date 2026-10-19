using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class EnvironmentSSS05 : BaseObject, IEnvironment
{
	private Light light;

	private Color LightColor;

	private Color FogColor;

	private float FogStart;

	private float FogEnd;

	public Vector3 CameraPosition;

	public Vector3 CameraTarget;

	public Vector3 CameraDirection;

	public Device DeviceMain { get; set; }

	public bool OnBossAttack { get; set; }

	public Vector3 CameraPos => CameraPosition;

	public float CameraAngle => (float)Math.Atan((CameraTarget.Z - CameraPosition.Z) / (CameraTarget.Y - CameraPosition.Y));

	public EnvironmentSSS05(StageDataPackage StageData)
	{
		base.StageData = StageData;
		DeviceMain = StageData.DeviceMain;
		base.Velocity = 1f;
		CameraPosition = new Vector3(400f, -460f, 210f);
		CameraTarget = new Vector3(400f, 0f, 0f);
		CameraDirection = new Vector3(0f, 0f, 1f);
		FogStart = 0f;
		FogEnd = 0f;
		light = default(Light);
		light.Type = LightType.Directional;
		new Background3DObject(StageData, "Stair", new PointF(0f, 0f), InsertAtFirst: false);
	}

	public override void Ctrl()
	{
		base.Time++;
		base.Velocity += base.Accelerate;
		CameraPosition.Y += base.Velocity;
		CameraTarget.Y += base.Velocity;
		CameraPosition.Z += base.Velocity;
		CameraTarget.Z += base.Velocity;
		if (CameraPosition.Y > -100f)
		{
			CameraPosition.Y -= 150f;
			CameraTarget.Y -= 150f;
			CameraPosition.Z -= 150f;
			CameraTarget.Z -= 150f;
		}
		CameraPosition.X += 1f / 18f * (float)Math.Cos((float)base.Time / 180f);
		CameraTarget.X += 1f / 18f * (float)Math.Cos((float)base.Time / 180f);
		if (base.Time > 100 && base.Time < 5050)
		{
			FogEnd += 1f;
			if (FogEnd > 700f)
			{
				FogEnd = 700f;
			}
		}
		if (base.TimeMain > 400 && base.TimeMain < 700)
		{
			CameraPosition.X -= 1f;
			CameraTarget.X -= 1f;
		}
		else if (base.TimeMain >= 700 && base.TimeMain < 800)
		{
			CameraPosition.X -= 0.5f;
			CameraTarget.X -= 0.5f;
		}
		else if (base.TimeMain >= 800 && base.TimeMain < 850)
		{
			CameraPosition.X -= 0.2f;
			CameraTarget.X -= 0.2f;
		}
		if (base.TimeMain > 200 && base.TimeMain < 300)
		{
			CameraPosition.Y += 0.5f;
			CameraPosition.Z -= 0.5f;
		}
		else if (base.TimeMain > 1100 && base.TimeMain < 1400)
		{
			CameraPosition.X += 0.1f;
			CameraTarget.X += 0.13f;
			CameraPosition.Z -= 0.1f;
			if (CameraDirection.X < 0.03f)
			{
				CameraDirection.X += 0.0001f;
			}
		}
		else if (base.TimeMain > 1800 && base.TimeMain < 2100)
		{
			if (CameraPosition.X > 30f)
			{
				CameraPosition.X -= 0.35f;
			}
			if (CameraTarget.X < 200f)
			{
				CameraTarget.X += 0.25f;
			}
			CameraPosition.Z -= 0.3f;
			CameraDirection.X -= 0.0001f;
		}
		if (base.TimeMain > 8200 && base.TimeMain < 8250)
		{
			BaseParticle3D baseParticle3D = new BaseParticle3D(base.StageData, "云（亮层）", new PointF(base.BoundRect.Width / 2 + base.Ran.Next(-20, 20), 240 + base.Ran.Next(-20, 20)), 0f, 0.0);
			baseParticle3D.TransparentValueF = 0f;
			baseParticle3D.TransparentVelocity = 0.2f;
			baseParticle3D.Scale = 2f;
			baseParticle3D.ColorValue = Color.Gray;
			BaseParticle3D baseParticle3D2 = baseParticle3D;
			baseParticle3D2.TransparentVelocityDictionary.Add(300, -0.2f);
			baseParticle3D2.AngularVelocityDegree = (float)base.Ran.Next(-50, 50) / 20f;
			baseParticle3D2.Depth = (float)(base.TimeMain - 8200) / 10f;
			baseParticle3D2.LifeTime = 1500;
			base.Background3D.BackgroundParticleList.Add(baseParticle3D2);
		}
		if (base.TimeMain == 8601)
		{
			base.Background3D.BackgroundList.Clear();
			Background3DObject background3DObject = new Background3DObject(base.StageData, "祭坛", new PointF(0f, 0f), InsertAtFirst: false);
			background3DObject.Scale = 10f;
			base.Velocity = 0f;
			CameraPosition = new Vector3(0f, -480f, 480f);
			CameraTarget = new Vector3(0f, 0f, 300f);
			CameraDirection = new Vector3(0f, 0f, 1f);
			FogEnd = 700f;
			FogStart = 200f;
		}
		if (base.TimeMain > 8600)
		{
			CameraPosition.X += 0.1f * (float)Math.Cos((float)base.Time / 180f);
			CameraTarget.X += 1f / 12f * (float)Math.Cos((float)base.Time / 180f);
			CameraPosition.Y -= 1f / 17f * (float)Math.Cos((float)base.Time / 180f);
			CameraTarget.Y -= 1f / 13f * (float)Math.Cos((float)base.Time / 180f);
		}
		LightColor = Color.White;
		light.Diffuse = LightColor;
		light.Ambient = LightColor;
		light.Direction = new Vector3(0f, 0f, -1f);
	}

	public void SetRenderPara()
	{
		DeviceMain.Clear(ClearFlags.ZBuffer | ClearFlags.Target, FogColor, 1f, 0);
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.SpriteMain.Draw2D(base.StageData.TextureObjectDictionary["夜空底色"], 1f, 0f, new PointF(base.BoundRect.Left + 205, base.BoundRect.Top + 240), Color.White);
		base.SpriteMain.Draw2D(base.StageData.TextureObjectDictionary["星星1"], 1f, 0f, new PointF(base.BoundRect.Left + 205, base.BoundRect.Top + 240), Color.FromArgb((int)(128.0 + 127.0 * Math.Sin((double)((float)base.Time / 200f * 2f) * Math.PI)), Color.White));
		base.SpriteMain.Draw2D(base.StageData.TextureObjectDictionary["星星2"], 1f, 0f, new PointF(base.BoundRect.Left + 205, base.BoundRect.Top + 240), Color.FromArgb((int)(128.0 - 127.0 * Math.Sin((double)((float)base.Time / 200f * 2f) * Math.PI)), Color.White));
		base.SpriteMain.End();
		DeviceMain.SetRenderState(RenderState.ZEnable, value: true);
		DeviceMain.SetRenderState(RenderState.ZWriteEnable, value: true);
		DeviceMain.SetRenderState(RenderState.CullMode, value: true);
		DeviceMain.SetRenderState(RenderState.AlphaBlendEnable, value: true);
		DeviceMain.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
		DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
		DeviceMain.SetTransform(TransformState.View, Matrix.LookAtLH(CameraPosition, CameraTarget, CameraDirection));
		DeviceMain.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH((float)Math.PI / 4f, 1f, 1f, FogEnd + 400f));
		DeviceMain.SetRenderState(RenderState.FogEnable, value: true);
		DeviceMain.SetRenderState(RenderState.FogTableMode, FogMode.Linear);
		DeviceMain.SetRenderState(RenderState.FogColor, FogColor.ToArgb());
		DeviceMain.SetRenderState(RenderState.FogStart, FogStart);
		DeviceMain.SetRenderState(RenderState.FogEnd, FogEnd);
		DeviceMain.SetLight(0, light);
		DeviceMain.SetRenderState(RenderState.Lighting, value: true);
		DeviceMain.EnableLight(0, enable: true);
	}
}
