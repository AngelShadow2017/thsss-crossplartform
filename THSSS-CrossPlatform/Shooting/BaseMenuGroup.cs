using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

public class BaseMenuGroup : BaseObject
{
	public int LastDown = 1;

	public int LastUp = 1;

	public int LastLeft = 1;

	public int LastRight = 1;

	public int LastZ = 1;

	public int LastX = 1;

	public int OnChangeMenu = 0;

	public int OnRemoveMenu = 0;

	public PointF MenuTitlePos1 = new PointF(280f, -30f);

	public PointF MenuTitlePos2 = new PointF(280f, -150f);

	public int MenuSelectIndex { get; set; }

	public int LastSelectTime { get; set; }

	public List<BaseMenuItem> MenuItemList { get; set; }

	public BaseMenuItem MenuTilte { get; set; }

	public BaseMenuGroup()
	{
	}

	public BaseMenuGroup(StageDataPackage StageData)
	{
		base.StageData = StageData;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (MenuItemList.Count <= 0)
		{
			return;
		}
		foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(MenuItemList)){
			x.Ctrl();
		};
		if (MenuTilte != null)
		{
			if (base.StageData.MenuGroupList[base.StageData.MenuGroupList.Count - 1] == this && base.TimeMain > OnRemoveMenu)
			{
				MenuTilte.DestPoint = MenuTitlePos1;
			}
			else
			{
				MenuTilte.DestPoint = MenuTitlePos2;
			}
			MenuTilte.Ctrl();
			MenuTilte.ColorValue = Color.White;
			MenuTilte.TransparentValueF = 255f;
		}
	}

	public virtual void MenuSelect()
	{
		if (base.TimeMain - LastSelectTime < 7 || base.TimeMain < OnChangeMenu)
		{
			return;
		}
		if (base.TimeMain == OnChangeMenu)
		{
			ProcessZ();
		}
		else if (base.TimeMain >= OnRemoveMenu)
		{
			if (base.TimeMain == OnRemoveMenu)
			{
				ProcessX();
				return;
			}
			ProcessKeys();
			LastDown = (base.KClass.ArrowDown ? (LastDown + 1) : 0);
			LastUp = (base.KClass.ArrowUp ? (LastUp + 1) : 0);
			LastLeft = (base.KClass.ArrowLeft ? (LastLeft + 1) : 0);
			LastRight = (base.KClass.ArrowRight ? (LastRight + 1) : 0);
			LastZ = (base.KClass.Key_Z ? (LastZ + 1) : 0);
			LastX = ((base.KClass.Key_X || base.KClass.Key_ESC) ? (LastX + 1) : 0);
		}
	}

	public virtual void ProcessKeys()
	{
		if (base.KClass.ArrowDown && (LastDown == 0 || LastDown > 30))
		{
			MenuSelectIndex++;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.ArrowUp && (LastUp == 0 || LastUp > 30))
		{
			MenuSelectIndex--;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
	}

	public virtual void ProcessZ()
	{
	}

	public virtual void ProcessX()
	{
		base.StageData.MenuGroupList.Remove(this);
		base.StageData.MenuGroupList[base.StageData.MenuGroupList.Count - 1].LastSelectTime = base.TimeMain;
	}

	public virtual void SelectItemChanged()
	{
		if (MenuSelectIndex < 0)
		{
			MenuSelectIndex += MenuItemList.Count;
		}
		else if (MenuSelectIndex > MenuItemList.Count - 1)
		{
			MenuSelectIndex -= MenuItemList.Count;
		}
		LastSelectTime = base.TimeMain;
		foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(MenuItemList)){
			x.Selected = false;
		};
		MenuItemList[MenuSelectIndex].Select();
	}

	public override void Show()
	{
		base.Show();
		if (MenuItemList.Count > 0)
		{
			foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(MenuItemList)){
				x.Show();
			};
			if (MenuTilte != null)
			{
				MenuTilte.Show();
			}
		}
	}
}
