namespace Shooting;

public class BaseSpell : BaseObject
{
	public BaseSpell()
	{
	}

	public BaseSpell(StageDataPackage StageData)
	{
		base.StageData = StageData;
		base.Position = base.MyPlane.Position;
		base.Damage = 12;
		base.LifeTime = 300;
		base.SpellList.Add(this);
		StageData.VibrateStart(base.LifeTime);
		new BackgroundBlack(StageData, base.Position);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time < 30)
		{
			for (int num = base.ItemList.Count - 1; num >= 0; num--)
			{
				if (HitCheck(base.ItemList[num]) && !(base.ItemList[num] is ShootingStarShard))
				{
					base.ItemList[num].Obtain = true;
					base.ItemList[num].Doubled = true;
				}
			}
		}
		base.MyPlane.DeadTime = -1;
		if (OutBoundary())
		{
			base.SpellList.Remove(this);
			base.MyPlane.Time = base.MyPlane.UnmatchedTime - 60;
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

	public override void HitCheckAll()
	{
		for (int num = base.BulletList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.BulletList[num]) && !base.BulletList[num].UnRemoveable)
			{
				base.BulletList[num].GiveEndEffect();
				base.BulletList[num].GiveItems();
				base.BulletList.RemoveAt(num);
			}
		}
		for (int num = base.EnemyPlaneList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.EnemyPlaneList[num]))
			{
				base.EnemyPlaneList[num].HealthPoint -= base.Damage;
				if (base.EnemyPlaneList[num].HealthPoint <= 0f)
				{
					base.EnemyPlaneList[num].GiveEndEffect();
					base.EnemyPlaneList[num].GiveItems();
					base.EnemyPlaneList.RemoveAt(num);
					base.StageData.SoundPlay("se_enep00.wav");
					break;
				}
			}
		}
		if (base.Boss != null && base.Boss.Time > base.Boss.UnmatchedTime && HitCheck(base.Boss))
		{
			base.Boss.HealthPoint -= (int)((float)base.Damage * (1f - base.Boss.Armon));
			if (base.Boss.HealthPoint <= 200f)
			{
				base.StageData.SoundPlay("se_damage01.wav");
			}
		}
	}
}
