using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SlimDX.Windows;

namespace Shooting;

public class RenderForm_Main : RenderForm
{
	private IContainer components = null;

	protected internal override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		((Form)this).Dispose(disposing);
	}

	private void InitializeComponent()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(RenderForm_Main));
		((Control)this).SuspendLayout();
		((ContainerControl)this).AutoScaleMode = (AutoScaleMode)0;
		((Form)this).ClientSize = new Size(640, 480);
		((Form)this).FormBorderStyle = (FormBorderStyle)3;
		((Form)this).Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
		((Form)this).MaximizeBox = false;
		((Control)this).Name = "RenderForm_Main";
		((Form)this).StartPosition = (FormStartPosition)1;
		((Control)this).Text = "东方夏夜祭 ～ Shining Shooting Star. ver 1.00";
		((Control)this).ResumeLayout(false);
	}

	public RenderForm_Main()
	{
		InitializeComponent();
	}
}
