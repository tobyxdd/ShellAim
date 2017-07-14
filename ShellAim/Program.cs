using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShellAim
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            IntPtr gameWindowHandle = Utils.GetGameHandler();
            if (gameWindowHandle == IntPtr.Zero)
            {
                MessageBox.Show("Unable to obtain information from the game process.", Utils.RandomString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMenu(gameWindowHandle));
        }
    }
}
