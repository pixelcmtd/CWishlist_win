using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterWindows
{
    public static class PInvoke
    {
        public static readonly IntPtr NULL = (IntPtr)null;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowExW(
            WindowStylesEx dwExStyle,
            IntPtr lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            WindowStyles dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMessageW(
            out MSG lpMsg,
            IntPtr hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIconW(
            IntPtr hInstance,
            IntPtr lpIconName);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorW(
            IntPtr hInstance,
            IntPtr lpIconName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        public static extern short RegisterClassExW(
            [In] ref WNDCLASSEXW lpwcx);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(
            IntPtr hWnd,
            int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(
            IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(
            [In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessageW(
            [In] ref MSG lpmsg);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(
            IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(
            int nExitCode);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(
            IntPtr hWnd,
            WM uMsg,
            UIntPtr wParam,
            IntPtr lParam);
    }
}
