using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS05_02 : BaseStory_SSS
{
	public Story_SSS05_02(StageDataPackage StageData)
		: base(StageData)
	{
		switch (base.MyPlane.Name)
		{
		case "Reimu":
			LoadConversation2(".\\Story\\5-2A.csv");
			break;
		case "Marisa":
			LoadConversation(".\\Story\\5-2B.csv");
			break;
		case "Sanae":
			LoadConversation(".\\Story\\5-2C.csv");
			break;
		case "Koishi":
			LoadConversation(".\\Story\\5-2D.csv");
			break;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == NameTime && LastTime < base.Time)
		{
			LastTime = base.Time;
			CharN = new CharacterName(base.StageData, "ename_Rakukun");
			new StoryEmitterStar(base.StageData, CharN.Position, 0f, 0.0);
		}
		else if (base.Time == MusicTime && LastTime < base.Time)
		{
			base.StageData.ChangeBGM(".\\BGM\\Boss05.wav", 0, 0, 255, 1000056, 6599345);
			LastTime = base.Time;
		}
		if (base.Time == Conv.Count)
		{
			MusicTitle musicTitle = new MusicTitle(base.StageData, "引燃夜空的星火", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
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
