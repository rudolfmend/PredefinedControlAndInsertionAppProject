namespace PredefinedControlAndInsertionAppProject
{
    /// <summary>
    /// Extension methods for ApplicationInfo class
    /// </summary>
    public static class ApplicationInfoExtensions
    {
        private static Dictionary<int, bool> _systemProcessCache = new Dictionary<int, bool>();

        public static bool IsSystemProcess(this ApplicationInfo appInfo)
        {
            if (_systemProcessCache.TryGetValue(appInfo.ProcessId, out bool isSystem))
            {
                return isSystem;
            }

            return false;
        }

        public static void SetIsSystemProcess(this ApplicationInfo appInfo, bool isSystem)
        {
            _systemProcessCache[appInfo.ProcessId] = isSystem;
        }
    }
}
