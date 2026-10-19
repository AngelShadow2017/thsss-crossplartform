using System;
using System.Drawing;

namespace Shooting;

public class Bullet_Touhou_01_01 : Bullet_Touhou_01
{
	public string SubBulletName { get; set; }

	public Bullet_Touhou_01_01(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, byte ColorType)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction, ColorType)
	{
	}

	public override bool OutBoundary()
	{
		if (base.LifeTime != 0 && base.Time > base.LifeTime)
		{
			return true;
		}
		int num;
		if (base.Reflect <= 0)
		{
			if (base.Time < 100)
			{
				return false;
			}
			num = ((base.Region >= 10) ? base.Region : 10);
			if ((float)(-num) < base.OriginalPosition.X && base.OriginalPosition.X < (float)(base.BoundRect.Width + num) && (float)(-num) < base.OriginalPosition.Y && base.OriginalPosition.Y < (float)(base.BoundRect.Height + num))
			{
				return false;
			}
			return true;
		}
		num = 5;
		if (base.OriginalPosition.X < (float)num || base.OriginalPosition.X >= (float)(base.BoundRect.Width - num))
		{
			base.Reflect--;
			base.Direction -= 2.0 * (base.Direction - Math.PI / 2.0);
			new BaseBullet_Touhou(base.StageData, SubBulletName, base.OriginalPosition, base.Velocity, base.Direction, base.ColorType);
			GiveEndEffect();
			return true;
		}
		if (base.OriginalPosition.Y > (float)(base.BoundRect.Bottom + num))
		{
			return true;
		}
		return false;
	}
}
