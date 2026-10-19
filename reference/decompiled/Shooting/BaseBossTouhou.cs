using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Shooting;

public class BaseBossTouhou : BaseBoss
{
	public TextureObject[,] TxtureObjects { get; set; }

	public float IndexX { get; set; }

	public int IndexY { get; set; }

	public int OnAction { get; set; }

	public float[] ArmonArray { get; set; }

	public BaseBossTouhou()
	{
	}

	public BaseBossTouhou(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.Boss = this;
		base.AngleWithDirection = false;
		base.Angle = Math.PI / 2.0;
		SetBossLifeLineTex();
	}

	public void SetBossLifeLineTex()
	{
	}

	public void MoveToPoint(PointF destOriginalPosition)
	{
		float num = (float)GetDistance(destOriginalPosition);
		if (num > Math.Abs(base.Velocity))
		{
			base.Direction = GetDirection(destOriginalPosition);
			base.Accelerate = (0f - base.Velocity) * base.Velocity / (2f * num);
			if (base.Velocity < 0f)
			{
				base.Velocity = 0f;
			}
		}
		else
		{
			base.Accelerate = 0f;
			base.Velocity = 0f;
		}
	}

	public void MoveUpDown()
	{
		if (base.Velocity < 0.01f)
		{
			if (base.Time % 80 == 0)
			{
				float y = base.DestPoint.Y;
				y += 10f;
				base.Velocity = 0.5f;
				base.DestPoint = new PointF(base.DestPoint.X, y);
			}
			else if (base.Time % 80 == 40)
			{
				float y = base.DestPoint.Y;
				y -= 10f;
				base.Velocity = 0.5f;
				base.DestPoint = new PointF(base.DestPoint.X, y);
			}
		}
	}

	public void MoveUpDown(int circle)
	{
		if (base.Velocity < 0.01f)
		{
			if (base.Time % circle == 0)
			{
				float y = base.DestPoint.Y;
				y += 10f;
				base.Velocity = 0.5f;
				base.DestPoint = new PointF(base.DestPoint.X, y);
			}
			else if (base.Time % circle == circle / 2)
			{
				float y = base.DestPoint.Y;
				y -= 10f;
				base.Velocity = 0.5f;
				base.DestPoint = new PointF(base.DestPoint.X, y);
			}
		}
	}

	public void RandomMove(int T, float velocity)
	{
		if (base.Time % T == 0)
		{
			base.DestPoint = new Point(base.Ran.Next(100, base.BoundRect.Width - 100), base.Ran.Next(70, 200));
			base.Velocity = velocity;
		}
		MoveToPoint(base.DestPoint);
	}

	public void RandomMove(int T, float velocity, Rectangle RangeRect)
	{
		if (base.Time % T == 0)
		{
			base.DestPoint = new Point(base.Ran.Next(RangeRect.Left, RangeRect.Right), base.Ran.Next(RangeRect.Top, RangeRect.Bottom));
			base.Velocity = velocity;
		}
		MoveToPoint(base.DestPoint);
	}

	public void RandomMove(int T, int Dist, float Velocity, Rectangle RangeRect)
	{
		if (base.Time % T == 0)
		{
			for (int i = 0; i < 300; i++)
			{
				double num = (double)base.Ran.Next(360) * Math.PI / 180.0;
				float num2 = base.OriginalPosition.X + (float)((double)Dist * Math.Sin(num));
				float num3 = base.OriginalPosition.Y + (float)((double)Dist * Math.Cos(num));
				if (num2 > (float)RangeRect.Left && num2 < (float)RangeRect.Right && num3 > (float)RangeRect.Top && num3 < (float)RangeRect.Bottom)
				{
					base.DestPoint = new PointF(num2, num3);
					base.Velocity = Velocity;
					break;
				}
			}
		}
		MoveToPoint(base.DestPoint);
	}

	public void RandomMove1(int T, float velocity)
	{
		if (base.Time % T == 0)
		{
			if (base.OriginalPosition.X < (float)(base.BoundRect.Width / 2 - 50))
			{
				base.DestPoint = new PointF(base.Ran.Next(base.BoundRect.Width / 2 + 50, base.BoundRect.Width - 110), base.Ran.Next(base.BoundRect.Top + 100, base.BoundRect.Top + 150));
			}
			else if (base.OriginalPosition.X > (float)(base.BoundRect.Width / 2 + 50))
			{
				base.DestPoint = new PointF(base.Ran.Next(110, base.BoundRect.Width / 2 - 50), base.Ran.Next(base.BoundRect.Top + 100, base.BoundRect.Top + 150));
			}
			else if (base.MyPlane.OriginalPosition.X < (float)(base.BoundRect.Width / 2))
			{
				base.DestPoint = new PointF(base.Ran.Next(110, base.BoundRect.Width / 2 - 50), base.Ran.Next(base.BoundRect.Top + 100, base.BoundRect.Top + 150));
			}
			else
			{
				base.DestPoint = new PointF(base.Ran.Next(base.BoundRect.Width / 2 + 50, base.BoundRect.Width - 110), base.Ran.Next(base.BoundRect.Top + 100, base.BoundRect.Top + 150));
			}
			base.Velocity = velocity;
		}
		MoveToPoint(base.DestPoint);
	}

	public virtual void TextureCtrl()
	{
		int num = (base.Mirrored ? IndexY : (-IndexY));
		if (OnAction > 0)
		{
			IndexY = 2;
			IndexX += 0.125f;
			OnAction--;
		}
		else if (base.Vx < -0.05f)
		{
			base.Mirrored = true;
			IndexY = 1;
			if (base.Vx < -1f)
			{
				if (IndexX < 2f)
				{
					IndexX += 0.125f;
				}
				else
				{
					IndexX = 2 + base.Time % 16 / 8;
				}
			}
			else if (base.Vx < -0.5f)
			{
				IndexX = 1f;
			}
			else
			{
				IndexX = 0f;
			}
		}
		else if (base.Vx > 0.05f)
		{
			base.Mirrored = false;
			IndexY = 1;
			if (base.Vx > 1f)
			{
				if (IndexX < 2f)
				{
					IndexX += 0.125f;
				}
				else
				{
					IndexX = 2 + base.Time % 16 / 8;
				}
			}
			else if (base.Vx > 0.5f)
			{
				IndexX = 1f;
			}
			else
			{
				IndexX = 0f;
			}
		}
		else
		{
			base.Mirrored = false;
			IndexY = 0;
			IndexX = base.TimeMain % 32 / 8;
		}
		if (num != (base.Mirrored ? IndexY : (-IndexY)))
		{
			IndexX = 0f;
		}
		if (IndexX > 3f)
		{
			IndexX = 3f;
		}
		base.TxtureObject = TxtureObjects[IndexY, (int)IndexX];
	}

	public override void Ctrl()
	{
		base.Ctrl();
		Shoot();
		TextureCtrl();
		if (OutBoundary())
		{
			base.Boss = null;
		}
	}

	public override bool OutBoundary()
	{
		if (base.Time > base.LifeTime)
		{
			return true;
		}
		return false;
	}

	public override void GiveItems()
	{
		PointF originalPosition;
		if (base.MyPlane.Power < 400)
		{
			for (int i = 0; i < 15; i++)
			{
				originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-49, 50), base.OriginalPosition.Y + (float)base.Ran.Next(-49, 50));
				new PowerItem_Touhou(base.StageData, originalPosition);
			}
			for (int i = 0; i < 15; i++)
			{
				originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-49, 50), base.OriginalPosition.Y + (float)base.Ran.Next(-49, 50));
				new ScoreItem_Touhou(base.StageData, originalPosition);
			}
		}
		else
		{
			for (int i = 0; i < 30; i++)
			{
				originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-49, 50), base.OriginalPosition.Y + (float)base.Ran.Next(-49, 50));
				new ScoreItem_Touhou(base.StageData, originalPosition);
			}
		}
	}

	protected virtual void LoadArmon(string fileName)
	{
		if (File.Exists(fileName))
		{
			char[] array = new char[1] { ',' };
			string[] array2 = File.ReadAllLines(fileName, Encoding.Default);
			ArmonArray = new float[array2.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				ArmonArray[i] = float.Parse(array2[i]);
			}
		}
	}
}
