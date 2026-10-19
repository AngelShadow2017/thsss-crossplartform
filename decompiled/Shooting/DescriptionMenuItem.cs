using System;
using System.Drawing;

namespace Shooting;

public class DescriptionMenuItem : BaseMenuItem
{
	public string Description { get; set; }

	public Color DefaultColor { get; set; }

	public DescriptionMenuItem(StageDataPackage StageData, string Name)
	{
		base.StageData = StageData;
		base.Name = Name;
		base.MaxVelocity = 100f;
		base.Selected = false;
		base.UnSelectVisible = true;
		DefaultColor = Color.Gray;
	}

	public override void Ctrl()
	{
		if (!base.Enabled)
		{
			return;
		}
		base.Time++;
		Move();
		base.Velocity += base.Accelerate;
		base.TransparentValueF += base.TransparentVelocity;
		if (base.Selected)
		{
			double num = 1.0 + Math.Sin((float)base.Time / 6f);
			base.ColorValue = Color.Yellow;
			base.TransparentVelocity = 25f;
		}
		else
		{
			base.ColorValue = DefaultColor;
			if (!base.UnSelectVisible)
			{
				base.TransparentVelocity = -25f;
			}
		}
		if (base.OnRemove)
		{
			base.MaxTransparent -= 15;
		}
	}

	public override void Select()
	{
		base.Time = 1;
		base.Selected = true;
		base.VibrateTime = 0;
		base.TwinkleTime = 0;
	}

	public override void Click()
	{
		base.Time = 1;
		base.VibrateTime = 0;
		base.TwinkleTime = base.Time + 30;
	}

	public void DrawText(string text, PointF Pos)
	{
		text = text.Replace("\t", "    ");
		text = text.Replace("\r\n", string.Empty);
		int num = 0;
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			PointF position = new PointF(Pos.X + 5f + (float)num, Pos.Y + 9f);
			if (base.TextureObjectDictionary.ContainsKey(array[i].ToString()))
			{
				base.SpriteMain.Draw2D(base.TextureObjectDictionary[array[i].ToString()], 1f, 0f, position, Color.FromArgb(base.TransparentValue, base.ColorValue));
			}
			num += 11;
			if (array[i] == '.')
			{
				num -= 6;
			}
		}
	}

	public override void Show()
	{
		DrawText(base.Name + " " + Description, base.OriginalPosition);
	}
}
