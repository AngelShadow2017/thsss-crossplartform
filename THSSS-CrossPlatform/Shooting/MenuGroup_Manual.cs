using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_Manual : BaseMenuGroup
{
	private List<BaseMenuItem> MenuItemListManual;

	public MenuGroup_Manual(StageDataPackage StageData)
		: base(StageData)
	{
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "Menu_Manual01"),
			new BaseMenuItem(StageData, "Menu_Manual02"),
			new BaseMenuItem(StageData, "Menu_Manual03"),
			new BaseMenuItem(StageData, "Menu_Manual04"),
			new BaseMenuItem(StageData, "Menu_Manual05"),
			new BaseMenuItem(StageData, "Menu_Manual06")
		};
		int num = 20;
		int num2 = 85;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.Position = new PointF(num, 90f);
			menuItem.DestPoint = new PointF(num, num2);
			num2 += 30;
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		MenuItemListManual = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "Manual01"),
			new BaseMenuItem(StageData, "Manual02"),
			new BaseMenuItem(StageData, "Manual03"),
			new BaseMenuItem(StageData, "Manual04"),
			new BaseMenuItem(StageData, "Manual05"),
			new BaseMenuItem(StageData, "Manual06")
		};
		foreach (BaseMenuItem item in MenuItemListManual)
		{
			item.Position = new PointF(0f, 0f);
			item.DestPoint = new PointF(0f, 0f);
			item.UnSelectVisible = false;
			item.Vibrateable = false;
			item.Twinkleable = false;
			item.TransparentValueF = 0f;
		}
		MenuItemListManual[base.MenuSelectIndex].Selected = true;
		base.TransparentValueF = 0f;
		base.MaxTransparent = 255;
		base.TransparentVelocity = 5f;
		base.TxtureObject = base.TextureObjectDictionary["MenuBackground"];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		base.AngleDegree = 90.0;
		base.ColorValue = Color.FromArgb(30, 110, 150);
	}

	public override void SelectItemChanged()
	{
		base.SelectItemChanged();
		foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(MenuItemListManual)){
			x.Selected = false;
		};
		MenuItemListManual[base.MenuSelectIndex].Select();
	}

	public override void Ctrl()
	{
		base.Ctrl();
		foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(MenuItemListManual)){
			x.Ctrl();
		};
	}

	public override void ProcessKeys()
	{
		base.ProcessKeys();
		if (!base.KClass.Key_Z || LastZ == 0)
		{
		}
		if ((!base.KClass.Key_X && !base.KClass.Key_ESC) || LastX != 0)
		{
			return;
		}
		OnRemoveMenu = base.TimeMain + 20;
		base.TransparentVelocity = -15f;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.OnRemove = true;
		}
		foreach (BaseMenuItem item in MenuItemListManual)
		{
			item.OnRemove = true;
		}
		base.StageData.SoundPlay("se_cancel00.wav");
	}

	public override void Show()
	{
		base.Show();
		foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(MenuItemListManual)){
			x.Show();
		};
	}
}
