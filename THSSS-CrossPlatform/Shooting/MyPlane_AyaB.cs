using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

public class MyPlane_AyaB : BaseMyPlane_Touhou
{
	public MyPlane_AyaB(StageDataPackage StageData, Point OriginalPosition)
		: base(StageData, OriginalPosition, "Aya")
	{
		base.WeaponType = "B";
		base.SubPlaneList = new List<BaseSubPlane>
		{
			new SubPlane_AyaB(StageData, base.Position),
			new SubPlane_AyaB(StageData, base.Position),
			new SubPlane_AyaB(StageData, base.Position),
			new SubPlane_AyaB(StageData, base.Position)
		};
		base.HighSpeed = 5f;
		base.LowSpeed = 2f;
	}

	public override void Shoot()
	{
		if (base.SpellList.Count > 0)
		{
			return;
		}
		if (base.Time % 3 == 0)
		{
			base.StageData.SoundPlay("se_plst00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			float x = base.OriginalPosition.X;
			float y = base.OriginalPosition.Y;
			new MyBullet_Aya(base.StageData, new PointF(x + 7f, y));
			new MyBullet_Aya(base.StageData, new PointF(x - 7f, y));
		}
		if (base.Time % 5 == 0)
		{
			for (int i = 0; i < base.PowerLevel; i++)
			{
				base.SubPlaneList[i].Shoot();
			}
		}
	}

	public override void Ctrl()
	{
		if (base.SpellList.Count <= 0)
		{
			base.Ctrl();
		}
	}

	public override void SpellShoot()
	{
		new Spell_AyaB(base.StageData);
		base.StageData.SoundPlay("se_cat00.wav");
		base.StageData.SoundPlay("se_nep00.wav");
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
				reference10 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 25f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 2:
			{
				ref PointF reference8 = ref base.SubPlanePoint[0];
				reference8 = new PointF(base.OriginalPosition.X - 6f, base.OriginalPosition.Y - 30f);
				ref PointF reference9 = ref base.SubPlanePoint[1];
				reference9 = new PointF(base.OriginalPosition.X + 6f, base.OriginalPosition.Y - 30f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 3:
			{
				ref PointF reference5 = ref base.SubPlanePoint[0];
				reference5 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 28f);
				ref PointF reference6 = ref base.SubPlanePoint[1];
				reference6 = new PointF(base.OriginalPosition.X - 10f, base.OriginalPosition.Y - 22f);
				ref PointF reference7 = ref base.SubPlanePoint[2];
				reference7 = new PointF(base.OriginalPosition.X + 10f, base.OriginalPosition.Y - 22f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[2].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 4:
			{
				ref PointF reference = ref base.SubPlanePoint[0];
				reference = new PointF(base.OriginalPosition.X - 6f, base.OriginalPosition.Y - 30f);
				ref PointF reference2 = ref base.SubPlanePoint[1];
				reference2 = new PointF(base.OriginalPosition.X + 6f, base.OriginalPosition.Y - 30f);
				ref PointF reference3 = ref base.SubPlanePoint[2];
				reference3 = new PointF(base.OriginalPosition.X - 15f, base.OriginalPosition.Y - 22f);
				ref PointF reference4 = ref base.SubPlanePoint[3];
				reference4 = new PointF(base.OriginalPosition.X + 15f, base.OriginalPosition.Y - 22f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[2].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[3].ShootDirection = -Math.PI / 2.0;
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
				reference20 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 35f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 2:
			{
				ref PointF reference18 = ref base.SubPlanePoint[0];
				reference18 = new PointF(base.OriginalPosition.X - 30f, base.OriginalPosition.Y - 0f);
				ref PointF reference19 = ref base.SubPlanePoint[1];
				reference19 = new PointF(base.OriginalPosition.X + 30f, base.OriginalPosition.Y - 0f);
				base.SubPlaneList[0].ShootDirection = -1.8207963267948966;
				base.SubPlaneList[1].ShootDirection = -1.3207963267948966;
				break;
			}
			case 3:
			{
				ref PointF reference15 = ref base.SubPlanePoint[0];
				reference15 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y + 30f);
				ref PointF reference16 = ref base.SubPlanePoint[1];
				reference16 = new PointF(base.OriginalPosition.X - 25f, base.OriginalPosition.Y + 20f);
				ref PointF reference17 = ref base.SubPlanePoint[2];
				reference17 = new PointF(base.OriginalPosition.X + 25f, base.OriginalPosition.Y + 20f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -1.920796320834432;
				base.SubPlaneList[2].ShootDirection = -1.220796332755361;
				break;
			}
			case 4:
			{
				ref PointF reference11 = ref base.SubPlanePoint[0];
				reference11 = new PointF(base.OriginalPosition.X - 15f, base.OriginalPosition.Y + 26f);
				ref PointF reference12 = ref base.SubPlanePoint[1];
				reference12 = new PointF(base.OriginalPosition.X + 15f, base.OriginalPosition.Y + 26f);
				ref PointF reference13 = ref base.SubPlanePoint[2];
				reference13 = new PointF(base.OriginalPosition.X - 30f, base.OriginalPosition.Y + 15f);
				ref PointF reference14 = ref base.SubPlanePoint[3];
				reference14 = new PointF(base.OriginalPosition.X + 30f, base.OriginalPosition.Y + 15f);
				base.SubPlaneList[0].ShootDirection = -1.8207963267948966;
				base.SubPlaneList[1].ShootDirection = -1.3207963267948966;
				base.SubPlaneList[2].ShootDirection = -2.0707963267948966;
				base.SubPlaneList[3].ShootDirection = -1.0707963267948966;
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

	public override void ShowCenter()
	{
		if (base.SpellList.Count <= 0)
		{
			base.ShowCenter();
		}
	}

	public override void Show()
	{
		if (base.SpellList.Count <= 0)
		{
			base.Show();
		}
	}
}
