using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_PlayerSelectEx : MenuGroup_PlayerSelect
{
	public MenuGroup_PlayerSelectEx(StageDataPackage StageData)
		: base(StageData, StageSelect: false)
	{
		base.MenuItemList = new List<BaseMenuItem>();
		PlayerDescription = new List<BaseMenuItem>();
		List<ClearHistory> list = StageData.PData.C_History.FindAll((ClearHistory x) => x.NoContinueClearTimes > 0 && x.MyPlaneFullName == "ReimuA");
		if (list.Count > 0)
		{
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
		}
		list = StageData.PData.C_History.FindAll((ClearHistory x) => x.ClearTimes > 0);
		list = StageData.PData.C_History.FindAll((ClearHistory x) => x.NoContinueClearTimes > 0 && x.MyPlaneFullName == "MarisaA");
		if (list.Count > 0)
		{
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
		}
		list = StageData.PData.C_History.FindAll((ClearHistory x) => x.ClearTimes > 0);
		list = StageData.PData.C_History.FindAll((ClearHistory x) => x.NoContinueClearTimes > 0 && x.MyPlaneFullName == "SanaeA");
		if (list.Count > 0)
		{
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
		}
		list = StageData.PData.C_History.FindAll((ClearHistory x) => x.ClearTimes > 0);
		list = StageData.PData.C_History.FindAll((ClearHistory x) => x.NoContinueClearTimes > 0 && x.MyPlaneFullName == "KoishiA");
		if (list.Count > 0)
		{
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
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		PlayerDescription[base.MenuSelectIndex].Selected = true;
	}

	public override void ProcessZ()
	{
		Point point = new Point(192, 398);
		switch (base.MenuItemList[base.MenuSelectIndex].Name)
		{
		case "Plane":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new BaseMyPlane(base.StageData.StateSwitchData.SDPswitch, point);
			break;
		case "FaceReimu_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Reimu(base.StageData.StateSwitchData.SDPswitch, point);
			break;
		case "FaceSanae_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Sanae(base.StageData.StateSwitchData.SDPswitch, point);
			break;
		case "FaceMarisa_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Marisa(base.StageData.StateSwitchData.SDPswitch, point);
			break;
		case "FaceKoishi_me":
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new MyPlane_Koishi(base.StageData.StateSwitchData.SDPswitch, point);
			break;
		default:
			base.StageData.StateSwitchData.SDPswitch.MyPlane = new BaseMyPlane(base.StageData.StateSwitchData.SDPswitch, point);
			break;
		}
		base.StageData.Rep.CreatRpy();
		base.StageData.StateSwitchData.NextState = "StEx";
		base.StageData.StateSwitchData.SDPswitch.SetReplayInfo(base.StageData.StateSwitchData.NextState);
	}

	public override void Show()
	{
		base.Show();
		if (base.StageData.MenuGroupList[base.StageData.MenuGroupList.Count - 1] == this)
		{
			foreach (BaseMenuItem x in SlimDX.Compat.ListCompat.Snapshot(PlayerDescription)){
				x.Show();
			};
		}
	}
}
