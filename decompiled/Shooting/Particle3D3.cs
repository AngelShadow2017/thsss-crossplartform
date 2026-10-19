using System.Drawing;

namespace Shooting;

public class Particle3D3 : BaseParticle3D
{
	private float maxScale;

	public Particle3D3(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, int LifeTime)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		base.Active = true;
		StageData.Particle3D.ParticleList.Add(this);
		base.MaxTransparent = 128;
		base.TransparentValueF = base.MaxTransparent;
		base.LifeTime = LifeTime;
		base.Accelerate = (0f - Velocity) / (float)LifeTime;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 1)
		{
			maxScale = base.Scale;
		}
		else if (base.LifeTime / 2 < base.Time && base.Time <= base.LifeTime)
		{
			base.Scale -= maxScale * 2f / (float)base.LifeTime;
		}
	}
}
