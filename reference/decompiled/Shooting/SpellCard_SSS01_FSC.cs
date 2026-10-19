using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS01_FSC : BaseFSC
{
	public SpellCard_SSS01_FSC(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "夜风「轮回的幸运之星」";
		}
		else
		{
			base.SC_Name = "夜风「轮回的幸运之星」";
		}
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		BackgroundMove2 backgroundMove = new BackgroundMove2(StageData, "Card01b")
		{
			DirectionDegree = 270.0
		};
		BackgroundMove2 backgroundMove2 = new BackgroundMove2(StageData, "Card01a")
		{
			DirectionDegree = -80.0,
			Length = 1024,
			TransparentValueF = 80f
		};
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceAmi_ct");
	}

	public override void Shoot()
	{
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
			base.Boss.Armon = 0f;
		}
		if (base.Time == 100)
		{
			base.Boss.OnAction = 10000;
			string fileName = (int)base.Difficulty switch
			{
				2 => ".\\CS\\St01\\关底Boss\\FSC符H.mbg", 
				3 => ".\\CS\\St01\\关底Boss\\FSC符L.mbg", 
				_ => ".\\CS\\St01\\关底Boss\\FSC符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
