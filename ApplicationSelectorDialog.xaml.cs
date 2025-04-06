using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Interaction logic for ApplicationSelectorDialog.xaml
    /// </summary>
    public partial class ApplicationSelectorDialog : Window
    {
        private readonly RunningApplicationsService _appService;
        private List<ApplicationInfo> _runningApps;

        // Result properties
        public List<UIElementInfo> SelectedElements { get; private set; } = new List<UIElementInfo>();
        public ApplicationInfo? SelectedApplication { get; private set; }
        public WindowInfo? SelectedWindow { get; private set; }

        public ApplicationSelectorDialog()
        {
            InitializeComponent();

            // Initialize services
            _appService = new RunningApplicationsService();
            _runningApps = new List<ApplicationInfo>();

            // Load running applications
            LoadRunningApplications();
        }

        private void LoadRunningApplications()
        {
            try
            {
                // Show loading indicator
                Mouse.OverrideCursor = Cursors.Wait;

                // Get running applications
                _runningApps = _appService.GetRunningApplications();

                // Sort the list by process name
                _runningApps = _runningApps
                    .OrderBy(a => a.ProcessName)
                    .ThenBy(a => a.MainWindowTitle)
                    .ToList();

                // Update the list view
                lvApplications.ItemsSource = _runningApps;

                // Clear windows list
                lvWindows.ItemsSource = null;

                // Update UI state
                BtnSelect.IsEnabled = false;
                BtnShowElements.IsEnabled = false;
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error loading running applications: {ex.Message}",
                                "Error", 5000);
            }
            finally
            {
                // Hide loading indicator
                Mouse.OverrideCursor = null;
            }
        }

        private void LoadWindowsForApplication(ApplicationInfo application)
        {
            try
            {
                // Show loading indicator
                Mouse.OverrideCursor = Cursors.Wait;

                // Get windows for the selected application
                var windows = _appService.GetWindowsForProcess(application.ProcessId);

                // Sort by window title
                windows = windows.OrderBy(w => w.WindowTitle).ToList();

                // Update the list view
                lvWindows.ItemsSource = windows;

                // Update UI state
                BtnShowElements.IsEnabled = false;
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error loading windows for application: {ex.Message}",
                                "Error", 5000);
            }
            finally
            {
                // Hide loading indicator
                Mouse.OverrideCursor = null;
            }
        }

        private void ShowElementsForWindow(WindowInfo window)
        {
            try
            {
                // Get UI elements for the window
                var elements = _appService.GetUIElementsForWindow(window.WindowHandle);

                if (elements.Count == 0)
                {
                    TimedMessageBox.Show("No interactive UI elements found in this window.",
                                  "No Elements", 5000);
                    return;
                }

                // Create a dialog to display the elements
                var elementsDialog = new ElementsPreviewDialog(elements, window.WindowTitle);
                elementsDialog.Owner = this;
                elementsDialog.ShowDialog();

                // Check if any elements were selected
                if (elementsDialog.SelectedElements.Count > 0)
                {
                    // Store selected elements to pass back to the main window
                    SelectedElements = elementsDialog.SelectedElements;

                    // Optionally auto-close the dialog if elements were selected
                    if (MessageBox.Show("Elements have been selected. Do you want to close this dialog and add them to your automation?",
                                      "Elements Selected", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                TimedMessageBox.Show($"Error detecting UI elements: {ex.Message}",
                              "Error", 5000);
            }
        }

        #region Event Handlers

        private void lvApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvApplications.SelectedItem is ApplicationInfo selectedApp)
            {
                // Load windows for the selected application
                LoadWindowsForApplication(selectedApp);

                // Update selected application
                SelectedApplication = selectedApp;
                SelectedWindow = null;

                // Update UI state
                BtnSelect.IsEnabled = false;
            }
            else
            {
                // Clear selection
                SelectedApplication = null;
                SelectedWindow = null;

                // Update UI state
                BtnSelect.IsEnabled = false;
                BtnShowElements.IsEnabled = false;
            }
        }

        private void lvWindows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvWindows.SelectedItem is WindowInfo selectedWindow)
            {
                // Update selected window
                SelectedWindow = selectedWindow;

                // Update UI state
                BtnSelect.IsEnabled = true;
                BtnShowElements.IsEnabled = true;
            }
            else
            {
                // Clear window selection
                SelectedWindow = null;

                // Update UI state
                BtnSelect.IsEnabled = false;
                BtnShowElements.IsEnabled = false;
            }
        }

        private void lvWindows_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Skontrolujte, či udalosť pochádza z platného ListView item
            if (lvWindows.SelectedItem is WindowInfo selectedWindow)
            {
                // Aktualizujte vybrané okno
                SelectedWindow = selectedWindow;

                // Zavolajte tú istú funkciu, ktorá sa volá pri kliknutí na tlačidlo
                ShowElementsForWindow(selectedWindow);
            }
        }

        private void BtnShowElements_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWindow != null)
            {
                ShowElementsForWindow(SelectedWindow);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadRunningApplications();
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedApplication != null && SelectedWindow != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                TimedMessageBox.Show("Please select both an application and a window.",
                                "Selection Required", 5000);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}
