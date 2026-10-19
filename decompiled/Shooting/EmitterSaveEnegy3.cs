using System;
using System.Drawing;

namespace Shooting;

internal class EmitterSaveEnegy3 : BaseEffect
{
	public EmitterSaveEnegy3(StageDataPackage StageData, PointF Position)
		: base(StageData)
	{
		base.Position = Position;
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.LifeTime = 90;
	}

	public override void Shoot()
	{
		if (base.Time % 5 == 0)
		{
			for (int i = 0; i < 5; i++)
			{
				double num = (double)((float)base.Ran.Next(360) / 180f) * Math.PI;
				int num2 = base.Ran.Next(10, 100);
				PointF position = new PointF((float)((double)base.Position.X + (double)num2 * Math.Cos(num)), (float)((double)base.Position.Y + (double)num2 * Math.Sin(num)));
				ParticleLongerByV particleLongerByV = new ParticleLongerByV(base.StageData, "590-1", position, 0f, num + Math.PI);
				particleLongerByV.TransparentValueF = 200f;
				particleLongerByV.LifeTime = 20;
				particleLongerByV.Scale = (float)base.Ran.Next(20, 50) / 100f;
				ParticleLongerByV particleLongerByV2 = particleLongerByV;
				particleLongerByV2.Velocity = (float)num2 / (float)particleLongerByV2.LifeTime;
			}
		}
	}

	public override void Show()
	{
	}
}
