using System;
using System.Drawing;

namespace Shooting;

internal class Boss_Freya07 : BaseBossTouhou
{
	private bool ShowWings { get; set; }

	private TextureObject TxtureObject2 { get; set; }

	private TextureObject[] TxtureObjects2 { get; set; }

	private int IndexX2 { get; set; }

	public Boss_Freya07(StageDataPackage StageData)
		: base(StageData)
	{
		base.TxtureObjects = new TextureObject[3, 4]
		{
			{
				base.TextureObjectDictionary["BossFreya-00"],
				base.TextureObjectDictionary["BossFreya-01"],
				base.TextureObjectDictionary["BossFreya-02"],
				base.TextureObjectDictionary["BossFreya-03"]
			},
			{
				base.TextureObjectDictionary["BossFreya-10"],
				base.TextureObjectDictionary["BossFreya-11"],
				base.TextureObjectDictionary["BossFreya-12"],
				base.TextureObjectDictionary["BossFreya-13"]
			},
			{
				base.TextureObjectDictionary["BossFreya-00"],
				base.TextureObjectDictionary["BossFreya-01"],
				base.TextureObjectDictionary["BossFreya-02"],
				base.TextureObjectDictionary["BossFreya-03"]
			}
		};
		TxtureObjects2 = new TextureObject[8]
		{
			base.TextureObjectDictionary["BossFreyaWing-00"],
			base.TextureObjectDictionary["BossFreyaWing-01"],
			base.TextureObjectDictionary["BossFreyaWing-02"],
			base.TextureObjectDictionary["BossFreyaWing-03"],
			base.TextureObjectDictionary["BossFreyaWing-04"],
			base.TextureObjectDictionary["BossFreyaWing-05"],
			base.TextureObjectDictionary["BossFreyaWing-06"],
			base.TextureObjectDictionary["BossFreyaWing-07"]
		};
		base.TxtureObject = base.TxtureObjects[0, 0];
		TxtureObject2 = TxtureObjects2[0];
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
		ShowWings = true;
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
			IndexX2 = base.TimeMain % 32 / 8;
		}
		if (num != (base.Mirrored ? base.IndexY : (-base.IndexY)))
		{
			base.IndexX = 0f;
			IndexX2 = 0;
		}
		if (base.IndexX > 3f)
		{
			base.IndexX = 3f;
		}
		if (IndexX2 > 7)
		{
			IndexX2 = 7;
		}
		base.TxtureObject = base.TxtureObjects[base.IndexY, (int)base.IndexX];
		TxtureObject2 = TxtureObjects2[IndexX2];
	}

	public override void GiveEndEffect()
	{
		base.TransparentValueF = 255f;
		new Background2DRemover(base.StageData);
		base.Background3D.WarpEnabled = false;
		base.LifeTime = base.Time + 150;
		base.Region = 0;
		base.StageData.SoundPlay("se_tan01.wav");
		new EndBoss_Touhou01(base.StageData, base.OriginalPosition, Color.LightPink, Color.Pink);
	}

	private void Ctrl3()
	{
		if (!base.OnSpell)
		{
			base.Armon = 0f;
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
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St02\\关底Boss\\1非E.mbg", 
					1 => ".\\CS\\St02\\关底Boss\\1非N.mbg", 
					2 => ".\\CS\\St02\\关底Boss\\1非H.mbg", 
					3 => ".\\CS\\St02\\关底Boss\\1非L.mbg", 
					_ => ".\\CS\\St02\\关底Boss\\1非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS02_02(base.StageData);
		}
	}

	private void Ctrl2()
	{
		if (!base.OnSpell)
		{
			ShowWings = false;
			if (base.Time == 100)
			{
				base.StageData.SoundPlay("se_boon01.wav");
				new EmitterGiveOutEnegy3D(base.StageData, base.Boss.OriginalPosition, Color.SkyBlue);
				new BackgroundWing(base.StageData, "Wing", base.Boss.OriginalPosition, 0f, 0.0, 0);
				BackgroundWing backgroundWing = new BackgroundWing(base.StageData, "Wing", base.Boss.OriginalPosition, 0f, Math.PI, 0);
				backgroundWing.Mirrored = true;
				base.StageData.VibrateStart(50);
			}
			base.Armon = 0.3f;
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
				string fileName = (int)base.Difficulty switch
				{
					0 => ".\\CS\\St02\\关底Boss\\2非E.mbg", 
					1 => ".\\CS\\St02\\关底Boss\\2非N.mbg", 
					2 => ".\\CS\\St02\\关底Boss\\2非H.mbg", 
					3 => ".\\CS\\St02\\关底Boss\\2非L.mbg", 
					_ => ".\\CS\\St02\\关底Boss\\2非L.mbg", 
				};
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.BossBinding = true;
			}
		}
		else if (base.Time == 1)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS02_03(base.StageData);
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
			if (base.Time == 150)
			{
				base.OnSpell = true;
				base.SpellTime = 3000;
			}
		}
		else if (base.Time == 151)
		{
			new BulletRemover2(base.StageData, base.OriginalPosition);
			new SpellCard_SSS02_04(base.StageData);
		}
	}

	public override void GiveItems()
	{
		PointF originalPosition;
		for (int i = 0; i < 15; i++)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-49, 50), base.OriginalPosition.Y + (float)base.Ran.Next(-49, 50));
			new PowerItem_Touhou(base.StageData, originalPosition);
		}
		for (int i = 0; i < 20; i++)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-49, 50), base.OriginalPosition.Y + (float)base.Ran.Next(-49, 50));
			new ScoreItem_Touhou(base.StageData, originalPosition);
		}
	}

	public override void Show()
	{
		if (ShowWings && TxtureObject2 != null)
		{
			base.SpriteMain.Draw2D(TxtureObject2, base.ScaleWidth, base.ScaleLength, (float)(base.Angle - Math.PI / 2.0), base.Position, Color.FromArgb(base.TransparentValue, base.ColorValue), base.Mirrored);
		}
		base.Show();
	}
}
