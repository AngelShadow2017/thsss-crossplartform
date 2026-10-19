using System;
using System.Collections.Generic;
using System.Drawing;
using SlimDX;

namespace Shooting;

public class BaseMyPlane : BaseObject
{
	private const int unmatchedTime = 300;

	private const int deltaGreen = 120;

	private int life = 2;

	private int spell = 2;

	private int lifeChip = 0;

	private int spellChip = 0;

	public readonly int nextSpellChip = 4;

	public readonly int[] nextLifeChip = new int[9] { 3, 5, 8, 10, 12, 15, 18, 20, 22 };

	private int lifeUpCount = 0;

	private int power = 100;

	private long score = 0L;

	private long hiScore = 0L;

	private bool lastX = false;

	private int starPoint = 0;

	public readonly int maxStarPoint = 3000;

	private int nextBluePoint = 300;

	private int nextGreenPoint = 3000;

	private int bluePoint = 0;

	private int greenPoint = 0;

	private int blueLevel = 0;

	private int greenLevel = 0;

	private EnchantmentType enchantmentState = EnchantmentType.None;

	private EnchantmentType lastColor = EnchantmentType.Green;

	private int enchantmentTime;

	private float rate = 1f;

	private int enchantmentCount = 0;

	private int enchantmentCountNeeded = 0;

	private bool OnRotate = false;

	private int Angle3D = 0;

	public string Name { get; set; }

	public string WeaponType { get; set; }

	public string FullName => Name + WeaponType;

	public bool SlowMove { get; private set; }

	public float HighSpeed { get; set; }

	public float LowSpeed { get; set; }

	public new float Vx { get; private set; }

	public new float Vy { get; private set; }

	public int DeadTime { get; set; }

	public int Life
	{
		get
		{
			return life;
		}
		set
		{
			if (value > 8)
			{
				life = 8;
			}
			else
			{
				life = value;
			}
		}
	}

	public int Spell
	{
		get
		{
			return spell;
		}
		set
		{
			if (value > 8)
			{
				spell = 8;
			}
			else if (value < 0)
			{
				spell = 0;
			}
			else
			{
				spell = value;
			}
		}
	}

	public int LifeChip
	{
		get
		{
			return lifeChip;
		}
		set
		{
			lifeChip = value;
			while (lifeChip >= nextLifeChip[lifeUpCount])
			{
				Extend();
				lifeChip -= nextLifeChip[lifeUpCount];
				lifeUpCount++;
			}
		}
	}

	public int SpellChip
	{
		get
		{
			return spellChip;
		}
		set
		{
			for (spellChip = value; spellChip >= nextSpellChip; spellChip -= nextSpellChip)
			{
				SpellExtand();
			}
		}
	}

	public int Power
	{
		get
		{
			return power;
		}
		set
		{
			if (value > 400)
			{
				power = 400;
			}
			else if (value < 100)
			{
				power = 100;
			}
			else
			{
				power = value;
			}
		}
	}

	public int PowerLevel => power / 100;

	public long Score
	{
		get
		{
			return score;
		}
		set
		{
			if (value > 9999999999L)
			{
				score = 9999999999L;
			}
			else if (value < 0)
			{
				score = 0L;
			}
			else
			{
				score = value;
			}
		}
	}

	public long HiScore
	{
		get
		{
			if (score > hiScore)
			{
				return score;
			}
			return hiScore;
		}
		set
		{
			if (value > 9999999999L)
			{
				hiScore = 9999999999L;
			}
			else if (value < 0)
			{
				hiScore = 0L;
			}
			else
			{
				hiScore = value;
			}
		}
	}

	public int BluePoint
	{
		get
		{
			return bluePoint;
		}
		set
		{
			bluePoint = value;
		}
	}

	public int GreenPoint
	{
		get
		{
			return greenPoint;
		}
		set
		{
			greenPoint = value;
		}
	}

	public int NextBluePoint
	{
		get
		{
			return nextBluePoint;
		}
		set
		{
			nextBluePoint = value;
		}
	}

	public int NextGreenPoint
	{
		get
		{
			return nextGreenPoint;
		}
		set
		{
			nextGreenPoint = value;
		}
	}

	public int StarPoint
	{
		get
		{
			return starPoint;
		}
		set
		{
			if (value >= maxStarPoint)
			{
				if (starPoint < maxStarPoint)
				{
					if (LastColor == EnchantmentType.Red)
					{
						ChangeEnchantment(EnchantmentType.Red);
					}
					else if (LastColor == EnchantmentType.Blue)
					{
						ChangeEnchantment(EnchantmentType.Blue);
					}
					else if (LastColor == EnchantmentType.Green)
					{
						ChangeEnchantment(EnchantmentType.Green);
					}
					base.StageData.SoundPlay("se_focusrot.wav");
					BaseBackground baseBackground = new BaseBackground(base.StageData, "pl02b", base.Position, 0f, 0.0);
					baseBackground.ScaleVelocity = 1f;
					baseBackground.LifeTime = 26;
					baseBackground.TransparentVelocity = -10f;
					baseBackground.Active = false;
					baseBackground.ColorValue = Color.Black;
					BaseBackground baseBackground2 = new BaseBackground(base.StageData, "pl02b", base.Position, 0f, 0.0);
					baseBackground2.ScaleVelocity = 1f;
					baseBackground2.LifeTime = 26;
					baseBackground2.TransparentVelocity = -10f;
					baseBackground2.Active = false;
					baseBackground2.ColorValue = Color.Black;
					BaseBackground baseBackground3 = new BaseBackground(base.StageData, "pl02b", base.Position, 0f, 0.0);
					baseBackground3.ScaleVelocity = 1f;
					baseBackground3.LifeTime = 26;
					baseBackground3.TransparentVelocity = -10f;
					baseBackground3.Active = false;
					baseBackground3.ColorValue = Color.Black;
					new EnchantmentMagicCircle(base.StageData, base.OriginalPosition);
				}
				starPoint = maxStarPoint;
			}
			else if (value < 0)
			{
				starPoint = 0;
			}
			else
			{
				starPoint = value;
			}
		}
	}

	public EnchantmentType EnchantmentState
	{
		get
		{
			return enchantmentState;
		}
		set
		{
			enchantmentState = value;
		}
	}

	public EnchantmentType LastColor
	{
		get
		{
			return lastColor;
		}
		set
		{
			lastColor = value;
		}
	}

	public int EnchantmentTime
	{
		get
		{
			return enchantmentTime;
		}
		set
		{
			if (value > 0)
			{
				enchantmentTime = value;
				return;
			}
			if (enchantmentTime > 0)
			{
				if (enchantmentTime == 1)
				{
					EnchantmentCount++;
					Rate += 0.1f;
					long num = 0L;
					if (enchantmentState == EnchantmentType.Blue)
					{
						num = HighItemScore * 20;
					}
					else if (enchantmentState == EnchantmentType.Green)
					{
						num = 1000000L;
					}
					else if (enchantmentState == EnchantmentType.Red)
					{
						num = 5120000L;
					}
					score += num;
					new EnchantmentBonus(base.StageData, num);
					base.StageData.SoundPlay("se_cardget.wav");
				}
				else
				{
					BaseEffect baseEffect = new BaseEffect(base.StageData, "pl02b", base.Position, 0f, 0.0);
					baseEffect.ScaleVelocity = 1f;
					baseEffect.LifeTime = 26;
					baseEffect.TransparentVelocity = -10f;
					baseEffect.Active = false;
					baseEffect.ColorValue = Color.Black;
					BaseEffect baseEffect2 = new BaseEffect(base.StageData, "pl02b", base.Position, 0f, 0.0);
					baseEffect2.ScaleVelocity = 1f;
					baseEffect2.LifeTime = 26;
					baseEffect2.TransparentVelocity = -10f;
					baseEffect2.Active = false;
					baseEffect2.ColorValue = Color.Black;
					BaseEffect baseEffect3 = new BaseEffect(base.StageData, "pl02b", base.Position, 0f, 0.0);
					baseEffect3.ScaleVelocity = 1f;
					baseEffect3.LifeTime = 26;
					baseEffect3.TransparentVelocity = -10f;
					baseEffect3.Active = false;
					baseEffect3.ColorValue = Color.Black;
				}
				if (enchantmentState == EnchantmentType.Green)
				{
					base.Time = UnmatchedTime - 60;
					DeadTime = -1;
					new SpellDisable(base.StageData);
				}
				StarPoint = 0;
				base.StageData.SoundPlay("ice005.wav");
				TextureObject txtureObject = base.TextureObjectDictionary["White"];
				int num2 = 16;
				int num3 = 8;
				Color color = Color.White;
				if (enchantmentState == EnchantmentType.Blue)
				{
					color = Color.Blue;
				}
				else if (enchantmentState == EnchantmentType.Green)
				{
					color = Color.Green;
				}
				else if (enchantmentState == EnchantmentType.Red)
				{
					color = Color.Red;
				}
				for (int i = 0; i < num2; i++)
				{
					for (int j = 0; j < num3; j++)
					{
						Model_Triangle model = new Model_Triangle(base.StageData.DeviceMain, txtureObject, i + "_" + j, new Vector2((float)i / (float)num2, (float)j / (float)num3), new Vector2((float)(i + 1) / (float)num2, (float)j / (float)num3), new Vector2((float)i / (float)num2, (float)(j + 1) / (float)num3), 640 / num2, 480 / num3);
						BaseParticle3D baseParticle3D = new BaseParticle3D();
						baseParticle3D.StageData = base.StageData;
						baseParticle3D.Position = new PointF(i * 640 / num2, j * 480 / num3);
						baseParticle3D.model = model;
						baseParticle3D.Velocity = (float)base.Ran.Next(20) / 10f;
						baseParticle3D.LifeTime = 30;
						baseParticle3D.DirectionDegree = base.Ran.Next(360);
						baseParticle3D.RotatingAxis = new Vector3(base.Ran.Next(-100, 100), base.Ran.Next(-100, 100), base.Ran.Next(-100, 100));
						baseParticle3D.AngularVelocity3D = (float)base.Ran.Next(20) / 100f;
						baseParticle3D.TransparentVelocity = -6f;
						baseParticle3D.ScaleVelocity = -0.005f;
						baseParticle3D.ColorValue = color;
						baseParticle3D.TransparentValueF = 150f;
						base.StageData.Particle3D.ParticleList.Add(baseParticle3D);
					}
				}
				for (int i = 0; i < num2; i++)
				{
					for (int j = 0; j < num3; j++)
					{
						Model_Triangle model = new Model_Triangle(base.StageData.DeviceMain, txtureObject, i + "_" + j, new Vector2((float)(i + 1) / (float)num2, (float)(j + 1) / (float)num3), new Vector2((float)i / (float)num2, (float)(j + 1) / (float)num3), new Vector2((float)(i + 1) / (float)num2, (float)j / (float)num3), 640 / num2, 480 / num3);
						BaseParticle3D baseParticle3D = new BaseParticle3D();
						baseParticle3D.StageData = base.StageData;
						baseParticle3D.Position = new PointF((i + 1) * 640 / num2, (j + 1) * 480 / num3);
						baseParticle3D.model = model;
						baseParticle3D.Angle = Math.PI;
						baseParticle3D.Velocity = (float)base.Ran.Next(20) / 10f;
						baseParticle3D.LifeTime = 30;
						baseParticle3D.DirectionDegree = base.Ran.Next(360);
						baseParticle3D.RotatingAxis = new Vector3(base.Ran.Next(-100, 100), base.Ran.Next(-100, 100), base.Ran.Next(-100, 100));
						baseParticle3D.AngularVelocity3D = (float)base.Ran.Next(20) / 100f;
						baseParticle3D.TransparentVelocity = -6f;
						baseParticle3D.ScaleVelocity = -0.005f;
						baseParticle3D.ColorValue = color;
						baseParticle3D.TransparentValueF = 150f;
						base.StageData.Particle3D.ParticleList.Add(baseParticle3D);
					}
				}
			}
			enchantmentTime = 0;
			enchantmentState = EnchantmentType.None;
		}
	}

	public int EnchantmentCount
	{
		get
		{
			return enchantmentCount;
		}
		set
		{
			enchantmentCount = value;
		}
	}

	public int EnchantmentCountNeeded
	{
		get
		{
			return enchantmentCountNeeded;
		}
		set
		{
			enchantmentCountNeeded = value;
		}
	}

	public bool FscEnabled => EnchantmentCount >= EnchantmentCountNeeded && base.Difficulty > DifficultLevel.Normal;

	public float Rate
	{
		get
		{
			return rate;
		}
		set
		{
			rate = value;
		}
	}

	public int HighItemScore { get; set; }

	public int Graze { get; set; }

	public int UnmatchedTime => 300;

	public TextureObject[] TxtureObjects { get; set; }

	public PointF[] SubPlanePoint { get; set; }

	public List<BaseSubPlane> SubPlaneList { get; set; }

	public bool SpellEnabled { get; set; }

	public int LifeUpCount
	{
		get
		{
			return lifeUpCount;
		}
		set
		{
			lifeUpCount = value;
		}
	}

	public BaseMyPlane()
	{
	}

	public BaseMyPlane(StageDataPackage StageData, Point Position)
	{
		base.StageData = StageData;
		base.Position = Position;
		TxtureObjects = new TextureObject[72];
		for (int i = 0; i < 72; i++)
		{
			TxtureObjects[i] = base.TextureObjectDictionary["MyPlane" + i.ToString("D3")];
		}
		base.TxtureObject = TxtureObjects[0];
		SubPlanePoint = new PointF[4];
		HighSpeed = 4.5f;
		LowSpeed = 2f;
		base.Region = 1;
		base.Time = UnmatchedTime;
		DeadTime = -1;
		SubPlaneList = new List<BaseSubPlane>
		{
			new BaseSubPlane(StageData, "MySubPlane", Position),
			new BaseSubPlane(StageData, "MySubPlane", Position),
			new BaseSubPlane(StageData, "MySubPlane", Position),
			new BaseSubPlane(StageData, "MySubPlane", Position)
		};
		Name = "Plane";
		WeaponType = "A";
		SpellEnabled = true;
		HighItemScore = 100000;
	}

	public virtual void Refresh()
	{
		base.Time = 300;
		EnchantmentCount = 0;
		foreach (BaseSubPlane x in SlimDX.Compat.ListCompat.LegacyItems(SubPlaneList)){
			x.Refresh();
		};
		SpellEnabled = true;
	}

	public override void Move()
	{
		Vx = 0f;
		Vy = 0f;
		float x = base.OriginalPosition.X;
		float y = base.OriginalPosition.Y;
		SlowMove = base.KClass.Key_Shift;
		if (!SlowMove)
		{
			if (base.KClass.ArrowRight)
			{
				Vx += HighSpeed;
			}
			if (base.KClass.ArrowLeft)
			{
				Vx -= HighSpeed;
			}
			if (base.KClass.ArrowUp)
			{
				Vy += HighSpeed;
			}
			if (base.KClass.ArrowDown)
			{
				Vy -= HighSpeed;
			}
		}
		else
		{
			if (base.KClass.ArrowRight)
			{
				Vx += LowSpeed;
			}
			if (base.KClass.ArrowLeft)
			{
				Vx -= LowSpeed;
			}
			if (base.KClass.ArrowUp)
			{
				Vy += LowSpeed;
			}
			if (base.KClass.ArrowDown)
			{
				Vy -= LowSpeed;
			}
		}
		if (Vx != 0f && Vy != 0f)
		{
			Vx = 0.71f * Vx;
			Vy = 0.71f * Vy;
		}
		x += Vx;
		y -= Vy;
		if (x < (float)(base.TxtureObject.Width / 2 - 6))
		{
			x = base.TxtureObject.Width / 2 - 6;
		}
		else if (x > (float)(base.BoundRect.Width - base.TxtureObject.Width / 2 + 6))
		{
			x = base.BoundRect.Width - base.TxtureObject.Width / 2 + 6;
		}
		if (y < (float)(base.TxtureObject.Height / 2))
		{
			y = base.TxtureObject.Height / 2;
		}
		else if (y >= (float)(base.BoundRect.Height - base.TxtureObject.Height / 2 + 8))
		{
			y = base.BoundRect.Height - base.TxtureObject.Height / 2 + 8;
		}
		base.OriginalPosition = new PointF(x, y);
	}

	public virtual void SubPlaneCtrl()
	{
		if (base.KClass.Key_Shift)
		{
			switch (PowerLevel)
			{
			case 1:
			{
				ref PointF reference10 = ref SubPlanePoint[0];
				reference10 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 30f);
				break;
			}
			case 2:
			{
				ref PointF reference8 = ref SubPlanePoint[0];
				reference8 = new PointF(base.OriginalPosition.X - 15f, base.OriginalPosition.Y - 30f);
				ref PointF reference9 = ref SubPlanePoint[1];
				reference9 = new PointF(base.OriginalPosition.X + 15f, base.OriginalPosition.Y - 30f);
				break;
			}
			case 3:
			{
				ref PointF reference5 = ref SubPlanePoint[0];
				reference5 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 30f);
				ref PointF reference6 = ref SubPlanePoint[1];
				reference6 = new PointF(base.OriginalPosition.X + 20f, base.OriginalPosition.Y - 20f);
				ref PointF reference7 = ref SubPlanePoint[2];
				reference7 = new PointF(base.OriginalPosition.X - 20f, base.OriginalPosition.Y - 20f);
				break;
			}
			case 4:
			{
				ref PointF reference = ref SubPlanePoint[0];
				reference = new PointF(base.OriginalPosition.X - 15f, base.OriginalPosition.Y - 40f);
				ref PointF reference2 = ref SubPlanePoint[1];
				reference2 = new PointF(base.OriginalPosition.X + 15f, base.OriginalPosition.Y - 40f);
				ref PointF reference3 = ref SubPlanePoint[2];
				reference3 = new PointF(base.OriginalPosition.X - 30f, base.OriginalPosition.Y - 30f);
				ref PointF reference4 = ref SubPlanePoint[3];
				reference4 = new PointF(base.OriginalPosition.X + 30f, base.OriginalPosition.Y - 30f);
				break;
			}
			}
		}
		else
		{
			switch (PowerLevel)
			{
			case 1:
			{
				ref PointF reference20 = ref SubPlanePoint[0];
				reference20 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 40f);
				break;
			}
			case 2:
			{
				ref PointF reference18 = ref SubPlanePoint[0];
				reference18 = new PointF(base.OriginalPosition.X - 25f, base.OriginalPosition.Y - 20f);
				ref PointF reference19 = ref SubPlanePoint[1];
				reference19 = new PointF(base.OriginalPosition.X + 25f, base.OriginalPosition.Y - 20f);
				break;
			}
			case 3:
			{
				ref PointF reference15 = ref SubPlanePoint[0];
				reference15 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 40f);
				ref PointF reference16 = ref SubPlanePoint[1];
				reference16 = new PointF(base.OriginalPosition.X + 30f, base.OriginalPosition.Y - 20f);
				ref PointF reference17 = ref SubPlanePoint[2];
				reference17 = new PointF(base.OriginalPosition.X - 30f, base.OriginalPosition.Y - 20f);
				break;
			}
			case 4:
			{
				ref PointF reference11 = ref SubPlanePoint[0];
				reference11 = new PointF(base.OriginalPosition.X - 25f, base.OriginalPosition.Y - 0f);
				ref PointF reference12 = ref SubPlanePoint[1];
				reference12 = new PointF(base.OriginalPosition.X + 25f, base.OriginalPosition.Y - 0f);
				ref PointF reference13 = ref SubPlanePoint[2];
				reference13 = new PointF(base.OriginalPosition.X - 50f, base.OriginalPosition.Y + 5f);
				ref PointF reference14 = ref SubPlanePoint[3];
				reference14 = new PointF(base.OriginalPosition.X + 50f, base.OriginalPosition.Y + 5f);
				break;
			}
			}
		}
		for (int i = 0; i < PowerLevel; i++)
		{
			SubPlaneList[i].Enabled = true;
			SubPlaneList[i].DestPoint = SubPlanePoint[i];
			SubPlaneList[i].Ctrl();
		}
		for (int i = PowerLevel; i < SubPlaneList.Count; i++)
		{
			SubPlaneList[i].Enabled = false;
			SubPlaneList[i].Position = base.Position;
		}
	}

	public virtual void TextureCtrl()
	{
		if (Vx > 0.5f)
		{
			if (SlowMove)
			{
				OnRotate = false;
				if (0 <= Angle3D && Angle3D < 30)
				{
					Angle3D += 5;
				}
				else if (Angle3D < 0)
				{
					Angle3D = 0;
				}
			}
			else if (!OnRotate)
			{
				if (0 <= Angle3D && Angle3D < 40)
				{
					Angle3D += 5;
				}
				else if (Angle3D < 0)
				{
					OnRotate = true;
				}
			}
			else
			{
				Angle3D += 13;
			}
		}
		else if (Vx < -0.5f)
		{
			if (SlowMove)
			{
				OnRotate = false;
				if (-30 < Angle3D && Angle3D <= 0)
				{
					Angle3D -= 5;
				}
				else if (Angle3D > 0)
				{
					Angle3D = 0;
				}
			}
			else if (!OnRotate)
			{
				if (-40 < Angle3D && Angle3D <= 0)
				{
					Angle3D -= 5;
				}
				else if (Angle3D > 0)
				{
					OnRotate = true;
				}
			}
			else
			{
				Angle3D -= 13;
			}
		}
		else
		{
			Angle3D /= 2;
			if (-10 < Angle3D && Angle3D < 10)
			{
				Angle3D = 0;
				OnRotate = false;
			}
		}
		if (Angle3D > 180)
		{
			Angle3D -= 360;
		}
		else if (Angle3D < -180)
		{
			Angle3D += 360;
		}
		int num = Angle3D / 5;
		if (num < 0)
		{
			num += 72;
		}
		else if (num > 72)
		{
			num -= 72;
		}
		base.TxtureObject = TxtureObjects[num];
	}

	public override void Shoot()
	{
		if (base.Time % 3 == 0)
		{
			base.StageData.SoundPlay("se_plst00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			float x = base.Position.X;
			float y = base.Position.Y;
			float num = 7f;
			num *= (float)Math.Cos((double)Angle3D * Math.PI / 180.0);
			new BaseMyBullet(base.StageData, "自机子弹", new PointF(x - num, y), 30f, -Math.PI / 2.0);
			new BaseMyBullet(base.StageData, "自机子弹", new PointF(x + num, y), 30f, -Math.PI / 2.0);
		}
		if (base.Time % 5 == 0)
		{
			for (int i = 0; i < PowerLevel; i++)
			{
				SubPlaneList[i].Shoot();
			}
		}
	}

	public virtual void SpellShoot()
	{
		new Spell(base.StageData);
		base.StageData.SoundPlay("se_nep00.wav");
	}

	public virtual void PreMiss()
	{
		if (base.Time < UnmatchedTime || base.Time < DeadTime || base.SpellList.Count != 0)
		{
			return;
		}
		if (EnchantmentState == EnchantmentType.Green)
		{
			BulletRemover_Small bulletRemover_Small = new BulletRemover_Small(base.StageData, base.OriginalPosition);
			bulletRemover_Small.Region = 500;
			ChangeEnchantment(EnchantmentType.None);
			foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.LegacyItems(base.EnemyPlaneList)){
				if (x is BaseSpellCard)
				{
					((BaseSpellCard)x).Missed = true;
				}
			};
		}
		else
		{
			new Emitter(base.StageData, base.MyPlane.Position);
			base.StageData.SoundPlay("se_pldead00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			DeadTime = base.Time + 20;
		}
	}

	public virtual void Miss()
	{
		if (EnchantmentState == EnchantmentType.Red || EnchantmentState == EnchantmentType.Blue)
		{
			ChangeEnchantment(EnchantmentType.None);
		}
		new BulletRemover(base.StageData, base.OriginalPosition);
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height + 100);
		Vx = 0f;
		Vy = 0f;
		Life--;
		Power -= 16;
		base.Time = 0;
		DeadTime = -1;
	}

	public override void Ctrl()
	{
		if (!base.Enabled)
		{
			return;
		}
		base.Time++;
		if (base.Time == DeadTime)
		{
			Miss();
		}
		else if (base.Time > DeadTime)
		{
			if (base.Time == 1 && Spell < 2)
			{
				Spell = 2;
			}
			if (base.Time > 70)
			{
				Move();
			}
			else
			{
				base.DestPoint = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height - 30);
				base.Velocity = 2f;
				base.Move();
				TextureCtrl();
			}
			TextureCtrl();
			base.Direction += 0.019999999552965164;
			SubPlaneCtrl();
			EnchantmentTime--;
			if (EnchantmentState == EnchantmentType.Blue)
			{
				foreach (BaseItem x in SlimDX.Compat.ListCompat.LegacyItems(base.ItemList)){
					if (!(x is ShootingStarShard))
					{
						x.Obtain = true;
						x.Doubled = true;
					}
				};
			}
		}
		if (base.Story != null || base.Time <= 70)
		{
			return;
		}
		if (base.KClass.Key_Z && base.TimeMain > 15)
		{
			Shoot();
		}
		if (base.KClass.Key_X && !lastX)
		{
			if (EnchantmentState == EnchantmentType.Green)
			{
				BulletRemover_Small bulletRemover_Small = new BulletRemover_Small(base.StageData, base.OriginalPosition);
				bulletRemover_Small.Region = 500;
				ChangeEnchantment(EnchantmentType.None);
				foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.LegacyItems(base.EnemyPlaneList)){
					if (x is BaseSpellCard)
					{
						((BaseSpellCard)x).Bombed = true;
					}
				};
			}
			else if (Spell > 0 && base.SpellList.Count == 0 && base.Time > 30 && SpellEnabled)
			{
				SpellShoot();
				Spell--;
			}
		}
		lastX = base.KClass.Key_X;
	}

	public void RetryClear()
	{
		EnchantmentTime = 0;
		Life = 2;
		Spell = 2;
		Power = 400;
		Score = base.StageData.ContinueTimes;
		Graze = 0;
		HiScore = base.StageData.S_History[0].Score;
		LifeChip = 0;
		SpellChip = 0;
		LifeUpCount = 0;
		StarPoint = 0;
		HighItemScore = 100000;
		Rate = 0f;
		LastColor = EnchantmentType.Green;
	}

	public void Extend()
	{
		if (Life < 8)
		{
			Life++;
		}
		else
		{
			Spell++;
		}
		base.StageData.SoundPlay("se_extend.wav");
		BaseEffect baseEffect = new BaseEffect(base.StageData, "Extend", new PointF(0f, 0f), 0f, Math.PI / 2.0);
		baseEffect.Active = false;
		baseEffect.OriginalPosition = new PointF(base.BoundRect.Width / 2, 112f);
		baseEffect.LifeTime = 90;
		baseEffect.TransparentValueF = 0f;
		baseEffect.Layer = 1;
		BaseEffect baseEffect2 = baseEffect;
		baseEffect2.TransparentVelocityDictionary.Add(1, 13f);
		baseEffect2.TransparentVelocityDictionary.Add(70, -13f);
	}

	public void SpellExtand()
	{
		base.StageData.SoundPlay("se_cardget.wav");
		Spell++;
		BaseEffect baseEffect = new BaseEffect(base.StageData, "SpellExtend", new PointF(0f, 0f), 0f, Math.PI / 2.0);
		baseEffect.Active = false;
		baseEffect.OriginalPosition = new PointF(base.BoundRect.Width / 2, 80f);
		baseEffect.LifeTime = 90;
		baseEffect.TransparentValueF = 0f;
		baseEffect.Layer = 1;
		BaseEffect baseEffect2 = baseEffect;
		baseEffect2.TransparentVelocityDictionary.Add(1, 13f);
		baseEffect2.TransparentVelocityDictionary.Add(70, -13f);
	}

	public void ChangeEnchantment(EnchantmentType EnchantmentState)
	{
		switch (EnchantmentState)
		{
		case EnchantmentType.None:
			EnchantmentTime = 0;
			break;
		case EnchantmentType.Red:
			this.EnchantmentState = EnchantmentState;
			EnchantmentTime = 600;
			break;
		case EnchantmentType.Blue:
			this.EnchantmentState = EnchantmentState;
			EnchantmentTime = 600;
			break;
		case EnchantmentType.Green:
			this.EnchantmentState = EnchantmentState;
			EnchantmentTime = 420;
			break;
		default:
			EnchantmentTime = 0;
			break;
		}
	}

	public override void Show()
	{
		if (base.Time >= DeadTime && base.Time != 0)
		{
			if (base.Time < UnmatchedTime && base.Time / 2 % 2 == 0)
			{
				base.SpriteMain.Draw2D(base.TxtureObject, 1f, 0f, base.Position, Color.DarkBlue);
			}
			else
			{
				base.SpriteMain.Draw2D(base.TxtureObject, 1f, 0f, base.Position, Color.White);
			}
			for (int i = 0; i < PowerLevel; i++)
			{
				SubPlaneList[i].Show();
			}
		}
	}

	public virtual void ShowCenter()
	{
		if (SlowMove)
		{
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["MyPlaneCenter1"], 1f, (float)base.Direction * 4f, base.Position, Color.White);
		}
	}
}
