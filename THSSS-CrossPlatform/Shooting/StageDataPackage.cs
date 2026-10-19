using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;

namespace Shooting;

public class StageDataPackage
{
	public StateSwitchDataPackage StateSwitchData;

	public ReplayInfo RepInfo;

	public long TimeTotal;

	public long RealTimeTotal;

	public int TimeMain;

	public Rectangle BoundRect;

	public BaseMyPlane MyPlane;

	public BaseBossTouhou Boss;

	public List<BaseBullet_Touhou> BulletList;

	public List<BaseObject> MyBulletList;

	public List<BaseEnemyPlane> EnemyPlaneList;

	public List<BaseEffect> EffectList;

	public List<BaseObject> SpellList;

	public List<BaseObject> InterfaceList;

	public List<BaseItem> ItemList;

	public BaseStory Story;

	public BackgroundManager Background;

	public BackgroundManager Background2;

	public BackgroundManager3D Background3D;

	public ParticleManager3D Particle3D;

	public List<XAudio2_Player> SoundPlayList;

	public List<BaseMenuGroup> MenuGroupList;

	public MyRandom Ran = new MyRandom();

	public bool C_Check = false;

	public bool OnReplay = false;

	public bool OnPractice = false;

	public DifficultLevel Difficulty;

	public int ContinueTimes = 0;

	private int VibrateTime = -1;

	private Rectangle VibrateRectSave;

	public int SlowMode = 0;

	public string CurrentStageName = null;

	public string SCBstring = null;

	public Dictionary<string, CS_Data> CS_Cathe = new Dictionary<string, CS_Data>();

	public ClearHistory C_History { get; set; }

	public List<ScoreHistory> S_History { get; set; }

	public GlobalDataPackage GlobalData { get; set; }

	public Device DeviceMain => GlobalData.DeviceMain;

	public Dictionary<string, IModel> ModelDictionary => GlobalData.ModelDictionary;

	public Dictionary<string, TextureObject> TextureObjectDictionary => GlobalData.TextureObjectDictionary;

	public Dictionary<string, XAudio2_Player> SoundDictionary => GlobalData.SoundDictionary;

	public Wave_Player BGM_Player
	{
		get
		{
			return GlobalData.BGM_Player;
		}
		set
		{
			GlobalData.BGM_Player = value;
		}
	}

	public KeyClass KClass => GlobalData.KClass;

	public MySprite SpriteMain => GlobalData.SpriteMain;

	public Replay Rep => GlobalData.Rep;

	public ScreenTexManager ScreenTexMan => GlobalData.ScreenTexMan;

	public Effect EffectMain
	{
		get
		{
			return GlobalData.EffectMain;
		}
		set
		{
			GlobalData.EffectMain = value;
		}
	}

	public Dictionary<string, Effect> EffectDictionary => GlobalData.EffectDictionary;

	public PlayerData PData
	{
		get
		{
			return GlobalData.PData;
		}
		set
		{
			GlobalData.PData = value;
		}
	}

	public string SlowRate
	{
		get
		{
			float num = 60f * (float)RealTimeTotal / (float)Stopwatch.Frequency;
			num = (num - (float)TimeTotal) * 100f / num;
			return ((num < 0f) ? 0f : num).ToString("F1") + "%";
		}
	}

	public StageDataPackage(GlobalDataPackage GlobalData)
	{
		this.GlobalData = GlobalData;
		Init();
	}

	public void Init()
	{
		BoundRect = new Rectangle(0, 0, 640, 480);
		BulletList = new List<BaseBullet_Touhou>();
		EnemyPlaneList = new List<BaseEnemyPlane>();
		MyBulletList = new List<BaseObject>();
		SpellList = new List<BaseObject>();
		EffectList = new List<BaseEffect>();
		ItemList = new List<BaseItem>();
		InterfaceList = new List<BaseObject>();
		SoundPlayList = new List<XAudio2_Player>();
		Background = new BackgroundManager();
		Background2 = new BackgroundManager();
		Background3D = new BackgroundManager3D(this);
		Particle3D = new ParticleManager3D(this);
		MenuGroupList = new List<BaseMenuGroup>();
		TimeMain = 0;
		RepInfo = new ReplayInfo();
	}

	public void Vibrate()
	{
		if (VibrateTime > TimeMain)
		{
			int num = 0;
			num = ((VibrateTime - TimeMain >= 20) ? 10 : ((VibrateTime - TimeMain) / 2));
			int num2 = Ran.Next(num) - num / 2;
			int num3 = Ran.Next(num) - num / 2;
			BoundRect = new Rectangle(VibrateRectSave.X + num2, VibrateRectSave.Y + num3, VibrateRectSave.Width, VibrateRectSave.Height);
		}
		else if (VibrateTime == TimeMain)
		{
			BoundRect = VibrateRectSave;
		}
	}

	public void VibrateStart(int CuntinueTime)
	{
		if (VibrateTime < TimeMain)
		{
			VibrateRectSave = BoundRect;
		}
		VibrateTime = TimeMain + CuntinueTime;
	}

	public void VibrateStop()
	{
		if (VibrateTime >= TimeMain)
		{
			VibrateTime = -1;
			BoundRect = VibrateRectSave;
		}
	}

	public void RemoveBullets()
	{
		foreach (BaseBullet_Touhou bullet in BulletList)
		{
			bullet.GiveEndEffect();
		}
		BulletList.Clear();
		foreach (BaseEnemyPlane enemyPlane in EnemyPlaneList)
		{
			enemyPlane.GiveEndEffect();
			if (enemyPlane.TxtureObject != null)
			{
				SoundPlay("se_enep00.wav");
			}
		}
		EnemyPlaneList.Clear();
		foreach (BaseObject myBullet in MyBulletList)
		{
			ParticleSmaller particleSmaller = new ParticleSmaller(this, "光点", myBullet.Position, myBullet.Velocity, myBullet.Direction);
			particleSmaller.LifeTime = 30;
			particleSmaller.Scale = 0.5f;
		}
		MyBulletList.Clear();
	}

	public void SoundPlay(string MusicName)
	{
		SoundPlay(MusicName, 0.5f);
	}

	public void SoundPlay(string MusicName, float VoicePos)
	{
		if (MusicName != null && SoundDictionary.ContainsKey(MusicName))
		{
			if (!SoundPlayList.Contains(SoundDictionary[MusicName]))
			{
				SoundDictionary[MusicName].VoicePos = VoicePos;
				SoundPlayList.Add(SoundDictionary[MusicName]);
			}
			else
			{
				SoundDictionary[MusicName].VoicePos = (VoicePos + SoundDictionary[MusicName].VoicePos) / 2f;
			}
		}
	}

	public void ChangeBGM(string MusicName, int PlayBegin, int PlayLength, int LoopCount, int LoopBegin, int LoopEnd)
	{
		int bGMVolume = GlobalData.BGMVolume;
		BGM_Player.FileName = MusicName;
		BGM_Player.Volume = bGMVolume;
		BGM_Player.PlayRepeat(PlayBegin, PlayLength, LoopCount, LoopBegin, LoopEnd);
	}

	public void ChangeBGM(string MusicName, bool Repeat)
	{
		if (Repeat)
		{
			ChangeBGM(MusicName, 0, 0, 255, 0, 0);
		}
		else
		{
			ChangeBGM(MusicName, 0, 0, 0, 0, 0);
		}
	}

	public void SetReplayInfo(string StartStage)
	{
		RepInfo.MyPlaneName = MyPlane.Name;
		RepInfo.Rank = Difficulty;
		RepInfo.WeaponType = MyPlane.WeaponType;
		RepInfo.StartStage = StartStage;
		SetReplayStageInfo();
	}

	public void SetReplayStageInfo()
	{
		if (!OnReplay && (RepInfo.MyPlaneData.Count <= 0 || RepInfo.MyPlaneData[RepInfo.MyPlaneData.Count - 1].DataPosition + 2 != Rep.DataPosition))
		{
			RepInfo.LastStage = CurrentStageName;
			RepInfo.SlowRate = SlowRate;
			RepInfo.MyPlaneData.Add(new MyPlaneInfo
			{
				Score = MyPlane.Score,
				Life = MyPlane.Life,
				Spell = MyPlane.Spell,
				Power = MyPlane.Power,
				Graze = MyPlane.Graze,
				PosX = MyPlane.OriginalPosition.X,
				PosY = MyPlane.OriginalPosition.Y,
				LifeChip = MyPlane.LifeChip,
				SpellChip = MyPlane.SpellChip,
				LifeUpCount = MyPlane.LifeUpCount,
				StarPoint = MyPlane.StarPoint,
				HighItemScore = MyPlane.HighItemScore,
				Rate = MyPlane.Rate,
				LastColor = MyPlane.LastColor,
				DataPosition = Rep.DataPosition
			});
			Rep.WriteKey(57358);
		}
	}

	public bool FindScore(ScoreHistory SH)
	{
		return SH.Rank == Difficulty && SH.MyPlaneFullName == MyPlane.FullName;
	}

	public bool FindClear(ClearHistory CH)
	{
		return CH.Rank == Difficulty && CH.MyPlaneFullName == MyPlane.FullName;
	}

	public void PreLoadCS(string Path)
	{
		string[] files = Directory.GetFiles(Path, "*.mbg");
		string[] array = files;
		foreach (string text in array)
		{
			FileInfo fileInfo = new FileInfo(text);
			CS_Data cS_Data = new CS_Data(text);
			cS_Data.String2Data(this);
			CS_Cathe.Add(text, cS_Data);
		}
		string[] directories = Directory.GetDirectories(Path);
		array = directories;
		foreach (string text in array)
		{
			PreLoadCS(text);
		}
	}

	public void ClearCSCathe()
	{
		CS_Cathe.Clear();
	}

	public CS_Data LoadCS(string FileName)
	{
		if (CS_Cathe.ContainsKey(FileName))
		{
			return CS_Cathe[FileName];
		}
		CS_Data cS_Data = new CS_Data(FileName);
		cS_Data.String2Data(this);
		return cS_Data;
	}

	public Texture DrawString(string text, System.Drawing.Font font, Brush brush, int width, int height)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Bitmap val = new Bitmap(width, height, (PixelFormat)2498570);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		val2.TextRenderingHint = (TextRenderingHint)4;
		val2.DrawString(text, font, brush, 0f, 0f);
		val2.Dispose();
		Texture texture = new Texture(DeviceMain, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
		SurfaceDescription levelDescription = texture.GetLevelDescription(0);
		DataRectangle dataRectangle = texture.LockRectangle(0, new Rectangle(0, 0, width, height), LockFlags.Discard);
		BitmapData val3 = val.LockBits(new Rectangle(0, 0, width, height), (ImageLockMode)1, ((Image)val).PixelFormat);
		IntPtr scan = val3.Scan0;
		dataRectangle.Data.WriteRange(scan, val3.Width * val3.Height * 4);
		val.UnlockBits(val3);
		texture.UnlockRectangle(0);
		((Image)val).Dispose();
		return texture;
	}

	public void Dispose()
	{
		if (Background3D != null)
		{
			Background3D.Dispose();
		}
		if (Particle3D != null)
		{
			Particle3D.Dispose();
		}
		if (Rep != null)
		{
			Rep.Dispose();
		}
	}
}
