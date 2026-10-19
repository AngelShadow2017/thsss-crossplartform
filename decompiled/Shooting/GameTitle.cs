using System;
using System.Drawing;

namespace Shooting;

internal class GameTitle : BaseEffect
{
	public GameTitle(StageDataPackage StageData, string textureName, Point Position, Point DestPoint)
		: base(StageData)
	{
		base.TxtureObject = base.TextureObjectDictionary[textureName];
		base.Position = Position;
		base.DestPoint = DestPoint;
		base.Velocity = 5f;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.Angle = 0f - (float)(base.Direction - Math.PI / 2.0);
		base.TransparentValueF += 2f;
	}
}
