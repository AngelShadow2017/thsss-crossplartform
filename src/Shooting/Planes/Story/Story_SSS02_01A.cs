using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS02_01A : BaseStory_SSS
{
	public Story_SSS02_01A(StageDataPackage StageData)
		: base(StageData)
	{
		LoadConversation2("Story/2-1A.csv");
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == NameTime && LastTime < base.Time)
		{
			LastTime = base.Time;
			CharN = new CharacterName(base.StageData, "ename_Rakuki");
			new StoryEmitterStar(base.StageData, CharN.Position, 0f, 0.0);
		}
		if (base.Time == MusicTime && LastTime < base.Time)
		{
			base.StageData.ChangeBGM("BGM/Boss02.wav", 0, 0, 255, 327663, 3504627);
			LastTime = base.Time;
		}
		if (base.Time == Conv.Count)
		{
			MusicTitle musicTitle = new MusicTitle(base.StageData, "疾风闪电", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Boss.Enabled = true;
			SBox.Dispose();
			base.Story = null;
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(144, 255, 120));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
		}
	}
}
