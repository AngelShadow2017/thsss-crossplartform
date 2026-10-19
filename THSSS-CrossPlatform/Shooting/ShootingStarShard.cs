using System;
using System.Drawing;

namespace Shooting;

internal class ShootingStarShard : BaseItem_Touhou
{
	public ShootingStarShard(StageDataPackage StageData, PointF OriginalPosition)
		: base(StageData, "StarShardBS", OriginalPosition)
	{
		base.Region = 8;
		base.AngularVelocity = 0f;
		base.DirectionDegree += (float)base.Ran.Next(-49, 50) / 10f;
		base.TransparentValueF = 0f;
		base.TransparentVelocity = 16f;
		base.Active = true;
		base.ColorValue = Color.Blue;
	}

	public override void HitCheckAll()
	{
		if (HitCheck(base.MyPlane, 48f))
		{
			base.Obtain = true;
		}
		if (HitCheck(base.MyPlane))
		{
			base.ItemList.Remove(this);
			if (base.ColorValue == Color.Red)
			{
				base.MyPlane.LastColor = EnchantmentType.Red;
			}
			else if (base.ColorValue == Color.Blue)
			{
				base.MyPlane.LastColor = EnchantmentType.Blue;
			}
			else if (base.ColorValue == Color.Green)
			{
				base.MyPlane.LastColor = EnchantmentType.Green;
			}
			base.MyPlane.StarPoint += 800;
			if (base.ColorValue == Color.Red)
			{
				base.MyPlane.LifeChip++;
			}
			else if (base.ColorValue == Color.Blue)
			{
				base.MyPlane.HighItemScore += 5000;
			}
			else if (base.ColorValue == Color.Green)
			{
				base.MyPlane.SpellChip++;
			}
			base.StageData.SoundPlay("se_item00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			for (int i = 0; i < 50; i++)
			{
				string textureName = "光点";
				ParticleSmaller particleSmaller = new ParticleSmaller(base.StageData, textureName, base.Position, base.Ran.Next(20, 200) / 10, (double)((float)base.Ran.Next(360) / 180f) * Math.PI);
				particleSmaller.LifeTime = 20;
				particleSmaller.Scale = (float)base.Ran.Next(100) / 100f;
				particleSmaller.TransparentValueF = 200f;
				particleSmaller.ColorValue = base.ColorValue;
			}
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 200)
		{
			base.ColorValue = Color.Red;
			base.TxtureObject = base.TextureObjectDictionary["StarShardRS"];
		}
		else if (base.Time == 300)
		{
			base.ColorValue = Color.Green;
			base.TxtureObject = base.TextureObjectDictionary["StarShardGS"];
		}
		base.GhostingColor = base.ColorValue;
		if (base.Time > 40)
		{
			base.AngularVelocityDegree = 3f;
			base.AngleDegree += base.AngularVelocityDegree;
		}
		if (base.Time % 4 == 0)
		{
			BaseEffect baseEffect = new BaseEffect(base.StageData, "StarShardW", base.Position, 0f, 0.0);
			baseEffect.Angle = base.Angle;
			baseEffect.ScaleVelocity = -0.0125f;
			baseEffect.TransparentVelocity = -3f;
			baseEffect.LifeTime = 80;
			baseEffect.ColorValue = base.ColorValue;
		}
	}

	public override void Show()
	{
		Color color = base.ColorValue;
		base.ColorValue = Color.White;
		base.Show();
		base.ColorValue = color;
	}
}
