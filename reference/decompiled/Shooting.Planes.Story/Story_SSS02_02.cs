using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS02_02 : BaseStory_SSS
{
	public Story_SSS02_02(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, "FaceRakuki_no")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "FaceAya_no")
		{
			TxtureObject2 = base.TextureObjectDictionary["FaceAya_base"]
		};
		SBox = new StoryBox(StageData);
		Conv.Add(new Conversation("。。。", CharLAcitve: true, CharRAcitve: false, "FaceAya_no", null));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
			new EndStage(base.StageData, "St3", NextInit: false);
			new ClearBonus(base.StageData, 20000000);
		}
	}
}
