using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class EnvironmentSSS02 : BaseObject, IEnvironment
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

	public EnvironmentSSS02(StageDataPackage StageData)
	{
		base.StageData = StageData;
		DeviceMain = StageData.DeviceMain;
		base.Velocity = 1f;
		CameraPosition = new Vector3(0f, 0f, 70f);
		CameraTarget = new Vector3(0f, 200f, 70f);
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
		base.Velocity += base.Accelerate;
		if (base.Velocity < 0.5f)
		{
			base.Velocity = 0.5f;
		}
		while ((float)PosY3 < CameraPosition.Y + 1600f)
		{
			Background3DObject background3DObject = new Background3DObject(base.StageData, "zhu", new PointF(200f, PosY3), InsertAtFirst: true);
			background3DObject.Angle3DX = -(float)Math.PI / 2f;
			background3DObject.Height = 256f;
			Background3DObject background3DObject2 = new Background3DObject(base.StageData, "zhu", new PointF(-200f, PosY3), InsertAtFirst: true);
			background3DObject2.Angle3DX = -(float)Math.PI / 2f;
			background3DObject2.Height = 256f;
			background3DObject2.Mirrored = true;
			Background3DObject background3DObject3 = new Background3DObject(base.StageData, "zhu", new PointF(200f, PosY3 + 100), InsertAtFirst: true);
			background3DObject3.Angle3DX = -(float)Math.PI / 2f;
			background3DObject3.Height = 256f;
			background3DObject3.Mirrored = true;
			Background3DObject background3DObject4 = new Background3DObject(base.StageData, "zhu", new PointF(-200f, PosY3 + 100), InsertAtFirst: true);
			background3DObject4.Angle3DX = -(float)Math.PI / 2f;
			background3DObject4.Height = 256f;
			PosY3 += 200;
		}
		while ((float)PosY2 < CameraPosition.Y + 1500f)
		{
			Background3DObject background3DObject5 = new Background3DObject(base.StageData, "BG02d", new PointF(0f, PosY2), InsertAtFirst: true);
			background3DObject5.Angle3DX = -(float)Math.PI;
			PosY2 += 310;
		}
		while ((float)PosY < CameraPosition.Y + 1500f)
		{
			Background3DObject background3DObject6 = new Background3DObject(base.StageData, "liba", new PointF(150f, PosY), InsertAtFirst: false);
			background3DObject6.Angle = 1.5707963705062866;
			background3DObject6.Angle3DY = -(float)Math.PI / 2f;
			background3DObject6.Height = 45f;
			Background3DObject background3DObject7 = new Background3DObject(base.StageData, "liba", new PointF(-150f, PosY), InsertAtFirst: false);
			background3DObject7.Angle = -1.5707963705062866;
			background3DObject7.Angle3DY = (float)Math.PI / 2f;
			background3DObject7.Height = 45f;
			Background3DObject background3DObject8 = new Background3DObject(base.StageData, "591-1R", new PointF(150f, PosY), InsertAtFirst: false);
			background3DObject8.Scale = 1.5f;
			background3DObject8.Active = true;
			background3DObject8.Angle3DX = -(float)Math.PI / 2f + base.StageData.Background3D.Envi.CameraAngle;
			background3DObject8.TransparentValueF = 0f;
			background3DObject8.TransparentVelocity = 10f;
			background3DObject8.Height = 60f;
			Background3DObject background3DObject9 = new Background3DObject(base.StageData, "591-1R", new PointF(-150f, PosY), InsertAtFirst: false);
			background3DObject9.Scale = 1.5f;
			background3DObject9.Active = true;
			background3DObject9.Angle3DX = -(float)Math.PI / 2f + base.StageData.Background3D.Envi.CameraAngle;
			background3DObject9.TransparentValueF = 0f;
			background3DObject9.TransparentVelocity = 10f;
			background3DObject9.Height = 60f;
			PosY += 133;
		}
		for (int num = base.Background3D.BackgroundList.Count - 1; num >= 0; num--)
		{
			if (base.Background3D.BackgroundList[num].OriginalPosition.Y < CameraPosition.Y - 310f)
			{
				base.Background3D.BackgroundList.RemoveAt(num);
			}
		}
		CameraPosition.Y += base.Velocity;
		CameraTarget.Y += base.Velocity;
		CameraPosition.X = 40f * (float)Math.Sin((float)base.Time / 360f);
		CameraTarget.X = 40f * (float)Math.Sin((float)(base.Time - 60) / 360f);
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
