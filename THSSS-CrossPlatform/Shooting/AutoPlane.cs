using System;
using System.Drawing;
using SlimDX;

namespace Shooting;

public class AutoPlane(StageDataPackage StageData, Point Position) : BaseMyPlane(StageData, Position)
{
	private int CtrlCircle = 1;

	private KeyClass KC = new KeyClass();

	public override void Ctrl()
	{
		if (base.Time % CtrlCircle == 0)
		{
			KC = new KeyClass();
			KC.Key_Z = true;
			double num = 0.0;
			double num2 = 0.0;
			Vector2 vector = GenerateMove();
			num = 0f - vector.X;
			num2 = 0f - vector.Y;
			if (vector.Length() < 80f)
			{
				KC.Key_Shift = true;
			}
			if (num > 1.0)
			{
				KC.ArrowLeft = true;
			}
			else if (num < -1.0)
			{
				KC.ArrowRight = true;
			}
			if (num2 > 1.0)
			{
				KC.ArrowUp = true;
			}
			else if (num2 < -1.0)
			{
				KC.ArrowDown = true;
			}
		}
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			if (x.HitCheck(this, x.Region) || base.DeadTime > base.Time)
			{
				KC.Key_X = true;
			}
		};
		base.StageData.GlobalData.KClass.Hex2Key(KC.Key2Hex());
		base.Ctrl();
	}

	private double GenerateDanger(PointF OriginalPos)
	{
		double tmp = 0.0;
		double d;
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			Vector2 vector = new Vector2(OriginalPos.X, OriginalPos.Y);
			Vector2 vector2 = new Vector2(x.OriginalPosition.X, x.OriginalPosition.Y);
			d = (vector - vector2).Length() - (float)base.Region - (float)x.Region;
			if (d < 200.0)
			{
				tmp += 7000.0 / d / d;
				Vector2 vector3 = x.Velocity * new Vector2((float)Math.Cos(x.Direction), (float)Math.Sin(x.Direction));
				vector2 += (float)GetDistance(OriginalPos) / 5f * vector3;
				d = (vector - vector2).Length() - (float)base.Region - (float)x.Region;
				tmp += 3000.0 / d / d;
			}
		};
		foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.Snapshot(base.EnemyPlaneList)){
			if (x.OriginalPosition.X > 0f && x.OriginalPosition.X < (float)base.BoundRect.Width && x.OriginalPosition.Y > 0f && x.OriginalPosition.Y < (float)base.BoundRect.Height)
			{
				Vector2 vector = new Vector2(OriginalPos.X, OriginalPos.Y);
				Vector2 vector2 = new Vector2(x.OriginalPosition.X, x.OriginalPosition.Y);
				d = (vector - vector2).Length() - (float)base.Region - (float)x.Region;
				tmp += 10000.0 / d / d;
			}
		};
		if (base.Boss != null)
		{
			d = base.Boss.GetDistance(OriginalPos) - (double)base.Boss.Region - (double)base.Region;
			tmp += 10000.0 / d / d;
		}
		return tmp;
	}

	private Vector2 GenerateDestPoint(int div, int count, float x, float y)
	{
		Vector2 result = Vector2.Zero;
		double num = 1000000000.0;
		double num2 = 0.0;
		x -= (float)(div * (count - 1) / 2);
		y -= (float)(div * (count - 1) / 2);
		if (x < 0f)
		{
			x = 0f;
		}
		else if (x > (float)(base.BoundRect.Width - div * (count - 1)))
		{
			x = base.BoundRect.Width - div * (count - 1);
		}
		if (y < 0f)
		{
			y = 0f;
		}
		else if (y > (float)(base.BoundRect.Height - div * (count - 1)))
		{
			y = base.BoundRect.Height - div * (count - 1);
		}
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < count; j++)
			{
				double num3 = GenerateDanger(new PointF(x, y));
				if (num3 < num)
				{
					num = num3;
					result = new Vector2(x, y);
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
				x += (float)div;
			}
			y += (float)div;
			x -= (float)(div * count);
		}
		if (num2 > 2.0)
		{
			return result;
		}
		return new Vector2(base.OriginalPosition.X, base.OriginalPosition.Y);
	}

	private Vector2 GenerateMove()
	{
		Vector2 forceSum = Vector2.Zero;
		float d;
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			Vector2 vector7 = new Vector2(base.OriginalPosition.X, base.OriginalPosition.Y);
			Vector2 vector8 = new Vector2(x.OriginalPosition.X, x.OriginalPosition.Y);
			Vector2 vector9 = vector7 - vector8;
			d = vector9.Length() - (float)base.Region - (float)x.Region;
			if (d < 200f)
			{
				vector9.Normalize();
				vector9 *= 5000f / d / d;
				forceSum += vector9;
				Vector2 vector10 = x.Velocity * new Vector2((float)Math.Cos(x.Direction), (float)Math.Sin(x.Direction));
				vector8 += 3f * vector10;
				vector9 = vector7 - vector8;
				d = vector9.Length() - (float)base.Region - (float)x.Region;
				vector9.Normalize();
				vector9 *= 5000f / d / d;
				forceSum += vector9;
				vector8 += 6f * vector10;
				vector9 = vector7 - vector8;
				d = vector9.Length() - (float)base.Region - (float)x.Region;
				vector9.Normalize();
				vector9 *= 2000f / d / d;
				forceSum += vector9;
			}
		};
		foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.Snapshot(base.EnemyPlaneList)){
			if (x.OriginalPosition.X > 0f && x.OriginalPosition.X < (float)base.BoundRect.Width && x.OriginalPosition.Y > 0f && x.OriginalPosition.Y < (float)base.BoundRect.Height)
			{
				Vector2 vector7 = new Vector2(base.OriginalPosition.X, base.OriginalPosition.Y);
				Vector2 vector8 = new Vector2(x.OriginalPosition.X, x.OriginalPosition.Y);
				Vector2 vector9 = vector7 - vector8;
				d = vector9.Length() - (float)base.Region - (float)x.Region;
				vector9.Normalize();
				vector9 *= 5000f / d / d;
				forceSum += vector9;
				Vector2 vector10 = x.Velocity * new Vector2((float)Math.Cos(x.Direction), (float)Math.Sin(x.Direction));
				vector8 += 5f * vector10;
				vector9 = vector7 - vector8;
				d = vector9.Length() - (float)base.Region - (float)x.Region;
				vector9.Normalize();
				vector9 *= 5000f / d / d;
				forceSum += vector9;
			}
		};
		if (base.Boss != null)
		{
			Vector2 vector = new Vector2(base.OriginalPosition.X, base.OriginalPosition.Y);
			Vector2 vector2 = new Vector2(base.Boss.OriginalPosition.X, base.Boss.OriginalPosition.Y);
			Vector2 vector3 = vector - vector2;
			d = vector3.Length() - (float)base.Region - (float)base.Boss.Region;
			vector3.Normalize();
			vector3 *= 10000f / d / d;
			forceSum += vector3;
		}
		Vector2 vector4 = GenerateDestPoint(40, 4, base.OriginalPosition.X, base.OriginalPosition.Y);
		vector4 = GenerateDestPoint(10, 4, vector4.X, vector4.Y);
		vector4 = GenerateDestPoint(2, 5, vector4.X, vector4.Y);
		Vector2 end = new Vector2(base.DestPoint.X, base.DestPoint.Y);
		if (base.Time > 300)
		{
			vector4 = Vector2.Lerp(vector4, end, 0.5f);
		}
		Vector2 vector5 = new Vector2(base.OriginalPosition.X, base.OriginalPosition.Y);
		Vector2 vector6 = vector4 - vector5;
		forceSum += 1f * vector6;
		base.DestPoint = new PointF(vector4.X, vector4.Y);
		if (base.Boss != null)
		{
			forceSum += new Vector2(5f * (base.Boss.OriginalPosition.X - base.OriginalPosition.X), 0f);
		}
		else
		{
			forceSum += new Vector2(0.1f * ((float)(base.BoundRect.Width / 2) - base.OriginalPosition.X), 0.1f * ((float)base.BoundRect.Height - base.OriginalPosition.Y));
			forceSum += GenerateEnemyBeat();
		}
		forceSum += GenerateItemGet();
		return forceSum;
	}

	private Vector2 GenerateEnemyBeat()
	{
		Vector2 result = Vector2.Zero;
		float tmp = 1000f;
		foreach (BaseEnemyPlane x in SlimDX.Compat.ListCompat.Snapshot(base.EnemyPlaneList)){
			if (x.OriginalPosition.Y > 0f && x.OriginalPosition.Y < 300f && x.HealthPoint < 20000f && Math.Abs(x.OriginalPosition.X - base.OriginalPosition.X) < tmp)
			{
				tmp = x.OriginalPosition.X;
			}
		};
		if (tmp < 1000f)
		{
			result = new Vector2(10f * (tmp - base.OriginalPosition.X), 0f);
		}
		return result;
	}

	private Vector2 GenerateItemGet()
	{
		if (base.BulletList.Count > 10)
		{
			return Vector2.Zero;
		}
		if (base.Boss != null)
		{
			return Vector2.Zero;
		}
		int itemCount = 0;
		foreach (BaseItem x in SlimDX.Compat.ListCompat.Snapshot(base.ItemList)){
			if (!x.Obtain)
			{
				itemCount++;
			}
		};
		if (itemCount > 5)
		{
			return new Vector2(0f, 2f * (110f - base.OriginalPosition.Y));
		}
		return Vector2.Zero;
	}

	public override void Show()
	{
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["Region"], 0.1f, 0f, new PointF(base.DestPoint.X + (float)base.BoundRect.Left, base.DestPoint.Y + (float)base.BoundRect.Top), Color.White);
		base.Show();
	}
}
