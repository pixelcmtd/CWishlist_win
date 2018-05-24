using System;
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
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static string[] args = null;
    }
}
