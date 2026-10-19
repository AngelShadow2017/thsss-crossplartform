namespace Shooting;

public class BaseBoss : BaseObject
{
	private const int unmatchedTime = 100;

	private int spellcardHP = 2000;

	private float armor = 0f;

	public int SpellcardHP
	{
		get
		{
			return spellcardHP;
		}
		set
		{
			spellcardHP = value;
		}
	}

	public int UnmatchedTime => 100;

	public int Life { get; set; }

	public int MaxHP { get; set; }

	public bool OnSpell { get; set; }

	public int SpellTime { get; set; }

	public float Armon
	{
		get
		{
			return armor;
		}
		set
		{
			if (value < 0f)
			{
				armor = 0f;
			}
			else if (value > 1f)
			{
				armor = 1f;
			}
			else
			{
				armor = value;
			}
		}
	}

	public override void HitCheckAll()
	{
		if (Life < 0 || !base.HitEnabled)
		{
			return;
		}
		if (base.MyPlane.HitEnabled && HitCheck(base.MyPlane))
		{
			base.MyPlane.PreMiss();
		}
		for (int num = base.MyBulletList.Count - 1; num >= 0; num--)
		{
			if (HitCheck(base.MyBulletList[num]))
			{
				if (base.Time > UnmatchedTime)
				{
					if (base.MyPlane.EnchantmentState == EnchantmentType.Red)
					{
						base.HealthPoint -= (float)base.MyBulletList[num].Damage * 1.25f * (1f - Armon);
					}
					else
					{
						base.HealthPoint -= (float)base.MyBulletList[num].Damage * (1f - Armon);
					}
				}
				base.MyBulletList[num].GiveEndEffect();
				base.MyBulletList.RemoveAt(num);
				base.MyPlane.Score += 10L;
				if (base.HealthPoint <= 200f)
				{
					base.StageData.SoundPlay("se_damage01.wav");
				}
			}
		}
	}
}
