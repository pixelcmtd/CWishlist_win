using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CWishlist_win
{
    static class Program
    {
        [STAThread]
        static void Main(string[] ca)
        {
            try
            {
                args = ca;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
                AllocConsole();
                SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
#endif
                form = new Form1();
                form.Show();
                form.init();
                Application.Run(form);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static string[] args { get; private set; } = null;
        public static Form1 form = null;

        public static readonly string appdata = Registry.
            CurrentUser.OpenSubKey("Volatile Environment", false).GetValue("APPDATA").ToString();

#if DEBUG
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
        delegate bool HandlerRoutine(CtrlType CtrlType);

        enum CtrlType : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2
        }

        static bool ConsoleCtrlCheck(CtrlType ctrlType)
        {
            if (ctrlType == CtrlType.CTRL_C_EVENT) Environment.Exit(0);
            if (ctrlType == CtrlType.CTRL_CLOSE_EVENT || ctrlType == CtrlType.CTRL_BREAK_EVENT)
            {
                form.Close();
                return true;
            }
            else return false;
        }
#endif
    }
}
