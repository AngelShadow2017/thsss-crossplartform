using System.Drawing;

namespace Shooting;

internal class EndBoss_FSC06 : EndBoss_Touhou_Small
{
	private int tempVolumn;

	public EndBoss_FSC06(StageDataPackage StageData, PointF OriginalPosition, Color Color1, Color Color2)
		: base(StageData, OriginalPosition, Color1, Color2)
	{
		tempVolumn = base.BGM_Player.Volume;
		base.LifeTime = 300;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		int num = base.LifeTime - 90;
		base.BGM_Player.Volume = (int)((float)tempVolumn * (1f - (float)base.Time / (float)num));
		PointF originalPosition = new PointF(base.BoundRect.Width / 2, 150f);
		if (base.Time == base.LifeTime - 60 || base.Time == base.LifeTime - 40 || base.Time == base.LifeTime - 20)
		{
			new EmitterSaveEnegy3D(base.StageData, originalPosition, Color.SkyBlue);
			base.StageData.SoundPlay("se_ch02.wav");
		}
		if (base.Time == base.LifeTime)
		{
			base.StageData.ChangeBGM("BGM/Boss06FSC.wav", 0, 0, 255, 0, 0);
			new Boss_Tensei02(base.StageData);
			base.Background3D.WarpEnabled = true;
		}
	}
}
