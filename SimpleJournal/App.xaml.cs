using System.Windows;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public static string Path { get; private set; } = string.Empty;

        public static bool IgnoreBackups { get; private set; } = false;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                // Path is the cli parameter for the Document
                Path = e.Args[0];

                // Check if /ignorebackups is set, this is only required for internal usage (BackupDialog)
                if (e.Args.Length >= 2)
                {
                    if (e.Args[1].Trim() == "/ignorebackups")
                        IgnoreBackups = true;
                }
            }

            GeneralHelper.ApplyTheming();
        }
    }
}
