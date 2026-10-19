using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS06_05 : BaseTimeSC
{
	public SpellCard_SSS06_05(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "「弹幕之海」";
		}
		else
		{
			base.SC_Name = "「弹幕之海」";
		}
		base.BaseScore = 35000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		base.Boss.SpellTime = base.Boss.Time + 3000 - 30;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss06(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic3(StageData, "FaceTensei_ct3");
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[8];
		base.Boss.HitEnabled = false;
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St06\\关底Boss\\5符E.mbg", 
				1 => ".\\CS\\St06\\关底Boss\\5符N.mbg", 
				2 => ".\\CS\\St06\\关底Boss\\5符H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\5符L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\5符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}

	public override void GiveEndEffect()
	{
		base.GiveEndEffect();
		base.Boss.HitEnabled = true;
	}
}
