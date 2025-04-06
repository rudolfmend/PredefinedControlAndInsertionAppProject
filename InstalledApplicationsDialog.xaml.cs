using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PredefinedControlAndInsertionAppProject
{
    [SupportedOSPlatform("windows7.0")]

    /// <summary>
    /// Interaction logic for InstalledApplicationsDialog.xaml
    /// </summary>
    public partial class InstalledApplicationsDialog : Window
    {
        private List<InstalledApplicationInfo> _allApplications = new List<InstalledApplicationInfo>();
        private CollectionViewSource _applicationsViewSource;

        public InstalledApplicationsDialog()
        {
            InitializeComponent();

            // Nastaviť zdroj dát pre ListView
            _applicationsViewSource = new CollectionViewSource();
            _applicationsViewSource.Filter += ApplicationsViewSource_Filter;
            lvApplications.ItemsSource = _applicationsViewSource.View;

            // Načítať zoznam aplikácií
            LoadApplications();
        }

        private void LoadApplications()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                _allApplications = GetInstalledApplications();
                _applicationsViewSource.Source = _allApplications;
                _applicationsViewSource.View.Refresh();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void ApplicationsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not InstalledApplicationInfo app)
                return;

            string searchText = txtSearch.Text.Trim().ToLower();

            e.Accepted = string.IsNullOrEmpty(searchText) ||
                        app.Name.ToLower().Contains(searchText) ||
                        app.ExecutablePath.ToLower().Contains(searchText);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _applicationsViewSource.View.Refresh();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadApplications();
        }

        private void LvApplications_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnLaunch.IsEnabled = lvApplications.SelectedItem != null;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (lvApplications.SelectedItem is InstalledApplicationInfo selectedApp)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = selectedApp.ExecutablePath,
                        UseShellExecute = true
                    });

                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error launching application: {ex.Message}");
                    MessageBox.Show($"Error launching application: {ex.Message}", "Launch Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private List<InstalledApplicationInfo> GetInstalledApplications()
        {
            var installedApps = new List<InstalledApplicationInfo>();

            try
            {
                // Získať zoznam nainštalovaných aplikácií z Start menu
                string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs";
                string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs";

                // Získať súbory .lnk zo Start menu
                GetShortcutsFromDirectory(startMenuPath, installedApps);
                GetShortcutsFromDirectory(commonStartMenuPath, installedApps);

                // Získať aplikácie z registra Windows (Apps & Features)
                GetAppsFromRegistry(installedApps);

                return installedApps.OrderBy(app => app.Name).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting installed applications: {ex.Message}");

                MessageBox.Show($"Error getting installed applications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return installedApps;
            }
        }

        private void GetShortcutsFromDirectory(string directory, List<InstalledApplicationInfo> apps)
        {
            if (!Directory.Exists(directory))
                return;

            // Získať všetky .lnk súbory z adresára a podadresárov
            foreach (var file in Directory.GetFiles(directory, "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    // Pre prácu so zástupcami potrebujeme Shell COM objekt
                    if (OperatingSystem.IsWindows())
                    {
                        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                        if (shellType == null)
                            continue;

                        dynamic? shell = Activator.CreateInstance(shellType);
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

                        // Ignorovať zástupcov, ktorí neukazujú na .exe súbory
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
                }
                catch
                {
                    // Ignorovať neplatné zástupcov
                    Console.WriteLine($"Error reading shortcut: {file}");
                }
            }
        }

        private void GetAppsFromRegistry(List<InstalledApplicationInfo> apps)
        {
            // Prehľadať kľúč registra pre nainštalované aplikácie
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

                                    // Hľadať .exe súbory v inštalačnom adresári
                                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                    {
                                        string[] exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);

                                        if (exeFiles.Length > 0)
                                        {
                                            // Použiť prvý .exe súbor ako spustiteľný súbor
                                            exePath = exeFiles[0];
                                        }
                                    }
                                    // Alebo použiť DisplayIcon, ak ukazuje na .exe súbor
                                    else if (!string.IsNullOrEmpty(displayIcon) && displayIcon.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Odstrániť prípadný parameter ikony (napr. ",0")
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
                                // Ignorovať chyby pri konkrétnych kľúčoch registra
                                Console.WriteLine($"Error reading registry key: {subkeyName}");
                            }
                        }
                    }
                }
            }

            // Prehľadať aj 64-bitový kľúč registra (pre 32-bitové aplikácie na 64-bitovom systéme)
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

                                    // Hľadať .exe súbory v inštalačnom adresári
                                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                    {
                                        string[] exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);

                                        if (exeFiles.Length > 0)
                                        {
                                            // Použiť prvý .exe súbor ako spustiteľný súbor
                                            exePath = exeFiles[0];
                                        }
                                    }
                                    // Alebo použiť DisplayIcon, ak ukazuje na .exe súbor
                                    else if (!string.IsNullOrEmpty(displayIcon) && displayIcon.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Odstrániť prípadný parameter ikony (napr. ",0")
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
                                // Ignorovať chyby pri konkrétnych kľúčoch registra
                                Console.WriteLine($"Error reading registry key: {subkeyName}");
                            }
                        }
                    }
                }
            }
        }
    }
}
