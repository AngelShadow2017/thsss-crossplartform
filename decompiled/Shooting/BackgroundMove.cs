using System.Drawing;

namespace Shooting;

public class BackgroundMove : BaseObject_CS
{
	public BackgroundMove(StageDataPackage StageData, string textureName)
		: base(StageData, textureName, new PointF(0f, 0f), 0f, 0.0)
	{
		base.Background.BackgroundList.Add(this);
		base.OriginalPosition = new PointF(base.BoundRect.Height / 2, base.BoundRect.Height / 2);
		base.Scale = (float)base.BoundRect.Height / (float)base.TxtureObject.Height;
		base.Velocity = 2f;
		base.DirectionDegree = 225.0;
		base.OutsideRegion = 1000;
	}

	public override void Ctrl()
	{
		base.AngleDegree = 0.0 - base.DirectionDegree + 90.0;
		base.Ctrl();
		float x = base.OriginalPosition.X;
		float y = base.OriginalPosition.Y;
		if (base.OriginalPosition.X <= (float)(-base.BoundRect.Height / 2))
		{
			x = base.OriginalPosition.X + (float)base.BoundRect.Height;
		}
		else if (base.OriginalPosition.X >= (float)(base.BoundRect.Height / 2))
		{
			x = base.OriginalPosition.X - (float)base.BoundRect.Height;
		}
		if (base.OriginalPosition.Y <= (float)(-base.BoundRect.Height / 2))
		{
			y = base.OriginalPosition.Y + (float)base.BoundRect.Height;
		}
		else if (base.OriginalPosition.Y >= (float)(base.BoundRect.Height / 2))
		{
			y = base.OriginalPosition.Y - (float)base.BoundRect.Height;
		}
		base.OriginalPosition = new PointF(x, y);
	}

	public override void Show()
	{
		float x = base.OriginalPosition.X;
		float y = base.OriginalPosition.Y;
		base.Show();
		x += (float)base.BoundRect.Height;
		base.OriginalPosition = new PointF(x, y);
		base.Show();
		y += (float)base.BoundRect.Height;
		base.OriginalPosition = new PointF(x, y);
		base.Show();
		x -= (float)base.BoundRect.Height;
		base.OriginalPosition = new PointF(x, y);
		base.Show();
		y -= (float)base.BoundRect.Height;
		base.OriginalPosition = new PointF(x, y);
	}
}
