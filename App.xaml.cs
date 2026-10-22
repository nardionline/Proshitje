using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace ProShitje.Wpf
{
    public partial class App : Application
    {
        private static readonly string LogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProShitje-startup-error.log");

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                WriteLog("AppDomain UnhandledException", args.ExceptionObject as Exception);
            };

            DispatcherUnhandledException += (s, args) =>
            {
                WriteLog("WPF DispatcherUnhandledException", args.Exception);
                MessageBox.Show(
                    "ProShitje hasi një gabim gjatë nisjes ose përdorimit.\n\n" +
                    (args.Exception == null ? "Gabim i panjohur." : args.Exception.Message) +
                    "\n\nLog-u u ruajt te:\n" + LogPath,
                    "ProShitje - Gabim",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };

            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                WriteLog("UnobservedTaskException", args.Exception);
            };

            try
            {
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                WriteLog("Startup exception", ex);
                MessageBox.Show(
                    "ProShitje nuk mund të niset.\n\n" + ex +
                    "\n\nLog-u u ruajt te:\n" + LogPath,
                    "ProShitje - Gabim gjatë nisjes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private static void WriteLog(string source, Exception ex)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.AppendLine(source);
                sb.AppendLine(ex == null ? "(No exception details available)" : ex.ToString());
                File.AppendAllText(LogPath, sb.ToString(), Encoding.UTF8);
            }
            catch
            {
                // Never allow diagnostic logging to crash the application.
            }
        }
    }
}
