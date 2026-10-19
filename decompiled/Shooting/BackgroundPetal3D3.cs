using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

internal class BackgroundPetal3D3 : BaseParticle3D
{
	public BackgroundPetal3D3(StageDataPackage StageData)
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
		if (base.Time % 4 == 0)
		{
			for (int i = 0; i < 3; i++)
			{
				BackgroundParticle3D2 backgroundParticle3D = new BackgroundParticle3D2(base.StageData, "Petal0" + base.Ran.Next(4), new PointF(base.StageData.Ran.Next(0, base.BoundRect.Width), base.StageData.Ran.Next(-150, (base.Time > 600) ? 100 : 300)), base.StageData.Ran.Next(3, 6), Math.PI / 2.0 + (double)((float)base.StageData.Ran.Next(-4, 3) / 10f), 250);
				backgroundParticle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
				backgroundParticle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(10) / 100f;
				backgroundParticle3D.Scale = (float)base.StageData.Ran.Next(4, 6) / 10f;
				backgroundParticle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
				backgroundParticle3D.MaxTransparent = 160;
				backgroundParticle3D.Depth = base.Ran.Next(-300, 0);
				backgroundParticle3D.MaxDepth = 1000f;
				backgroundParticle3D.Direction3D = ((base.Time > 600) ? (Math.PI / 6.0) : (Math.PI / 4.0));
			}
		}
	}

	public override void Show()
	{
	}
}
