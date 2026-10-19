using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

internal class Background3DObject : BaseObject
{
	public IModel model { get; set; }

	public float Angle3DX { get; set; }

	public float Angle3DY { get; set; }

	public float Height { get; set; }

	public float OriginalPositionZ { get; set; }

	public float VelocityZ { get; set; }

	public Device DeviceMain => base.StageData.DeviceMain;

	public Background3DObject(StageDataPackage StageData, string modelName, PointF OriginalPosition, bool InsertAtFirst)
		: base(StageData, null, new PointF(0f, 0f), 0f, 0.0)
	{
		base.OriginalPosition = OriginalPosition;
		if (modelName != null)
		{
			model = StageData.ModelDictionary[modelName];
		}
		if (InsertAtFirst)
		{
			base.Background3D.BackgroundList.Insert(0, this);
		}
		else
		{
			base.Background3D.BackgroundList.Add(this);
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		OriginalPositionZ += VelocityZ;
	}

	public override bool OutBoundary()
	{
		if (base.Time > base.LifeTime && base.LifeTime > 0)
		{
			return true;
		}
		return false;
	}

	private void SetupMatrices()
	{
		Matrix identity = Matrix.Identity;
		identity *= Matrix.RotationZ((float)base.Angle);
		identity *= Matrix.Scaling(new Vector3(base.Scale, base.Scale, base.Scale));
		if (base.Mirrored)
		{
			identity *= Matrix.RotationY((float)Math.PI);
		}
		identity *= Matrix.RotationY(Angle3DY);
		identity *= Matrix.RotationX(Angle3DX);
		identity *= Matrix.Translation(base.OriginalPosition.X, base.OriginalPosition.Y, OriginalPositionZ + Height);
		DeviceMain.SetTransform(TransformState.World, identity);
	}

	public override void Show()
	{
		SetupMatrices();
		model.TransparentValue = base.TransparentValue;
		model.ColorValue = base.ColorValue;
		if (base.Active)
		{
			DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.One);
		}
		else
		{
			DeviceMain.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
		}
		model.SetModel();
		model.Draw();
	}

	public override void Dispose()
	{
	}
}
