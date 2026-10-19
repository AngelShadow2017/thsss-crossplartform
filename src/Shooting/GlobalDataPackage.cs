using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX.Direct3D9;
using SlimDX.XAudio2;

namespace Shooting;

public class GlobalDataPackage
{
	public const int WindowWidth = 640;

	public const int WindowHeight = 480;

	public XAudio2 DeviceXaudio2;

	public MasteringVoice MasteringVoice;

	public Device DeviceMain;

	public PresentParameters presentParams;

	public MySprite SpriteMain;

	public Effect EffectMain;

	public Dictionary<string, Effect> EffectDictionary;

	public Dictionary<string, TextureObject> TextureObjectDictionary;

	public Dictionary<string, XAudio2_Player> SoundDictionary;

	public Dictionary<string, IModel> ModelDictionary;

	public Dictionary<string, bool[,]> BulletPicDictionary;

	public Wave_Player BGM_Player;

	public List<string> LoadingList;

	public KeyClass KClass = new KeyClass();

	public bool Clear = false;

	public Replay Rep;

	public PlayerData PData;

	public ScreenTexManager ScreenTexMan;

	public IGameState LastState;

	public int BGMVolume = 100;

	public int SEVolume = 80;

	public string FontType = "宋体";

	private bool SoundInitSuccess;

	public GlobalDataPackage()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		TextureObjectDictionary = new Dictionary<string, TextureObject>();
		try
		{
			DeviceXaudio2 = new XAudio2();
			MasteringVoice = new MasteringVoice(DeviceXaudio2);
			SoundInitSuccess = true;
			BGM_Player = new Wave_Player(DeviceXaudio2);
		}
		catch
		{
			MessageBox.Show("音频设备故障", "Sound Initial Error");
		}
	}

	public void LoadResources1()
	{
		ScreenTexMan = new ScreenTexManager(DeviceMain);
		LoadEffect();
		LoadingList = new List<string>();
		LoadFolderTextureList("Image");
	}

	public void LoadResources()
	{
		PData = new PlayerData();
		LoadSound();
		LoadBulletPic();
		while (LoadingList.Count > 0)
		{
			Thread.Sleep(1);
		}
		Hash.Init();
	}

	public void LoadFolderTextureList(string Path)
	{
		if (!(Path == "Image/NowLoading"))
		{
			string[] files = Directory.GetFiles(Path, "*.png");
			string[] array = files;
			foreach (string fileName in array)
			{
				FileInfo fileInfo = new FileInfo(fileName);
				LoadingList.Add(fileInfo.FullName);
			}
			string[] directories = Directory.GetDirectories(Path);
			array = directories;
			foreach (string fileName in array)
			{
				LoadFolderTextureList(fileName);
			}
		}
	}

	private void LoadEffect()
	{
		var effects = new Dictionary<string, Effect>();
		try
		{
			foreach (string name in new[] { "Blur", "Warp", "Filter" })
				effects.Add(name, Effect.FromFile(DeviceMain, "FX/" + name + ".fx", ShaderFlags.None));
		}
		catch (Exception error)
		{
			foreach (var effect in effects.Values) effect.Dispose();
			throw new InvalidOperationException("Failed to load game FX resources; initialization stopped instead of retrying indefinitely.", error);
		}
		if (EffectDictionary != null)
			foreach (var effect in EffectDictionary.Values) effect.Dispose();
		EffectDictionary = effects;
	}

	public void LoadModelDictionary()
	{
		ModelDictionary = new Dictionary<string, IModel>();
		LoadModel("Petal00", 0);
		LoadModel("Petal01", 0);
		LoadModel("Petal02", 0);
		LoadModel("Petal03", 0);
		LoadModel("Petal10", 0);
		LoadModel("Petal11", 0);
		LoadModel("Petal12", 0);
		LoadModel("Petal13", 0);
		LoadModel("Petal20", 0);
		LoadModel("Petal21", 0);
		LoadModel("Petal22", 0);
		LoadModel("Petal23", 0);
		LoadModel("Petal30", 0);
		LoadModel("Petal31", 0);
		LoadModel("Petal32", 0);
		LoadModel("Petal33", 0);
		LoadModel("Effect-0", 0);
		LoadModel("Effect-1", 0);
		LoadModel("BG02d", 0);
		LoadModel("liba", 0);
		LoadModel("zhu", 0);
		LoadModel("柳树", 0);
		LoadModel("草地", 0);
		LoadModel("地砖", 0);
		LoadModel("黑暗滤镜", 0);
		LoadModel("极光素材", 0);
		LoadModel("万花镜素材", 0);
		LoadModel("草地ex", 0);
		LoadModel("Tree1", 0);
		LoadModel("Tree2", 0);
		LoadModel("591-1", 0);
		LoadModel("591-1R", 0);
		LoadModel("Leaf", 0);
		LoadModel("Tree", 0);
		LoadModel("Cloud01", 0);
		LoadModel("Cloud02", 0);
		LoadModel("夜空（暗）", 0);
		LoadModel("夜空（亮）", 0);
		LoadModel("云（暗层）", 0);
		LoadModel("云（亮层）", 0);
		LoadModel("BG01a", 0);
		LoadModel("Stair", 2);
		LoadModel("祭坛", 2);
	}

	private void LoadModel(string Name, int ModelType)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		int num = 0;
		while (!flag && num < 20)
		{
			try
			{
				switch (ModelType)
				{
				case 0:
				{
					IModel value2 = new Model(DeviceMain, TextureObjectDictionary[Name], Name);
					ModelDictionary.Add(Name, value2);
					break;
				}
				case 1:
				{
					IModel value = new Model_Circle(DeviceMain, TextureObjectDictionary[Name], Name);
					ModelDictionary.Add(Name, value);
					break;
				}
				case 2:
				{
					IModel model = new Model_Mesh(DeviceMain, Name)
					{
						GlobalData = this
					};
					ModelDictionary.Add(model.Name, model);
					break;
				}
				}
				flag = true;
			}
			catch
			{
				flag = false;
				num++;
			}
		}
		if (num == 20)
		{
			MessageBox.Show(Name + "模型生成时出错", "LoadModel Error");
		}
	}

	private void LoadBulletPic()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		try
		{
			BulletPicDictionary = new Dictionary<string, bool[,]>();
			for (int i = 0; i < Directory.GetFiles("BPic", "*.png").Length; i++)
			{
				FileInfo fileInfo = new FileInfo(Directory.GetFiles("BPic", "*.png")[i]);
				Bitmap val = (Bitmap)Image.FromFile(fileInfo.FullName);
				bool[,] array = new bool[((Image)val).Width, ((Image)val).Height];
				for (int j = 0; j < ((Image)val).Width; j++)
				{
					for (int k = 0; k < ((Image)val).Height; k++)
					{
						array[j, k] = val.GetPixel(j, k).G < 128;
					}
				}
				BulletPicDictionary.Add(fileInfo.Name, array);
			}
		}
		catch
		{
			MessageBox.Show("图像读取错误", "LoadBPic Error");
		}
	}

	public void LoadSound()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!SoundInitSuccess)
		{
			return;
		}
		SoundDictionary = new Dictionary<string, XAudio2_Player>();
		for (int i = 0; i < Directory.GetFiles("Sound", "*.wav").Length; i++)
		{
			FileInfo fileInfo = new FileInfo(Directory.GetFiles("Sound", "*.wav")[i]);
			SoundDictionary.Add(fileInfo.Name, new XAudio2_Player(fileInfo.FullName, DeviceXaudio2)
			{
				Volume = SEVolume
			});
			if (!SoundDictionary[fileInfo.Name].loadSuccess)
			{
				MessageBox.Show(fileInfo.Name + "读取错误", "LoadSound Error");
			}
		}
	}

	public void TextureObjectLoader(string imageFileName)
	{
		TextureObjectLoader(imageFileName, 0);
	}

	public void TextureObjectLoader(string imageFileName, int colorKey)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!File.Exists(imageFileName))
		{
			return;
		}
		Bitmap val = (Bitmap)Image.FromFile(imageFileName);
		Rectangle rect = new Rectangle(0, 0, ((Image)val).Width, ((Image)val).Height);
		((Image)val).Dispose();
		int num = 0;
		Texture texture;
		while (!SetTexture(imageFileName, colorKey, rect, out texture) && num++ < 20)
		{
		}

		if (num >= 20)
		{
			MessageBox.Show(imageFileName + "纹理读取错误", "LoadTexture Error");
		}
		FileInfo fileInfo = new FileInfo(imageFileName);
		string path = fileInfo.FullName.Replace(fileInfo.Extension, ".txt");
		if (File.Exists(path))
		{
			char[] separator = new char[1] { '\t' };
			string[] array = File.ReadAllLines(path, Encoding.GetEncoding(936));
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(separator);
				TextureObject textureObject = new TextureObject();
				textureObject.TXTure = texture;
				textureObject.PosRect = new Rectangle(Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]), Convert.ToInt32(array2[3]), Convert.ToInt32(array2[4]));
				textureObject.OffsetX = Convert.ToInt32(array2[5]);
				textureObject.OffsetY = Convert.ToInt32(array2[6]);
				textureObject.SrcWidth = rect.Width;
				textureObject.SrcHeight = rect.Height;
				TextureObjectDictionary.Add(array2[0], textureObject);
			}
		}
		else
		{
			TextureObject textureObject2 = new TextureObject();
			textureObject2.TXTure = texture;
			textureObject2.PosRect = new Rectangle(0, 0, rect.Width, rect.Height);
			textureObject2.SrcWidth = rect.Width;
			textureObject2.SrcHeight = rect.Height;
			TextureObjectDictionary.Add(fileInfo.Name.Replace(".png", ""), textureObject2);
		}
	}

	private bool SetTexture(string imageFileName, int colorKey, Rectangle Rect, out Texture texture)
	{
		try
		{
			texture = Texture.FromFile(DeviceMain, imageFileName, Rect.Width, Rect.Height, 0, Usage.None, Format.Unknown, Pool.Managed, Filter.Linear, Filter.None, colorKey);
			return true;
		}
		catch
		{
			texture = null;
			return false;
		}
	}

	public void BGM_Play(string FileName)
	{
		try
		{
			int bGMVolume = BGMVolume;
			BGM_Player.FileName = FileName;
			BGM_Player.Volume = bGMVolume;
			BGM_Player.Play();
		}
		catch
		{
		}
	}

	public void BGM_OFF()
	{
		BGM_Player.Stop();
	}

	public void BGM_Pause()
	{
		BGM_Player.Puase();
	}

	public void BGM_Resume()
	{
		if (BGM_Player != null)
		{
			BGM_Player.Resume();
		}
	}

	public void OnLostDevice()
	{
		if (SpriteMain != null)
		{
			SpriteMain.sprite.OnLostDevice();
		}
		if (ScreenTexMan != null)
		{
			ScreenTexMan.Dispose();
		}
		if (EffectDictionary == null)
		{
			return;
		}
		foreach (Effect value in EffectDictionary.Values)
		{
			value.OnLostDevice();
		}
	}

	public void OnResetDevice()
	{
		SpriteMain.sprite.OnResetDevice();
		DeviceMain.Reset(presentParams);
		if (ScreenTexMan != null)
		{
			ScreenTexMan.Reset();
		}
		if (EffectDictionary == null)
		{
			return;
		}
		foreach (Effect value in EffectDictionary.Values)
		{
			value.OnResetDevice();
		}
	}

	public void Dispose()
	{
		DeviceMain.Direct3D.Dispose();
		SpriteMain.Dispose();
		if (TextureObjectDictionary != null)
		{
			foreach (TextureObject value in TextureObjectDictionary.Values)
			{
				value.Dispose();
			}
		}
		if (SoundDictionary != null)
		{
			foreach (XAudio2_Player value2 in SoundDictionary.Values)
			{
				value2.Dispose();
			}
		}
		if (BGM_Player != null)
		{
			BGM_Player.Dispose();
		}
		if (ScreenTexMan != null)
		{
			ScreenTexMan.Dispose();
		}
		if (EffectDictionary != null)
		{
			foreach (Effect value3 in EffectDictionary.Values)
			{
				value3.Dispose();
			}
		}
		if (ModelDictionary != null)
		{
			foreach (Model value4 in ModelDictionary.Values)
			{
				value4.Dispose();
			}
		}
		if (DeviceXaudio2 != null)
		{
			DeviceXaudio2.Dispose();
		}
	}
}
