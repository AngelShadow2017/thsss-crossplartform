using System;
using System.Drawing;

namespace Shooting;

internal class CharacterName : BaseStoryItem
{
	public CharacterName(StageDataPackage StageData, string textureName)
		: base(StageData)
	{
		base.Position = new PointF(base.BoundRect.Right - 70, base.BoundRect.Top + 300);
		base.Velocity = 0f;
		base.Direction = Math.PI / 2.0;
		base.TransparentValueF = 0f;
		base.TxtureObject = base.TextureObjectDictionary[textureName];
		TransparentVelocityDictionary.Add(10, 20f);
		TransparentVelocityDictionary.Add(210, -10f);
	}
}
