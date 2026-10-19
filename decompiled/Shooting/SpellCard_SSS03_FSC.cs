using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS03_FSC : BaseFSC
{
	public SpellCard_SSS03_FSC(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "叶符「德律阿得斯的圣枝」";
		}
		else
		{
			base.SC_Name = "叶符「德律阿得斯的圣枝」";
		}
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		BackgroundRotate backgroundRotate = new BackgroundRotate(StageData, "Card03f")
		{
			Scale = 1f,
			AngularVelocityDegree = 0f,
			AngleDegree = 90.0,
			AngleWithDirection = false
		};
		BackgroundMove2 backgroundMove = new BackgroundMove2(StageData, "Card03d")
		{
			Velocity = 1f,
			DirectionDegree = 0.0,
			TransparentValueF = 155f,
			Scale = 1f,
			Active = false,
			AngularVelocityDegree = 0f
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
		backgroundMove = new BackgroundMove2(StageData, "Card03e")
		{
			Velocity = 1f,
			DirectionDegree = 180.0,
			TransparentValueF = 60f,
			Scale = 1f,
			Active = false,
			AngularVelocityDegree = 0f
		};
		backgroundMove.EventGroupList = new List<EventGroup>();
		backgroundMove.EventsExecutionList = new List<Execution>();
		eventGroup = new EventGroup
		{
			index = 0,
			tag = "0",
			t = 200,
			addtime = 200
		};
		obj = new Event();
		obj.EventString = "当前帧=1：不透明度变化到155，正弦，400";
		obj.String2BulletEvent();
		eventGroup.EventList.Add(obj);
		backgroundMove.EventGroupList.Add(eventGroup);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceSeiryuu_ct");
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
				2 => ".\\CS\\St03\\关底Boss\\FSC符H.mbg", 
				3 => ".\\CS\\St03\\关底Boss\\FSC符L.mbg", 
				_ => ".\\CS\\St03\\关底Boss\\FSC符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
