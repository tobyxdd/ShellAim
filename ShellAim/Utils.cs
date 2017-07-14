using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShellAim
{
    static class Utils
    {
        private static Random random = new Random();
        public static string RandomString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -";
            return new string(Enumerable.Repeat(chars, random.Next(4, 20))
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static IntPtr GetGameHandler()
        {
            IntPtr gameWindowHandle;
            try
            {
                gameWindowHandle = Process.GetProcessesByName(Constants.PROCESS_NAME)[0].MainWindowHandle;
                return gameWindowHandle;
            }
            catch (Exception e)
            {
                return IntPtr.Zero;
            }
        }
    }
}
