using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PredefinedControlAndInsertionAppProject
{
    public static class ApplicationFinder
    {
        /// <summary>
        /// Finds all applications on the system, including hidden ones
        /// </summary>
        public static List<ApplicationInfoBase> FindAllApplications(bool includeSystemApps = false, bool includeHiddenApps = false)
        {
            var allApps = new List<ApplicationInfoBase>();

            // Add running applications
            allApps.AddRange(FindRunningApplications(includeSystemApps));

            // Add installed desktop applications
            allApps.AddRange(FindInstalledDesktopApplications(includeSystemApps, includeHiddenApps));

            // Add UWP applications if on Windows 10+
            if (Environment.OSVersion.Version.Major >= 10)
            {
                try
                {
                    allApps.AddRange(FindUwpApplications(includeSystemApps));
                }
                catch
                {
                    Console.WriteLine("Error accessing UWP applications.");
                    // UWP API not available or failed
                }
            }

            // Remove duplicates based on executable path or AppUserModelId
            return RemoveDuplicates(allApps);
        }

        /// <summary>
        /// Finds running applications
        /// </summary>
        public static List<DesktopApplicationInfo> FindRunningApplications(bool includeSystemApps = false)
        {
            var apps = new List<DesktopApplicationInfo>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    // Skip processes without a main window and not specifically looking for hidden apps
                    if (process.MainWindowHandle == IntPtr.Zero)
                        continue;

                    bool isSystemApp = IsSystemProcess(process);
                    if (isSystemApp && !includeSystemApps)
                        continue;

                    var app = new DesktopApplicationInfo
                    {
                        Name = !string.IsNullOrEmpty(process.MainWindowTitle) ? process.MainWindowTitle : process.ProcessName,
                        ExecutablePath = GetProcessPath(process),
                        ProcessId = process.Id,
                        WindowTitle = process.MainWindowTitle,
                        WindowHandle = process.MainWindowHandle,
                        IsSystemApplication = isSystemApp
                    };

                    apps.Add(app);
                }
                catch
                {
                    Console.WriteLine($"Error accessing process {process.ProcessName}: {process.Id}");
                    // Skip processes that can't be accessed
                }
            }

            return apps;
        }

        /// <summary>
        /// Searches the system for installed applications using registry and known paths
        /// </summary>
        public static List<DesktopApplicationInfo> FindInstalledDesktopApplications(bool includeSystemApps = false, bool includeHiddenApps = false)
        {
            var apps = new List<DesktopApplicationInfo>();

            // Search Start Menu
            SearchStartMenuApplications(apps, includeSystemApps);

            // Search Program Files directories
            SearchProgramFilesApplications(apps, includeSystemApps);

            // Search registry for installed applications
            SearchRegistryApplications(apps, includeSystemApps, includeHiddenApps);

            return apps;
        }

        /// <summary>
        /// Finds UWP applications if on Windows 10+
        /// </summary>
        public static List<UwpApplicationInfo> FindUwpApplications(bool includeSystemApps = false)
        {
            var apps = new List<UwpApplicationInfo>();

            if (Environment.OSVersion.Version.Major < 10)
                return apps;

            try
            {
                var uwpApps = UwpApplicationHelper.GetInstalledUwpApplications();

                foreach (var uwpApp in uwpApps)
                {
                    // Skip system apps if not requested
                    if (IsSystemUwpApp(uwpApp) && !includeSystemApps)
                        continue;

                    apps.Add(uwpApp);
                }
            }
            catch
            {
                Console.WriteLine("Error accessing UWP applications.");
                // UWP API not available
            }

            return apps;
        }

        // Helper methods for searching different sources and checking system apps
        private static void SearchStartMenuApplications(List<DesktopApplicationInfo> apps, bool includeSystemApps)
        {
            try
            {
                // Získať cesty k priečinkom Štart Menu
                string userStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\Programs";
                string commonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) + "\\Programs";

                // Hľadanie zástupcov v používateľskom priečinku Start Menu
                if (System.IO.Directory.Exists(userStartMenu))
                {
                    SearchDirectoryForShortcuts(userStartMenu, apps, includeSystemApps);
                }

                // Hľadanie zástupcov v spoločnom priečinku Start Menu
                if (System.IO.Directory.Exists(commonStartMenu))
                {
                    SearchDirectoryForShortcuts(commonStartMenu, apps, includeSystemApps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching Start Menu: {ex.Message}");
            }
        }

        private static void SearchDirectoryForShortcuts(string directory, List<DesktopApplicationInfo> apps, bool includeSystemApps)
        {
            // Hľadanie všetkých .lnk súborov v priečinku a podadresároch
            foreach (var file in System.IO.Directory.GetFiles(directory, "*.lnk", System.IO.SearchOption.AllDirectories))
            {
                try
                {
                    // Použitie WScript.Shell na prácu so zástupcami
                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
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

                    string targetPath = shortcut.TargetPath;
                    string name = System.IO.Path.GetFileNameWithoutExtension(file);

                    // Ignorovať zástupcov, ktorí neukazujú na .exe súbory
                    if (!string.IsNullOrEmpty(targetPath) && System.IO.Path.GetExtension(targetPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        var appInfo = new DesktopApplicationInfo
                        {
                            Name = name,
                            ExecutablePath = targetPath,
                            ShortcutPath = file,
                            IsSystemApplication = IsSystemFolder(file)
                        };

                        // Pridať aplikáciu len ak nie je systémová alebo užívateľ chce aj systémové
                        if (!appInfo.IsSystemApplication || includeSystemApps)
                        {
                            apps.Add(appInfo);
                        }
                    }

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing shortcut {file}: {ex.Message}");
                }
            }
        }

        private static bool IsSystemFolder(string path)
        {
            // Kontrola, či je priečinok v systémovej oblasti
            string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string commonProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);

            return path.StartsWith(windowsPath, StringComparison.OrdinalIgnoreCase) ||
                   (path.StartsWith(programFilesPath, StringComparison.OrdinalIgnoreCase) &&
                    path.IndexOf("\\Windows\\", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void SearchRegistryApplications(List<DesktopApplicationInfo> apps, bool includeSystemApps, bool includeHiddenApps)
        {
            try
            {
                // Hľadať v 64-bitovom registri
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (key != null)
                    {
                        foreach (var subkeyName in key.GetSubKeyNames())
                        {
                            ProcessRegistryKey(key, subkeyName, apps, includeSystemApps, includeHiddenApps);
                        }
                    }
                }

                // Hľadať v 32-bitovom registri na 64-bitovom Windows
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (key != null)
                    {
                        foreach (var subkeyName in key.GetSubKeyNames())
                        {
                            ProcessRegistryKey(key, subkeyName, apps, includeSystemApps, includeHiddenApps);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching registry: {ex.Message}");
            }
        }

        private static void ProcessRegistryKey(Microsoft.Win32.RegistryKey parentKey, string subkeyName,
                                              List<DesktopApplicationInfo> apps, bool includeSystemApps, bool includeHiddenApps)
        {
            try
            {
                using (var subkey = parentKey.OpenSubKey(subkeyName))
                {
                    if (subkey == null) return;

                    // Získať hodnoty z registra
                    string? displayName = subkey.GetValue("DisplayName") as string;
                    string? installLocation = subkey.GetValue("InstallLocation") as string;
                    string? uninstallString = subkey.GetValue("UninstallString") as string;
                    string? displayIcon = subkey.GetValue("DisplayIcon") as string;
                    string? systemComponent = subkey.GetValue("SystemComponent") as string;

                    // Preskočiť položky bez názvu
                    if (string.IsNullOrEmpty(displayName))
                        return;

                    // Kontrola, či je to systémová komponenta
                    bool isSystem = systemComponent == "1";
                    if (isSystem && !includeSystemApps)
                        return;

                    // Zistiť cestu k exe súboru
                    string exePath = GetExePathFromRegistry(installLocation ?? string.Empty, uninstallString ?? string.Empty, displayIcon ?? string.Empty);

                    if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
                    {
                        // Kontrola, či je súbor skrytý
                        bool isHidden = (System.IO.File.GetAttributes(exePath) & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
                        if (isHidden && !includeHiddenApps)
                            return;

                        var appInfo = new DesktopApplicationInfo
                        {
                            Name = displayName ?? string.Empty,
                            ExecutablePath = exePath,
                            IsSystemApplication = isSystem,
                            IsHidden = isHidden
                        };

                        apps.Add(appInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing registry key {subkeyName}: {ex.Message}");
            }
        }

        private static string GetExePathFromRegistry(string installLocation, string uninstallString, string displayIcon)
        {
            // Najprv skúsiť DisplayIcon, často obsahuje cestu k exe súboru
            if (!string.IsNullOrEmpty(displayIcon) && displayIcon.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                // Odstrániť parameter ikony (napr. ",0")
                int commaIndex = displayIcon.IndexOf(',');
                if (commaIndex > 0)
                    displayIcon = displayIcon.Substring(0, commaIndex);

                displayIcon = displayIcon.Trim('"', ' ');

                if (System.IO.File.Exists(displayIcon))
                    return displayIcon;
            }

            // Skúsiť hľadať exe súbory v inštalačnom adresári
            if (!string.IsNullOrEmpty(installLocation) && System.IO.Directory.Exists(installLocation))
            {
                var exeFiles = System.IO.Directory.GetFiles(installLocation, "*.exe", System.IO.SearchOption.TopDirectoryOnly);
                if (exeFiles.Length > 0)
                    return exeFiles[0];
            }

            // Skúsiť extrahovať cestu z UninstallString
            if (!string.IsNullOrEmpty(uninstallString))
            {
                string path = uninstallString.Trim('"', ' ');

                // Hľadať prvý výskyt exe
                int exeIndex = path.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                if (exeIndex > 0)
                {
                    path = path.Substring(0, exeIndex + 4);

                    if (System.IO.File.Exists(path))
                        return path;
                }
            }

            return string.Empty;
        }

        private static bool IsSystemProcess(Process process)
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

        private static bool IsSystemUwpApp(UwpApplicationInfo app)
        {
            // Zoznam vydavateľov systémových UWP aplikácií
            string[] systemPublishers = new[] {
                "CN=Microsoft Corporation",
                "CN=Microsoft Windows",
                "CN=Microsoft.WindowsStore"
            };

            // Zoznam prefixov názvov systémových UWP aplikácií
            string[] systemAppPrefixes = new[] {
                "Microsoft.",
                "Windows.",
                "MSWindows"
            };

            // Kontrola či vydavateľ je v zozname systémových vydavateľov
            if (systemPublishers.Any(publisher => app.Publisher.Contains(publisher)))
                return true;

            // Kontrola či názov začína niektorým zo systémových prefixov
            if (systemAppPrefixes.Any(prefix => app.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        private static string GetProcessPath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? "";
            }
            catch
            {
                // Nemáme prístupové práva na získanie cesty k modulu procesu
                Console.WriteLine($"Error accessing process path for {process.ProcessName}: {process.Id}");
                return "";
            }
        }

        private static void SearchProgramFilesApplications(List<DesktopApplicationInfo> apps, bool includeSystemApps)
        {
            try
            {
                // Získať cesty k priečinkom Program Files
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                // Hľadanie .exe súborov v Program Files
                if (System.IO.Directory.Exists(programFiles))
                {
                    SearchDirectoryForExecutables(programFiles, apps, includeSystemApps);
                }

                // Hľadanie .exe súborov v Program Files (x86)
                if (System.IO.Directory.Exists(programFilesX86) && programFilesX86 != programFiles)
                {
                    SearchDirectoryForExecutables(programFilesX86, apps, includeSystemApps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching Program Files: {ex.Message}");
            }
        }

        private static void SearchDirectoryForExecutables(string directory, List<DesktopApplicationInfo> apps, bool includeSystemApps)
        {
            // Získať všetky podpriečinky prvej úrovne (napr. adresáre výrobcov)
            var vendorDirs = System.IO.Directory.GetDirectories(directory);

            foreach (var vendorDir in vendorDirs)
            {
                try
                {
                    bool isSystemDir = IsSystemFolder(vendorDir);

                    // Preskočiť systémové priečinky, ak užívateľ nechce systémové aplikácie
                    if (isSystemDir && !includeSystemApps)
                        continue;

                    // Hľadať exe súbory v podpriečinkoch
                    var exeFiles = System.IO.Directory.GetFiles(vendorDir, "*.exe", System.IO.SearchOption.AllDirectories);

                    foreach (var exeFile in exeFiles)
                    {
                        try
                        {
                            // Zistiť, či súbor má atribút skrytý
                            bool isHidden = (System.IO.File.GetAttributes(exeFile) & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;

                            // Ignorovať pomocné exe súbory v resources/bin priečinkoch
                            if (exeFile.Contains("\\resources\\") || exeFile.Contains("\\bin\\"))
                                continue;

                            // Vytvoriť info o aplikácii
                            var appInfo = new DesktopApplicationInfo
                            {
                                Name = System.IO.Path.GetFileNameWithoutExtension(exeFile),
                                ExecutablePath = exeFile,
                                IsSystemApplication = isSystemDir,
                                IsHidden = isHidden
                            };

                            apps.Add(appInfo);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing executable {exeFile}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error accessing vendor directory {vendorDir}: {ex.Message}");
                }
            }
        }

        private static List<ApplicationInfoBase> RemoveDuplicates(List<ApplicationInfoBase> apps)
        {
            // Použiť Dictionary na identifikáciu duplikátov
            var uniqueApps = new Dictionary<string, ApplicationInfoBase>();

            foreach (var app in apps)
            {
                string key;

                if (app is DesktopApplicationInfo desktopApp)
                {
                    // Pre desktopové aplikácie použiť cestu k exe súboru ako kľúč
                    if (string.IsNullOrEmpty(desktopApp.ExecutablePath))
                        continue;

                    key = desktopApp.ExecutablePath.ToLower();
                }
                else if (app is UwpApplicationInfo uwpApp)
                {
                    // Pre UWP aplikácie použiť AppUserModelId ako kľúč
                    if (string.IsNullOrEmpty(uwpApp.AppUserModelId))
                        continue;

                    key = uwpApp.AppUserModelId.ToLower();
                }
                else
                {
                    // Pre iné typy použiť ToString
                    key = app.ToString().ToLower();
                }

                // Ak aplikácia s týmto kľúčom už existuje, uprednostniť tú s ProcessId (bežiacu)
                if (uniqueApps.TryGetValue(key, out var existingApp))
                {
                    if (app is DesktopApplicationInfo newDesktopApp &&
                        existingApp is DesktopApplicationInfo existingDesktopApp)
                    {
                        // Uprednostniť bežiacu aplikáciu
                        if (newDesktopApp.ProcessId.HasValue && !existingDesktopApp.ProcessId.HasValue)
                        {
                            uniqueApps[key] = app;
                        }
                    }
                }
                else
                {
                    uniqueApps[key] = app;
                }
            }

            return uniqueApps.Values.ToList();
        }
    }
}
