using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS05_01 : BaseStory_SSS
{
	private int circle = 120;

	private int tCount = 0;

	public Story_SSS05_01(StageDataPackage StageData)
		: base(StageData)
	{
		switch (base.MyPlane.Name)
		{
		case "Reimu":
			LoadConversation2(".\\Story\\5-1A.csv");
			break;
		case "Marisa":
			LoadConversation(".\\Story\\5-1B.csv");
			break;
		case "Sanae":
			LoadConversation(".\\Story\\5-1C.csv");
			break;
		case "Koishi":
			LoadConversation(".\\Story\\5-1D.csv");
			break;
		}
	}

	public override void Ctrl()
	{
		tCount++;
		if (base.Time < tCount / circle)
		{
			base.Time++;
		}
		base.Ctrl();
		if (base.Time == NameTime && LastTime < base.Time)
		{
			LastTime = base.Time;
		}
		else if (base.Time == MusicTime && LastTime < base.Time)
		{
			LastTime = base.Time;
		}
		if (base.Time == Conv.Count)
		{
			base.Boss.Enabled = true;
			SBox.Dispose();
			base.Story = null;
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(144, 255, 120));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpColorKey = 2;
			base.Background3D.WarpEnabled = true;
		}
	}
}
