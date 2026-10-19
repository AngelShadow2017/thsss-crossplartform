using System;
using System.Drawing;

namespace Shooting;

public class BaseMyPlane_Touhou : BaseMyPlane
{
	public float IndexX { get; set; }

	public int IndexY { get; set; }

	public BaseMyPlane_Touhou(StageDataPackage StageData, Point OriginalPosition, string Name)
	{
		base.StageData = StageData;
		base.OriginalPosition = OriginalPosition;
		base.Name = Name;
		base.Region = 1;
		base.Time = base.UnmatchedTime;
		base.DeadTime = -1;
		base.SubPlanePoint = new PointF[4];
		string key = Name + IndexY + (int)IndexX;
		base.TxtureObject = base.TextureObjectDictionary[key];
		base.SpellEnabled = true;
		base.HighItemScore = 10000;
	}

	public override void TextureCtrl()
	{
		int indexY = IndexY;
		if (base.Vx > 0.5f)
		{
			IndexY = 2;
			if (IndexX < 4f)
			{
				IndexX += 0.5f;
			}
			else
			{
				IndexX = 4 + base.Time % 24 / 6;
			}
		}
		else if (base.Vx < -0.5f)
		{
			IndexY = 1;
			if (IndexX < 4f)
			{
				IndexX += 0.5f;
			}
			else
			{
				IndexX = 4 + base.Time % 24 / 6;
			}
		}
		else
		{
			IndexY = 0;
			IndexX = base.Time % 48 / 6;
		}
		if (indexY != IndexY)
		{
			IndexX = 1f;
		}
		if (IndexX > 7f)
		{
			IndexX = 7f;
		}
		string key = base.Name + IndexY + (int)IndexX;
		base.TxtureObject = base.TextureObjectDictionary[key];
	}

	public override void ShowCenter()
	{
		if (base.SlowMove)
		{
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["SlowEffect01"], 1f, (0f - (float)base.Direction) * 4f, new PointF(base.Position.X, base.Position.Y), 120);
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["SlowEffect01"], 1f, (float)base.Direction * 4f, new PointF(base.Position.X, base.Position.Y), 120);
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["Center"], 1f, (float)base.Direction * 4f, new PointF(base.Position.X, base.Position.Y), Color.White);
		}
	}

	public override void Miss()
	{
		double num = -2.2707963148739676;
		for (int i = 0; i < 8; i++)
		{
			PointF originalPosition = new PointF((float)((double)base.OriginalPosition.X + 40.0 * Math.Cos(num)), (float)((double)base.OriginalPosition.Y + 40.0 * Math.Sin(num)));
			PowerItem_Touhou powerItem_Touhou = new PowerItem_Touhou(base.StageData, originalPosition);
			powerItem_Touhou.Direction = num;
			powerItem_Touhou.Velocity = 5f;
			num += 0.1999999965940203;
		}
		base.Miss();
	}
}
