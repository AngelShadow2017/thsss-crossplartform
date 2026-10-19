using System;
using System.Drawing;

namespace Shooting;

public class BulletPicEmitter01 : BaseBulleEmitter
{
	private bool[,] BulletPic;

	private BaseObject Bullet;

	private int indexY = 0;

	private int length;

	private byte ColorType;

	public BulletPicEmitter01(StageDataPackage StageData, string BPicName, BaseObject Bullet, PointF OriginalPosition, double Direction, byte ColorType)
		: base(StageData)
	{
		base.OriginalPosition = OriginalPosition;
		BulletPic = StageData.GlobalData.BulletPicDictionary[BPicName];
		this.Bullet = Bullet;
		this.ColorType = ColorType;
		length = (int)Math.Sqrt(BulletPic.Length);
		base.LifeTime = length * 2 + 10;
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time >= base.LifeTime)
		{
			foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
				x.Enabled = true;
			};
		}
		if (base.Time % 2 != 0 || indexY >= length)
		{
			return;
		}
		for (int num = 0; num < length; num++)
		{
			if (BulletPic[num, indexY])
			{
				PointF originalPosition = new PointF(base.OriginalPosition.X + (float)(num - length / 2) * 5f, base.OriginalPosition.Y + (float)(indexY - length / 2) * 5f);
				BaseBullet_Touhou baseBullet_Touhou = new BaseBullet_Touhou(base.StageData, null, originalPosition, 0f, 0.0);
				baseBullet_Touhou.Copy(Bullet);
				baseBullet_Touhou.OriginalPosition = originalPosition;
				baseBullet_Touhou.Velocity = (float)baseBullet_Touhou.GetDistance(base.OriginalPosition) * 3f / (float)length;
				baseBullet_Touhou.DirectionDegree = base.Ran.Next(360);
				baseBullet_Touhou.Enabled = false;
				base.StageData.SoundPlay("se_tan00a.wav");
			}
		}
		indexY++;
	}

	public override void Shoot()
	{
	}
}
