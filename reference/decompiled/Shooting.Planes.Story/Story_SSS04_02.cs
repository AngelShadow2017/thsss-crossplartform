namespace Shooting.Planes.Story;

internal class Story_SSS04_02 : BaseStory_SSS
{
	public Story_SSS04_02(StageDataPackage StageData)
		: base(StageData)
	{
		switch (base.MyPlane.Name)
		{
		case "Reimu":
			LoadConversation2(".\\Story\\4-2A.csv");
			break;
		case "Marisa":
			LoadConversation(".\\Story\\4-2B.csv");
			break;
		case "Sanae":
			LoadConversation(".\\Story\\4-2C.csv");
			break;
		case "Koishi":
			LoadConversation(".\\Story\\4-2D.csv");
			break;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
			new EndStage(base.StageData, "St5", NextInit: false);
			new ClearBonus(base.StageData, 40000000);
		}
	}
}
