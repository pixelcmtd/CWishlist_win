using Microsoft.Win32;
using System;
using System.IO;
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
                form = new Form1();
                Application.Run(form);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static string[] args { get; private set; } = null;

        public static Form1 form = null;

        public static readonly string appdata = Registry.CurrentUser.OpenSubKey("Volatile Environment", false).GetValue("APPDATA").ToString();
    }
}
