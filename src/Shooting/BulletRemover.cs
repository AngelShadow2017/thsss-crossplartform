using System;
using System.Drawing;

namespace Shooting;

public class BulletRemover : BaseBackground
{
	public BulletRemover(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = 30;
		base.Region = 2;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Region += 20;
		for (int num = base.BulletList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.BulletList[num], base.Region) && !base.BulletList[num].UnRemoveable)
			{
				base.BulletList[num].GiveEndEffect();
				base.BulletList.RemoveAt(num);
			}
		}
		for (int num = base.EnemyPlaneList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.EnemyPlaneList[num], base.Region))
			{
				base.EnemyPlaneList[num].HealthPoint -= 20f;
				if (base.EnemyPlaneList[num].HealthPoint <= 0f)
				{
					base.EnemyPlaneList[num].GiveEndEffect();
					base.EnemyPlaneList[num].GiveItems();
					base.EnemyPlaneList.RemoveAt(num);
					base.StageData.SoundPlay("se_enep00.wav");
				}
			}
		}
	}
}
