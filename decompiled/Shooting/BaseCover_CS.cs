using System;
using System.Collections.Generic;

namespace Shooting;

[Serializable]
public class BaseCover_CS : BaseEmitter_CS
{
	public bool Roundness { get; set; }

	public int CtrlID { get; set; }

	public List<EventGroup> SubEventGroupList { get; set; }

	public BaseCover_CS(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.EventGroupList = new List<EventGroup>();
		base.EventsExecutionList = new List<Execution>();
		SubEventGroupList = new List<EventGroup>();
		base.HealthPoint = 1000000f;
		base.HitEnabled = false;
		base.Region = 1;
		base.Angle = Math.PI / 2.0;
	}

	public override void Shoot()
	{
		base.AngleWithDirection = Roundness;
	}

	public override void EventCtrl()
	{
		base.EventGroupList.ForEach(delegate(EventGroup a)
		{
			a.Update(this);
		});
		base.EventsExecutionList.ForEach(delegate(Execution a)
		{
			a.Update(this);
		});
	}

	public override void HitCheckAll()
	{
		if (base.Time < base.StartTime || base.Time > base.StartTime + base.Duration)
		{
			return;
		}
		base.BulletList.ForEach(delegate(BaseBullet_Touhou b)
		{
			if (b.Cover && (Type == 0 || b.ID == CtrlID) && HitCheck(b))
			{
				SubEventGroupList.ForEach(delegate(EventGroup e)
				{
					e.Update(b);
				});
			}
		});
	}

	public override bool HitCheck(BaseObject Target)
	{
		if (Roundness)
		{
			if (base.ScaleLength == base.ScaleWidth)
			{
				if (GetDistance(Target) - (double)Target.Region < (double)((float)base.Region * base.Scale))
				{
					return true;
				}
			}
			else
			{
				double num = (base.AngleWithDirection ? (base.Direction + base.Angle) : base.Angle);
				double distance = GetDistance(Target);
				double num2 = GetDirection(Target);
				double num3 = distance * Math.Cos((float)(num - num2));
				double num4 = distance * Math.Sin((float)(num - num2));
				double num5 = (float)base.Region * base.ScaleWidth;
				double num6 = (float)base.Region * base.ScaleLength;
				if (num4 * num4 / num5 / num5 + num3 * num3 / num6 / num6 < 1.0)
				{
					return true;
				}
			}
			return false;
		}
		if (Math.Abs(Target.OriginalPosition.X - base.OriginalPosition.X) < (float)base.Region * base.ScaleWidth && Math.Abs(Target.OriginalPosition.Y - base.OriginalPosition.Y) < (float)base.Region * base.ScaleLength)
		{
			return true;
		}
		return false;
	}
}
