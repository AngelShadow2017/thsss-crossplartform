using System.Drawing;

namespace Shooting;

public class Item_Line : BaseInterface
{
	private int vl = 0;

	public int Delta = 13;

	public int Value
	{
		get
		{
			return vl;
		}
		set
		{
			if (value < 0)
			{
				vl = 0;
			}
			else if (value > MaxValue)
			{
				vl = MaxValue;
			}
			else
			{
				vl = value;
			}
		}
	}

	public int MaxValue { get; set; }

	public string Text { get; set; }

	public Color TextColorValue { get; set; }

	public string FontType { get; set; }

	public Item_Line(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, "ItemLine", OriginalPosition)
	{
		MaxValue = 1;
		base.TransparentValueF = 0f;
	}

	public virtual void DrawText(Point Pos, float Scale, Color color)
	{
		int num = 0;
		char[] array = Text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '.' || array[i] == ',')
			{
				num += (int)(8f * Scale);
			}
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '.' || array[i] == ',')
			{
				num -= (int)(5f * Scale);
			}
			PointF position = new PointF(Pos.X + 5 + num, Pos.Y + 6);
			if (array[i].ToString() != " ")
			{
				base.SpriteMain.Draw2D(base.TextureObjectDictionary[FontType + array[i]], Scale, 0f, position, Color.FromArgb(base.TransparentValue, color));
			}
			num += (int)((float)Delta * Scale);
			if (array[i] == '.' || array[i] == ',')
			{
				num -= (int)(3f * Scale);
			}
		}
	}

	public override void Show()
	{
		if (MaxValue != 0)
		{
			int num = base.TxtureObject.Width * Value / MaxValue;
			if (num > 0)
			{
				Size size = new Size(num, base.TxtureObject.Height);
				Rectangle rectangle = new Rectangle(new Point(0, 0), size);
				base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, size, base.TxtureObject.LeftTop, 0f, base.OriginalPosition, Color.FromArgb(base.TransparentValue * 7 / 10, base.ColorValue));
			}
		}
		Point pos = new Point((int)base.OriginalPosition.X, (int)base.OriginalPosition.Y + 3);
		DrawText(pos, 1f, base.ColorValue);
	}
}
