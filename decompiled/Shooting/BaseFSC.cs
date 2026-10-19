namespace Shooting;

internal class BaseFSC : BaseSpellCard
{
	public BaseFSC(StageDataPackage StageData)
		: base(StageData)
	{
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
