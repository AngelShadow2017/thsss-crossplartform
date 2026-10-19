using System;
using System.Drawing;

namespace Shooting;

public class MarisaLaser : BaseEffect
{
	public MarisaLaser(StageDataPackage StageData, string textureName, PointF OriginalPosition, float v, double drctn)
		: base(StageData, textureName, new PointF(0f, 0f), v, drctn)
	{
		base.StageData = StageData;
		base.OriginalPosition = OriginalPosition;
		base.Damage = 4;
		base.Region = 128;
		base.MaxVelocity = 1000f;
		base.MinVelocity = -1000f;
		base.Velocity = v;
		base.LifeTime = 5;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Velocity = 192f;
		bool flag = false;
		for (int num = base.EnemyPlaneList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.EnemyPlaneList[num]))
			{
				flag = true;
				if (base.MyPlane.EnchantmentState == EnchantmentType.Red)
				{
					base.EnemyPlaneList[num].HealthPoint -= (float)base.Damage * 1.25f;
				}
				else
				{
					base.EnemyPlaneList[num].HealthPoint -= base.Damage;
				}
				if (base.EnemyPlaneList[num].HealthPoint < 10000f)
				{
					base.MyPlane.Score += 10L;
				}
				if (base.EnemyPlaneList[num].HealthPoint <= 0f)
				{
					base.EnemyPlaneList[num].GiveEndEffect();
					base.EnemyPlaneList[num].GiveItems();
					base.EnemyPlaneList.RemoveAt(num);
					base.StageData.SoundPlay("se_enep00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
					break;
				}
			}
		}
		if (base.Boss != null && HitCheck(base.Boss))
		{
			flag = true;
			if (base.Boss.Time > base.Boss.UnmatchedTime)
			{
				if (base.MyPlane.EnchantmentState == EnchantmentType.Red)
				{
					base.Boss.HealthPoint -= (float)base.Damage * 1.25f * (1f - base.Boss.Armon);
				}
				else
				{
					base.Boss.HealthPoint -= (float)base.Damage * (1f - base.Boss.Armon);
				}
			}
			base.MyPlane.Score += 10L;
			if (base.Boss.HealthPoint <= 200f)
			{
				base.StageData.SoundPlay("se_damage01.wav");
			}
		}
		if (flag)
		{
			base.Damage = 0;
		}
	}

	public override bool HitCheck(BaseObject Target)
	{
		double distance = GetDistance(Target);
		double num = GetDirection(Target);
		double num2 = distance * Math.Cos((float)(base.Direction - num));
		double num3 = distance * Math.Sin((float)(base.Direction - num));
		double num4 = (float)base.TxtureObject.Width * base.ScaleWidth * 0.5f;
		double num5 = (float)base.TxtureObject.Height * base.ScaleLength * 0.5f;
		if (num3 * num3 / num5 / num5 + (num2 - 256.0 + 64.0 - (double)((float)base.TxtureObject.Height * base.ScaleLength / 2f)) * (num2 - 256.0 + 64.0 - (double)((float)base.TxtureObject.Height * base.ScaleLength / 2f)) / num4 / num4 < 1.0)
		{
			return true;
		}
		return false;
	}

	public override void Show()
	{
		int num = base.TimeMain % 8 * 8;
		SizeF destinationSize = new SizeF((float)(base.TxtureObject.Width - 64) * base.ScaleWidth, (float)base.TxtureObject.Height * base.ScaleLength);
		base.SpriteMain.Draw2D(base.TxtureObject.TXTure, new Rectangle(base.TxtureObject.PosRect.X + 64 - num, base.TxtureObject.PosRect.Y, base.TxtureObject.PosRect.Width - 64, base.TxtureObject.PosRect.Height), destinationSize, new PointF((base.TxtureObject.Width - 64) / 2, base.TxtureObject.Height / 2), (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.FromArgb(base.TransparentValue, base.ColorValue));
	}

	public override void ShowRegion()
	{
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["Region"], (float)(base.TxtureObject.Width - 64) * 1f / (float)base.TextureObjectDictionary["Region"].Width * base.ScaleWidth, (float)base.TxtureObject.Height * 1f / (float)base.TextureObjectDictionary["Region"].Height * base.ScaleLength, (float)(base.Direction - Math.PI / 2.0 + base.Angle), base.Position, Color.White, base.Mirrored);
	}
}
