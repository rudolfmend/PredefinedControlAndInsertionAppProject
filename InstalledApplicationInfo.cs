namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Information about an installed application
    /// </summary>
    public class InstalledApplicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string ShortcutPath { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }
    }
}
