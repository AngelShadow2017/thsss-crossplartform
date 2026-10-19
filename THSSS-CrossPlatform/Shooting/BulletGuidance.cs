using System;
using System.Drawing;

namespace Shooting;

public class BulletGuidance : BaseObject
{
	private bool targetFlag = false;

	public BaseObject Target { get; set; }

	public BulletGuidance(StageDataPackage StageData, string textureName, PointF Position, float Velocity, double Direction)
		: base(StageData, textureName, Position, Velocity, Direction)
	{
		base.Damage = 3;
		base.Region = 15;
		base.LifeTime = 100;
		base.MyBulletList.Add(this);
		if (base.Boss != null && base.Boss.OriginalPosition.X > 0f && base.Boss.OriginalPosition.X < (float)base.BoundRect.Width && base.Boss.OriginalPosition.Y > 0f && base.Boss.OriginalPosition.Y < (float)base.BoundRect.Height && base.Boss.HitEnabled)
		{
			Target = base.Boss;
		}
		base.GhostingCount = 5;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (Target == null && !targetFlag)
		{
			double dist = 300.0;
			base.EnemyPlaneList.ForEach(delegate(BaseEnemyPlane x)
			{
				if (x.HitEnabled)
				{
					double distance = GetDistance(x);
					if (distance < dist)
					{
						Target = x;
						dist = distance;
					}
				}
			});
		}
		else
		{
			targetFlag = true;
		}
		if (Target != null)
		{
			if (Target.HealthPoint > 0f)
			{
				double num = GetDirection(Target);
				double num2 = base.Direction;
				if (num > Math.PI)
				{
					num -= Math.PI * 2.0;
				}
				if (num2 > Math.PI)
				{
					num2 -= Math.PI * 2.0;
				}
				double num3 = num - num2;
				if (num3 > Math.PI)
				{
					num3 -= Math.PI * 2.0;
				}
				if (Math.Abs(num3) > 0.019999999552965164 && GetDistance(Target) > 50.0)
				{
					base.Direction += num3 / 5.0;
				}
				else
				{
					base.Direction += num3;
				}
			}
			else
			{
				Target = null;
			}
		}
		if (OutBoundary())
		{
			base.MyBulletList.Remove(this);
		}
	}

	public override bool OutBoundary()
	{
		if (base.LifeTime != 0 && base.Time > base.LifeTime)
		{
			return true;
		}
		if (base.Time < 10)
		{
			return false;
		}
		int num = 2;
		if ((float)(-num) < base.OriginalPosition.X && base.OriginalPosition.X < (float)(base.BoundRect.Width + num) && (float)(-num) < base.OriginalPosition.Y && base.OriginalPosition.Y < (float)(base.BoundRect.Height + num))
		{
			return false;
		}
		return true;
	}

	public override void GiveEndEffect()
	{
		ParticleBiger particleBiger = new ParticleBiger(base.StageData, null, base.Position, 2f, base.Direction);
		particleBiger.Scale = 1.2f;
		particleBiger.LifeTime = 15;
		particleBiger.TxtureObject = base.TxtureObject;
		particleBiger.Angle = base.Angle;
	}
}
