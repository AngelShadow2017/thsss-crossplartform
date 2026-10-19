using System.Drawing;
using System.IO;
using Shooting.Planes.Story;

namespace Shooting;

internal class GameState_SSS04 : BaseStage, IGameState
{
	public GameState_SSS04(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "St4";
	}

	public override void Drama()
	{
		base.Drama();
		int[] array = new int[3] { 0, 0, 8850 };
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
			if (base.StageData.C_History.PracticeLevel < 4)
			{
				base.StageData.C_History.PracticeLevel = 4;
			}
			base.StageData.ChangeBGM("BGM/Stage04.wav", (testStartTime + 60) * 44100 / 60, 0, 255, 575990, 7775976);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "万花世界的光与影", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Background3D.Envi = new EnvironmentSSS04(base.StageData);
			BackgroundTransitionIn backgroundTransitionIn = new BackgroundTransitionIn(base.StageData);
			backgroundTransitionIn.Delay = 20;
			backgroundTransitionIn.LifeTime = 150;
			if (testStartTime > base.TimeMain)
			{
				base.TimeMain = testStartTime;
			}
			if (base.TimeMain < array[2])
			{
				string[] array2 = (int)base.StageData.Difficulty switch
				{
					0 => Directory.GetFiles("CS/St04/A/E/", "*.mbg"), 
					1 => Directory.GetFiles("CS/St04/A/N/", "*.mbg"), 
					2 => Directory.GetFiles("CS/St04/A/H/", "*.mbg"), 
					3 => Directory.GetFiles("CS/St04/A/L/", "*.mbg"), 
					_ => Directory.GetFiles("CS/St04/A/L/", "*.mbg"), 
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
			bossStar.ColorType = 2;
		}
		else if (array[2] == base.TimeMain)
		{
			base.StageData.RemoveBullets();
			new Boss_Kage01(base.StageData);
			new Story_SSS04_01(base.StageData);
			base.Background3D.WarpEnabled = false;
			base.Background3D.WarpColorKey = 0;
		}
	}

	public override void BGM_ON()
	{
	}
}
