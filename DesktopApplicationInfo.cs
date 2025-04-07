using System;
using System.IO;
using System.Diagnostics;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Classic desktop application information. Attributes and methods specific to desktop applications.
    /// </summary>
    public class DesktopApplicationInfo : ApplicationInfoBase
    {
        /// <summary>
        /// Path to the executable file
        /// </summary>
        public string ExecutablePath { get; set; } = string.Empty;

        /// <summary>
        /// Path to the shortcut file (.lnk) if this application was detected from a shortcut
        /// </summary>
        public string ShortcutPath { get; set; } = string.Empty;

        /// <summary>
        /// Process ID if the application is currently running
        /// </summary>
        public int? ProcessId { get; set; }

        /// <summary>
        /// Main window title if the application is currently running
        /// </summary>
        public string WindowTitle { get; set; } = string.Empty;

        /// <summary>
        /// Main window handle if the application is currently running
        /// </summary>
        public IntPtr WindowHandle { get; set; } = IntPtr.Zero;

        /// <summary>
        /// Gets the application type
        /// </summary>
        public override ApplicationType Type => ApplicationType.Desktop;

        /// <summary>
        /// Determines if the application can be launched
        /// </summary>
        public override bool CanLaunch => !string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath);

        /// <summary>
        /// Determines if the application is currently running
        /// </summary>
        public bool IsRunning => ProcessId.HasValue && ProcessId.Value > 0;

        /// <summary>
        /// Attempts to launch the application
        /// </summary>
        /// <returns>True if the application was launched successfully, false otherwise</returns>
        public bool Launch()
        {
            try
            {
                if (!CanLaunch)
                    return false;

                Process.Start(new ProcessStartInfo
                {
                    FileName = ExecutablePath,
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to activate the running application's window
        /// </summary>
        /// <returns>True if the window was activated, false otherwise</returns>
        public bool Activate()
        {
            if (!IsRunning || WindowHandle == IntPtr.Zero)
                return false;

            try
            {
                return NativeMethods.SetForegroundWindow(WindowHandle);
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Determines if this is a system application
        /// </summary>
        public bool IsSystemApplication { get; set; }

        /// <summary>
        /// Determines if this is a hidden application
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Command line arguments for launching the application
        /// </summary>
        public string Arguments { get; set; } = string.Empty;

        /// <summary>
        /// Launches the application with specified arguments
        /// </summary>
        public bool Launch(string arguments)
        {
            try
            {
                if (!CanLaunch)
                    return false;

                Process.Start(new ProcessStartInfo
                {
                    FileName = ExecutablePath,
                    Arguments = arguments,
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
