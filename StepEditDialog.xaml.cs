using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
        public UIElement SelectedTarget { get; private set; } = new UIElement();
        public string Value { get; private set; } = string.Empty;

        public StepEditDialog(IEnumerable<UIElement> availableElements, SequenceStep? currentStep = null)
        {
            InitializeComponent();

            // Populate target elements ComboBox
            cmbTarget.ItemsSource = availableElements;

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

                // Set target and value if provided
                if (currentStep.Target != "Select a UI Element")
                {
                    foreach (UIElement element in availableElements)
                    {
                        if (element.Name == currentStep.Target)
                        {
                            cmbTarget.SelectedItem = element;
                            break;
                        }
                    }
                }

                // Set value (for Textbox or Wait actions)
                if (currentStep.Action == "SetValue" || currentStep.Action == "Wait")
                {
                    if (cmbTarget.SelectedItem is UIElement target)
                    {
                        txtValue.Text = target.Value;
                    }
                }
            }

            // Set initial UI state
            UpdateUIBasedOnAction();

            // Add event handlers
            cmbAction.SelectionChanged += CmbAction_SelectionChanged;
            cmbTarget.SelectionChanged += CmbTarget_SelectionChanged;
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
                stepDescription.Text = "This step will set a value in the target element (usually a textbox or other input field).";
            }
            else if (action == "Click")
            {
                valueGrid.Visibility = Visibility.Collapsed;
                stepDescription.Text = "This step will click on the target element (usually a button or other clickable control).";
            }
            else if (action == "Wait")
            {
                valueGrid.Visibility = Visibility.Visible;
                valueLabel.Text = "Seconds:";
                stepDescription.Text = "This step will wait for the specified number of seconds before continuing to the next step.";
            }
        }

        private void CmbAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIBasedOnAction();
        }

        private void CmbTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTarget.SelectedItem is UIElement selectedElement &&
                cmbAction.SelectedItem is ComboBoxItem actionItem &&
                actionItem.Content.ToString() == "SetValue")
            {
                // Automatically populate the value from the selected element
                txtValue.Text = selectedElement.Value;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (cmbAction.SelectedItem == null)
            {
                MessageBox.Show("Please select an action.", "Missing Input",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string action = (cmbAction.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

            if (action != "Wait" && cmbTarget.SelectedItem == null)
            {
                MessageBox.Show("Please select a target element.", "Missing Input",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (action == "Wait" && (string.IsNullOrEmpty(txtValue.Text) || !double.TryParse(txtValue.Text, out _)))
            {
                MessageBox.Show("Please enter a valid number of seconds for the wait time.",
                                "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Store results
            SelectedAction = action;
            SelectedTarget = cmbTarget.SelectedItem as UIElement ?? new UIElement();
            Value = txtValue.Text;

            // Close dialog with success
            DialogResult = true;
            Close();
        }
    }
}
