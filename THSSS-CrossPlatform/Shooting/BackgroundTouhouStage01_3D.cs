using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

internal class BackgroundTouhouStage01_3D : BaseBackgroundTouhouStage_3D
{
	public int Height { get; set; }

	public BackgroundTouhouStage01_3D(StageDataPackage StageData, string textureName)
		: base(StageData, textureName)
	{
		vertexBuffer = new VertexBuffer(base.DeviceMain, 4 * TexturedVertex.SizeBytes, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
	}

	public override void SetupVertex()
	{
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		dataStream.WriteRange(new TexturedVertex[4]
		{
			new TexturedVertex(new Vector3(-500f, 0f, 0f), new Vector2(0f, 0f)),
			new TexturedVertex(new Vector3(-500f, 1000f, 0f), new Vector2(0f, 1f)),
			new TexturedVertex(new Vector3(500f, 0f, 0f), new Vector2(1f, 0f)),
			new TexturedVertex(new Vector3(500f, 1000f, 0f), new Vector2(1f, 1f))
		});
		vertexBuffer.Unlock();
	}

	public override void SetupMatrices()
	{
		Matrix identity = Matrix.Identity;
		identity *= Matrix.Translation(base.OriginalPosition.X, base.OriginalPosition.Y, Height);
		base.DeviceMain.SetTransform(TransformState.World, identity);
	}

	public override void Show()
	{
		base.Show();
		base.OriginalPosition = new PointF(base.OriginalPosition.X - 1000f, base.OriginalPosition.Y);
		SetupMatrices();
		base.DeviceMain.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		base.OriginalPosition = new PointF(base.OriginalPosition.X + 1000f, base.OriginalPosition.Y);
		SetupMatrices();
		base.DeviceMain.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		base.OriginalPosition = new PointF(base.OriginalPosition.X + 1000f, base.OriginalPosition.Y);
		SetupMatrices();
		base.DeviceMain.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
		base.OriginalPosition = new PointF(base.OriginalPosition.X - 1000f, base.OriginalPosition.Y);
	}
}
