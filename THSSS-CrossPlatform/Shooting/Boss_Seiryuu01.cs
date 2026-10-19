using System.Drawing;

namespace Shooting;

internal class Boss_Seiryuu01 : BaseBossTouhou
{
	public Boss_Seiryuu01(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossSeiryuu-00"],
				base.TextureObjectDictionary["BossSeiryuu-01"],
				base.TextureObjectDictionary["BossSeiryuu-02"],
				base.TextureObjectDictionary["BossSeiryuu-03"]
			},
			{
				base.TextureObjectDictionary["BossSeiryuu-10"],
				base.TextureObjectDictionary["BossSeiryuu-11"],
				base.TextureObjectDictionary["BossSeiryuu-12"],
				base.TextureObjectDictionary["BossSeiryuu-13"]
			},
			{
				base.TextureObjectDictionary["BossSeiryuu-20"],
				base.TextureObjectDictionary["BossSeiryuu-21"],
				base.TextureObjectDictionary["BossSeiryuu-22"],
				base.TextureObjectDictionary["BossSeiryuu-23"]
			}
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, 150f);
		base.DestPoint = new Point(base.BoundRect.Width / 2, 150);
		base.MaxHP = 10000;
		base.HealthPoint = base.MaxHP;
		base.Life = 3;
		base.Region = 20;
		base.Velocity = 4f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 2400;
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
			new EndBoss_FSC03(base.StageData, base.OriginalPosition, Color.ForestGreen, Color.Green);
		}
		else
		{
			base.LifeTime = base.Time + 150;
			new EndBoss_Touhou03(base.StageData, base.OriginalPosition, Color.ForestGreen, Color.Green);
		}
	}

	private void Ctrl3()
	{
		if (!base.OnSpell)
		{
			base.Armon = 0.25f;
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 50)
			{
				RandomMove(206, 20, 2f, new Rectangle(base.BoundRect.Width / 2 - 45, 100, 90, 40));
			}
			if (base.Time == 60)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => "CS/St03/关底Boss/1非E.mbg", 
					1 => "CS/St03/关底Boss/1非N.mbg", 
					2 => "CS/St03/关底Boss/1非H.mbg", 
					3 => "CS/St03/关底Boss/1非L.mbg", 
					_ => "CS/St03/关底Boss/1非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS03_03(base.StageData);
		}
	}

	private void Ctrl2()
	{
		if (!base.OnSpell)
		{
			base.Armon = 0.25f;
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 120);
			}
			else if (base.Time > 150)
			{
				RandomMove(206, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 160)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => "CS/St03/关底Boss/2非E.mbg", 
					1 => "CS/St03/关底Boss/2非N.mbg", 
					2 => "CS/St03/关底Boss/2非H.mbg", 
					3 => "CS/St03/关底Boss/2非L.mbg", 
					_ => "CS/St03/关底Boss/2非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS03_04(base.StageData);
		}
	}

	private void Ctrl1()
	{
		if (!base.OnSpell)
		{
			base.Armon = 0.25f;
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 150)
			{
				RandomMove(206, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 160)
			{
				string fileName = (int)base.Difficulty switch
				{
					0 => "CS/St03/关底Boss/3非E.mbg", 
					1 => "CS/St03/关底Boss/3非N.mbg", 
					2 => "CS/St03/关底Boss/3非H.mbg", 
					3 => "CS/St03/关底Boss/3非L.mbg", 
					_ => "CS/St03/关底Boss/3非U.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS03_05(base.StageData);
		}
	}
}
