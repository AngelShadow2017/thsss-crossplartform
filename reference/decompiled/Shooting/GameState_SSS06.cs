using System.Drawing;
using System.IO;
using Shooting.Planes.Story;

namespace Shooting;

internal class GameState_SSS06 : BaseStage, IGameState
{
	public GameState_SSS06(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "St6";
	}

	public override void Drama()
	{
		base.Drama();
		int[] array = new int[3] { 2550, 3000, 8550 };
		if (base.TimeMain == 1)
		{
			switch ((int)base.StageData.Difficulty)
			{
			case 2:
				base.MyPlane.EnchantmentCountNeeded = 6;
				break;
			case 3:
				base.MyPlane.EnchantmentCountNeeded = 7;
				break;
			}
		}
		if (base.TimeMain == 2)
		{
			if (base.StageData.C_History.PracticeLevel < 6)
			{
				base.StageData.C_History.PracticeLevel = 6;
			}
			base.StageData.ChangeBGM(".\\BGM\\Stage06.wav", (testStartTime + 60) * 44100 / 60, 0, 255, 0, 6220746);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "银河的彼方", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Background3D.Envi = new EnvironmentSSS06(base.StageData);
			BackgroundTransitionIn backgroundTransitionIn = new BackgroundTransitionIn(base.StageData);
			backgroundTransitionIn.Delay = 30;
			backgroundTransitionIn.LifeTime = 250;
			if (testStartTime > base.TimeMain)
			{
				base.TimeMain = testStartTime;
			}
			if (base.TimeMain < array[2])
			{
				string[] array2 = (int)base.StageData.Difficulty switch
				{
					0 => Directory.GetFiles(".\\CS\\St06\\A\\E\\", "*.mbg"), 
					1 => Directory.GetFiles(".\\CS\\St06\\A\\N\\", "*.mbg"), 
					2 => Directory.GetFiles(".\\CS\\St06\\A\\H\\", "*.mbg"), 
					3 => Directory.GetFiles(".\\CS\\St06\\A\\L\\", "*.mbg"), 
					_ => Directory.GetFiles(".\\CS\\St06\\A\\L\\", "*.mbg"), 
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
		if (array[2] - 130 == base.TimeMain)
		{
			BossStar bossStar = new BossStar(base.StageData, new PointF(base.BoundRect.Width / 2, 150f));
			bossStar.ColorType = 4;
		}
		else if (array[2] == base.TimeMain)
		{
			base.StageData.RemoveBullets();
			new Boss_Tensei01(base.StageData);
			new Story_SSS06_01(base.StageData);
			base.Background3D.WarpEnabled = false;
			base.Background3D.WarpColorKey = 0;
		}
	}

	public override void BGM_ON()
	{
	}
}
