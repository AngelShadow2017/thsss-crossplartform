using System;
using System.Drawing;

namespace Shooting;

public class Background2DRemover : BaseEffect
{
	public Background2DRemover(StageDataPackage StageData)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.LifeTime = 20;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		foreach (BaseObject x in SlimDX.Compat.ListCompat.LegacyItems(base.Background.BackgroundList)){
			x.TransparentValueF -= base.MaxTransparent / base.LifeTime;
		};
		if (base.Time >= base.LifeTime)
		{
			base.Background.BackgroundList.Clear();
		}
	}

	public override void Show()
	{
	}
}
