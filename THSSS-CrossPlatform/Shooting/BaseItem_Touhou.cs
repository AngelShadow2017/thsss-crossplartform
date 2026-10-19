using System;
using System.Drawing;

namespace Shooting;

public class BaseItem_Touhou : BaseItem
{
	public BaseItem_Touhou(StageDataPackage StageData, string textureName, PointF OriginalPosition)
		: base(StageData, textureName, OriginalPosition)
	{
		base.OriginalPosition = OriginalPosition;
		base.Velocity = 4f;
		base.Direction = 4.71238898038469;
		base.Angle = 0.0;
		base.Accelerate = (0f - base.Velocity) / 40f;
		base.AngularVelocity = 0.2f;
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
		if (base.Velocity < 0f)
		{
			base.Velocity = 0f;
		}
		base.Angle += base.AngularVelocity;
		base.TransparentValueF += base.TransparentVelocity;
		if (base.Obtain)
		{
			base.Direction = Math.PI + Math.Atan2(base.Position.Y - base.MyPlane.Position.Y, base.Position.X - base.MyPlane.Position.X);
			base.Accelerate = 1f;
			if (base.Velocity > 10f)
			{
				base.Velocity = 10f;
			}
		}
		else if (base.Time == 40)
		{
			base.AngularVelocity = 0f;
			base.Angle = 0.0;
		}
		else if (base.Time > 40)
		{
			base.Direction = Math.PI / 2.0;
			base.Accelerate = 0.15f;
			if (base.Velocity > 1.5f)
			{
				base.Velocity = 1.5f;
			}
		}
		if (base.OriginalPosition.Y < 0f)
		{
			base.TransparentValueF = 255 + 2 * (int)base.OriginalPosition.Y;
		}
		else
		{
			base.TransparentValueF = 255f;
		}
		if (OutBoundary())
		{
			base.ItemList.Remove(this);
		}
	}

	public override void HitCheckAll()
	{
		if (base.Time >= 10)
		{
			if (HitCheck(base.MyPlane, 48f))
			{
				base.Obtain = true;
			}
			if (base.MyPlane.OriginalPosition.Y < 128f)
			{
				base.Obtain = true;
				base.Doubled = true;
			}
		}
	}
}
