using System;
using System.Collections.Generic;
using System.Drawing;
using Shooting.Planes.Story;

namespace Shooting;

internal class MenuGroup_SCPractice : BaseMenuGroup
{
	private DescriptionBox DescriptionBox;

	private int deltaY = 20;

	public MenuGroup_SCPractice(StageDataPackage StageData)
		: base(StageData)
	{
		base.MenuSelectIndex = 0;
		base.MenuItemList = new List<BaseMenuItem>();
		List<SpellCardHistory> list = StageData.PData.SC_History.FindAll((SpellCardHistory a) => a.MyPlaneFullName == "ReimuA");
		for (int num = 0; num < list.Count; num++)
		{
			base.MenuItemList.Add(new SCMenuItem(StageData, list[num].Index.ToString().PadRight(3))
			{
				Description = (list[num].Rank.ToString().PadLeft(10) + list[num].Recorded.ToString().PadLeft(2) + "/" + list[num].History.ToString().PadLeft(2)).PadLeft(40),
				TxtureObject = StageData.TextureObjectDictionary["欧" + list[num].Name],
				DefaultColor = ((list[num].Recorded > 0) ? Color.SkyBlue : Color.White)
			});
		}
		int num2 = 100;
		int num3 = 100;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.TransparentValueF = 0f;
			menuItem.Position = new PointF(num2, 90f);
			menuItem.DestPoint = new PointF(num2, num3);
			menuItem.MaxVelocity = 10000f;
			num3 += deltaY;
		}
		base.MenuItemList[base.MenuSelectIndex].Selected = true;
		MenuTitlePos1 = new PointF(140f, -30f);
		MenuTitlePos2 = new PointF(140f, -150f);
		base.MenuTilte = new BaseMenuItem(StageData, "MenuTitle_MusicRoom")
		{
			Position = MenuTitlePos2
		};
		base.TransparentValueF = 0f;
		base.TransparentVelocity = 5f;
		base.TxtureObject = base.TextureObjectDictionary["MenuBackground"];
		base.Scale = 0.625f;
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		base.AngleDegree = 90.0;
		base.ColorValue = Color.SlateBlue;
		DescriptionBox = new DescriptionBox(StageData)
		{
			MaxTransparent = 0,
			OffsetX = 50
		};
	}

	public override void SelectItemChanged()
	{
		base.SelectItemChanged();
		int num = 100;
		int num2 = 100;
		int num3 = (base.MenuSelectIndex - 8) * deltaY;
		if (num3 < 0)
		{
			num3 = 0;
		}
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(num, num2 - num3);
			num2 += deltaY;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		DescriptionBox.Ctrl();
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.TransparentValueF = 400f - Math.Abs(260f - menuItem.Position.Y) * 2f;
		}
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
		DescriptionBox.TransparentVelocity = -15f;
		foreach (BaseMenuItem menuItem in base.MenuItemList)
		{
			menuItem.DestPoint = new PointF(100f, 60f);
			menuItem.OnRemove = true;
		}
		base.StageData.SoundPlay("se_cancel00.wav");
	}

	public override void Show()
	{
		base.Show();
		DescriptionBox.Show();
	}
}
