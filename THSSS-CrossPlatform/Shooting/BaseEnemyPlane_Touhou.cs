using System;
using System.Collections.Generic;
using System.Drawing;
using SlimDX;

namespace Shooting;

public class BaseEnemyPlane_Touhou : BaseEnemyPlane
{
	private TextureObject[,] TxtureObjects { get; set; }

	private float IndexX { get; set; }

	private int IndexY { get; set; }

	public byte ColorType { get; set; }

	public int ItemCount
	{
		set
		{
			RedCount = value;
			BlueCount = value;
		}
	}

	public int RedCount { get; set; }

	public int BlueCount { get; set; }

	public int GreenCount { get; set; }

	public int ClearRadius { get; set; }

	public bool StarFall { get; set; }

	public BaseEnemyPlane_Touhou()
	{
	}

	public BaseEnemyPlane_Touhou(StageDataPackage StageData, string EnemyName, PointF OriginalPosition, float Velocity, double Direction, byte ColorType)
		: this(StageData, EnemyName, OriginalPosition, Velocity, Direction)
	{
		this.ColorType = ColorType;
	}

	public BaseEnemyPlane_Touhou(StageDataPackage StageData, string EnemyName, PointF OriginalPosition, float Velocity, double Direction)
		: base(StageData, null, OriginalPosition, Velocity, Direction)
	{
		base.OriginalPosition = OriginalPosition;
		if (EnemyName != null)
		{
			TxtureObjects = new TextureObject[2, 8]
			{
				{
					base.TextureObjectDictionary[EnemyName + "_0"],
					base.TextureObjectDictionary[EnemyName + "_1"],
					base.TextureObjectDictionary[EnemyName + "_2"],
					base.TextureObjectDictionary[EnemyName + "_3"],
					base.TextureObjectDictionary[EnemyName + "_0"],
					base.TextureObjectDictionary[EnemyName + "_1"],
					base.TextureObjectDictionary[EnemyName + "_2"],
					base.TextureObjectDictionary[EnemyName + "_3"]
				},
				{
					base.TextureObjectDictionary[EnemyName + "_4"],
					base.TextureObjectDictionary[EnemyName + "_5"],
					base.TextureObjectDictionary[EnemyName + "_6"],
					base.TextureObjectDictionary[EnemyName + "_7"],
					base.TextureObjectDictionary[EnemyName + "_8"],
					base.TextureObjectDictionary[EnemyName + "_9"],
					base.TextureObjectDictionary[EnemyName + "_A"],
					base.TextureObjectDictionary[EnemyName + "_B"]
				}
			};
		}
		base.EventGroupList = new List<EventGroup>();
		base.EventsExecutionList = new List<Execution>();
	}

	public override void Ctrl()
	{
		base.Ctrl();
		TextureCtrl();
	}

	public virtual void TextureCtrl()
	{
		base.AngleWithDirection = false;
		base.Angle = Math.PI / 2.0;
		int num = (base.Mirrored ? IndexY : (-IndexY));
		if (base.Vx < -0.5f)
		{
			base.Mirrored = true;
			IndexY = 1;
			if (IndexX < 3f)
			{
				IndexX += 1f / 6f;
			}
			else
			{
				IndexX = 3 + base.Time % 24 / 6;
			}
		}
		else if (base.Vx > 0.5f)
		{
			base.Mirrored = false;
			IndexY = 1;
			if (IndexX < 3f)
			{
				IndexX += 1f / 6f;
			}
			else
			{
				IndexX = 3 + base.Time % 24 / 6;
			}
		}
		else
		{
			base.Mirrored = false;
			IndexY = 0;
			IndexX = base.Time % 48 / 6;
		}
		if (num != (base.Mirrored ? IndexY : (-IndexY)))
		{
			IndexX = 0f;
		}
		if (IndexX > 7f)
		{
			IndexX = 7f;
		}
		base.TxtureObject = TxtureObjects[IndexY, (int)IndexX];
	}

	public override void Shoot()
	{
	}

	public override void GiveEndEffect()
	{
		Color color;
		switch (ColorType)
		{
		case 0:
		{
			color = Color.Red;
			EndEnemy endEnemy6 = new EndEnemy(base.StageData, "EndEnemy0", base.Position, 0f, 0.0);
			endEnemy6.Scale = 0.2f;
			break;
		}
		case 1:
		{
			color = Color.CornflowerBlue;
			EndEnemy endEnemy5 = new EndEnemy(base.StageData, "EndEnemy1", base.Position, 0f, 0.0);
			endEnemy5.Scale = 0.2f;
			break;
		}
		case 2:
		{
			color = Color.Green;
			EndEnemy endEnemy4 = new EndEnemy(base.StageData, "EndEnemy2", base.Position, 0f, 0.0);
			endEnemy4.Scale = 0.2f;
			break;
		}
		case 3:
		{
			color = Color.Yellow;
			EndEnemy endEnemy3 = new EndEnemy(base.StageData, "EndEnemy3", base.Position, 0f, 0.0);
			endEnemy3.Scale = 0.2f;
			break;
		}
		default:
		{
			color = Color.White;
			EndEnemy endEnemy = new EndEnemy(base.StageData, "EndEnemy2", base.Position, 0f, 0.0);
			endEnemy.Scale = 0.2f;
			EndEnemy endEnemy2 = new EndEnemy(base.StageData, "EndEnemy3", base.Position, 0f, 0.0);
			endEnemy2.Scale = 0.2f;
			break;
		}
		}
		for (int i = 0; i < 10; i++)
		{
			Particle3D3 particle3D = new Particle3D3(base.StageData, "Effect-0", base.OriginalPosition, (float)base.Ran.Next(40) / 10f, (float)base.Ran.Next(360) / 180f * (float)Math.PI, 40);
			particle3D.Angle = (float)base.StageData.Ran.Next(0, 360) * (float)Math.PI / 180f;
			particle3D.AngularVelocity3D = (float)base.StageData.Ran.Next(10, 30) / 100f;
			particle3D.Scale = (float)base.StageData.Ran.Next(12, 18) / 10f;
			particle3D.RotatingAxis = new Vector3(base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100), base.StageData.Ran.Next(-100, 100));
			particle3D.ColorValue = color;
			particle3D.MaxTransparent = 200;
		}
	}

	public override void GiveItems()
	{
		PointF originalPosition;
		for (int i = 0; i < RedCount; i++)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-RedCount * 5, RedCount * 5), base.OriginalPosition.Y + (float)base.Ran.Next(-RedCount * 5, RedCount * 5));
			new PowerItem_Touhou(base.StageData, originalPosition);
		}
		for (int i = 0; i < BlueCount; i++)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-BlueCount * 5, BlueCount * 5), base.OriginalPosition.Y + (float)base.Ran.Next(-BlueCount * 5, BlueCount * 5));
			new ScoreItem_Touhou(base.StageData, originalPosition);
		}
		for (int i = 0; i < GreenCount; i++)
		{
			originalPosition = new PointF(base.OriginalPosition.X + (float)base.Ran.Next(-GreenCount * 5, GreenCount * 5), base.OriginalPosition.Y + (float)base.Ran.Next(-GreenCount * 5, GreenCount * 5));
			new BeliefItem_Touhou(base.StageData, originalPosition);
		}
		if (ClearRadius != 0)
		{
			BulletRemover_Small bulletRemover_Small = new BulletRemover_Small(base.StageData, base.OriginalPosition);
			bulletRemover_Small.Region = ClearRadius;
			bulletRemover_Small.LifeTime = 60;
		}
		if (StarFall)
		{
			BaseEffect baseEffect = new BaseEffect(base.StageData, "StarShardBS", base.Position, 8f, -Math.PI / 2.0);
			baseEffect.GhostingCount = 40;
			baseEffect.AngularVelocityDegree = 5f;
			new ShootingStarShard(base.StageData, new PointF(base.OriginalPosition.X, 0f));
		}
	}

	public override void EventCtrl()
	{
		// EventGroup.Update appends to EventsExecutionList while we walk it. The
		// original .NET 2.0 List.ForEach did not bump a version, so .NET modern
		// rejects it: snapshot first, which reproduces "new items run next frame".
		EventGroup[] eventGroups = base.EventGroupList.ToArray();
		foreach (EventGroup eventGroup in eventGroups)
		{
			eventGroup.Update(this);
		}
		Execution[] executions = base.EventsExecutionList.ToArray();
		foreach (Execution execution in executions)
		{
			execution.Update(this);
		}
	}
}
