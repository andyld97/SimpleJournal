using SimpleJournal.Common;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static string Path { get; private set; } = string.Empty;

        public static bool IgnoreBackups { get; private set; } = false;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                // Path is the cli parameter for the document
                Path = e.Args[0];

                // Check if /ignorebackups is set, this is only required for internal usage (BackupDialog)
                if (e.Args.Length >= 2)
                {
                    if (e.Args[1].Trim() == "/ignorebackups")
                        IgnoreBackups = true;
                }
            }

            // Test
            /* var cultureInfoTest = new CultureInfo("nl");
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture = cultureInfoTest; */

            GeneralHelper.ApplyTheming();
        }
    }
}
