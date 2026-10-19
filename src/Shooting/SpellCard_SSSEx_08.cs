using System.Drawing;

namespace Shooting;

internal class SpellCard_SSSEx_08 : BaseSpellCard
{
	public SpellCard_SSSEx_08(StageDataPackage StageData)
		: base(StageData)
	{
		base.SC_Name = "仙境「风花雪月」";
		base.BaseScore = 40000000L;
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new BackgroundBossEx(StageData);
		switch (base.MyPlane.Name)
		{
		case "Reimu":
			new FullPic2(StageData, "FaceRika_ct0");
			break;
		case "Marisa":
			new FullPic2(StageData, "FaceRika_ct1");
			break;
		case "Sanae":
			new FullPic2(StageData, "FaceRika_ct2");
			break;
		case "Koishi":
			new FullPic2(StageData, "FaceRika_ct3");
			break;
		default:
			new FullPic2(StageData, "FaceRika_ct0");
			break;
		}
	}

	public override void Shoot()
	{
		base.Boss.Armon = base.Boss.ArmonArray[15];
		if (base.Time == 100)
		{
			string fileName = "CS/StEx/关底Boss/8符.mbg";
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
