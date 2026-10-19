using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class GameState_testStage : BaseStage, IGameState
{
	public GameState_testStage(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "Stage_test";
	}

	public override void Drama()
	{
		if (base.TimeMain == 1)
		{
			base.StageData.SetReplayInfo(base.StageName);
			if (base.MyPlane == null)
			{
				base.MyPlane = new BaseMyPlane(base.StageData, new Point(base.BoundRect.Width, base.BoundRect.Height - 50));
			}
			BackgroundRotate backgroundRotate = new BackgroundRotate(base.StageData, "Card02b");
			backgroundRotate.Scale = 1.3f;
			backgroundRotate.TransparentValueF = 255f;
			backgroundRotate.AngularVelocityDegree = -0.5f;
			BackgroundRotate backgroundRotate2 = new BackgroundRotate(base.StageData, "Card02a");
			backgroundRotate2.DirectionDegree = 90.0;
			backgroundRotate2.TransparentValueF = 155f;
			backgroundRotate2.Scale = 1f;
			backgroundRotate2.Active = false;
			backgroundRotate2.AngularVelocityDegree = 0f;
			BackgroundRotate backgroundRotate3 = backgroundRotate2;
			backgroundRotate3.EventGroupList = new List<EventGroup>();
			backgroundRotate3.EventsExecutionList = new List<Execution>();
			EventGroup eventGroup = new EventGroup();
			eventGroup.index = 0;
			eventGroup.tag = "0";
			eventGroup.t = 200;
			eventGroup.addtime = 200;
			Event obj = new Event();
			obj.EventString = "当前帧=1：不透明度变化到60，正弦，400";
			obj.String2BulletEvent();
			eventGroup.EventList.Add(obj);
			backgroundRotate3.EventGroupList.Add(eventGroup);
			BackgroundRotate backgroundRotate4 = new BackgroundRotate(base.StageData, "Card02c");
			backgroundRotate4.Scale = 0.7f;
			backgroundRotate4.Active = true;
			backgroundRotate4.AngularVelocityDegree = 0.2f;
		}
		if (base.TimeMain == 50)
		{
		}
		if (base.TimeMain % 400 != 100)
		{
		}
	}

	public override void BGM_ON()
	{
	}
}
