using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

[Serializable]
public class BaseEmitter_CS : BaseEnemyPlane
{
	public bool BindingState { get; set; }

	public int BindingID { get; set; }

	public bool BindWithDirection { get; set; }

	public int StartTime { get; set; }

	public int Duration { get; set; }

	public PointF EmitPoint { get; set; }

	public float EmitRadius { get; set; }

	public double RadiusDirection { get; set; }

	public PointF RadiusDirectionPoint { get; set; }

	public int Way { get; set; }

	public int Circle { get; set; }

	public double EmitDirection { get; set; }

	public PointF EmitDirectionPoint { get; set; }

	public double Range { get; set; }

	public bool RDirectionWithDirection { get; set; }

	public float RanX { get; set; }

	public float RanY { get; set; }

	public float RanRadius { get; set; }

	public double RanRadiusDirection { get; set; }

	public int RanWay { get; set; }

	public int RanCircle { get; set; }

	public double RanEmitDirection { get; set; }

	public double RanRange { get; set; }

	public bool DeepBinding { get; set; }

	public bool RangeShoot { get; set; }

	public bool MotionBinding { get; set; }

	public string SoundName { get; set; }

	public bool SpecifySE { get; set; }

	public int EffectType { get; set; }

	public int Count { get; set; }

	public float DeltaV { get; set; }

	public CS_Data CSData { get; set; }

	private BaseBullet_CS CSBullet { get; set; }

	private BaseStraightLaser_CS CSLaserS { get; set; }

	private BaseRadialLaser_CS CSLaserR { get; set; }

	private BaseBendLaser_CS CSLaserB { get; set; }

	private BaseEnemyPlane_Touhou CSEnemy { get; set; }

	private BaseEffect_CS CSEffect { get; set; }

	public EmitterMode EmitterMode { get; set; }

	public BaseObject_CS SubBullet
	{
		get
		{
			if (EmitterMode == EmitterMode.Enemy)
			{
				return CSEnemy;
			}
			if (EmitterMode == EmitterMode.Effect)
			{
				return CSEffect;
			}
			if (EmitterMode == EmitterMode.StraightLaser)
			{
				return CSLaserS;
			}
			if (EmitterMode == EmitterMode.RadialLaser)
			{
				return CSLaserR;
			}
			if (EmitterMode == EmitterMode.BendLaser)
			{
				return CSLaserB;
			}
			return CSBullet;
		}
		set
		{
			if (EmitterMode == EmitterMode.Enemy)
			{
				CSEnemy = (BaseEnemyPlane_Touhou)value;
			}
			else if (EmitterMode == EmitterMode.Effect)
			{
				CSEffect = (BaseEffect_CS)value;
			}
			else if (EmitterMode == EmitterMode.StraightLaser)
			{
				CSLaserS = (BaseStraightLaser_CS)value;
			}
			else if (EmitterMode == EmitterMode.RadialLaser)
			{
				CSLaserR = (BaseRadialLaser_CS)value;
			}
			else if (EmitterMode == EmitterMode.BendLaser)
			{
				CSLaserB = (BaseBendLaser_CS)value;
			}
			else
			{
				CSBullet = (BaseBullet_CS)value;
			}
		}
	}

	public BaseEmitter_CS()
	{
	}

	public BaseEmitter_CS(StageDataPackage StageData, CS_Data CSData, EmitterMode EmitterMode)
	{
		base.StageData = StageData;
		this.CSData = CSData;
		base.EventGroupList = new List<EventGroup>();
		base.EventsExecutionList = new List<Execution>();
		base.HealthPoint = 1000000f;
		base.HitEnabled = false;
		base.Region = 0;
		Count = 1;
		this.EmitterMode = EmitterMode;
		switch (EmitterMode)
		{
		case EmitterMode.Bullet:
			CSBullet = new BaseBullet_CS(StageData);
			break;
		case EmitterMode.StraightLaser:
			CSLaserS = new BaseStraightLaser_CS(StageData);
			break;
		case EmitterMode.RadialLaser:
			CSLaserR = new BaseRadialLaser_CS(StageData);
			break;
		case EmitterMode.BendLaser:
			CSLaserB = new BaseBendLaser_CS(StageData);
			break;
		case EmitterMode.Effect:
			CSEffect = new BaseEffect_CS(StageData);
			break;
		}
	}

	public override void Ctrl()
	{
		if (base.Time == StartTime)
		{
			base.Velocity += (float)((double)base.RanVelocity * base.Ran.NextPMDouble());
			base.DirectionDegree += base.RanDirection * base.Ran.NextPMDouble();
			base.AccelerateCS += (float)((double)base.RanAccelerate * base.Ran.NextPMDouble());
			base.AccDirection += (float)(base.RanAccDirection * base.Ran.NextPMDouble());
		}
		if (base.LifeTime > StartTime + Duration)
		{
			base.LifeTime = StartTime + Duration;
		}
		if (Range != 360.0)
		{
			RangeShoot = true;
		}
		if (BindingObj != null && BindingObj is BaseEnemyPlane_Touhou && BindingObj.HealthPoint <= 0f)
		{
			base.EnemyPlaneList.Remove(this);
		}
		base.Ctrl();
	}

	public override void EventCtrl()
	{
		foreach (EventGroup a in SlimDX.Compat.ListCompat.LegacyItems(base.EventGroupList)){
			a.Update(this);
		};
		foreach (Execution a in SlimDX.Compat.ListCompat.LegacyItems(base.EventsExecutionList)){
			a.Update(this);
		};
	}

	public override void Shoot()
	{
		if (base.Time >= StartTime && base.Time <= StartTime + Duration)
		{
			int num = Circle + (int)((double)RanCircle * base.Ran.NextPMDouble());
			num = ((num < 1) ? 1 : num);
			if (!DeepBinding && (base.Time - StartTime + 1) % num == 0)
			{
				ShootBullet();
			}
			else if (DeepBinding && base.Time % num == 0)
			{
				ShootBullet();
			}
		}
	}

	public void ShootBullet()
	{
		float x = EmitPoint.X;
		float y = EmitPoint.Y;
		x = ((x == -99999f) ? base.MyPlane.OriginalPosition.X : ((x != -99998f) ? (x - 320f + 192f) : base.OriginalPosition.X));
		y = ((y == -99999f) ? base.MyPlane.OriginalPosition.Y : ((y != -99998f) ? (y - 240f + 224f) : base.OriginalPosition.Y));
		if (EmitterMode == EmitterMode.StraightLaser || EmitterMode == EmitterMode.RadialLaser || EmitterMode == EmitterMode.BendLaser)
		{
			x = base.OriginalPosition.X;
			y = base.OriginalPosition.Y;
		}
		PointF originalPosition = new PointF(x + (float)((double)RanX * base.Ran.NextPMDouble()), y + (float)((double)RanY * base.Ran.NextPMDouble()));
		double edi;
		if (EmitDirection == -99999.0)
		{
			edi = GetDirection(base.MyPlane) + (RanEmitDirection * base.Ran.NextPMDouble() + SubBullet.RanDirection * base.Ran.NextPMDouble()) * Math.PI / 180.0;
		}
		else
		{
			edi = (EmitDirection + RanEmitDirection * base.Ran.NextPMDouble() + SubBullet.RanDirection * base.Ran.NextPMDouble()) * Math.PI / 180.0;
		}
		double num = ((RadiusDirection != -99999.0) ? ((RadiusDirection + RanRadiusDirection * base.Ran.NextPMDouble()) * Math.PI / 180.0) : (GetDirection(base.MyPlane) + RanRadiusDirection * base.Ran.NextPMDouble() * Math.PI / 180.0));
		float num2 = EmitRadius + (float)((double)RanRadius * base.Ran.NextPMDouble());
		int num3 = Way + (int)((double)RanWay * base.Ran.NextPMDouble());
		float num4 = SubBullet.Velocity + (float)((double)SubBullet.RanVelocity * base.Ran.NextPMDouble());
		float accelerateCS = SubBullet.AccelerateCS + (float)((double)SubBullet.RanAccelerate * base.Ran.NextPMDouble());
		double accDirection = SubBullet.AccDirection + SubBullet.RanAccDirection;
		double num5 = Range + (double)(float)(RanRange * base.Ran.NextPMDouble());
		if (RDirectionWithDirection)
		{
			num += edi;
		}
		double num6 = num5 * Math.PI / 180.0 / (double)num3;
		edi -= (double)(num3 - 1) * num6 / 2.0;
		num -= (double)(num3 - 1) * num6 / 2.0;
		if (EffectType == 2)
		{
			new EmitterSaveEnegy3D(base.StageData, originalPosition, CSEffect.ColorValue);
			base.StageData.SoundPlay("se_ch02.wav");
			return;
		}
		if (EffectType == 3)
		{
			new EmitterGiveOutEnegy3D(base.StageData, originalPosition, CSEffect.ColorValue);
			base.StageData.SoundPlay("se_cat00.wav");
			base.StageData.SoundPlay("se_enep02.wav");
			return;
		}
		PointF p = default(PointF);
		for (int i = 0; i < num3; i++)
		{
			ref PointF reference = ref p;
			reference = new PointF((float)((double)originalPosition.X + (double)num2 * Math.Cos(num)), (float)((double)originalPosition.Y + (double)num2 * Math.Sin(num)));
			for (int j = 0; j < Count; j++)
			{
				if (EmitterMode == EmitterMode.Bullet || EmitterMode == EmitterMode.StraightLaser || EmitterMode == EmitterMode.RadialLaser || EmitterMode == EmitterMode.BendLaser)
				{
					BaseBullet_Touhou b = new BaseBullet_Touhou(base.StageData);
					if (CSBullet != null)
					{
						b = (BaseBullet_CS)CSBullet.Clone();
						b.OriginalPosition = p;
						b.GhostingCount = b.GhostingCount;
						b.AngleDegree += CSBullet.RanAngle * base.Ran.NextPMDouble();
					}
					else if (CSLaserS != null)
					{
						b = (BaseStraightLaser_CS)CSLaserS.Clone();
						b.OriginalPosition = p;
						b.Angle = -Math.PI / 2.0;
						b.Active = true;
					}
					else if (CSLaserR != null)
					{
						b = (BaseRadialLaser_CS)CSLaserR.Clone();
						b.OriginalPosition = p;
						b.Angle = Math.PI / 2.0;
						b.UnRemoveable = true;
						b.Active = true;
					}
					else if (CSLaserB != null)
					{
						b = (BaseBendLaser_CS)CSLaserB.Clone();
						b.OriginalPosition = p;
						b.UnRemoveable = true;
						b.Active = true;
					}
					b.GhostingCount = b.GhostingCount;
					b.Velocity = num4 - (float)j * DeltaV;
					b.Direction = edi;
					b.AccelerateCS = accelerateCS;
					b.AccDirection = accDirection;
					b.ID = base.ID;
					b.LayerID = base.LayerID;
					if (MotionBinding)
					{
						b.SetBinding(this);
					}
					base.BulletList.Add(b);
					if (EmitterMode != EmitterMode.Bullet)
					{
						continue;
					}
					foreach (BaseEmitter_CS em in SlimDX.Compat.ListCompat.LegacyItems(CSData.EmitterList)){
						if (em.BindingID == base.ID)
						{
							b.UnRemoveable = true;
							BaseEmitter_CS baseEmitter_CS = (BaseEmitter_CS)em.Clone();
							base.StageData.EnemyPlaneList.Add(baseEmitter_CS);
							baseEmitter_CS.OriginalPosition = p;
							baseEmitter_CS.LifeTime = SubBullet.LifeTime;
							baseEmitter_CS.ColorValue = SubBullet.ColorValue;
							baseEmitter_CS.TransparentValueF = SubBullet.TransparentValueF;
							baseEmitter_CS.Direction = edi;
							baseEmitter_CS.DestPoint = SubBullet.DestPoint;
							baseEmitter_CS.Active = SubBullet.Active;
							baseEmitter_CS.OutBound = SubBullet.OutBound;
							if (baseEmitter_CS.BindWithDirection)
							{
								baseEmitter_CS.EmitDirection += edi * 180.0 / Math.PI;
							}
							baseEmitter_CS.SetBinding(b);
							if (!baseEmitter_CS.DeepBinding)
							{
								baseEmitter_CS.Time = base.Time;
								baseEmitter_CS.LifeTime = Math.Min(SubBullet.LifeTime + base.Time, em.StartTime + em.Duration);
							}
						}
					};
				}
				else if (EmitterMode == EmitterMode.Enemy)
				{
					BaseEnemyPlane_Touhou enemy = (BaseEnemyPlane_Touhou)SubBullet.Clone();
					enemy.LifeTime = 0;
					enemy.OriginalPosition = p;
					enemy.GhostingCount = enemy.GhostingCount;
					enemy.Velocity = num4 - (float)j * DeltaV;
					enemy.Direction = edi;
					enemy.AccelerateCS = accelerateCS;
					enemy.AccDirection = accDirection;
					enemy.ID = base.ID;
					enemy.LayerID = base.LayerID;
					if (MotionBinding)
					{
						enemy.SetBinding(this);
					}
					base.EnemyPlaneList.Add(enemy);
					foreach (BaseEmitter_CS em in SlimDX.Compat.ListCompat.LegacyItems(CSData.EmitterList)){
						if (em.BindingID == base.ID)
						{
							BaseEmitter_CS baseEmitter_CS = (BaseEmitter_CS)em.Clone();
							base.StageData.EnemyPlaneList.Add(baseEmitter_CS);
							baseEmitter_CS.OriginalPosition = p;
							baseEmitter_CS.LifeTime = SubBullet.LifeTime;
							baseEmitter_CS.ColorValue = SubBullet.ColorValue;
							baseEmitter_CS.TransparentValueF = SubBullet.TransparentValueF;
							baseEmitter_CS.Direction = edi;
							baseEmitter_CS.DestPoint = SubBullet.DestPoint;
							baseEmitter_CS.Active = SubBullet.Active;
							baseEmitter_CS.OutBound = SubBullet.OutBound;
							baseEmitter_CS.SetBinding(enemy);
							if (baseEmitter_CS.BindWithDirection)
							{
								baseEmitter_CS.EmitDirection += edi * 180.0 / Math.PI;
							}
							if (!baseEmitter_CS.DeepBinding)
							{
								baseEmitter_CS.Time = base.Time;
								baseEmitter_CS.LifeTime = Math.Min(SubBullet.LifeTime + base.Time, em.StartTime + em.Duration);
							}
						}
					};
				}
				else if (EmitterMode == EmitterMode.Effect)
				{
					BaseEffect_CS baseEffect_CS = (BaseEffect_CS)CSEffect.Clone();
					baseEffect_CS.OriginalPosition = p;
					baseEffect_CS.GhostingCount = baseEffect_CS.GhostingCount;
					baseEffect_CS.AngleDegree += CSEffect.RanAngle * base.Ran.NextPMDouble();
					baseEffect_CS.Velocity = num4 - (float)j * DeltaV;
					baseEffect_CS.Direction = edi;
					baseEffect_CS.AccelerateCS = accelerateCS;
					baseEffect_CS.AccDirection = accDirection;
					baseEffect_CS.ID = base.ID;
					baseEffect_CS.LayerID = base.LayerID;
					if (MotionBinding)
					{
						baseEffect_CS.SetBinding(this);
					}
					base.EffectList.Add(baseEffect_CS);
				}
			}
			num += num6;
			edi += num6;
			if (SpecifySE)
			{
				base.StageData.SoundPlay(SoundName);
			}
			else if (EmitterMode == EmitterMode.Bullet)
			{
				base.StageData.SoundPlay("se_tan00a.wav", originalPosition.X / (float)base.BoundRect.Width);
			}
			else if (EmitterMode == EmitterMode.StraightLaser || EmitterMode == EmitterMode.RadialLaser)
			{
				base.StageData.SoundPlay("se_lazer00.wav", originalPosition.X / (float)base.BoundRect.Width);
			}
		}
	}

	public override object Clone()
	{
		BaseEmitter_CS baseEmitter_CS = (BaseEmitter_CS)base.Clone();
		if (CSBullet != null)
		{
			baseEmitter_CS.CSBullet = (BaseBullet_CS)CSBullet.Clone();
		}
		if (CSLaserS != null)
		{
			baseEmitter_CS.CSLaserS = (BaseStraightLaser_CS)CSLaserS.Clone();
		}
		if (CSLaserR != null)
		{
			baseEmitter_CS.CSLaserR = (BaseRadialLaser_CS)CSLaserR.Clone();
		}
		if (CSLaserB != null)
		{
			baseEmitter_CS.CSLaserB = (BaseBendLaser_CS)CSLaserB.Clone();
		}
		if (CSEnemy != null)
		{
			baseEmitter_CS.CSEnemy = (BaseEnemyPlane_Touhou)CSEnemy.Clone();
		}
		if (CSEffect != null)
		{
			baseEmitter_CS.CSEffect = (BaseEffect_CS)CSEffect.Clone();
		}
		return baseEmitter_CS;
	}

	public override bool HitCheck(BaseObject MyPlane)
	{
		return false;
	}
}
