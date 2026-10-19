using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS02_FSC : BaseFSC
{
	public SpellCard_SSS02_FSC(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "秘术「龙卷旋风之箭」";
		}
		else
		{
			base.SC_Name = "秘术「龙卷旋风之箭」";
		}
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		BackgroundRotate backgroundRotate = new BackgroundRotate(StageData, "Card02b")
		{
			Scale = 1.3f,
			TransparentValueF = 255f,
			AngularVelocityDegree = -0.5f
		};
		BackgroundMove2 backgroundMove = new BackgroundMove2(StageData, "Card02a")
		{
			DirectionDegree = -80.0,
			TransparentValueF = 155f,
			Scale = 1f,
			Active = false
		};
		backgroundMove.EventGroupList = new List<EventGroup>();
		backgroundMove.EventsExecutionList = new List<Execution>();
		EventGroup eventGroup = new EventGroup
		{
			index = 0,
			tag = "0",
			t = 200,
			addtime = 200
		};
		Event obj = new Event();
		obj.EventString = "当前帧=1：不透明度变化到60，正弦，400";
		obj.String2BulletEvent();
		eventGroup.EventList.Add(obj);
		backgroundMove.EventGroupList.Add(eventGroup);
		BackgroundRotate backgroundRotate2 = new BackgroundRotate(StageData, "Card02c")
		{
			Scale = 0.7f,
			Active = true,
			AngularVelocityDegree = 0.2f
		};
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceRakuki_ct");
	}

	public override void Shoot()
	{
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
			base.Boss.Armon = 0f;
		}
		if (base.Time == 100)
		{
			string fileName = (int)base.Difficulty switch
			{
				2 => "CS/St02/关底Boss/FSC符H.mbg", 
				3 => "CS/St02/关底Boss/FSC符L.mbg", 
				_ => "CS/St02/关底Boss/FSC符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
