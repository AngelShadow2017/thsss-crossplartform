using System;
using System.Drawing;
using System.Threading;
using SlimDX.Direct3D9;

namespace Shooting;

internal class GameState_MusicLoading : BaseMenu, IGameState
{
	private bool LoadFlag;

	private Thread LoadingThread;

	public GameState_MusicLoading(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
	}

	public GameState_MusicLoading()
	{
	}

	public override void Init()
	{
		LoadFlag = false;
		base.Background.Light = 0;
	}

	public override void UpdateData()
	{
		base.UpdateData();
		if (base.TimeMain == 1)
		{
			LoadingThread = new Thread(PreloadMusic);
			LoadingThread.Start();
		}
		if (LoadFlag && base.TimeMain > 0)
		{
			base.StageData.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = base.StageData.GlobalData.LastState.StageName,
				NeedInit = false
			};
			((BaseGameState)base.StageData.GlobalData.LastState).TimeSycn();
			SlimDX.Compat.ThreadCompat.Stop(LoadingThread);
		}
		if (base.TimeMain < 50)
		{
			base.Background.Light += 5;
		}
		else if (base.TimeMain > 60)
		{
			if (base.TimeMain % 60 >= 30)
			{
				base.Background.Light += 4;
			}
			else if (base.TimeMain % 60 < 30)
			{
				base.Background.Light -= 4;
			}
		}
		Particle particle = new Particle(base.StageData, "Star", new PointF(base.StageData.Ran.Next(400, 640), base.StageData.Ran.Next(440, 580)), base.StageData.Ran.Next(10, 20) / 10, -Math.PI / 2.0 - (double)((float)base.StageData.Ran.Next(1, 5) / 10f));
		particle.Active = true;
		particle.Scale = (float)base.StageData.Ran.Next(25, 70) / 100f;
		particle.LifeTime = 70;
	}

	private void PreloadMusic()
	{
		if (base.GlobalData.LastState is BaseStage)
		{
			switch (((BaseStage)base.GlobalData.LastState).StageName)
			{
			case "St1":
				base.BGM_Player.PreLoad("BGM/Stage01.wav");
				base.BGM_Player.PreLoad("BGM/Boss01.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/St01");
				break;
			case "St2":
				base.BGM_Player.PreLoad("BGM/Stage02.wav");
				base.BGM_Player.PreLoad("BGM/Boss02.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/St02");
				break;
			case "St3":
				base.BGM_Player.PreLoad("BGM/Stage03.wav");
				base.BGM_Player.PreLoad("BGM/Boss03.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/St03");
				break;
			case "St4":
				base.BGM_Player.PreLoad("BGM/Stage04.wav");
				base.BGM_Player.PreLoad("BGM/Boss04.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/St04");
				break;
			case "St5":
				base.BGM_Player.PreLoad("BGM/Stage05.wav");
				base.BGM_Player.PreLoad("BGM/Boss05.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/St05");
				break;
			case "St6":
				base.BGM_Player.PreLoad("BGM/Stage06.wav");
				base.BGM_Player.PreLoad("BGM/Boss06.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/St06");
				break;
			case "StEx":
				base.BGM_Player.PreLoad("BGM/StageEx.wav");
				base.BGM_Player.PreLoad("BGM/BossEx.wav");
				base.GlobalData.LastState.StageData.ClearCSCathe();
				base.GlobalData.LastState.StageData.PreLoadCS("CS/StEx");
				break;
			}
		}
		LoadFlag = true;
	}

	public override void Render()
	{
		base.GlobalData.LastState.Render();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
		base.DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
		foreach (BaseEffect x in SlimDX.Compat.ListCompat.LegacyItems(base.EffectList)){
			x.Show();
		};
		base.SpriteMain.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.SpriteMain.Draw2D(base.GlobalData.TextureObjectDictionary["NowLoading-0"], 1f, 0f, new Point(520, 440), base.Background.Light);
		foreach (BaseEffect x in SlimDX.Compat.ListCompat.LegacyItems(base.StageData.EffectList)){
			if (!x.Active)
			{
				x.Show();
			}
		};
		base.SpriteMain.End();
	}

	public override void BGM_Pause()
	{
	}

	public override void BGM_Resume()
	{
	}
}
