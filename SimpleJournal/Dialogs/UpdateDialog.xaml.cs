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
            txtVersion.Text = string.Format(Properties.Resources.strUpdateDialogVersionText, v.ToString(3));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Consts.DOWNLOAD_URL);
                DialogResult = true;

                // Exit to make sure user can easily update without problems
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string message = string.Format(Properties.Resources.strUpdateDialogFailedToOpenBrowserAutomatically, ex.Message, Consts.DOWNLOAD_URL, Consts.HOMEPAGE_URL);
                MessageBox.Show(this, message, Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }             
        }
    }
}
