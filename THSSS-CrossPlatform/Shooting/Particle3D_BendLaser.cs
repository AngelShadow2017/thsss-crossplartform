using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class Particle3D_BendLaser : BaseParticle3D
{
	private VertexBuffer vertexBuffer;

	private Material material;

	private int Length;

	private int Width;

	private int ColorType;

	public Particle3D_BendLaser(StageDataPackage StageData, int Length, int Width, int ColorType)
	{
		StageData.Particle3D.ParticleList.Add(this);
		base.Active = true;
		base.StageData = StageData;
		this.ColorType = ColorType;
		base.TxtureObject = base.TextureObjectDictionary["Laser0_" + ColorType];
		this.Length = Length;
		this.Width = Width;
		vertexBuffer = new VertexBuffer(base.DeviceMain, 2 * Length * TexturedVertex.SizeBytes, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
		material = default(Material);
	}

	public override void Ctrl()
	{
		base.Time++;
	}

	private void SetupVertex()
	{
		Length = base.GhostingCount;
		int num = base.GhostingCount - 1;
		while (num > 0)
		{
			double num2 = Math.Sqrt((base.LastPoints[num].X - base.LastPoints[num - 1].X) * (base.LastPoints[num].X - base.LastPoints[num - 1].X) + (base.LastPoints[num].Y - base.LastPoints[num - 1].Y) * (base.LastPoints[num].Y - base.LastPoints[num - 1].Y));
			if (num2 < 1.0)
			{
				Length--;
				num--;
				continue;
			}
			break;
		}
		if (Length < 2)
		{
			Length = 2;
		}
		TexturedVertex[] array = new TexturedVertex[2 * Length];
		double num3 = Math.Atan2(base.LastPoints[0].Y - base.LastPoints[1].Y, base.LastPoints[0].X - base.LastPoints[1].X);
		ref TexturedVertex reference = ref array[0];
		reference = new TexturedVertex(new Vector3(base.LastPoints[0].X - (float)(Width / 2) * (float)Math.Sin(num3), base.LastPoints[0].Y + (float)(Width / 2) * (float)Math.Cos(num3), 0f), new Vector2(0f, (float)ColorType / 16f));
		ref TexturedVertex reference2 = ref array[1];
		reference2 = new TexturedVertex(new Vector3(base.LastPoints[0].X + (float)(Width / 2) * (float)Math.Sin(num3), base.LastPoints[0].Y - (float)(Width / 2) * (float)Math.Cos(num3), 0f), new Vector2(0f, (float)(ColorType + 1) / 16f));
		for (num = 1; num < Length; num++)
		{
			ref TexturedVertex reference3 = ref array[2 * num];
			reference3 = new TexturedVertex(new Vector3(base.LastPoints[num].X - (float)(Width / 2) * (float)Math.Sin(num3), base.LastPoints[num].Y + (float)(Width / 2) * (float)Math.Cos(num3), 0f), new Vector2((float)num / ((float)Length - 1f), (float)ColorType / 16f));
			ref TexturedVertex reference4 = ref array[2 * num + 1];
			reference4 = new TexturedVertex(new Vector3(base.LastPoints[num].X + (float)(Width / 2) * (float)Math.Sin(num3), base.LastPoints[num].Y - (float)(Width / 2) * (float)Math.Cos(num3), 0f), new Vector2((float)num / ((float)Length - 1f), (float)(ColorType + 1) / 16f));
			if (base.LastPoints[num - 1] != base.LastPoints[num])
			{
				num3 = Math.Atan2(base.LastPoints[num - 1].Y - base.LastPoints[num].Y, base.LastPoints[num - 1].X - base.LastPoints[num].X);
			}
		}
		DataStream dataStream = vertexBuffer.Lock(0, 0, LockFlags.Discard);
		dataStream.WriteRange(array);
		vertexBuffer.Unlock();
	}

	public override bool OutBoundary()
	{
		return false;
	}

	public override void SetupMatrices()
	{
		Matrix identity = Matrix.Identity;
		identity *= Matrix.Translation(-base.StageData.BoundRect.Width / 2, -base.StageData.BoundRect.Height / 2, 0f);
		base.DeviceMain.SetTransform(TransformState.World, identity);
	}

	public override void Show()
	{
		SetupVertex();
		base.DeviceMain.SetStreamSource(0, vertexBuffer, 0, TexturedVertex.SizeBytes);
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
	}

	public override void Dispose()
	{
		vertexBuffer.Dispose();
	}
}
