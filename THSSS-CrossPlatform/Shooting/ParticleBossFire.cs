using System.Drawing;

namespace Shooting;

internal class ParticleBossFire : BaseObject
{
	public ParticleBossFire(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction, int LifeTime)
	{
		base.StageData = StageData;
		base.Velocity = Velocity;
		base.Direction = Direction;
		base.OriginalPosition = OriginalPosition;
		base.LifeTime = LifeTime;
		if (textureName != null)
		{
			base.TxtureObject = base.TextureObjectDictionary[textureName];
		}
		base.Active = true;
		base.Background.BackgroundList.Add(this);
		base.ScaleLength = 0.5f;
		base.ScaleWidth = 0.3f;
		base.DirectionDegree = 90 + base.Ran.Next(-4, 5);
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
		if (base.Time > 0)
		{
			base.ScaleLength += 0.07f;
			base.ScaleWidth += 0.03f;
			base.TransparentValueF -= base.MaxTransparent / base.LifeTime;
		}
		if (OutBoundary())
		{
			base.Background.BackgroundList.Remove(this);
		}
	}
}
