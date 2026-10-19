using System;
using System.Drawing;

namespace Shooting;

public class BaseItem : BaseObject
{
	public float AngularVelocity { get; set; }

	public bool Obtain { get; set; }

	public bool Doubled { get; set; }

	public BaseItem(StageDataPackage StageData, string textureName, PointF Position)
		: base(StageData, textureName, Position, 0f, 0.0)
	{
		base.ItemList.Add(this);
		base.Velocity = base.Ran.Next(1, 6);
		base.Direction = (double)((float)base.Ran.Next(180, 360) / 360f) * Math.PI * 2.0;
		base.Angle = 0.0;
		if ((float)base.Direction - (float)Math.PI / 2f > 0f)
		{
			AngularVelocity = -0.2f;
		}
		else
		{
			AngularVelocity = 0.2f;
		}
	}

	public override void HitCheckAll()
	{
		if (HitCheck(base.MyPlane, 200f))
		{
			Obtain = true;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Angle += AngularVelocity;
		if (base.Time == 12)
		{
			base.Direction = Math.PI / 2.0;
			AngularVelocity = 0f;
			base.Angle = 0.0;
		}
		else if (base.Time > 12)
		{
			if (Obtain)
			{
				base.Direction = Math.PI + Math.Atan2(base.Position.Y - base.MyPlane.Position.Y, base.Position.X - base.MyPlane.Position.X);
				base.Velocity = 8f;
			}
			else
			{
				base.Velocity = 4f;
			}
		}
		if (OutBoundary())
		{
			base.ItemList.Remove(this);
		}
	}

	public override bool OutBoundary()
	{
		if (base.OriginalPosition.X < 0f)
		{
			base.OriginalPosition = new PointF(0f, base.OriginalPosition.Y);
		}
		else if (base.OriginalPosition.X > (float)base.BoundRect.Width)
		{
			base.OriginalPosition = new PointF(base.BoundRect.Width, base.OriginalPosition.Y);
		}
		if (base.Time < 30)
		{
			return false;
		}
		if (base.Position.Y > (float)(base.BoundRect.Bottom + 30))
		{
			return true;
		}
		return false;
	}
}
