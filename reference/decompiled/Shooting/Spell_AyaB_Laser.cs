using System;
using System.Drawing;

namespace Shooting;

public class Spell_AyaB_Laser : BaseSpell
{
	private BaseObject Target;

	public Spell_AyaB_Laser(StageDataPackage StageData, PointF OriginalPosition, bool Left)
	{
		base.StageData = StageData;
		base.SpellList.Add(this);
		base.OriginalPosition = OriginalPosition;
		base.Damage = 50;
		base.Region = 25;
		base.LifeTime = 20;
		base.Velocity = 50f;
		base.ScaleLength = 2f;
		base.ScaleWidth = 1f;
		base.TxtureObject = base.TextureObjectDictionary["Laser"];
		if (base.Boss != null && base.Boss.OriginalPosition.X > 0f && base.Boss.OriginalPosition.X < (float)base.BoundRect.Width && base.Boss.OriginalPosition.Y > 0f && base.Boss.OriginalPosition.Y < (float)base.BoundRect.Height)
		{
			Target = base.Boss;
		}
		if (Target == null)
		{
			double dist = 500.0;
			base.EnemyPlaneList.ForEach(delegate(BaseEnemyPlane x)
			{
				double distance = GetDistance(x);
				if (distance < dist)
				{
					Target = x;
					dist = distance;
				}
			});
		}
		if (Target == null)
		{
			Target = new BaseObject
			{
				OriginalPosition = new PointF(OriginalPosition.X + (float)base.Ran.Next(-80, 80), OriginalPosition.Y + (float)base.Ran.Next(-80, 80))
			};
			if (Target.OriginalPosition.Y < 0f)
			{
				Target.OriginalPosition = new PointF(Target.OriginalPosition.X, 0f);
			}
			else if (Target.OriginalPosition.Y > (float)base.BoundRect.Height)
			{
				Target.OriginalPosition = new PointF(Target.OriginalPosition.X, base.BoundRect.Height);
			}
		}
		base.OriginalPosition = new PointF(Left ? (-150) : (base.BoundRect.Width + 150), base.Ran.Next((int)Target.OriginalPosition.Y - 80, (int)Target.OriginalPosition.Y + 80));
		base.Direction = GetDirection(Target);
		base.SpellList[0].OriginalPosition = Target.OriginalPosition;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (Target != null)
		{
			if (Target.HealthPoint <= 0f)
			{
				Target = null;
			}
			if (OutBoundary())
			{
				Target = null;
				base.MyPlane.Time = base.MyPlane.UnmatchedTime - 90;
			}
		}
	}

	public override bool HitCheck(BaseObject Target)
	{
		double distance = GetDistance(Target);
		double num = GetDirection(Target);
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

	public override void Show()
	{
		SizeF destinationSize = new SizeF((float)base.TxtureObject.Width * base.ScaleWidth, (float)base.TxtureObject.Height * base.ScaleLength);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, base.TxtureObject.PosRect, destinationSize, new PointF(base.TxtureObject.Width / 2, 0f), (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.FromArgb(base.TransparentValue, base.ColorValue));
	}

	public override void ShowRegion()
	{
		PointF originalPosition = base.OriginalPosition;
		base.OriginalPosition = new PointF(base.OriginalPosition.X + (float)base.TxtureObject.Height * base.ScaleLength * 0.5f * (float)Math.Cos(base.Direction), base.OriginalPosition.Y + (float)base.TxtureObject.Height * base.ScaleLength * 0.5f * (float)Math.Sin(base.Direction));
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["Region"], 2f * (float)base.Region / (float)base.TextureObjectDictionary["Region"].Width * base.ScaleWidth, (float)base.TxtureObject.Height * 0.9f / (float)base.TextureObjectDictionary["Region"].Height * base.ScaleLength, (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.White, base.Mirrored);
		base.OriginalPosition = originalPosition;
	}
}
