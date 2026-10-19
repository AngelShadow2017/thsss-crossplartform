using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class Model_Triangle : Model, IModel
{
	public Vector2 LeftTop;

	public Vector2 RightTop;

	public Vector2 LeftBottom;

	public int Width;

	public int Height;

	public Model_Triangle()
	{
	}

	public Model_Triangle(Device DeviceMain, TextureObject TxtureObject, string Name, Vector2 LeftTop, Vector2 RightTop, Vector2 LeftBottom, int Width, int Height)
	{
		base.TransparentValue = 255;
		base.ColorValue = Color.White;
		base.DeviceMain = DeviceMain;
		base.TxtureObject = TxtureObject;
		base.Name = Name;
		this.LeftTop = LeftTop;
		this.RightTop = RightTop;
		this.LeftBottom = LeftBottom;
		this.Width = Width;
		this.Height = Height;
		vertexDecl = new VertexDeclaration(DeviceMain, TexturedVertex.VertexElements);
		vertexBuffer = new VertexBuffer(DeviceMain, 3 * TexturedVertex.SizeBytes, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
		material = default(Material);
		SetupVertex();
	}

	public override void SetupVertex()
	{
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		dataStream.WriteRange(new TexturedVertex[3]
		{
			new TexturedVertex(new Vector3(0f, 0f, 0f), LeftTop),
			new TexturedVertex(new Vector3(Width, 0f, 0f), RightTop),
			new TexturedVertex(new Vector3(0f, Height, 0f), LeftBottom)
		});
		vertexBuffer.Unlock();
	}

	public override void SetModel()
	{
		base.DeviceMain.SetStreamSource(0, vertexBuffer, 0, TexturedVertex.SizeBytes);
		base.DeviceMain.SetTexture(0, base.TxtureObject.TXTure);
	}

	public override void Draw()
	{
		SetupMaterial();
		base.DeviceMain.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
	}
}
