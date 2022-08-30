using Helper;
using SimpleJournal.Modules;
using System.Windows;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für About.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();

            // This will only be required for a chrome dialog:
            // ModuleHelper.ApplyTabbedModuleToWindow(AboutModule, this);
        }

        public AboutDialog ShowFeedbackPage()
        {
            AboutModule.TabControl.SelectedIndex = 2;
            return this;
        }

        public AboutDialog ShowChangelogPage()
        {
            AboutModule.TabControl.SelectedIndex = 1;
            return this;
        }
    }
}