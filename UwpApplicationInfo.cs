using System;
using System.Threading.Tasks;

namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Represents a UWP & Microsoft Store application information.
    /// Detection and launching methods for UWP applications.
    /// Info about the package and publisher.
    /// </summary>
    public class UwpApplicationInfo : ApplicationInfoBase
    {
        /// <summary>
        /// App User Model ID - unique identifier used to launch the application
        /// </summary>
        public string AppUserModelId { get; set; } = string.Empty;

        /// <summary>
        /// Package family name
        /// </summary>
        public string FamilyName { get; set; } = string.Empty;

        /// <summary>
        /// Full package name including version
        /// </summary>
        public string PackageFullName { get; set; } = string.Empty;

        /// <summary>
        /// Application publisher
        /// </summary>
        public string Publisher { get; set; } = string.Empty;

        /// <summary>
        /// Application description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if this is a framework package
        /// </summary>
        public bool IsFramework { get; set; }

        /// <summary>
        /// Date when the application was installed
        /// </summary>
        public DateTime InstalledDate { get; set; }

        /// <summary>
        /// Application version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets the application type
        /// </summary>
        public override ApplicationType Type => ApplicationType.Uwp;

        /// <summary>
        /// Determines if the application can be launched
        /// </summary>
        public override bool CanLaunch => !string.IsNullOrEmpty(AppUserModelId);

        /// <summary>
        /// Attempts to launch the UWP application
        /// </summary>
        /// <returns>A task representing the launch operation</returns>
        public async Task<bool> LaunchAsync()
        {
            if (!CanLaunch)
                return false;

            try
            {
                return await UwpApplicationHelper.LaunchUwpApplicationAsync(AppUserModelId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Synchronous wrapper for LaunchAsync
        /// </summary>
        public bool Launch()
        {
            return LaunchAsync().GetAwaiter().GetResult();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
