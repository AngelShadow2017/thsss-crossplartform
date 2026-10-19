using System.Drawing;

namespace Shooting.Planes.Story;

internal class Story_SSS01_02C : BaseStory_SSS
{
	public Story_SSS01_02C(StageDataPackage StageData)
		: base(StageData)
	{
		CharR = new CharacterR_Touhou(StageData, " ")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(base.BoundRect.Width - 60, 380f)
		};
		CharL = new CharacterL_Touhou(StageData, "FaceSanae_lo")
		{
			TxtureObject2 = base.TextureObjectDictionary[" "],
			OriginalPosition = new PointF(70f, 380f)
		};
		SBox = new StoryBox(StageData);
		Conv.Add(new Conversation("难道说因为流星的缘故妖精也变得暴走起来了吗？", CharLAcitve: true, CharRAcitve: false, "FaceSanae_lo", null));
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
