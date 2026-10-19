namespace Shooting.Planes.Story;

internal class Story_SSS01_02A : BaseStory_SSS
{
	public Story_SSS01_02A(StageDataPackage StageData)
		: base(StageData)
	{
		LoadConversation2(".\\Story\\1-2A.csv");
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
