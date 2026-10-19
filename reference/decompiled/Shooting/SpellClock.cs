using System.Drawing;

namespace Shooting;

public class SpellClock : BaseObject
{
	private int timeRemaining;

	public int TimeRemaining
	{
		get
		{
			return timeRemaining;
		}
		set
		{
			if (value < 0)
			{
				timeRemaining = 0;
			}
			else if (value > 5940)
			{
				timeRemaining = 5940;
			}
			else
			{
				timeRemaining = value;
			}
		}
	}

	public SpellClock(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.OriginalPosition = new PointF(base.BoundRect.Left + 25, 40f);
	}

	public override void Ctrl()
	{
		if (base.Boss == null)
		{
			return;
		}
		base.Ctrl();
		TimeRemaining = base.Boss.SpellTime - base.Boss.Time;
		if (base.Boss.Life > 0)
		{
			if (TimeRemaining <= 300)
			{
				base.ColorValue = Color.FromArgb(255, 100, 100);
				if (0 < TimeRemaining && TimeRemaining % 60 == 0)
				{
					if (TimeRemaining <= 120)
					{
						base.StageData.SoundPlay("se_timeout2.wav");
					}
					else
					{
						base.StageData.SoundPlay("se_timeout.wav");
					}
				}
			}
			else if (TimeRemaining <= 600)
			{
				base.ColorValue = Color.FromArgb(255, 200, 200);
			}
			else
			{
				base.ColorValue = Color.White;
			}
		}
		if (base.MyPlane.OriginalPosition.Y < 30f && base.MyPlane.OriginalPosition.X < base.OriginalPosition.X + 60f && base.MyPlane.OriginalPosition.X > base.OriginalPosition.X - 60f)
		{
			base.TransparentValueF -= 10f;
			if (base.TransparentValueF < 50f)
			{
				base.TransparentValueF = 50f;
			}
		}
		else
		{
			base.TransparentValueF += 10f;
		}
	}

	public override void Show()
	{
		if (base.Boss == null || base.Boss.Life <= 0)
		{
			return;
		}
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["球球"], 1f, 0f, base.OriginalPosition, Color.FromArgb(base.TransparentValue, Color.White));
		string text = "tsnum";
		int num = 3;
		int num2 = 9;
		int num3 = 0;
		char[] array = ((float)TimeRemaining / 60f).ToString("F1").ToCharArray();
		for (int num4 = array.Length - 1; num4 >= 0; num4--)
		{
			if (array[num4] == '.')
			{
				num2 = 10;
				text = "tnum";
				num3 = 4;
			}
			PointF position = new PointF(base.OriginalPosition.X + 18f - (float)((22 - num2) * (array.Length - 1 - num4)) + (float)num3, base.OriginalPosition.Y + (float)num);
			base.SpriteMain.Draw2D(base.TextureObjectDictionary[text + array[num4]], base.Scale, 0f, position, Color.FromArgb(base.TransparentValue, base.ColorValue));
			if (array[num4] == '.')
			{
				num = 0;
				num2 = 3;
				num3 = 20;
			}
		}
	}
}
