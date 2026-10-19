using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

public class MyPlane_Reimu : BaseMyPlane_Touhou
{
	public MyPlane_Reimu(StageDataPackage StageData, Point OriginalPosition)
		: base(StageData, OriginalPosition, "Reimu")
	{
		base.WeaponType = "A";
		base.SubPlaneList = new List<BaseSubPlane>
		{
			new Shinmyoumaru(StageData, base.Position),
			new SubPlane_Reimu(StageData, base.Position),
			new SubPlane_Reimu(StageData, base.Position),
			new SubPlane_Reimu(StageData, base.Position)
		};
		base.HighSpeed = 4.5f;
		base.LowSpeed = 2f;
	}

	public override void Shoot()
	{
		if (base.Time % 3 == 0)
		{
			base.StageData.SoundPlay("se_plst00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			float x = base.OriginalPosition.X;
			float y = base.OriginalPosition.Y;
			BaseMyBullet_Touhou baseMyBullet_Touhou = new BaseMyBullet_Touhou(base.StageData, "ReimuBullet", new PointF(x - 7f, y), 30f, -Math.PI / 2.0);
			baseMyBullet_Touhou.Damage = 14;
			BaseMyBullet_Touhou baseMyBullet_Touhou2 = new BaseMyBullet_Touhou(base.StageData, "ReimuBullet", new PointF(x + 7f, y), 30f, -Math.PI / 2.0);
			baseMyBullet_Touhou2.Damage = 14;
		}
		if (base.Time % 4 == 0)
		{
			for (int i = 0; i < base.PowerLevel; i++)
			{
				base.SubPlaneList[i].Shoot();
			}
		}
	}

	public override void SpellShoot()
	{
		if (base.KClass.Key_Shift)
		{
			new Spell_Reimu(base.StageData);
		}
		else
		{
			new Spell_Shinmyoumaru(base.StageData);
		}
	}

	public override void SubPlaneCtrl()
	{
		if (base.KClass.Key_Shift)
		{
			switch (base.PowerLevel)
			{
			case 1:
			{
				ref PointF reference10 = ref base.SubPlanePoint[0];
				reference10 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 70f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 2:
			{
				ref PointF reference8 = ref base.SubPlanePoint[0];
				reference8 = new PointF(base.OriginalPosition.X - 0f, base.OriginalPosition.Y - 70f);
				ref PointF reference9 = ref base.SubPlanePoint[1];
				reference9 = new PointF(base.OriginalPosition.X + 0f, base.OriginalPosition.Y - 70f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 3:
			{
				ref PointF reference5 = ref base.SubPlanePoint[0];
				reference5 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 70f);
				ref PointF reference6 = ref base.SubPlanePoint[1];
				reference6 = new PointF(base.OriginalPosition.X - 24f, base.OriginalPosition.Y - 0f);
				ref PointF reference7 = ref base.SubPlanePoint[2];
				reference7 = new PointF(base.OriginalPosition.X + 24f, base.OriginalPosition.Y - 0f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = 4.756022211684548;
				base.SubPlaneList[2].ShootDirection = 4.6687557490848315;
				break;
			}
			case 4:
			{
				ref PointF reference = ref base.SubPlanePoint[0];
				reference = new PointF(base.OriginalPosition.X - 0f, base.OriginalPosition.Y - 70f);
				ref PointF reference2 = ref base.SubPlanePoint[1];
				reference2 = new PointF(base.OriginalPosition.X + 0f, base.OriginalPosition.Y - 70f);
				ref PointF reference3 = ref base.SubPlanePoint[2];
				reference3 = new PointF(base.OriginalPosition.X - 24f, base.OriginalPosition.Y - 0f);
				ref PointF reference4 = ref base.SubPlanePoint[3];
				reference4 = new PointF(base.OriginalPosition.X + 24f, base.OriginalPosition.Y - 0f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[2].ShootDirection = 4.756022211684548;
				base.SubPlaneList[3].ShootDirection = 4.6687557490848315;
				break;
			}
			}
		}
		else
		{
			switch (base.PowerLevel)
			{
			case 1:
			{
				ref PointF reference20 = ref base.SubPlanePoint[0];
				reference20 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 48f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 2:
			{
				ref PointF reference18 = ref base.SubPlanePoint[0];
				reference18 = new PointF(base.OriginalPosition.X - 0f, base.OriginalPosition.Y - 48f);
				ref PointF reference19 = ref base.SubPlanePoint[1];
				reference19 = new PointF(base.OriginalPosition.X + 0f, base.OriginalPosition.Y + 48f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 3:
			{
				ref PointF reference15 = ref base.SubPlanePoint[0];
				reference15 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 48f);
				ref PointF reference16 = ref base.SubPlanePoint[1];
				reference16 = new PointF(base.OriginalPosition.X - 48f, base.OriginalPosition.Y + 0f);
				ref PointF reference17 = ref base.SubPlanePoint[2];
				reference17 = new PointF(base.OriginalPosition.X + 48f, base.OriginalPosition.Y + 0f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[2].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 4:
			{
				ref PointF reference11 = ref base.SubPlanePoint[0];
				reference11 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 48f);
				ref PointF reference12 = ref base.SubPlanePoint[1];
				reference12 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y + 48f);
				ref PointF reference13 = ref base.SubPlanePoint[2];
				reference13 = new PointF(base.OriginalPosition.X - 48f, base.OriginalPosition.Y + 0f);
				ref PointF reference14 = ref base.SubPlanePoint[3];
				reference14 = new PointF(base.OriginalPosition.X + 48f, base.OriginalPosition.Y + 0f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[2].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[3].ShootDirection = -Math.PI / 2.0;
				break;
			}
			}
		}
		for (int i = 0; i < base.PowerLevel; i++)
		{
			base.SubPlaneList[i].Enabled = true;
			base.SubPlaneList[i].DestPoint = base.SubPlanePoint[i];
			base.SubPlaneList[i].Ctrl();
		}
		for (int i = base.PowerLevel; i < base.SubPlaneList.Count; i++)
		{
			base.SubPlaneList[i].Enabled = false;
			base.SubPlaneList[i].Position = base.Position;
		}
	}
}
