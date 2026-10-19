using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS04_04B : BaseTimeSC
{
	public SpellCard_SSS04_04B(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "星符「急速彗星」";
		}
		else
		{
			base.SC_Name = "星符「急速彗星」";
		}
		base.BaseScore = 25000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 100);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss04(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceKage_ct");
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[5];
		base.Boss.HitEnabled = false;
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => "CS/St04/关底Boss/4符E1.mbg", 
				1 => "CS/St04/关底Boss/4符N1.mbg", 
				2 => "CS/St04/关底Boss/4符H1.mbg", 
				3 => "CS/St04/关底Boss/4符L1.mbg", 
				_ => "CS/St04/关底Boss/4符U1.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
		if ((base.Time - 100) % 200 == 0)
		{
			base.Boss.OnAction = 150;
		}
	}
}
