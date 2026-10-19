using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_RankSelectEx : MenuGroup_RankSelect
{
	public MenuGroup_RankSelectEx(StageDataPackage StageData)
		: base(StageData, StageSelect: false)
	{
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "Menu_Extra")
		};
		int num = 200 + base.MenuSelectIndex * 150;
		int num2 = 200 - base.MenuSelectIndex * 100;
		int num3 = -200;
		int num4 = 0;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.Position = new PointF(num + num3, 48f);
			menuItem.DestPoint = new PointF(num, num2);
			menuItem.Scale = 0.8f;
			menuItem.Vibrateable = false;
			menuItem.MaxVelocity = 30f;
			num2 += 100;
			num -= 150;
			num3 -= 100;
			num4++;
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
	}

	public override void SelectItemChanged()
	{
		if (base.MenuSelectIndex < 0)
		{
			base.MenuSelectIndex = 0;
		}
		else if (base.MenuSelectIndex > base.MenuItemList.Count - 1)
		{
			base.MenuSelectIndex = base.MenuItemList.Count - 1;
		}
		base.LastSelectTime = base.TimeMain;
		base.MenuItemList.ForEach(delegate(BaseMenuItem x)
		{
			x.Selected = false;
		});
		base.MenuItemList[base.MenuSelectIndex].Select();
	}

	public override void ProcessZ()
	{
		base.StageData.StateSwitchData.SDPswitch.Difficulty = DifficultLevel.Extra;
		base.StageData.MenuGroupList.Add(new MenuGroup_PlayerSelectEx(base.StageData)
		{
			OriginalPosition = base.OriginalPosition
		});
	}
}
