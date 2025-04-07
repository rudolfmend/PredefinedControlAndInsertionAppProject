using System.IO;
using System.Windows;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Interaction logic for SaveConfigDialog.xaml
    /// </summary>
    public partial class SaveConfigDialog : Window
    {
        // Available configurations
        private readonly string[] _existingConfigs;

        // Result property
        public string ConfigurationName { get; private set; } = string.Empty;

        public SaveConfigDialog()
        {
            InitializeComponent();

            // Get existing configurations
            _existingConfigs = ConfigurationManager.GetAvailableConfigurations().ToArray();

            // Focus the text box
            txtConfigName.Focus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get the entered name
            string name = txtConfigName.Text.Trim();

            // Validate name
            if (string.IsNullOrEmpty(name))
            {
                ShowWarning("Please enter a name for the configuration.");
                return;
            }

            // Check for invalid characters
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ShowWarning("The name contains invalid characters. Please use only letters, numbers, and simple punctuation.");
                return;
            }

            // Check if the name already exists
            if (_existingConfigs.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                // Confirm overwrite
                var result = MessageBox.Show($"A configuration named '{name}' already exists. Do you want to overwrite it?",
                                           "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Set the result and close the dialog
            ConfigurationName = name;
            DialogResult = true;
            Close();
        }

        private void ShowWarning(string message)
        {
            txtWarning.Text = message;
            txtWarning.Visibility = Visibility.Visible;
        }
    }
}
