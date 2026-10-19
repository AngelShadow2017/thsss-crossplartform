using System.Drawing;
using System.IO;
using Shooting.Planes.Story;

namespace Shooting;

internal class GameState_SSS02 : BaseStage, IGameState
{
	public GameState_SSS02(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "St2";
	}

	public override void Drama()
	{
		base.Drama();
		int[] array = new int[3] { 4680, 5000, 7890 };
		if (base.TimeMain == 1)
		{
			switch ((int)base.StageData.Difficulty)
			{
			case 2:
				base.MyPlane.EnchantmentCountNeeded = 5;
				break;
			case 3:
				base.MyPlane.EnchantmentCountNeeded = 6;
				break;
			}
		}
		if (base.TimeMain == 2)
		{
			if (base.StageData.C_History.PracticeLevel < 2)
			{
				base.StageData.C_History.PracticeLevel = 2;
			}
			base.StageData.ChangeBGM(".\\BGM\\Stage02.wav", (testStartTime + 60) * 44100 / 60, 0, 255, 1194228, 6866370);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "灯火竹林", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Background3D.Envi = new EnvironmentSSS02(base.StageData);
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
					0 => Directory.GetFiles(".\\CS\\St02\\A\\E\\", "*.mbg"), 
					1 => Directory.GetFiles(".\\CS\\St02\\A\\N\\", "*.mbg"), 
					2 => Directory.GetFiles(".\\CS\\St02\\A\\H\\", "*.mbg"), 
					3 => Directory.GetFiles(".\\CS\\St02\\A\\L\\", "*.mbg"), 
					_ => Directory.GetFiles(".\\CS\\St02\\A\\L\\", "*.mbg"), 
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
			base.StageData.RemoveBullets();
			new Boss_Rakuki01(base.StageData);
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(144, 255, 120));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
			base.Background3D.WarpColorKey = 2;
		}
		else if (base.TimeMain > array[1] && base.Boss == null && !base.RoadFlag)
		{
			base.RoadFlag = true;
			string[] array2 = (int)base.StageData.Difficulty switch
			{
				0 => Directory.GetFiles(".\\CS\\St02\\B\\E\\", "*.mbg"), 
				1 => Directory.GetFiles(".\\CS\\St02\\B\\N\\", "*.mbg"), 
				2 => Directory.GetFiles(".\\CS\\St02\\B\\H\\", "*.mbg"), 
				3 => Directory.GetFiles(".\\CS\\St02\\B\\L\\", "*.mbg"), 
				_ => Directory.GetFiles(".\\CS\\St02\\B\\L\\", "*.mbg"), 
			};
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
			new Boss_Rakuki02(base.StageData);
			switch (base.MyPlane.Name)
			{
			case "Aya":
				new Story_SSS02_01(base.StageData);
				break;
			case "Reimu":
				new Story_SSS02_01A(base.StageData);
				break;
			case "Marisa":
				new Story_SSS02_01B(base.StageData);
				break;
			case "Sanae":
				new Story_SSS02_01C(base.StageData);
				break;
			case "Koishi":
				new Story_SSS02_01D(base.StageData);
				break;
			default:
				new Story_SSS02_01X(base.StageData);
				break;
			}
			base.Background3D.WarpEnabled = false;
			base.Background3D.WarpColorKey = 2;
		}
	}

	public override void BGM_ON()
	{
	}
}
