namespace Shooting;

internal class BaseSpellCard : BaseBulleEmitter
{
	public SpellCardHistory SC_Data;

	public bool Bombed { get; set; }

	public bool Missed { get; set; }

	public long BaseScore { get; set; }

	public int SpellTime { get; set; }

	public string SC_Name { get; set; }

	public BaseSpellCard(StageDataPackage StageData)
		: base(StageData)
	{
		if (base.Boss != null)
		{
			BaseScore = 10000000L;
			base.OriginalPosition = base.Boss.OriginalPosition;
			SpellTime = base.Boss.SpellTime - base.Boss.Time;
			base.Background3D.Enabled3D = false;
			SC_Name = null;
		}
	}

	public override void Ctrl()
	{
		base.Ctrl();
		if (base.Time == 1)
		{
			SC_Data = base.StageData.PData.SC_History.Find(FindSC);
			if (!base.StageData.OnReplay)
			{
				SC_Data.History++;
			}
		}
		if (base.KClass.Key_X && base.MyPlane.Spell > 0)
		{
			Bombed = true;
		}
		if (base.MyPlane.Time <= 1)
		{
			Missed = true;
		}
		if (base.Time > 210 && base.SpellList.Count > 0)
		{
			Bombed = true;
		}
		long spellCardBonus = GetSpellCardBonus();
		if (spellCardBonus > 0)
		{
			base.StageData.SCBstring = GetSpellCardBonus().ToString().PadRight(14, ' ') + (SC_Data.Recorded + "/" + SC_Data.History).PadLeft(6);
		}
		else
		{
			base.StageData.SCBstring = "Failed".PadRight(14, ' ') + (SC_Data.Recorded + "/" + SC_Data.History).PadLeft(6);
		}
	}

	public override void GiveEndEffect()
	{
		if (!Bombed && !Missed && base.Time < SpellTime)
		{
			base.StageData.SoundPlay("se_cardget.wav");
			new SpellCardBonus(base.StageData, GetSpellCardBonus());
			if (!base.StageData.OnReplay)
			{
				SC_Data.Recorded++;
			}
		}
		base.Background3D.Enabled3D = true;
		base.StageData.SCBstring = null;
	}

	public virtual long GetSpellCardBonus()
	{
		if (Bombed || Missed)
		{
			return 0L;
		}
		long num = BaseScore - BaseScore * base.Time / 2 / SpellTime;
		num /= 10;
		return num * 10;
	}

	public bool FindSC(SpellCardHistory SCH)
	{
		return SCH.Rank == base.Difficulty && SCH.MyPlaneFullName == base.MyPlane.FullName && SCH.Name == SC_Name;
	}
}
