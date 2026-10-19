using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS04_02B : BaseSpellCard
{
	public SpellCard_SSS04_02B(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "镜符「山吹色波纹裂隙」";
		}
		else
		{
			base.SC_Name = "镜符「山吹色波纹裂隙」";
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
		base.Boss.Armon = base.Boss.ArmonArray[3];
		if (base.Time > 100)
		{
			base.Boss.RandomMove(205, 20, 2f, new Rectangle(base.BoundRect.Width / 2 - 45, 100, 90, 40));
		}
		if (base.Time == 150)
		{
			base.Boss.OnAction = 10000;
			string fileName = (int)base.Difficulty switch
			{
				0 => "CS/St04/关底Boss/2符E1.mbg", 
				1 => "CS/St04/关底Boss/2符N1.mbg", 
				2 => "CS/St04/关底Boss/2符H1.mbg", 
				3 => "CS/St04/关底Boss/2符L1.mbg", 
				_ => "CS/St04/关底Boss/2符U1.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
