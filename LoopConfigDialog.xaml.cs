using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static PredefinedControlAndInsertionAppProject.MainWindow;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Interaction logic for LoopConfigDialog.xaml
    /// </summary>
    public partial class LoopConfigDialog : Window
    {
        public LoopControl LoopParameters { get; private set; }

        public LoopConfigDialog(IEnumerable<AppUIElement> availableElements, 
                                IEnumerable<SequenceStep>? selectedSteps = null, 
                                LoopControl? existingParams = null) 
        {
            InitializeComponent();

            LoopParameters = existingParams ?? new LoopControl();

            // Nastavte ItemsSource pre ComboBox
            cmbExitElement.ItemsSource = availableElements;

            // Ak existujú vybrané kroky, zobraziť ich v ListView
            if (selectedSteps != null && lvSelectedSteps != null)
            {
                lvSelectedSteps.ItemsSource = selectedSteps.ToList();
            }

            // Ak existuje existujúca konfigurácia, nastavte UI podľa nej
            if (existingParams != null)
            {
                if (existingParams.IsInfiniteLoop)
                {
                    if (!string.IsNullOrEmpty(existingParams.ExitConditionElementName))
                    {
                        // Nastavte condition-based loop
                        rbExitCondition.IsChecked = true;
                        txtExitValue.Text = existingParams.ExitConditionValue ?? "";

                        // Nájdite a nastavte vybraný element v ComboBoxe
                        foreach (var element in availableElements)
                        {
                            if (element.Name == existingParams.ExitConditionElementName)
                            {
                                cmbExitElement.SelectedItem = element;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Nastavte infinite loop
                        rbInfinite.IsChecked = true;
                    }
                }
                else
                {
                    // Nastavte fixed count loop
                    rbFixedCount.IsChecked = true;
                    txtIterationCount.Text = existingParams.IterationCount.ToString();
                }
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Update LoopParameters based on UI selections
            if (rbFixedCount.IsChecked == true)
            {
                LoopParameters.IsInfiniteLoop = false;
                LoopParameters.ExitConditionElementName = null;

                // Parse iteration count with validation
                if (int.TryParse(txtIterationCount.Text, out int count) && count > 0)
                {
                    LoopParameters.IterationCount = count;
                }
                else
                {
                    TimedMessageBox.Show("Please enter a valid iteration count", "Invalid Input", 5000);
                    return;
                }
            }
            else if (rbInfinite.IsChecked == true)
            {
                LoopParameters.IsInfiniteLoop = true;
                LoopParameters.ExitConditionElementName = null;
            }
            else if (rbExitCondition.IsChecked == true)
            {
                if (cmbExitElement.SelectedItem is AppUIElement selectedElement)
                {
                    LoopParameters.ExitConditionElementName = selectedElement.Name;
                    LoopParameters.ExitConditionValue = txtExitValue.Text;
                    LoopParameters.IsInfiniteLoop = true; // Loop until condition is met
                }
                else
                {
                    TimedMessageBox.Show("Please select an element for the exit condition", "Invalid Selection", 5000);
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
