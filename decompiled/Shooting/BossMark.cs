using System;
using System.Drawing;

namespace Shooting;

public class BossMark : BaseObject
{
	public BossMark(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.TxtureObject = base.TextureObjectDictionary["BossMark"];
		base.Position = new PointF(base.BoundRect.Left + base.BoundRect.Width / 2, 472f);
		base.AngleDegree = 90.0;
	}

	public override void Ctrl()
	{
		base.Time++;
		if (base.Boss != null)
		{
			base.OriginalPosition = new PointF(base.Boss.OriginalPosition.X + (float)base.BoundRect.X, 472f);
			base.TransparentValueF = 150 + (int)(100.0 * Math.Sin((double)base.Time * Math.PI * 2.0 / 60.0));
		}
	}

	public override void Show()
	{
		if (base.Boss != null && base.TxtureObject != null)
		{
			base.SpriteMain.Draw2D(base.TxtureObject, base.ScaleWidth, base.ScaleLength, (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.OriginalPosition, Color.FromArgb(base.TransparentValue, base.ColorValue), base.Mirrored);
		}
	}
}
