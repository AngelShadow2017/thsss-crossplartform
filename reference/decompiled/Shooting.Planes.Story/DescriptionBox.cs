using System.Drawing;
using SlimDX.Direct3D9;

namespace Shooting.Planes.Story;

internal class DescriptionBox : BaseStoryItem
{
	private Texture StringTexture;

	private Font sysfont;

	private Font DXfont { get; set; }

	public Color FontColor { get; set; }

	public string Text { get; set; }

	public int OffsetX { get; set; }

	public int OffsetY { get; set; }

	public bool Shadowed { get; set; }

	public DescriptionBox(StageDataPackage StageData)
		: base(StageData)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		base.TxtureObject = base.TextureObjectDictionary["对话框"];
		base.Position = new PointF(base.BoundRect.Left, base.BoundRect.Bottom - base.TxtureObject.Height * base.BoundRect.Width / base.TxtureObject.Width);
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.TransparentValueF = 0f;
		base.MaxTransparent = 200;
		base.TransparentVelocity = 10f;
		OffsetX = 20;
		OffsetY = 20;
		sysfont = new Font(StageData.GlobalData.FontType, 16f, (FontStyle)1);
		DXfont = new Font(StageData.DeviceMain, sysfont);
		FontColor = Color.White;
	}

	public override void Ctrl()
	{
		if (StringTexture != null)
		{
			StringTexture.Dispose();
		}
		StringTexture = base.StageData.DrawString(Text, sysfont, Brushes.White, 1024, 256);
		base.Ctrl();
	}

	public override void Show()
	{
		SizeF destinationSize = new SizeF(base.BoundRect.Width, base.TxtureObject.Height * base.BoundRect.Width / base.TxtureObject.Width);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, base.Position, Color.FromArgb(base.TransparentValue, Color.White));
		if (Text != null && StringTexture != null)
		{
			if (Shadowed)
			{
				base.SpriteMain.Draw2D(StringTexture, new Rectangle(0, 0, 1024, 256), new SizeF(1024f, 256f), new PointF((int)base.Position.X + 1 + OffsetX, (int)base.Position.Y + 1 + (int)((float)OffsetY * destinationSize.Height / 90f)), Color.FromArgb(150, 0, 0, 0));
			}
			base.SpriteMain.Draw2D(StringTexture, new Rectangle(0, 0, 1024, 256), new SizeF(1024f, 256f), new PointF((int)base.Position.X + OffsetX, (int)base.Position.Y + (int)((float)OffsetY * destinationSize.Height / 90f)), Color.White);
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
