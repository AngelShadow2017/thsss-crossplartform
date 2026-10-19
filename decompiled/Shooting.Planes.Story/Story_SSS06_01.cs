using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS06_01 : BaseStory_SSS
{
	public Story_SSS06_01(StageDataPackage StageData)
		: base(StageData)
	{
		switch (base.MyPlane.Name)
		{
		case "Reimu":
			LoadConversation2(".\\Story\\6-1A.csv");
			break;
		case "Marisa":
			LoadConversation(".\\Story\\6-1B.csv");
			break;
		case "Sanae":
			LoadConversation(".\\Story\\6-1C.csv");
			break;
		case "Koishi":
			LoadConversation(".\\Story\\6-1D.csv");
			break;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == NameTime && LastTime < base.Time)
		{
			CharN = new CharacterName(base.StageData, "ename_Tensei");
			new StoryEmitterStar(base.StageData, CharN.Position, 0f, 0.0);
			LastTime = base.Time;
		}
		else if (base.Time == MusicTime && LastTime < base.Time)
		{
			base.StageData.ChangeBGM(".\\BGM\\Boss06.wav", 0, 0, 255, 1697409, 10789065);
			LastTime = base.Time;
		}
		if (base.Time == Conv.Count)
		{
			MusicTitle musicTitle = new MusicTitle(base.StageData, "？？？？？", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Boss.Enabled = true;
			SBox.Dispose();
			base.Story = null;
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.SkyBlue);
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
		}
	}
}
