using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS04_03C : BaseSpellCard
{
	public SpellCard_SSS04_03C(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "岚符「大孔雀暴风」";
		}
		else
		{
			base.SC_Name = "岚符「大孔雀暴风」";
		}
		base.BaseScore = 25000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 128);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss04(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceKage_ct");
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[4];
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => "CS/St04/关底Boss/3符E2.mbg", 
				1 => "CS/St04/关底Boss/3符N2.mbg", 
				2 => "CS/St04/关底Boss/3符H2.mbg", 
				3 => "CS/St04/关底Boss/3符L2.mbg", 
				_ => "CS/St04/关底Boss/3符U2.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
		if ((base.Time - 160) % 363 == 0)
		{
			base.Boss.OnAction = 250;
		}
	}
}
