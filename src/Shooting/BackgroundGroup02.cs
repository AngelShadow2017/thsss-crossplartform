using System.Drawing;

namespace Shooting;

internal class BackgroundGroup02
{
	public BackgroundGroup02(StageDataPackage StageData)
	{
		new BackgroundTouhouStage02_3D(StageData);
		for (int i = 0; i < 20; i++)
		{
			BackgroundTree backgroundTree = new BackgroundTree(StageData, "Background_Sunflowers", new PointF(StageData.Ran.Next(25, 300), 5400 - i * 50 + 25), InsertAtFirst: false)
			{
				Mirrored = true,
				Scale = 0.5f
			};
			BackgroundTree backgroundTree2 = new BackgroundTree(StageData, "Background_Sunflowers", new PointF(StageData.Ran.Next(-300, -25), 5400 - i * 50 + 31), InsertAtFirst: false)
			{
				Mirrored = true,
				Scale = 0.5f
			};
			BackgroundTree backgroundTree3 = new BackgroundTree(StageData, "Background_Sunflowers", new PointF(StageData.Ran.Next(-300, -25), 5400 - i * 50 + 6), InsertAtFirst: false)
			{
				Scale = 0.5f
			};
			BackgroundTree backgroundTree4 = new BackgroundTree(StageData, "Background_Sunflowers", new PointF(StageData.Ran.Next(25, 300), 5400 - i * 50), InsertAtFirst: false)
			{
				Scale = 0.5f
			};
		}
		for (int i = 0; i < 20; i++)
		{
			BackgroundTree backgroundTree5 = new BackgroundTree(StageData, "ea1655f0", new PointF(300f, 4200 - i * 200 + 100), InsertAtFirst: false)
			{
				Mirrored = true
			};
			new BackgroundTree(StageData, "ea1655f0", new PointF(290f, 4200 - i * 200), InsertAtFirst: false);
			BackgroundTree backgroundTree6 = new BackgroundTree(StageData, "ea1655f0", new PointF(-400f, 4200 - i * 200 + 162), InsertAtFirst: false)
			{
				Mirrored = true
			};
			new BackgroundTree(StageData, "ea1655f0", new PointF(-410f, 4200 - i * 200 + 62), InsertAtFirst: false);
		}
	}
}
