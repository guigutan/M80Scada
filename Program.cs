using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M80Scada
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            int processCount = 0;
            System.Diagnostics.Process[] pa = System.Diagnostics.Process.GetProcesses();//获取当前进程数组。
            foreach (System.Diagnostics.Process PTest in pa)
            {
                if (PTest.ProcessName == System.Diagnostics.Process.GetCurrentProcess().ProcessName)
                {
                    processCount += 1;
                }
            }
            if (processCount > 1)
            {
                DialogResult dr; //如果程序已经运行，则给出提示。并退出本进程。

                dr = MessageBox.Show(System.Diagnostics.Process.GetCurrentProcess().ProcessName + "  程序已经在运行！", "退出程序", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //可能你不需要弹出窗口，在这里可以屏蔽掉
                return; //Exit;

            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
