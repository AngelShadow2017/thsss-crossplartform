using System;
using System.Drawing;

namespace Shooting.Planes.Story;

internal class StoryEmitterStar : BaseObject
{
	public StoryEmitterStar(StageDataPackage StageData, PointF Position, float Velocity, double Direction)
		: base(StageData, null, Position, Velocity, Direction)
	{
		base.LifeTime = 30;
		base.Story.StoryEffectList.Add(this);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		StoryStar storyStar = new StoryStar(base.StageData, "Star", new PointF(base.Ran.Next((int)base.Position.X - 70, (int)base.Position.X + 70), base.Position.Y + 20f), base.StageData.Ran.Next(10, 20) / 10, -Math.PI / 2.0 - (double)((float)base.StageData.Ran.Next(1, 5) / 10f));
		storyStar.Active = true;
		storyStar.Scale = (float)base.StageData.Ran.Next(15, 40) / 80f;
		storyStar.LifeTime = 40;
		StoryStar item = storyStar;
		base.Story.StoryEffectList.Add(item);
	}
}
