using System.Drawing;

namespace Shooting;

internal class Boss_Rika01 : BaseBossTouhou
{
	public Boss_Rika01(StageDataPackage StageData)
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
		base.Life = 10;
		base.Region = 20;
		base.Velocity = 4f;
		base.DirectionDegree = 90.0;
		base.SpellTime = 3600;
		base.LifeTime = 100000;
		base.OnSpell = false;
		SetBossLifeLineTex();
		LoadArmon("CS/StEx/关底Boss/ctrl.csv");
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
		case 10:
			Ctrl10();
			break;
		case 9:
			Ctrl9();
			break;
		case 8:
			Ctrl8();
			break;
		case 7:
			Ctrl7();
			break;
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
			new EndBoss_FSCEx(base.StageData, base.OriginalPosition, Color.White, Color.Blue);
		}
		else
		{
			base.LifeTime = base.Time + 150;
			new EndBoss_TouhouEx(base.StageData, base.OriginalPosition, Color.White, Color.Blue);
		}
	}

	private void Ctrl10()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[0];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time <= 50)
			{
			}
			if (base.Time == 60)
			{
				string fileName = "CS/StEx/关底Boss/1非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_01A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_01B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_01C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_01D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_01A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl9()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[2];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = "CS/StEx/关底Boss/2非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_02A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_02B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_02C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_02D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_02A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl8()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[4];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time <= 150)
			{
			}
			if (base.Time == 160)
			{
				string fileName = "CS/StEx/关底Boss/3非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_03A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_03B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_03C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_03D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_03A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl7()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[6];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = "CS/StEx/关底Boss/4非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_04A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_04B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_04C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_04D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_04A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl6()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[8];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = "CS/StEx/关底Boss/5非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_05A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_05B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_05C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_05D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_05A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl5()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[10];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = "CS/StEx/关底Boss/6非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_06A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_06B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_06C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_06D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_06A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl4()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[12];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 150)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 160)
			{
				string fileName = "CS/StEx/关底Boss/7非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			switch (base.MyPlane.Name)
			{
			case "Reimu":
				new SpellCard_SSSEx_07A(base.StageData);
				break;
			case "Marisa":
				new SpellCard_SSSEx_07B(base.StageData);
				break;
			case "Sanae":
				new SpellCard_SSSEx_07C(base.StageData);
				break;
			case "Koishi":
				new SpellCard_SSSEx_07D(base.StageData);
				break;
			default:
				new SpellCard_SSSEx_07A(base.StageData);
				break;
			}
		}
	}

	private void Ctrl3()
	{
		if (!base.OnSpell)
		{
			base.Armon = base.ArmonArray[14];
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 160);
			}
			else if (base.Time > 50)
			{
				RandomMove(346, 30, 2f, new Rectangle(base.BoundRect.Width / 2 - 60, 100, 120, 40));
			}
			if (base.Time == 60)
			{
				string fileName = "CS/StEx/关底Boss/8非.mbg";
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSSEx_08(base.StageData);
		}
	}

	private void Ctrl2()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 180);
				base.SpellTime -= 1200;
			}
			if (base.Time == 100)
			{
				base.OnSpell = true;
			}
		}
		else if (base.Time == 101)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSSEx_09(base.StageData);
		}
	}

	private void Ctrl1()
	{
		if (!base.OnSpell)
		{
			if (base.Time == 1)
			{
				new Background2DRemover(base.StageData);
				base.DestPoint = new Point(base.BoundRect.Width / 2, 140);
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
			new SpellCard_SSSEx_10(base.StageData);
		}
	}

	public override void HitCheckAll()
	{
		if (base.OnSpell && base.StageData.SpellList.Count > 0 && base.MyPlane.Name != "Koishi")
		{
			foreach (BaseObject x in SlimDX.Compat.ListCompat.LegacyItems(base.StageData.SpellList)){
				x.Damage = 0;
			};
		}
		else
		{
			base.HitCheckAll();
		}
	}
}
