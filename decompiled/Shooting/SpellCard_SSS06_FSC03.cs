using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS06_FSC03 : BaseTimeSC
{
	public SpellCard_SSS06_FSC03(StageDataPackage StageData)
		: base(StageData)
	{
		base.SC_Name = "第三个愿望「永生不灭」";
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
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
			base.Boss.Armon = base.Boss.ArmonArray[12];
		}
		if (base.Time == 100)
		{
			string fileName = (int)base.Difficulty switch
			{
				2 => ".\\CS\\St06\\关底Boss\\FSC3H.mbg", 
				3 => ".\\CS\\St06\\关底Boss\\FSC3L.mbg", 
				_ => ".\\CS\\St06\\关底Boss\\FSC3L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}

	public override void GiveEndEffect()
	{
		if (!base.Bombed && !base.Missed && base.Time >= base.SpellTime)
		{
			base.StageData.SoundPlay("se_cardget.wav");
			new SpellCardBonus(base.StageData, GetSpellCardBonus());
			if (!base.StageData.OnReplay)
			{
				SC_Data.Recorded++;
			}
		}
		base.Background3D.Enabled3D = true;
		base.StageData.SCBstring = null;
	}

	public override long GetSpellCardBonus()
	{
		if (base.Bombed || base.Missed)
		{
			return 0L;
		}
		return 99999990L;
	}
}
