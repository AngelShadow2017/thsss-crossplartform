using System;
using System.Drawing;

namespace Shooting;

public class BossStarEmitter01 : BaseEffect
{
	public int ColorType;

	public BossStarEmitter01(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = 130;
	}

	public override void Shoot()
	{
		if (base.Time % 1 == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				float num = base.Ran.Next(0, 15);
				double num2 = (double)base.Ran.Next(360) * Math.PI / 180.0;
				PointF position = new PointF(base.Position.X + num * (float)Math.Sin(num2), base.Position.Y + num * (float)Math.Cos(num2));
				Particle particle = new Particle(base.StageData, "Star0" + ColorType, position, (float)base.StageData.Ran.Next(1, 15) / 10f, base.Direction);
				particle.Scale = (float)base.StageData.Ran.Next(15, 40) / 100f;
				particle.MaxTransparent = 255;
				particle.LifeTime = 40 + base.Ran.Next(60);
			}
		}
	}
}
