namespace Shooting;

internal class SpellDisable : BaseEffect
{
	private bool spellenable;

	public SpellDisable(StageDataPackage StageData)
		: base(StageData)
	{
		spellenable = base.MyPlane.SpellEnabled;
		base.LifeTime = 30;
		base.MyPlane.SpellEnabled = false;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == base.LifeTime)
		{
			base.MyPlane.SpellEnabled = spellenable;
		}
	}
}
