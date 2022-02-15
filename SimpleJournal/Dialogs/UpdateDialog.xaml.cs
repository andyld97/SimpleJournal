using Microsoft.Web.WebView2.Core;
using SimpleJournal.Data;
using SimpleJournal.Documents.UI;
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
            try
            {
                txtVersion.Text = string.Format(Properties.Resources.strUpdateDialogVersionText, v.ToString(4));
            }
            catch
            {
                txtVersion.Text = string.Format(Properties.Resources.strUpdateDialogVersionText, v.ToString(3));
            }

            Loaded += UpdateDialog_Loaded;
        }

        private async void UpdateDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load changelog
                var webView2Envoirnment = await CoreWebView2Environment.CreateAsync(null, Consts.WebView2CachePath);
                await webChangelog.EnsureCoreWebView2Async(webView2Envoirnment);
                webChangelog.Source = new Uri(string.Format(Consts.ChangelogUrl, Properties.Resources.strLang, Settings.Instance.UseDarkMode ? 1 : 0));
            }
            catch (Exception)
            { }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GeneralHelper.OpenUri(new Uri(Consts.DonwloadUrl));
                DialogResult = true;

                // Exit to make sure user can easily update without problems
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string message = string.Format(Properties.Resources.strUpdateDialogFailedToOpenBrowserAutomatically, ex.Message, Consts.DonwloadUrl, Consts.HomePageUrl);
                MessageBox.Show(this, message, Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }             
        }
    }
}
