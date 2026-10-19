using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_RankSelect : BaseMenuGroup
{
	private bool StageSelect;

	public MenuGroup_RankSelect(StageDataPackage StageData, bool StageSelect)
		: base(StageData)
	{
		this.StageSelect = StageSelect;
		base.MenuSelectIndex = 1;
		base.MenuItemList = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "Menu_Easy"),
			new BaseMenuItem(StageData, "Menu_Normal"),
			new BaseMenuItem(StageData, "Menu_Hard"),
			new BaseMenuItem(StageData, "Menu_Lunatic")
		};
		int num = 220 + base.MenuSelectIndex * 150;
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
		MenuTitlePos1 = new PointF(0f, 0f);
		MenuTitlePos2 = new PointF(0f, -150f);
		base.MenuTilte = new BaseMenuItem(StageData, "MenuTitle_RankSelect")
		{
			Position = MenuTitlePos2
		};
		base.TransparentValueF = 0f;
		base.TransparentVelocity = 20f;
		base.TxtureObject = base.TextureObjectDictionary["MenuBackground"];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		base.AngleDegree = 90.0;
		base.Scale = 0.625f;
		base.MaxScale = 0.75f;
		base.ScaleVelocity = 0.005f;
		base.DestPoint = new PointF(base.BoundRect.Width / 2 + 30 * (base.MenuSelectIndex - 1), base.BoundRect.Height / 2 - 20 * (base.MenuSelectIndex - 1));
		base.Velocity = 4f;
		base.AngleWithDirection = false;
	}

	public override void SelectItemChanged()
	{
		base.SelectItemChanged();
		int num = 220 + base.MenuSelectIndex * 150;
		int num2 = 200 - base.MenuSelectIndex * 100;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(num, num2);
			num2 += 100;
			num -= 150;
		}
		base.DestPoint = new PointF(base.BoundRect.Width / 2 + 30 * (base.MenuSelectIndex - 1), base.BoundRect.Height / 2 - 20 * (base.MenuSelectIndex - 1));
		base.Velocity = 4f;
	}

	public override void ProcessKeys()
	{
		if ((base.KClass.ArrowDown && (LastDown == 0 || LastDown > 30)) || (base.KClass.ArrowLeft && (LastLeft == 0 || LastLeft > 30)))
		{
			base.MenuSelectIndex++;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if ((base.KClass.ArrowUp && (LastUp == 0 || LastUp > 30)) || (base.KClass.ArrowRight && (LastRight == 0 || LastRight > 30)))
		{
			base.MenuSelectIndex--;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.Key_Z && LastZ == 0)
		{
			base.MenuItemList[base.MenuSelectIndex].Click();
			OnChangeMenu = base.TimeMain + 20;
			base.StageData.SoundPlay("se_ok00.wav");
		}
		if ((!base.KClass.Key_X && !base.KClass.Key_ESC) || LastX != 0)
		{
			return;
		}
		OnRemoveMenu = base.TimeMain + 20;
		base.TransparentVelocity = -15f;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(0f, 0f);
			menuItem.OnRemove = true;
		}
		base.StageData.SoundPlay("se_cancel00.wav");
	}

	public override void ProcessZ()
	{
		switch (base.MenuItemList[base.MenuSelectIndex].Name)
		{
		case "Menu_Easy":
			base.StageData.StateSwitchData.SDPswitch.Difficulty = DifficultLevel.Easy;
			break;
		case "Menu_Normal":
			base.StageData.StateSwitchData.SDPswitch.Difficulty = DifficultLevel.Normal;
			break;
		case "Menu_Hard":
			base.StageData.StateSwitchData.SDPswitch.Difficulty = DifficultLevel.Hard;
			break;
		case "Menu_Lunatic":
			base.StageData.StateSwitchData.SDPswitch.Difficulty = DifficultLevel.Lunatic;
			break;
		case "Ultra":
			base.StageData.StateSwitchData.SDPswitch.Difficulty = DifficultLevel.Ultra;
			break;
		default:
			base.StageData.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = "此功能尚未开通",
				NeedInit = true,
				SDPswitch = base.StageData
			};
			break;
		}
		base.StageData.MenuGroupList.Add(new MenuGroup_PlayerSelect(base.StageData, StageSelect));
	}

	public override void Show()
	{
		base.Show();
		base.MenuItemList[base.MenuSelectIndex].Show();
	}
}
