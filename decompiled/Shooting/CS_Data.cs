using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Shooting;

public class CS_Data
{
	private string[] MBGtext;

	public List<BaseEmitter_CS> EmitterList { get; private set; }

	public int TimeTotal { get; private set; }

	public CS_Data()
	{
	}

	public CS_Data(string FileName)
	{
		MBGtext = File.ReadAllLines(FileName, Encoding.UTF8);
		EmitterList = new List<BaseEmitter_CS>();
	}

	public void String2Data(StageDataPackage StageData)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		EmitterList.Clear();
		int num = 0;
		if (MBGtext[num++] != "Crazy Storm Data 1.01")
		{
			MessageBox.Show("当前暂不支持除1.01以外的数据格式", "错误", (MessageBoxButtons)0, (MessageBoxIcon)16);
			return;
		}
		string text = MBGtext[num++];
		if (text.Contains("Types"))
		{
			int num2 = int.Parse(text.Split(new char[1] { ' ' })[0]);
			for (int i = 0; i < num2; i++)
			{
				string text2 = MBGtext[num++];
				string[] array = text2.Split(new char[1] { '_' });
			}
			text = MBGtext[num++];
		}
		if (text.Contains("GlobalEvents"))
		{
			int num3 = int.Parse(text.Split(new char[1] { ' ' })[0]);
			for (int j = 0; j < num3; j++)
			{
				string text3 = MBGtext[num++];
			}
			text = MBGtext[num++];
		}
		if (text.Contains("Sounds"))
		{
			int num4 = int.Parse(text.Split(new char[1] { ' ' })[0]);
			for (int k = 0; k < num4; k++)
			{
				string text4 = MBGtext[num++];
			}
			text = MBGtext[num++];
		}
		text = MBGtext[num++];
		TimeTotal = int.Parse(text.Split(new char[1] { ':' })[1]);
		for (int l = 0; l < 4; l++)
		{
			text = MBGtext[num++];
			if (!(text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' })[0] != "empty"))
			{
				continue;
			}
			int num5 = int.Parse(text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' })[3]);
			for (int m = 0; m < num5; m++)
			{
				string text5 = MBGtext[num++];
				string[] array2 = text5.Split(new char[1] { ',' });
				EmitterMode emitterMode = EmitterMode.Bullet;
				if (array2.Length > 91)
				{
					if (bool.Parse(array2[81]))
					{
						emitterMode = EmitterMode.Enemy;
					}
					else if (int.Parse(array2[91]) > 0)
					{
						emitterMode = EmitterMode.Effect;
					}
				}
				BaseEmitter_CS baseEmitter_CS = new BaseEmitter_CS(StageData, this, emitterMode);
				if (baseEmitter_CS.EmitterMode == EmitterMode.Enemy)
				{
					EnemyFactory enemyFactory = new EnemyFactory(array2[82]);
					enemyFactory.HealthPoint = int.Parse(array2[83]);
					enemyFactory.RedCount = int.Parse(array2[84]);
					enemyFactory.BlueCount = int.Parse(array2[85]);
					enemyFactory.ColorType = byte.Parse(array2[86]);
					enemyFactory.BackFire = bool.Parse(array2[87]);
					enemyFactory.ClearRadius = int.Parse(array2[88]);
					enemyFactory.GreenCount = int.Parse(array2[89]);
					enemyFactory.StarFall = array2[90] != "0";
					baseEmitter_CS.SubBullet = enemyFactory.GenerateEnemy(StageData);
				}
				baseEmitter_CS.ID = int.Parse(array2[0]);
				baseEmitter_CS.LayerID = int.Parse(array2[1]);
				baseEmitter_CS.BindingState = bool.Parse(array2[2]);
				baseEmitter_CS.BindingID = int.Parse(array2[3]);
				baseEmitter_CS.BindWithDirection = bool.Parse(array2[4]);
				baseEmitter_CS.CS_Position = new PointF(float.Parse(array2[6]), float.Parse(array2[7]));
				baseEmitter_CS.StartTime = int.Parse(array2[8]);
				baseEmitter_CS.Duration = int.Parse(array2[9]);
				baseEmitter_CS.EmitPoint = new PointF(float.Parse(array2[10]), float.Parse(array2[11]));
				baseEmitter_CS.EmitRadius = int.Parse(array2[12]);
				baseEmitter_CS.RadiusDirection = float.Parse(array2[13]);
				string text6 = array2[14].Replace("{", "").Replace("}", "");
				baseEmitter_CS.RadiusDirectionPoint = new PointF(float.Parse(text6.Split(new char[1] { ' ' })[0].Split(new char[1] { ':' })[1]), float.Parse(text6.Split(new char[1] { ' ' })[1].Split(new char[1] { ':' })[1]));
				baseEmitter_CS.Way = int.Parse(array2[15]);
				baseEmitter_CS.Circle = int.Parse(array2[16]);
				baseEmitter_CS.EmitDirection = float.Parse(array2[17]);
				text6 = array2[18].Replace("{", "").Replace("}", "");
				baseEmitter_CS.EmitDirectionPoint = new PointF(float.Parse(text6.Split(new char[1] { ' ' })[0].Split(new char[1] { ':' })[1]), float.Parse(text6.Split(new char[1] { ' ' })[1].Split(new char[1] { ':' })[1]));
				baseEmitter_CS.Range = int.Parse(array2[19]);
				baseEmitter_CS.Velocity = float.Parse(array2[20]);
				baseEmitter_CS.DirectionDegree = float.Parse(array2[21]);
				text6 = array2[22].Replace("{", "").Replace("}", "");
				baseEmitter_CS.DestPoint = new PointF(float.Parse(text6.Split(new char[1] { ' ' })[0].Split(new char[1] { ':' })[1]), float.Parse(text6.Split(new char[1] { ' ' })[1].Split(new char[1] { ':' })[1]));
				baseEmitter_CS.AccelerateCS = float.Parse(array2[23]);
				baseEmitter_CS.AccDirection = float.Parse(array2[24]);
				baseEmitter_CS.SubBullet.LifeTime = int.Parse(array2[26]);
				baseEmitter_CS.SubBullet.Type = int.Parse(array2[27]);
				baseEmitter_CS.SubBullet.ScaleWidth = float.Parse(array2[28]);
				baseEmitter_CS.SubBullet.ScaleLength = float.Parse(array2[29]);
				baseEmitter_CS.SubBullet.ColorValue = Color.FromArgb(int.Parse(array2[30]), int.Parse(array2[31]), int.Parse(array2[32]));
				baseEmitter_CS.SubBullet.TransparentValueF = int.Parse(array2[33]) * 255 / 100;
				baseEmitter_CS.SubBullet.AngleDegree = float.Parse(array2[34]) + 180f;
				baseEmitter_CS.SubBullet.AngleWithDirection = bool.Parse(array2[36]);
				baseEmitter_CS.SubBullet.Velocity = float.Parse(array2[37]);
				baseEmitter_CS.SubBullet.DirectionDegree = float.Parse(array2[38]);
				baseEmitter_CS.SubBullet.DestPoint = new PointF(float.Parse(text6.Split(new char[1] { ' ' })[0].Split(new char[1] { ':' })[1]), float.Parse(text6.Split(new char[1] { ' ' })[1].Split(new char[1] { ':' })[1]));
				baseEmitter_CS.SubBullet.AccelerateCS = float.Parse(array2[40]);
				baseEmitter_CS.SubBullet.AccDirection = float.Parse(array2[41]);
				baseEmitter_CS.SubBullet.ScaleX = float.Parse(array2[43]);
				baseEmitter_CS.SubBullet.ScaleY = float.Parse(array2[44]);
				baseEmitter_CS.SubBullet.BeginningEffect = bool.Parse(array2[45]);
				baseEmitter_CS.SubBullet.EndingEffect = bool.Parse(array2[46]);
				baseEmitter_CS.SubBullet.Active = bool.Parse(array2[47]);
				baseEmitter_CS.SubBullet.Ghosting = bool.Parse(array2[48]);
				baseEmitter_CS.SubBullet.OutBound = bool.Parse(array2[49]);
				baseEmitter_CS.SubBullet.UnRemoveable = bool.Parse(array2[50]);
				text6 = array2[51];
				for (int num6 = 0; num6 < text6.Split(new char[1] { '&' }).Length - 1; num6++)
				{
					string text7 = text6.Split(new char[1] { '&' })[num6];
					EventGroup eventGroup = new EventGroup();
					eventGroup.index = num6;
					eventGroup.tag = text7.Split(new char[1] { '|' })[0];
					eventGroup.t = int.Parse(text7.Split(new char[1] { '|' })[1]);
					eventGroup.addtime = int.Parse(text7.Split(new char[1] { '|' })[2]);
					for (int num7 = 0; num7 < text7.Split(new char[1] { '|' })[3].Split(new char[1] { ';' }).Length - 1; num7++)
					{
						Event obj = new Event();
						obj.EventString = text7.Split(new char[1] { '|' })[3].Split(new char[1] { ';' })[num7];
						obj.String2EmitterEvent();
						eventGroup.EventList.Add(obj);
					}
					baseEmitter_CS.EventGroupList.Add(eventGroup);
				}
				text6 = array2[52];
				for (int num8 = 0; num8 < text6.Split(new char[1] { '&' }).Length - 1; num8++)
				{
					string text8 = text6.Split(new char[1] { '&' })[num8];
					EventGroup eventGroup2 = new EventGroup();
					eventGroup2.index = num8;
					eventGroup2.tag = text8.Split(new char[1] { '|' })[0];
					eventGroup2.t = int.Parse(text8.Split(new char[1] { '|' })[1]);
					eventGroup2.addtime = int.Parse(text8.Split(new char[1] { '|' })[2]);
					for (int num9 = 0; num9 < text8.Split(new char[1] { '|' })[3].Split(new char[1] { ';' }).Length - 1; num9++)
					{
						Event obj = new Event();
						obj.EventString = text8.Split(new char[1] { '|' })[3].Split(new char[1] { ';' })[num9];
						obj.String2BulletEvent();
						eventGroup2.EventList.Add(obj);
					}
					baseEmitter_CS.SubBullet.EventGroupList.Add(eventGroup2);
				}
				baseEmitter_CS.RanX = float.Parse(array2[53]);
				baseEmitter_CS.RanY = float.Parse(array2[54]);
				baseEmitter_CS.RanRadius = int.Parse(array2[55]);
				baseEmitter_CS.RanRadiusDirection = float.Parse(array2[56]);
				baseEmitter_CS.RanWay = int.Parse(array2[57]);
				baseEmitter_CS.RanCircle = int.Parse(array2[58]);
				baseEmitter_CS.RanEmitDirection = float.Parse(array2[59]);
				baseEmitter_CS.RanRange = int.Parse(array2[60]);
				baseEmitter_CS.RanVelocity = float.Parse(array2[61]);
				baseEmitter_CS.RanDirection = float.Parse(array2[62]);
				baseEmitter_CS.RanAccelerate = float.Parse(array2[63]);
				baseEmitter_CS.RanAccDirection = float.Parse(array2[64]);
				baseEmitter_CS.SubBullet.RanAngle = float.Parse(array2[65]);
				baseEmitter_CS.SubBullet.RanVelocity = float.Parse(array2[66]);
				baseEmitter_CS.SubBullet.RanDirection = float.Parse(array2[67]);
				baseEmitter_CS.SubBullet.RanAccelerate = float.Parse(array2[68]);
				baseEmitter_CS.SubBullet.RanAccDirection = float.Parse(array2[69]);
				if (array2.Length > 72)
				{
					baseEmitter_CS.SubBullet.Cover = bool.Parse(array2[70]);
					baseEmitter_CS.SubBullet.Rebound = bool.Parse(array2[71]);
					baseEmitter_CS.SubBullet.Force = bool.Parse(array2[72]);
				}
				if (array2.Length > 73)
				{
					baseEmitter_CS.DeepBinding = bool.Parse(array2[73]);
				}
				if (array2.Length > 79)
				{
					baseEmitter_CS.MotionBinding = bool.Parse(array2[74]);
					if (baseEmitter_CS.SubBullet is BaseBullet_Touhou)
					{
						((BaseBullet_Touhou)baseEmitter_CS.SubBullet).Reflect = byte.Parse(array2[75]);
						((BaseBullet_Touhou)baseEmitter_CS.SubBullet).Layer = int.Parse(array2[78]);
					}
					baseEmitter_CS.SoundName = array2[76];
					baseEmitter_CS.SpecifySE = bool.Parse(array2[77]);
					if (int.Parse(array2[79]) != 0)
					{
						baseEmitter_CS.SubBullet.Region = int.Parse(array2[79]);
						((BaseBullet_Touhou)baseEmitter_CS.SubBullet).SizeValue = baseEmitter_CS.SubBullet.Region * 2;
					}
				}
				if (array2.Length > 95)
				{
					baseEmitter_CS.EffectType = int.Parse(array2[91]);
					if (array2[92] != null && StageData.TextureObjectDictionary.ContainsKey(array2[92]))
					{
						baseEmitter_CS.SubBullet.TxtureObject = StageData.TextureObjectDictionary[array2[92]];
					}
					baseEmitter_CS.Count = int.Parse(array2[94]);
					baseEmitter_CS.Count = ((baseEmitter_CS.Count < 1) ? 1 : baseEmitter_CS.Count);
					baseEmitter_CS.DeltaV = float.Parse(array2[95]);
				}
				if (array2.Length > 110)
				{
					baseEmitter_CS.RDirectionWithDirection = !(array2[96] == "0");
				}
				EmitterList.Add(baseEmitter_CS);
			}
			if (text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' }).Length < 7)
			{
				continue;
			}
			num5 = int.Parse(text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' })[4]);
			for (int n = 0; n < num5; n++)
			{
				string text5 = MBGtext[num++];
				string[] array2 = text5.Split(new char[1] { ',' });
				BaseEmitter_CS baseEmitter_CS2 = (bool.Parse(array2[29]) ? new BaseEmitter_CS(StageData, this, EmitterMode.RadialLaser) : ((array2.Length <= 61) ? new BaseEmitter_CS(StageData, this, EmitterMode.StraightLaser) : ((!bool.Parse(array2[61])) ? new BaseEmitter_CS(StageData, this, EmitterMode.StraightLaser) : new BaseEmitter_CS(StageData, this, EmitterMode.BendLaser))));
				baseEmitter_CS2.ID = int.Parse(array2[0]);
				baseEmitter_CS2.LayerID = int.Parse(array2[1]);
				baseEmitter_CS2.BindingState = bool.Parse(array2[2]);
				baseEmitter_CS2.BindingID = int.Parse(array2[3]);
				baseEmitter_CS2.BindWithDirection = bool.Parse(array2[4]);
				baseEmitter_CS2.CS_Position = new PointF(float.Parse(array2[6]), float.Parse(array2[7]));
				baseEmitter_CS2.StartTime = int.Parse(array2[8]);
				baseEmitter_CS2.Duration = int.Parse(array2[9]);
				baseEmitter_CS2.EmitRadius = int.Parse(array2[10]);
				baseEmitter_CS2.RadiusDirection = float.Parse(array2[11]);
				baseEmitter_CS2.Way = int.Parse(array2[13]);
				baseEmitter_CS2.Circle = int.Parse(array2[14]);
				baseEmitter_CS2.EmitDirection = float.Parse(array2[15]);
				baseEmitter_CS2.Range = int.Parse(array2[17]);
				baseEmitter_CS2.Velocity = float.Parse(array2[18]);
				baseEmitter_CS2.DirectionDegree = float.Parse(array2[19]);
				baseEmitter_CS2.AccelerateCS = float.Parse(array2[21]);
				baseEmitter_CS2.AccDirection = float.Parse(array2[22]);
				baseEmitter_CS2.SubBullet.LifeTime = int.Parse(array2[24]);
				baseEmitter_CS2.SubBullet.Type = int.Parse(array2[25]);
				if (baseEmitter_CS2.EmitterMode == EmitterMode.StraightLaser || baseEmitter_CS2.EmitterMode == EmitterMode.BendLaser)
				{
					baseEmitter_CS2.SubBullet.ScaleLength = float.Parse(array2[26]);
					baseEmitter_CS2.SubBullet.GhostingCount = int.Parse(array2[27]);
				}
				else
				{
					baseEmitter_CS2.SubBullet.MaxScaleW = (float)int.Parse(array2[27]) / 256f;
					baseEmitter_CS2.SubBullet.ScaleWidth = baseEmitter_CS2.SubBullet.MaxScaleW;
					baseEmitter_CS2.SubBullet.MaxScaleL = 1f;
					baseEmitter_CS2.SubBullet.ScaleLength = 0.2f;
				}
				baseEmitter_CS2.SubBullet.TransparentValueF = int.Parse(array2[28]) * 255 / 100;
				baseEmitter_CS2.SubBullet.Velocity = float.Parse(array2[30]);
				baseEmitter_CS2.SubBullet.DirectionDegree = float.Parse(array2[31]);
				baseEmitter_CS2.SubBullet.AccelerateCS = float.Parse(array2[33]);
				baseEmitter_CS2.SubBullet.AccDirection = float.Parse(array2[34]);
				baseEmitter_CS2.SubBullet.Active = bool.Parse(array2[38]);
				baseEmitter_CS2.SubBullet.UnRemoveable = bool.Parse(array2[40]);
				string text9 = array2[42];
				for (int num10 = 0; num10 < text9.Split(new char[1] { '&' }).Length - 1; num10++)
				{
					string text10 = text9.Split(new char[1] { '&' })[num10];
					EventGroup eventGroup = new EventGroup();
					eventGroup.index = num10;
					eventGroup.tag = text10.Split(new char[1] { '|' })[0];
					eventGroup.t = int.Parse(text10.Split(new char[1] { '|' })[1]);
					eventGroup.addtime = int.Parse(text10.Split(new char[1] { '|' })[2]);
					for (int num11 = 0; num11 < text10.Split(new char[1] { '|' })[3].Split(new char[1] { ';' }).Length - 1; num11++)
					{
						Event obj = new Event();
						obj.EventString = text10.Split(new char[1] { '|' })[3].Split(new char[1] { ';' })[num11];
						obj.String2EmitterEvent();
						eventGroup.EventList.Add(obj);
					}
					baseEmitter_CS2.EventGroupList.Add(eventGroup);
				}
				text9 = array2[43];
				for (int num12 = 0; num12 < text9.Split(new char[1] { '&' }).Length - 1; num12++)
				{
					string text11 = text9.Split(new char[1] { '&' })[num12];
					EventGroup eventGroup2 = new EventGroup();
					eventGroup2.index = num12;
					eventGroup2.tag = text11.Split(new char[1] { '|' })[0];
					eventGroup2.t = int.Parse(text11.Split(new char[1] { '|' })[1]);
					eventGroup2.addtime = int.Parse(text11.Split(new char[1] { '|' })[2]);
					for (int num13 = 0; num13 < text11.Split(new char[1] { '|' })[3].Split(new char[1] { ';' }).Length - 1; num13++)
					{
						Event obj = new Event();
						obj.EventString = text11.Split(new char[1] { '|' })[3].Split(new char[1] { ';' })[num13];
						obj.String2BulletEvent();
						eventGroup2.EventList.Add(obj);
					}
					baseEmitter_CS2.SubBullet.EventGroupList.Add(eventGroup2);
				}
				baseEmitter_CS2.RanRadius = int.Parse(array2[44]);
				baseEmitter_CS2.RanRadiusDirection = float.Parse(array2[45]);
				baseEmitter_CS2.RanWay = int.Parse(array2[46]);
				baseEmitter_CS2.RanCircle = int.Parse(array2[47]);
				baseEmitter_CS2.RanEmitDirection = float.Parse(array2[48]);
				baseEmitter_CS2.RanRange = int.Parse(array2[49]);
				baseEmitter_CS2.RanVelocity = float.Parse(array2[50]);
				baseEmitter_CS2.RanDirection = float.Parse(array2[51]);
				baseEmitter_CS2.RanAccelerate = float.Parse(array2[52]);
				baseEmitter_CS2.RanAccDirection = float.Parse(array2[53]);
				baseEmitter_CS2.SubBullet.RanVelocity = float.Parse(array2[54]);
				baseEmitter_CS2.SubBullet.RanDirection = float.Parse(array2[55]);
				baseEmitter_CS2.SubBullet.RanAccelerate = float.Parse(array2[56]);
				baseEmitter_CS2.SubBullet.RanAccDirection = float.Parse(array2[57]);
				if (array2.Length > 58)
				{
					baseEmitter_CS2.DeepBinding = bool.Parse(array2[58]);
				}
				if (array2.Length > 62)
				{
					if (baseEmitter_CS2.SubBullet is BaseBullet_Touhou)
					{
						((BaseBullet_Touhou)baseEmitter_CS2.SubBullet).Reflect = byte.Parse(text5.Split(new char[1] { ',' })[59]);
					}
					baseEmitter_CS2.MotionBinding = bool.Parse(text5.Split(new char[1] { ',' })[60]);
					if (baseEmitter_CS2.EmitterMode == EmitterMode.StraightLaser)
					{
						((BaseStraightLaser_CS)baseEmitter_CS2.SubBullet).LaserHead = bool.Parse(text5.Split(new char[1] { ',' })[62]);
					}
				}
				EmitterList.Add(baseEmitter_CS2);
			}
			num5 = int.Parse(text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' })[5]);
			for (int num14 = 0; num14 < num5; num14++)
			{
				string text5 = MBGtext[num++];
				string[] array2 = text5.Split(new char[1] { ',' });
				BaseCover_CS baseCover_CS = new BaseCover_CS(StageData);
				baseCover_CS.MaxScale = 1000f;
				baseCover_CS.ID = int.Parse(array2[0]);
				baseCover_CS.LayerID = int.Parse(array2[1]);
				baseCover_CS.CS_Position = new PointF(float.Parse(array2[2]), float.Parse(array2[3]));
				baseCover_CS.StartTime = int.Parse(array2[4]);
				baseCover_CS.Duration = int.Parse(array2[5]);
				baseCover_CS.ScaleWidth = int.Parse(array2[6]);
				baseCover_CS.ScaleLength = int.Parse(array2[7]);
				baseCover_CS.Roundness = bool.Parse(array2[8]);
				baseCover_CS.Type = int.Parse(array2[9]);
				baseCover_CS.CtrlID = int.Parse(array2[10]);
				baseCover_CS.Velocity = float.Parse(array2[11]);
				baseCover_CS.DirectionDegree = float.Parse(array2[12]);
				baseCover_CS.AccelerateCS = float.Parse(array2[14]);
				baseCover_CS.AccDirection = float.Parse(array2[15]);
				string text12 = array2[17];
				for (int num15 = 0; num15 < text12.Split(new char[1] { '&' }).Length - 1; num15++)
				{
					string text13 = text12.Split(new char[1] { '&' })[num15];
					EventGroup eventGroup = new EventGroup();
					eventGroup.index = num15;
					eventGroup.tag = text13.Split(new char[1] { '|' })[0];
					eventGroup.t = int.Parse(text13.Split(new char[1] { '|' })[1]);
					eventGroup.addtime = int.Parse(text13.Split(new char[1] { '|' })[2]);
					for (int num16 = 0; num16 < text13.Split(new char[1] { '|' })[3].Split(new char[1] { ';' }).Length - 1; num16++)
					{
						Event obj = new Event();
						obj.EventString = text13.Split(new char[1] { '|' })[3].Split(new char[1] { ';' })[num16];
						obj.String2EmitterEvent();
						eventGroup.EventList.Add(obj);
					}
					baseCover_CS.EventGroupList.Add(eventGroup);
				}
				text12 = array2[18];
				for (int num17 = 0; num17 < text12.Split(new char[1] { '&' }).Length - 1; num17++)
				{
					string text14 = text12.Split(new char[1] { '&' })[num17];
					EventGroup eventGroup2 = new EventGroup();
					eventGroup2.index = num17;
					eventGroup2.tag = text14.Split(new char[1] { '|' })[0];
					eventGroup2.t = int.Parse(text14.Split(new char[1] { '|' })[1]);
					eventGroup2.addtime = int.Parse(text14.Split(new char[1] { '|' })[2]);
					for (int num18 = 0; num18 < text14.Split(new char[1] { '|' })[3].Split(new char[1] { ';' }).Length - 1; num18++)
					{
						Event obj = new Event();
						obj.EventString = text14.Split(new char[1] { '|' })[3].Split(new char[1] { ';' })[num18];
						obj.String2BulletEvent();
						eventGroup2.EventList.Add(obj);
					}
					baseCover_CS.SubEventGroupList.Add(eventGroup2);
				}
				baseCover_CS.RanVelocity = float.Parse(array2[19]);
				baseCover_CS.RanDirection = float.Parse(array2[20]);
				baseCover_CS.RanAccelerate = float.Parse(array2[21]);
				baseCover_CS.RanAccDirection = float.Parse(array2[22]);
				if (array2.Length >= 24)
				{
					baseCover_CS.BindingID = int.Parse(array2[23]);
				}
				if (array2.Length >= 25 && array2[24] != "")
				{
					baseCover_CS.DeepBinding = bool.Parse(array2[24]);
				}
				EmitterList.Add(baseCover_CS);
			}
			num5 = int.Parse(text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' })[6]);
			for (int num19 = 0; num19 < num5; num19++)
			{
				string text5 = MBGtext[num++];
			}
			num5 = int.Parse(text.Split(new char[1] { ':' })[1].Split(new char[1] { ',' })[7]);
			for (int num20 = 0; num20 < num5; num20++)
			{
				string text5 = MBGtext[num++];
			}
		}
	}

	private void AddNode(int id, TreeNode treeNode)
	{
		EmitterList.ForEach(delegate(BaseEmitter_CS b)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			if (b.BindingID == id)
			{
				string text = ((!(b is BaseCover_CS)) ? b.EmitterMode.ToString() : "Cover");
				TreeNode val = new TreeNode(b.ID.ToString());
				val.Name = b.ID.ToString();
				val.Text = "ID：" + b.ID + "  " + text;
				treeNode.Nodes.Add(val);
				AddNode(b.ID, val);
			}
		});
	}
}
