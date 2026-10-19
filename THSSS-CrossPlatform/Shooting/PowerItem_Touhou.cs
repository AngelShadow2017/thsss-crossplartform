using System.Drawing;

namespace Shooting;

internal class PowerItem_Touhou : BaseItem_Touhou
{
	public PowerItem_Touhou(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, "Item-0", OriginalPosition)
	{
		base.Region = 5;
	}

	public override void HitCheckAll()
	{
		base.HitCheckAll();
		if (HitCheck(base.MyPlane))
		{
			base.ItemList.Remove(this);
			float num = base.MyPlane.Power;
			base.MyPlane.Power += 2;
			if (num != (float)base.MyPlane.Power && base.MyPlane.Power % 100 == 0)
			{
				base.StageData.SoundPlay("se_powerup.wav");
			}
			int num2 = ((base.MyPlane.Power == 400) ? 51200 : 10);
			base.MyPlane.Score += num2;
			base.MyPlane.StarPoint += 20;
			if (base.MyPlane.Power == 400)
			{
				ParticleScoreValue particleScoreValue = new ParticleScoreValue(base.StageData, base.OriginalPosition, num2);
			}
			base.StageData.SoundPlay("se_item00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
		}
	}

	public override void Show()
	{
		base.Show();
		if (base.OriginalPosition.Y < -8f)
		{
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["Item-8"], 1f, 0f, new PointF(base.Position.X, base.BoundRect.Top), Color.FromArgb(base.TransparentValue, base.ColorValue));
		}
	}
}
