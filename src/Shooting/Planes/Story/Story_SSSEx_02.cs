namespace Shooting.Planes.Story;

internal class Story_SSSEx_02 : BaseStory_SSS
{
	public Story_SSSEx_02(StageDataPackage StageData)
		: base(StageData)
	{
		switch (base.MyPlane.Name)
		{
		case "Reimu":
			LoadConversation2("Story/Ex-2A.csv");
			break;
		case "Marisa":
			LoadConversation("Story/Ex-2B.csv");
			break;
		case "Sanae":
			LoadConversation("Story/Ex-2C.csv");
			break;
		case "Koishi":
			LoadConversation("Story/Ex-2D.csv");
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
			new EndStage(base.StageData, "GameSetMenu", NextInit: true);
			new ClearBonus(base.StageData, 70000000);
		}
	}
}
