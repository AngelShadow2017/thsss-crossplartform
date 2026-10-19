using System;
using System.Drawing;

namespace Shooting;

internal class BulletLaser : BaseBullet
{
	public float DesScaleWidth { get; set; }

	public float DesScaleLength { get; set; }

	public BulletLaser(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		base.ScaleLength = 0.1f;
		DesScaleLength = 3f;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.ScaleLength <= DesScaleLength)
		{
			base.ScaleLength += 0.02f;
		}
	}

	public override bool HitCheck(BaseObject MyPlane)
	{
		double distance = GetDistance(MyPlane);
		double num = GetDirection(MyPlane);
		double num2 = distance * Math.Cos((float)(base.Direction - num));
		double num3 = distance * Math.Sin((float)(base.Direction - num));
		double num4 = (float)base.Region * base.ScaleWidth;
		double num5 = (float)base.TxtureObject.Height * base.ScaleLength * 0.45f;
		if (num3 * num3 / num4 / num4 + (num2 - (double)((float)base.TxtureObject.Height * base.ScaleLength / 2f)) * (num2 - (double)((float)base.TxtureObject.Height * base.ScaleLength / 2f)) / num5 / num5 < 1.0)
		{
			return true;
		}
		return false;
	}

	public override void GiveItems()
	{
	}

	public override void Show()
	{
		if (base.TxtureObject != null)
		{
			SizeF destinationSize = new SizeF((float)base.TxtureObject.Width * base.ScaleWidth, (float)base.TxtureObject.Height * base.ScaleLength);
			base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, new PointF(base.TxtureObject.Width / 2, 0f), (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.FromArgb(base.TransparentValue, base.ColorValue));
		}
	}

	public override void ShowRegion()
	{
		PointF originalPosition = base.OriginalPosition;
		base.OriginalPosition = new PointF(base.OriginalPosition.X + (float)base.TxtureObject.Height * base.ScaleLength * 0.5f * (float)Math.Cos(base.Direction), base.OriginalPosition.Y + (float)base.TxtureObject.Height * base.ScaleLength * 0.5f * (float)Math.Sin(base.Direction));
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["Region"], 2f * (float)base.Region / (float)base.TextureObjectDictionary["Region"].Width * base.ScaleWidth, (float)base.TxtureObject.Height * 0.9f / (float)base.TextureObjectDictionary["Region"].Height * base.ScaleLength, (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.White, base.Mirrored);
		base.OriginalPosition = originalPosition;
	}
}
