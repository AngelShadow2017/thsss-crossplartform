using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS06_06 : BaseSpellCard
{
	private int flag = 1;

	public SpellCard_SSS06_06(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "「流星神话」";
		}
		else
		{
			base.SC_Name = "「流星神话」";
		}
		base.BaseScore = 35000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss06(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic3(StageData, "FaceTensei_ct3");
	}

	public override void Shoot()
	{
		if (base.SpellList.Count > 0 || base.MyPlane.Time < base.MyPlane.UnmatchedTime - 30)
		{
			base.Boss.Armon = 1f;
		}
		else
		{
			base.Boss.Armon = base.Boss.ArmonArray[9];
		}
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
		}
		if (base.Time == 150)
		{
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St06\\关底Boss\\6符P1E.mbg", 
				1 => ".\\CS\\St06\\关底Boss\\6符P1N.mbg", 
				2 => ".\\CS\\St06\\关底Boss\\6符P1H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\6符P1L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\6符P1L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
		if ((base.Boss.HealthPoint < (float)(base.Boss.MaxHP * 3) / 4f || base.Time > 1800) && flag == 1)
		{
			flag++;
			ClearEmitter();
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St06\\关底Boss\\6符P2E.mbg", 
				1 => ".\\CS\\St06\\关底Boss\\6符P2N.mbg", 
				2 => ".\\CS\\St06\\关底Boss\\6符P2H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\6符P2L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\6符P2L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
		if ((base.Boss.HealthPoint < (float)(base.Boss.MaxHP * 2) / 4f || base.Time > 3600) && flag == 2)
		{
			flag++;
			ClearEmitter();
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St06\\关底Boss\\6符P3E.mbg", 
				1 => ".\\CS\\St06\\关底Boss\\6符P3N.mbg", 
				2 => ".\\CS\\St06\\关底Boss\\6符P3H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\6符P3L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\6符P2L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
		if ((base.Boss.HealthPoint < (float)base.Boss.MaxHP / 4f || base.Time > 5400) && flag == 3)
		{
			flag++;
			ClearEmitter();
			string fileName = (int)base.Difficulty switch
			{
				0 => ".\\CS\\St06\\关底Boss\\6符P4E.mbg", 
				1 => ".\\CS\\St06\\关底Boss\\6符P4N.mbg", 
				2 => ".\\CS\\St06\\关底Boss\\6符P4H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\6符P4L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\6符P4L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}

	private void ClearEmitter()
	{
		for (int num = base.EnemyPlaneList.Count - 1; num >= 0; num--)
		{
			if (!(base.EnemyPlaneList[num] is BaseSpellCard))
			{
				base.EnemyPlaneList[num].GiveEndEffect();
				base.EnemyPlaneList.RemoveAt(num);
			}
		}
		foreach (BaseBullet_Touhou bullet in base.BulletList)
		{
			bullet.GiveEndEffect();
		}
		base.BulletList.Clear();
	}
}
