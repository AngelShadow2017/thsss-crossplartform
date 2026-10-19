using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS03_02X : BaseStory_SSS
{
	public Story_SSS03_02X(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, "FaceSeiryuu_lo")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "Myplane")
		{
			TxtureObject2 = base.TextureObjectDictionary["Myplane"]
		};
		SBox = new StoryBox(StageData);
		Conv.Add(new Conversation("。。。", CharLAcitve: false, CharRAcitve: true, "Myplane", "FaceSeiryuu_lo"));
		Conv.Add(new Conversation("这是意外。", CharLAcitve: true, CharRAcitve: false, "Myplane", "FaceSeiryuu_lo"));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
			new EndStage(base.StageData, "St4", NextInit: false);
			new ClearBonus(base.StageData, 30000000);
		}
	}
}
