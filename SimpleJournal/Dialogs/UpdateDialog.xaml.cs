using SimpleJournal.Data;
using System;
using System.Windows;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        public UpdateDialog(Version v)
        {
            InitializeComponent();
            webChangelog.Navigate(string.Format(Consts.CHANGELOG_URL, Properties.Resources.strLang, Settings.Instance.UseDarkMode ? 1 : 0));

            string versionText = Properties.Resources.strUpdateDialogVersionText;
            versionText = versionText.Replace("{0}", v.ToString());

            this.txtVersion.Text = versionText;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Consts.DOWNLOAD_URL);
                this.DialogResult = true;

                // Exit to make sure user can easily update without problems
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string message = Properties.Resources.strUpdateDialogFailedToOpenBrowserAutomatically;
                message = message.Replace("{0}", ex.Message);
                message = message.Replace("{1}", Consts.DOWNLOAD_URL);
                message = message.Replace("{2}", "https://simplejournal.ca-soft.net");

                MessageBox.Show(this, message, Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }             
        }
    }
}
