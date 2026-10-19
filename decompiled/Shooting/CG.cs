using System;
using System.Drawing;

namespace Shooting;

internal class CG : BaseStoryItem
{
	public CG(StageDataPackage StageData, string textureName)
		: base(StageData)
	{
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.Active = false;
		base.TransparentValueF = 0f;
		base.TransparentVelocity = 20f;
		base.Angle = 0f - (float)(base.Direction - Math.PI / 2.0);
	}
}
