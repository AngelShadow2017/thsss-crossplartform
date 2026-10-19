using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_StageSelect : BaseMenuGroup
{
	public MenuGroup_StageSelect(StageDataPackage StageData)
		: base(StageData)
	{
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>();
		base.MenuItemList.Add(new BaseMenuItem(StageData, "Stage01"));
		base.MenuItemList.Add(new BaseMenuItem(StageData, "Boss01"));
		int practiceLevel = 1;
		foreach (ClearHistory a in SlimDX.Compat.ListCompat.LegacyItems(StageData.PData.C_History)){
			if (a.Rank == StageData.StateSwitchData.SDPswitch.Difficulty && a.MyPlaneFullName == StageData.StateSwitchData.SDPswitch.MyPlane.FullName)
			{
				practiceLevel = a.PracticeLevel;
			}
		};
		if (practiceLevel > 1)
		{
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Stage02"));
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Boss02"));
		}
		if (practiceLevel > 2)
		{
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Stage03"));
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Boss03"));
		}
		if (practiceLevel > 3)
		{
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Stage04"));
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Boss04"));
		}
		if (practiceLevel > 4)
		{
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Stage05"));
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Boss05"));
		}
		if (practiceLevel > 5)
		{
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Stage06"));
			base.MenuItemList.Add(new BaseMenuItem(StageData, "Boss06"));
		}
		int num = 440;
		int num2 = 120;
		int num3 = 0;
		int num4 = 0;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.Position = new PointF(num + num3, 120f);
			if (num4 % 2 == 0)
			{
				menuItem.DestPoint = new PointF(num, num2);
			}
			else
			{
				menuItem.DestPoint = new PointF(num + 80, num2);
				num2 += 32;
			}
			num4++;
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		base.MenuTilte = new BaseMenuItem(StageData, "MenuTitle_StageSelect")
		{
			Position = MenuTitlePos2
		};
		base.TransparentValueF = 0f;
		base.MaxTransparent = 210;
		base.TransparentVelocity = 10f;
		base.AngleDegree = 90.0;
		base.Scale = 0.75f;
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2 + 30 * (int)(StageData.StateSwitchData.SDPswitch.Difficulty - 1), base.BoundRect.Height / 2 - 20 * (int)(StageData.StateSwitchData.SDPswitch.Difficulty - 1));
	}

	public override void ProcessKeys()
	{
		if (base.KClass.ArrowDown && (LastDown == 0 || LastDown > 30))
		{
			base.MenuSelectIndex += 2;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.ArrowUp && (LastUp == 0 || LastUp > 30))
		{
			base.MenuSelectIndex -= 2;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.ArrowLeft && (LastLeft == 0 || LastLeft > 30))
		{
			base.MenuSelectIndex--;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.ArrowRight && (LastRight == 0 || LastRight > 30))
		{
			base.MenuSelectIndex++;
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
			menuItem.DestPoint = new PointF(480f, 120f);
			menuItem.OnRemove = true;
		}
		base.StageData.SoundPlay("se_cancel00.wav");
	}

	public override void ProcessZ()
	{
		base.StageData.StateSwitchData.SDPswitch.MyPlane.Life = 9;
		base.StageData.StateSwitchData.SDPswitch.OnPractice = true;
		switch (base.MenuItemList[base.MenuSelectIndex].Name)
		{
		case "Stage01":
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "St1";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Stage02":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 300;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "St2";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Stage03":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "St3";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Stage04":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "St4";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Stage05":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "St5";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Stage06":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "St6";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Boss01":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 200;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Bs1";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Boss02":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 300;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Bs2";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Boss03":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Bs3";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Boss04":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Bs4";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Boss05":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Bs5";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "Boss06":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Bs6";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		case "StageEx":
			base.StageData.StateSwitchData.SDPswitch.MyPlane.Power = 400;
			base.StageData.Rep.CreatRpy();
			base.StageData.StateSwitchData.NextState = "Stage_test";
			base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			break;
		}
	}
}
