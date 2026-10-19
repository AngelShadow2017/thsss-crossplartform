using System.Drawing;

namespace Shooting;

internal class SpellCard_SSS05_FSC : BaseFSC
{
	public SpellCard_SSS05_FSC(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Difficulty < DifficultLevel.Hard)
		{
			base.SC_Name = "烈焰「焚秽净世」";
		}
		else
		{
			base.SC_Name = "烈焰「焚秽净世」";
		}
		base.Boss.DestPoint = new Point(base.BoundRect.Width / 2, 120);
		base.Boss.Velocity = 4f;
		StageData.SoundPlay("se_cat00.wav");
		new BackgroundBoss05(StageData);
		new BulletTitle(StageData, base.SC_Name);
		new SpellCardAttack(StageData);
		new FullPic2(StageData, "FaceRakukun_ct");
	}

	public override void Shoot()
	{
		if (base.Time > 100)
		{
			base.Boss.MoveUpDown();
			base.Boss.Armon = base.Boss.ArmonArray[8];
		}
		if (base.Time == 100)
		{
			string fileName = (int)base.Difficulty switch
			{
				2 => "CS/St05/关底Boss/FSC符H.mbg", 
				3 => "CS/St05/关底Boss/FSC符L.mbg", 
				_ => "CS/St05/关底Boss/FSC符L.mbg", 
			};
			CS_Data cSData = base.StageData.LoadCS(fileName);
			new CSEmitterController(base.StageData, cSData);
		}
	}
}
