using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Management;
using System.Windows.Forms;

namespace PredefinedControlAndInsertionAppProject
{
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
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
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
                    System.Windows.MessageBox.Show($"Error launching application: {ex.Message}", "Launch Error",
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

                System.Windows.MessageBox.Show($"Error getting installed applications: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return installedApps;
            }
        }

        private void GetShortcutsFromDirectory(string directory, List<InstalledApplicationInfo> apps)
        {
            if (!Directory.Exists(directory))
                return;

            // Detekcia operačného systému Windows pomocou WMI
            string osName = "Unknown";
            string osVersion = "Unknown";
            int osMajorVersion = 0;
            int osMinorVersion = 0;
            bool isWindows10OrNewer = false;

            try
            {
                // Použitie WMI na zistenie operačného systému
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                using (var results = searcher.Get())
                {
                    foreach (var result in results)
                    {
                        osName = result["Caption"]?.ToString()?.Trim() ?? "Unknown";
                        osVersion = result["Version"]?.ToString()?.Trim() ?? "Unknown";

                        // Rozdelenie verzie na komponenty
                        string[] versionParts = osVersion.Split('.');
                        if (versionParts.Length >= 2)
                        {
                            int.TryParse(versionParts[0], out osMajorVersion);
                            int.TryParse(versionParts[1], out osMinorVersion);
                        }

                        // Kontrola, či je Windows 10 alebo novší
                        isWindows10OrNewer = osMajorVersion >= 10;

                        break;
                    }
                }

                Console.WriteLine($"Detected OS: {osName}, Version: {osVersion}, Major: {osMajorVersion}, Minor: {osMinorVersion}");
            }
            catch (Exception ex)
            {
                // Ak zlyhá WMI, použijeme fallback na Environment.OSVersion
                Console.WriteLine($"WMI detection failed: {ex.Message}. Using fallback method.");

                Version osVersionFallback = Environment.OSVersion.Version;
                osMajorVersion = osVersionFallback.Major;
                osMinorVersion = osVersionFallback.Minor;
                isWindows10OrNewer = osMajorVersion >= 10;

                Console.WriteLine($"Fallback OS detection: Major: {osMajorVersion}, Minor: {osMinorVersion}");
            }

            // Definícia search patterns - presunuté do vnútra metódy
            string[] searchPatterns = { "*.lnk" };

            // Pre Windows 10 a novšie môžeme hľadať aj ďalšie typy odkazov
            if (isWindows10OrNewer)
            {
                // Pre Windows 10 a novšie - hľadáme aj ďalšie typy odkazov
                searchPatterns = new[] { "*.lnk", "*.url" };
            }

            foreach (var pattern in searchPatterns)
            {
                foreach (var file in Directory.GetFiles(directory, pattern, SearchOption.AllDirectories))
                {
                    try
                    {
                        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                        if (shellType == null)
                            continue;

                        dynamic? shell = Activator.CreateInstance(shellType);
                        if (shell == null)
                            continue;

                        // Ještě přísnější null-check
                        dynamic? shortcut = null;
                        try
                        {
                            shortcut = shell.CreateShortcut(file);
                        }
                        catch
                        {
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                            continue;
                        }

                        if (shortcut == null)
                        {
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                            continue;
                        }

                        // Přidejte explicitní kontroly pro každou vlastnost
                        string? targetPath = null;
                        try
                        {
                            targetPath = shortcut.TargetPath as string;
                        }
                        catch
                        {
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                            continue;
                        }

                        string name = Path.GetFileNameWithoutExtension(file);

                        // Ještě přísnější kontrola cesty
                        bool isValidExe = !string.IsNullOrEmpty(targetPath) &&
                            Path.GetExtension(targetPath ?? "").Equals(".exe", StringComparison.OrdinalIgnoreCase);

                        bool isUwpApp = false;
                        if (isWindows10OrNewer && !string.IsNullOrEmpty(targetPath))
                        {
                            // Přidejte try-catch pro případ, že by přístup k vlastnostem selhal
                            try
                            {
                                isUwpApp = (targetPath ?? "").Contains("WindowsApps");
                            }
                            catch
                            {
                                isUwpApp = false;
                            }
                        }

                        if (isValidExe || isUwpApp)
                        {
                            apps.Add(new InstalledApplicationInfo
                            {
                                Name = name,
                                ExecutablePath = targetPath ?? string.Empty,
                                ShortcutPath = file
                            });
                        }

                        // Uvoľnenie COM objektov
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing shortcut {file}: {ex.Message}");
                    }
                }
            }

            // Dodatočné spracovanie pre Windows 10 a novšie - vyhľadávanie UWP aplikácií v Start Menu
            if (isWindows10OrNewer)
            {
                try
                {
                    // Cesta k Start Menu pre všetkých používateľov
                    string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");

                    // Vyhľadajte UWP odkazy, ktoré mohli byť vynechané vyššie
                    SearchUwpApplications(startMenuPath, apps);

                    // Cesta k Start Menu pre aktuálneho používateľa
                    string userStartMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
                    SearchUwpApplications(userStartMenuPath, apps);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching for UWP applications: {ex.Message}");
                }
            }
        }

        // Pomocná metóda pre vyhľadávanie UWP aplikácií
        private void SearchUwpApplications(string directory, List<InstalledApplicationInfo> apps)
        {
            // Zbytek kódu zůstává stejný jako v předchozí verzi
            if (!Directory.Exists(directory))
                return;

            foreach (var file in Directory.GetFiles(directory, "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    // Preskočiť, ak sme už tento súbor spracovali
                    if (apps.Any(a => a.ShortcutPath.Equals(file, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType == null)
                        continue;

                    dynamic? shell = Activator.CreateInstance(shellType);
                    if (shell == null)
                        continue;

                    dynamic? shortcut = shell?.CreateShortcut(file);
                    if (shortcut == null)
                    {
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                        continue;
                    }

                    string? targetPath = shortcut?.TargetPath as string;
                    string? arguments = shortcut?.Arguments as string;
                    string name = Path.GetFileNameWithoutExtension(file);

                    // UWP aplikácie často ukazujú na explorer.exe s argumentmi obsahujúcimi protokol shell:AppsFolder
                    bool isUwpLink = false;
                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        isUwpLink = (targetPath ?? "").Contains("WindowsApps") ||
                                    (!string.IsNullOrEmpty(arguments) && arguments.Contains("shell:AppsFolder"));
                    }

                    if (isUwpLink)
                    {
                        apps.Add(new InstalledApplicationInfo
                        {
                            Name = name,
                            ExecutablePath = targetPath ?? string.Empty,
                            ShortcutPath = file
                        });
                    }

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing UWP shortcut {file}: {ex.Message}");
                }
            }
        }

        private void GetAppsFromRegistry(List<InstalledApplicationInfo> apps)
        {
            // Prehľadať kľúč registra pre nainštalované aplikácie
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
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
                                            Name = displayName ?? "",
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
            var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            if (regKey != null)
            {
                using (var key = regKey)
                {
                    // Zabezpečíme, že key.GetSubKeyNames() vráti prázdne pole, ak by key bolo null
                    string[] subkeyNames = key?.GetSubKeyNames() ?? Array.Empty<string>();
                    foreach (var subkeyName in subkeyNames)
                    {
                        if (key == null)  // Explicitne skontrolujte, či key nie je null
                            continue;

                        var subKey = key.OpenSubKey(subkeyName);
                        if (subKey != null)
                        {
                            using (var subkey = subKey)
                            {
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
                                                Name = displayName ?? "",
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
}
