using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

internal class EmitterGiveOutEnegy3D : BaseEffect
{
	public EmitterGiveOutEnegy3D(StageDataPackage StageData, PointF OriginalPosition, Color color)
		: base(StageData)
	{
		base.OriginalPosition = OriginalPosition;
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.LifeTime = 25;
		base.ColorValue = color;
		BaseEffect baseEffect = new BaseEffect(StageData, "Effect-2", base.Position, 0f, 0.0)
		{
			LifeTime = 100,
			Scale = 1f,
			ScaleVelocity = 0.8f,
			ColorValue = color,
			TransparentValueF = 180f,
			TransparentVelocity = -2f
		};
	}

	public override void Shoot()
	{
		if (base.Time == 1)
		{
			for (int i = 0; i < 50; i++)
			{
				Particle3D1 particle3D = new Particle3D1(base.StageData, "Effect-1", base.OriginalPosition, (float)base.Ran.Next(50, 130) / 10f, (float)base.Ran.Next(360) / 180f * (float)Math.PI, 30 + base.StageData.Ran.Next(50));
				particle3D.Direction3D = (float)base.StageData.Ran.Next(0, 60) * (float)Math.PI / 180f;
				particle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				particle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(10, 18) / 100f;
				particle3D.Scale = (float)base.StageData.Ran.Next(20, 30) / 10f;
				particle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				particle3D.ColorValue = base.ColorValue;
			}
		}
	}

	public override void Show()
	{
	}
}
