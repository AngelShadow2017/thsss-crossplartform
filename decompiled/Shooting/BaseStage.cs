using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Shooting.Properties;
using SlimDX.Direct3D9;

namespace Shooting;

internal class BaseStage : BaseGameState, IGameState
{
	private IContainer components = null;

	private GameRegion GameRegion_Main;

	private GameRegion gameRegion_Score;

	private GameRegion gameRegion_Player;

	private GameRegion gameRegion_Spell;

	private GameRegion gameRegion_Power;

	private GameRegion gameRegion_LifeSelf;

	private GameRegion gameRegion_SpellLine;

	private GameRegion gameRegion_ScoreValue;

	private GameRegion gameRegion_BluePoint;

	private GameRegion gameRegion_GreenPoint;

	private GameRegion gameRegion_Graze;

	private GameRegion gameRegion_HiScoreValue;

	private GameRegion gameRegion1;

	private GameRegion gameRegion_HiScore;

	protected int testStartTime = 0;

	private string testString;

	private byte Slowflag { get; set; }

	protected bool RoadFlag { get; set; }

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((ContainerControl)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0706: Unknown result type (might be due to invalid IL or missing references)
		//IL_0786: Unknown result type (might be due to invalid IL or missing references)
		gameRegion_ScoreValue = new GameRegion();
		gameRegion_SpellLine = new GameRegion();
		gameRegion_Power = new GameRegion();
		gameRegion_Spell = new GameRegion();
		gameRegion_Player = new GameRegion();
		gameRegion_Score = new GameRegion();
		gameRegion_LifeSelf = new GameRegion();
		GameRegion_Main = new GameRegion();
		gameRegion_BluePoint = new GameRegion();
		gameRegion_GreenPoint = new GameRegion();
		gameRegion_Graze = new GameRegion();
		gameRegion_HiScoreValue = new GameRegion();
		gameRegion1 = new GameRegion();
		gameRegion_HiScore = new GameRegion();
		((Control)this).SuspendLayout();
		((Control)gameRegion_ScoreValue).BackColor = Color.Transparent;
		((Control)gameRegion_ScoreValue).BackgroundImageLayout = (ImageLayout)4;
		((UserControl)gameRegion_ScoreValue).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_ScoreValue).Location = new Point(488, 70);
		((Control)gameRegion_ScoreValue).Margin = new Padding(10, 1, 3, 1);
		((Control)gameRegion_ScoreValue).Name = "gameRegion_ScoreValue";
		((Control)gameRegion_ScoreValue).Size = new Size(110, 16);
		((Control)gameRegion_ScoreValue).TabIndex = 7;
		((Control)gameRegion_SpellLine).BackColor = Color.Transparent;
		((Control)gameRegion_SpellLine).BackgroundImage = (Image)(object)Resources.FiveStar;
		((Control)gameRegion_SpellLine).BackgroundImageLayout = (ImageLayout)4;
		((UserControl)gameRegion_SpellLine).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_SpellLine).Location = new Point(504, 127);
		((Control)gameRegion_SpellLine).Margin = new Padding(10, 1, 3, 1);
		((Control)gameRegion_SpellLine).Name = "gameRegion_SpellLine";
		((Control)gameRegion_SpellLine).Size = new Size(110, 16);
		((Control)gameRegion_SpellLine).TabIndex = 6;
		((Control)gameRegion_Power).BackColor = Color.Transparent;
		((Control)gameRegion_Power).BackgroundImage = (Image)(object)Resources.FiveStar;
		((Control)gameRegion_Power).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_Power).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_Power).Location = new Point(442, 188);
		((Control)gameRegion_Power).Name = "gameRegion_Power";
		((Control)gameRegion_Power).Size = new Size(163, 25);
		((Control)gameRegion_Power).TabIndex = 5;
		((Control)gameRegion_Spell).BackColor = Color.Transparent;
		((Control)gameRegion_Spell).BackgroundImage = (Image)(object)Resources.Spell;
		((Control)gameRegion_Spell).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_Spell).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_Spell).Location = new Point(428, 125);
		((Control)gameRegion_Spell).Name = "gameRegion_Spell";
		((Control)gameRegion_Spell).Size = new Size(70, 16);
		((Control)gameRegion_Spell).TabIndex = 4;
		((Control)gameRegion_Player).BackColor = Color.Transparent;
		((Control)gameRegion_Player).BackgroundImage = (Image)(object)Resources.Player;
		((Control)gameRegion_Player).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_Player).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_Player).Location = new Point(428, 99);
		((Control)gameRegion_Player).Name = "gameRegion_Player";
		((Control)gameRegion_Player).Size = new Size(70, 16);
		((Control)gameRegion_Player).TabIndex = 3;
		((Control)gameRegion_Score).BackColor = Color.Transparent;
		((Control)gameRegion_Score).BackgroundImage = (Image)(object)Resources.Score;
		((Control)gameRegion_Score).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_Score).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_Score).Location = new Point(428, 67);
		((Control)gameRegion_Score).Name = "gameRegion_Score";
		((Control)gameRegion_Score).Size = new Size(70, 16);
		((Control)gameRegion_Score).TabIndex = 2;
		((Control)gameRegion_LifeSelf).BackColor = Color.Transparent;
		((Control)gameRegion_LifeSelf).BackgroundImage = (Image)(object)Resources.MyPlane;
		((Control)gameRegion_LifeSelf).BackgroundImageLayout = (ImageLayout)4;
		((UserControl)gameRegion_LifeSelf).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_LifeSelf).Location = new Point(504, 101);
		((Control)gameRegion_LifeSelf).Margin = new Padding(10, 1, 3, 1);
		((Control)gameRegion_LifeSelf).Name = "gameRegion_LifeSelf";
		((Control)gameRegion_LifeSelf).Size = new Size(110, 16);
		((Control)gameRegion_LifeSelf).TabIndex = 0;
		((Control)GameRegion_Main).Location = new Point(36, 16);
		((Control)GameRegion_Main).MaximumSize = new Size(384, 448);
		((Control)GameRegion_Main).MinimumSize = new Size(384, 448);
		((Control)GameRegion_Main).Name = "GameRegion_Main";
		((Control)GameRegion_Main).Size = new Size(384, 448);
		((Control)GameRegion_Main).TabIndex = 1;
		((Control)gameRegion_BluePoint).BackColor = Color.Transparent;
		((Control)gameRegion_BluePoint).BackgroundImage = (Image)(object)Resources.FiveStar;
		((Control)gameRegion_BluePoint).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_BluePoint).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_BluePoint).Location = new Point(442, 214);
		((Control)gameRegion_BluePoint).Name = "gameRegion_BluePoint";
		((Control)gameRegion_BluePoint).Size = new Size(163, 25);
		((Control)gameRegion_BluePoint).TabIndex = 9;
		((Control)gameRegion_GreenPoint).BackColor = Color.Transparent;
		((Control)gameRegion_GreenPoint).BackgroundImage = (Image)(object)Resources.FiveStar;
		((Control)gameRegion_GreenPoint).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_GreenPoint).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_GreenPoint).Location = new Point(442, 240);
		((Control)gameRegion_GreenPoint).Name = "gameRegion_GreenPoint";
		((Control)gameRegion_GreenPoint).Size = new Size(163, 25);
		((Control)gameRegion_GreenPoint).TabIndex = 10;
		((Control)gameRegion_Graze).BackColor = Color.Transparent;
		((Control)gameRegion_Graze).BackgroundImage = (Image)(object)Resources.FiveStar;
		((Control)gameRegion_Graze).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_Graze).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_Graze).Location = new Point(442, 266);
		((Control)gameRegion_Graze).Name = "gameRegion_Graze";
		((Control)gameRegion_Graze).Size = new Size(163, 25);
		((Control)gameRegion_Graze).TabIndex = 11;
		((Control)gameRegion_HiScoreValue).BackColor = Color.Transparent;
		((Control)gameRegion_HiScoreValue).BackgroundImageLayout = (ImageLayout)4;
		((UserControl)gameRegion_HiScoreValue).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_HiScoreValue).Location = new Point(488, 47);
		((Control)gameRegion_HiScoreValue).Margin = new Padding(10, 1, 3, 1);
		((Control)gameRegion_HiScoreValue).Name = "gameRegion_HiScoreValue";
		((Control)gameRegion_HiScoreValue).Size = new Size(110, 16);
		((Control)gameRegion_HiScoreValue).TabIndex = 12;
		((Control)gameRegion1).BackColor = Color.Transparent;
		((Control)gameRegion1).BackgroundImageLayout = (ImageLayout)4;
		((Control)gameRegion1).Location = new Point(451, 48);
		((Control)gameRegion1).Margin = new Padding(10, 3, 3, 3);
		((Control)gameRegion1).Name = "gameRegion1";
		((Control)gameRegion1).Size = new Size(165, 20);
		((Control)gameRegion1).TabIndex = 12;
		((Control)gameRegion_HiScore).BackColor = Color.Transparent;
		((Control)gameRegion_HiScore).BackgroundImage = (Image)(object)Resources.Score;
		((Control)gameRegion_HiScore).BackgroundImageLayout = (ImageLayout)0;
		((UserControl)gameRegion_HiScore).BorderStyle = (BorderStyle)2;
		((Control)gameRegion_HiScore).Location = new Point(428, 44);
		((Control)gameRegion_HiScore).Name = "gameRegion_HiScore";
		((Control)gameRegion_HiScore).Size = new Size(70, 16);
		((Control)gameRegion_HiScore).TabIndex = 13;
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)0;
		((Control)this).BackgroundImage = (Image)(object)Resources.background;
		((Control)this).Controls.Add((Control)(object)gameRegion_Score);
		((Control)this).Controls.Add((Control)(object)gameRegion_HiScore);
		((Control)this).Controls.Add((Control)(object)gameRegion_HiScoreValue);
		((Control)this).Controls.Add((Control)(object)gameRegion_Graze);
		((Control)this).Controls.Add((Control)(object)gameRegion_GreenPoint);
		((Control)this).Controls.Add((Control)(object)gameRegion_BluePoint);
		((Control)this).Controls.Add((Control)(object)gameRegion_ScoreValue);
		((Control)this).Controls.Add((Control)(object)gameRegion_SpellLine);
		((Control)this).Controls.Add((Control)(object)gameRegion_Spell);
		((Control)this).Controls.Add((Control)(object)gameRegion_Player);
		((Control)this).Controls.Add((Control)(object)gameRegion_LifeSelf);
		((Control)this).Controls.Add((Control)(object)GameRegion_Main);
		((Control)this).Controls.Add((Control)(object)gameRegion_Power);
		((Control)this).Name = "BaseStage";
		((Control)this).Size = new Size(640, 480);
		((Control)this).ResumeLayout(false);
	}

	public BaseStage(GlobalDataPackage GlobalData)
		: base(GlobalData)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		InitializeComponent();
		Font font = new Font("Arial", 14f, (FontStyle)1);
		base.DXFont = new Font(base.DeviceMain, font);
	}

	public override void Init()
	{
		base.BoundRect = new Rectangle(((Control)GameRegion_Main).Location, ((Control)GameRegion_Main).Size);
		Point originalPosition = new Point(base.BoundRect.Width / 2, base.BoundRect.Bottom - 50);
		if (base.MyPlane == null)
		{
			base.MyPlane = new MyPlane_Aya(base.StageData, originalPosition);
		}
		base.Boss = null;
		base.BulletList = new List<BaseBullet_Touhou>();
		base.EnemyPlaneList = new List<BaseEnemyPlane>();
		base.MyBulletList = new List<BaseObject>();
		base.SpellList = new List<BaseObject>();
		base.EffectList = new List<BaseEffect>();
		base.ItemList = new List<BaseItem>();
		base.InterfaceList = new List<BaseObject>
		{
			new StarPointLineG(base.StageData, new PointF(66f, 404f)),
			new StarPointLineB(base.StageData, new PointF(-6f, 404f)),
			new StarPointLineR(base.StageData, new PointF(-6f, 404f)),
			new BaseInterface(base.StageData, "TotalBackgroundHalf", new Point(0, 0))
			{
				Delay = 0
			},
			new BossLife_Line(base.StageData, "表示符卡数量的星星（单个）", new PointF(base.BoundRect.Left + 65, base.BoundRect.Top + 22)),
			new HP_Line(base.StageData, "HP_Line1", new PointF(base.BoundRect.Left + base.BoundRect.Width / 2 + 18, base.BoundRect.Top + 13)),
			new SpellClock(base.StageData),
			new MyLife_Line(base.StageData, base.MyPlane.TxtureObject, ((Control)gameRegion_LifeSelf).Location)
			{
				Delay = 30
			},
			new Item_Line_LifeChip(base.StageData, new Point(((Control)gameRegion_LifeSelf).Location.X + 82, ((Control)gameRegion_LifeSelf).Location.Y + 11))
			{
				Delay = 35
			},
			new MySpell_Line(base.StageData, "Bomb", ((Control)gameRegion_SpellLine).Location)
			{
				Delay = 40
			},
			new Item_Line_SpellChip(base.StageData, new Point(((Control)gameRegion_SpellLine).Location.X + 82, ((Control)gameRegion_SpellLine).Location.Y + 11))
			{
				Delay = 45
			},
			new BaseInterface(base.StageData, "最高得分", ((Control)gameRegion_HiScore).Location)
			{
				Delay = 10
			},
			new BaseInterface(base.StageData, "InterfaceLine00", new Point(((Control)gameRegion_HiScore).Location.X + 60, ((Control)gameRegion_HiScore).Location.Y + 16))
			{
				Delay = 15
			},
			new BaseInterface(base.StageData, "得分", ((Control)gameRegion_Score).Location)
			{
				Delay = 20
			},
			new BaseInterface(base.StageData, "InterfaceLine01", new Point(((Control)gameRegion_Score).Location.X + 60, ((Control)gameRegion_Score).Location.Y + 16))
			{
				Delay = 25
			},
			new BaseInterface(base.StageData, "剩余人数", ((Control)gameRegion_Player).Location)
			{
				Delay = 30
			},
			new BaseInterface(base.StageData, "InterfaceLine02", new Point(((Control)gameRegion_Player).Location.X + 60, ((Control)gameRegion_Player).Location.Y + 22))
			{
				Delay = 35
			},
			new BaseInterface(base.StageData, "SC", ((Control)gameRegion_Spell).Location)
			{
				Delay = 40
			},
			new BaseInterface(base.StageData, "InterfaceLine03", new Point(((Control)gameRegion_Spell).Location.X + 60, ((Control)gameRegion_Spell).Location.Y + 22))
			{
				Delay = 45
			},
			new BaseInterface(base.StageData, "灵力", ((Control)gameRegion_Power).Location)
			{
				Delay = 70
			},
			new BaseInterface(base.StageData, "InterfaceLine04", new Point(((Control)gameRegion_Spell).Location.X + 60, ((Control)gameRegion_Power).Location.Y + 16))
			{
				Delay = 75
			},
			new BaseInterface(base.StageData, "最大得点", ((Control)gameRegion_BluePoint).Location)
			{
				Delay = 80
			},
			new BaseInterface(base.StageData, "InterfaceLine05", new Point(((Control)gameRegion_Spell).Location.X + 60, ((Control)gameRegion_BluePoint).Location.Y + 16))
			{
				Delay = 85
			},
			new BaseInterface(base.StageData, "擦弹", new Point(((Control)gameRegion_Graze).Location.X, ((Control)gameRegion_GreenPoint).Location.Y))
			{
				Delay = 90
			},
			new Item_Line_HiScore(base.StageData, new Point(((Control)gameRegion_HiScoreValue).Location.X - 20, ((Control)gameRegion_HiScoreValue).Location.Y))
			{
				Delay = 10
			},
			new Item_Line_Score(base.StageData, new Point(((Control)gameRegion_ScoreValue).Location.X - 20, ((Control)gameRegion_ScoreValue).Location.Y))
			{
				Delay = 20
			},
			new Item_Line_Red(base.StageData, new Point(((Control)gameRegion_Power).Location.X + 50, ((Control)gameRegion_Power).Location.Y))
			{
				Delay = 70
			},
			new Item_Line_Blue(base.StageData, new Point(((Control)gameRegion_BluePoint).Location.X + 50, ((Control)gameRegion_BluePoint).Location.Y))
			{
				Delay = 80
			},
			new Item_Line_Gray(base.StageData, new Point(((Control)gameRegion_Graze).Location.X + 50, ((Control)gameRegion_GreenPoint).Location.Y))
			{
				Delay = 90
			},
			new BossMark(base.StageData),
			new InterfaceDifficulty(base.StageData, new PointF((base.BoundRect.Right + 640) / 2, 24f))
			{
				Delay = 110
			}
		};
		if (base.StageData.Difficulty > DifficultLevel.Normal)
		{
			base.InterfaceList.Add(new BaseInterface(base.StageData, "BD", new Point(((Control)gameRegion_Spell).Location.X, ((Control)gameRegion_Spell).Location.Y + 26))
			{
				Delay = 50
			});
			base.InterfaceList.Add(new MyBorder_Line(base.StageData, "Border", new Point(((Control)gameRegion_SpellLine).Location.X, ((Control)gameRegion_SpellLine).Location.Y + 26))
			{
				Delay = 50
			});
		}
		base.SoundPlayList = new List<XAudio2_Player>();
		base.Background = new BackgroundManager();
		base.Background3D = new BackgroundManager3D(base.StageData);
		base.Particle3D = new ParticleManager3D(base.StageData);
		base.TimeMain = 0;
		base.StageData.Ran = new MyRandom(0);
		base.StageData.C_History = base.StageData.PData.C_History.Find(base.StageData.FindClear);
		base.StageData.S_History = base.StageData.PData.S_History.FindAll(base.StageData.FindScore);
		if (!base.StageData.OnReplay)
		{
			base.StageData.C_History.StartTimes++;
		}
		base.MyPlane.HiScore = base.StageData.S_History[0].Score;
	}

	public override void UpdateData()
	{
		if (base.StageData.OnReplay && base.KClass.Key_minus)
		{
			Slowflag++;
			if (Slowflag < 4)
			{
				return;
			}
			Slowflag = 0;
		}
		base.UpdateData();
		if (base.StageData.SlowMode > 0)
		{
			base.StageData.SlowMode--;
			if (base.TimeMain % 2 == 0)
			{
				return;
			}
		}
		if (base.KClass.Key_ESC)
		{
			base.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = "PauseMenu",
				NeedInit = true,
				SDPswitch = new StageDataPackage(base.GlobalData)
			};
			base.GlobalData.LastState = this;
			base.StageData.SoundPlay("se_pause.wav");
		}
		if (base.StageData.Rep != null)
		{
			if (base.StageData.OnReplay)
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
			else
			{
				base.StageData.Rep.WriteKey(base.StageData.KClass.Key2Hex());
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
		for (int num2 = base.MyBulletList.Count - 1; num2 >= 0; num2--)
		{
			base.MyBulletList[num2].Ctrl();
		}
		base.MyPlane.Ctrl();
		for (int num2 = base.SpellList.Count - 1; num2 >= 0; num2--)
		{
			base.SpellList[num2].Ctrl();
		}
		for (int num2 = base.EnemyPlaneList.Count - 1; num2 >= 0; num2--)
		{
			base.EnemyPlaneList[num2].Ctrl();
		}
		for (int num2 = base.BulletList.Count - 1; num2 >= 0; num2--)
		{
			base.BulletList[num2].Ctrl();
		}
		if (base.Boss != null)
		{
			base.Boss.Ctrl();
		}
		for (int num2 = base.EffectList.Count - 1; num2 >= 0; num2--)
		{
			base.EffectList[num2].Ctrl();
		}
		base.Particle3D.Ctrl();
		for (int num2 = base.ItemList.Count - 1; num2 >= 0; num2--)
		{
			base.ItemList[num2].Ctrl();
		}
		base.StageData.Vibrate();
		HitCheckAllinOne();
		for (int num2 = base.InterfaceList.Count - 1; num2 >= 0; num2--)
		{
			base.InterfaceList[num2].Ctrl();
		}
		base.SoundPlayList.ForEach(delegate(XAudio2_Player x)
		{
			x.Play();
		});
		if (base.StageData.OnReplay && base.TimeMain % 6 != 0 && base.KClass.Key_plus && base.MyPlane.Life >= 0 && base.StateSwitchData == null)
		{
			UpdateData();
		}
		base.GlobalData.LastState = this;
	}

	public virtual void HitCheckAllinOne()
	{
		if (base.MyPlane.Life < 0)
		{
			base.StateSwitchData = new StateSwitchDataPackage
			{
				NextState = "GameOverMenu",
				NeedInit = true,
				SDPswitch = new StageDataPackage(base.GlobalData)
			};
		}
		base.ItemList.ForEach(delegate(BaseItem x)
		{
			x.HitCheckAll();
		});
		if (base.Story == null)
		{
			base.EnemyPlaneList.ForEach(delegate(BaseEnemyPlane x)
			{
				x.HitCheckAll();
			});
			base.BulletList.ForEach(delegate(BaseBullet_Touhou x)
			{
				x.HitCheckAll();
			});
			if (base.Boss != null)
			{
				base.Boss.HitCheckAll();
			}
			base.SpellList.ForEach(delegate(BaseObject x)
			{
				x.HitCheckAll();
			});
		}
	}

	public virtual void ShowInterface()
	{
		if (base.MyPlane != null)
		{
			base.MyPlane.ShowCenter();
		}
		base.SpriteMain.Draw2D(base.TextureObjectDictionary["TotalBackground"], 1f, 0f, new PointF(320f, 240f), 255);
		DrawText(base.FPS.ToString("F1"), new Point(580, 460));
	}

	public virtual void Drama()
	{
		if (!base.StageData.OnReplay)
		{
			base.StageData.C_History.PlayingTime++;
		}
		if (base.TimeMain == 1)
		{
			if (base.StageData.StateSwitchData == null)
			{
				base.StateSwitchData = new StateSwitchDataPackage
				{
					NextState = "MusicLoading",
					NeedInit = true,
					SDPswitch = new StageDataPackage(base.GlobalData)
				};
			}
			base.GlobalData.LastState = this;
			base.StageData.CurrentStageName = base.StageName;
			RoadFlag = false;
			base.EffectList.Clear();
			base.MyPlane.Refresh();
			base.Ran = new MyRandom(0);
		}
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
		TextureObject txtureObject = base.TextureObjectDictionary["White"];
		switch (base.MyPlane.EnchantmentState)
		{
		case EnchantmentType.Red:
			base.SpriteMain.Draw2D(txtureObject, 100f, 0f, new PointF((base.BoundRect.Left + base.BoundRect.Right) / 2, (base.BoundRect.Top + base.BoundRect.Height) / 2), Color.FromArgb(180, 30, 0, 0));
			break;
		case EnchantmentType.Blue:
			base.SpriteMain.Draw2D(txtureObject, 100f, 0f, new PointF((base.BoundRect.Left + base.BoundRect.Right) / 2, (base.BoundRect.Top + base.BoundRect.Height) / 2), Color.FromArgb(180, 0, 0, 30));
			break;
		case EnchantmentType.Green:
			base.SpriteMain.Draw2D(txtureObject, 100f, 0f, new PointF((base.BoundRect.Left + base.BoundRect.Right) / 2, (base.BoundRect.Top + base.BoundRect.Height) / 2), Color.FromArgb(180, 0, 30, 0));
			break;
		}
		base.Background2.Show(Active: false);
		if (base.Boss != null)
		{
			base.Boss.Show();
		}
		base.MyBulletList.ForEach(delegate(BaseObject x)
		{
			x.Show();
		});
		base.MyPlane.Show();
		base.EnemyPlaneList.ForEach(delegate(BaseEnemyPlane x)
		{
			x.Show();
		});
		base.ItemList.ForEach(delegate(BaseItem x)
		{
			if (!x.Active)
			{
				x.Show();
			}
		});
		base.SpriteMain.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.DeviceMain.SetRenderState(RenderState.SourceBlend, 5);
		base.DeviceMain.SetRenderState(RenderState.DestinationBlend, 2);
		base.BulletList.ForEach(delegate(BaseBullet_Touhou x)
		{
			if (x.Active)
			{
				x.Show();
			}
		});
		base.EffectList.ForEach(delegate(BaseEffect x)
		{
			if (x.Active)
			{
				x.Show();
			}
		});
		base.SpellList.ForEach(delegate(BaseObject x)
		{
			x.Show();
		});
		base.ItemList.ForEach(delegate(BaseItem x)
		{
			if (x.Active)
			{
				x.Show();
			}
		});
		base.SpriteMain.End();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		base.BulletList.ForEach(delegate(BaseBullet_Touhou x)
		{
			if (!x.Active && x.Layer < 0)
			{
				x.Show();
			}
		});
		base.BulletList.ForEach(delegate(BaseBullet_Touhou x)
		{
			if (!x.Active && x.Layer == 0)
			{
				x.Show();
			}
		});
		base.BulletList.ForEach(delegate(BaseBullet_Touhou x)
		{
			if (!x.Active && x.Layer > 0)
			{
				x.Show();
			}
		});
		base.EffectList.ForEach(delegate(BaseEffect x)
		{
			if (!x.Active && x.Layer < 0)
			{
				x.Show();
			}
		});
		base.EffectList.ForEach(delegate(BaseEffect x)
		{
			if (!x.Active && x.Layer == 0)
			{
				x.Show();
			}
		});
		base.EffectList.ForEach(delegate(BaseEffect x)
		{
			if (!x.Active && x.Layer > 0)
			{
				x.Show();
			}
		});
		base.SpriteMain.End();
		base.Particle3D.Show();
		base.SpriteMain.Begin(SpriteFlags.AlphaBlend);
		if (base.Story != null)
		{
			base.Story.Show();
		}
		ShowInterface();
		base.InterfaceList.ForEach(delegate(BaseObject x)
		{
			x.Show();
		});
		base.SpriteMain.End();
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
