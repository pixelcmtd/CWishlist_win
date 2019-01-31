using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static BetterWindows.PInvoke;

namespace BetterWindows
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr WndProc(IntPtr hwnd, WM msg, UIntPtr wParam, IntPtr lParam);

    public class Window
    {
        WNDCLASSEXW wc;
        IntPtr hwnd;
        MSG msg;
        IntPtr hInstance;

        public Window(WindowStyles winStyles,
                      WindowStylesEx winStylesEx,
                      ClassStyles classStyles,
                      WndProc wndProc,
                      string className,
                      string title,
                      int x, int y,
                      int wid, int hei)
        {
            wc = WNDCLASSEXW.New();
            wc.style = classStyles;
            wc.lpfnWndProc = wndProc;
            wc.cbClsExtra = 0;
            wc.cbWndExtra = 0;
            hInstance = Marshal.GetHINSTANCE(typeof(Window).Assembly.GetModules()[0]);
            wc.hIcon = LoadIconW(NULL, (IntPtr)32512); //IDI_APPLICATION
            wc.hCursor = LoadCursorW(NULL, (IntPtr)32512); //IDC_ARROW
            wc.hbrBackground = (IntPtr)6;
            wc.lpszMenuName = null;
            wc.lpszClassName = className;
            wc.hIconSm = wc.hIcon;
            short clsatm;
            if ((clsatm = RegisterClassExW(ref wc)) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            hwnd = CreateWindowExW(
                winStylesEx,
                (IntPtr)clsatm,
                title,
                winStyles,
                x, y,
                wid, hei,
                NULL, NULL,
                hInstance, NULL);
            if (hwnd == NULL)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            ShowWindow(hwnd, 5);
            UpdateWindow(hwnd);
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public void Hide()
        {
            ShowWindow(hwnd, 0);
            UpdateWindow(hwnd);
        }

        /// <summary>
        /// Blocks the current thread as long as the window is open.
        /// </summary>
        /// <returns>The last wParam.</returns>
        public int EnterMainLoop()
        {
            while (GetMessageW(out msg, NULL, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessageW(ref msg);
            }
            return (int)msg.wParam.ToUInt32();
        }
    }
}
