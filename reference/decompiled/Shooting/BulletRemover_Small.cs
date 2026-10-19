using System;
using System.Drawing;

namespace Shooting;

public class BulletRemover_Small : BaseBackground
{
	public BulletRemover_Small(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = 10;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		for (int num = base.BulletList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.BulletList[num], base.Region) && !base.BulletList[num].UnRemoveable)
			{
				base.BulletList[num].GiveEndEffect();
				base.BulletList[num].GiveItems();
				base.BulletList.RemoveAt(num);
			}
		}
	}
}
