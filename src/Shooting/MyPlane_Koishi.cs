using System;
using System.Collections.Generic;
using System.Drawing;

namespace Shooting;

public class MyPlane_Koishi : BaseMyPlane_Touhou
{
	private bool GodMode;

	public MyPlane_Koishi(StageDataPackage StageData, Point OriginalPosition)
		: base(StageData, OriginalPosition, "Koishi")
	{
		base.WeaponType = "A";
		base.SubPlaneList = new List<BaseSubPlane>
		{
			new SubPlane_Koishi(StageData, base.Position),
			new SubPlane_Koishi(StageData, base.Position),
			new SubPlane_Koishi(StageData, base.Position),
			new SubPlane_Koishi(StageData, base.Position)
		};
		base.HighSpeed = 5f;
		base.LowSpeed = 2f;
	}

	public override void Shoot()
	{
		if (base.Time % 3 == 0)
		{
			base.StageData.SoundPlay("se_plst00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			float x = base.OriginalPosition.X;
			float y = base.OriginalPosition.Y;
			BaseMyBullet_Touhou baseMyBullet_Touhou = new BaseMyBullet_Touhou(base.StageData, "KoishiBullet", new PointF(x + 7f, y), 30f, -Math.PI / 2.0);
			baseMyBullet_Touhou.Damage = 13;
			BaseMyBullet_Touhou baseMyBullet_Touhou2 = new BaseMyBullet_Touhou(base.StageData, "KoishiBullet", new PointF(x - 7f, y), 30f, -Math.PI / 2.0);
			baseMyBullet_Touhou2.Damage = 13;
		}
		if (base.Time % 10 == 0)
		{
			for (int i = 0; i < base.PowerLevel; i++)
			{
				base.SubPlaneList[i].Shoot();
			}
		}
	}

	public override void SpellShoot()
	{
		if (!GodMode)
		{
			new Spell_Koishi(base.StageData);
			GodMode = true;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		base.TransparentVelocity = (GodMode ? (-10) : 10);
		base.TransparentValueF += base.TransparentVelocity;
		base.SpellEnabled = !GodMode;
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
				reference10 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 44f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 2:
			{
				ref PointF reference8 = ref base.SubPlanePoint[0];
				reference8 = new PointF(base.OriginalPosition.X - 7f, base.OriginalPosition.Y - 36f);
				ref PointF reference9 = ref base.SubPlanePoint[1];
				reference9 = new PointF(base.OriginalPosition.X + 7f, base.OriginalPosition.Y - 36f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 3:
			{
				ref PointF reference5 = ref base.SubPlanePoint[2];
				reference5 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 36f);
				ref PointF reference6 = ref base.SubPlanePoint[1];
				reference6 = new PointF(base.OriginalPosition.X - 16f, base.OriginalPosition.Y - 29f);
				ref PointF reference7 = ref base.SubPlanePoint[0];
				reference7 = new PointF(base.OriginalPosition.X + 16f, base.OriginalPosition.Y - 29f);
				base.SubPlaneList[2].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 4:
			{
				ref PointF reference = ref base.SubPlanePoint[3];
				reference = new PointF(base.OriginalPosition.X - 7f, base.OriginalPosition.Y - 36f);
				ref PointF reference2 = ref base.SubPlanePoint[2];
				reference2 = new PointF(base.OriginalPosition.X + 7f, base.OriginalPosition.Y - 36f);
				ref PointF reference3 = ref base.SubPlanePoint[1];
				reference3 = new PointF(base.OriginalPosition.X - 16f, base.OriginalPosition.Y - 29f);
				ref PointF reference4 = ref base.SubPlanePoint[0];
				reference4 = new PointF(base.OriginalPosition.X + 16f, base.OriginalPosition.Y - 29f);
				base.SubPlaneList[3].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[2].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = -Math.PI / 2.0;
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
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
				reference20 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y - 44f);
				base.SubPlaneList[0].ShootDirection = -Math.PI / 2.0;
				break;
			}
			case 2:
			{
				ref PointF reference18 = ref base.SubPlanePoint[0];
				reference18 = new PointF(base.OriginalPosition.X - 32f, base.OriginalPosition.Y - 32f);
				ref PointF reference19 = ref base.SubPlanePoint[1];
				reference19 = new PointF(base.OriginalPosition.X + 32f, base.OriginalPosition.Y - 32f);
				base.SubPlaneList[0].ShootDirection = 3.9269908169872414;
				base.SubPlaneList[1].ShootDirection = 5.497787143782138;
				break;
			}
			case 3:
			{
				ref PointF reference15 = ref base.SubPlanePoint[2];
				reference15 = new PointF(base.OriginalPosition.X, base.OriginalPosition.Y + 44f);
				ref PointF reference16 = ref base.SubPlanePoint[1];
				reference16 = new PointF(base.OriginalPosition.X - 32f, base.OriginalPosition.Y - 32f);
				ref PointF reference17 = ref base.SubPlanePoint[0];
				reference17 = new PointF(base.OriginalPosition.X + 32f, base.OriginalPosition.Y - 32f);
				base.SubPlaneList[2].ShootDirection = Math.PI / 2.0;
				base.SubPlaneList[1].ShootDirection = 3.9269908169872414;
				base.SubPlaneList[0].ShootDirection = 5.497787143782138;
				break;
			}
			case 4:
			{
				ref PointF reference11 = ref base.SubPlanePoint[3];
				reference11 = new PointF(base.OriginalPosition.X - 32f, base.OriginalPosition.Y + 32f);
				ref PointF reference12 = ref base.SubPlanePoint[2];
				reference12 = new PointF(base.OriginalPosition.X + 32f, base.OriginalPosition.Y + 32f);
				ref PointF reference13 = ref base.SubPlanePoint[1];
				reference13 = new PointF(base.OriginalPosition.X - 32f, base.OriginalPosition.Y - 32f);
				ref PointF reference14 = ref base.SubPlanePoint[0];
				reference14 = new PointF(base.OriginalPosition.X + 32f, base.OriginalPosition.Y - 32f);
				base.SubPlaneList[3].ShootDirection = Math.PI * 3.0 / 4.0;
				base.SubPlaneList[2].ShootDirection = Math.PI / 4.0;
				base.SubPlaneList[1].ShootDirection = 3.9269908169872414;
				base.SubPlaneList[0].ShootDirection = 5.497787143782138;
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

	public override void PreMiss()
	{
		if (base.Time < base.UnmatchedTime || base.Time < base.UnmatchedTime || base.Time < base.DeadTime || base.SpellList.Count != 0)
		{
			return;
		}
		if (base.EnchantmentState == EnchantmentType.Green)
		{
			BulletRemover_Small bulletRemover_Small = new BulletRemover_Small(base.StageData, base.OriginalPosition);
			bulletRemover_Small.Region = 500;
			ChangeEnchantment(EnchantmentType.None);
			foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.LegacyItems(base.EnemyPlaneList)){
				if (x is BaseSpellCard)
				{
					((BaseSpellCard)x).Missed = true;
				}
			};
		}
		else if (GodMode)
		{
			GodMode = false;
			base.StageData.SoundPlay("ice005.wav");
			BulletRemover_Small bulletRemover_Small2 = new BulletRemover_Small(base.StageData, base.MyPlane.OriginalPosition);
			bulletRemover_Small2.Region = 150;
			base.Time = base.UnmatchedTime - 60;
			new ItemGetter(base.StageData);
			foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.LegacyItems(base.EnemyPlaneList)){
				if (x is BaseSpellCard)
				{
					((BaseSpellCard)x).Missed = true;
				}
			};
		}
		else
		{
			new Emitter(base.StageData, base.MyPlane.Position);
			base.StageData.SoundPlay("se_pldead00.wav", base.OriginalPosition.X / (float)base.BoundRect.Width);
			base.DeadTime = base.Time + 20;
		}
	}

	public override void Refresh()
	{
		if (GodMode)
		{
			SpellExtand();
			GodMode = false;
		}
		base.Refresh();
	}

	public override void Show()
	{
		if (base.Time >= base.DeadTime && base.Time != 0)
		{
			if (base.Time < base.UnmatchedTime && base.Time / 2 % 2 == 0)
			{
				base.SpriteMain.Draw2D(base.TxtureObject, 1f, 0f, base.Position, Color.FromArgb(base.TransparentValue, Color.DarkBlue));
			}
			else
			{
				base.SpriteMain.Draw2D(base.TxtureObject, 1f, 0f, base.Position, Color.FromArgb(base.TransparentValue, Color.White));
			}
			if (GodMode)
			{
				base.SpriteMain.Draw2D(base.TextureObjectDictionary["jiejie"], 1f, (0f - (float)base.Direction) * 4f, base.Position, Color.FromArgb(255 - base.TransparentValue, Color.White));
			}
			for (int i = 0; i < base.PowerLevel; i++)
			{
				base.SubPlaneList[i].Show();
			}
		}
	}
}
