using System;
using System.Drawing;

namespace Shooting.Planes.Story;

internal class StoryStar : BaseObject
{
	private int Tran;

	public StoryStar(StageDataPackage StageData, string textureName, PointF Position, float Velocity, double Direction)
		: base(StageData, textureName, Position, Velocity, Direction)
	{
		base.TransparentValueF = 180f;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time > 10)
		{
			base.TransparentValueF -= 5f;
		}
		double num = 80.0 * Math.Sin((double)((float)base.Time * 33f / 180f) * Math.PI);
		Tran = base.TransparentValue + (int)num;
		if (Tran < 0)
		{
			Tran = 0;
		}
		else if (Tran > 255)
		{
			Tran = 255;
		}
	}

	public override void Show()
	{
		SizeF destinationSize = new SizeF((float)base.TxtureObject.Width * base.Scale, (float)base.TxtureObject.Height * base.Scale);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, base.TxtureObject.RotatingCenter, 0f, base.Position, Color.FromArgb(Tran, Color.White));
	}
}
