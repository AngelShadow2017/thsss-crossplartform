using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

internal class EndBoss_Touhou : BaseEffect
{
	public int R1;

	public int R2;

	public int G1;

	public int G2;

	public int B1;

	public int B2;

	public EndBoss_Touhou(StageDataPackage StageData, PointF OriginalPosition, Color Color1, Color Color2)
		: this(StageData)
	{
		base.OriginalPosition = OriginalPosition;
		R2 = Math.Max(Color1.R, Color2.R);
		R1 = Math.Min(Color1.R, Color2.R);
		G2 = Math.Max(Color1.G, Color2.G);
		G1 = Math.Min(Color1.G, Color2.G);
		B2 = Math.Max(Color1.B, Color2.B);
		B1 = Math.Min(Color1.B, Color2.B);
	}

	public EndBoss_Touhou(StageDataPackage StageData)
		: base(StageData)
	{
		base.LifeTime = 250;
	}

	public override void Move()
	{
		if (base.Boss != null)
		{
			base.Position = base.Boss.Position;
		}
	}

	public override void Shoot()
	{
		if (base.Time == 1)
		{
			base.StageData.SoundPlay("se_enep01.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			for (int i = 0; i < 20; i++)
			{
				Particle3D2 particle3D = new Particle3D2(base.StageData, "Effect-1", base.OriginalPosition, (float)base.Ran.Next(10, 80) / 10f, (float)base.Ran.Next(360) / 180f * (float)Math.PI, 30 + base.StageData.Ran.Next(50));
				particle3D.Direction3D = (float)base.StageData.Ran.Next(0, 30) * (float)Math.PI / 180f;
				particle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				particle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(10, 18) / 100f;
				particle3D.Scale = (float)base.StageData.Ran.Next(30, 40) / 10f;
				particle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				particle3D.ColorValue = Color.FromArgb(base.Ran.Next(R1, R2), base.Ran.Next(G1, G2), base.Ran.Next(B1, B2));
			}
			base.StageData.VibrateStart(50);
		}
		else if (base.Time == 100)
		{
			base.StageData.SoundPlay("se_enep01.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			for (int i = 0; i < 80; i++)
			{
				Particle3D2 particle3D2 = new Particle3D2(base.StageData, "Effect-1", base.OriginalPosition, (float)base.Ran.Next(10, 140) / 10f, (float)base.Ran.Next(360) / 180f * (float)Math.PI, 30 + base.StageData.Ran.Next(50));
				particle3D2.Direction3D = (float)base.StageData.Ran.Next(0, 30) * (float)Math.PI / 180f;
				particle3D2.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				particle3D2.AngularVelocity3D = (float)base.StageData.Ran.Next(10, 18) / 100f;
				particle3D2.Scale = (float)base.StageData.Ran.Next(30, 40) / 10f;
				particle3D2.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				particle3D2.ColorValue = Color.FromArgb(base.Ran.Next(R1, R2), base.Ran.Next(G1, G2), base.Ran.Next(B1, B2));
			}
			base.StageData.VibrateStart(90);
			base.Boss = null;
		}
	}

	public override void Show()
	{
	}
}
