using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

[assembly: System.CLSCompliant(false)]

namespace PredefinedControlAndInsertionAppProject
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? _instanceMutex = null;
        private const string MutexName = "PredefinedControlAndInsertionApp_SingleInstanceMutex";
        protected override void OnStartup(StartupEventArgs e)
        {
            // Create a mutex with a unique name for the application
            bool createdNew;
            _instanceMutex = new Mutex(true, MutexName, out createdNew);
            // Check if another instance is already running
            if (!createdNew)
            {
                // Another instance already exists
                MessageBox.Show("The application is already running. Multiple instances cannot be run.",
                        "Duplicate run", MessageBoxButton.OK, MessageBoxImage.Information);
                // Activate the existing application window
                ActivateRunningInstance();
                // Terminate this instance
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }
        private void ActivateRunningInstance()
        {
            // Find a process with the same name
            var currentProcess = Process.GetCurrentProcess();
            var runningProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                .FirstOrDefault(p => p.Id != currentProcess.Id);
            if (runningProcess != null)
            {
                // Try to activate the window of the already running application
                NativeMethods.SetForegroundWindow(runningProcess.MainWindowHandle);
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            // Release mutex when application exits
            if (_instanceMutex != null)
            {
                _instanceMutex.ReleaseMutex();
                _instanceMutex.Close();
                _instanceMutex = null;
            }
            base.OnExit(e);
        }
    }
}
