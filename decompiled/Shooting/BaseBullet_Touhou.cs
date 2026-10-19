using System;
using System.Drawing;

namespace Shooting;

public class BaseBullet_Touhou : BaseObject_CS
{
	public int Layer { get; set; }

	public byte ColorType { get; set; }

	public int SizeValue { get; set; }

	public byte Reflect { get; set; }

	public bool Grazed { get; set; }

	public BaseBullet_Touhou(StageDataPackage StageData)
		: base(StageData)
	{
	}

	public BaseBullet_Touhou(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, byte ColorType)
		: this(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		this.ColorType = ColorType;
		base.BeginningEffect = true;
	}

	public BaseBullet_Touhou(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		base.BulletList.Add(this);
		base.OriginalPosition = OriginalPosition;
		base.AngleDegree = 180.0;
		SizeValue = 32;
		SetRegion(textureName);
	}

	public void SetRegion(string textureName)
	{
		if (textureName != null)
		{
			char[] separator = new char[1] { '_' };
			string[] array = textureName.Split(separator);
			switch (array[0])
			{
			case "bullet0":
				base.Region = 1;
				break;
			case "bullet1":
				base.Region = 1;
				break;
			case "bullet2":
				base.Region = 2;
				break;
			case "bullet3":
				base.Region = 2;
				break;
			case "bullet4":
				base.Region = 1;
				break;
			case "bullet5":
				base.Region = 1;
				break;
			case "bullet6":
				base.Region = 1;
				break;
			case "bullet7":
				base.Region = 1;
				break;
			case "bullet8":
				base.Region = 1;
				break;
			case "bullet9":
				base.Region = 1;
				break;
			case "bullet10":
				base.Region = 2;
				break;
			case "bullet11":
				base.Region = 2;
				break;
			case "bullet12":
				base.Region = 1;
				break;
			case "bullet13":
				base.Region = 3;
				break;
			case "bullet15":
				base.Region = 1;
				break;
			case "bullet20":
				base.Region = 6;
				break;
			case "bullet21":
				base.Region = 6;
				break;
			case "bullet22":
				base.Region = 3;
				break;
			case "bullet23":
				base.Region = 3;
				break;
			case "bullet24":
				base.Region = 3;
				break;
			case "bullet26":
				base.Region = 16;
				base.AngularVelocityDegree = 10f;
				base.Active = true;
				SizeValue = 80;
				base.OutsideRegion = 32;
				break;
			case "bullet50":
				base.Region = 12;
				base.Active = true;
				SizeValue = 64;
				base.OutsideRegion = 64;
				break;
			case "bullet40":
				base.Region = 6;
				SizeValue = 32;
				base.OutsideRegion = 32;
				break;
			case "bullet46":
				base.Region = 1;
				break;
			case "bullet61":
				base.Region = 2;
				break;
			case "bullet62":
				base.Region = 3;
				break;
			case "bullet64":
				base.Region = 3;
				break;
			case "bullet65":
				base.Region = 3;
				break;
			case "bullet66":
				base.Region = 3;
				break;
			case "bullet67":
				base.Region = 3;
				break;
			}
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 1 && base.BeginningEffect)
		{
			ParticleBeginBullet particleBeginBullet = new ParticleBeginBullet(base.StageData, base.Position, ColorType);
			particleBeginBullet.Active = base.Active;
		}
		if (base.Time == base.LifeTime && base.EndingEffect)
		{
			GiveEndEffect();
		}
		if (OutBoundary())
		{
			base.BulletList.Remove(this);
		}
	}

	public override bool OutBoundary()
	{
		if (base.LifeTime != 0 && base.Time > base.LifeTime)
		{
			return true;
		}
		if (Reflect <= 0)
		{
			return base.OutBoundary();
		}
		if (base.OriginalPosition.X < 0f)
		{
			Reflect--;
			base.Direction -= 2.0 * (base.Direction - Math.PI / 2.0);
			base.OriginalPosition = new PointF(0f - base.OriginalPosition.X, base.OriginalPosition.Y);
		}
		else if (base.OriginalPosition.X >= (float)base.BoundRect.Width)
		{
			Reflect--;
			base.Direction -= 2.0 * (base.Direction - Math.PI / 2.0);
			base.OriginalPosition = new PointF((float)(2 * base.BoundRect.Width) - base.OriginalPosition.X, base.OriginalPosition.Y);
		}
		if (base.OriginalPosition.Y < 20f && Reflect > 0)
		{
			Reflect--;
			base.Direction = 0.0 - base.Direction;
			base.OriginalPosition = new PointF(base.OriginalPosition.X, 20f - base.OriginalPosition.Y);
		}
		return false;
	}

	public override void HitCheckAll()
	{
		if (!base.MyPlane.HitEnabled)
		{
			return;
		}
		if (HitCheck(base.MyPlane))
		{
			base.MyPlane.PreMiss();
			if (!base.UnRemoveable)
			{
				GiveEndEffect();
				base.BulletList.Remove(this);
			}
		}
		else if (!Grazed && HitCheck(base.MyPlane, 24f / base.ScaleLength + (float)base.Region))
		{
			Grazed = true;
			base.MyPlane.Graze++;
			if (base.MyPlane.Graze % 10 == 0)
			{
				base.MyPlane.HighItemScore += 10;
			}
			base.StageData.SoundPlay("se_graze.wav");
			BaseEffect baseEffect = new BaseEffect(base.StageData, "GrazeG", base.MyPlane.Position, 5f, base.Ran.NextDouble() * Math.PI * 2.0);
			baseEffect.Scale = 1f;
			baseEffect.TransparentVelocity = -20f;
			baseEffect.ScaleVelocity = -0.05f;
			baseEffect.AngleWithDirection = false;
		}
	}

	public override void GiveEndEffect()
	{
		new ParticleEndBullet(base.StageData, base.OriginalPosition, ColorType, SizeValue);
	}

	public override void GiveItems()
	{
		new StarItem_Touhou(base.StageData, base.OriginalPosition);
	}
}
