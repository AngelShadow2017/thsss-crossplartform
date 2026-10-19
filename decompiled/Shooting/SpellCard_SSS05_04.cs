using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS05_04 : BaseSpellCard
{
	public SpellCard_SSS05_04(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "焰符「原始的烈焰」";
		}
		else
		{
			base.SC_Name = "焰符「八百里的烈焰」";
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
		base.Boss.Armon = base.Boss.ArmonArray[6];
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St05\\关底Boss\\4符E.mbg", 
				1 => ".\\CS\\St05\\关底Boss\\4符N.mbg", 
				2 => ".\\CS\\St05\\关底Boss\\4符H.mbg", 
				3 => ".\\CS\\St05\\关底Boss\\4符L.mbg", 
				_ => ".\\CS\\St05\\关底Boss\\4符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
