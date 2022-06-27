using Helper;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleJournal.Common.Helper;
using SimpleJournal.Dialogs;
using SimpleJournal.Documents.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für About.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        private bool isLoaded;

        public AboutDialog()
        {
            InitializeComponent();

            TextVersion.Text = $"{Consts.NormalVersion}";
#if UWP
            TextVersion.Text = $"{Consts.StoreVersion} (Store)";
#endif

            Loaded += AboutDialog_Loaded;

            // https://stackoverflow.com/a/58136318/6237448
            if (System.Environment.Version < Consts.CompiledDotnetVersion)
            {
                TextDotNetVersion.Foreground = new SolidColorBrush(Colors.Orange);
                TextUpdateNet.Visibility = Visibility.Visible;
            }
            else
                TextUpdateNet.Visibility = Visibility.Collapsed;

            TextDotNetVersion.Text = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

            if (Properties.Resources.strLang != "de")
                TextClock.Text = string.Empty;
        }

        private async void LinkUpdateNET_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            await GeneralHelper.UpdateNETCoreVersionAsync();
        }

        public AboutDialog ShowFeedbackPage()
        {
            MainTabControl.SelectedIndex = 2;
            return this;
        }

        public AboutDialog ShowChangelogPage()
        {
            MainTabControl.SelectedIndex = 1;
            return this;
        }

        private async void AboutDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Prevent multiple calls
            if (isLoaded)
                return;

            isLoaded = true;

            await Initialize();
        }

        public async Task Initialize()
        {
            var result = await UpdateHelper.CheckForUpdatesAsync();

            if (result.Result == Common.UpdateResult.NoUpdateAvaialble)
                TextVersion.Text += $" - {Properties.Resources.strVersionUpToDate}";
            else if (result.Result == Common.UpdateResult.DevVersion)
            {
                TextNewVersionAvailable.Text = Properties.Resources.strUnpublishedDevVersion;
                TextNewVersionAvailable.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (result.Result == Common.UpdateResult.UpdateAvailable)
                TextNewVersionAvailable.Text = $"*** {Properties.Resources.strNewerVersionAvailable} {result.Version} ***";
            else
            {

            }
            

            try
            {
                // Load changelog
                var webView2Envoirnment = await CoreWebView2Environment.CreateAsync(null, Consts.WebView2CachePath);
                await BrowserChangelog.EnsureCoreWebView2Async(webView2Envoirnment);
                BrowserChangelog.Source = new Uri(string.Format(Consts.ChangelogUrl, Properties.Resources.strLang, Settings.Instance.UseDarkMode ? 1 : 0));
            }
            catch (Exception)
            { }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            GeneralHelper.OpenUri(e.Uri);
        }

        private async void btnSendFeedback_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string mail = txtMail.Text;
            string content = txtFeedback.Text;

            if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show(this, Properties.Resources.strPleaseEnterValidText, Properties.Resources.strEmptyText, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Encode to base64 and ensure it can be transmitted via url, so "=" is not allowed in url => use µ instead
            string nameBase64 = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(name)).Replace("=", "µ");
            string mailBase64 = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(mail)).Replace("=", "µ");
            string contentBase64 = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(content)).Replace("=", "µ");
            string url = string.Format(Consts.FeedbackUrl, nameBase64, mailBase64, contentBase64);

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var result = await httpClient.GetAsync(url);

                    if (result.IsSuccessStatusCode)
                    {
                        // If feedback was sended successfully, clear it
                        txtFeedback.Text = string.Empty;

                        MessageBox.Show(this, Properties.Resources.strFeedbackSent, Properties.Resources.strSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else 
                        throw new Exception($"HTTP Status Code: {result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"{Properties.Resources.strFailedToSendFeedback}\n\n{ex.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrlDataProtection_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            GeneralHelper.OpenUri(new Uri(string.Format(Consts.DataProtectionUrl, Properties.Resources.strLang)));
        }
    }
}