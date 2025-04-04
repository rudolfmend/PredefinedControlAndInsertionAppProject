using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.Versioning;
using static MouseHook;
using System.ComponentModel.Design.Serialization;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Collections for UI binding
        private ObservableCollection<UIElement> _uiElements = [];
        private ObservableCollection<CalculationRule> _calculationRules = [];
        private ObservableCollection<SequenceStep> _sequenceSteps = [];

        // For automation capture
        private bool _isCapturing = false;
        private CancellationTokenSource? _captureTokenSource;

        // For automation execution
        private CancellationTokenSource? _executionTokenSource;
        private bool _isExecuting = false;

        private FlaUIAutomation _flaAutomation;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCollections();
            ConfigureDataGrid();
            _flaAutomation = new FlaUIAutomation();
        }

        private void InitializeCollections()
        {
            // Set as DataContext for controls (collections are initialized in field declarations)
            dgElements.ItemsSource = _uiElements;
            dgCalculations.ItemsSource = _calculationRules;
            LbSequence.ItemsSource = _sequenceSteps;
        }

        private void ConfigureDataGrid()
        {
            // Configure the operations ComboBox in calculations DataGrid
            if (dgCalculations.Columns[1] is DataGridComboBoxColumn operationsColumn)
            {
                var operations = new List<string> { "Add", "Subtract", "Multiply", "Divide", "Custom Formula" };
                operationsColumn.ItemsSource = operations;
            }
        }

        #region Event Handlers

        private void BtnRefreshApps_Click(object sender, RoutedEventArgs e)
        {
            RefreshRunningApplications();
        }

        private void BtnDetectApp_Click(object sender, RoutedEventArgs e)
        {
            // Create the application selector dialog
            var appSelectorDialog = new ApplicationSelectorDialog();

            // Show the dialog as a modal dialog
            bool? result = appSelectorDialog.ShowDialog();

            // Process the result
            if (result == true)
            {
                // Check if application and window were selected
                if (appSelectorDialog.SelectedApplication != null && appSelectorDialog.SelectedWindow != null)
                {
                    // Get the selected application and window
                    ApplicationInfo selectedApp = appSelectorDialog.SelectedApplication;
                    WindowInfo selectedWindow = appSelectorDialog.SelectedWindow;

                    // Update UI with selected application information
                    SetSelectedProcessByName(selectedApp.ProcessName); // Treba implementovať túto metódu
                    txtWindowTitle.Text = selectedWindow.WindowTitle;
                }

                // Check if UI elements were selected
                if (appSelectorDialog.SelectedElements.Count > 0)
                {
                    // Add selected elements to the UI elements collection
                    foreach (var elementInfo in appSelectorDialog.SelectedElements)
                    {
                        _uiElements.Add(new UIElement
                        {
                            Name = !string.IsNullOrEmpty(elementInfo.Name) ? elementInfo.Name :
                                   (!string.IsNullOrEmpty(elementInfo.AutomationId) ? elementInfo.AutomationId : elementInfo.ControlTypeName),
                            ElementType = MapControlTypeNameToElementType(elementInfo.ControlTypeName),
                            AutomationId = elementInfo.AutomationId,
                            Value = "",
                            IsTarget = false
                        });
                    }

                    // Show confirmation message
                    MessageBox.Show($"{appSelectorDialog.SelectedElements.Count} UI element(s) have been added to your automation.",
                                  "Elements Added", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // Helper method to map control type names to element types
        private static string MapControlTypeNameToElementType(string controlTypeName)
        {
            switch (controlTypeName.ToLower())
            {
                case "button":
                    return "Button";
                case "edit":
                    return "TextBox";
                case "checkbox":
                    return "CheckBox";
                case "combobox":
                    return "ComboBox";
                case "list":
                    return "ListBox";
                case "radiobutton":
                    return "RadioButton";
                case "tab":
                    return "TabControl";
                case "text":
                    return "Label";
                case "menu":
                    return "Menu";
                default:
                    return controlTypeName;
            }
        }

        private void BtnLaunchApp_Click(object sender, RoutedEventArgs e)
        {
            LaunchTargetApplication();
        }

        private async void BtnStartCapture_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCapturing)
            {
                // Start capture mode
                _isCapturing = true;
                BtnStartCapture.Content = "Stop Capture";

                // Change cursor to indicate capture mode
                this.Cursor = Cursors.Cross;

                // Initialize cancellation token
                _captureTokenSource = new CancellationTokenSource();

                try
                {
                    // Start the capture task
                    await StartElementCaptureAsync(_captureTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Capture was canceled, which is normal
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during element capture: {ex.Message}", "Capture Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Reset UI state
                    _isCapturing = false;
                    BtnStartCapture.Content = "Start Element Capture";
                    this.Cursor = Cursors.Arrow;
                }
            }
            else
            {
                // Stop capture mode
                _captureTokenSource?.Cancel();
                _isCapturing = false;
                BtnStartCapture.Content = "Start Element Capture";
                this.Cursor = Cursors.Arrow;
            }
        }

        private void BtnAddElement_Click(object sender, RoutedEventArgs e)
        {
            _uiElements.Add(new UIElement
            {
                Name = "NewElement",
                ElementType = "TextBox",
                AutomationId = "",
                Value = "",
                IsTarget = false
            });
        }

        private void BtnRemoveElement_Click(object sender, RoutedEventArgs e)
        {
            if (dgElements.SelectedItem is UIElement selectedElement)
            {
                _uiElements.Remove(selectedElement);
            }
            else
            {
                MessageBox.Show("Please select an element to remove.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAddStep_Click(object sender, RoutedEventArgs e)
        {
            // Open the edit dialog to create a new step
            var editDialog = new StepEditDialog(_uiElements);
            editDialog.Owner = this;

            bool? result = editDialog.ShowDialog();

            if (result == true)
            {
                // Create a new step with the next step number
                int nextStepNumber = _sequenceSteps.Count + 1;

                _sequenceSteps.Add(new SequenceStep
                {
                    StepNumber = nextStepNumber,
                    Action = editDialog.SelectedAction,
                    Target = editDialog.SelectedTarget?.Name ?? "Select a UI Element"
                });

                // If it's a SetValue action, update the element's value
                if (editDialog.SelectedAction == "SetValue" && editDialog.SelectedTarget != null)
                {
                    editDialog.SelectedTarget.Value = editDialog.Value;
                }
            }
        }

        private void BtnEditStep_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedStep();
        }

        // metóda pre dvojklik na položku v ListBox
        private void LbSequence_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditSelectedStep();
        }


        private void EditSelectedStep()
        {
            if (LbSequence.SelectedItem is SequenceStep selectedStep)
            {
                // Open the edit dialog with the selected step
                var editDialog = new StepEditDialog(_uiElements, selectedStep);
                editDialog.Owner = this;

                bool? result = editDialog.ShowDialog();

                if (result == true)
                {
                    // Update the step properties
                    selectedStep.Action = editDialog.SelectedAction;
                    selectedStep.Target = editDialog.SelectedTarget?.Name ?? "Select a UI Element";

                    // If it's a SetValue action, update the element's value
                    if (editDialog.SelectedAction == "SetValue" && editDialog.SelectedTarget != null)
                    {
                        editDialog.SelectedTarget.Value = editDialog.Value;
                    }

                    // Refresh the ListBox to show updated values
                    var selectedIndex = LbSequence.SelectedIndex;
                    LbSequence.Items.Refresh();
                    LbSequence.SelectedIndex = selectedIndex;
                }
            }
            else
            {
                MessageBox.Show("Please select a step to edit.", "No Selection",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRemoveStep_Click(object sender, RoutedEventArgs e)
        {
            if (LbSequence.SelectedItem is SequenceStep selectedStep)
            {
                _sequenceSteps.Remove(selectedStep);

                // Update step numbers for remaining steps
                for (int i = 0; i < _sequenceSteps.Count; i++)
                {
                    _sequenceSteps[i].StepNumber = i + 1;
                }
            }
            else
            {
                MessageBox.Show("Please select a step to remove.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnAddLoop_Click(object sender, RoutedEventArgs e)
        {
            // Check if steps are selected
            if (LbSequence.SelectedItems.Count < 2)
            {
                MessageBox.Show("Please select at least two steps to include in the loop",
                              "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected steps
            var selectedIndices = LbSequence.SelectedItems.Cast<SequenceStep>()
                                            .Select(s => _sequenceSteps.IndexOf(s))
                                            .OrderBy(i => i)
                                            .ToList();

            // Ensure selected steps are contiguous
            if (selectedIndices.Last() - selectedIndices.First() + 1 != selectedIndices.Count)
            {
                MessageBox.Show("Please select contiguous steps for the loop",
                              "Non-contiguous Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open loop configuration dialog
            var loopDialog = new LoopConfigDialog(_uiElements);
            loopDialog.Owner = this;

            if (loopDialog.ShowDialog() == true)
            {
                // Create loop start and end steps
                int loopStartIndex = selectedIndices.First();
                int loopEndIndex = selectedIndices.Last();

                // Update loop parameters
                var loopParams = loopDialog.LoopParameters;
                loopParams.StartStepIndex = loopStartIndex;
                loopParams.EndStepIndex = loopEndIndex;

                // Update step properties
                _sequenceSteps[loopStartIndex].IsLoopStart = true;
                _sequenceSteps[loopStartIndex].LoopParameters = loopParams;
                _sequenceSteps[loopEndIndex].IsLoopEnd = true;

                // Refresh the list to show visual indicators
                RefreshSequenceList();
            }
        }

        private void RefreshSequenceList()
        {
            // Update the display to show loop structure
            // This might involve custom styling or indentation in your list
            LbSequence.Items.Refresh();
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (LbSequence.SelectedItem is SequenceStep selectedStep)
            {
                int index = _sequenceSteps.IndexOf(selectedStep);
                if (index > 0)
                {
                    // Swap with the previous item
                    _sequenceSteps.Move(index, index - 1);

                    // Update step numbers
                    for (int i = 0; i < _sequenceSteps.Count; i++)
                    {
                        _sequenceSteps[i].StepNumber = i + 1;
                    }

                    // Keep the same item selected
                    LbSequence.SelectedIndex = index - 1;
                }
            }
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (LbSequence.SelectedItem is SequenceStep selectedStep)
            {
                int index = _sequenceSteps.IndexOf(selectedStep);
                if (index < _sequenceSteps.Count - 1)
                {
                    // Swap with the next item
                    _sequenceSteps.Move(index, index + 1);

                    // Update step numbers
                    for (int i = 0; i < _sequenceSteps.Count; i++)
                    {
                        _sequenceSteps[i].StepNumber = i + 1;
                    }

                    // Keep the same item selected
                    LbSequence.SelectedIndex = index + 1;
                }
            }
        }

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            // Open save dialog
            var saveDialog = new SaveConfigDialog();
            saveDialog.Owner = this;

            bool? result = saveDialog.ShowDialog();

            if (result == true && !string.IsNullOrEmpty(saveDialog.ConfigurationName))
            {
                // Create configuration object
                var config = new ConfigurationManager.AutomationConfiguration
                {
                    ProcessName = (CmbRunningApps.SelectedItem as ProcessInfo)?.Name ?? string.Empty,
                    ProcessId = (CmbRunningApps.SelectedItem as ProcessInfo)?.ProcessId ?? 0,
                    WindowTitle = txtWindowTitle.Text,
                    UIElements = new List<ConfigurationManager.UIElementConfig>(),
                    CalculationRules = new List<ConfigurationManager.CalculationRuleConfig>(),
                    SequenceSteps = new List<ConfigurationManager.SequenceStepConfig>()
                };

                // Convert UI elements
                foreach (var element in _uiElements)
                {
                    config.UIElements.Add(ConfigurationManager.ConvertUIElement(element));
                }

                // Convert calculation rules
                foreach (var rule in _calculationRules)
                {
                    config.CalculationRules.Add(ConfigurationManager.ConvertCalculationRule(rule));
                }

                // Convert sequence steps
                foreach (var step in _sequenceSteps)
                {
                    config.SequenceSteps.Add(ConfigurationManager.ConvertSequenceStep(step));
                }

                // Save configuration
                if (ConfigurationManager.SaveConfiguration(saveDialog.ConfigurationName, config))
                {
                    MessageBox.Show($"Configuration '{saveDialog.ConfigurationName}' has been saved successfully.",
                                  "Configuration Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            // Open load dialog
            var loadDialog = new LoadConfigDialog();
            loadDialog.Owner = this;

            bool? result = loadDialog.ShowDialog();

            if (result == true && !string.IsNullOrEmpty(loadDialog.SelectedConfiguration))
            {
                // Load configuration
                var config = ConfigurationManager.LoadConfiguration(loadDialog.SelectedConfiguration);

                if (config != null)
                {
                    // Confirm load
                    var confirmResult = MessageBox.Show(
                        "Loading this configuration will replace your current settings. Continue?",
                        "Confirm Load", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirmResult != MessageBoxResult.Yes)
                        return;

                    // Update application path and window title
                    SetSelectedProcessByName(config.ProcessName);
                    txtWindowTitle.Text = config.WindowTitle;

                    // Clear current collections
                    _uiElements.Clear();
                    _calculationRules.Clear();
                    _sequenceSteps.Clear();

                    // Convert and add UI elements
                    foreach (var elementConfig in config.UIElements)
                    {
                        _uiElements.Add(ConfigurationManager.ConvertUIElementConfig(elementConfig));
                    }

                    // Convert and add calculation rules
                    foreach (var ruleConfig in config.CalculationRules)
                    {
                        _calculationRules.Add(ConfigurationManager.ConvertCalculationRuleConfig(ruleConfig));
                    }

                    // Convert and add sequence steps
                    foreach (var stepConfig in config.SequenceSteps)
                    {
                        _sequenceSteps.Add(ConfigurationManager.ConvertSequenceStepConfig(stepConfig));
                    }

                    // Display success message
                    MessageBox.Show($"Configuration '{loadDialog.SelectedConfiguration}' loaded successfully.",
                                  "Configuration Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SetSelectedProcessByName(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return;

            var processes = CmbRunningApps.ItemsSource as List<ProcessInfo>;
            if (processes == null)
                return;

            var matchingProcess = processes.FirstOrDefault(p => p.Name.Equals(processName, StringComparison.OrdinalIgnoreCase));
            if (matchingProcess != null)
            {
                CmbRunningApps.SelectedItem = matchingProcess;
            }
        }

        private async void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (!_isExecuting)
            {
                // Start execution
                _isExecuting = true;
                BtnExecute.Content = "Stop Execution";

                // Get repeat interval if enabled
                bool shouldRepeat = chkRepeat.IsChecked ?? false;
                int interval = 0;

                if (shouldRepeat)
                {
                    if (!int.TryParse(txtInterval.Text, out interval) || interval <= 0)
                    {
                        MessageBox.Show("Please enter a valid interval in seconds.", "Invalid Interval",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Initialize cancellation token
                _executionTokenSource = new CancellationTokenSource();

                try
                {
                    // Execute automation sequence
                    await ExecuteAutomationAsync(shouldRepeat, interval, _executionTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Execution was canceled, which is normal
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during automation execution: {ex.Message}", "Execution Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Reset UI state
                    _isExecuting = false;
                    BtnExecute.Content = "Execute Automation";
                }
            }
            else
            {
                // Stop execution
                _executionTokenSource?.Cancel();
                _isExecuting = false;
                BtnExecute.Content = "Execute Automation";
            }
        }

        #endregion

        #region Automation Methods

        // Pomocná konverzná metóda
        private static string ConvertControlTypeToElementType(string controlType)
        {
            switch (controlType)
            {
                case "Button": return "Button";
                case "Edit": return "TextBox";
                case "CheckBox": return "CheckBox";
                case "ComboBox": return "ComboBox";
                case "List": return "ListBox";
                case "RadioButton": return "RadioButton";
                case "Tab": return "TabControl";
                case "Text": return "Label";
                default: return controlType;
            }
        }

        // Metóda na získanie zoznamu bežiacich aplikácií s viditeľnými oknami
        private List<ProcessInfo> GetRunningApplications()
        {
            var result = new List<ProcessInfo>();

            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    // Ignorovať procesy bez hlavného okna alebo so skrytým oknom
                    if (process.MainWindowHandle == IntPtr.Zero || !NativeMethods.IsWindowVisible(process.MainWindowHandle))
                        continue;

                    // Ignorovať systémové procesy a procesy bez titulku
                    if (string.IsNullOrEmpty(process.MainWindowTitle) || IsSystemProcess(process))
                        continue;

                    result.Add(new ProcessInfo
                    {
                        ProcessId = process.Id,
                        Name = process.ProcessName,
                        WindowTitle = process.MainWindowTitle,
                        WindowHandle = process.MainWindowHandle
                    });
                }
                catch
                {
                    // Ignorovať procesy, ku ktorým nemáme prístup
                }
            }

            return result;
        }

        // Trieda pre informácie o procese
        public class ProcessInfo
        {
            public int ProcessId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string WindowTitle { get; set; } = string.Empty;
            public IntPtr WindowHandle { get; set; }

            public override string ToString()
            {
                return $"{Name} - {WindowTitle}";
            }
        }

        // Metóda na kontrolu, či je proces systémový
        private bool IsSystemProcess(Process process)
        {
            string[] systemProcessNames = new[] {
        "explorer", "searchui", "shellexperiencehost", "startmenuexperiencehost",
        "applicationframehost", "systemsettings", "textinputhost"
    };

            return systemProcessNames.Contains(process.ProcessName.ToLower());
        }

        // Metóda pre načítanie do UI
        private void RefreshRunningApplications()
        {
            var apps = GetRunningApplications();
            CmbRunningApps.ItemsSource = apps;

            if (apps.Count > 0)
                CmbRunningApps.SelectedIndex = 0;
        }

        // Úprava metódy LaunchTargetApplication
        private bool LaunchTargetApplication()
        {
            if (CmbRunningApps.SelectedItem is ProcessInfo selectedApp)
            {
                try
                {
                    // Skontrolovať, či proces stále beží
                    Process process = Process.GetProcessById(selectedApp.ProcessId);

                    // Aktivovať okno
                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);

                    // Nastaviť názov okna
                    txtWindowTitle.Text = process.MainWindowTitle;

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to application: {ex.Message}",
                                  "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            MessageBox.Show("Please select a running application.",
                          "No Application Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private async Task StartElementCaptureAsync(CancellationToken cancellationToken)
        {
            MessageBox.Show("Click on UI elements in the target application to capture them.\n\n" +
                          "Press ESC or click 'Stop Capture' when done.",
                          "Element Capture Mode", MessageBoxButton.OK, MessageBoxImage.Information);

            // Registrujeme globálny hook pre myš
            MouseHook.Start();
            MouseHook.MouseAction += async (p, button) =>
            {
                if (button == MouseButtons.Left && _isCapturing)
                {
                    try
                    {
                        await Task.Delay(200); // Malé oneskorenie pre stabilizáciu UI

                        // Pokúsime sa získať okno na aktuálnej pozícii
                        var desktop = AutomationElement.RootElement;
                        var elementFromPoint = AutomationElement.FromPoint(new System.Windows.Point(p.X, p.Y));

                        if (elementFromPoint != null)
                        {
                            // Získame informácie o elemente
                            string elementName = TryGetAutomationProperty(elementFromPoint, AutomationElement.NameProperty) ?? "Unknown";
                            string automationId = TryGetAutomationProperty(elementFromPoint, AutomationElement.AutomationIdProperty) ?? "";
                            string className = TryGetAutomationProperty(elementFromPoint, AutomationElement.ClassNameProperty) ?? "Unknown";
                            string elementType = DetermineElementType(elementFromPoint);
                            string value = TryGetElementValue(elementFromPoint);

                            // Vypísať ladiace informácie
                            Console.WriteLine($"Element: {elementName}, Type: {elementType}, AutomationId: {automationId}");

                            // Pridať do kolekcie
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _uiElements.Add(new UIElement
                                {
                                    Name = elementName,
                                    ElementType = elementType,
                                    AutomationId = automationId,
                                    Value = value,
                                    IsTarget = false
                                });
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error capturing element: {ex.Message}");
                    }
                }
            };

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            finally
            {
                MouseHook.Stop();
            }
        }

        private async Task ExecuteAutomationAsync(bool repeat, int intervalSeconds, CancellationToken cancellationToken)
        {
            do
            {
                // Ensure target application is running
                if (!LaunchTargetApplication())
                {
                    return;
                }

                // Find the main window of the target application
                string windowTitle = txtWindowTitle.Text.Trim();
                AutomationElement? targetAppWindow = FindWindowByTitle(windowTitle);

                if (targetAppWindow == null)
                {
                    MessageBox.Show("Could not find the target application window.", "Window Not Found",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Execute sequence with loop support
                int currentStepIndex = 0;
                Stack<LoopContext> loopStack = new Stack<LoopContext>();

                while (currentStepIndex < _sequenceSteps.Count && !cancellationToken.IsCancellationRequested)
                {
                    var step = _sequenceSteps[currentStepIndex];

                    // Handle loop start
                    if (step.IsLoopStart && step.LoopParameters != null)
                    {
                        loopStack.Push(new LoopContext
                        {
                            StartIndex = currentStepIndex,
                            EndIndex = step.LoopParameters.EndStepIndex,
                            CurrentIteration = 1,
                            MaxIterations = step.LoopParameters.IterationCount,
                            IsInfinite = step.LoopParameters.IsInfiniteLoop,
                            ExitConditionElement = step.LoopParameters.ExitConditionElementName,
                            ExitConditionValue = step.LoopParameters.ExitConditionValue
                        });

                        // Move to next step (first step inside the loop)
                        currentStepIndex++;
                        continue;
                    }

                    // Handle loop end
                    if (step.IsLoopEnd && loopStack.Count > 0)
                    {
                        var currentLoop = loopStack.Peek();

                        bool shouldContinueLoop = false;

                        // Check if infinite loop or if we haven't reached max iterations
                        if (currentLoop.IsInfinite || currentLoop.CurrentIteration < currentLoop.MaxIterations)
                        {
                            // Check for exit condition if applicable
                            if (!string.IsNullOrEmpty(currentLoop.ExitConditionElement))
                            {
                                // Get the current value of the exit condition element
                                string currentValue = GetElementValue(currentLoop.ExitConditionElement);

                                // Continue loop if exit condition is not met
                                shouldContinueLoop = currentValue != currentLoop.ExitConditionValue;
                            }
                            else
                            {
                                // No exit condition, continue based on iteration count
                                shouldContinueLoop = true;
                            }
                        }

                        if (shouldContinueLoop)
                        {
                            // Increment iteration counter
                            currentLoop.CurrentIteration++;

                            // Jump back to start of loop
                            currentStepIndex = currentLoop.StartIndex + 1;
                            continue;
                        }
                        else
                        {
                            // Exit loop
                            loopStack.Pop();
                            currentStepIndex++;
                            continue;
                        }
                    }

                    // Get UI element info for this step
                    UIElement? targetElement = FindUIElementByName(step.Target);

                    if (targetElement == null)
                    {
                        MessageBox.Show($"UI Element '{step.Target}' not found in the configuration.",
                                      "Element Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                        currentStepIndex++;
                        continue;
                    }

                    // Locate element in the target application
                    AutomationElement? element = FindElementInWindow(targetAppWindow, targetElement);

                    if (element == null)
                    {
                        MessageBox.Show($"Could not find element '{targetElement.Name}' in the target application.",
                                      "Element Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                        currentStepIndex++;
                        continue;
                    }

                    // Execute action based on step type
                    await ExecuteStepAction(step, element, targetElement);

                    // Add a small delay between steps
                    await Task.Delay(500, cancellationToken);

                    // Move to next step
                    currentStepIndex++;
                }

                // Wait for the specified interval if repeating
                if (repeat && intervalSeconds > 0)
                {
                    await Task.Delay(intervalSeconds * 1000, cancellationToken);
                }

            } while (repeat && !cancellationToken.IsCancellationRequested);
        }

        // Helper class for tracking loop execution state
        private class LoopContext
        {
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public int CurrentIteration { get; set; }
            public int MaxIterations { get; set; }
            public bool IsInfinite { get; set; }
            public string? ExitConditionElement { get; set; }
            public string? ExitConditionValue { get; set; }
        }

        // Helper method to get the current value of an element
        private string GetElementValue(string elementName)
        {
            // Find the element by name in your UI elements collection
            var element = _uiElements.FirstOrDefault(e => e.Name == elementName);

            // Use your existing mechanisms to get the current value
            // This would likely involve FlaUI or UI Automation to fetch the current value
            // Return empty string if element not found or value can't be retrieved

            return ""; // Placeholder - implement actual value retrieval
        }


        private async Task ExecuteStepAction(SequenceStep step, AutomationElement element, UIElement elementConfig)
        {
            // Try FlaUI approach first
            var flaElements = _flaAutomation.CaptureUIElements();
            var targetFlaElement = flaElements.FirstOrDefault(e => e.Name == elementConfig.Name ||
                                                                e.AutomationId == elementConfig.AutomationId);

            if (targetFlaElement != null)
            {
                // FlaUI implementation
                switch (step.Action)
                {
                    case "SetValue":
                        string flaValueToSet = CalculateValueForElement(elementConfig.Name);
                        if (_flaAutomation.SetElementValue(targetFlaElement, flaValueToSet))
                            return; // Success, no need to try legacy method
                        break;

                    case "Click":
                        if (_flaAutomation.ClickElement(targetFlaElement))
                            return; // Success, no need to try legacy method
                        break;

                    case "Wait":
                        // Wait for specified time
                        if (int.TryParse(elementConfig.Value, out int flaWaitTime))
                        {
                            await Task.Delay(flaWaitTime * 1000);
                        }
                        else
                        {
                            await Task.Delay(1000); // Default to 1 second
                        }
                        return; // Wait action always succeeds
                }
            }

            // Fallback to traditional UI Automation (if FlaUI fails or element not found)
            switch (step.Action)
            {
                case "SetValue":
                    // Calculate value to set if needed
                    string valueToSet = CalculateValueForElement(elementConfig.Name);

                    // Set value to the element
                    if (!SetElementValue(element, valueToSet))
                    {
                        // If direct set failed, try alternative method - simulating keystrokes
                        try
                        {
                            // First focus the element
                            element.SetFocus();

                            // Wait for the element to get focus
                            await Task.Delay(300);

                            // Clear existing content (CTRL+A, then DELETE)
                            SendKeys.SendWait("^a");
                            await Task.Delay(100);
                            SendKeys.SendWait("{DELETE}");
                            await Task.Delay(100);

                            // Send the new value
                            SendKeys.SendWait(valueToSet);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error sending keystrokes: {ex.Message}",
                                          "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;

                case "Click":
                    // Click on the element
                    if (!ClickElement(element))
                    {
                        // If direct click failed, try alternative method - simulating mouse click
                        try
                        {
                            System.Windows.Rect rect = element.Current.BoundingRectangle;
                            int centerX = (int)(rect.Left + rect.Width / 2);
                            int centerY = (int)(rect.Top + rect.Height / 2);

                            // Move cursor and click
                            NativeMethods.SetCursorPos(centerX, centerY);
                            await Task.Delay(100);
                            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            await Task.Delay(100);
                            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error simulating mouse click: {ex.Message}",
                                          "Click Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    break;

                case "Wait":
                    // Wait for specified time
                    if (int.TryParse(elementConfig.Value, out int waitTime))
                    {
                        await Task.Delay(waitTime * 1000);
                    }
                    else
                    {
                        await Task.Delay(1000); // Default to 1 second
                    }
                    break;

                default:
                    MessageBox.Show($"Unknown action: {step.Action}", "Action Error",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        #endregion

        #region Helper Methods

        private static string? TryGetAutomationProperty(AutomationElement element, AutomationProperty property)
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

        private static string TryGetElementValue(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
                {
                    ValuePattern valuePattern = (ValuePattern)patternObj;
                    return valuePattern.Current.Value ?? "";
                }
            }
            catch
            {
                // Ignore errors when trying to get element value
            }

            return "";
        }

        private static string DetermineElementType(AutomationElement element)
        {
            try
            {
                ControlType? controlType = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty) as ControlType;

                if (controlType == ControlType.Button)
                    return "Button";
                else if (controlType == ControlType.Edit)
                    return "TextBox";
                else if (controlType == ControlType.CheckBox)
                    return "CheckBox";
                else if (controlType == ControlType.ComboBox)
                    return "ComboBox";
                else if (controlType == ControlType.List)
                    return "ListBox";
                else if (controlType == ControlType.RadioButton)
                    return "RadioButton";
                else if (controlType == ControlType.Tab)
                    return "TabControl";
                else if (controlType == ControlType.Text)
                    return "Label";
                else if (controlType == ControlType.Menu)
                    return "Menu";
                else
                    return controlType?.ProgrammaticName ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static AutomationElement? FindWindowByTitle(string title)
        {
            try
            {
                return AutomationElement.RootElement.FindFirst(
                    TreeScope.Children,
                    new PropertyCondition(AutomationElement.NameProperty, title));
            }
            catch
            {
                return null;
            }
        }

        private UIElement? FindUIElementByName(string name)
        {
            foreach (var element in _uiElements)
            {
                if (element.Name == name)
                {
                    return element;
                }
            }

            return null;
        }

        private static AutomationElement? FindElementInWindow(AutomationElement window, UIElement elementConfig)
        {
            // Try to find by automation ID first if available
            if (!string.IsNullOrEmpty(elementConfig.AutomationId))
            {
                try
                {
                    var elementByAutomationId = window.FindFirst(
                        TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.AutomationIdProperty, elementConfig.AutomationId));

                    if (elementByAutomationId != null)
                        return elementByAutomationId;
                }
                catch
                {
                    // Ignore errors and try other methods
                }
            }

            // Try to find by name
            try
            {
                var elementByName = window.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, elementConfig.Name));

                if (elementByName != null)
                    return elementByName;
            }
            catch
            {
                // Ignore errors and try other methods
            }

            // If still not found, try to find by element type
            try
            {
                ControlType targetType = GetControlTypeFromString(elementConfig.ElementType);

                var elements = window.FindAll(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, targetType));

                if (elements != null && elements.Count > 0)
                    return elements[0]; // Return the first matching element
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private static ControlType GetControlTypeFromString(string typeName)
        {
            switch (typeName)
            {
                case "Button": return ControlType.Button;
                case "TextBox": return ControlType.Edit;
                case "CheckBox": return ControlType.CheckBox;
                case "ComboBox": return ControlType.ComboBox;
                case "ListBox": return ControlType.List;
                case "RadioButton": return ControlType.RadioButton;
                case "TabControl": return ControlType.Tab;
                case "Label": return ControlType.Text;
                case "Menu": return ControlType.Menu;
                default: return ControlType.Custom;
            }
        }

        private static bool SetElementValue(AutomationElement element, string value)
        {
            try
            {
                // Try to use ValuePattern first
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj1))
                {
                    ValuePattern valuePattern = (ValuePattern)patternObj1;
                    valuePattern.SetValue(value);
                    return true;
                }
                // Remove the LegacyIAccessiblePattern part
                // Try to use TextPattern
                else if (element.TryGetCurrentPattern(TextPattern.Pattern, out object patternObj3))
                {
                    // Focus the element first
                    element.SetFocus();
                    Thread.Sleep(100);

                    // Send key presses to clear and set text
                    SendKeys.SendWait("^a");
                    Thread.Sleep(50);
                    SendKeys.SendWait("{DELETE}");
                    Thread.Sleep(50);

                    // Send the new value
                    SendKeys.SendWait(value);
                    return true;
                }
                else
                {
                    // Fallback to direct keyboard input
                    element.SetFocus();
                    Thread.Sleep(100);

                    // Clear existing content
                    SendKeys.SendWait("^a");
                    Thread.Sleep(50);
                    SendKeys.SendWait("{DELETE}");
                    Thread.Sleep(50);

                    // Send the new value
                    SendKeys.SendWait(value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting value: {ex.Message}", "Set Value Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private static bool ClickElement(AutomationElement element)
        {
            try
            {
                // Try to use InvokePattern first
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out object patternObj1))
                {
                    InvokePattern invokePattern = (InvokePattern)patternObj1;
                    invokePattern.Invoke();
                    return true;
                }
                // Try to use TogglePattern
                else if (element.TryGetCurrentPattern(TogglePattern.Pattern, out object patternObj2))
                {
                    TogglePattern togglePattern = (TogglePattern)patternObj2;
                    togglePattern.Toggle();
                    return true;
                }
                // Try to use ExpandCollapsePattern
                else if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object patternObj3))
                {
                    ExpandCollapsePattern expandPattern = (ExpandCollapsePattern)patternObj3;
                    if (expandPattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                        expandPattern.Expand();
                    else
                        expandPattern.Collapse();
                    return true;
                }
                // Try to use SelectionItemPattern
                else if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object patternObj4))
                {
                    SelectionItemPattern selectionPattern = (SelectionItemPattern)patternObj4;
                    selectionPattern.Select();
                    return true;
                }
                // Remove LegacyIAccessiblePattern part and add fallback
                else
                {
                    // Fallback to simulating mouse click
                    System.Windows.Rect rect = element.Current.BoundingRectangle;
                    int centerX = (int)(rect.Left + rect.Width / 2);
                    int centerY = (int)(rect.Top + rect.Height / 2);

                    // Move cursor and click
                    NativeMethods.SetCursorPos(centerX, centerY);
                    Thread.Sleep(100);
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    Thread.Sleep(100);
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clicking element: {ex.Message}", "Click Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private string CalculateValueForElement(string elementName)
        {
            // Find calculation rule for this element
            foreach (var rule in _calculationRules)
            {
                if (rule.TargetField == elementName)
                {
                    // Parse input values
                    double value1, value2;
                    if (!double.TryParse(rule.Value1, out value1))
                        value1 = 0;

                    if (!double.TryParse(rule.Value2, out value2))
                        value2 = 0;

                    // Perform calculation based on operation
                    switch (rule.Operation)
                    {
                        case "Add":
                            return (value1 + value2).ToString();

                        case "Subtract":
                            return (value1 - value2).ToString();

                        case "Multiply":
                            return (value1 * value2).ToString();

                        case "Divide":
                            if (value2 == 0)
                                return "Error: Division by zero";

                            return (value1 / value2).ToString();

                        case "Custom Formula":
                            // TODO: Implement custom formula parsing/calculation
                            return rule.Formula;

                        default:
                            return rule.Value1; // Default to first value
                    }
                }
            }

            // If no calculation rule found, return empty string
            return "";
        }
        #endregion

        #region Model Classes

        public class LoopControl
        {
            public int StartStepIndex { get; set; }
            public int EndStepIndex { get; set; }
            public int IterationCount { get; set; } = 1; // Default to 1 iteration
            public bool IsInfiniteLoop { get; set; } = false;
            public string? ExitConditionElementName { get; set; }
            public string? ExitConditionValue { get; set; }
        }


        public class CalculationRule
        {
            public string TargetField { get; set; } = string.Empty;
            public string Operation { get; set; } = string.Empty;
            public string Value1 { get; set; } = string.Empty;
            public string Value2 { get; set; } = string.Empty;
            public string Formula { get; set; } = string.Empty;
        }

        public class SequenceStep
        {
            public int StepNumber { get; set; }
            public string Action { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public bool IsLoopStart { get; set; } = false;
            public bool IsLoopEnd { get; set; } = false;
            public LoopControl? LoopParameters { get; set; }
        }

        #endregion
    }
}
