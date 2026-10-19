using System.Drawing;

namespace Shooting;

internal class FullPic2 : BaseObject
{
	public FullPic2(StageDataPackage StageData, string textureName)
	{
		base.StageData = StageData;
		base.Position = new PointF(base.BoundRect.Left + 440, base.BoundRect.Top + 200);
		base.Velocity = 9f;
		base.DirectionDegree = 165.0;
		base.Active = false;
		base.LifeTime = 200;
		base.Accelerate = -0.2f;
		AccelerateDictionary.Add(90, 1f);
		TransparentVelocityDictionary.Add(120, -6f);
		base.AngleWithDirection = false;
		base.AngleDegree = 90.0;
		base.TxtureObject = base.TextureObjectDictionary[textureName];
		base.Background2.BackgroundList.Add(this);
		base.OutsideRegion = 500;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Velocity < 1.2f)
		{
			base.Velocity = 1.2f;
		}
	}
}
