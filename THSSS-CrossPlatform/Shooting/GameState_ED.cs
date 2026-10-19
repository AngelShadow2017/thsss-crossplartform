using System.Drawing;
using System.IO;
using Shooting.Planes.Story;
using SlimDX.Direct3D9;

namespace Shooting;

internal class GameState_ED : BaseGameState, IGameState
{
	private int testStartTime = 0;

	private bool StageFlag;

	private int[] keyTime = new int[1] { 500 };

	public GameState_ED(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		base.StageName = "All";
		System.Drawing.Font font = new System.Drawing.Font("Arial", 14f, (FontStyle)3);
		base.DXFont = new SlimDX.Direct3D9.Font(base.DeviceMain, font);
	}

	public override void UpdateData()
	{
		base.TimeMain++;
		base.SoundPlayList.Clear();
		if (base.StageData.Rep != null && base.StageData.OnReplay)
		{
			int num = base.StageData.Rep.ReadKey();
			if (num == 57358)
			{
				num = base.StageData.Rep.ReadKey();
			}
			if (num == 4080)
			{
				base.StateSwitchData = new StateSwitchDataPackage
				{
					NextState = "GameOverMenu",
					NeedInit = true,
					SDPswitch = new StageDataPackage(base.GlobalData)
				};
				base.GlobalData.LastState = this;
				base.StageData.KClass.Hex2Key(0);
				base.StageData.SoundPlay("se_pause.wav");
			}
			else
			{
				base.StageData.KClass.Hex2Key(num);
			}
		}
		base.Background3D.Ctrl();
		base.Background.Ctrl();
		base.Background2.Ctrl();
		Drama();
		if (base.Story != null)
		{
			base.Story.Ctrl();
		}
		for (int num2 = base.EnemyPlaneList.Count - 1; num2 >= 0; num2--)
		{
			base.EnemyPlaneList[num2].Ctrl();
		}
		for (int num2 = base.BulletList.Count - 1; num2 >= 0; num2--)
		{
			base.BulletList[num2].Ctrl();
		}
		for (int num2 = base.EffectList.Count - 1; num2 >= 0; num2--)
		{
			base.EffectList[num2].Ctrl();
		}
		base.Particle3D.Ctrl();
		for (int num2 = base.InterfaceList.Count - 1; num2 >= 0; num2--)
		{
			base.InterfaceList[num2].Ctrl();
		}
		base.StageData.Vibrate();
		foreach (XAudio2_Player x in SlimDX.Compat.ListCompat.Snapshot(base.SoundPlayList)){
			x.Play();
		};
		if (base.TimeMain % 6 != 0 && base.KClass.Key_plus && base.MyPlane.Life >= 0 && base.StateSwitchData == null)
		{
			UpdateData();
		}
		base.GlobalData.LastState = this;
	}

	public override void Init()
	{
		base.StageData.C_History = base.StageData.PData.C_History.Find(base.StageData.FindClear);
	}

	public void Drama()
	{
		if (base.TimeMain == 2)
		{
			if (base.StageData.C_History.PracticeLevel < 7)
			{
				base.StageData.C_History.PracticeLevel = 7;
			}
			StageFlag = false;
			BackgroundTransitionIn backgroundTransitionIn = new BackgroundTransitionIn(base.StageData);
			backgroundTransitionIn.Delay = 0;
			backgroundTransitionIn.LifeTime = 280;
			base.BoundRect = new Rectangle(128, 0, 384, 480);
			if (base.StageData.ContinueTimes == 0)
			{
				base.StageData.ChangeBGM("BGM/ED.wav", 0, 0, 0, 0, 0);
				new Story_SSStaff(base.StageData);
			}
			base.StageData.CurrentStageName = "All";
			base.StageData.RepInfo.LastStage = base.StageData.CurrentStageName;
			base.StageData.C_History = base.StageData.PData.C_History.Find(base.StageData.FindClear);
			base.StageData.C_History.ClearTimes++;
			if (base.StageData.ContinueTimes == 0)
			{
				base.StageData.C_History.NoContinueClearTimes++;
			}
			base.StageData.S_History = base.StageData.PData.S_History.FindAll(base.StageData.FindScore);
		}
		if (base.TimeMain > 2 && base.Story == null && !StageFlag)
		{
			StageFlag = true;
			base.TimeMain = 3;
		}
		if (base.Story != null)
		{
			return;
		}
		if (base.TimeMain == 5)
		{
			base.StageData.ChangeBGM("BGM/Staff.wav", 0, 0, 0, 0, 0);
			string[] files = Directory.GetFiles("CS/Staff/", "*.mbg");
			string[] array = files;
			foreach (string fileName in array)
			{
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
				cSEmitterController.OnRoad = true;
				cSEmitterController.Time = base.TimeMain - 60;
			}
			base.BoundRect = new Rectangle(128, 0, 640, 480);
		}
		if (!StageFlag || base.TimeMain <= keyTime[0] || !base.KClass.Key_Z)
		{
			return;
		}
		if (base.StageData.ContinueTimes == 0)
		{
			EndStage ed = new EndStage(base.StageData, "FinalReplaySaveMenu", NextInit: true);
			ed.CtrlCall = delegate
			{
				if (ed.Time == ed.LifeTime)
				{
					base.BoundRect = new Rectangle(0, 0, 640, 480);
				}
			};
			base.GlobalData.LastState = this;
			if (base.StageData.C_History.PracticeLevel < 8)
			{
				base.StageData.C_History.PracticeLevel = 8;
			}
		}
		else
		{
			new EndStage(base.StageData, "MainMenu", NextInit: true);
			base.GlobalData.LastState = this;
		}
	}

	public override void BGM_ON()
	{
	}

	public override void Render()
	{
		base.GlobalData.ScreenTexMan.Begin();
		base.Background3D.Show();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
		base.DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
		base.Background2.Show(Active: true);
		base.SpriteMain.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.Background2.Show(Active: false);
		base.SpriteMain.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
		base.DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			if (x.Active)
			{
				x.Show();
			}
		};
		foreach (BaseEffect x in SlimDX.Compat.ListCompat.Snapshot(base.EffectList)){
			if (x.Active)
			{
				x.Show();
			}
		};
		base.SpriteMain.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			if (!x.Active && x.Layer < 0)
			{
				x.Show();
			}
		};
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			if (!x.Active && x.Layer == 0)
			{
				x.Show();
			}
		};
		foreach (BaseBullet_Touhou x in SlimDX.Compat.ListCompat.Snapshot(base.BulletList)){
			if (!x.Active && x.Layer > 0)
			{
				x.Show();
			}
		};
		foreach (BaseEffect x in SlimDX.Compat.ListCompat.Snapshot(base.EffectList)){
			if (!x.Active && x.Layer < 0)
			{
				x.Show();
			}
		};
		foreach (BaseEffect x in SlimDX.Compat.ListCompat.Snapshot(base.EffectList)){
			if (!x.Active && x.Layer == 0)
			{
				x.Show();
			}
		};
		foreach (BaseEffect x in SlimDX.Compat.ListCompat.Snapshot(base.EffectList)){
			if (!x.Active && x.Layer > 0)
			{
				x.Show();
			}
		};
		if (base.Story != null)
		{
			base.Story.Show();
		}
		base.SpriteMain.End();
		base.Particle3D.Show();
		base.GlobalData.ScreenTexMan.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.GlobalData.EffectMain = base.GlobalData.EffectDictionary["Filter"];
		base.GlobalData.EffectMain.Technique = "technique1";
		base.GlobalData.EffectMain.Begin();
		base.GlobalData.EffectMain.BeginPass(0);
		base.GlobalData.SpriteMain.Draw2D(base.GlobalData.ScreenTexMan.RenderTexture, new Point(0, 0), 0f, new PointF(0f, 0f), Color.White);
		base.SpriteMain.End();
		base.GlobalData.EffectMain.EndPass();
		base.GlobalData.EffectMain.End();
	}
}
