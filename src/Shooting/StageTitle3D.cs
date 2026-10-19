using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class StageTitle3D : BaseParticle3D
{
	protected VertexBuffer vertexBuffer;

	protected VertexDeclaration vertexDecl;

	protected Material material;

	protected int Length = 40;

	public StageTitle3D(StageDataPackage StageData, string TextureName)
	{
		base.StageData = StageData;
		base.OriginalPosition = new PointF(base.BoundRect.Width / 2, base.BoundRect.Height / 2);
		if (TextureName != null)
		{
			base.TxtureObject = base.TextureObjectDictionary[TextureName];
		}
		StageData.Particle3D.ParticleList.Add(this);
		vertexDecl = new VertexDeclaration(base.DeviceMain, TexturedColoredVertex.VertexElements);
		vertexBuffer = new VertexBuffer(base.DeviceMain, 2 * Length * TexturedColoredVertex.SizeBytes, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
		material = default(Material);
		SetupVertex();
		base.LifeTime = 300;
		base.TransparentValueF = 0f;
		base.ColorValue = Color.White;
		base.Active = false;
	}

	public virtual void SetupVertex()
	{
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		float x = 0f - base.TxtureObject.RotatingCenter.X;
		float x2 = (float)base.TxtureObject.Width - base.TxtureObject.RotatingCenter.X;
		float num = 0f - base.TxtureObject.RotatingCenter.Y;
		float num2 = (float)base.TxtureObject.Height - base.TxtureObject.RotatingCenter.Y;
		float x3 = (float)base.TxtureObject.PosRect.X / (float)base.TxtureObject.SrcWidth;
		float x4 = (float)(base.TxtureObject.PosRect.X + base.TxtureObject.Width) / (float)base.TxtureObject.SrcWidth;
		float num3 = (float)base.TxtureObject.PosRect.Y / (float)base.TxtureObject.SrcHeight;
		float num4 = (float)(base.TxtureObject.PosRect.Y + base.TxtureObject.Height) / (float)base.TxtureObject.SrcHeight;
		TexturedColoredVertex[] array = new TexturedColoredVertex[2 * Length];
		for (int i = 0; i < Length; i++)
		{
			int num5 = -10 * i + base.Time * 3;
			if (num5 < 0)
			{
				num5 = 0;
			}
			else if (num5 > base.TransparentValue)
			{
				num5 = base.TransparentValue;
			}
			ref TexturedColoredVertex reference = ref array[2 * i];
			reference = new TexturedColoredVertex(new Vector3(x, num + (num2 - num) * (float)i / (float)Length, 0f), new Vector2(x3, num3 + (num4 - num3) * (float)i / (float)Length), Color.FromArgb(num5, base.ColorValue).ToArgb());
			ref TexturedColoredVertex reference2 = ref array[2 * i + 1];
			reference2 = new TexturedColoredVertex(new Vector3(x2, num + (num2 - num) * (float)i / (float)Length, 0f), new Vector2(x4, num3 + (num4 - num3) * (float)i / (float)Length), Color.FromArgb(num5, base.ColorValue).ToArgb());
		}
		dataStream.WriteRange(array);
		vertexBuffer.Unlock();
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time < base.LifeTime / 3)
		{
			base.TransparentValueF += (float)base.MaxTransparent * 3f / (float)base.LifeTime;
		}
		else if (base.Time > base.LifeTime * 2 / 3)
		{
			base.TransparentValueF -= (float)base.MaxTransparent * 3f / (float)base.LifeTime;
		}
	}

	public override void Show()
	{
		base.DeviceMain.EnableLight(0, enable: false);
		base.DeviceMain.SetRenderState(RenderState.Lighting, value: false);
		base.DeviceMain.VertexDeclaration = vertexDecl;
		base.DeviceMain.VertexFormat = TexturedColoredVertex.Format;
		SetupVertex();
		base.DeviceMain.SetStreamSource(0, vertexBuffer, 0, TexturedColoredVertex.SizeBytes);
		base.DeviceMain.SetTexture(0, base.TxtureObject.TXTure);
		SetupMatrices();
		if (base.Active)
		{
			base.DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.One);
		}
		else
		{
			base.DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
		}
		material.Diffuse = new Color4(Color.FromArgb(base.TransparentValue, base.ColorValue));
		material.Ambient = new Color4(Color.FromArgb(base.TransparentValue, base.ColorValue));
		base.DeviceMain.Material = material;
		base.DeviceMain.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2 * Length - 2);
		base.DeviceMain.EnableLight(0, enable: true);
		base.DeviceMain.SetRenderState(RenderState.Lighting, value: true);
		base.DeviceMain.VertexDeclaration = base.StageData.Particle3D.vertexDecl;
		base.DeviceMain.VertexFormat = TexturedVertex.Format;
	}
}
