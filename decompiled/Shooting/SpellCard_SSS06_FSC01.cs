using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS06_FSC01 : BaseFSC
{
	public SpellCard_SSS06_FSC01(StageDataPackage StageData)
		: base(StageData)
	{
		base.SC_Name = "第一个愿望「无尽之财」";
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss06(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic3(StageData, "FaceTensei_ct3");
	}

	public override void Shoot()
	{
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
			base.Boss.Armon = base.Boss.ArmonArray[10];
		}
		if (base.Time == 100)
		{
			string fileName = (int)base.Difficulty switch
			{
				2 => ".\\CS\\St06\\关底Boss\\FSC1H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\FSC1L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\FSC1L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
