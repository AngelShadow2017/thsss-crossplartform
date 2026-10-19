using System.Drawing;
using SlimDX.Direct3D9;

namespace Shooting;

internal class StoryBox : BaseStoryItem
{
	private string text;

	private Texture StringTexture;

	private System.Drawing.Font sysfont;

	private char[] TextArray;

	private string text2;

	private Texture StringTexture2;

	public Color FontColor2;

	public string DrawText2;

	private char[] TextArray2;

	private SlimDX.Direct3D9.Font DXfont { get; set; }

	public Color FontColor { get; set; }

	public int OffsetX { get; set; }

	public int OffsetY { get; set; }

	public bool Shadowed { get; set; }

	public string DrawText { get; set; }

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (value != text)
			{
				text = value;
				TextArray = text.ToCharArray();
				DrawText = "";
				base.Time = 0;
			}
		}
	}

	public string Text2
	{
		get
		{
			return text2;
		}
		set
		{
			if (value != text2)
			{
				text2 = value;
				TextArray2 = text2.ToCharArray();
				DrawText2 = "";
				base.Time = 0;
			}
		}
	}

	public StoryBox(StageDataPackage StageData)
		: base(StageData)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		base.TxtureObject = base.TextureObjectDictionary["对话框"];
		base.Position = new PointF(base.BoundRect.Left, base.BoundRect.Bottom - base.TxtureObject.Height * base.BoundRect.Width / base.TxtureObject.Width - 8);
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.TransparentValueF = 0f;
		base.TransparentVelocity = 10f;
		OffsetX = 20;
		OffsetY = 21;
		Shadowed = true;
		sysfont = new System.Drawing.Font(StageData.GlobalData.FontType, 16f, (FontStyle)1);
		DXfont = new SlimDX.Direct3D9.Font(StageData.DeviceMain, sysfont);
		FontColor = Color.White;
	}

	public override void Ctrl()
	{
		if (DrawText != null && DrawText.Replace("\r\n", "") == text)
		{
			base.TransparentValueF = base.MaxTransparent;
			return;
		}
		if (TextArray != null && base.Time < TextArray.Length)
		{
			if ((float)DXfont.MeasureString(base.SpriteMain.sprite, DrawText + TextArray[base.Time], DrawTextFormat.Left).Width / 1.15f > (float)(base.TxtureObject.Width - OffsetX))
			{
				DrawText += "\r\n";
			}
			DrawText += TextArray[base.Time];
		}
		if (StringTexture != null)
		{
			StringTexture.Dispose();
		}
		StringTexture = base.StageData.DrawString(DrawText, sysfont, Brushes.White, 512, 256);
		if (TextArray2 != null && base.Time < TextArray2.Length)
		{
			if ((float)DXfont.MeasureString(base.SpriteMain.sprite, DrawText2 + TextArray2[base.Time], DrawTextFormat.Left).Width / 1.15f > (float)(base.TxtureObject.Width - OffsetX))
			{
				DrawText2 += "\r\n";
			}
			DrawText2 += TextArray2[base.Time];
		}
		if (StringTexture2 != null)
		{
			StringTexture2.Dispose();
		}
		StringTexture2 = base.StageData.DrawString(DrawText2, sysfont, Brushes.White, 512, 256);
		base.Ctrl();
	}

	public void DrawFullText()
	{
		if (TextArray != null)
		{
			while (base.Time < TextArray.Length)
			{
				if ((float)DXfont.MeasureString(base.SpriteMain.sprite, DrawText + TextArray[base.Time], DrawTextFormat.Left).Width / 1.15f > (float)(base.TxtureObject.Width - OffsetX))
				{
					DrawText += "\r\n";
				}
				DrawText += TextArray[base.Time];
				base.Time++;
			}
		}
		if (StringTexture != null)
		{
			StringTexture.Dispose();
		}
		StringTexture = base.StageData.DrawString(DrawText, sysfont, Brushes.White, 512, 256);
		if (TextArray2 != null)
		{
			while (base.Time < TextArray2.Length)
			{
				if ((float)DXfont.MeasureString(base.SpriteMain.sprite, DrawText2 + TextArray2[base.Time], DrawTextFormat.Left).Width / 1.15f > (float)(base.TxtureObject.Width - OffsetX))
				{
					DrawText2 += "\r\n";
				}
				DrawText2 += TextArray2[base.Time];
				base.Time++;
			}
		}
		if (StringTexture2 != null)
		{
			StringTexture2.Dispose();
		}
		StringTexture2 = base.StageData.DrawString(DrawText2, sysfont, Brushes.White, 512, 256);
	}

	public void SetFont(float FontSize)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		if (sysfont != null)
		{
			FontSize = FontSize * 96f / (float)Dpi.PixelsPerXLogicalInch;
			if (sysfont.Size != FontSize)
			{
				sysfont = new System.Drawing.Font(base.StageData.GlobalData.FontType, FontSize, (FontStyle)1);
				DXfont = new SlimDX.Direct3D9.Font(base.StageData.DeviceMain, sysfont);
			}
		}
	}

	public override void Show()
	{
		SizeF destinationSize = new SizeF(base.TxtureObject.Width, 116f);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, base.TxtureObject.LeftTop, 0f, base.Position, Color.FromArgb(base.TransparentValue, Color.White));
		if (Text != null && StringTexture != null)
		{
			if (Shadowed)
			{
				base.SpriteMain.Draw2D(StringTexture, new Rectangle(0, 0, 512, 256), new SizeF(512f, 256f), new PointF((int)base.Position.X + 2 + OffsetX, (int)base.Position.Y + 2 + (int)((float)OffsetY * destinationSize.Height / 90f)), Color.FromArgb(150, 0, 0, 0));
			}
			base.SpriteMain.Draw2D(StringTexture, new Rectangle(0, 0, 512, 256), new SizeF(512f, 256f), new PointF((int)base.Position.X + OffsetX, (int)base.Position.Y + (int)((float)OffsetY * destinationSize.Height / 90f)), FontColor);
			if (StringTexture2 != null)
			{
				base.SpriteMain.Draw2D(StringTexture2, new Rectangle(0, 0, 512, 256), new SizeF(512f, 256f), new PointF((int)base.Position.X + OffsetX, (int)base.Position.Y + (int)((float)OffsetY * destinationSize.Height / 90f)), FontColor2);
			}
		}
	}

	public override void Dispose()
	{
		DXfont.Dispose();
		if (StringTexture != null)
		{
			StringTexture.Dispose();
		}
	}
}
