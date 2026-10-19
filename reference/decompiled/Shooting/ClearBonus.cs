using System;
using System.Drawing;

namespace Shooting;

internal class ClearBonus : BaseEffect
{
	private PointF ValuePoint;

	private int Index = 0;

	private char[] BonusCharArray { get; set; }

	public ClearBonus(StageDataPackage StageData, int Bonus)
		: base(StageData)
	{
		base.Layer = 1;
		base.LifeTime = 150;
		base.TxtureObject = base.TextureObjectDictionary["StageClear"];
		base.MyPlane.Score += Bonus;
		base.TransparentVelocity = 6f;
		base.TransparentValueF = 0f;
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, 150f);
		base.AngleDegree = 90.0;
		BonusCharArray = Bonus.ToString().ToCharArray();
		int num = 0;
		for (int i = 0; i < BonusCharArray.Length; i++)
		{
			num += base.TextureObjectDictionary["Num" + BonusCharArray[i]].Width;
		}
		ValuePoint = new PointF(base.OriginalPosition.X - (float)(num / 2), base.OriginalPosition.Y + 36f);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 100)
		{
			base.TransparentVelocity = -6f;
		}
		if (base.Time == 20)
		{
			while (Index < BonusCharArray.Length)
			{
				ValuePoint = new PointF(ValuePoint.X + (float)(base.TextureObjectDictionary["Num" + BonusCharArray[Index]].Width / 2), ValuePoint.Y);
				BaseEffect baseEffect = new BaseEffect(base.StageData, "Num" + BonusCharArray[Index], new PointF(0f, 0f), 0f, Math.PI / 2.0);
				baseEffect.Active = false;
				baseEffect.OriginalPosition = ValuePoint;
				baseEffect.ColorValue = Color.FromArgb(255 - Index * 10, 255 - Index * 5, 255 - Index * 10);
				baseEffect.LifeTime = 100;
				baseEffect.TransparentValueF = 0f;
				BaseEffect baseEffect2 = baseEffect;
				baseEffect2.TransparentVelocityDictionary.Add(1, 13f);
				baseEffect2.TransparentVelocityDictionary.Add(80, -13f);
				ValuePoint = new PointF(ValuePoint.X + (float)(base.TextureObjectDictionary["Num" + BonusCharArray[Index]].Width / 2), ValuePoint.Y);
				Index++;
			}
		}
	}
}
