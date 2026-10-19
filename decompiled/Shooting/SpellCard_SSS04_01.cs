using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS04_01 : BaseSpellCard
{
	public SpellCard_SSS04_01(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "魔镜「幻影之境」";
		}
		else
		{
			base.SC_Name = "魔镜「虚无幽幻之境」";
		}
		base.BaseScore = 25000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss04(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		FullPic2 fullPic = new FullPic2(StageData, "FaceKage_ct")
		{
			Scale = 0.8f
		};
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[1];
		if (base.Time > 50)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 100)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St04\\关底Boss\\1符E.mbg", 
				1 => ".\\CS\\St04\\关底Boss\\1符N.mbg", 
				2 => ".\\CS\\St04\\关底Boss\\1符H.mbg", 
				3 => ".\\CS\\St04\\关底Boss\\1符L.mbg", 
				_ => ".\\CS\\St04\\关底Boss\\1符U.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
			cSEmitterController.BossBinding = true;
		}
	}
}
