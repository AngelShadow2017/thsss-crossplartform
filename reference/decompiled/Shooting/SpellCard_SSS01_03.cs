using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS01_03 : BaseSpellCard
{
	public SpellCard_SSS01_03(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "四叶「大自然之花环」";
		}
		else
		{
			base.SC_Name = "四叶「大自然之花环」";
		}
		base.BaseScore = 10000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		BackgroundMove2 backgroundMove = new BackgroundMove2(StageData, "Card01b")
		{
			DirectionDegree = 270.0
		};
		BackgroundMove2 backgroundMove2 = new BackgroundMove2(StageData, "Card01a")
		{
			DirectionDegree = -80.0,
			Length = 1024,
			TransparentValueF = 80f
		};
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceAmi_ct");
	}

	public override void Shoot()
	{
		base.Boss.Armon = 0.88f;
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			base.Boss.OnAction = 10000;
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St01\\关底Boss\\2符E.mbg", 
				1 => ".\\CS\\St01\\关底Boss\\2符N.mbg", 
				2 => ".\\CS\\St01\\关底Boss\\2符H.mbg", 
				3 => ".\\CS\\St01\\关底Boss\\2符L.mbg", 
				_ => ".\\CS\\St01\\关底Boss\\2符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
