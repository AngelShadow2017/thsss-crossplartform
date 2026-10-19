using System.Drawing;

namespace Shooting;

internal class Boss_Tensei01 : BaseBossTouhou
{
	public Boss_Tensei01(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossTensei-00"],
				base.TextureObjectDictionary["BossTensei-01"],
				base.TextureObjectDictionary["BossTensei-02"],
				base.TextureObjectDictionary["BossTensei-03"]
			},
			{
				base.TextureObjectDictionary["BossTensei-10"],
				base.TextureObjectDictionary["BossTensei-11"],
				base.TextureObjectDictionary["BossTensei-12"],
				base.TextureObjectDictionary["BossTensei-13"]
			},
			{
				base.TextureObjectDictionary["BossTensei-20"],
				base.TextureObjectDictionary["BossTensei-21"],
				base.TextureObjectDictionary["BossTensei-22"],
				base.TextureObjectDictionary["BossTensei-23"]
			}
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, 150f);
		base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
		base.MaxHP = 10000;
		base.HealthPoint = base.MaxHP;
		base.Life = 6;
		base.Region = 20;
		base.Velocity = 4f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 3600;
		base.LifeTime = 100000;
		base.OnSpell = false;
		SetBossLifeLineTex();
		LoadArmon(".\\CS\\St06\\关底Boss\\ctrl.csv");
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
		case 6:
			Ctrl6();
			break;
		case 5:
			Ctrl5();
			break;
		case 4:
			Ctrl4();
			break;
		case 3:
			Ctrl3();
			break;
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
		if (base.MyPlane.FscEnabled)
		{
			base.LifeTime = base.Time + 50;
			new EndBoss_FSC06(base.StageData, base.OriginalPosition, Color.White, Color.Blue);
		}
		else
		{
			base.LifeTime = base.Time + 150;
			new EndBoss_Touhou06(base.StageData, base.OriginalPosition, Color.White, Color.Blue);
		}
	}

	private void Ctrl6()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[0];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St06\\关底Boss\\1非E.mbg", 
					1 => ".\\CS\\St06\\关底Boss\\1非N.mbg", 
					2 => ".\\CS\\St06\\关底Boss\\1非H.mbg", 
					3 => ".\\CS\\St06\\关底Boss\\1非L.mbg", 
					_ => ".\\CS\\St06\\关底Boss\\1非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS06_01(base.StageData);
		}
	}

	private void Ctrl5()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[2];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St06\\关底Boss\\2非E.mbg", 
					1 => ".\\CS\\St06\\关底Boss\\2非N.mbg", 
					2 => ".\\CS\\St06\\关底Boss\\2非H.mbg", 
					3 => ".\\CS\\St06\\关底Boss\\2非L.mbg", 
					_ => ".\\CS\\St06\\关底Boss\\2非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS06_02(base.StageData);
		}
	}

	private void Ctrl4()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[4];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
			}
			else if (base.Time > 150)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 160)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St06\\关底Boss\\3非E.mbg", 
					1 => ".\\CS\\St06\\关底Boss\\3非N.mbg", 
					2 => ".\\CS\\St06\\关底Boss\\3非H.mbg", 
					3 => ".\\CS\\St06\\关底Boss\\3非L.mbg", 
					_ => ".\\CS\\St06\\关底Boss\\3非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS06_03(base.StageData);
		}
	}

	private void Ctrl3()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[6];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				new TenseiWing(base.StageData);
				base.StageData.VibrateStart(90);
			}
			if (base.Time == 150)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St06\\关底Boss\\4非E.mbg", 
					1 => ".\\CS\\St06\\关底Boss\\4非N.mbg", 
					2 => ".\\CS\\St06\\关底Boss\\4非H.mbg", 
					3 => ".\\CS\\St06\\关底Boss\\4非L.mbg", 
					_ => ".\\CS\\St06\\关底Boss\\4非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS06_04(base.StageData);
		}
	}

	private void Ctrl2()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
			}
			if (base.Time == 100)
			{
				base.OnSpell = true;
			}
		}
		else if (base.Time == 101)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS06_05(base.StageData);
		}
	}

	private void Ctrl1()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
			}
			if (base.Time == 100)
			{
				base.OnSpell = true;
				base.SpellTime = 7200;
			}
		}
		else if (base.Time == 101)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS06_06(base.StageData);
		}
	}
}
