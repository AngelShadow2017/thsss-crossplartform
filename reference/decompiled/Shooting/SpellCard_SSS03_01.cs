using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS03_01 : BaseSpellCard
{
	public SpellCard_SSS03_01(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "火符「逼近的火焰」";
		}
		else
		{
			base.SC_Name = "火符「燃烧地狱火海」";
		}
		base.BaseScore = 20000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		BackgroundRotate backgroundRotate = new BackgroundRotate(StageData, "Card03b")
		{
			Scale = 1.3f,
			TransparentValueF = 255f,
			AngularVelocityDegree = -0.5f
		};
		BackgroundRotate backgroundRotate2 = new BackgroundRotate(StageData, "Card03a")
		{
			DirectionDegree = 90.0,
			TransparentValueF = 155f,
			Scale = 1f,
			Active = false,
			AngularVelocityDegree = 0f
		};
		backgroundRotate2.EventGroupList = new List<EventGroup>();
		backgroundRotate2.EventsExecutionList = new List<Execution>();
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
		backgroundRotate2.EventGroupList.Add(eventGroup);
		BackgroundRotate backgroundRotate3 = new BackgroundRotate(StageData, "Card03c")
		{
			Scale = 0.7f,
			Active = true,
			AngularVelocityDegree = 0.2f
		};
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		FullPic2 fullPic = new FullPic2(StageData, "FaceKoreirei_ct")
		{
			Scale = 0.8f
		};
	}

	public override void Shoot()
	{
		base.Boss.Armon = 0.15f;
		if (base.Time > 50)
		{
			base.Boss.RandomMove(150, 20, 2f, new Rectangle(base.BoundRect.Width / 2 - 45, 100, 90, 40));
		}
		if (base.Time == 100)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St03\\道中Boss\\1符E.mbg", 
				1 => ".\\CS\\St03\\道中Boss\\1符N.mbg", 
				2 => ".\\CS\\St03\\道中Boss\\1符H.mbg", 
				3 => ".\\CS\\St03\\道中Boss\\1符L.mbg", 
				_ => ".\\CS\\St03\\道中Boss\\1符U.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
			cSEmitterController.BossBinding = true;
		}
	}
}
