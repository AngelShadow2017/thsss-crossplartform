using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SlimDX.Windows;

namespace Shooting;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		AppDomain.CurrentDomain.SetData("Encoding_Default", Encoding.GetEncoding("GBK"));
		new Mutex(initiallyOwned: false, "AA", out var createdNew);
		if (createdNew)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Game_Main game_Main = new Game_Main();
			if (game_Main.initSuccess)
			{
				MessagePump.Run((Form)(object)game_Main.Form_Main, game_Main.MainProcess);
			}
		}
		else
		{
			MessageBox.Show("东方白丝祭(误)已启动，\r\n程序不允许双开。", "程序已启动");
		}
	}
}
