using System;
using System.IO;
using System.Windows;

namespace SmartXDashboard
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handler for unhandled UI thread exceptions
            DispatcherUnhandledException += (s, args) =>
            {
                LogErrorToFile(args.Exception);
                MessageBox.Show($"An unexpected error occurred: {args.Exception.Message}\n\nDetails logged to error.log",
                                "SmartX Dashboard Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // Prevent hard crash
            };

            // Global exception handler for background task exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    LogErrorToFile(ex);
                }
            };
        }

        private void LogErrorToFile(Exception ex)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] EXCEPTION: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n\n";
                File.AppendAllText("error.log", logMessage);
            }
            catch
            {
                // Fallback catch if file writing fails
            }
        }
    }
}