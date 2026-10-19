using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Shooting;

internal class MenuGroup_ReplayStageSelect : BaseMenuGroup
{
	private const string NullDescription = "----------";

	private string RepIndex;

	public MenuGroup_ReplayStageSelect(StageDataPackage StageData, PointF OriginalPosition, DescriptionMenuItem RepMenuItem)
		: base(StageData)
	{
		RepIndex = RepMenuItem.Name;
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>();
		for (int i = 0; i < 6; i++)
		{
			base.MenuItemList.Add(new DescriptionMenuItem(StageData, $"Stage{i + 1:00}")
			{
				Description = "----------"
			});
		}
		base.MenuItemList.Add(new DescriptionMenuItem(StageData, "StageEx")
		{
			Description = "----------"
		});
		float x = OriginalPosition.X + 12f;
		float num = OriginalPosition.Y + 12f;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.Position = new PointF(x, OriginalPosition.Y + 10f);
			menuItem.DestPoint = new PointF(x, num);
			num += 17f;
		}
		string text = ".\\Replay\\thSSS_" + RepIndex + ".rpy";
		if (File.Exists(text))
		{
			ReplayInfo replayInfo = Replay.ReadTitle(text);
			if (replayInfo.StartStage == "StEx")
			{
				((DescriptionMenuItem)base.MenuItemList[6]).Description = replayInfo.MyPlaneData[1].Score.ToString().PadLeft(10);
			}
			else
			{
				int num2 = ((!replayInfo.StartStage.Contains("St")) ? Convert.ToInt32(replayInfo.StartStage.Replace("Bs", "")) : Convert.ToInt32(replayInfo.StartStage.Replace("St", "")));
				for (int i = 0; i < replayInfo.MyPlaneData.Count - 1; i++)
				{
					if (num2 - 1 + i < 6)
					{
						((DescriptionMenuItem)base.MenuItemList[num2 - 1 + i]).Description = replayInfo.MyPlaneData[i + 1].Score.ToString().PadLeft(10);
					}
				}
			}
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		base.MenuTilte = new DescriptionMenuItem(StageData, RepIndex)
		{
			Description = RepMenuItem.Description,
			OriginalPosition = RepMenuItem.OriginalPosition,
			Selected = true
		};
		MenuTitlePos1 = new PointF(RepMenuItem.OriginalPosition.X, 16f);
		MenuTitlePos2 = RepMenuItem.OriginalPosition;
		base.TxtureObject = base.TextureObjectDictionary["MenuBackground"];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		base.AngleDegree = 90.0;
		base.ColorValue = Color.SkyBlue;
	}

	public override void ProcessKeys()
	{
		base.ProcessKeys();
		if (base.KClass.Key_Z && LastZ == 0)
		{
			base.MenuItemList[base.MenuSelectIndex].Click();
			OnChangeMenu = base.TimeMain + 1;
		}
		if ((!base.KClass.Key_X && !base.KClass.Key_ESC) || LastX != 0)
		{
			return;
		}
		OnRemoveMenu = base.TimeMain + 20;
		base.TransparentVelocity = -15f;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(base.MenuItemList[0].OriginalPosition.X, base.MenuItemList[0].OriginalPosition.Y);
			menuItem.OnRemove = true;
		}
		base.StageData.SoundPlay("se_cancel00.wav");
	}

	public override void ProcessZ()
	{
		if (((DescriptionMenuItem)base.MenuItemList[base.MenuSelectIndex]).Description != "----------")
		{
			base.StageData.SoundPlay("se_ok00.wav");
			string fileName = ".\\Replay\\thSSS_" + RepIndex + ".rpy";
			base.StageData.Rep.LoadRpy(fileName);
			ReplayInfo replayInfo = Replay.ReadTitle(fileName);
			base.StageData.RepInfo = replayInfo;
			string text = ((base.MenuSelectIndex >= 6) ? "StEx" : ((!replayInfo.StartStage.Contains("St")) ? ("Bs" + (base.MenuSelectIndex + 1)) : ("St" + (base.MenuSelectIndex + 1))));
			base.StageData.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = text,
				NeedInit = true,
				SDPswitch = new StageDataPackage(base.StageData.GlobalData)
				{
					OnReplay = true,
					Difficulty = replayInfo.Rank
				}
			};
			int num = ((!(text == "StEx")) ? ((!replayInfo.StartStage.Contains("St")) ? (base.MenuSelectIndex - Convert.ToInt32(replayInfo.StartStage.Replace("Bs", "")) + 1) : (base.MenuSelectIndex - Convert.ToInt32(replayInfo.StartStage.Replace("St", "")) + 1)) : 0);
			Point point = new Point(192, 398);
			if (num >= 0)
			{
				point = new Point((int)replayInfo.MyPlaneData[num].PosX, (int)replayInfo.MyPlaneData[num].PosY);
			}
			BaseMyPlane baseMyPlane = replayInfo.MyPlaneName switch
			{
				"Aya" => replayInfo.WeaponType switch
				{
					"A" => new MyPlane_Aya(base.StageData.StateSwitchData.SDPswitch, point), 
					"B" => new MyPlane_AyaB(base.StageData.StateSwitchData.SDPswitch, point), 
					_ => new BaseMyPlane(base.StageData.StateSwitchData.SDPswitch, point), 
				}, 
				"Plane" => replayInfo.WeaponType switch
				{
					"A" => new BaseMyPlane(base.StageData.StateSwitchData.SDPswitch, point), 
					"B" => new MyPlane_PlaneB(base.StageData.StateSwitchData.SDPswitch, point), 
					_ => new BaseMyPlane(base.StageData.StateSwitchData.SDPswitch, point), 
				}, 
				"Reimu" => new MyPlane_Reimu(base.StageData.StateSwitchData.SDPswitch, point), 
				"Sanae" => new MyPlane_Sanae(base.StageData.StateSwitchData.SDPswitch, point), 
				"Marisa" => new MyPlane_Marisa(base.StageData.StateSwitchData.SDPswitch, point), 
				"Koishi" => new MyPlane_Koishi(base.StageData.StateSwitchData.SDPswitch, point), 
				_ => new BaseMyPlane(base.StageData.StateSwitchData.SDPswitch, point), 
			};
			if (num >= 0)
			{
				baseMyPlane.Life = replayInfo.MyPlaneData[num].Life;
				baseMyPlane.Spell = replayInfo.MyPlaneData[num].Spell;
				baseMyPlane.Power = replayInfo.MyPlaneData[num].Power;
				baseMyPlane.Score = replayInfo.MyPlaneData[num].Score;
				baseMyPlane.OriginalPosition = new PointF(replayInfo.MyPlaneData[num].PosX, replayInfo.MyPlaneData[num].PosY);
				baseMyPlane.Graze = replayInfo.MyPlaneData[num].Graze;
				baseMyPlane.LifeUpCount = replayInfo.MyPlaneData[num].LifeUpCount;
				baseMyPlane.LifeChip = replayInfo.MyPlaneData[num].LifeChip;
				baseMyPlane.SpellChip = replayInfo.MyPlaneData[num].SpellChip;
				baseMyPlane.StarPoint = replayInfo.MyPlaneData[num].StarPoint;
				baseMyPlane.HighItemScore = replayInfo.MyPlaneData[num].HighItemScore;
				baseMyPlane.Rate = replayInfo.MyPlaneData[num].Rate;
				baseMyPlane.LastColor = replayInfo.MyPlaneData[num].LastColor;
				base.StageData.Rep.DataPosition = replayInfo.MyPlaneData[num].DataPosition;
			}
			base.StageData.StateSwitchData.SDPswitch.MyPlane = baseMyPlane;
			base.StageData.StateSwitchData.SDPswitch.RepInfo = replayInfo;
		}
	}
}
