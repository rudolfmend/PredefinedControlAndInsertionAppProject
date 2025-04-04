using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class MouseHook
{
    public delegate void MouseActionHandler(System.Drawing.Point point, MouseButtons button);
    public static event MouseActionHandler? MouseAction; // Make nullable

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    private static readonly LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static void Start()
    {
        _hookID = SetHook(_proc);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        {
            // Check if MainModule is null before using it
            ProcessModule? curModule = curProcess.MainModule;
            if (curModule == null)
            {
                // Fallback if MainModule is null
                return SetWindowsHookEx(14, proc, IntPtr.Zero, 0);
            }

            return SetWindowsHookEx(14, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)0x0201) // WM_LBUTTONDOWN
            {
                // Check if the pointer is valid before attempting to use it
                if (lParam != IntPtr.Zero)
                {
                    // Use nullable reference to handle possible null result
                    MSLLHOOKSTRUCT? hookStructNullable = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                    // Only proceed if structure was successfully retrieved
                    if (hookStructNullable.HasValue)
                    {
                        MSLLHOOKSTRUCT hookStruct = hookStructNullable.Value;
                        MouseAction?.Invoke(new System.Drawing.Point(hookStruct.pt.x, hookStruct.pt.y), MouseButtons.Left);
                    }
                }
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public enum MouseButtons
    {
        Left, Right, Middle
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
