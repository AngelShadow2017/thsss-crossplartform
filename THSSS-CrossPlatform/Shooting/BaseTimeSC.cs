namespace Shooting;

internal class BaseTimeSC : BaseSpellCard
{
	public BaseTimeSC(StageDataPackage StageData)
		: base(StageData)
	{
		base.Boss.Armon = 1f;
		base.Boss.HitEnabled = false;
		base.Boss.TransparentValueF = 200f;
	}

	public override void GiveEndEffect()
	{
		if (!base.Bombed && !base.Missed)
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
		base.Boss.HitEnabled = true;
		base.Boss.TransparentValueF = 255f;
	}

	public override long GetSpellCardBonus()
	{
		if (base.Bombed || base.Missed)
		{
			return 0L;
		}
		return base.BaseScore + (long)base.Difficulty * 2000000L;
	}
}
