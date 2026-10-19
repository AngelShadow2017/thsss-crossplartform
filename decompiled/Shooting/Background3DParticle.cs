using System;
using System.Drawing;

namespace Shooting;

internal class Background3DParticle : Background3DObject
{
	private double tr;

	public Background3DParticle(StageDataPackage StageData, string modelName, PointF OriginalPosition, bool InsertAtFirst)
		: base(StageData, modelName, OriginalPosition, InsertAtFirst)
	{
	}

	public override void Show()
	{
		float transparentValueF = base.TransparentValueF;
		tr = 50.0 * Math.Sin((float)base.Time / 2f);
		base.TransparentValueF += (int)tr;
		base.Show();
		base.TransparentValueF = transparentValueF;
	}
}
