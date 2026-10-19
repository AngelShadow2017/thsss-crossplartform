using System;
using System.Drawing;

namespace Shooting;

public class BaseBullet : BaseBullet_Touhou
{
	private int highLightValue = 0;

	public int HighLightValue
	{
		get
		{
			return highLightValue;
		}
		set
		{
			if (value < 0)
			{
				highLightValue = 0;
			}
			else if (value > 255)
			{
				highLightValue = 255;
			}
			else
			{
				highLightValue = value;
			}
		}
	}

	public BaseBullet(StageDataPackage StageData, string textureName, PointF OriginalPosition, float Velocity, double Direction)
		: base(StageData, textureName, OriginalPosition, Velocity, Direction)
	{
		HighLightValue = 200;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (HighLightValue > 0)
		{
			HighLightValue -= 10;
		}
		else
		{
			HighLightValue = 0;
		}
	}

	public override void HitCheckAll()
	{
		if (base.MyPlane.HitEnabled && HitCheck(base.MyPlane))
		{
			base.MyPlane.PreMiss();
		}
	}

	public override void GiveEndEffect()
	{
		ParticleSmaller particleSmaller = new ParticleSmaller(base.StageData, "光点", base.Position, base.Velocity, base.Direction);
		particleSmaller.LifeTime = 30;
	}

	public override void Show()
	{
		base.Show();
		if (HighLightValue > 0)
		{
			int width = base.TextureObjectDictionary["光点"].Width;
			int height = base.TextureObjectDictionary["光点"].Height;
			SizeF destinationSize = new SizeF((float)width * base.Scale, (float)height * base.Scale);
			base.SpriteMain.Draw2D(base.TextureObjectDictionary["光点"].TXTure, base.TextureObjectDictionary["光点"].PosRect, destinationSize, base.TextureObjectDictionary["光点"].RotatingCenter, (float)(base.Direction - Math.PI / 2.0), base.Position, Color.FromArgb(HighLightValue, Color.White));
		}
	}
}
