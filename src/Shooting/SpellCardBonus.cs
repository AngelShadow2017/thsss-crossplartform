using System.Drawing;

namespace Shooting;

internal class SpellCardBonus : BaseEffect
{
	private PointF ValuePoint;

	private int Index = 0;

	private char[] BonusCharArray { get; set; }

	public SpellCardBonus(StageDataPackage StageData, long Bonus)
		: base(StageData)
	{
		base.Layer = 1;
		base.LifeTime = 150;
		base.TxtureObject = base.TextureObjectDictionary["GetSpellCardBonus"];
		base.MyPlane.Score += Bonus;
		base.TransparentVelocity = 100f;
		base.TransparentValueF = 0f;
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, 120f);
		base.AngleDegree = 90.0;
		BonusCharArray = Bonus.ToString().ToCharArray();
		int num = 0;
		for (int i = 0; i < BonusCharArray.Length; i++)
		{
			num += base.TextureObjectDictionary["Num" + BonusCharArray[i]].Width;
		}
		ValuePoint = new PointF(base.OriginalPosition.X - (float)(num / 2), base.OriginalPosition.Y + 40f);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 100)
		{
			base.TransparentVelocity = -10f;
		}
		if (base.Time >= 20 && base.Time % 4 == 0 && Index < BonusCharArray.Length)
		{
			ValuePoint = new PointF(ValuePoint.X + (float)(base.TextureObjectDictionary["Num" + BonusCharArray[Index]].Width / 2), ValuePoint.Y);
			ParticleBonusNum particleBonusNum = new ParticleBonusNum(base.StageData, "Num" + BonusCharArray[Index], ValuePoint);
			particleBonusNum.ColorValue = Color.FromArgb(255 - Index * 10, 255 - Index * 5, 255 - Index * 10);
			ValuePoint = new PointF(ValuePoint.X + (float)(base.TextureObjectDictionary["Num" + BonusCharArray[Index]].Width / 2), ValuePoint.Y);
			Index++;
		}
	}
}
