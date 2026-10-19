using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class Model_Circle : Model, IModel
{
	private int Length = 64;

	private int Width = 16;

	private int Radius = 100;

	public Model_Circle(Device DeviceMain, TextureObject TxtureObject, string Name)
	{
		base.TransparentValue = 255;
		base.ColorValue = Color.White;
		base.DeviceMain = DeviceMain;
		base.TxtureObject = TxtureObject;
		base.Name = Name;
		vertexDecl = new VertexDeclaration(DeviceMain, TexturedVertex.VertexElements);
		vertexBuffer = new VertexBuffer(DeviceMain, 2 * (Length + 1) * TexturedVertex.SizeBytes, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
		material = default(Material);
		SetupVertex();
	}

	public override void SetupVertex()
	{
		float x = (float)base.TxtureObject.PosRect.X / (float)base.TxtureObject.SrcWidth;
		float x2 = (float)(base.TxtureObject.PosRect.X + base.TxtureObject.Width) / (float)base.TxtureObject.SrcWidth;
		TexturedVertex[] array = new TexturedVertex[2 * (Length + 1)];
		for (int i = 0; i <= Length; i++)
		{
			double num = (double)(i * 2) * Math.PI / (double)Length;
			int num2 = Radius - Width / 2;
			float y = (float)(i % (Length / 3)) / (float)(Length / 3);
			ref TexturedVertex reference = ref array[2 * i];
			reference = new TexturedVertex(new Vector3((float)num2 * (float)Math.Cos(num), (float)num2 * (float)Math.Sin(num), 0f), new Vector2(x, y));
			num2 = Radius + Width / 2;
			ref TexturedVertex reference2 = ref array[2 * i + 1];
			reference2 = new TexturedVertex(new Vector3((float)num2 * (float)Math.Cos(num), (float)num2 * (float)Math.Sin(num), 0f), new Vector2(x2, y));
		}
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		dataStream.WriteRange(array);
		vertexBuffer.Unlock();
	}

	public override void Draw()
	{
		SetupMaterial();
		base.DeviceMain.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2 * Length);
	}
}
