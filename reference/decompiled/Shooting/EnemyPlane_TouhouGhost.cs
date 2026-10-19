using System;
using System.Drawing;

namespace Shooting;

internal class EnemyPlane_TouhouGhost : BaseEnemyPlane_Touhou
{
	public bool BackFire { get; set; }

	public Color BackFireColor { get; set; }

	public EnemyPlane_TouhouGhost(StageDataPackage StageData, PointF OriginalPosition, float Velocity, double Direction, byte ColorType)
		: base(StageData, null, OriginalPosition, Velocity, Direction, ColorType)
	{
		base.HealthPoint = 30f;
		base.Scale = 0.2f;
		base.MaxScale = 1f;
		base.ScaleVelocity = 0.2f;
	}

	public override void GiveItems()
	{
		PointF originalPosition;
		if (base.ColorType == 0 || base.ColorType == 2)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-5, 5), base.OriginalPosition.Y + (float)base.Ran.Next(-5, 5));
			new PowerItem_Touhou(base.StageData, originalPosition);
		}
		else if (base.ColorType == 1 || base.ColorType == 3)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-5, 5), base.OriginalPosition.Y + (float)base.Ran.Next(-5, 5));
			new ScoreItem_Touhou(base.StageData, base.OriginalPosition);
		}
		if (base.Difficulty <= DifficultLevel.Normal)
		{
			return;
		}
		int num = ((base.Difficulty <= DifficultLevel.Hard) ? 1 : 8);
		double num2 = GetDirection(base.MyPlane);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				int num3 = base.ColorType * 3 + 2;
				new Bullet_Touhou_01(base.StageData, "bullet2_" + num3, base.OriginalPosition, 1.5f + (float)j * 0.4f, num2, (byte)num3);
			}
			num2 += Math.PI * 2.0 / (double)num;
		}
	}

	public override void Shoot()
	{
	}

	public override void TextureCtrl()
	{
		base.TxtureObject = base.TextureObjectDictionary["EnemyGhost" + base.ColorType + "_" + base.Time % 32 / 4];
		int maxValue = 6;
		PointF position = new PointF(base.Position.X + (float)base.Ran.Next(maxValue) * (float)Math.Sin((double)base.Ran.Next(360) * Math.PI / 180.0), base.Position.Y + (float)base.Ran.Next(maxValue) * (float)Math.Cos((double)base.Ran.Next(360) * Math.PI / 180.0));
		BaseEffect baseEffect = new BaseEffect(base.StageData, "EnemyGhost" + base.ColorType + "_" + base.Time % 32 / 4, position, 1.5f, -Math.PI / 2.0);
		baseEffect.Scale = 0.5f;
		baseEffect.LifeTime = 13;
		baseEffect.ScaleVelocity = -0.04f;
	}
}
