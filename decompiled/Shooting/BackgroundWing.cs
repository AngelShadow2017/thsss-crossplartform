using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

public class BackgroundWing : BaseParticle3D
{
	private int lastlife;

	public BackgroundWing(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, int LifeTime)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		StageData.Particle3D.ParticleList.Add(this);
		base.LifeTime = LifeTime;
		if (base.Boss != null)
		{
			SetBinding(base.Boss);
		}
		base.Scale = 0.1f;
		base.MaxScale = 0.6f;
		base.ScaleVelocity = 0.02f;
		base.RotatingAxis = new Vector3(0f, 1f, 0f);
		lastlife = base.Boss.Life;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Angle3D = 0f - (float)(Math.PI / 18.0 * (1.0 - Math.Sin((float)base.Time / 30f)));
		if (base.Boss != null)
		{
			if (base.Boss.Time > base.Boss.UnmatchedTime && base.Time > 50 && base.Boss.HitEnabled)
			{
				for (int num = base.MyBulletList.Count - 1; num >= 0; num--)
				{
					if (base.MyBulletList[num].OriginalPosition.Y < base.OriginalPosition.Y && base.MyBulletList[num].OriginalPosition.X > base.OriginalPosition.X - 100f && base.MyBulletList[num].OriginalPosition.X < base.OriginalPosition.X + 100f)
					{
						if (base.Boss.Time > base.Boss.UnmatchedTime)
						{
							base.Boss.HealthPoint -= (float)base.MyBulletList[num].Damage * (1f - base.Boss.Armon) / 3f;
						}
						base.MyBulletList[num].GiveEndEffect();
						base.MyBulletList.RemoveAt(num);
						base.MyPlane.Score += 100L;
					}
				}
			}
			if (base.Boss.Life < lastlife)
			{
				base.LifeTime = base.Time + 10;
				base.TransparentVelocity = -25f;
				lastlife = base.Boss.Life;
			}
		}
		if (base.Time == base.LifeTime)
		{
			BulletPicEmitter.ShootItem(base.StageData, "WingItem.png", base.OriginalPosition, 10f);
		}
	}
}
