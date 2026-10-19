using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class BaseParticle3D : BaseObject
{
	public IModel model { get; set; }

	public Device DeviceMain => base.StageData.DeviceMain;

	public float Depth { get; set; }

	public float MaxDepth { get; set; }

	public double Direction3D { get; set; }

	public Vector3 RotatingAxis { get; set; }

	public float Angle3D { get; set; }

	public float AngularVelocity3D { get; set; }

	public BaseParticle3D(StageDataPackage StageData, string modelName, PointF OriginalPosition, float Velocity, double Direction)
		: base(StageData, modelName, OriginalPosition, Velocity, Direction)
	{
		base.OriginalPosition = OriginalPosition;
		model = StageData.ModelDictionary[modelName];
		MaxDepth = 80f;
	}

	public BaseParticle3D()
	{
	}

	public override void Move()
	{
		if (base.DestPoint != new PointF(0f, 0f))
		{
			_ = base.DestPoint;
			if (true)
			{
				MovePoint();
			}
		}
		float num = base.Velocity * (float)Math.Cos(Direction3D);
		base.Position = new PointF((float)((double)base.Position.X + (double)num * Math.Cos(base.Direction)), (float)((double)base.Position.Y + (double)num * Math.Sin(base.Direction)));
		Depth += base.Velocity * (float)Math.Sin(Direction3D);
		if (Depth > MaxDepth)
		{
			Depth = MaxDepth;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		Angle3D += AngularVelocity3D;
		if (base.Velocity < 0f)
		{
			base.Velocity = 0f;
		}
	}

	public virtual void SetupMatrices()
	{
		Matrix identity = Matrix.Identity;
		if (base.Mirrored)
		{
			identity *= Matrix.RotationY((float)Math.PI);
		}
		identity *= Matrix.Scaling(new Vector3(base.Scale, base.Scale, base.Scale));
		identity *= Matrix.RotationZ((float)base.Angle);
		identity *= Matrix.RotationAxis(RotatingAxis, base.Mirrored ? (0f - Angle3D) : Angle3D);
		identity *= Matrix.Translation(base.OriginalPosition.X - (float)(base.StageData.BoundRect.Width / 2), base.OriginalPosition.Y - (float)(base.StageData.BoundRect.Height / 2), Depth);
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
		model.Draw();
	}

	public override void Dispose()
	{
	}
}
