// FlaUIAutomation.cs
using System.Windows;
using System.Runtime.Versioning;
using FlaUI.Core.Conditions;
//  alias pre konfliktný ControlType
using FlaUIControlType = FlaUI.Core.Definitions.ControlType;
using FlaUI.UIA3;
// Aliasy pre ďalšie konfliktné triedy
using FlaUIApplication = FlaUI.Core.Application;
using FlaUIWindow = FlaUI.Core.AutomationElements.Window;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]
    public class FlaUIAutomation : IDisposable
    {
        private UIA3Automation _automation;
        private FlaUIApplication? _application;
        private FlaUIWindow? _mainWindow;

        public FlaUIAutomation()
        {
            _automation = new UIA3Automation();
        }

        public bool LaunchApplication(string appPath)
        {
            try
            {
                // Launch application
                _application = FlaUIApplication.Launch(appPath);

                // Wait for the application to initialize
                Thread.Sleep(1000);

                // Get the main window
                _mainWindow = _application.GetMainWindow(_automation);

                return _mainWindow != null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error launching application: {ex.Message}", "Launch Error",
                               System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public string GetMainWindowTitle()
        {
            if (_mainWindow != null)
                return _mainWindow.Title;
            return string.Empty;
        }

        public bool AttachToApplication(string processName, string windowTitle)
        {
            try
            {
                // Find all processes with the given name
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                    return false;

                // Attach to the first process
                _application = FlaUIApplication.Attach(processes[0]);

                // Get all windows
                var windows = _application.GetAllTopLevelWindows(_automation);

                // Find window with matching title
                foreach (var window in windows)
                {
                    if (window.Title.Contains(windowTitle))
                    {
                        _mainWindow = window;
                        return true;
                    }
                }

                // If no specific window found, use the main window
                if (windows.Length > 0)
                {
                    _mainWindow = windows[0];
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error attaching to application: {ex.Message}", "Attach Error",
                               System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public List<AutomationElementInfo> CaptureUIElements()
        {
            var elements = new List<AutomationElementInfo>();

            if (_mainWindow == null)
                return elements;

            try
            {
                // Použite alias FlaUIControlType namiesto ControlType
                var allElements = _mainWindow.FindAllDescendants(cf => cf.ByControlType(FlaUIControlType.Button)
                    .Or(cf.ByControlType(FlaUIControlType.Edit))
                    .Or(cf.ByControlType(FlaUIControlType.CheckBox))
                    .Or(cf.ByControlType(FlaUIControlType.ComboBox))
                    .Or(cf.ByControlType(FlaUIControlType.List))
                    .Or(cf.ByControlType(FlaUIControlType.RadioButton))
                    .Or(cf.ByControlType(FlaUIControlType.Tab))
                    .Or(cf.ByControlType(FlaUIControlType.Text)));

                foreach (var element in allElements)
                {
                    // Skip hidden or disabled elements
                    if (!element.IsEnabled)
                        continue;

                    // Create element info
                    var elementInfo = new AutomationElementInfo
                    {
                        Name = element.Name,
                        AutomationId = element.AutomationId,
                        ControlType = element.ControlType.ToString(),
                        ClassName = element.ClassName,
                        IsEnabled = element.IsEnabled,
                        NativeElement = element
                    };

                    // Get value if applicable
                    if (element.Patterns.Value.IsSupported)
                    {
                        elementInfo.Value = element.Patterns.Value.Pattern.Value.Value;
                    }

                    elements.Add(elementInfo);
                }
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error capturing UI elements: {ex.Message}", "Capture Error",
                               5000);
            }

            return elements;
        }

        public bool SetElementValue(AutomationElementInfo elementInfo, string value)
        {
            if (elementInfo?.NativeElement == null)
                return false;

            try
            {
                var element = elementInfo.NativeElement;

                // Try using Value pattern
                if (element.Patterns.Value.IsSupported)
                {
                    element.Patterns.Value.Pattern.SetValue(value);
                    return true;
                }

                // Try to focus and simulate typing
                element.Focus();
                Thread.Sleep(100);

                // Select all and delete
                System.Windows.Forms.SendKeys.SendWait("^a");
                Thread.Sleep(50);
                System.Windows.Forms.SendKeys.SendWait("{DELETE}");
                Thread.Sleep(50);

                // Type new value
                System.Windows.Forms.SendKeys.SendWait(value);
                return true;
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error setting value: {ex.Message}", "Set Value Error",
                               5000);
                return false;
            }
        }

        public bool ClickElement(AutomationElementInfo elementInfo)
        {
            if (elementInfo?.NativeElement == null)
                return false;

            try
            {
                var element = elementInfo.NativeElement;

                // Try invoke pattern for buttons
                if (element.Patterns.Invoke.IsSupported)
                {
                    element.Patterns.Invoke.Pattern.Invoke();
                    return true;
                }

                // Try toggle pattern for checkboxes
                if (element.Patterns.Toggle.IsSupported)
                {
                    element.Patterns.Toggle.Pattern.Toggle();
                    return true;
                }

                // Try selection item pattern
                if (element.Patterns.SelectionItem.IsSupported)
                {
                    element.Patterns.SelectionItem.Pattern.Select();
                    return true;
                }

                // Try expandcollapse pattern
                if (element.Patterns.ExpandCollapse.IsSupported)
                {
                    if (element.Patterns.ExpandCollapse.Pattern.ExpandCollapseState ==
                        FlaUI.Core.Definitions.ExpandCollapseState.Collapsed)
                        element.Patterns.ExpandCollapse.Pattern.Expand();
                    else
                        element.Patterns.ExpandCollapse.Pattern.Collapse();
                    return true;
                }

                // Fallback to click
                element.Click();
                return true;
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error clicking element: {ex.Message}", "Click Error",
                               5000);
                return false;
            }
        }

        public void Dispose()
        {
            _application?.Dispose();
            _automation?.Dispose();
        }
    }

    public class AutomationElementInfo
    {
        public string Name { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }        

        public FlaUI.Core.AutomationElements.AutomationElement? NativeElement { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return $"{Name} ({ControlType})";
            else if (!string.IsNullOrEmpty(AutomationId))
                return $"{AutomationId} ({ControlType})";
            else
                return ControlType;
        }
    }
}
