using System;
using System.Drawing;

namespace Shooting;

internal class Emitter2 : BaseEffect
{
	public Emitter2(StageDataPackage StageData, PointF Position)
		: base(StageData)
	{
		base.Position = Position;
		base.Velocity = 0f;
		base.Direction = 0.0;
		base.LifeTime = 2;
	}

	public override void Shoot()
	{
		for (int i = 0; i < 1; i++)
		{
			string text = ((base.MyPlane.Name == "Aya") ? "MyParticle" : "MyParticle01");
			ParticleSmaller particleSmaller = new ParticleSmaller(textureName: base.MyPlane.Name switch
			{
				"Aya" => "MyParticle", 
				"Reimu" => "Light01", 
				"Marisa" => "Light03", 
				"Sanae" => "Light04", 
				"Koishi" => "Light05", 
				_ => "MyParticle01", 
			}, StageData: base.StageData, Position: base.Position, Velocity: base.Ran.Next(100, 300) / 10, Direction: (double)((float)base.Ran.Next(0, 180) / 180f) * Math.PI);
			particleSmaller.LifeTime = 10;
			particleSmaller.Scale = (float)base.Ran.Next(60) / 100f;
			particleSmaller.TransparentValueF = 200f;
			particleSmaller.Accelerate = (0f - particleSmaller.Velocity) / (float)particleSmaller.LifeTime;
		}
	}
}
