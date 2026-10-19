using System.Drawing;

namespace Shooting;

internal class Boss_Koreirei01 : BaseBossTouhou
{
	public Boss_Koreirei01(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossKoreirei-00"],
				base.TextureObjectDictionary["BossKoreirei-01"],
				base.TextureObjectDictionary["BossKoreirei-02"],
				base.TextureObjectDictionary["BossKoreirei-03"]
			},
			{
				base.TextureObjectDictionary["BossKoreirei-10"],
				base.TextureObjectDictionary["BossKoreirei-11"],
				base.TextureObjectDictionary["BossKoreirei-12"],
				base.TextureObjectDictionary["BossKoreirei-13"]
			},
			{
				base.TextureObjectDictionary["BossKoreirei-20"],
				base.TextureObjectDictionary["BossKoreirei-21"],
				base.TextureObjectDictionary["BossKoreirei-22"],
				base.TextureObjectDictionary["BossKoreirei-23"]
			}
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		base.OriginalPosition = new PointF(30f, -50f);
		base.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.MaxHP = 10000;
		base.HealthPoint = base.MaxHP;
		base.Life = 2;
		base.Region = 20;
		base.Velocity = 6f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 1200;
		base.LifeTime = 100000;
		base.OnSpell = false;
		base.SpellcardHP = 0;
		SetBossLifeLineTex();
		new ItemGetter(StageData);
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
		case 2:
			Ctrl2();
			break;
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
			base.OnAction = 0;
			if (base.HealthPoint <= 0f && base.Life >= -1)
			{
				GiveItems();
				new ShootingStarShard(base.StageData, new PointF(base.BoundRect.Width / 2, 0f));
			}
			if (base.Life <= 0)
			{
				if (base.Life == 0)
				{
					base.Velocity = 0f;
					new BulletRemover2(base.StageData, base.OriginalPosition);
					base.HealthPoint = 0f;
					GiveEndEffect();
					base.Life = -1;
				}
				else if (base.Time == base.LifeTime - 100)
				{
					base.DestPoint = new Point(base.Ran.Next((int)base.OriginalPosition.X - 30, (int)base.OriginalPosition.X + 30), -200);
					base.Velocity = 5f;
					base.Background3D.WarpEnabled = false;
				}
			}
			else
			{
				new BulletRemover2(base.StageData, base.OriginalPosition);
				base.StageData.SoundPlay("se_tan00.wav");
				base.HealthPoint = base.MaxHP;
				base.Time = 0;
				base.OnSpell = false;
				base.DestPoint = new Point(base.BoundRect.Width / 2, 100);
				base.Velocity = 4f;
			}
		}
	}

	public override void GiveEndEffect()
	{
		new EndBoss_Touhou_Small(base.StageData, base.OriginalPosition, Color.Yellow, Color.OrangeRed);
		new Background2DRemover(base.StageData);
		base.Background3D.WarpEnabled = false;
		new LifeItem_Touhou(base.StageData, base.OriginalPosition);
	}

	private void Ctrl2()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 140);
			}
			else
			{
				RandomMove1(150, 4f);
			}
			if (base.Time == 90)
			{
				base.OnSpell = true;
			}
		}
		else if (base.Time == 91)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			base.Armon = 0.4f;
			new SpellCard_SSS03_01(base.StageData);
		}
	}

	private void Ctrl1()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 180);
			}
			else
			{
				RandomMove1(150, 4f);
			}
			if (base.Time == 100)
			{
				base.OnSpell = true;
				base.SpellTime = 2400;
			}
		}
		else if (base.Time == 101)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			base.Armon = 0.4f;
			new SpellCard_SSS03_02(base.StageData);
		}
	}
}
