using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

internal class BackgroundPetal3D : BaseParticle3D
{
	public BackgroundPetal3D(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.Background3D.BackgroundParticleList.Add(this);
	}

	public override void Ctrl()
	{
		base.Time++;
		Shoot();
	}

	public override void Shoot()
	{
		if (base.Time % 5 == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				BackgroundParticle3D1 backgroundParticle3D = new BackgroundParticle3D1(base.StageData, "Petal3" + base.Ran.Next(4), new PointF(base.StageData.Ran.Next(280, base.BoundRect.Width + 160), base.StageData.Ran.Next(-20, 0)), (float)base.StageData.Ran.Next(15, 25) / 10f, Math.PI / 2.0 + (double)((float)base.StageData.Ran.Next(1, 8) / 10f), 200);
				backgroundParticle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				backgroundParticle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(10) / 100f;
				backgroundParticle3D.Scale = (float)base.StageData.Ran.Next(35, 50) / 100f;
				backgroundParticle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				backgroundParticle3D.MaxTransparent = 200;
				backgroundParticle3D.Depth = base.Ran.Next(0, 30);
			}
		}
	}

	public override void Show()
	{
	}
}
