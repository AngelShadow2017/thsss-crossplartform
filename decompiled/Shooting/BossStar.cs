using System;
using System.Drawing;

namespace Shooting;

public class BossStar : BaseEffect
{
	public int ColorType;

	public BossStar(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = 130;
	}

	public override void Shoot()
	{
		if (base.Time == 1)
		{
			int num = 110;
			double num2 = Math.PI / 2.0;
			for (int i = 0; i < 3; i++)
			{
				PointF originalPosition = new PointF((float)((double)base.OriginalPosition.X + (double)num * Math.Cos(num2)), (float)((double)base.OriginalPosition.Y + (double)num * Math.Sin(num2)));
				BossStarEmitter01 bossStarEmitter = new BossStarEmitter01(base.StageData, originalPosition);
				bossStarEmitter.Velocity = 2.8f;
				bossStarEmitter.DirectionDegree = num2 * 180.0 / Math.PI + 45.0;
				bossStarEmitter.DirectionVelocityDegree = 2f;
				bossStarEmitter.ColorType = ColorType;
				num2 += Math.PI * 2.0 / 3.0;
			}
		}
		else if (base.Time == base.LifeTime)
		{
			BaseEffect baseEffect = new BaseEffect(base.StageData, "Light0" + ColorType, base.Position, 0f, 0.0);
			baseEffect.LifeTime = 50;
			baseEffect.ScaleVelocity = 0.4f;
			baseEffect.TransparentVelocityDictionary.Add(20, -5f);
		}
	}
}
