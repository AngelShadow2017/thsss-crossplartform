using System;
using System.Drawing;

namespace Shooting;

public class Bullet_BendLaser : BaseBullet_Touhou
{
	private Particle3D_BendLaser PL;

	public Bullet_BendLaser(StageDataPackage StageData)
		: base(StageData)
	{
	}

	public Bullet_BendLaser(StageDataPackage StageData, PointF OriginalPosition, float Velocity, double Direction, byte ColorType, int Length)
		: this(StageData, OriginalPosition, Velocity, Direction, ColorType, Length, 16)
	{
	}

	public Bullet_BendLaser(StageDataPackage StageData, PointF OriginalPosition, float Velocity, double Direction, byte ColorType, int Length, int Width)
		: this(StageData, null, OriginalPosition, Velocity, Direction, Length)
	{
		base.ColorType = ColorType;
		ParticleBeginBullet particleBeginBullet = new ParticleBeginBullet(StageData, base.Position, ColorType)
		{
			Scale = 0.5f,
			Active = true
		};
		PL = new Particle3D_BendLaser(StageData, base.GhostingCount, Width, ColorType)
		{
			OriginalPosition = base.OriginalPosition,
			LastPoints = base.LastPoints
		};
	}

	public Bullet_BendLaser(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, int Length)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		base.GhostingCount = Length;
		base.Region = 2;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 1 && PL == null)
		{
			PL = new Particle3D_BendLaser(base.StageData, base.GhostingCount, (int)(base.ScaleLength * 16f), base.ColorType)
			{
				OriginalPosition = base.OriginalPosition,
				LastPoints = base.LastPoints
			};
		}
		PL.LastPoints = base.LastPoints;
		PL.TransparentValueF = base.TransparentValueF;
		if (base.Time % 8 == 0)
		{
			base.Grazed = false;
		}
	}

	public override bool OutBoundary()
	{
		if (base.LifeTime != 0 && base.Time > base.LifeTime)
		{
			PL.Dispose();
			base.StageData.Particle3D.ParticleList.Remove(PL);
			return true;
		}
		if (base.Reflect <= 0)
		{
			if (base.Time < 100)
			{
				return false;
			}
			int num = ((base.OutsideRegion >= 10) ? (base.OutsideRegion * 2) : 20);
			if ((float)(-num) < base.LastPoints[base.LastPoints.Length - 1].X && base.LastPoints[base.LastPoints.Length - 1].X < (float)(base.BoundRect.Width + num) && (float)(-num) < base.LastPoints[base.LastPoints.Length - 1].Y && base.LastPoints[base.LastPoints.Length - 1].Y < (float)(base.BoundRect.Height + num))
			{
				return false;
			}
			if ((float)(-num) < base.OriginalPosition.X && base.OriginalPosition.X < (float)(base.BoundRect.Width + num) && (float)(-num) < base.OriginalPosition.Y && base.OriginalPosition.Y < (float)(base.BoundRect.Height + num))
			{
				return false;
			}
			PL.Dispose();
			base.StageData.Particle3D.ParticleList.Remove(PL);
			return true;
		}
		if (base.OriginalPosition.X < 0f || base.OriginalPosition.X >= (float)base.BoundRect.Width)
		{
			base.Reflect--;
			base.Direction -= 2.0 * (base.Direction - Math.PI / 2.0);
		}
		if (base.OriginalPosition.Y < 20f)
		{
			base.Reflect--;
			base.Direction = 0.0 - base.Direction;
		}
		return false;
	}

	public override bool HitCheck(BaseObject Target, float Radius)
	{
		for (int i = 3; i < base.LastPoints.Length - 3; i++)
		{
			PointF pointF = base.LastPoints[i];
			double num = Math.Sqrt((pointF.X - Target.OriginalPosition.X) * (pointF.X - Target.OriginalPosition.X) + (pointF.Y - Target.OriginalPosition.Y) * (pointF.Y - Target.OriginalPosition.Y));
			if (num - (double)Target.Region < (double)Radius)
			{
				return true;
			}
		}
		return false;
	}

	public override void GiveEndEffect()
	{
		for (int i = 0; i < base.LastPoints.Length; i += 8)
		{
			new ParticleEndBullet(base.StageData, base.LastPoints[i], base.ColorType, base.SizeValue);
		}
		base.StageData.Particle3D.ParticleList.Remove(PL);
	}

	public override void GiveItems()
	{
		int num = 5;
		int num2 = base.Region / num;
		for (int i = 0; i < base.LastPoints.Length; i += 8)
		{
			for (int j = 0; j <= num2; j++)
			{
				double num3 = base.Direction;
				for (int k = 0; k < j * 5 + 1; k++)
				{
					new StarItem_Touhou(OriginalPosition: new PointF((float)((double)base.LastPoints[i].X + (double)j * Math.Cos(num3)), (float)((double)base.LastPoints[i].Y + (double)j * Math.Sin(num3))), StageData: base.StageData);
					num3 += Math.PI * 2.0 / (double)(j * 6);
				}
			}
		}
	}

	public override void Show()
	{
	}

	public override void ShowRegion()
	{
		PointF originalPosition = base.OriginalPosition;
		for (int i = 3; i < base.LastPoints.Length - 3; i++)
		{
			base.OriginalPosition = base.LastPoints[i];
			base.ShowRegion();
		}
		base.OriginalPosition = originalPosition;
	}
}
