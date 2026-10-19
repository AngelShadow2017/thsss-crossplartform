using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS05_02 : BaseSpellCard
{
	public SpellCard_SSS05_02(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "机关「火焰发射装置」";
		}
		else
		{
			base.SC_Name = "焰机「星夜的火焰舞曲」";
		}
		base.BaseScore = 30000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss05(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceRakukun_ct");
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[3];
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => "CS/St05/关底Boss/2符E.mbg", 
				1 => "CS/St05/关底Boss/2符N.mbg", 
				2 => "CS/St05/关底Boss/2符H.mbg", 
				3 => "CS/St05/关底Boss/2符L.mbg", 
				_ => "CS/St05/关底Boss/2符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
