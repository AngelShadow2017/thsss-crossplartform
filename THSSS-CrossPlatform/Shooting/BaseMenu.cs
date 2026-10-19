using System.Windows.Forms;

namespace Shooting;

internal class BaseMenu : BaseGameState, IGameState
{
	public int BackgroundLight { get; set; }

	public BaseMenu(GlobalDataPackage GlobalData)
	{
		((Control)this).Visible = false;
		((Control)this).Enabled = false;
		base.GlobalData = GlobalData;
	}

	public BaseMenu()
	{
	}

	public override void Init()
	{
		MenuInit();
	}

	public virtual void MenuInit()
	{
	}

	public override void UpdateData()
	{
		base.UpdateData();
		int count = base.StageData.MenuGroupList.Count;
		if (count > 0)
		{
			base.StageData.MenuGroupList[count - 1].MenuSelect();
		}
		foreach (BaseMenuGroup x in SlimDX.Compat.ListCompat.Snapshot(base.StageData.MenuGroupList)){
			x.Ctrl();
		};
		base.Background3D.Ctrl();
		base.Background.Ctrl();
		base.Background2.Ctrl();
		for (int num = base.EffectList.Count - 1; num >= 0; num--)
		{
			base.EffectList[num].Ctrl();
		}
		base.Particle3D.Ctrl();
		foreach (XAudio2_Player x in SlimDX.Compat.ListCompat.Snapshot(base.SoundPlayList)){
			x.Play();
		};
	}

	public virtual void BackgroundDrama()
	{
	}

	private void InitializeComponent()
	{
		((Control)this).SuspendLayout();
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)0;
		((Control)this).Name = "BaseMenu";
		((Control)this).ResumeLayout(false);
	}
}
