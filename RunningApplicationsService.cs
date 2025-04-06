using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Service for detecting running applications and their windows
    /// </summary>
    public class RunningApplicationsService
    {
        // Win32 API import for getting window text
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        // Delegate for EnumWindows callback
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Get all running applications with visible windows
        /// </summary>
        /// <returns>List of application information</returns>
        public List<ApplicationInfo> GetRunningApplications()
        {
            var applications = new List<ApplicationInfo>();
            var processesWithWindows = new HashSet<int>();

            // Enumerate all top-level windows
            EnumWindows((hWnd, lParam) =>
            {
                // Check if window is visible
                if (!IsWindowVisible(hWnd))
                    return true; // Continue enumeration

                // Get window title
                int length = GetWindowTextLength(hWnd);
                if (length == 0)
                    return true; // Skip windows without title

                StringBuilder sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                // Get process ID for this window
                uint processId;
                GetWindowThreadProcessId(hWnd, out processId);

                // Add to set of processes with windows
                processesWithWindows.Add((int)processId);

                try
                {
                    // Get process information
                    Process process = Process.GetProcessById((int)processId);

                    // Skip system processes
                    if (IsSystemProcess(process))
                        return true;

                    // Create application info
                    var appInfo = new ApplicationInfo
                    {
                        ProcessId = (int)processId,
                        ProcessName = process.ProcessName,
                        MainWindowTitle = title,
                        MainWindowHandle = hWnd,
                        ExecutablePath = GetProcessExecutablePath(process),
                        Windows = new List<WindowInfo>
                        {
                            new WindowInfo
                            {
                                WindowHandle = hWnd,
                                WindowTitle = title
                            }
                        }
                    };

                    // Check if this process is already in our list
                    var existingApp = applications.FirstOrDefault(a => a.ProcessId == appInfo.ProcessId);
                    if (existingApp != null)
                    {
                        // Add window to existing application
                        existingApp.Windows.Add(appInfo.Windows[0]);
                    }
                    else
                    {
                        // Add new application
                        applications.Add(appInfo);
                    }
                }
                catch
                {
                    // Process might have exited, or we don't have access
                }

                return true; // Continue enumeration
            }, IntPtr.Zero);

            return applications;
        }

        /// <summary>
        /// Get all windows for a specific process
        /// </summary>
        /// <param name="processId">Process ID</param>
        /// <returns>List of window information</returns>
        public List<WindowInfo> GetWindowsForProcess(int processId)
        {
            var windows = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                // Get process ID for this window
                uint winProcessId;
                GetWindowThreadProcessId(hWnd, out winProcessId);

                // Check if this window belongs to our process
                if (winProcessId == processId && IsWindowVisible(hWnd))
                {
                    // Get window title
                    int length = GetWindowTextLength(hWnd);
                    if (length == 0)
                        return true; // Skip windows without title

                    StringBuilder sb = new StringBuilder(length + 1);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    string title = sb.ToString();

                    // Add window info
                    windows.Add(new WindowInfo
                    {
                        WindowHandle = hWnd,
                        WindowTitle = title
                    });
                }

                return true; // Continue enumeration
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary>
        /// Get all UI elements for a window
        /// </summary>
        /// <param name="windowHandle">Window handle</param>
        /// <returns>List of UI element information</returns>
        public List<UIElementInfo> GetUIElementsForWindow(IntPtr windowHandle)
        {
            var elements = new List<UIElementInfo>();

            try
            {
                // Get the automation element for the window
                AutomationElement window = AutomationElement.FromHandle(windowHandle);
                if (window == null)
                    return elements;

                // Find all UI elements
                Condition condition = new PropertyCondition(
                    AutomationElement.IsControlElementProperty, true);

                AutomationElementCollection allElements = window.FindAll(
                    TreeScope.Descendants, condition);

                // Process each element
                foreach (AutomationElement element in allElements)
                {
                    try
                    {
                        string name = TryGetAutomationProperty(element, AutomationElement.NameProperty) ?? "";
                        string automationId = TryGetAutomationProperty(element, AutomationElement.AutomationIdProperty) ?? "";
                        string className = TryGetAutomationProperty(element, AutomationElement.ClassNameProperty) ?? "";
                        ControlType? controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty) as ControlType;

                        // Only add interactive elements with a name or automation ID
                        if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(automationId))
                        {
                            if (IsInteractiveElement(element, controlType))
                            {
                                elements.Add(new UIElementInfo
                                {
                                    Name = name,
                                    AutomationId = automationId,
                                    ClassName = className,
                                    ControlTypeName = controlType?.ProgrammaticName ?? "Unknown"
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Skip elements that cause exceptions
                    }
                }
            }
            catch
            {
                // Return empty list if window can't be accessed
            }

            return elements;
        }

        /// <summary>
        /// Check if an element is interactive (can be clicked, typed into, etc.)
        /// </summary>
        private bool IsInteractiveElement(AutomationElement element, ControlType? controlType)
        {
            try
            {
                // Check for common interactive control types
                if (controlType == ControlType.Button ||
                    controlType == ControlType.Edit ||
                    controlType == ControlType.CheckBox ||
                    controlType == ControlType.ComboBox ||
                    controlType == ControlType.List ||
                    controlType == ControlType.ListItem ||
                    controlType == ControlType.MenuItem ||
                    controlType == ControlType.RadioButton ||
                    controlType == ControlType.Tab ||
                    controlType == ControlType.TabItem)
                {
                    return true;
                }

                // Check if element supports common interaction patterns
                return element.GetSupportedPatterns().Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a process is a system process that should be excluded
        /// </summary>
        private bool IsSystemProcess(Process process)
        {
            // List of system process names to exclude
            string[] systemProcesses = new[]
            {
                "explorer", "SearchUI", "ShellExperienceHost", "StartMenuExperienceHost", "Taskmgr",
                "ApplicationFrameHost", "SystemSettings", "TextInputHost", "SearchApp", "sihost"
            };

            return systemProcesses.Contains(process.ProcessName.ToLower());
        }

        /// <summary>
        /// Get the executable path for a process
        /// </summary>
        private string GetProcessExecutablePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? "";
            }
            catch
            {
                // Can't access main module (e.g., system process)
                return "";
            }
        }

        /// <summary>
        /// Try to get a property value from an automation element
        /// </summary>
        private string? TryGetAutomationProperty(AutomationElement element, AutomationProperty property)
        {
            try
            {
                object? value = element.GetCurrentPropertyValue(property);
                return value?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Information about a running application
    /// </summary>
    public class ApplicationInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string MainWindowTitle { get; set; } = string.Empty;
        public IntPtr MainWindowHandle { get; set; }
        public string ExecutablePath { get; set; } = string.Empty;
        public List<WindowInfo> Windows { get; set; } = new List<WindowInfo>();

        public override string ToString()
        {
            return $"{ProcessName} - {MainWindowTitle}";
        }
    }


    /// <summary>
    /// Information about a window
    /// </summary>
    public class WindowInfo
    {
        public IntPtr WindowHandle { get; set; }
        public string WindowTitle { get; set; } = string.Empty;

        public override string ToString()
        {
            return WindowTitle;
        }
    }

    /// <summary>
    /// Information about a UI element
    /// </summary>
    public class UIElementInfo
    {
        public string Name { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ControlTypeName { get; set; } = string.Empty;

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return $"{Name} ({ControlTypeName})";
            else if (!string.IsNullOrEmpty(AutomationId))
                return $"{AutomationId} ({ControlTypeName})";
            else
                return ControlTypeName;
        }
    }
}
