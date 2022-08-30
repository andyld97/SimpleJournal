using SimpleJournal.Modules;
using System.ComponentModel;
using System.Windows;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();     
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SettingsModule.OnClosing();
        }
    }
}