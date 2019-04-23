using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Tools
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
		static void Main(String[] args)
		{
			Process[] process = Process.GetProcessesByName("Tools");
			if (process.Length > 1) {
				
				return;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			MainWindow m = new MainWindow();
			if (args.Length > 0)
			{
				if (args[0].EndsWith(".gba") && File.Exists(args[0]))
					m.LoadEveryThing(args[0]);
				if (args.Length > 1)
				{
					m.script_offset.Text = args[1];
					m.button6_Click(null, null);
				}
			}
			else
			{
				String file_name = new PokeConfig("rom.ini").Get("last_open","last_open");
				m.init(file_name);
			}
			Application.Run(m);
		}
	}
}
