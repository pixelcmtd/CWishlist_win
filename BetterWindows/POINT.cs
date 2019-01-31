using System.Runtime.InteropServices;

namespace BetterWindows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }
}
