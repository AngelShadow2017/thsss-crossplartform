using System.Drawing;

namespace Shooting;

public class BackgroundMove2 : BaseObject_CS
{
	public int Length;

	private float Width;

	public BackgroundMove2(StageDataPackage StageData, string textureName)
		: base(StageData, textureName, new PointF(0f, 0f), 0f, 0.0)
	{
		base.Background.BackgroundList.Add(this);
		base.OriginalPosition = new PointF(base.BoundRect.Height / 2, base.BoundRect.Height / 2);
		Length = base.BoundRect.Height * 2;
		base.Velocity = 2f;
		base.DirectionDegree = 225.0;
		base.OutsideRegion = 10000;
	}

	public override void Ctrl()
	{
		base.Scale = (float)Length / (float)base.TxtureObject.Height;
		Width = (float)base.TxtureObject.Width * base.Scale;
		base.AngleDegree = 0.0 - base.DirectionDegree + 90.0;
		base.Ctrl();
		float x = base.OriginalPosition.X;
		float y = base.OriginalPosition.Y;
		if (base.OriginalPosition.X <= (0f - Width) / 2f)
		{
			x = base.OriginalPosition.X + Width;
		}
		else if (base.OriginalPosition.X >= Width / 2f)
		{
			x = base.OriginalPosition.X - Width;
		}
		if (base.OriginalPosition.Y <= (float)(-Length / 2))
		{
			y = base.OriginalPosition.Y + (float)Length;
		}
		else if (base.OriginalPosition.Y >= (float)(Length / 2))
		{
			y = base.OriginalPosition.Y - (float)Length;
		}
		base.OriginalPosition = new PointF(x, y);
	}

	public override void Show()
	{
		float x = base.OriginalPosition.X;
		float y = base.OriginalPosition.Y;
		base.Show();
		x += Width;
		base.OriginalPosition = new PointF(x, y);
		base.Show();
		y += (float)Length;
		base.OriginalPosition = new PointF(x, y);
		base.Show();
		x -= Width;
		base.OriginalPosition = new PointF(x, y);
		base.Show();
		y -= (float)Length;
		base.OriginalPosition = new PointF(x, y);
	}
}
