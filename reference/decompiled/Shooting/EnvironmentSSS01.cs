using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class EnvironmentSSS01 : BaseObject, IEnvironment
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

	public EnvironmentSSS01(StageDataPackage StageData)
	{
		base.StageData = StageData;
		DeviceMain = StageData.DeviceMain;
		base.Velocity = 2f;
		CameraPosition = new Vector3(0f, 2200f, 130f);
		CameraTarget = new Vector3(0f, 2400f, 50f);
		FogStart = 0f;
		FogEnd = 50f;
		LightColor = Color.White;
		light = default(Light);
		light.Type = LightType.Directional;
	}

	public override void Ctrl()
	{
		base.Time++;
		base.Velocity += base.Accelerate;
		if (base.Time % 2048 == 0)
		{
			base.Background3D.BackgroundList.ForEach(delegate(BaseObject x)
			{
				if (x is Background3DObject && ((Background3DObject)x).model.Name != "591-1")
				{
					x.OriginalPosition = new PointF(x.OriginalPosition.X - 1024f, x.OriginalPosition.Y);
				}
			});
		}
		if (base.Time % 30 == 0)
		{
			Background3DObject background3DObject = new Background3DObject(OriginalPosition: new PointF(base.Ran.Next(-200, 200), CameraPos.Y + (float)base.Ran.Next(100, 500)), StageData: base.StageData, modelName: "591-1", InsertAtFirst: false);
			background3DObject.Scale = (float)base.Ran.Next(40, 60) / 100f;
			background3DObject.ScaleVelocity = -0.004f;
			background3DObject.Velocity = (float)base.Ran.Next(-10, 10) / 20f;
			background3DObject.DirectionDegree = base.Ran.Next(360);
			background3DObject.VelocityZ = (float)base.Ran.Next(6, 15) / 20f;
			background3DObject.LifeTime = 200;
			background3DObject.Active = true;
			background3DObject.Angle3DX = -(float)Math.PI / 2f + base.StageData.Background3D.Envi.CameraAngle;
			background3DObject.TransparentValueF = 0f;
			background3DObject.TransparentVelocity = 10f;
		}
		if (6000 == base.TimeMain)
		{
			OnBossAttack = true;
		}
		CameraPosition.Y += base.Velocity;
		CameraTarget.Y += base.Velocity;
		if (CameraPosition.Y > 3500f)
		{
			CameraPosition.Y -= 2000f;
			CameraTarget.Y -= 2000f;
			base.Background3D.BackgroundList.ForEach(delegate(BaseObject x)
			{
				if (x is Background3DObject && ((Background3DObject)x).model.Name == "591-1")
				{
					x.OriginalPosition = new PointF(x.OriginalPosition.X, x.OriginalPosition.Y - 2000f);
				}
			});
		}
		CameraPosition.X = 40f * (float)Math.Sin((float)base.Time / 180f);
		CameraTarget.X = 40f * (float)Math.Sin((float)base.Time / 180f);
		CameraDirection = new Vector3(0.02f * (float)Math.Sin((float)base.Time / 90f), 1f, 0f);
		if (OnBossAttack)
		{
			if (CameraPosition.Z < 430f)
			{
				CameraPosition.Z += 1f;
				CameraTarget.Z += 1.05f;
			}
			base.Background3D.BackgroundList.ForEach(delegate(BaseObject x)
			{
				if (x is Background3DObject)
				{
					x.TransparentValueF -= 0.5f;
				}
			});
		}
		if (!OnBossAttack)
		{
			if (base.Time <= 980)
			{
				FogEnd += 2f;
				if (FogEnd > 400f)
				{
					FogEnd = 400f;
				}
			}
			if (base.Time > 980)
			{
				FogStart += 1f;
				if (FogStart > 130f)
				{
					FogStart = 130f;
				}
				FogEnd += 1f;
				if (FogEnd > 510f)
				{
					FogEnd = 510f;
				}
			}
		}
		else
		{
			FogStart += 1f;
			if (FogStart > 230f)
			{
				FogStart = 230f;
			}
			FogEnd += 1f;
			if (FogEnd > 1010f)
			{
				FogEnd = 1010f;
			}
		}
		light.Diffuse = LightColor;
		light.Ambient = LightColor;
		light.Direction = new Vector3(0f, 0f, -1f);
	}

	public void SetRenderPara()
	{
		DeviceMain.Clear(ClearFlags.Target, FogColor, 0f, 0);
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.SpriteMain.Draw2D(base.StageData.TextureObjectDictionary["BG01a"], 1f, 0f, new PointF(base.BoundRect.Left + 256, base.BoundRect.Top + 256), Color.White);
		base.SpriteMain.End();
		DeviceMain.SetRenderState(RenderState.ZEnable, value: false);
		DeviceMain.SetRenderState(RenderState.CullMode, value: true);
		DeviceMain.SetRenderState(RenderState.AlphaBlendEnable, value: true);
		DeviceMain.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
		DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
		DeviceMain.SetTransform(TransformState.View, Matrix.LookAtLH(CameraPosition, CameraTarget, CameraDirection));
		DeviceMain.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH((float)Math.PI / 2f, 1f, 1f, FogEnd + 200f));
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
