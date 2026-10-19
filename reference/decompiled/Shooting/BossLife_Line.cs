using System.Drawing;

namespace Shooting;

public class BossLife_Line : Life_Line
{
	public BossLife_Line(StageDataPackage StageData, string textureName, PointF OriginalPosition)
		: base(StageData, textureName, OriginalPosition)
	{
		base.TransparentVelocity = 0f;
		base.TransparentValueF = 0f;
	}

	public override void Ctrl()
	{
		if (base.Boss == null)
		{
			return;
		}
		base.Ctrl();
		if (base.MyPlane.OriginalPosition.X < (float)base.Life * size && base.MyPlane.OriginalPosition.Y < size + base.OriginalPosition.Y)
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
		base.Life = base.Boss.Life;
	}

	public override void Show()
	{
		if (base.Boss == null)
		{
			return;
		}
		size = 16f;
		SizeF destinationSize = new SizeF(size, size);
		for (int i = 0; i < base.Life - 1; i++)
		{
			PointF position;
			if (i < 5)
			{
				position = new PointF(base.OriginalPosition.X + (float)i * size, base.OriginalPosition.Y);
				base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
			}
			else
			{
				position = new PointF(base.OriginalPosition.X + (float)(i - 5) * size, base.OriginalPosition.Y + 16f);
				base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, new PointF(0f, 0f), 0f, position, Color.FromArgb(base.TransparentValue, Color.White));
			}
		}
	}
}
