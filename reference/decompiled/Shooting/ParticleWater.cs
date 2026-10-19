using System.Drawing;

namespace Shooting;

internal class ParticleWater : BaseEffect
{
	public TextureObject[] TxtureObjects { get; set; }

	public ParticleWater(StageDataPackage StageData, PointF p, float v, double drctn)
		: base(StageData, null, p, v, drctn)
	{
		TxtureObjects = new TextureObject[6]
		{
			base.TextureObjectDictionary["588-1"],
			base.TextureObjectDictionary["589-1"],
			base.TextureObjectDictionary["590-1"],
			base.TextureObjectDictionary["591-1"],
			base.TextureObjectDictionary["592-1"],
			base.TextureObjectDictionary["593-1"]
		};
		base.LifeTime = 17;
		base.Ay = 0.6f;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.TxtureObject = TxtureObjects[base.Time % 18 / 3];
	}
}
