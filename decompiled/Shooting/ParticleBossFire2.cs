using System;
using System.Drawing;

namespace Shooting;

internal class ParticleBossFire2 : BaseObject
{
	public double AngularVelocity { get; set; }

	public ParticleBossFire2(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, int LifeTime)
	{
		base.StageData = StageData;
		base.Velocity = Velocity;
		base.Direction = Direction;
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = LifeTime;
		base.Scale = 0.01f;
		AngularVelocity = (double)base.Ran.Next(-2, 2) * Math.PI / 180.0;
		if (textureName != null)
		{
			base.TxtureObject = base.TextureObjectDictionary[textureName];
		}
		base.Active = true;
		base.Background.BackgroundList.Add(this);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time > 0)
		{
			base.TransparentValueF -= 255 / base.LifeTime;
		}
		if (base.Boss != null)
		{
			base.OriginalPosition = base.Boss.OriginalPosition;
		}
		base.Direction += AngularVelocity;
		if (base.Time > 0)
		{
			base.Scale += 0.025f;
			base.TransparentValueF -= base.MaxTransparent / base.LifeTime;
		}
		if (OutBoundary())
		{
			base.Background.BackgroundList.Remove(this);
		}
	}
}
