using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class EnvironmentSSS06 : BaseObject, IEnvironment
{
	private Light light;

	private Color LightColor;

	private Color FogColor;

	private float FogStart;

	private float FogEnd;

	public Vector3 CameraPosition;

	public Vector3 CameraTarget;

	public Vector3 CameraDirection;

	private int PosY = -1000;

	private int PosY2 = -1000;

	private int PosY3 = -1000;

	public Device DeviceMain { get; set; }

	public bool OnBossAttack { get; set; }

	public Vector3 CameraPos => CameraPosition;

	public float CameraAngle => (float)Math.Atan((CameraTarget.Z - CameraPosition.Z) / (CameraTarget.Y - CameraPosition.Y));

	public EnvironmentSSS06(StageDataPackage StageData)
	{
		base.StageData = StageData;
		DeviceMain = StageData.DeviceMain;
		base.Velocity = 0.5f;
		CameraPosition = new Vector3(0f, 0f, 170f);
		CameraTarget = new Vector3(0f, 200f, 170f);
		CameraDirection = new Vector3(0f, 0f, 1f);
		FogStart = 0f;
		FogEnd = 200f;
		LightColor = Color.White;
		light = default(Light);
		light.Type = LightType.Directional;
	}

	public override void Ctrl()
	{
		base.Time++;
		if (base.Time > 50)
		{
			FogStart += 1f;
			if (FogStart > 10f)
			{
				FogStart = 10f;
			}
			FogEnd += 20f;
			if (FogEnd > 900f)
			{
				FogEnd = 900f;
			}
		}
		base.Velocity += base.Accelerate;
		if (base.Velocity < 0.5f)
		{
			base.Velocity = 0.5f;
		}
		CameraPosition.Y += base.Velocity;
		CameraTarget.Y += base.Velocity;
		CameraPosition.X = 40f * (float)Math.Sin((float)base.Time / 360f);
		CameraTarget.X = 40f * (float)Math.Sin((float)(base.Time - 60) / 360f);
		if (base.TimeMain == 3)
		{
			BaseParticle3D baseParticle3D = new BaseParticle3D(base.StageData, "BG01a", new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2), 0f, Math.PI / 2.0);
			baseParticle3D.TransparentVelocityDictionary.Add(500, -2f);
			base.Background3D.BackgroundParticleList.Add(baseParticle3D);
			Background3DObject background3DObject = new Background3DObject(base.StageData, "祭坛", new PointF(0f, 0f), InsertAtFirst: false);
			background3DObject.Scale = 10f;
			base.Velocity = 0f;
			CameraPosition = new Vector3(0f, -550f, 280f);
			CameraTarget = new Vector3(0f, 0f, 100f);
			CameraDirection = new Vector3(0f, 0f, 1f);
		}
		if (base.TimeMain > 0 && base.TimeMain < 800)
		{
			CameraTarget.Z += 1.5f;
			CameraPosition.Z += 1.5f;
		}
		else if (base.TimeMain == 800)
		{
			PosY2 -= 4000;
			base.Background3D.BackgroundList.Clear();
			while ((float)PosY2 < CameraPosition.Y + 6132f)
			{
				Background3DObject background3DObject2 = new Background3DObject(base.StageData, "云（亮层）", new PointF(0f, PosY2), InsertAtFirst: true);
				background3DObject2.Scale = 3f;
				background3DObject2.Mirrored = false;
				background3DObject2.OriginalPositionZ = CameraPosition.Z - 200f;
				background3DObject2.Angle3DX = -(float)Math.PI;
				background3DObject2.TransparentValueF = 0f;
				background3DObject2.MaxTransparent = 250;
				background3DObject2.TransparentVelocity = 2.5f;
				background3DObject2.Velocity = 0f;
				Background3DObject bo = new Background3DObject(base.StageData, "Cloud01", new PointF(0f, PosY2 + 250), InsertAtFirst: false)
				{
					Scale = 3f,
					Mirrored = true,
					OriginalPositionZ = CameraPosition.Z - 200f + 20f,
					Angle3DX = -(float)Math.PI,
					TransparentValueF = 0f,
					MaxTransparent = 50,
					TransparentVelocity = 0.5f,
					Velocity = 0.5f,
					Direction = -Math.PI / 2.0
				};
				bo.CtrlCall = delegate
				{
					if (bo.Time > 100)
					{
						bo.TransparentVelocity = -(float)Math.PI / 5f * (float)Math.Sin((double)((float)(base.Time - 100) / 300f) * Math.PI * 2.0);
					}
				};
				PosY2 += 1314;
			}
		}
		else if (base.TimeMain > 800)
		{
			if (base.TimeMain < 1000)
			{
				CameraPosition.Z += 1f;
			}
			if (base.TimeMain < 8000 && base.Time % 10 == 0)
			{
				Background3DObject background3DObject3 = new Background3DObject(OriginalPosition: new PointF(base.Ran.Next(-400, 400), CameraPos.Y + (float)base.Ran.Next(100, 500)), StageData: base.StageData, modelName: "591-1", InsertAtFirst: false);
				background3DObject3.Scale = (float)base.Ran.Next(80, 100) / 100f;
				background3DObject3.ScaleVelocity = -0.004f;
				background3DObject3.Velocity = (float)base.Ran.Next(-10, 10) / 20f;
				background3DObject3.DirectionDegree = base.Ran.Next(360);
				background3DObject3.VelocityZ = (float)base.Ran.Next(6, 15) / 20f;
				background3DObject3.LifeTime = 200;
				background3DObject3.Active = true;
				background3DObject3.Angle3DX = -(float)Math.PI / 2f + base.StageData.Background3D.Envi.CameraAngle;
				background3DObject3.TransparentValueF = 0f;
				background3DObject3.TransparentVelocity = 10f;
				background3DObject3.OriginalPositionZ = CameraPosition.Z - 200f;
			}
			if (8000 < base.TimeMain && base.TimeMain < 8200)
			{
				CameraTarget.Z += 1.5f;
			}
		}
		if (base.TimeMain == 8200)
		{
			BaseParticle3D baseParticle3D = new BaseParticle3D(base.StageData, "夜空（暗）", new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2), 0f, Math.PI / 2.0);
			baseParticle3D.TransparentValueF = 0f;
			baseParticle3D.TransparentVelocity = 3f;
			base.Background3D.BackgroundParticleList.Add(baseParticle3D);
		}
		if (base.TimeMain == 8401 && base.Background3D.BackgroundParticleList.Count == 0)
		{
			BaseParticle3D baseParticle3D = new BaseParticle3D(base.StageData, "夜空（暗）", new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2), 0f, Math.PI / 2.0);
			base.Background3D.BackgroundParticleList.Add(baseParticle3D);
		}
		for (int num = base.Background3D.BackgroundList.Count - 1; num >= 0; num--)
		{
			if (base.Background3D.BackgroundList[num].OriginalPosition.Y < CameraPosition.Y - 1000f)
			{
				base.Background3D.BackgroundList.RemoveAt(num);
			}
		}
		light.Diffuse = LightColor;
		light.Ambient = LightColor;
		light.Direction = new Vector3(0f, 0f, -1f);
	}

	public void SetRenderPara()
	{
		DeviceMain.Clear(ClearFlags.ZBuffer | ClearFlags.Target, FogColor, 1f, 0);
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.SpriteMain.End();
		DeviceMain.SetRenderState(RenderState.ZEnable, base.TimeMain < 800);
		DeviceMain.SetRenderState(RenderState.CullMode, value: true);
		DeviceMain.SetRenderState(RenderState.AlphaBlendEnable, value: true);
		DeviceMain.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
		DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
		DeviceMain.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
		DeviceMain.SetTransform(TransformState.View, Matrix.LookAtLH(CameraPosition, CameraTarget, CameraDirection));
		DeviceMain.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH((float)Math.PI / 4f, 1f, 1f, FogEnd + 200f));
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
