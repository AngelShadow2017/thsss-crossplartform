using System;
using System.Drawing;

namespace Shooting;

internal class BackgroundGroupSSS01
{
	public BackgroundGroupSSS01(StageDataPackage StageData)
	{
		BackgroundTouhouStage02_3D backgroundTouhouStage02_3D = new BackgroundTouhouStage02_3D(StageData)
		{
			TxtureObject = StageData.TextureObjectDictionary["BG01b"]
		};
		for (int i = 0; i < 20; i++)
		{
			BackgroundTree backgroundTree = new BackgroundTree(StageData, "Tree", new PointF(290f, 4200 - i * 200 + 100), InsertAtFirst: false)
			{
				Mirrored = true
			};
			new BackgroundTree(StageData, "Tree", new PointF(290f, 4200 - i * 200), InsertAtFirst: false);
			BackgroundTree backgroundTree2 = new BackgroundTree(StageData, "Tree", new PointF(-400f, 4200 - i * 200 + 162), InsertAtFirst: false)
			{
				Mirrored = true
			};
			new BackgroundTree(StageData, "Tree", new PointF(-400f, 4200 - i * 200 + 62), InsertAtFirst: false);
		}
		for (int i = 0; i < 10; i++)
		{
			Background3DObject background3DObject = new Background3DObject(StageData, "Cloud02", new PointF(0f, i * 500), InsertAtFirst: false)
			{
				Scale = 2f,
				Mirrored = false,
				OriginalPositionZ = -10f,
				Angle3DX = -(float)Math.PI,
				TransparentValueF = 50f,
				Velocity = 0.5f
			};
			Background3DObject background3DObject2 = new Background3DObject(StageData, "Cloud02", new PointF(-1024f, i * 500), InsertAtFirst: false)
			{
				Scale = 2f,
				Mirrored = false,
				OriginalPositionZ = -10f,
				Angle3DX = -(float)Math.PI,
				TransparentValueF = 50f,
				Velocity = 0.5f
			};
			Background3DObject background3DObject3 = new Background3DObject(StageData, "Cloud02", new PointF(-2048f, i * 500), InsertAtFirst: false)
			{
				Scale = 2f,
				Mirrored = false,
				OriginalPositionZ = -10f,
				Angle3DX = -(float)Math.PI,
				TransparentValueF = 50f,
				Velocity = 0.5f
			};
			Background3DObject background3DObject4 = new Background3DObject(StageData, "Cloud01", new PointF(0f, i * 500 + 250), InsertAtFirst: false)
			{
				Scale = 2f,
				Mirrored = true,
				OriginalPositionZ = -20f,
				Angle3DX = -(float)Math.PI,
				TransparentValueF = 50f,
				Velocity = -0.5f
			};
			Background3DObject background3DObject5 = new Background3DObject(StageData, "Cloud01", new PointF(1024f, i * 500 + 250), InsertAtFirst: false)
			{
				Scale = 2f,
				Mirrored = true,
				OriginalPositionZ = -20f,
				Angle3DX = -(float)Math.PI,
				TransparentValueF = 50f,
				Velocity = -0.5f
			};
			Background3DObject background3DObject6 = new Background3DObject(StageData, "Cloud01", new PointF(2048f, i * 500 + 250), InsertAtFirst: false)
			{
				Scale = 2f,
				Mirrored = true,
				OriginalPositionZ = -20f,
				Angle3DX = -(float)Math.PI,
				TransparentValueF = 50f,
				Velocity = -0.5f
			};
		}
	}
}
