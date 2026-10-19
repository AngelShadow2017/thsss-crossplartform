namespace Shooting;

internal class EndStage : BaseEffect
{
	private int tempVolumn;

	private string nextState = null;

	private bool nextInit = false;

	public int FadeOutTime { get; set; }

	public EndStage(StageDataPackage StageData, string NextState, bool NextInit)
		: this(StageData, NextState, NextInit, 50)
	{
	}

	public EndStage(StageDataPackage StageData, string NextState, bool NextInit, int FadeOutTime)
		: base(StageData)
	{
		tempVolumn = base.BGM_Player.Volume;
		base.LifeTime = 160;
		nextState = NextState;
		nextInit = NextInit;
		this.FadeOutTime = FadeOutTime;
		base.MyPlane.SpellEnabled = false;
		if (base.MyPlane.EnchantmentTime > 50)
		{
			base.MyPlane.EnchantmentTime = 50;
		}
		new BackgroundTransitionOut(StageData, FadeOutTime);
	}

	public override void Ctrl()
	{
		base.Ctrl();
		int num = base.LifeTime - 100;
		base.BGM_Player.Volume = (int)((float)tempVolumn * (1f - (float)base.Time / (float)num));
		if (base.Time == num)
		{
			base.StageData.ChangeBGM(base.BGM_Player.FileName, Repeat: false);
			base.BGM_Player.Stop();
		}
		if (base.Time != base.LifeTime)
		{
			return;
		}
		base.BGM_Player.Volume = tempVolumn;
		if (nextInit)
		{
			if (base.StageData.OnReplay && nextState == "MainMenu")
			{
				base.StageData.Rep.CloseRpy();
				base.StageData.OnReplay = false;
			}
			base.StageData.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = nextState,
				NeedInit = true,
				SDPswitch = new StageDataPackage(base.StageData.GlobalData)
			};
		}
		else if (base.StageData.OnPractice)
		{
			base.StageData.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = "GameSetMenu",
				NeedInit = true,
				SDPswitch = new StageDataPackage(base.StageData.GlobalData)
			};
		}
		else
		{
			base.StageData.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = nextState,
				NeedInit = false,
				SDPswitch = base.StageData
			};
			base.MyPlane.Refresh();
			base.StageData.SetReplayStageInfo();
		}
		base.Background3D.Clear();
		base.Background.Clear();
		base.Background2.Clear();
		BackgroundTransitionOut backgroundTransitionOut = new BackgroundTransitionOut(base.StageData, 2);
		backgroundTransitionOut.TransparentValueF = 255f;
		backgroundTransitionOut.TransparentVelocity = 0f;
		backgroundTransitionOut.LifeTime = 1;
		base.BulletList.Clear();
		base.EnemyPlaneList.Clear();
		base.ItemList.Clear();
		base.TimeMain = 0;
		base.MyPlane.Refresh();
		base.Ran = new MyRandom(0);
		base.StageData.VibrateStop();
	}

	public override void Show()
	{
	}
}
