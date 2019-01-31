using System;
using System.Runtime.InteropServices;

namespace BetterWindows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASSEXW
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public ClassStyles style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProc lpfnWndProc;
        [MarshalAs(UnmanagedType.I4)]
        public int cbClsExtra;
        [MarshalAs(UnmanagedType.I4)]
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
        public IntPtr hIconSm;

        public static WNDCLASSEXW New()
        {
            var nw = new WNDCLASSEXW();
            nw.cbSize = Marshal.SizeOf(typeof(WNDCLASSEXW));
            return nw;
        }
    }
}
