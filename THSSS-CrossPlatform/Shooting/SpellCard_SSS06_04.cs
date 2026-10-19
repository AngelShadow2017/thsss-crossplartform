using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS06_04 : BaseSpellCard
{
	public SpellCard_SSS06_04(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "星图「宇宙棋盘」";
		}
		else
		{
			base.SC_Name = "星图「宇宙棋盘」";
		}
		base.BaseScore = 35000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss06(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic3(StageData, "FaceTensei_ct2");
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[7];
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => "CS/St06/关底Boss/4符E.mbg", 
				1 => "CS/St06/关底Boss/4符N.mbg", 
				2 => "CS/St06/关底Boss/4符H.mbg", 
				3 => "CS/St06/关底Boss/4符L.mbg", 
				_ => "CS/St06/关底Boss/4符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
