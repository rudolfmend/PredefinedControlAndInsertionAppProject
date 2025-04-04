using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static PredefinedControlAndInsertionAppProject.MainWindow;
using System.Runtime.Versioning;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]

    /// <summary>
    /// Interaction logic for LoopConfigDialog.xaml
    /// </summary>
    public partial class LoopConfigDialog : Window
    {
        public LoopControl LoopParameters { get; private set; }

        public LoopConfigDialog(IEnumerable<UIElement> availableElements, LoopControl? existingParams = null)
        {
            LoopParameters = existingParams ?? new LoopControl();
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
                    MessageBox.Show("Please enter a valid iteration count", "Invalid Input");
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
                if (cmbExitElement.SelectedItem is UIElement selectedElement)
                {
                    LoopParameters.ExitConditionElementName = selectedElement.Name;
                    LoopParameters.ExitConditionValue = txtExitValue.Text;
                    LoopParameters.IsInfiniteLoop = true; // Loop until condition is met
                }
                else
                {
                    MessageBox.Show("Please select an element for the exit condition", "Invalid Selection");
                    return;
                }
            }

            DialogResult = true;
            Close();
        }
    }
}
