using System;
using System.Drawing;
using System.IO;
using Shooting.Planes.Story;

namespace Shooting;

internal class GameState_SSS01 : BaseStage, IGameState
{
	public GameState_SSS01(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		base.StageName = "St1";
	}

	public override void Drama()
	{
		base.Drama();
		int[] array = new int[3] { 3180, 4000, 6440 };
		if (base.TimeMain == 1)
		{
			switch ((int)base.StageData.Difficulty)
			{
			case 2:
				base.MyPlane.EnchantmentCountNeeded = 3;
				break;
			case 3:
				base.MyPlane.EnchantmentCountNeeded = 4;
				break;
			}
			new TransitionIn(base.StageData);
		}
		if (base.TimeMain == 2)
		{
			if (base.StageData.C_History.PracticeLevel < 1)
			{
				base.StageData.C_History.PracticeLevel = 1;
			}
			base.StageData.ChangeBGM("BGM/Stage01.wav", (testStartTime + 60) * 44100 / 60, 0, 0, 0, 0);
			MusicTitle musicTitle = new MusicTitle(base.StageData, "宁静夏夜的微风", new Point(base.BoundRect.Width, base.BoundRect.Height - 16));
			musicTitle.OriginalPosition = new PointF(base.BoundRect.Width, base.BoundRect.Height + 100);
			musicTitle.Scale = 0.5f;
			base.Background3D.Envi = new EnvironmentSSS01(base.StageData);
			BackgroundTransitionIn backgroundTransitionIn = new BackgroundTransitionIn(base.StageData);
			backgroundTransitionIn.Delay = 200;
			backgroundTransitionIn.LifeTime = 320;
			new BackgroundGroupSSS01(base.StageData);
			if (testStartTime > base.TimeMain)
			{
				base.TimeMain = testStartTime;
			}
			if (base.TimeMain < array[0])
			{
				string[] array2 = (int)base.StageData.Difficulty switch
				{
					0 => Directory.GetFiles("CS/St01/A/E/", "*.mbg"), 
					1 => Directory.GetFiles("CS/St01/A/N/", "*.mbg"), 
					2 => Directory.GetFiles("CS/St01/A/H/", "*.mbg"), 
					3 => Directory.GetFiles("CS/St01/A/L/", "*.mbg"), 
					_ => Directory.GetFiles("CS/St01/A/L/", "*.mbg"), 
				};
				foreach (string fileName in array2)
				{
					CS_Data cSData = base.StageData.LoadCS(fileName);
					CSEmitterController cSEmitterController = new CSEmitterController(base.StageData, cSData);
					cSEmitterController.OnRoad = true;
					cSEmitterController.Time = base.TimeMain + 60;
				}
			}
			TextTwinkle textTwinkle = new TextTwinkle(base.StageData, base.StageData.Difficulty.ToString(), new PointF(base.BoundRect.X + base.BoundRect.Width / 2, base.BoundRect.Y + 16), 0f, Math.PI / 2.0);
			textTwinkle.TwinkleColor = Color.Gray;
			textTwinkle.Circle = 10;
			textTwinkle.LifeTime = 150;
		}
		else if (base.TimeMain == 30)
		{
			TextTwinkle textTwinkle2 = new TextTwinkle(base.StageData, "ItemGetBorderLine", new PointF(base.BoundRect.X + base.BoundRect.Width / 2, base.BoundRect.Y + 130), 0f, Math.PI / 2.0);
			textTwinkle2.TwinkleColor = Color.OrangeRed;
			TextTwinkle textTwinkle3 = new TextTwinkle(base.StageData, "ItemGetBorderLine01", new PointF(base.BoundRect.X + base.BoundRect.Width / 2 - 168, base.BoundRect.Y + 130), 0f, Math.PI / 2.0);
			textTwinkle3.TwinkleColor = Color.OrangeRed;
			TextTwinkle textTwinkle4 = new TextTwinkle(base.StageData, "ItemGetBorderLine01", new PointF(base.BoundRect.X + base.BoundRect.Width / 2 + 158, base.BoundRect.Y + 130), 0f, Math.PI / 2.0);
			textTwinkle4.TwinkleColor = Color.OrangeRed;
		}
		if (base.TimeMain == array[0])
		{
			base.StageData.RemoveBullets();
			new Boss_Ami01(base.StageData);
			new EmitterBossFire(base.StageData, base.Boss.OriginalPosition, Color.FromArgb(40, 255, 20));
			MagicCircle magicCircle = new MagicCircle(base.StageData, "MagicCircleSix");
			magicCircle.Scale = 1.5f;
			base.Background3D.WarpEnabled = true;
			base.Background3D.WarpColorKey = 0;
		}
		else if (base.TimeMain > array[1] && base.Boss == null && !base.RoadFlag)
		{
			base.RoadFlag = true;
			string[] array2 = (int)base.StageData.Difficulty switch
			{
				0 => Directory.GetFiles("CS/St01/B/E/", "*.mbg"), 
				1 => Directory.GetFiles("CS/St01/B/N/", "*.mbg"), 
				2 => Directory.GetFiles("CS/St01/B/H/", "*.mbg"), 
				3 => Directory.GetFiles("CS/St01/B/L/", "*.mbg"), 
				_ => Directory.GetFiles("CS/St01/B/L/", "*.mbg"), 
			};
			foreach (string fileName in array2)
			{
				CS_Data cSData = new CS_Data(fileName);
				cSData.String2Data(base.StageData);
				CSEmitterController cSEmitterController2 = new CSEmitterController(base.StageData, cSData);
				cSEmitterController2.OnRoad = true;
				cSEmitterController2.Time = base.TimeMain + 60;
			}
		}
		else if (array[2] - 130 == base.TimeMain)
		{
			BossStar bossStar = new BossStar(base.StageData, new PointF(base.BoundRect.Width / 2, 150f));
			bossStar.ColorType = 6;
		}
		else if (array[2] == base.TimeMain)
		{
			base.StageData.RemoveBullets();
			new Boss_Ami02(base.StageData);
			switch (base.MyPlane.Name)
			{
			case "Aya":
				new Story_SSS01_01(base.StageData);
				break;
			case "Reimu":
				new Story_SSS01_01A(base.StageData);
				break;
			case "Marisa":
				new Story_SSS01_01B(base.StageData);
				break;
			case "Sanae":
				new Story_SSS01_01C(base.StageData);
				break;
			case "Koishi":
				new Story_SSS01_01D(base.StageData);
				break;
			default:
				new Story_SSS01_01X(base.StageData);
				break;
			}
			base.Background3D.WarpEnabled = false;
			base.Background3D.WarpColorKey = 0;
		}
	}

	public override void BGM_ON()
	{
	}
}
