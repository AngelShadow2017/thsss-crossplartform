using System.Drawing;

namespace Shooting;

internal class BackgroundGroup01
{
	public BackgroundGroup01(StageDataPackage StageData)
	{
		BackgroundTouhouStage01B_3D backgroundTouhouStage01B_3D = new BackgroundTouhouStage01B_3D(StageData, "stage01a1")
		{
			OriginalPosition = new PointF(0f, 0f)
		};
		BackgroundTouhouStage01B_3D backgroundTouhouStage01B_3D2 = new BackgroundTouhouStage01B_3D(StageData, "stage01a1")
		{
			OriginalPosition = new PointF(0f, 1000f)
		};
		BackgroundTouhouStage01B_3D backgroundTouhouStage01B_3D3 = new BackgroundTouhouStage01B_3D(StageData, "stage01a1")
		{
			OriginalPosition = new PointF(0f, 2000f)
		};
		BackgroundTouhouStage01B_3D backgroundTouhouStage01B_3D4 = new BackgroundTouhouStage01B_3D(StageData, "stage01a1")
		{
			OriginalPosition = new PointF(0f, 3000f)
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D = new BackgroundTouhouStage01C_3D(StageData, "world02c")
		{
			OriginalPosition = new PointF(0f, 0f),
			TransparentValueF = 128f,
			Time = 300,
			Height = 50
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D2 = new BackgroundTouhouStage01C_3D(StageData, "world02c")
		{
			OriginalPosition = new PointF(0f, 1000f),
			TransparentValueF = 128f,
			Time = 300,
			Height = 50
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D3 = new BackgroundTouhouStage01C_3D(StageData, "world02c")
		{
			OriginalPosition = new PointF(0f, 2000f),
			TransparentValueF = 128f,
			Time = 300,
			Height = 50
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D4 = new BackgroundTouhouStage01C_3D(StageData, "world02c")
		{
			OriginalPosition = new PointF(0f, 3000f),
			TransparentValueF = 128f,
			Time = 300,
			Height = 50
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D5 = new BackgroundTouhouStage01C_3D(StageData, "world02c2")
		{
			OriginalPosition = new PointF(0f, 0f),
			TransparentValueF = 128f,
			DirectionDegree = 180.0,
			Height = 100
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D6 = new BackgroundTouhouStage01C_3D(StageData, "world02c2")
		{
			OriginalPosition = new PointF(0f, 1000f),
			TransparentValueF = 128f,
			DirectionDegree = 180.0,
			Height = 100
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D7 = new BackgroundTouhouStage01C_3D(StageData, "world02c2")
		{
			OriginalPosition = new PointF(0f, 2000f),
			TransparentValueF = 128f,
			DirectionDegree = 180.0,
			Height = 100
		};
		BackgroundTouhouStage01C_3D backgroundTouhouStage01C_3D8 = new BackgroundTouhouStage01C_3D(StageData, "world02c2")
		{
			OriginalPosition = new PointF(0f, 3000f),
			TransparentValueF = 128f,
			DirectionDegree = 180.0,
			Height = 100
		};
	}
}
