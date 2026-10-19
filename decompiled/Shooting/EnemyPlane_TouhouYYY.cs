using System.Drawing;

namespace Shooting;

internal class EnemyPlane_TouhouYYY : BaseEnemyPlane_Touhou
{
	private BaseEffect circle0;

	private BaseEffect circle1;

	public bool BackFire { get; set; }

	public Color BackFireColor { get; set; }

	public EnemyPlane_TouhouYYY(StageDataPackage StageData, PointF OriginalPosition, float Velocity, double Direction, byte ColorType)
		: base(StageData, null, OriginalPosition, Velocity, Direction, ColorType)
	{
		base.HealthPoint = 30f;
		base.Region = 8;
		base.TxtureObject = base.TextureObjectDictionary["EnemyYYY" + (ColorType + 4) + "_1"];
		base.AngularVelocityDegree = 5f;
	}

	public override void Shoot()
	{
	}

	public override void TextureCtrl()
	{
	}

	public override void GiveEndEffect()
	{
		base.GiveEndEffect();
		base.EffectList.Remove(circle0);
		base.EffectList.Remove(circle1);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 1)
		{
			circle0 = new BaseEffect(base.StageData, "EnemyYYY" + (base.ColorType + 4) + "_0", base.Position, 0f, 0.0)
			{
				Active = false,
				AngularVelocityDegree = -5f,
				AngleDegree = 90.0
			};
			circle1 = new BaseEffect(base.StageData, "EnemyYYY" + (base.ColorType + 4) + "_1", base.Position, 0f, 0.0)
			{
				Scale = 1.5f,
				AngularVelocityDegree = -10f
			};
			circle0.SetBinding(this);
			circle1.SetBinding(this);
		}
		if (OutBoundary())
		{
			base.EffectList.Remove(circle0);
			base.EffectList.Remove(circle1);
		}
	}
}
