using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.System;
using Windows.ApplicationModel;
using System.Windows;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Helper class for working with UWP/Windows Store applications
    /// </summary>
    public static class UwpApplicationHelper
    {
        /// <summary>
        /// Gets all installed UWP applications using PackageManager API
        /// </summary>
        /// <returns>List of UWP application information</returns>
        public static List<UwpApplicationInfo> GetInstalledUwpApplications()
        {
            var uwpApps = new List<UwpApplicationInfo>();

            try
            {
                PackageManager packageManager = new PackageManager();
                var packages = packageManager.FindPackagesForUser("");

                foreach (var package in packages)
                {
                    try
                    {
                        // Get manifest AppList entry
                        var manifestApplications = package.GetAppListEntriesAsync().AsTask().GetAwaiter().GetResult();

                        foreach (var appEntry in manifestApplications)
                        {
                            string appId = appEntry.AppUserModelId;

                            // Get display info
                            var displayInfo = appEntry.DisplayInfo;
                            string displayName = displayInfo.DisplayName;
                            string description = displayInfo.Description;

                            // Skip system apps with empty names
                            if (string.IsNullOrEmpty(displayName))
                                continue;

                            // Get package logo
                            var logo = displayInfo.GetLogo(new Windows.Foundation.Size(64, 64));

                            // Create app info
                            var uwpApp = new UwpApplicationInfo
                            {
                                Name = displayName,
                                Description = description,
                                AppUserModelId = appId,
                                FamilyName = package.Id.FamilyName,
                                PackageFullName = package.Id.FullName,
                                Publisher = package.Id.Publisher,
                                IsFramework = package.IsFramework,
                                InstalledDate = package.InstalledDate.DateTime,
                                Version = $"{package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}"
                            };

                            // Add to list if it's not a framework package
                            if (!uwpApp.IsFramework)
                            {
                                uwpApps.Add(uwpApp);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting UWP app info: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading UWP applications: {ex.Message}",
                    "UWP Apps Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return uwpApps.OrderBy(a => a.Name).ToList();
        }

        /// <summary>
        /// Launch a UWP application by its App User Model ID
        /// </summary>
        /// <param name="appUserModelId">The App User Model ID</param>
        /// <returns>True if launch was successful, false otherwise</returns>
        public static async Task<bool> LaunchUwpApplicationAsync(string appUserModelId)
        {
            try
            {
                return await Launcher.LaunchUriAsync(new Uri("shell:AppsFolder\\" + appUserModelId));
            }
            catch
            {
                return false;
            }
        }
    }
}
