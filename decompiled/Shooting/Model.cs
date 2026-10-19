using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class Model : IModel
{
	public VertexBuffer vertexBuffer;

	public VertexDeclaration vertexDecl;

	public Material material;

	public string Name { get; set; }

	public TextureObject TxtureObject { get; set; }

	public Device DeviceMain { get; set; }

	public int TransparentValue { get; set; }

	public Color ColorValue { get; set; }

	public Model()
	{
	}

	public Model(Device DeviceMain, TextureObject TxtureObject, string Name)
	{
		TransparentValue = 255;
		ColorValue = Color.White;
		this.DeviceMain = DeviceMain;
		this.TxtureObject = TxtureObject;
		this.Name = Name;
		vertexDecl = new VertexDeclaration(DeviceMain, TexturedVertex.VertexElements);
		vertexBuffer = new VertexBuffer(DeviceMain, 4 * TexturedVertex.SizeBytes, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
		material = default(Material);
		SetupVertex();
	}

	public virtual void SetupVertex()
	{
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		float x = 0f - TxtureObject.RotatingCenter.X;
		float x2 = (float)TxtureObject.Width - TxtureObject.RotatingCenter.X;
		float y = 0f - TxtureObject.RotatingCenter.Y;
		float y2 = (float)TxtureObject.Height - TxtureObject.RotatingCenter.Y;
		float x3 = (float)TxtureObject.PosRect.X / (float)TxtureObject.SrcWidth;
		float x4 = (float)(TxtureObject.PosRect.X + TxtureObject.Width) / (float)TxtureObject.SrcWidth;
		float y3 = (float)TxtureObject.PosRect.Y / (float)TxtureObject.SrcHeight;
		float y4 = (float)(TxtureObject.PosRect.Y + TxtureObject.Height) / (float)TxtureObject.SrcHeight;
		dataStream.WriteRange(new TexturedVertex[4]
		{
			new TexturedVertex(new Vector3(x, y, 0f), new Vector2(x3, y3)),
			new TexturedVertex(new Vector3(x2, y, 0f), new Vector2(x4, y3)),
			new TexturedVertex(new Vector3(x, y2, 0f), new Vector2(x3, y4)),
			new TexturedVertex(new Vector3(x2, y2, 0f), new Vector2(x4, y4))
		});
		vertexBuffer.Unlock();
	}

	public void SetupMaterial()
	{
		material.Diffuse = new Color4(Color.FromArgb(TransparentValue, ColorValue));
		material.Ambient = new Color4(Color.FromArgb(TransparentValue, ColorValue));
		DeviceMain.Material = material;
	}

	public virtual void SetModel()
	{
		DeviceMain.SetStreamSource(0, vertexBuffer, 0, TexturedVertex.SizeBytes);
		DeviceMain.SetTexture(0, TxtureObject.TXTure);
	}

	public void SetVertexFormat()
	{
		DeviceMain.VertexDeclaration = vertexDecl;
		DeviceMain.VertexFormat = TexturedVertex.Format;
	}

	public virtual void Draw()
	{
		SetupMaterial();
		DeviceMain.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
	}

	public virtual void Dispose()
	{
		if (vertexBuffer != null)
		{
			vertexBuffer.Dispose();
			vertexDecl.Dispose();
		}
	}
}
