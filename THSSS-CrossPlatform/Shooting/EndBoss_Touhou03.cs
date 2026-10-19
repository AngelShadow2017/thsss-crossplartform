using System.Drawing;
using Shooting.Planes.Story;

namespace Shooting;

internal class EndBoss_Touhou03 : EndBoss_Touhou
{
	public EndBoss_Touhou03(StageDataPackage StageData, PointF OriginalPosition, Color Color1, Color Color2)
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
				new Story_SSS03_02(base.StageData);
				break;
			case "Reimu":
				new Story_SSS03_02A(base.StageData);
				break;
			case "Marisa":
				new Story_SSS03_02B(base.StageData);
				break;
			case "Sanae":
				new Story_SSS03_02C(base.StageData);
				break;
			case "Koishi":
				new Story_SSS03_02D(base.StageData);
				break;
			default:
				new Story_SSS03_02X(base.StageData);
				break;
			}
		}
	}
}
