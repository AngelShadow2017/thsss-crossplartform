using System;
using System.Drawing;

namespace Shooting;

public class Shinmyoumaru : BaseSubPlane
{
	public float IndexX { get; set; }

	public int IndexY { get; set; }

	public Shinmyoumaru(StageDataPackage StageData, PointF Position)
		: base(StageData, "ReimuSubPlane", Position)
	{
		base.AngularVelocityDegree = 5f;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		TextureCtrl();
	}

	public void TextureCtrl()
	{
		if (!base.KClass.Key_Shift)
		{
			base.TxtureObject = base.TextureObjectDictionary["ReimuSubPlane"];
			return;
		}
		base.Angle = Math.PI / 2.0;
		int indexY = IndexY;
		if (base.Vx > 0.5f)
		{
			IndexY = 2;
			if (IndexX < 4f)
			{
				IndexX += 0.5f;
			}
			else
			{
				IndexX = 4 + base.Time % 24 / 6;
			}
		}
		else if (base.Vx < -0.5f)
		{
			IndexY = 1;
			if (IndexX < 4f)
			{
				IndexX += 0.5f;
			}
			else
			{
				IndexX = 4 + base.Time % 24 / 6;
			}
		}
		else
		{
			IndexY = 0;
			IndexX = base.Time % 48 / 6;
		}
		if (indexY != IndexY)
		{
			IndexX = 1f;
		}
		if (IndexX > 7f)
		{
			IndexX = 7f;
		}
		string key = "Maru" + IndexY + (int)IndexX;
		base.TxtureObject = base.TextureObjectDictionary[key];
	}

	public override void Shoot()
	{
		if (base.KClass.Key_Shift)
		{
			BaseMyBullet_Touhou baseMyBullet_Touhou = new BaseMyBullet_Touhou(base.StageData, "MaruBullet", base.OriginalPosition, 18f, base.ShootDirection + Math.PI / 60.0);
			baseMyBullet_Touhou.Angle = 1.5707963705062866;
			baseMyBullet_Touhou.Damage = 5;
			BaseMyBullet_Touhou baseMyBullet_Touhou2 = new BaseMyBullet_Touhou(base.StageData, "MaruBullet", base.OriginalPosition, 18f, base.ShootDirection - Math.PI / 60.0);
			baseMyBullet_Touhou2.Angle = 1.5707963705062866;
			baseMyBullet_Touhou2.Damage = 5;
		}
		else
		{
			BulletGuidance bulletGuidance = new BulletGuidance(base.StageData, "ReimuSubBullet", base.Position, 8f, base.ShootDirection);
			bulletGuidance.TransparentValueF = 180f;
			bulletGuidance.Angle = 1.5707963705062866;
			bulletGuidance.Damage = 9;
		}
	}

	public override void Show()
	{
		if (!base.Enabled || base.TxtureObject == null)
		{
			return;
		}
		if (!base.KClass.Key_Shift)
		{
			double num = (double)(-base.Time * 5 % 360) * Math.PI / 180.0;
			if (base.MyPlane.Power >= 400)
			{
				base.SpriteMain.Draw2D(base.TxtureObject, 1.3f, 1.3f, 0f - (float)num, base.Position, Color.FromArgb(base.TransparentValue * 6 / 10, base.ColorValue), base.Mirrored);
			}
			base.SpriteMain.Draw2D(base.TxtureObject, base.ScaleWidth, base.ScaleLength, (float)num, base.Position, Color.FromArgb(base.TransparentValue, base.ColorValue), base.Mirrored);
		}
		else
		{
			base.Show();
		}
	}
}
