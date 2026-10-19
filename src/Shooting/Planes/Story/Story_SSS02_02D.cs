namespace Shooting.Planes.Story;

internal class Story_SSS02_02D : BaseStory_SSS
{
	public Story_SSS02_02D(StageDataPackage StageData)
		: base(StageData)
	{
		LoadConversation("Story/2-2D.csv");
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
			new EndStage(base.StageData, "St3", NextInit: false);
			new ClearBonus(base.StageData, 20000000);
		}
	}
}
