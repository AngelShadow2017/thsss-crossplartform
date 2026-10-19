using System;
using System.Drawing;

namespace Shooting;

internal class MusicTitle : BaseEffect
{
	public new PointF Position => new PointF(base.OriginalPosition.X + 36f, base.OriginalPosition.Y + 16f);

	public MusicTitle(StageDataPackage StageData, string textureName)
		: this(StageData, textureName, new Point(0, 0))
	{
		base.DestPoint = new Point(base.BoundRect.Width, base.BoundRect.Height - 16);
		base.Scale = 0.5f;
		base.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
	}

	public MusicTitle(StageDataPackage StageData, string textureName, Point DestPoint)
		: base(StageData)
	{
		base.TxtureObject = base.TextureObjectDictionary[textureName];
		base.OriginalPosition = new PointF(base.BoundRect.Width - 10, 350f);
		base.Velocity = 4f;
		base.Direction = Math.PI / 2.0;
		base.Scale = 2f;
		base.DestPoint = DestPoint;
		base.LifeTime = 240;
	}

	public override void Move()
	{
		float num = (float)GetDistance(base.DestPoint);
		if (num > 50f)
		{
			base.Velocity = 7f;
		}
		else if (num > 35f)
		{
			base.Velocity = 5f;
		}
		else if (num > 15f)
		{
			base.Velocity = 3f;
		}
		else if (num > 1f)
		{
			base.Velocity = 1f;
		}
		else
		{
			base.Velocity = 0f;
		}
		base.Direction = GetDirection(base.DestPoint);
		base.Angle = 0f - (float)(base.Direction - Math.PI / 2.0);
		base.Move();
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Scale > 1f)
		{
			base.Scale -= 0.03f;
		}
		if (base.Time >= base.LifeTime - 35)
		{
			base.MaxTransparent -= 8;
		}
	}

	public override bool OutBoundary()
	{
		if (base.Time > base.LifeTime)
		{
			return true;
		}
		return false;
	}

	public override void Show()
	{
		SizeF destinationSize = new SizeF((float)base.TxtureObject.Width * base.Scale, (float)base.TxtureObject.Height * base.Scale);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, base.TxtureObject.RightTop, 0f, Position, Color.FromArgb(base.TransparentValue, Color.White));
	}
}
