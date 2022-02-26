using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using SimpleJournal.Documents.UI;
using System;
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
            try
            {           
                // Load version
                using (HttpClient httpClient = new HttpClient())
                {
                    string versionsJSON = await httpClient.GetStringAsync(Consts.VersionUrl);
                    dynamic result = JsonConvert.DeserializeObject(versionsJSON);

                    string currentNormalVersion = result.current.normal;
                    string currentStoreVersion = result.current.store;

                    string currentVersion = string.Empty;
                    string newVersion = null;

#if UWP
                    newVersion = currentStoreVersion;
                    currentVersion = Consts.StoreVersion.ToString();
#else
                    currentVersion = Consts.NormalVersion.ToString();
                    newVersion = currentNormalVersion;
#endif

                    if (currentVersion == newVersion)
                        TextVersion.Text += $" - {Properties.Resources.strVersionUpToDate}";
                    else
                    {
                        if (new Version(currentVersion) > new Version(newVersion))
                        {
                            TextNewVersionAvailable.Text = Properties.Resources.strUnpublishedDevVersion;
                            TextNewVersionAvailable.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                            TextNewVersionAvailable.Text = $"*** {Properties.Resources.strNewerVersionAvailable} {newVersion} ***";
                    }
                }
            }
            catch (Exception)
            { }

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
    }
}