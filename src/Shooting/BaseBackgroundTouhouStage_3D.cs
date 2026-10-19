using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

internal class BaseBackgroundTouhouStage_3D : BaseObject
{
	public VertexBuffer vertexBuffer;

	public VertexDeclaration vertexDecl;

	public Material material;

	public Device DeviceMain => base.StageData.DeviceMain;

	public BaseBackgroundTouhouStage_3D(StageDataPackage StageData, string textureName)
		: base(StageData, textureName, new PointF(0f, 0f), 0f, 0.0)
	{
		base.OriginalPosition = new PointF(0f, 0f);
		base.Background3D.BackgroundList.Add(this);
		vertexDecl = new VertexDeclaration(DeviceMain, TexturedVertex.VertexElements);
	}

	public void SetupMaterial()
	{
		material = default(Material);
		material.Diffuse = new Color4(Color.FromArgb(base.TransparentValue, base.ColorValue));
		material.Ambient = new Color4(Color.FromArgb(base.TransparentValue, base.ColorValue));
	}

	public override bool OutBoundary()
	{
		return false;
	}

	public virtual void SetupVertex()
	{
	}

	public virtual void SetupMatrices()
	{
		DeviceMain.SetTransform(TransformState.World, Matrix.Identity);
	}

	public override void Show()
	{
		SetupVertex();
		SetupMaterial();
		SetupMatrices();
		DeviceMain.Material = material;
		DeviceMain.SetTexture(0, base.TxtureObject.TXTure);
		DeviceMain.SetStreamSource(0, vertexBuffer, 0, TexturedVertex.SizeBytes);
		DeviceMain.VertexDeclaration = vertexDecl;
		DeviceMain.VertexFormat = TexturedVertex.Format;
	}

	public override void Dispose()
	{
		if (vertexBuffer != null)
		{
			vertexBuffer.Dispose();
			vertexDecl.Dispose();
		}
	}
}
