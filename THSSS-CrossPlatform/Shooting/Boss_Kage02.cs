using System.Drawing;

namespace Shooting;

internal class Boss_Kage02 : BaseBossTouhou
{
	public Boss_Kage02(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossKage-00"],
				base.TextureObjectDictionary["BossKage-01"],
				base.TextureObjectDictionary["BossKage-02"],
				base.TextureObjectDictionary["BossKage-03"]
			},
			{
				base.TextureObjectDictionary["BossKage-10"],
				base.TextureObjectDictionary["BossKage-11"],
				base.TextureObjectDictionary["BossKage-12"],
				base.TextureObjectDictionary["BossKage-13"]
			},
			{
				base.TextureObjectDictionary["BossKage-20"],
				base.TextureObjectDictionary["BossKage-21"],
				base.TextureObjectDictionary["BossKage-22"],
				base.TextureObjectDictionary["BossKage-23"]
			}
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, 150f);
		base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
		base.MaxHP = 10000;
		base.HealthPoint = base.MaxHP;
		base.Life = 1;
		base.Region = 20;
		base.Velocity = 4f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 2400;
		base.LifeTime = 100000;
		base.OnSpell = false;
		SetBossLifeLineTex();
		LoadArmon("CS/St04/关底Boss/ctrl.csv");
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
		base.Region = 0;
		base.StageData.SoundPlay("se_tan01.wav");
		base.LifeTime = base.Time + 150;
		new EndBoss_Touhou04(base.StageData, base.OriginalPosition, Color.Pink, Color.Violet);
	}

	public override void TextureCtrl()
	{
		int num = (base.Mirrored ? base.IndexY : (-base.IndexY));
		if (base.OnAction > 0)
		{
			base.IndexY = 2;
			base.IndexX += 0.125f;
			base.OnAction--;
		}
		else if (base.Vx < -0.05f)
		{
			base.Mirrored = false;
			base.IndexY = 1;
			if (base.Vx < -1f)
			{
				if (base.IndexX < 2f)
				{
					base.IndexX += 0.125f;
				}
				else
				{
					base.IndexX = 2 + base.Time % 16 / 8;
				}
			}
			else if (base.Vx < -0.5f)
			{
				base.IndexX = 1f;
			}
			else
			{
				base.IndexX = 0f;
			}
		}
		else if (base.Vx > 0.05f)
		{
			base.Mirrored = true;
			base.IndexY = 1;
			if (base.Vx > 1f)
			{
				if (base.IndexX < 2f)
				{
					base.IndexX += 0.125f;
				}
				else
				{
					base.IndexX = 2 + base.Time % 16 / 8;
				}
			}
			else if (base.Vx > 0.5f)
			{
				base.IndexX = 1f;
			}
			else
			{
				base.IndexX = 0f;
			}
		}
		else
		{
			base.Mirrored = false;
			base.IndexY = 0;
			base.IndexX = base.TimeMain % 32 / 8;
		}
		if (num != (base.Mirrored ? base.IndexY : (-base.IndexY)))
		{
			base.IndexX = 0f;
		}
		if (base.IndexX > 3f)
		{
			base.IndexX = 3f;
		}
		base.TxtureObject = base.TxtureObjects[base.IndexY, (int)base.IndexX];
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
		else
		{
			base.Boss.Armon = 0.4f;
			if (base.Time == 2)
			{
				new SpellCard_SSS04_FSC(base.StageData);
			}
		}
	}

	public override void GiveItems()
	{
		base.GiveItems();
		new SpellItem_Touhou(base.StageData, base.OriginalPosition);
	}
}
