using System.Drawing;

namespace Shooting;

internal class Boss_Rika02 : BaseBossTouhou
{
	public Boss_Rika02(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossRika-00"],
				base.TextureObjectDictionary["BossRika-01"],
				base.TextureObjectDictionary["BossRika-02"],
				base.TextureObjectDictionary["BossRika-03"]
			},
			{
				base.TextureObjectDictionary["BossRika-10"],
				base.TextureObjectDictionary["BossRika-11"],
				base.TextureObjectDictionary["BossRika-12"],
				base.TextureObjectDictionary["BossRika-13"]
			},
			{
				base.TextureObjectDictionary["BossRika-20"],
				base.TextureObjectDictionary["BossRika-21"],
				base.TextureObjectDictionary["BossRika-22"],
				base.TextureObjectDictionary["BossRika-23"]
			}
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, 120f);
		base.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.MaxHP = 10000;
		base.HealthPoint = base.MaxHP;
		base.Life = 1;
		base.Region = 20;
		base.Velocity = 4f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 3600;
		base.LifeTime = 100000;
		base.OnSpell = false;
		SetBossLifeLineTex();
		LoadArmon(".\\CS\\StEx\\关底Boss\\ctrl.csv");
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (!base.OnSpell)
		{
			base.Armon = 0f;
		}
		switch (base.Life)
		{
		case 1:
			Ctrl1();
			break;
		case 0:
			base.Velocity = 0f;
			base.DestPoint = new Point(base.Ran.Next(100, base.BoundRect.Width - 100), base.Ran.Next(70, 200));
			break;
		}
		MoveToPoint(base.DestPoint);
		if (!base.OnSpell)
		{
			if (base.HealthPoint < (float)base.SpellcardHP || base.Time > base.SpellTime)
			{
				base.HealthPoint = base.SpellcardHP;
				base.Time = 0;
				base.OnSpell = true;
				new ShootingStarShard(base.StageData, new PointF(base.BoundRect.Width / 2, 0f));
			}
		}
		else
		{
			if (!(base.HealthPoint <= 0f) && base.Time <= base.SpellTime)
			{
				return;
			}
			base.Life--;
			if (base.HealthPoint <= 0f && base.Life >= -1)
			{
				GiveItems();
				new ShootingStarShard(base.StageData, new PointF(base.BoundRect.Width / 2, 0f));
			}
			if (base.Life <= 0)
			{
				if (base.Life == 0)
				{
					base.DestPoint = new Point(base.Ran.Next((int)base.OriginalPosition.X - 30, (int)base.OriginalPosition.X + 30), base.Ran.Next((int)base.OriginalPosition.Y + 30, (int)base.OriginalPosition.Y + 50));
					base.Velocity = 0.5f;
					new BulletRemover2(base.StageData, base.OriginalPosition);
					base.HealthPoint = 0f;
					GiveEndEffect();
					base.Life = -1;
				}
			}
			else
			{
				new BulletRemover2(base.StageData, base.OriginalPosition);
				base.StageData.SoundPlay("se_tan00.wav");
				base.HealthPoint = base.MaxHP;
				base.Time = 0;
				base.OnSpell = false;
				base.DestPoint = new Point(base.BoundRect.Width / 2, 120);
				base.Velocity = 4f;
			}
		}
	}

	public override void GiveEndEffect()
	{
		base.TransparentValueF = 255f;
		new Background2DRemover(base.StageData);
		base.Background3D.WarpEnabled = false;
		base.LifeTime = base.Time + 150;
		base.Region = 0;
		base.StageData.SoundPlay("se_tan01.wav");
		new EndBoss_TouhouEx(base.StageData, base.OriginalPosition, Color.White, Color.Blue);
	}

	private void Ctrl1()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				base.OnSpell = true;
				base.SpellTime = 3000;
			}
		}
		else if (base.Time == 2)
		{
			new SpellCard_SSSEx_FSC(base.StageData);
		}
	}
}
