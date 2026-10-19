using System;
using System.Drawing;

namespace Shooting;

public class Bullet_RadialLaser : BaseBullet_Touhou
{
	private BaseEffect beginEffect;

	public int UnmatchedTime { get; set; }

	public Bullet_RadialLaser(StageDataPackage StageData)
		: base(StageData)
	{
	}

	public Bullet_RadialLaser(StageDataPackage StageData, PointF OriginalPosition, float Velocity, double Direction, byte ColorType, int Length)
		: this(StageData, OriginalPosition, Velocity, Direction, ColorType, Length, 16)
	{
	}

	public Bullet_RadialLaser(StageDataPackage StageData, PointF OriginalPosition, float Velocity, double Direction, byte ColorType, int Length, int Width)
		: this(StageData, "Laser0_" + ColorType, OriginalPosition, Velocity, Direction, Length)
	{
		base.ColorType = ColorType;
		base.Active = true;
		base.Angle = Math.PI / 2.0;
		base.MaxScaleL = (float)Width / (float)base.TxtureObject.Height;
	}

	public Bullet_RadialLaser(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, int Length)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		base.UnRemoveable = true;
		base.Region = 2;
		base.ScaleVW = 0.5f;
		base.MaxScaleW = (float)Length / (float)base.TxtureObject.Width;
		base.ScaleWidth = base.MaxScaleW;
		base.MaxScaleL = 1f;
		base.ScaleLength = 0.2f;
		UnmatchedTime = 60;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 1)
		{
			beginEffect = new ParticleBeginBullet(base.StageData, base.Position, base.ColorType)
			{
				Scale = base.MaxScaleW * 0.5f,
				Active = true,
				LifeTime = base.LifeTime
			};
			beginEffect.SetBinding(this);
		}
		if (base.Time == UnmatchedTime)
		{
			base.ScaleVL = 0.1f;
		}
		if (base.Time == base.LifeTime - 8)
		{
			base.TransparentVelocity = -base.MaxTransparent / 8;
			base.ScaleVL = (0f - base.ScaleLength) / 8f;
		}
		if (base.Time % 8 == 0)
		{
			base.Grazed = false;
		}
	}

	public override bool OutBoundary()
	{
		if (base.LifeTime != 0 && base.Time > base.LifeTime)
		{
			return true;
		}
		return false;
	}

	public override bool HitCheck(BaseObject Target, float Radius)
	{
		if (base.Time < UnmatchedTime)
		{
			return false;
		}
		double distance = GetDistance(Target);
		double num = GetDirection(Target);
		double num2 = distance * Math.Cos((float)(base.Direction - num));
		double num3 = distance * Math.Sin((float)(base.Direction - num));
		double num4 = Radius * base.ScaleLength + (float)Target.Region * Target.Scale;
		double num5 = ((float)base.TxtureObject.Width + Radius) * base.ScaleWidth * 0.45f + (float)Target.Region * Target.Scale;
		if (num3 * num3 / num4 / num4 + (num2 - (double)((float)base.TxtureObject.Width * base.ScaleWidth / 2f)) * (num2 - (double)((float)base.TxtureObject.Width * base.ScaleWidth / 2f)) / num5 / num5 < 1.0)
		{
			return true;
		}
		return false;
	}

	public override void GiveEndEffect()
	{
		beginEffect.LifeTime = beginEffect.Time + 1;
	}

	public override void Show()
	{
		if (base.TxtureObject != null)
		{
			SizeF destinationSize = new SizeF((float)base.TxtureObject.Width * base.ScaleWidth, (float)base.TxtureObject.Height * base.ScaleLength);
			base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, new PointF(0f, base.TxtureObject.Height / 2), (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.FromArgb(base.TransparentValue, base.ColorValue));
		}
	}

	public override void ShowRegion()
	{
		PointF originalPosition = base.OriginalPosition;
		base.OriginalPosition = new PointF(base.OriginalPosition.X + (float)base.TxtureObject.Width * base.ScaleWidth * 0.5f * (float)Math.Cos(base.Direction), base.OriginalPosition.Y + (float)base.TxtureObject.Width * base.ScaleWidth * 0.5f * (float)Math.Sin(base.Direction));
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["Region"], (float)base.TxtureObject.Width * 0.9f / (float)base.TextureObjectDictionary["Region"].Height * base.ScaleWidth, 2f * (float)base.Region / (float)base.TextureObjectDictionary["Region"].Width * base.ScaleLength, (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.Red, base.Mirrored);
		base.OriginalPosition = originalPosition;
	}
}
