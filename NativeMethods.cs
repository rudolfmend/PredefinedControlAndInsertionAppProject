using System;
using System.Runtime.InteropServices;
namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Common native methods used across the application
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        internal static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(POINT Point);

        // Pridaná metóda IsWindowVisible
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        // Pridaná metóda GetWindowThreadProcessId
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        // Pridaná zjednodušená verzia GetWindowThreadProcessId, ktorá vracia len ProcessId
        internal static bool GetWindowProcessId(IntPtr hWnd, out int processId)
        {
            processId = 0;
            try
            {
                GetWindowThreadProcessId(hWnd, out processId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal const int MOUSEEVENTF_LEFTDOWN = 0x02;
        internal const int MOUSEEVENTF_LEFTUP = 0x04;

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        internal static System.Windows.Point GetCursorPosition()
        {
            POINT point;
            GetCursorPos(out point);
            return new System.Windows.Point(point.X, point.Y);
        }
    }
}
