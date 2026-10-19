using System;
using System.Drawing;

namespace Shooting;

public class BackgroundYoumu(StageDataPackage StageData, string textureName) : BackgroundRotate(StageData, textureName)
{
	private double[] lastAngle = new double[10];

	private float sc;

	public override void Ctrl()
	{
		base.Ctrl();
		for (int num = lastAngle.Length - 1; num > 0; num--)
		{
			lastAngle[num] = lastAngle[num - 1];
		}
		lastAngle[0] = base.Angle;
		if (base.StageData.SlowMode > 0)
		{
			base.Active = true;
			base.ColorValue = Color.Pink;
			base.AngularVelocityDegree = 1f;
		}
		else
		{
			base.Active = false;
			base.ColorValue = Color.White;
			sc = (float)Math.Sin((double)((float)base.Time / 103.5f) * Math.PI * 2.0) / 16f;
			base.Scale = 2.5f + sc;
			base.AngularVelocityDegree = -0.5f;
		}
	}

	public override void Show()
	{
		if (base.StageData.SlowMode > 0)
		{
			double num = base.Angle;
			int num2 = base.TransparentValue;
			for (int num3 = lastAngle.Length - 1; num3 >= 0; num3 -= 2)
			{
				base.Angle = lastAngle[num3];
				base.TransparentValueF = num2 * (lastAngle.Length - num3 + 15) / (lastAngle.Length + 1 + 15);
				base.Show();
			}
			base.Angle = num;
			base.TransparentValueF = num2;
		}
		base.Show();
	}
}
