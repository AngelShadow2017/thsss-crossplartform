using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

internal class EmitterSaveEnegy3D : BaseEffect
{
	public EmitterSaveEnegy3D(StageDataPackage StageData, PointF OriginalPosition, Color color)
		: base(StageData)
	{
		base.OriginalPosition = OriginalPosition;
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.LifeTime = 25;
		base.ColorValue = color;
		BaseEffect baseEffect = new BaseEffect(StageData, "Effect-2", base.Position, 0f, 0.0)
		{
			LifeTime = 50,
			Scale = 14f,
			ScaleVelocity = -0.25f,
			ColorValue = color,
			TransparentValueF = 0f,
			TransparentVelocity = 6f,
			MaxTransparent = 128
		};
	}

	public override void Shoot()
	{
		if (base.Time % 2 == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				double num = (double)((float)base.Ran.Next(360) / 180f) * Math.PI;
				int num2 = base.Ran.Next(150, 200);
				PointF originalPosition = new PointF((float)((double)base.OriginalPosition.X + (double)num2 * Math.Cos(num)), (float)((double)base.OriginalPosition.Y + (double)num2 * Math.Sin(num)));
				Particle3D1 particle3D = new Particle3D1(base.StageData, "Effect-1", originalPosition, 0f, num + Math.PI, base.Ran.Next(50, 80));
				particle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				particle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(10, 18) / 100f;
				particle3D.Scale = (float)base.StageData.Ran.Next(30, 40) / 10f;
				particle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				particle3D.ColorValue = base.ColorValue;
				Particle3D1 particle3D2 = particle3D;
				particle3D2.Velocity = 2f * (float)num2 / (float)particle3D2.LifeTime;
				particle3D2.Accelerate = (0f - particle3D2.Velocity) * particle3D2.Velocity / (float)(2 * num2);
				particle3D2.LifeTime -= 15;
			}
		}
	}

	public override void Show()
	{
	}
}
