using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS04_05 : BaseTimeSC
{
	public SpellCard_SSS04_05(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "虹奥义「光幕结界」";
		}
		else
		{
			base.SC_Name = "虹奥义「光幕结界」";
		}
		base.BaseScore = 25000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss04(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceKage_ct");
		base.Boss.TransparentValueF = 0f;
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[6];
		base.Boss.HitEnabled = false;
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St04\\关底Boss\\5符E.mbg", 
				1 => ".\\CS\\St04\\关底Boss\\5符N.mbg", 
				2 => ".\\CS\\St04\\关底Boss\\5符H.mbg", 
				3 => ".\\CS\\St04\\关底Boss\\5符L.mbg", 
				_ => ".\\CS\\St04\\关底Boss\\5符U.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
