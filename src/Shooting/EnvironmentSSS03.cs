using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class EnvironmentSSS03 : BaseObject, IEnvironment
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

	public EnvironmentSSS03(StageDataPackage StageData)
	{
		base.StageData = StageData;
		DeviceMain = StageData.DeviceMain;
		base.Velocity = 1f;
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
		base.Velocity += base.Accelerate;
		if (base.Velocity < 0.5f)
		{
			base.Velocity = 0.5f;
		}
		while ((float)PosY3 < CameraPosition.Y + 4200f)
		{
			Background3DObject background3DObject = new Background3DObject(base.StageData, "柳树", new PointF(300f, PosY3), InsertAtFirst: true);
			background3DObject.Angle3DX = -(float)Math.PI / 2f;
			background3DObject.Height = 400f;
			Background3DObject background3DObject2 = new Background3DObject(base.StageData, "柳树", new PointF(-300f, PosY3), InsertAtFirst: true);
			background3DObject2.Angle3DX = -(float)Math.PI / 2f;
			background3DObject2.Height = 400f;
			background3DObject2.Mirrored = true;
			Background3DObject background3DObject3 = new Background3DObject(base.StageData, "柳树", new PointF(300f, PosY3 + 200), InsertAtFirst: true);
			background3DObject3.Angle3DX = -(float)Math.PI / 2f;
			background3DObject3.Height = 400f;
			background3DObject3.Mirrored = true;
			Background3DObject background3DObject4 = new Background3DObject(base.StageData, "柳树", new PointF(-300f, PosY3 + 200), InsertAtFirst: true);
			background3DObject4.Angle3DX = -(float)Math.PI / 2f;
			background3DObject4.Height = 400f;
			PosY3 += 400;
		}
		while ((float)PosY < CameraPosition.Y + 3072f)
		{
			Background3DObject background3DObject5 = new Background3DObject(base.StageData, "地砖", new PointF(0f, PosY), InsertAtFirst: true);
			background3DObject5.Angle3DX = -(float)Math.PI;
			background3DObject5.Scale = 1f;
			PosY += 256;
		}
		while ((float)PosY2 < CameraPosition.Y + 1950f)
		{
			Background3DObject background3DObject6 = new Background3DObject(base.StageData, "草地", new PointF(0f, PosY2), InsertAtFirst: true);
			background3DObject6.Angle3DX = -(float)Math.PI;
			background3DObject6.Scale = 5f;
			PosY2 += 1625;
		}
		for (int num = base.Background3D.BackgroundList.Count - 1; num >= 0; num--)
		{
			if (base.Background3D.BackgroundList[num].OriginalPosition.Y < CameraPosition.Y - 1000f)
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
