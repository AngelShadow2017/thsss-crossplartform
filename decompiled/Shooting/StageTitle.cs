using System;
using System.Drawing;

namespace Shooting;

public class StageTitle : BaseEffect
{
	public StageTitle(StageDataPackage StageData, string TextureName)
		: base(StageData, TextureName, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2 - 80);
		base.LifeTime = 200;
		base.TransparentValueF = 0f;
		base.Active = false;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time < base.LifeTime / 3)
		{
			base.TransparentValueF += base.MaxTransparent * 3 / base.LifeTime;
		}
		else if (base.Time > base.LifeTime * 2 / 3)
		{
			base.TransparentValueF -= base.MaxTransparent * 3 / base.LifeTime;
		}
	}
}
