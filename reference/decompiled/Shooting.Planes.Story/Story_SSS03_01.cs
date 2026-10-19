using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS03_01 : BaseStory_SSS
{
	public Story_SSS03_01(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, "FaceSeiryuu_no")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "FaceAya_no")
		{
			TxtureObject2 = base.TextureObjectDictionary["FaceAya_base"]
		};
		SBox = new StoryBox(StageData);
		CharN = new CharacterName(StageData, "ename_Seiryuu");
		new StoryEmitterStar(StageData, CharN.Position, 0f, 0.0);
		Conv.Add(new Conversation("。。。", CharLAcitve: false, CharRAcitve: true, "FaceAya_no", "FaceSeiryuu_no"));
		Conv.Add(new Conversation("。。。。。。", CharLAcitve: true, CharRAcitve: false, "FaceAya_no", "FaceSeiryuu_no"));
		Conv.Add(new Conversation("。。。", CharLAcitve: false, CharRAcitve: true, "FaceAya_no", "FaceSeiryuu_no"));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			base.Boss.Enabled = true;
			base.StageData.ChangeBGM(".\\BGM\\Boss03.wav", 0, 0, 255, 1286397, 6275430);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "青柳传说", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			SBox.Dispose();
			base.Story = null;
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(40, 255, 20));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
		}
	}
}
