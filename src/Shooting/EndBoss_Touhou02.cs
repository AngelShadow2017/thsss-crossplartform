using System.Drawing;
using Shooting.Planes.Story;

namespace Shooting;

internal class EndBoss_Touhou02 : EndBoss_Touhou
{
	public EndBoss_Touhou02(StageDataPackage StageData, PointF OriginalPosition, Color Color1, Color Color2)
		: base(StageData, OriginalPosition, Color1, Color2)
	{
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == base.LifeTime)
		{
			switch (base.MyPlane.Name)
			{
			case "Aya":
				new Story_SSS02_02(base.StageData);
				break;
			case "Reimu":
				new Story_SSS02_02A(base.StageData);
				break;
			case "Marisa":
				new Story_SSS02_02B(base.StageData);
				break;
			case "Sanae":
				new Story_SSS02_02C(base.StageData);
				break;
			case "Koishi":
				new Story_SSS02_02D(base.StageData);
				break;
			default:
				new Story_SSS02_02X(base.StageData);
				break;
			}
		}
	}
}
