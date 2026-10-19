using System;
using System.Drawing;

namespace Shooting;

public class ItemGetter : BaseEffect
{
	public ItemGetter(StageDataPackage StageData)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.LifeTime = 2;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		foreach (BaseItem x in SlimDX.Compat.ListCompat.Snapshot(base.ItemList)){
			x.Obtain = true;
			x.Doubled = true;
		};
	}

	public override void Show()
	{
	}
}
