using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]

    /// <summary>
    /// Interaction logic for LoadConfigDialog.xaml
    /// </summary>
    public partial class LoadConfigDialog : Window
    {
        // Result property
        public string SelectedConfiguration { get; private set; } = string.Empty;

        public LoadConfigDialog()
        {
            InitializeComponent();

            // Load available configurations
            LoadConfigurations();
        }

        private void LoadConfigurations()
        {
            // Get available configurations
            List<string> configs = ConfigurationManager.GetAvailableConfigurations();

            if (configs.Count == 0)
            {
                // No configurations found
                lbConfigurations.Items.Add("No saved configurations found.");
                lbConfigurations.IsEnabled = false;
                BtnLoad.IsEnabled = false;
                BtnDelete.IsEnabled = false;
            }
            else
            {
                // Sort configurations alphabetically
                configs = configs.OrderBy(c => c).ToList();

                // Clear and add items
                lbConfigurations.Items.Clear();
                foreach (string config in configs)
                {
                    lbConfigurations.Items.Add(config);
                }

                lbConfigurations.IsEnabled = true;
            }
        }

        private void LbConfigurations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable/disable buttons based on selection
            bool hasSelection = lbConfigurations.SelectedItem != null &&
                               lbConfigurations.IsEnabled;

            BtnLoad.IsEnabled = hasSelection;
            BtnDelete.IsEnabled = hasSelection;
        }

        private void LbConfigurations_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbConfigurations.SelectedItem != null && lbConfigurations.IsEnabled)
            {
                LoadSelectedConfiguration();
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedConfiguration();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Get selected configuration
            string? config = lbConfigurations.SelectedItem as string;

            if (string.IsNullOrEmpty(config) || !lbConfigurations.IsEnabled)
                return;

            // Confirm deletion
            var result = MessageBox.Show($"Are you sure you want to delete the configuration '{config}'?",
                                       "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Delete the configuration file
                string configPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PredefinedControlApp",
                    $"{config}.json");

                if (File.Exists(configPath))
                {
                    File.Delete(configPath);

                    // Reload configurations
                    LoadConfigurations();

                    TimedMessageBox.Show($"Configuration '{config}' has been deleted.",
                                  "Configuration Deleted", 5000);
                }
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error deleting configuration: {ex.Message}",
                              "Delete Error", 5000);
            }
        }

        private void LoadSelectedConfiguration()
        {
            // Get selected configuration
            string? config = lbConfigurations.SelectedItem as string;

            if (string.IsNullOrEmpty(config) || !lbConfigurations.IsEnabled)
                return;

            // Set the result and close the dialog
            SelectedConfiguration = config;
            DialogResult = true;
            Close();
        }
    }
}
