using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class MenuGroup_PlayerData : BaseMenuGroup
{
	private List<BaseMenuItem> MenuItemListPlane;

	private int MenuPlaneSelectIndex;

	private List<DescriptionMenuItem> ScorePageList;

	private int ScorePageIndex = 0;

	private List<BaseMenuItem> ClearHistoryList;

	public MenuGroup_PlayerData(StageDataPackage StageData)
		: base(StageData)
	{
		base.TransparentValueF = 0f;
		base.MaxTransparent = 255;
		base.TransparentVelocity = 5f;
		base.TxtureObject = base.TextureObjectDictionary["MenuBackground"];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		base.AngleDegree = 90.0;
		base.ColorValue = Color.FromArgb(30, 110, 150);
		MenuTitlePos1 = new PointF(140f, -30f);
		MenuTitlePos2 = new PointF(140f, -150f);
		base.MenuTilte = new BaseMenuItem(StageData, "MenuTitle_PlayerData")
		{
			Position = MenuTitlePos2
		};
		ClearHistoryList = new List<BaseMenuItem>();
		MenuPlaneSelectIndex = 0;
		MenuItemListPlane = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "History_ReimuA"),
			new BaseMenuItem(StageData, "History_MarisaA"),
			new BaseMenuItem(StageData, "History_SanaeA"),
			new BaseMenuItem(StageData, "History_KoishiA")
		};
		int num = 90;
		int num2 = 90;
		int num3 = 0;
		foreach (BaseMenuItem item in MenuItemListPlane)
		{
			item.DestPoint = new PointF(num, num2);
			item.Position = new PointF(num + num3, num2);
			item.UnSelectVisible = false;
			item.Vibrateable = false;
		}
		MenuItemListPlane[base.MenuSelectIndex].Selected = true;
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>
		{
			new BaseMenuItem(StageData, "History_Easy"),
			new BaseMenuItem(StageData, "History_Normal"),
			new BaseMenuItem(StageData, "History_Hard"),
			new BaseMenuItem(StageData, "History_Lunatic"),
			new BaseMenuItem(StageData, "History_Extra")
		};
		num = 480;
		num2 = 90;
		num3 = 0;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(num, num2);
			menuItem.Position = new PointF(num + num3, num2);
			menuItem.UnSelectVisible = false;
			menuItem.Vibrateable = false;
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		ScorePageList = new List<DescriptionMenuItem>();
		AddNewScorePage();
	}

	public override void Ctrl()
	{
		base.Ctrl();
		MenuItemListPlane.ForEach(delegate(BaseMenuItem x)
		{
			x.Ctrl();
		});
		ScorePageList.ForEach(delegate(DescriptionMenuItem x)
		{
			x.Ctrl();
		});
		ClearHistoryList.ForEach(delegate(BaseMenuItem x)
		{
			x.Ctrl();
		});
	}

	public override void ProcessKeys()
	{
		base.ProcessKeys();
		if (base.KClass.Key_Z && LastZ == 0)
		{
			ScorePageIndex++;
			int num = ((base.MenuSelectIndex == 2 || base.MenuSelectIndex == 3) ? 5 : 4);
			if (ScorePageIndex > num)
			{
				ScorePageIndex = 0;
			}
			AddNewScorePage();
			base.StageData.SoundPlay("se_ok00.wav");
		}
		if ((base.KClass.Key_X || base.KClass.Key_ESC) && LastX == 0)
		{
			OnRemoveMenu = base.TimeMain + 20;
			base.TransparentVelocity = -15f;
			foreach (BaseMenuItem menuItem in base.MenuItemList)
			{
				menuItem.OnRemove = true;
			}
			foreach (BaseMenuItem item in MenuItemListPlane)
			{
				item.OnRemove = true;
			}
			foreach (DescriptionMenuItem scorePage in ScorePageList)
			{
				scorePage.OnRemove = true;
			}
			foreach (BaseMenuItem clearHistory in ClearHistoryList)
			{
				clearHistory.OnRemove = true;
			}
			base.StageData.SoundPlay("se_cancel00.wav");
		}
		if (base.KClass.ArrowRight && (LastRight == 0 || LastRight > 30))
		{
			MenuPlaneSelectIndex++;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
		if (base.KClass.ArrowLeft && (LastLeft == 0 || LastLeft > 30))
		{
			MenuPlaneSelectIndex--;
			base.StageData.SoundPlay("se_select00.wav");
			SelectItemChanged();
		}
	}

	public override void SelectItemChanged()
	{
		base.SelectItemChanged();
		if (MenuPlaneSelectIndex < 0)
		{
			MenuPlaneSelectIndex += MenuItemListPlane.Count;
		}
		else if (MenuPlaneSelectIndex > MenuItemListPlane.Count - 1)
		{
			MenuPlaneSelectIndex -= MenuItemListPlane.Count;
		}
		MenuItemListPlane.ForEach(delegate(BaseMenuItem x)
		{
			x.Selected = false;
		});
		MenuItemListPlane[MenuPlaneSelectIndex].Select();
		ScorePageIndex = 0;
		AddNewScorePage();
	}

	private void AddNewScorePage()
	{
		ScorePageList.Clear();
		List<ScoreHistory> list = base.StageData.PData.S_History.FindAll(FindScore);
		List<SpellCardHistory> list2 = base.StageData.PData.SC_History.FindAll(FindSC);
		List<ClearHistory> list3 = base.StageData.PData.C_History.FindAll(FindClear);
		switch (ScorePageIndex)
		{
		case 0:
		{
			for (int i = 0; i < list.Count; i++)
			{
				ScorePageList.Add(new DescriptionMenuItem(base.StageData, (i + 1).ToString().PadRight(2))
				{
					Description = list[i].Data2DrawString(),
					DefaultColor = Color.FromArgb(230 - 15 * i, 230 - 12 * i, 255)
				});
			}
			int num = 50;
			int num2 = 130;
			int num3 = 0;
			foreach (DescriptionMenuItem scorePage in ScorePageList)
			{
				scorePage.DestPoint = new PointF(num, num2);
				scorePage.Position = new PointF(num + num3, 100f);
				num2 += 20;
			}
			break;
		}
		case 1:
		{
			for (int i = 0; i < 10; i++)
			{
				if (list2[i].History > 0)
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["欧" + list2[i].Name],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
				else
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["???"],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
			}
			int num = 80;
			int num2 = 130;
			int num3 = 0;
			foreach (DescriptionMenuItem scorePage2 in ScorePageList)
			{
				scorePage2.DestPoint = new PointF(num, num2);
				scorePage2.Position = new PointF(num + num3, 100f);
				num2 += 20;
			}
			break;
		}
		case 2:
		{
			for (int i = 10; i < 20; i++)
			{
				if (list2[i].History > 0)
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["欧" + list2[i].Name],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
				else
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["???"],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
			}
			int num = 80;
			int num2 = 130;
			int num3 = 0;
			foreach (DescriptionMenuItem scorePage3 in ScorePageList)
			{
				scorePage3.DestPoint = new PointF(num, num2);
				scorePage3.Position = new PointF(num + num3, 100f);
				num2 += 20;
			}
			break;
		}
		case 3:
		{
			for (int i = 20; i < Math.Min(list2.Count, 30); i++)
			{
				if (list2[i].History > 0)
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["欧" + list2[i].Name],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
				else
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["???"],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
			}
			int num = 80;
			int num2 = 130;
			int num3 = 0;
			foreach (DescriptionMenuItem scorePage4 in ScorePageList)
			{
				scorePage4.DestPoint = new PointF(num, num2);
				scorePage4.Position = new PointF(num + num3, 100f);
				num2 += 20;
			}
			break;
		}
		case 4:
		{
			for (int i = 30; i < Math.Min(list2.Count, 40); i++)
			{
				if (list2[i].History > 0)
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["欧" + list2[i].Name],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
				else
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["???"],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
			}
			int num = 80;
			int num2 = 130;
			int num3 = 0;
			foreach (DescriptionMenuItem scorePage5 in ScorePageList)
			{
				scorePage5.DestPoint = new PointF(num, num2);
				scorePage5.Position = new PointF(num + num3, 100f);
				num2 += 20;
			}
			break;
		}
		case 5:
		{
			for (int i = 40; i < Math.Min(list2.Count, 50); i++)
			{
				if (list2[i].History > 0)
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["欧" + list2[i].Name],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
				else
				{
					ScorePageList.Add(new SCMenuItem(base.StageData, list2[i].Index.ToString().PadRight(3))
					{
						Description = (list2[i].Recorded.ToString().PadLeft(2) + "/" + list2[i].History.ToString().PadLeft(2)).PadLeft(40),
						TxtureObject = base.StageData.TextureObjectDictionary["???"],
						DefaultColor = ((list2[i].Recorded > 0) ? Color.SkyBlue : Color.White)
					});
				}
			}
			int num = 80;
			int num2 = 130;
			int num3 = 0;
			foreach (DescriptionMenuItem scorePage6 in ScorePageList)
			{
				scorePage6.DestPoint = new PointF(num, num2);
				scorePage6.Position = new PointF(num + num3, 100f);
				num2 += 20;
			}
			break;
		}
		}
		int num4 = 215;
		int num5 = 350;
		ClearHistoryList = new List<BaseMenuItem>();
		ClearHistoryList.Add(new SCMenuItem(base.StageData, "")
		{
			OriginalPosition = new PointF(num4, num5),
			DestPoint = new PointF(num4, num5),
			Scale = 1f,
			OffX = 0,
			OffY = -5,
			TxtureObject = base.StageData.TextureObjectDictionary["History_游戏次数"],
			Description = list3[0].StartTimes.ToString().PadLeft(18, ' '),
			DefaultColor = Color.FromArgb(230, 230, 255)
		});
		num5 += 24;
		long playingTime = list3[0].PlayingTime;
		int num6 = (int)(playingTime / 216000);
		int num7 = (int)(playingTime % 216000 / 3600);
		int num8 = (int)(playingTime % 3600 / 60);
		string text = num6.ToString("d2") + ":" + num7.ToString("d2") + ":" + num8.ToString("d2");
		ClearHistoryList.Add(new SCMenuItem(base.StageData, "")
		{
			OriginalPosition = new PointF(num4, num5),
			DestPoint = new PointF(num4, num5),
			Scale = 1f,
			OffX = 0,
			OffY = -5,
			TxtureObject = base.StageData.TextureObjectDictionary["History_游戏时间"],
			Description = text.PadLeft(18, ' '),
			DefaultColor = Color.FromArgb(230, 230, 255)
		});
		num5 += 24;
		ClearHistoryList.Add(new SCMenuItem(base.StageData, "")
		{
			OriginalPosition = new PointF(num4, num5),
			DestPoint = new PointF(num4, num5),
			Scale = 1f,
			OffX = 0,
			OffY = -5,
			TxtureObject = base.StageData.TextureObjectDictionary["History_通关次数"],
			Description = list3[0].ClearTimes.ToString().PadLeft(18, ' '),
			DefaultColor = Color.FromArgb(230, 230, 255)
		});
		num5 += 24;
		ClearHistoryList.Add(new SCMenuItem(base.StageData, "")
		{
			OriginalPosition = new PointF(num4, num5),
			DestPoint = new PointF(num4, num5),
			Scale = 1f,
			OffX = 0,
			OffY = -5,
			TxtureObject = base.StageData.TextureObjectDictionary["History_无续关通关次数"],
			Description = list3[0].NoContinueClearTimes.ToString().PadLeft(18, ' '),
			DefaultColor = Color.FromArgb(230, 230, 255)
		});
		num5 += 24;
		num4 -= 70;
		ClearHistoryList.Add(new BaseMenuItem(base.StageData, "History_切换")
		{
			OriginalPosition = new PointF(num4, num5),
			DestPoint = new PointF(num4, num5),
			Scale = 1f,
			Selected = true
		});
		ClearHistoryList.Add(new BaseMenuItem(base.StageData, "History_Arrow")
		{
			OriginalPosition = new PointF(70f, 95f),
			DestPoint = new PointF(70f, 95f),
			Selected = true
		});
		ClearHistoryList.Add(new BaseMenuItem(base.StageData, "History_Arrow")
		{
			OriginalPosition = new PointF(322f, 112f),
			DestPoint = new PointF(322f, 112f),
			Selected = true,
			AngleDegree = 180.0
		});
		ClearHistoryList.Add(new BaseMenuItem(base.StageData, "History_Arrow")
		{
			OriginalPosition = new PointF(480f, 89f),
			DestPoint = new PointF(480f, 89f),
			Selected = true,
			AngleDegree = 90.0
		});
		ClearHistoryList.Add(new BaseMenuItem(base.StageData, "History_Arrow")
		{
			OriginalPosition = new PointF(464f, 119f),
			DestPoint = new PointF(464f, 119f),
			Selected = true,
			AngleDegree = -90.0
		});
	}

	public override void Show()
	{
		base.Show();
		MenuItemListPlane.ForEach(delegate(BaseMenuItem x)
		{
			x.Show();
		});
		ScorePageList.ForEach(delegate(DescriptionMenuItem x)
		{
			x.Show();
		});
		ClearHistoryList.ForEach(delegate(BaseMenuItem x)
		{
			x.Show();
		});
	}

	public bool FindScore(ScoreHistory SH)
	{
		return SH.Rank.ToString() == base.MenuItemList[base.MenuSelectIndex].Name.Replace("History_", string.Empty) && SH.MyPlaneFullName == MenuItemListPlane[MenuPlaneSelectIndex].Name.Replace("History_", string.Empty);
	}

	public bool FindClear(ClearHistory CH)
	{
		return CH.Rank.ToString() == base.MenuItemList[base.MenuSelectIndex].Name.Replace("History_", string.Empty) && CH.MyPlaneFullName == MenuItemListPlane[MenuPlaneSelectIndex].Name.Replace("History_", string.Empty);
	}

	public bool FindSC(SpellCardHistory SCH)
	{
		return SCH.Rank.ToString() == base.MenuItemList[base.MenuSelectIndex].Name.Replace("History_", string.Empty) && SCH.MyPlaneFullName == MenuItemListPlane[MenuPlaneSelectIndex].Name.Replace("History_", string.Empty);
	}
}
