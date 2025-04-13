using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using static PredefinedControlAndInsertionAppProject.MainWindow;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Dialog for editing automation step properties
    /// </summary>
    public partial class StepEditDialog : Window
    {
        // Result properties
        public string SelectedAction { get; private set; } = string.Empty;
        public AppUIElement SelectedTarget { get; private set; } = new AppUIElement();
        public string Value { get; private set; } = string.Empty;

        // Click properties - explicitly qualified with MainWindow
        public MainWindow.ClickMode SelectedClickMode { get; private set; } = MainWindow.ClickMode.SingleClick;
        public int ClickCount { get; private set; } = 1;
        public string ClickConditionElement { get; private set; } = string.Empty;
        public string ClickConditionValue { get; private set; } = string.Empty;
        public int ClickInterval { get; private set; } = 500;

        // Reference to the step being edited (if any)
        private SequenceStep? _editedStep;
        private IEnumerable<AppUIElement> _uiElements;

        public StepEditDialog(IEnumerable<AppUIElement> availableElements, SequenceStep? currentStep = null)
        {
            InitializeComponent();
            _uiElements = availableElements;
            _editedStep = currentStep;

            // Set data source for SearchableComboBox
            searchableCmbTarget.ItemsSource = availableElements;
            searchableCmbConditionElement.ItemsSource = availableElements;

            // Set initial values if editing an existing step
            if (currentStep != null)
            {
                // Set action
                foreach (ComboBoxItem item in cmbAction.Items)
                {
                    if (item.Content.ToString() == currentStep.Action)
                    {
                        cmbAction.SelectedItem = item;
                        break;
                    }
                }

                // Set target
                if (currentStep.Target != "Select a UI Element")
                {
                    searchableCmbTarget.SelectItemByName(currentStep.Target);
                }

                // Set value (for Textbox or Wait actions)
                if (currentStep.Action == "SetValue" || currentStep.Action == "Wait")
                {
                    txtValue.Text = GetElementValue(currentStep.Target);
                }

                // Inicializácia hodnôt pre kliknutie
                if (currentStep.Action == "Click")
                {
                    InitializeClickOptions(currentStep);
                }
            }

            // Set initial UI state
            UpdateUIBasedOnAction();

            // Add event handlers
            cmbAction.SelectionChanged += CmbAction_SelectionChanged;
            searchableCmbTarget.SelectionChanged += SearchableCmbTarget_SelectionChanged;
        }

        //  event handler for - SearchableComboBox
        private void SearchableCmbTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (searchableCmbTarget.SelectedItem is AppUIElement selectedElement &&
                cmbAction.SelectedItem is ComboBoxItem actionItem &&
                actionItem.Content.ToString() == "SetValue")
            {
                // Automatically populate the value from the selected element
                txtValue.Text = selectedElement.Value;
            }
        }

        // Metóda pre získanie hodnoty z UI elementu podľa mena
        private string GetElementValue(string elementName)
        {
            foreach (var element in _uiElements)
            {
                if (element.Name == elementName)
                {
                    return element.Value;
                }
            }
            return string.Empty;
        }

        private void InitializeClickOptions(SequenceStep step)
        {
            // Nastaviť vybraný režim kliknutia
            foreach (ComboBoxItem item in cmbClickMode.Items)
            {
                if (item.Tag.ToString() == step.ClickMode.ToString())
                {
                    cmbClickMode.SelectedItem = item;
                    break;
                }
            }

            // Set the values based on the mode
            // Nastaviť hodnoty podľa režimu
            txtClickCount.Text = step.ClickCount.ToString();
            txtClickInterval.Text = step.ClickInterval.ToString();

            // Set the condition
            // Nastaviť podmienku
            if (step.ClickMode == MainWindow.ClickMode.ConditionalClicks)
            {
                // Find and select the element in the combobox for the condition
                // Nájsť a vybrať element v comboboxe pre podmienku
                if (!string.IsNullOrEmpty(step.ClickConditionElement))
                {
                    string elementName = step.ClickConditionElement!;
                    searchableCmbConditionElement.SelectItemByName(elementName!);
                    txtConditionValue.Text = step.ClickConditionValue ?? string.Empty;
                }
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (cmbAction.SelectedItem == null)
            {
                TimedMessageBox.Show("Please select an action.", "Missing Input", 5000);
                return;
            }

            string action = (cmbAction.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

            if (action != "Wait" && searchableCmbTarget.SelectedItem == null)
            {
                TimedMessageBox.Show("Please select a target element.", "Missing Input", 5000);
                return;
            }

            if (action == "Wait" && (string.IsNullOrEmpty(txtValue.Text) || !double.TryParse(txtValue.Text, out _)))
            {
                TimedMessageBox.Show("Please enter a valid number of seconds for the wait time.",
                                "Invalid Input", 5000);
                return;
            }

            // Validácia pre Click options
            if (action == "Click")
            {
                if (cmbClickMode.SelectedItem is ComboBoxItem clickModeItem)
                {
                    string clickMode = clickModeItem.Tag.ToString();

                    if (clickMode == "MultipleClicks")
                    {
                        if (string.IsNullOrEmpty(txtClickCount.Text) || !int.TryParse(txtClickCount.Text, out int clickCount) || clickCount <= 0)
                        {
                            TimedMessageBox.Show("Please enter a valid number of clicks (greater than 0).",
                                "Invalid Input", 5000);
                            return;
                        }
                    }
                    else if (clickMode == "ConditionalClicks")
                    {
                        if (searchableCmbConditionElement.SelectedItem == null)
                        {
                            TimedMessageBox.Show("Please select an element for the condition.",
                                "Missing Input", 5000);
                            return;
                        }

                        if (string.IsNullOrEmpty(txtConditionValue.Text))
                        {
                            TimedMessageBox.Show("Please enter a value for the condition.",
                                "Missing Input", 5000);
                            return;
                        }
                    }

                    // Validácia intervalu
                    if (string.IsNullOrEmpty(txtClickInterval.Text) || !int.TryParse(txtClickInterval.Text, out int interval) || interval <= 0)
                    {
                        TimedMessageBox.Show("Please enter a valid interval in milliseconds (greater than 0).",
                            "Invalid Input", 5000);
                        return;
                    }
                }
            }

            // Store results
            SelectedAction = action;
            SelectedTarget = searchableCmbTarget.SelectedItem as AppUIElement ?? new AppUIElement();
            Value = txtValue.Text;

            // Store click options if relevant
            if (action == "Click")
            {
                if (cmbClickMode.SelectedItem is ComboBoxItem clickModeItem)
                {
                    // Konverzia reťazca na enum (explicitne kvalifikovaný)
                    Enum.TryParse<MainWindow.ClickMode>(clickModeItem.Tag.ToString(), out MainWindow.ClickMode mode);
                    SelectedClickMode = mode;

                    // Ostatné click properties
                    int.TryParse(txtClickCount.Text, out int clickCount);
                    ClickCount = clickCount > 0 ? clickCount : 1;

                    int.TryParse(txtClickInterval.Text, out int interval);
                    ClickInterval = interval > 0 ? interval : 500;

                    // Podmienka
                    if (mode == MainWindow.ClickMode.ConditionalClicks)
                    {
                        ClickConditionElement = (searchableCmbConditionElement.SelectedItem as AppUIElement)?.Name ?? string.Empty;
                        ClickConditionValue = txtConditionValue.Text;
                    }

                    // Ak upravujeme existujúci krok, aktualizujeme ho priamo
                    if (_editedStep != null)
                    {
                        _editedStep.ClickMode = SelectedClickMode;
                        _editedStep.ClickCount = ClickCount;
                        _editedStep.ClickInterval = ClickInterval;
                        _editedStep.ClickConditionElement = ClickConditionElement;
                        _editedStep.ClickConditionValue = ClickConditionValue;
                    }
                }
            }

            // Close dialog with success
            DialogResult = true;
            Close();
        }

        private void UpdateUIBasedOnAction()
        {
            if (cmbAction.SelectedItem == null)
                return;

            string? action = (cmbAction.SelectedItem as ComboBoxItem)?.Content?.ToString();

            // Show/hide and update value field based on action
            if (action == "SetValue")
            {
                valueGrid.Visibility = Visibility.Visible;
                valueLabel.Text = "Value:";
                grpClickOptions.Visibility = Visibility.Collapsed;
                stepDescription.Text = "This step will set a value in the target element (usually a textbox or other input field).";
            }
            else if (action == "Click")
            {
                valueGrid.Visibility = Visibility.Collapsed;
                grpClickOptions.Visibility = Visibility.Visible;

                // Inicializácia click options pri prvom zobrazení
                if (cmbClickMode.SelectedItem == null)
                {
                    cmbClickMode.SelectedIndex = 0;
                }

                stepDescription.Text = "This step will click on the target element (usually a button or other clickable control).";

                // Update visibility of click options
                UpdateClickOptionsVisibility();
            }
            else if (action == "Wait")
            {
                valueGrid.Visibility = Visibility.Visible;
                valueLabel.Text = "Seconds:";
                grpClickOptions.Visibility = Visibility.Collapsed;
                stepDescription.Text = "This step will wait for the specified number of seconds before continuing to the next step.";
            }
        }

        private void CmbAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIBasedOnAction();
        }

        private void CmbClickMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClickOptionsVisibility();
        }

        private void UpdateClickOptionsVisibility()
        {
            // Skryť všetky ovládacie prvky najprv
            lblClickCount.Visibility = Visibility.Collapsed;
            txtClickCount.Visibility = Visibility.Collapsed;
            lblConditionElement.Visibility = Visibility.Collapsed;
            searchableCmbConditionElement.Visibility = Visibility.Collapsed;  // Oprava - používať searchableCmbConditionElement
            lblConditionValue.Visibility = Visibility.Collapsed;
            txtConditionValue.Visibility = Visibility.Collapsed;

            // Zobraziť relevantné ovládacie prvky podľa vybraného režimu
            if (cmbClickMode.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedMode = selectedItem.Tag.ToString();

                switch (selectedMode)
                {
                    case "MultipleClicks":
                        lblClickCount.Visibility = Visibility.Visible;
                        txtClickCount.Visibility = Visibility.Visible;
                        break;

                    case "ConditionalClicks":
                        lblConditionElement.Visibility = Visibility.Visible;
                        searchableCmbConditionElement.Visibility = Visibility.Visible;  // Oprava - používať searchableCmbConditionElement
                        lblConditionValue.Visibility = Visibility.Visible;
                        txtConditionValue.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        // Metóda pre kontrolu vstupu - len číselné hodnoty
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
