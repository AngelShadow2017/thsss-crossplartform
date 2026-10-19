using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS02_01X : BaseStory_SSS
{
	public Story_SSS02_01X(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, "FaceRakuki_no")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "Myplane")
		{
			TxtureObject2 = base.TextureObjectDictionary["Myplane"]
		};
		SBox = new StoryBox(StageData);
		CharN = new CharacterName(StageData, "ename_Rakuki");
		new StoryEmitterStar(StageData, CharN.Position, 0f, 0.0);
		Conv.Add(new Conversation("。。。", CharLAcitve: false, CharRAcitve: true, "Myplane", "FaceRakuki_no"));
		Conv.Add(new Conversation("。。。。。。", CharLAcitve: true, CharRAcitve: false, "Myplane", "FaceRakuki_no"));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			base.Boss.Enabled = true;
			base.StageData.ChangeBGM(".\\BGM\\Boss02.wav", 0, 0, 255, 327663, 3504627);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "疾风闪电", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			SBox.Dispose();
			base.Story = null;
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(144, 255, 120));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
		}
	}
}
