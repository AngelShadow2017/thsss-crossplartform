using System;
using System.Drawing;

namespace Shooting;

public class EDPetal_Emitter : BaseEffect
{
	private string EDText;

	public int Hold { get; set; }

	public EDPetal_Emitter(StageDataPackage StageData, PointF OriginalPosition, string EDText)
		: base(StageData, null, new Point(0, 0), 0f, Math.PI / 2.0)
	{
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = 130;
		this.EDText = EDText;
	}

	public override void Shoot()
	{
		if (base.Time == 10)
		{
			BaseEffect baseEffect = new BaseEffect(base.StageData, EDText, base.OriginalPosition, 0f, 0.0);
			baseEffect.Scale = base.Scale;
			baseEffect.Direction = Math.PI / 2.0;
			baseEffect.OriginalPosition = base.OriginalPosition;
			baseEffect.Active = false;
			baseEffect.TransparentValueF = 0f;
			baseEffect.TransparentVelocity = 10f;
			baseEffect.LifeTime = 322 + Hold;
			baseEffect.TransparentVelocityDictionary.Add(300 + Hold, -12f);
		}
		for (int i = 0; i < 5; i++)
		{
			if (base.Time == 1 + i * 3)
			{
				EDPetal eDPetal = new EDPetal(base.StageData, "ED_Petal", base.OriginalPosition, 0f, 0.0);
				eDPetal.DestAngleDegree = 72 * i;
				eDPetal.LifeTime = 300 + Hold;
			}
		}
	}
}
