using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]

    /// <summary>
    /// Interaction logic for ListOfAllAppsDialog.xaml
    /// </summary>
    public partial class ListOfAllAppsDialog : Window
    {
        // Collections for running applications
        private ObservableCollection<ApplicationInfo> _runningApplications = new ObservableCollection<ApplicationInfo>();
        private CollectionViewSource _runningApplicationsViewSource;

        // Collections for installed applications
        private ObservableCollection<InstalledApplicationInfo> _installedApplications = new ObservableCollection<InstalledApplicationInfo>();
        private CollectionViewSource _installedApplicationsViewSource;

        // Collections for UWP applications
        private ObservableCollection<UwpApplicationInfo> _uwpApplications = new ObservableCollection<UwpApplicationInfo>();
        private CollectionViewSource _uwpApplicationsViewSource;

        // Currently active applications collection
        private enum ActiveAppList { Running, Installed, Uwp }
        private ActiveAppList _activeList = ActiveAppList.Running;

        // Selected application information
        public ApplicationInfo? SelectedApplication { get; private set; }
        public InstalledApplicationInfo? SelectedInstalledApplication { get; private set; }
        public UwpApplicationInfo? SelectedUwpApplication { get; private set; }

        // Service for detecting running applications
        private RunningApplicationsService _runningAppsService = new RunningApplicationsService();

        public ListOfAllAppsDialog()
        {
            InitializeComponent();

            // ***************************************
            // Na začiatku metódy InitializeComponent alebo v konštruktore:
            bool uwpApiAvailable = false;
            try
            {
                // Pokus o vytvorenie inštancie PackageManager pre kontrolu dostupnosti API
                var packageManager = new Windows.Management.Deployment.PackageManager();
                uwpApiAvailable = true;
            }
            catch
            {
                uwpApiAvailable = false;
            }

            // Len načítať UWP aplikácie, ak je API dostupné
            if (Environment.OSVersion.Version.Major >= 10 && uwpApiAvailable)
            {
                LoadUwpApplications();
            }
            else
            {
                // Deaktivovať UWP záložku
                foreach (TabItem item in tabApplications.Items)
                {
                    if (item.Header.ToString() == "Store Applications")
                    {
                        item.IsEnabled = false;
                        item.Header = "Store Applications (Windows 10+ only)";
                        break;
                    }
                }
            }
            // ***************************************

            // Initialize running applications view
            _runningApplicationsViewSource = new CollectionViewSource();
            _runningApplicationsViewSource.Source = _runningApplications;
            _runningApplicationsViewSource.Filter += RunningApplicationsViewSource_Filter;
            dgRunningApplications.ItemsSource = _runningApplicationsViewSource.View;

            // Initialize installed applications view
            _installedApplicationsViewSource = new CollectionViewSource();
            _installedApplicationsViewSource.Source = _installedApplications;
            _installedApplicationsViewSource.Filter += InstalledApplicationsViewSource_Filter;
            lvInstalledApplications.ItemsSource = _installedApplicationsViewSource.View;

            // Initialize UWP applications view
            _uwpApplicationsViewSource = new CollectionViewSource();
            _uwpApplicationsViewSource.Source = _uwpApplications;
            _uwpApplicationsViewSource.Filter += UwpApplicationsViewSource_Filter;
            lvUwpApplications.ItemsSource = _uwpApplicationsViewSource.View;

            // Load all application lists on startup
            LoadRunningApplications();
            LoadInstalledApplications();

            // Only load UWP apps if running on Windows 10 or newer
            if (Environment.OSVersion.Version.Major >= 10)
            {
                LoadUwpApplications();
            }
            else
            {
                // Disable UWP tab if not on Windows 10 or newer
                foreach (TabItem item in tabApplications.Items)
                {
                    if (item.Header.ToString() == "Store Applications")
                    {
                        item.IsEnabled = false;
                        item.Header = "Store Applications (Windows 10+ only)";
                        break;
                    }
                }
            }

            // Set initial state
            _activeList = ActiveAppList.Running;
            UpdateButtonState();
        }

        #region Running Applications

        private void LoadRunningApplications()
        {
            _runningApplications.Clear();

            try
            {
                // Get running applications
                var apps = _runningAppsService.GetRunningApplications();

                // Mark system processes
                foreach (var app in apps)
                {
                    app.SetIsSystemProcess(IsSystemProcess(Process.GetProcessById(app.ProcessId)));
                    _runningApplications.Add(app);
                }

                // Order by process name
                var ordered = _runningApplications.OrderBy(p => p.ProcessName).ToList();
                _runningApplications.Clear();
                foreach (var app in ordered)
                {
                    _runningApplications.Add(app);
                }

                // Refresh the view to apply initial filtering
                _runningApplicationsViewSource.View.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading running applications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunningApplicationsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not ApplicationInfo app)
                return;

            // Get filter text
            string filterText = txtFilterRunning.Text?.Trim().ToLower() ?? string.Empty;

            // Filter by name, process ID, or window title
            bool matchesFilter = string.IsNullOrEmpty(filterText) ||
                                app.ProcessName.ToLower().Contains(filterText) ||
                                app.ProcessId.ToString().Contains(filterText) ||
                                app.MainWindowTitle.ToLower().Contains(filterText);

            // Filter system processes if needed
            bool showSystemProcess = chkShowSystemProcesses.IsChecked ?? false;

            e.Accepted = matchesFilter && (showSystemProcess || !app.IsSystemProcess());
        }

        private bool IsSystemProcess(Process process)
        {
            string[] systemProcessNames = new[] {
                "explorer", "searchui", "shellexperiencehost", "startmenuexperiencehost",
                "applicationframehost", "systemsettings", "textinputhost", "runtimebroker",
                "winlogon", "wininit", "csrss", "services", "lsass", "svchost", "dwm",
                "smss", "ctfmon", "conhost", "taskhost", "taskhostw", "sihost", "fontdrvhost",
                "winstore.app", "backgroundtaskhost", "SecurityHealthService", "dllhost",
                "Registry", "System", "Idle", "Memory Compression", "ShellExperienceHost"
            };

            return systemProcessNames.Contains(process.ProcessName.ToLower());
        }

        private void TxtFilterRunning_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Refresh the view when the filter text changes
            _runningApplicationsViewSource.View.Refresh();
        }

        private void ChkShowSystemProcesses_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Refresh the view when the checkbox state changes
            _runningApplicationsViewSource.View.Refresh();
        }

        private void BtnRefreshRunning_Click(object sender, RoutedEventArgs e)
        {
            // Reload the applications list
            LoadRunningApplications();
        }

        private void DgRunningApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedApplication = dgRunningApplications.SelectedItem as ApplicationInfo;
            SelectedInstalledApplication = null;
            SelectedUwpApplication = null;
            UpdateButtonState();
        }

        #endregion

        #region Installed Applications

        private void LoadInstalledApplications()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                _installedApplications.Clear();

                // Get installed applications
                var apps = GetInstalledApplications();

                foreach (var app in apps)
                {
                    _installedApplications.Add(app);
                }

                _installedApplicationsViewSource.View.Refresh();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void InstalledApplicationsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not InstalledApplicationInfo app)
                return;

            string searchText = txtFilterInstalled.Text?.Trim().ToLower() ?? string.Empty;

            e.Accepted = string.IsNullOrEmpty(searchText) ||
                        app.Name.ToLower().Contains(searchText) ||
                        app.ExecutablePath.ToLower().Contains(searchText);
        }

        private void TxtFilterInstalled_TextChanged(object sender, TextChangedEventArgs e)
        {
            _installedApplicationsViewSource.View.Refresh();
        }

        private void BtnRefreshInstalled_Click(object sender, RoutedEventArgs e)
        {
            LoadInstalledApplications();
        }

        private void LvInstalledApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedInstalledApplication = lvInstalledApplications.SelectedItem as InstalledApplicationInfo;
            SelectedApplication = null;
            SelectedUwpApplication = null;
            UpdateButtonState();
        }

        private List<InstalledApplicationInfo> GetInstalledApplications()
        {
            var installedApps = new List<InstalledApplicationInfo>();

            try
            {
                // Get installed applications from Start menu
                string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs";
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs";

                // Get .lnk files from Start menu
                GetShortcutsFromDirectory(startMenuPath, installedApps);
                GetShortcutsFromDirectory(commonStartMenuPath, installedApps);

                // Get applications from Windows registry (Apps & Features)
                GetAppsFromRegistry(installedApps);

                return installedApps.OrderBy(app => app.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting installed applications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return installedApps;
            }
        }

        private void GetShortcutsFromDirectory(string directory, List<InstalledApplicationInfo> apps)
        {
            if (!Directory.Exists(directory))
                return;

            // Get all .lnk files from directory and subdirectories
            foreach (var file in Directory.GetFiles(directory, "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    // We need Shell COM object to work with shortcuts
                    Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType == null)
                        continue;

                    dynamic shell = Activator.CreateInstance(shellType);
                    if (shell == null)
                        continue;

                    dynamic shortcut = shell.CreateShortcut(file);
                    if (shortcut == null)
                    {
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                        continue;
                    }

                    string? targetPath = shortcut.TargetPath;
                    string name = Path.GetFileNameWithoutExtension(file);

                    // Ignore shortcuts that don't point to .exe files
                    if (!string.IsNullOrEmpty(targetPath) && Path.GetExtension(targetPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        apps.Add(new InstalledApplicationInfo
                        {
                            Name = name,
                            ExecutablePath = targetPath,
                            ShortcutPath = file
                        });
                    }

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                }
                catch
                {
                    // Ignore invalid shortcuts
                }
            }
        }

        private void GetAppsFromRegistry(List<InstalledApplicationInfo> apps)
        {
            // Process registry key for installed applications
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (key != null)
                {
                    foreach (var subkeyName in key.GetSubKeyNames())
                    {
                        using (var subkey = key.OpenSubKey(subkeyName))
                        {
                            if (subkey == null)
                                continue;

                            try
                            {
                                string? displayName = subkey.GetValue("DisplayName") as string;
                                string? installLocation = subkey.GetValue("InstallLocation") as string;
                                string? displayIcon = subkey.GetValue("DisplayIcon") as string;

                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    string exePath = "";

                                    // Look for .exe files in installation directory
                                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                    {
                                        string[] exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);

                                        if (exeFiles.Length > 0)
                                        {
                                            // Use first .exe file as executable
                                            exePath = exeFiles[0];
                                        }
                                    }
                                    // Or use DisplayIcon if it points to an .exe file
                                    else if (!string.IsNullOrEmpty(displayIcon) && displayIcon.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Remove icon parameter (e.g., ",0")
                                        string[] iconParts = displayIcon.Split(',');
                                        exePath = iconParts[0].Trim();
                                    }

                                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                                    {
                                        apps.Add(new InstalledApplicationInfo
                                        {
                                            Name = displayName,
                                            ExecutablePath = exePath
                                        });
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore errors with specific registry keys
                            }
                        }
                    }
                }
            }

            // Also process 64-bit registry key (for 32-bit apps on 64-bit system)
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (key != null)
                {
                    foreach (var subkeyName in key.GetSubKeyNames())
                    {
                        using (var subkey = key.OpenSubKey(subkeyName))
                        {
                            if (subkey == null)
                                continue;

                            try
                            {
                                string? displayName = subkey.GetValue("DisplayName") as string;
                                string? installLocation = subkey.GetValue("InstallLocation") as string;
                                string? displayIcon = subkey.GetValue("DisplayIcon") as string;

                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    string exePath = "";

                                    // Look for .exe files in installation directory
                                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                    {
                                        string[] exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);

                                        if (exeFiles.Length > 0)
                                        {
                                            // Use first .exe file as executable
                                            exePath = exeFiles[0];
                                        }
                                    }
                                    // Or use DisplayIcon if it points to an .exe file
                                    else if (!string.IsNullOrEmpty(displayIcon) && displayIcon.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Remove icon parameter (e.g., ",0")
                                        string[] iconParts = displayIcon.Split(',');
                                        exePath = iconParts[0].Trim();
                                    }

                                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                                    {
                                        apps.Add(new InstalledApplicationInfo
                                        {
                                            Name = displayName,
                                            ExecutablePath = exePath
                                        });
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore errors with specific registry keys
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region UWP Applications

        private void LoadUwpApplications()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                _uwpApplications.Clear();

                // Get UWP applications
                var apps = UwpApplicationHelper.GetInstalledUwpApplications();

                foreach (var app in apps)
                {
                    _uwpApplications.Add(app);
                }

                _uwpApplicationsViewSource.View.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading UWP applications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void UwpApplicationsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not UwpApplicationInfo app)
                return;

            string searchText = txtFilterUwp.Text?.Trim().ToLower() ?? string.Empty;

            e.Accepted = string.IsNullOrEmpty(searchText) ||
                        app.Name.ToLower().Contains(searchText) ||
                        app.Publisher.ToLower().Contains(searchText) ||
                        app.Description.ToLower().Contains(searchText) ||
                        app.AppUserModelId.ToLower().Contains(searchText);
        }

        private void TxtFilterUwp_TextChanged(object sender, TextChangedEventArgs e)
        {
            _uwpApplicationsViewSource.View.Refresh();
        }

        private void BtnRefreshUwp_Click(object sender, RoutedEventArgs e)
        {
            LoadUwpApplications();
        }

        private void LvUwpApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedUwpApplication = lvUwpApplications.SelectedItem as UwpApplicationInfo;
            SelectedApplication = null;
            SelectedInstalledApplication = null;
            UpdateButtonState();

            // Update details panel
            if (SelectedUwpApplication != null)
            {
                txtAppUserModelId.Text = SelectedUwpApplication.AppUserModelId;
                txtPackageFullName.Text = SelectedUwpApplication.PackageFullName;
                txtInstalledDate.Text = SelectedUwpApplication.InstalledDate.ToString("g");
            }
            else
            {
                txtAppUserModelId.Text = "";
                txtPackageFullName.Text = "";
                txtInstalledDate.Text = "";
            }
        }

        #endregion

        #region Common functionality

        private void TabApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update active list based on selected tab
            if (tabApplications.SelectedIndex == 0)
            {
                _activeList = ActiveAppList.Running;
            }
            else if (tabApplications.SelectedIndex == 1)
            {
                _activeList = ActiveAppList.Installed;
            }
            else if (tabApplications.SelectedIndex == 2)
            {
                _activeList = ActiveAppList.Uwp;

                // Load UWP apps if not already loaded
                if (_uwpApplications.Count == 0 && Environment.OSVersion.Version.Major >= 10)
                {
                    LoadUwpApplications();
                }
            }

            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            // Enable or disable the Select and Launch buttons based on selection
            bool hasSelection = false;

            switch (_activeList)
            {
                case ActiveAppList.Running:
                    hasSelection = SelectedApplication != null;
                    break;
                case ActiveAppList.Installed:
                    hasSelection = SelectedInstalledApplication != null;
                    break;
                case ActiveAppList.Uwp:
                    hasSelection = SelectedUwpApplication != null;
                    break;
            }

            btnSelect.IsEnabled = hasSelection;
            btnLaunchApp.IsEnabled = hasSelection;

            // Update button text based on context
            if (_activeList == ActiveAppList.Running)
            {
                btnLaunchApp.Content = "Activate Window";
            }
            else
            {
                btnLaunchApp.Content = "Launch Application";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Set DialogResult to false and close
            DialogResult = false;
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (_activeList == ActiveAppList.Running && SelectedApplication != null)
            {
                // Set DialogResult to true and close
                DialogResult = true;
            }
            else if (_activeList == ActiveAppList.Installed && SelectedInstalledApplication != null)
            {
                // For installed app, try to find a running instance
                // If not found, the calling code can handle launching it
                MessageBox.Show("This will select the installed application. If it's not running, you may need to launch it first.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            else if (_activeList == ActiveAppList.Uwp && SelectedUwpApplication != null)
            {
                // For UWP app, try to find a running instance 
                // If not found, the calling code can handle launching it
                MessageBox.Show("This will select the UWP application. If it's not running, you may need to launch it first.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
        }

        private async void BtnLaunchApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_activeList == ActiveAppList.Running && SelectedApplication != null)
                {
                    // Activate existing running application
                    Process process = Process.GetProcessById(SelectedApplication.ProcessId);

                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                    }
                    else
                    {
                        MessageBox.Show("The selected application does not have a visible window to activate.",
                            "Cannot Launch", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (_activeList == ActiveAppList.Installed && SelectedInstalledApplication != null)
                {
                    // Launch installed application
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedInstalledApplication.ExecutablePath,
                        UseShellExecute = true
                    });
                }
                else if (_activeList == ActiveAppList.Uwp && SelectedUwpApplication != null)
                {
                    // Launch UWP application
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        bool result = await UwpApplicationHelper.LaunchUwpApplicationAsync(SelectedUwpApplication.AppUserModelId);

                        if (!result)
                        {
                            MessageBox.Show("Failed to launch the UWP application. The app might be restricted or unavailable.",
                                "Launch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching application: {ex.Message}", "Launch Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
