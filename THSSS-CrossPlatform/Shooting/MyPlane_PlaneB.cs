using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

public class MyPlane_PlaneB : BaseMyPlane
{
	public MyPlane_PlaneB(StageDataPackage StageData, Point OriginalPosition)
		: base(StageData, OriginalPosition)
	{
		base.WeaponType = "B";
		base.SubPlaneList = new List<BaseSubPlane>
		{
			new SubPlane_PlaneB(StageData, base.Position),
			new SubPlane_PlaneB(StageData, base.Position),
			new SubPlane_PlaneB(StageData, base.Position),
			new SubPlane_PlaneB(StageData, base.Position)
		};
		base.HighSpeed = 4.5f;
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
			float x = base.Position.X;
			float y = base.Position.Y;
			new BaseMyBullet(base.StageData, "自机子弹", new PointF(x - 7f, y), 30f, -Math.PI / 2.0);
			new BaseMyBullet(base.StageData, "自机子弹", new PointF(x + 7f, y), 30f, -Math.PI / 2.0);
		}
		if (base.Time % 5 == 0)
		{
			for (int i = 0; i < base.PowerLevel; i++)
			{
				base.SubPlaneList[i].Shoot();
			}
		}
	}

	public override void SpellShoot()
	{
		new Spell_PlaneB(base.StageData);
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
				reference10 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 30f);
				break;
			}
			case 2:
			{
				ref PointF reference8 = ref base.SubPlanePoint[0];
				reference8 = new PointF(base.OriginalPosition.X - 12f, base.OriginalPosition.Y - 30f);
				ref PointF reference9 = ref base.SubPlanePoint[1];
				reference9 = new PointF(base.OriginalPosition.X + 12f, base.OriginalPosition.Y - 30f);
				break;
			}
			case 3:
			{
				ref PointF reference5 = ref base.SubPlanePoint[0];
				reference5 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 30f);
				ref PointF reference6 = ref base.SubPlanePoint[1];
				reference6 = new PointF(base.OriginalPosition.X + 15f, base.OriginalPosition.Y - 20f);
				ref PointF reference7 = ref base.SubPlanePoint[2];
				reference7 = new PointF(base.OriginalPosition.X - 15f, base.OriginalPosition.Y - 20f);
				break;
			}
			case 4:
			{
				ref PointF reference = ref base.SubPlanePoint[0];
				reference = new PointF(base.OriginalPosition.X - 8f, base.OriginalPosition.Y - 35f);
				ref PointF reference2 = ref base.SubPlanePoint[1];
				reference2 = new PointF(base.OriginalPosition.X + 8f, base.OriginalPosition.Y - 35f);
				ref PointF reference3 = ref base.SubPlanePoint[2];
				reference3 = new PointF(base.OriginalPosition.X - 22f, base.OriginalPosition.Y - 25f);
				ref PointF reference4 = ref base.SubPlanePoint[3];
				reference4 = new PointF(base.OriginalPosition.X + 22f, base.OriginalPosition.Y - 25f);
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
				reference20 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 40f);
				break;
			}
			case 2:
			{
				ref PointF reference18 = ref base.SubPlanePoint[0];
				reference18 = new PointF(base.OriginalPosition.X - 25f, base.OriginalPosition.Y - 20f);
				ref PointF reference19 = ref base.SubPlanePoint[1];
				reference19 = new PointF(base.OriginalPosition.X + 25f, base.OriginalPosition.Y - 20f);
				break;
			}
			case 3:
			{
				ref PointF reference15 = ref base.SubPlanePoint[0];
				reference15 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 40f);
				ref PointF reference16 = ref base.SubPlanePoint[1];
				reference16 = new PointF(base.OriginalPosition.X + 30f, base.OriginalPosition.Y - 20f);
				ref PointF reference17 = ref base.SubPlanePoint[2];
				reference17 = new PointF(base.OriginalPosition.X - 30f, base.OriginalPosition.Y - 20f);
				break;
			}
			case 4:
			{
				ref PointF reference11 = ref base.SubPlanePoint[0];
				reference11 = new PointF(base.OriginalPosition.X - 25f, base.OriginalPosition.Y - 0f);
				ref PointF reference12 = ref base.SubPlanePoint[1];
				reference12 = new PointF(base.OriginalPosition.X + 25f, base.OriginalPosition.Y - 0f);
				ref PointF reference13 = ref base.SubPlanePoint[2];
				reference13 = new PointF(base.OriginalPosition.X - 50f, base.OriginalPosition.Y + 5f);
				ref PointF reference14 = ref base.SubPlanePoint[3];
				reference14 = new PointF(base.OriginalPosition.X + 50f, base.OriginalPosition.Y + 5f);
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
