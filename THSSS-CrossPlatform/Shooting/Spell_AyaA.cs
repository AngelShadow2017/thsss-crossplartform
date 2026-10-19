using System;
using System.Drawing;

namespace Shooting;

public class Spell_AyaA : BaseSpell
{
	public Spell_AyaA(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.OriginalPosition = base.MyPlane.OriginalPosition;
		base.Damage = 10;
		base.Region = 30;
		base.LifeTime = 300;
		base.Direction = Math.PI / 2.0;
		base.SpellList.Add(this);
		new BackgroundBlack(StageData, base.Position);
		BackgroundMySC backgroundMySC = new BackgroundMySC(StageData, "76-4")
		{
			LifeTime = 350,
			MaxTransparent = 180,
			TransparentValueF = 0f,
			TransparentVelocity = 4f,
			ColorValue = Color.Green
		};
		BackgroundMySC backgroundMySC2 = new BackgroundMySC(StageData, "76-4")
		{
			LifeTime = 350,
			MaxTransparent = 150,
			TransparentValueF = 0f,
			TransparentVelocity = 4f,
			ColorValue = Color.Green,
			AngularVelocityDegree = -0.5f,
			Active = true
		};
		StageData.VibrateStart(base.LifeTime);
		base.MyPlane.HighSpeed = 2f;
		base.MyPlane.LowSpeed = 1f;
		base.TransparentValueF = 0f;
		base.TransparentVelocity = 4f;
		base.ScaleLength = 0.2f;
		base.ScaleVelocity = 0.05f;
		base.MaxScale = 2f;
	}

	public override void Move()
	{
		base.OriginalPosition = base.MyPlane.OriginalPosition;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time < 16)
		{
			base.TxtureObject = base.TextureObjectDictionary["wind" + $"{base.Time % 16 / 2:00}"];
		}
		else if (base.Time < 284)
		{
			base.TxtureObject = base.TextureObjectDictionary["wind" + $"{7 + (base.Time - 16) % 36 / 2:00}"];
		}
		else
		{
			base.TxtureObject = base.TextureObjectDictionary["wind" + $"{25 + (base.Time - 300 + 16) % 16 / 2:00}"];
		}
		base.TxtureObject.OffsetY = 100;
		if (base.Time == 240)
		{
			base.TransparentVelocity = -4f;
		}
	}

	public override bool OutBoundary()
	{
		bool flag = base.OutBoundary();
		if (flag)
		{
			base.MyPlane.HighSpeed = 5f;
			base.MyPlane.LowSpeed = 2f;
		}
		return flag;
	}

	public override bool HitCheck(BaseObject Target)
	{
		if (Target.OriginalPosition.Y - base.OriginalPosition.Y < 2.8f * (Target.OriginalPosition.X - base.OriginalPosition.X) + 136f && Target.OriginalPosition.Y - base.OriginalPosition.Y < -2.8f * (Target.OriginalPosition.X - base.OriginalPosition.X) + 136f)
		{
			return true;
		}
		return false;
	}

	public override void HitCheckAll()
	{
		for (int num = base.BulletList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.BulletList[num]) && !base.BulletList[num].UnRemoveable)
			{
				base.BulletList[num].GiveEndEffect();
				base.BulletList[num].GiveItems();
				base.BulletList.RemoveAt(num);
			}
		}
		for (int num = base.EnemyPlaneList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.EnemyPlaneList[num]))
			{
				int num2 = base.Damage + (int)((400.0 - base.EnemyPlaneList[num].GetDistance(base.MyPlane)) / 50.0);
				if (num2 < base.Damage)
				{
					num2 = base.Damage;
				}
				else if (num2 > 19)
				{
					num2 = 19;
				}
				base.EnemyPlaneList[num].HealthPoint -= num2;
				if (base.EnemyPlaneList[num].HealthPoint <= 0f)
				{
					base.EnemyPlaneList[num].GiveEndEffect();
					base.EnemyPlaneList[num].GiveItems();
					base.EnemyPlaneList.RemoveAt(num);
					base.StageData.SoundPlay("se_enep00.wav");
					break;
				}
			}
		}
		if (base.Boss != null && base.Boss.Time > base.Boss.UnmatchedTime && HitCheck(base.Boss))
		{
			int num2 = base.Damage + (int)((400.0 - base.Boss.GetDistance(base.MyPlane)) / 40.0);
			if (num2 < base.Damage)
			{
				num2 = base.Damage;
			}
			else if (num2 > 19)
			{
				num2 = 19;
			}
			base.Boss.HealthPoint -= (int)((float)num2 * (1f - base.Boss.Armon));
			if (base.Boss.HealthPoint <= 200f)
			{
				base.StageData.SoundPlay("se_damage01.wav");
			}
		}
	}
}
