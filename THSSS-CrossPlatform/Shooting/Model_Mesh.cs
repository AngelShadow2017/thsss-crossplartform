using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class Model_Mesh : Model, IModel
{
	public GlobalDataPackage GlobalData;

	private Mesh mesh;

	private ExtendedMaterial[] materials;

	private Texture[] textures;

	public bool CullMode = false;

	public Model_Mesh(Device DeviceMain, string ModelName)
	{
		base.TransparentValue = 255;
		base.ColorValue = Color.White;
		base.DeviceMain = DeviceMain;
		base.Name = ModelName;
		SetupVertex();
	}

	public override void SetupVertex()
	{
		this.mesh = Mesh.FromFile(base.DeviceMain, "Model/" + base.Name + ".x", MeshFlags.Managed);
		materials = this.mesh.GetMaterials();
		if ((this.mesh.VertexFormat & VertexFormat.Normal) == 0)
		{
			Mesh mesh = this.mesh.Clone(base.DeviceMain, MeshFlags.SystemMemory, this.mesh.VertexFormat | VertexFormat.Normal);
			mesh.ComputeNormals();
			this.mesh.Dispose();
			this.mesh = mesh;
		}
		if (materials.Length < 1)
		{
			return;
		}
		textures = new Texture[materials.Length];
		for (int i = 0; i < materials.Length; i++)
		{
			textures[i] = null;
			if (!string.IsNullOrEmpty(materials[i].TextureFileName))
			{
				textures[i] = Texture.FromFile(base.DeviceMain, "Model/" + materials[i].TextureFileName);
			}
		}
	}

	public override void SetModel()
	{
	}

	public override void Draw()
	{
		for (int i = 0; i < materials.Length; i++)
		{
			Material materialD3D = materials[i].MaterialD3D;
			materialD3D.Diffuse = new Color4(Color.FromArgb(base.TransparentValue, base.ColorValue));
			materialD3D.Ambient = new Color4(Color.FromArgb(base.TransparentValue, base.ColorValue));
			base.DeviceMain.Material = materialD3D;
			base.DeviceMain.SetTexture(0, textures[i]);
			mesh.DrawSubset(i);
		}
		base.DeviceMain.VertexFormat = TexturedVertex.Format;
	}

	public override void Dispose()
	{
		Texture[] array = textures;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Dispose();
		}
		mesh.Dispose();
	}
}
