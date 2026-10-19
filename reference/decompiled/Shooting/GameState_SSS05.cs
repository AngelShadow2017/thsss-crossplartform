using System.Drawing;
using System.IO;
using Shooting.Planes.Story;

namespace Shooting;

internal class GameState_SSS05 : BaseStage, IGameState
{
	public GameState_SSS05(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "St5";
	}

	public override void Drama()
	{
		base.Drama();
		int[] array = new int[3] { 2500, 3000, 8750 };
		if (base.TimeMain == 1)
		{
			switch ((int)base.StageData.Difficulty)
			{
			case 2:
				base.MyPlane.EnchantmentCountNeeded = 8;
				break;
			case 3:
				base.MyPlane.EnchantmentCountNeeded = 9;
				break;
			}
		}
		if (base.TimeMain == 2)
		{
			if (base.StageData.C_History.PracticeLevel < 5)
			{
				base.StageData.C_History.PracticeLevel = 5;
			}
			base.StageData.ChangeBGM(".\\BGM\\Stage05.wav", (testStartTime + 60) * 44100 / 60, 0, 255, 66150, 0);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "记忆中遥远的星星", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Background3D.Envi = new EnvironmentSSS05(base.StageData);
			BackgroundTransitionIn backgroundTransitionIn = new BackgroundTransitionIn(base.StageData);
			backgroundTransitionIn.Delay = 200;
			backgroundTransitionIn.LifeTime = 320;
			if (testStartTime > base.TimeMain)
			{
				base.TimeMain = testStartTime;
			}
			if (base.TimeMain < array[0])
			{
				string[] array2 = (int)base.StageData.Difficulty switch
				{
					0 => Directory.GetFiles(".\\CS\\St05\\A\\E\\", "*.mbg"), 
					1 => Directory.GetFiles(".\\CS\\St05\\A\\N\\", "*.mbg"), 
					2 => Directory.GetFiles(".\\CS\\St05\\A\\H\\", "*.mbg"), 
					3 => Directory.GetFiles(".\\CS\\St05\\A\\L\\", "*.mbg"), 
					_ => Directory.GetFiles(".\\CS\\St05\\A\\L\\", "*.mbg"), 
				};
				foreach (string fileName in array2)
				{
					CS_Data cSData = base.StageData.LoadCS(fileName);
					CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
					cSEmitterController.OnRoad = true;
					cSEmitterController.Time = base.TimeMain - 60;
				}
			}
		}
		if (base.TimeMain == array[0])
		{
			new Boss_Rakuki04(base.StageData);
			base.StageData.RemoveBullets();
		}
		else if (base.TimeMain == array[0] + 90)
		{
			new Story_SSS05_01(base.StageData);
		}
		else if (base.TimeMain > array[1] && base.Boss == null && !base.RoadFlag)
		{
			base.RoadFlag = true;
			string[] array2 = (int)base.StageData.Difficulty switch
			{
				0 => Directory.GetFiles(".\\CS\\St05\\B\\E\\", "*.mbg"), 
				1 => Directory.GetFiles(".\\CS\\St05\\B\\N\\", "*.mbg"), 
				2 => Directory.GetFiles(".\\CS\\St05\\B\\H\\", "*.mbg"), 
				3 => Directory.GetFiles(".\\CS\\St05\\B\\L\\", "*.mbg"), 
				_ => Directory.GetFiles(".\\CS\\St05\\B\\L\\", "*.mbg"), 
			};
			foreach (string fileName in array2)
			{
				CS_Data cSData = base.StageData.LoadCS(fileName);
				CSEmitterController cSEmitterController2 = new CSEmitterController(base.StageData, cSData);
				cSEmitterController2.OnRoad = true;
				cSEmitterController2.Time = base.TimeMain - 60;
			}
		}
		if (array[2] - 130 == base.TimeMain)
		{
			BossStar bossStar = new BossStar(base.StageData, new PointF(base.BoundRect.Width / 2, 150f));
			bossStar.ColorType = 1;
		}
		else if (array[2] == base.TimeMain)
		{
			base.StageData.RemoveBullets();
			new Boss_Rakukun01(base.StageData);
			new Story_SSS05_02(base.StageData);
			base.Background3D.WarpEnabled = false;
			base.Background3D.WarpColorKey = 0;
		}
	}

	public override void BGM_ON()
	{
	}
}
