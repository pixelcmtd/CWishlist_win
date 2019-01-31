using BetterWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BetterWindows.PInvoke;
using static BetterWindows.WM;

namespace BetterWindows.Tests
{
    class Program
    {
        static IntPtr WndProc(IntPtr hwnd, WM msg, UIntPtr wParam, IntPtr lParam)
        {
            switch(msg)
            {
                case CLOSE: DestroyWindow(hwnd); break;
                case DESTROY: PostQuitMessage(0); break;
                default: return DefWindowProc(hwnd, msg, wParam, lParam);
            }
            return NULL;
        }

        static void Main(string[] args)
        {
            Window w = new Window(
                WindowStyles.WS_BORDER,
                WindowStylesEx.WS_EX_CLIENTEDGE,
                0,
                WndProc,
                "awesomeWindow",
                "Awesome Window to test BetterWindows",
                unchecked((int)0x80000000), unchecked((int)0x80000000),
                1000, 1000);
            w.Show();
            w.EnterMainLoop();
        }
    }
}
