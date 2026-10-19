using System.Drawing;

namespace Shooting;

internal class Boss_Ami01 : BaseBossTouhou
{
	public Boss_Ami01(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossAmi-00"],
				base.TextureObjectDictionary["BossAmi-01"],
				base.TextureObjectDictionary["BossAmi-02"],
				base.TextureObjectDictionary["BossAmi-03"]
			},
			{
				base.TextureObjectDictionary["BossAmi-10"],
				base.TextureObjectDictionary["BossAmi-11"],
				base.TextureObjectDictionary["BossAmi-12"],
				base.TextureObjectDictionary["BossAmi-13"]
			},
			{
				base.TextureObjectDictionary["BossAmi-20"],
				base.TextureObjectDictionary["BossAmi-21"],
				base.TextureObjectDictionary["BossAmi-22"],
				base.TextureObjectDictionary["BossAmi-23"]
			}
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		base.OriginalPosition = new PointF(30f, -50f);
		base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
		base.MaxHP = 10000;
		base.HealthPoint = base.MaxHP;
		base.Life = 1;
		base.Region = 20;
		base.Velocity = 6f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 1200;
		base.LifeTime = 100000;
		base.OnSpell = false;
		SetBossLifeLineTex();
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
		new Background2DRemover(base.StageData);
		base.Background3D.WarpEnabled = false;
		base.LifeTime = base.Time + 150;
		base.Region = 0;
		base.StageData.SoundPlay("se_tan01.wav");
	}

	private void Ctrl1()
	{
		if (!base.OnSpell)
		{
			base.Armon = 0f;
			if (base.Time == 1)
			{
				base.DestPoint = new Point(base.BoundRect.Width / 2, 120);
			}
			else if (base.Time > 100)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 100)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St01\\道中Boss\\1非E.mbg", 
					1 => ".\\CS\\St01\\道中Boss\\1非N.mbg", 
					2 => ".\\CS\\St01\\道中Boss\\1非H.mbg", 
					3 => ".\\CS\\St01\\道中Boss\\1非L.mbg", 
					_ => ".\\CS\\St01\\道中Boss\\1非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
				cSEmitterController.BaseOriginalPosition = base.OriginalPosition;
			}
			return;
		}
		base.Armon = 0.8f;
		if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			if (base.Difficulty > DifficultLevel.Normal)
			{
				base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
				base.Boss.Velocity = 4f;
				new SpellCard_SSS01_01(base.StageData);
			}
		}
		if (base.Time == 100 && base.Difficulty <= DifficultLevel.Normal)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St01\\道中Boss\\2非E.mbg", 
				1 => ".\\CS\\St01\\道中Boss\\2非N.mbg", 
				_ => ".\\CS\\St01\\道中Boss\\2非N.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			CSEmitterController cSEmitterController2 = new CSEmitterController(base.StageData, cSData);
			cSEmitterController2.BossBinding = true;
			cSEmitterController2.BaseOriginalPosition = new Point(base.BoundRect.Width / 2, 120);
		}
		if (base.Time > 100 && base.Difficulty <= DifficultLevel.Normal)
		{
			RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
		}
	}
}
