﻿using Newtonsoft.Json;
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
        private readonly HttpClient httpClient = new HttpClient();

        public AboutDialog()
        {
            InitializeComponent();

            TextVersion.Text = $"{Consts.NormalVersion}";
#if UWP
            TextVersion.Text = $"{Consts.StoreVersion} (Store)";
#endif

            // Load changelog
            try
            {
                txtChangelog.Navigate(string.Format(Consts.CHANGELOG_URL, Properties.Resources.strLang, Data.Settings.Instance.UseDarkMode ? 1 : 0));
            }
            catch (Exception)
            { }

            Loaded += AboutDialog_Loaded;
        }

        private async void AboutDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await Initialize();
        }

        public async Task Initialize()
        {
            try
            {
                string versionsJSON = await httpClient.GetStringAsync(Consts.GetVersionURL);
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
                        TextNewVersionAvailable.Foreground = new System.Windows.Media.SolidColorBrush(Colors.Red);
                    }
                    else
                        TextNewVersionAvailable.Text = $"*** {Properties.Resources.strNewerVersionAvailable} {newVersion} ***";
                }
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

            string url = $"https://code-a-software.net/simplejournal/feedback.php?name={nameBase64}&mail={mailBase64}&content={contentBase64}";

            try
            {
                var result = await httpClient.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    // If feedback was sended successfully, clear it
                    txtFeedback.Text = string.Empty;

                    MessageBox.Show(this, Properties.Resources.strFeedbackSent, Properties.Resources.strSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(this, Properties.Resources.strFailedToSendFeedback, Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
