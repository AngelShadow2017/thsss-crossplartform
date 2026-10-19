using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

internal class BackgroundLeaf3D : BaseParticle3D
{
	public BackgroundLeaf3D(StageDataPackage StageData)
	{
		base.StageData = StageData;
		StageData.Particle3D.ParticleList.Add(this);
	}

	public override void Ctrl()
	{
		base.Time++;
		Shoot();
	}

	public override void Shoot()
	{
		if (base.Time % 20 == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				BackgroundParticle3D1 backgroundParticle3D = new BackgroundParticle3D1(base.StageData, "Leaf", new PointF(base.StageData.Ran.Next(130, 700), base.StageData.Ran.Next(-50, -20)), base.StageData.Ran.Next(2, 4), Math.PI / 2.0 + (double)((float)base.StageData.Ran.Next(1, 8) / 10f), 200);
				backgroundParticle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				backgroundParticle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(5) / 100f;
				backgroundParticle3D.Scale = (float)base.StageData.Ran.Next(5, 20) / 10f;
				backgroundParticle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				backgroundParticle3D.ColorValue = Color.FromArgb(base.StageData.Ran.Next(150, 250), 255, base.StageData.Ran.Next(150, 255));
			}
		}
	}

	public override void Show()
	{
	}
}
