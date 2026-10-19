using System;
using System.Drawing;

namespace Shooting;

internal class Emitter : BaseEffect
{
	public Emitter(StageDataPackage StageData, PointF Position)
		: base(StageData)
	{
		base.Position = Position;
		base.Velocity = 0f;
		base.Direction = 0.0;
	}

	public override void Shoot()
	{
		for (int i = 0; i < 10; i++)
		{
			string textureName = ((base.MyPlane.Name == "Aya") ? "MyParticle" : "MyParticle01");
			ParticleSmaller particleSmaller = new ParticleSmaller(base.StageData, textureName, base.Position, base.Ran.Next(20, 200) / 10, (double)((float)base.Ran.Next(360) / 180f) * Math.PI);
			particleSmaller.LifeTime = 20;
			particleSmaller.Scale = (float)base.Ran.Next(100) / 100f;
			particleSmaller.TransparentValueF = 200f;
		}
	}

	public override bool OutBoundary()
	{
		if (base.Time >= 4)
		{
			return true;
		}
		return false;
	}

	public override void Show()
	{
	}
}
