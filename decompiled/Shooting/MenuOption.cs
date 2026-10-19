using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuOption : BaseMenuGroup
{
	private BaseNum BGMvolumn;

	private BaseNum SEvolumn;

	public MenuOption(StageDataPackage StageData)
		: base(StageData)
	{
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "Menu_BGM_Vol"),
			new BaseMenuItem(StageData, "Menu_SE_Vol"),
			new BaseMenuItem(StageData, "Menu_Quit")
		};
		int num = 220;
		int num2 = 220;
		int num3 = -200;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(num, num2);
			menuItem.Position = new PointF(num + num3, 140f);
			num2 += 24;
			num3 -= 50;
		}
		BGMvolumn = new BaseNum(StageData, new PointF(base.MenuItemList[0].DestPoint.X + 130f, base.MenuItemList[0].DestPoint.Y + 16f))
		{
			Num = base.BGM_Player.Volume,
			Count = 3
		};
		SEvolumn = new BaseNum(StageData, new PointF(base.MenuItemList[1].DestPoint.X + 130f, base.MenuItemList[1].DestPoint.Y + 16f))
		{
			Num = StageData.GlobalData.SEVolume,
			Count = 3
		};
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
	}

	public override void ProcessKeys()
	{
		base.ProcessKeys();
		if (base.KClass.Key_Z && LastZ == 0)
		{
			switch (base.MenuItemList[base.MenuSelectIndex].Name)
			{
			case "Menu_Quit":
				OnRemoveMenu = base.TimeMain + 20;
				base.TransparentVelocity = -15f;
				foreach (BaseMenuItem menuItem in base.MenuItemList)
				{
					menuItem.DestPoint = new PointF(60f, 160f);
					menuItem.OnRemove = true;
				}
				break;
			default:
				base.StageData.StateSwitchData = new StateSwitchDataPackage
				{
					NextState = "此功能尚未开通",
					NeedInit = true,
					SDPswitch = base.StageData
				};
				break;
			case "Menu_BGM_Vol":
			case "Menu_SE_Vol":
				break;
			}
			base.StageData.SoundPlay("se_ok00.wav");
		}
		if ((base.KClass.Key_X || base.KClass.Key_ESC) && LastX == 0)
		{
			base.MenuSelectIndex = base.MenuItemList.Count - 1;
			SelectItemChanged();
			base.StageData.SoundPlay("se_cancel00.wav");
		}
		if (base.KClass.ArrowRight && (LastRight == 0 || LastRight > 30))
		{
			switch (base.MenuItemList[base.MenuSelectIndex].Name)
			{
			case "Menu_BGM_Vol":
				base.BGM_Player.Volume += 5;
				BGMvolumn.Num = base.BGM_Player.Volume;
				break;
			case "Menu_SE_Vol":
				foreach (XAudio2_Player value in base.StageData.SoundDictionary.Values)
				{
					value.Volume += 5;
					SEvolumn.Num = value.Volume;
				}
				base.StageData.SoundPlay("se_ok00.wav");
				break;
			}
			base.LastSelectTime = base.TimeMain;
			base.StageData.GlobalData.BGMVolume = base.BGM_Player.Volume;
			base.StageData.GlobalData.SEVolume = SEvolumn.Num;
		}
		if (!base.KClass.ArrowLeft || (LastLeft != 0 && LastLeft <= 30))
		{
			return;
		}
		switch (base.MenuItemList[base.MenuSelectIndex].Name)
		{
		case "Menu_BGM_Vol":
			base.BGM_Player.Volume -= 5;
			BGMvolumn.Num = base.BGM_Player.Volume;
			break;
		case "Menu_SE_Vol":
			foreach (XAudio2_Player value2 in base.StageData.SoundDictionary.Values)
			{
				value2.Volume -= 5;
				SEvolumn.Num = value2.Volume;
			}
			base.StageData.SoundPlay("se_ok00.wav");
			break;
		}
		base.LastSelectTime = base.TimeMain;
		base.StageData.GlobalData.BGMVolume = base.BGM_Player.Volume;
		base.StageData.GlobalData.SEVolume = SEvolumn.Num;
	}

	public override void Show()
	{
		base.Show();
		BGMvolumn.Show();
		SEvolumn.Show();
	}
}
