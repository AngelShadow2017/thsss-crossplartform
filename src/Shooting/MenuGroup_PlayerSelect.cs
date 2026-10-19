using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_PlayerSelect : BaseMenuGroup
{
	private bool StageSelect;

	protected List<BaseMenuItem> PlayerDescription;

	public MenuGroup_PlayerSelect(StageDataPackage StageData, bool StageSelect)
		: base(StageData)
	{
		this.StageSelect = StageSelect;
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>();
		PlayerDescription = new List<BaseMenuItem>();
		base.MenuItemList.Add(new CharacterMenuItem(StageData, "FaceReimu_me")
		{
			DestPoint1 = new PointF(-128f, 10f),
			DestPoint2 = new PointF(-228f, 10f),
			TransparentValueF = 0f
		});
		PlayerDescription.Add(new CharacterMenuItem(StageData, "DescriptionReimu")
		{
			DestPoint1 = new PointF(190f, 10f),
			DestPoint2 = new PointF(260f, 10f),
			TransparentValueF = 0f
		});
		base.MenuItemList.Add(new CharacterMenuItem(StageData, "FaceMarisa_me")
		{
			DestPoint1 = new PointF(-128f, 10f),
			DestPoint2 = new PointF(-228f, 10f),
			TransparentValueF = 0f
		});
		PlayerDescription.Add(new CharacterMenuItem(StageData, "DescriptionMarisa")
		{
			DestPoint1 = new PointF(190f, 10f),
			DestPoint2 = new PointF(260f, 10f),
			TransparentValueF = 0f
		});
		base.MenuItemList.Add(new CharacterMenuItem(StageData, "FaceSanae_me")
		{
			DestPoint1 = new PointF(0f, 30f),
			DestPoint2 = new PointF(-100f, 30f),
			TransparentValueF = 0f
		});
		PlayerDescription.Add(new CharacterMenuItem(StageData, "DescriptionSanae")
		{
			DestPoint1 = new PointF(190f, 10f),
			DestPoint2 = new PointF(260f, 10f),
			TransparentValueF = 0f
		});
		base.MenuItemList.Add(new CharacterMenuItem(StageData, "FaceKoishi_me")
		{
			DestPoint1 = new PointF(-128f, 10f),
			DestPoint2 = new PointF(-228f, 10f),
			TransparentValueF = 0f
		});
		PlayerDescription.Add(new CharacterMenuItem(StageData, "DescriptionKoishi")
		{
			DestPoint1 = new PointF(190f, 10f),
			DestPoint2 = new PointF(260f, 10f),
			TransparentValueF = 0f
		});
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		PlayerDescription[base.MenuSelectIndex].Selected = true;
		base.MenuTilte = new BaseMenuItem(StageData, "MenuTitle_PlayerSelect")
		{
			Position = MenuTitlePos2
		};
		base.TransparentValueF = 0f;
		base.MaxTransparent = 255;
		base.TransparentVelocity = 10f;
		base.TxtureObject = base.TextureObjectDictionary["MenuBackground"];
		base.AngleDegree = 90.0;
		base.Scale = 0.75f;
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2 + 30 * (int)(StageData.StateSwitchData.SDPswitch.Difficulty - 1), base.BoundRect.Height / 2 - 20 * (int)(StageData.StateSwitchData.SDPswitch.Difficulty - 1));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.LegacyItems(PlayerDescription)){
			x.Ctrl();
		};
	}

	public override void ProcessKeys()
	{
		if (base.KClass.ArrowLeft && LastLeft == 0)
		{
			base.MenuSelectIndex--;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.ArrowRight && LastRight == 0)
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
			menuItem.Selected = false;
			menuItem.OnRemove = true;
		}
		foreach (BaseMenuItem item in PlayerDescription)
		{
			item.Selected = false;
			item.OnRemove = true;
		}
		base.StageData.SoundPlay("se_cancel00.wav");
	}

	public override void SelectItemChanged()
	{
		base.SelectItemChanged();
		for (int i = 0; i < PlayerDescription.Count; i++)
		{
			if (i == base.MenuSelectIndex)
			{
				PlayerDescription[i].Select();
			}
			else
			{
				PlayerDescription[i].Selected = false;
			}
		}
	}

	public override void ProcessZ()
	{
		Point originalPosition = new Point(192, 398);
		switch (base.MenuItemList[base.MenuSelectIndex].Name)
		{
		case "FaceAya_ct":
			base.StageData.MenuGroupList.Add(new MenuGroup_WeaponSelect(base.StageData, StageSelect, 0));
			break;
		case "Plane":
			base.StageData.MenuGroupList.Add(new MenuGroup_WeaponSelect(base.StageData, StageSelect, 1));
			break;
		case "FaceReimu_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Reimu(base.StageData.StateSwitchData.SDPswitch, originalPosition);
			if (!StageSelect)
			{
				base.StageData.Rep.CreatRpy();
				base.StageData.StateSwitchData.NextState = "St1";
				base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			}
			else
			{
				base.StageData.MenuGroupList.Add(new MenuGroup_StageSelect(base.StageData));
			}
			break;
		case "FaceSanae_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Sanae(base.StageData.StateSwitchData.SDPswitch, originalPosition);
			if (!StageSelect)
			{
				base.StageData.Rep.CreatRpy();
				base.StageData.StateSwitchData.NextState = "St1";
				base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			}
			else
			{
				base.StageData.MenuGroupList.Add(new MenuGroup_StageSelect(base.StageData));
			}
			break;
		case "FaceMarisa_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Marisa(base.StageData.StateSwitchData.SDPswitch, originalPosition);
			if (!StageSelect)
			{
				base.StageData.Rep.CreatRpy();
				base.StageData.StateSwitchData.NextState = "St1";
				base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			}
			else
			{
				base.StageData.MenuGroupList.Add(new MenuGroup_StageSelect(base.StageData));
			}
			break;
		case "FaceKoishi_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Koishi(base.StageData.StateSwitchData.SDPswitch, originalPosition);
			if (!StageSelect)
			{
				base.StageData.Rep.CreatRpy();
				base.StageData.StateSwitchData.NextState = "St1";
				base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
			}
			else
			{
				base.StageData.MenuGroupList.Add(new MenuGroup_StageSelect(base.StageData));
			}
			break;
		}
	}

	public override void Show()
	{
		base.Show();
		if (base.StageData.MenuGroupList[base.StageData.MenuGroupList.Count - 1] == this)
		{
			foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.LegacyItems(PlayerDescription)){
				x.Show();
			};
		}
	}
}
