using System;
using System.Drawing;

namespace Shooting;

public class Spell_AyaB : BaseSpell
{
	private PointF tmpPoint;

	private int tmpRegion;

	private int Circle = 32;

	private bool Left = true;

	public Spell_AyaB(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.OriginalPosition = base.MyPlane.OriginalPosition;
		tmpPoint = base.MyPlane.OriginalPosition;
		base.LifeTime = 270;
		base.SpellList.Add(this);
		BaseEffect baseEffect = new BaseEffect(StageData, null, base.Position, 0f, Math.PI / 2.0)
		{
			TxtureObject = base.MyPlane.TxtureObject,
			TransparentVelocity = -20f,
			ScaleVelocity = 0.1f
		};
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
		tmpRegion = base.MyPlane.Region;
		base.MyPlane.Region = -100;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time > 64 && base.Time <= 96)
		{
			Circle = 16;
		}
		else if (base.Time > 96 && base.Time <= 112)
		{
			Circle = 8;
		}
		if (base.Time % Circle == 0)
		{
			Spell_AyaB_Laser spell_AyaB_Laser = new Spell_AyaB_Laser(base.StageData, base.OriginalPosition, Left);
			if (base.Time <= 96)
			{
				BaseEffect baseEffect = new BaseEffect(base.StageData, "Ayaya", new PointF(spell_AyaB_Laser.Position.X + 360f * (float)Math.Cos(spell_AyaB_Laser.Direction), spell_AyaB_Laser.Position.Y + 360f * (float)Math.Sin(spell_AyaB_Laser.Direction)), spell_AyaB_Laser.Velocity, spell_AyaB_Laser.Direction);
				baseEffect.Active = false;
				baseEffect.Mirrored = !Left;
				baseEffect.Scale = 1.5f;
				baseEffect.GhostingCount = 5;
			}
			Left = !Left;
			base.StageData.SoundPlay("se_tan01.wav");
			base.StageData.VibrateStart(8);
		}
		base.ItemList.ForEach(delegate(BaseItem x)
		{
			x.Region = 1000;
			x.Doubled = true;
		});
		if (base.Time == base.LifeTime)
		{
			base.ItemList.ForEach(delegate(BaseItem x)
			{
				x.Obtain = true;
			});
			BaseEffect baseEffect2 = new BaseEffect(base.StageData, null, base.MyPlane.Position, 0f, Math.PI / 2.0);
			baseEffect2.TxtureObject = base.MyPlane.TxtureObject;
			baseEffect2.Scale = 3f;
			baseEffect2.ScaleVelocity = -0.1f;
			baseEffect2.LifeTime = 20;
			baseEffect2.TransparentValueF = 0f;
			baseEffect2.TransparentVelocity = 10f;
			base.MyPlane.Region = tmpRegion;
		}
	}

	public override void HitCheckAll()
	{
	}

	public override void Show()
	{
	}
}
