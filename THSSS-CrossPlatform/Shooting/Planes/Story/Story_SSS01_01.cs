using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS01_01 : BaseStory_SSS
{
	public Story_SSS01_01(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, "FaceAmi01")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "FaceAya_no")
		{
			TxtureObject2 = base.TextureObjectDictionary["FaceAya_base"]
		};
		SBox = new StoryBox(StageData);
		CharN = new CharacterName(StageData, "ename_Ami");
		new StoryEmitterStar(StageData, CharN.Position, 0f, 0.0);
		Conv.Add(new Conversation("。。。", CharLAcitve: false, CharRAcitve: true, "FaceAya_no", "FaceAmi01"));
		Conv.Add(new Conversation("。。。。。。", CharLAcitve: true, CharRAcitve: false, "FaceAya_no", null));
		Conv.Add(new Conversation("从前有座山，", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi01"));
		Conv.Add(new Conversation("山上有座庙，", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi01"));
		Conv.Add(new Conversation("庙里有只文文在卖报，", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi01"));
		Conv.Add(new Conversation("。。。。。。", CharLAcitve: true, CharRAcitve: false, "FaceAya_sw", null));
		Conv.Add(new Conversation("报上写着，", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi02"));
		Conv.Add(new Conversation("从前有座山，山上有座庙，庙里有只文文在卖报", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi03"));
		Conv.Add(new Conversation("报上写着，", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi04"));
		Conv.Add(new Conversation("从前有座山，山上有座庙，庙里有只文文在卖报", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi05"));
		Conv.Add(new Conversation("报上写着，", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi06"));
		Conv.Add(new Conversation("喂，有完没完了", CharLAcitve: true, CharRAcitve: false, "FaceAya_sw", null));
		Conv.Add(new Conversation("好了，开打吧", CharLAcitve: false, CharRAcitve: true, null, "FaceAmi07"));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			base.Boss.Enabled = true;
			base.StageData.ChangeBGM("BGM/Boss01.wav", 0, 0, 255, 754110, 3294270);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "喧闹吧！在这不眠之夜", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
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
