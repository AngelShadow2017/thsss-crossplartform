using System;
using System.Drawing;

namespace Shooting;

public class BulletRemover2 : BaseBackground
{
	public BulletRemover2(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = 30;
		base.Region = 2;
		foreach (BaseEnemyPlane enemyPlane in base.EnemyPlaneList)
		{
			enemyPlane.GiveEndEffect();
		}
		base.EnemyPlaneList.Clear();
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Region += 25;
		for (int num = base.BulletList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.BulletList[num], base.Region))
			{
				base.BulletList[num].GiveEndEffect();
				base.BulletList[num].GiveItems();
				base.BulletList.RemoveAt(num);
			}
		}
	}
}
