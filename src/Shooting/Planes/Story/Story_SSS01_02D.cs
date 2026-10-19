namespace Shooting.Planes.Story;

internal class Story_SSS01_02D : BaseStory_SSS
{
	public Story_SSS01_02D(StageDataPackage StageData)
		: base(StageData)
	{
		LoadConversation("Story/1-2D.csv");
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
			new EndStage(base.StageData, "St2", NextInit: false);
			new ClearBonus(base.StageData, 10000000);
		}
	}
}
