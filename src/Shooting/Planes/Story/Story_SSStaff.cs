namespace Shooting.Planes.Story;

internal class Story_SSStaff : BaseStory_SSS
{
	private CG Cg;

	public Story_SSStaff(StageDataPackage StageData)
		: base(StageData)
	{
		// The staff roll is reachable without a player ship in the scene (for
		// example when entering the ending directly), so fall back to the default
		// conversation instead of dereferencing a null MyPlane.
		switch (base.MyPlane?.Name)
		{
		case "Marisa":
			LoadConversation("Story/Staff-1B.csv");
			break;
		case "Sanae":
			LoadConversation("Story/Staff-1C.csv");
			break;
		case "Koishi":
			LoadConversation("Story/Staff-1D.csv");
			break;
		case "Reimu":
		default:
			LoadConversation("Story/Staff-1A.csv");
			break;
		}
		SBox.MaxTransparent = 0;
		Cg = new CG(StageData, null);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		CharL.TxtureObject = null;
		CharR.TxtureObject = null;
		Cg.Ctrl();
		if (base.Time < Conv.Count)
		{
			Cg.TxtureObject = base.TextureObjectDictionary[Conv[base.Time].CharL];
		}
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
		}
	}

	public override void Show()
	{
		Cg.Show();
		base.Show();
	}
}
