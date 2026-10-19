using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS01_02B : BaseStory_SSS
{
	public Story_SSS01_02B(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, " ")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "FaceMarisa_hp2")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(70f, 340f)
		};
		SBox = new StoryBox(StageData);
		Conv.Add(new Conversation("呼…轻松轻松~", CharLAcitve: true, CharRAcitve: false, null, null));
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == Conv.Count)
		{
			SBox.Dispose();
			base.Story = null;
			new EndStage(base.StageData, "St2", NextInit: false);
			new ClearBonus(base.StageData, 10000000);
		}
	}
}
