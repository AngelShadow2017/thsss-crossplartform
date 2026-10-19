using System;
using System.Drawing;
using System.IO;
using Shooting.Planes.Story;

namespace Shooting;

internal class GameState_SSSEx : BaseStage, IGameState
{
	private new int testStartTime = 0;

	public GameState_SSSEx(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "StEx";
	}

	public override void Drama()
	{
		base.Drama();
		int[] array = new int[3] { 4920, 5060, 10260 };
		if (base.TimeMain == 1)
		{
			base.MyPlane.EnchantmentCountNeeded = 10;
		}
		if (base.TimeMain == 62)
		{
			if (base.StageData.C_History.PracticeLevel < 8)
			{
				base.StageData.C_History.PracticeLevel = 8;
			}
			base.StageData.ChangeBGM("BGM/StageEx.wav", (testStartTime + 60) * 44100 / 60, 0, 255, 1341963, 7621362);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "来自仙界的新风", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Background3D.Envi = new EnvironmentSSSEx(base.StageData);
			BackgroundTransitionIn backgroundTransitionIn = new BackgroundTransitionIn(base.StageData);
			backgroundTransitionIn.Delay = 0;
			backgroundTransitionIn.LifeTime = 320;
			if (testStartTime > base.TimeMain)
			{
				base.TimeMain = testStartTime;
			}
			if (base.TimeMain < array[0])
			{
				string[] files = Directory.GetFiles("CS/StEx/A/", "*.mbg");
				string[] array2 = files;
				foreach (string fileName in array2)
				{
					CS_Data cSData = base.StageData.LoadCS(fileName);
					CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
					cSEmitterController.OnRoad = true;
					cSEmitterController.Time = base.TimeMain - 60;
				}
			}
			TextTwinkle textTwinkle = new TextTwinkle(base.StageData, base.StageData.Difficulty.ToString(), new PointF(base.BoundRect.X + base.BoundRect.Width / 2, base.BoundRect.Y + 16), 0f, Math.PI / 2.0);
			textTwinkle.TwinkleColor = Color.Gray;
			textTwinkle.Circle = 10;
			textTwinkle.LifeTime = 150;
		}
		if (base.TimeMain == array[0])
		{
			base.StageData.RemoveBullets();
			new Boss_Seiryuu03(base.StageData);
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(40, 255, 20));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
			base.Background3D.WarpColorKey = 2;
		}
		else if (base.TimeMain > array[1] && base.Boss == null && !base.RoadFlag)
		{
			base.RoadFlag = true;
			string[] files = Directory.GetFiles("CS/StEx/B/", "*.mbg");
			string[] array2 = files;
			foreach (string fileName in array2)
			{
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController2 = new CSEmitterController(base.StageData, cSData);
				cSEmitterController2.OnRoad = true;
				cSEmitterController2.Time = base.TimeMain - 60;
			}
		}
		else if (array[2] - 130 == base.TimeMain)
		{
			BossStar bossStar = new BossStar(base.StageData, new PointF(base.BoundRect.Width / 2, 120f));
			bossStar.ColorType = 5;
		}
		else if (array[2] == base.TimeMain)
		{
			base.StageData.RemoveBullets();
			new Boss_Rika01(base.StageData);
			new Story_SSSEx_01(base.StageData);
			base.Background3D.WarpEnabled = false;
			base.Background3D.WarpColorKey = 2;
		}
	}

	public override void BGM_ON()
	{
	}
}
