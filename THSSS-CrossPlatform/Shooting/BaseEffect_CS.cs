using System.Collections.Generic;

namespace Shooting;

public class BaseEffect_CS : BaseEffect
{
	public BaseEffect_CS()
	{
	}

	public BaseEffect_CS(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.EventGroupList = new List<EventGroup>();
		base.EventsExecutionList = new List<Execution>();
	}

	public override void EventCtrl()
	{
		foreach (EventGroup a in SlimDX.Compat.ListCompat.Snapshot(base.EventGroupList)){
			a.Update(this);
		};
		foreach (Execution a in SlimDX.Compat.ListCompat.Snapshot(base.EventsExecutionList)){
			a.Update(this);
		};
	}
}
