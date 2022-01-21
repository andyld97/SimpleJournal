using ImageMagick;
using SimpleJournal.Data;
using SimpleJournal.Helper;
using SimpleJournal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using SimpleJournal.Documents;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaction logic for PDFConversationDialog.xaml
    /// </summary>
    public partial class PDFConversationDialog : Window
    {
        private readonly string sourceFileName = string.Empty;
        private string destinationFileName = string.Empty;

        private Timer timer = new Timer() { Interval = 1000 };
        private PrintTicket currentTicket = null;

        public string DestinationFileName => destinationFileName;

        public PDFConversationDialog(string sourceFileName)
        {
            InitializeComponent();
            this.sourceFileName = sourceFileName;
            TextSource.Text = sourceFileName;

            timer.Tick += Timer_Tick;
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            // ToDo: *** Translate
            // ToDo: *** Merge API into this project and make those classes Common!
            // ToDo: *** Extend API so that it supports all options from this dialog!

            if (currentTicket == null)
            {
                timer.Stop();
                return;
            }

            // Refresh ticket information
            using (HttpClient client = new HttpClient())
            {
                string url = $"{Consts.ConverterAPIUrl}/api/ticket/{currentTicket.ID}";

                var result = await client.GetAsync(url);
                var temp = await System.Text.Json.JsonSerializer.DeserializeAsync<PrintTicket>(await result.Content.ReadAsStreamAsync());
                if (temp != null)
                    currentTicket = temp;

                Dispatcher.Invoke(() =>
                {
                    if (currentTicket.Percentage > 0)
                    {
                        Progress.IsIndeterminate = false;
                        Progress.Value = currentTicket.Percentage;
                    }

                    if (currentTicket.Status == TicketStatus.Prepearing)
                        TextState.Text = $"Preparing ticket {currentTicket.Name} ...";
                    else if (currentTicket.Status == TicketStatus.InProgress)
                        TextState.Text = $"Working on ticket {currentTicket.Name} ...";
                    else if (currentTicket.Status == TicketStatus.Saving)
                        TextState.Text = $"Saving ticket {currentTicket.Name} ...";
                    else
                    {
                        TextState.Text = currentTicket.Status.ToString();

                        if (currentTicket.Status == TicketStatus.Failed)
                        {
                            timer.Stop();
                            Progress.Value = 0;
                            Progress.IsIndeterminate = false;
                            MessageBox.Show(currentTicket.ErorrMessage, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                });

                if (currentTicket.IsCompleted && currentTicket.Status == TicketStatus.Completed)
                {
                    timer.Stop();
                    Progress.Value = 100;

                    // Download file
                    try
                    {

                        var response = await client.GetAsync($"{Consts.ConverterAPIUrl}/api/ticket/{currentTicket.ID}/download/{currentTicket.Documents.FirstOrDefault()}");
                        if (response.IsSuccessStatusCode)
                        {
                            var str = await response.Content.ReadAsStreamAsync();

                            using (System.IO.FileStream fs = new System.IO.FileStream(destinationFileName, System.IO.FileMode.Create))
                            {
                                await str.CopyToAsync(fs);
                            }                
                        }
                        else
                            throw new Exception("Http Status Code: " + response.StatusCode);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());

                        SetInputPanelState(false);
                        return;
                    }

                    DialogResult = true;
                }
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal" })
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    destinationFileName = sfd.FileName;
                    TextDestination.Text = destinationFileName;
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            GeneralHelper.OpenUri(new Uri(Consts.GhostScriptDownloadUrl));
        }
        
        private void SetInputPanelState(bool state)
        {
            PanelInput.Visibility = (state ? Visibility.Visible : Visibility.Hidden);
            PanelProgress.Visibility = (state ? Visibility.Hidden : Visibility.Visible);
            PanelInput.IsEnabled = state;
        }

        private async void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            bool useAllPages = RadioAllPages.IsChecked.Value;
            bool useOnlineConverstation = CheckUseOnlineConverter.IsChecked.Value; 
            int pageFrom = NumPageFrom.Value;
            int pageTo = NumPageTo.Value;

            // Validate input params
            if (string.IsNullOrEmpty(destinationFileName) || string.IsNullOrEmpty(sourceFileName) || !System.IO.File.Exists(sourceFileName))
            {
                MessageBox.Show(this, Properties.Resources.strPDFConversationDialog_InvalidInputMessage, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Further validation
            if (!useAllPages && pageFrom > pageTo)
            {
                MessageBox.Show(this, Properties.Resources.strPDFConversationDialog_InvalidInputMessage, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (useOnlineConverstation)
            {
                // Upload file
                var printTicket = await UploadFileAsync(sourceFileName, $"{Consts.ConverterAPIUrl}/api/pdf/upload");

                if (printTicket == null)
                {
                    MessageBox.Show("error");
                    return;
                }
                else
                {
                    currentTicket = printTicket;
                    SetInputPanelState(false);
                    Progress.IsIndeterminate = true;
                    TextState.Text = $"Ticket {printTicket.Name} is in the queue!";
                    timer.Start();
                }

                return;
            }

            SetInputPanelState(false);
            Progress.IsIndeterminate = true;
            TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_ReadingPDFDocument, System.IO.Path.GetFileName(sourceFileName));

            // Read PDF Document
            MagickImageCollection images = null;
            try
            {
                images = await PdfHelper.ReadPDFFileAsync(sourceFileName);
            }
            catch (Exception ex)
            { 
                MessageBox.Show(this, $"{Properties.Resources.strPDFConversationDialog_GhostscriptMessage}\n\n{ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                SetInputPanelState(true);
                return;
            }

            // Limit pages to Consts.MaxPDFPagesPerJournal (if more split the document into multiple documents ..100, ..200)
            Journal currentJournal = new Journal();

            if (images.Count > Consts.MaxPDFPagesPerJournal)
            {
                Progress.IsIndeterminate = false;

                // Calculate the amount of journals required
                int n = (int)Math.Ceiling(images.Count / (double)Consts.MaxPDFPagesPerJournal);
                string firstFileName = string.Empty;

                List<Journal> journals = new List<Journal>() { new Journal() };
                currentJournal = journals.FirstOrDefault();

                int journalCounter = 1;
                for (int p = 0; p < images.Count; p++)
                {
                    var image = images[p];
                    var page = await PdfHelper.CreatePdfJournalPageAsync(image);

                    if (currentJournal.Pages.Count < Consts.MaxPDFPagesPerJournal)
                        currentJournal.Pages.Add(page);
                    else
                    {
                        currentJournal = new Journal();
                        currentJournal.Pages.Add(page);
                        journals.Add(currentJournal);
                        journalCounter++;
                    }

                    double percentage = Math.Round(((p + 1) / (double)images.Count) * 100.0);

                    // Update gui status message
                    Dispatcher.Invoke(new Action(() =>
                    {
                        int maxPages = (journals.Count == n ? images.Count % Consts.MaxPDFPagesPerJournal : Consts.MaxPDFPagesPerJournal);
                        TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_WritingPageStatus, (p % Consts.MaxPDFPagesPerJournal) + 1, maxPages, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{journalCounter}.journal");
                        Progress.Value = percentage;
                    }));
                }

                int counter = 1;
                foreach (var journal in journals)
                {
                    // Generate fileName
                    string newFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destinationFileName), System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{counter}.journal");

                    if (string.IsNullOrEmpty(firstFileName))
                        firstFileName = newFileName;

                    // Update gui status message
                    Dispatcher.Invoke(new Action(() =>
                    {
                        TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Saving, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{counter}.journal");
                    }));

                    await journal.SaveAsync(newFileName, hideStatus: true);
                    counter++;
                }

                // Set destination filename to the first document
                destinationFileName = firstFileName;

                DialogResult = true;
            }
            else
            {
                // Create a new journal
                Journal journal = new Journal();

                if (images != null)
                {
                    Progress.IsIndeterminate = false;
                    int count = images.Count;

                    if (!useAllPages && pageTo > count)
                    {
                        int nPageTo = Math.Min(Consts.MaxPDFPagesPerJournal, count);
                        string message = string.Format(Properties.Resources.strPDFCOnversationDialog_TooFewPagesMessage, nPageTo, pageFrom, nPageTo);
                        if (MessageBox.Show(this, message, Properties.Resources.strSure, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        {
                            PanelInput.Visibility = Visibility.Visible;
                            PanelProgress.Visibility = Visibility.Collapsed;
                            PanelInput.IsEnabled = true;
                            return;
                        }
                        else
                            pageTo = nPageTo;
                    }

                    int start = (useAllPages ? 0 : pageFrom - 1);
                    int end = (useAllPages ? count : pageTo);

                    for (int i = start; i < end; i++)
                    {
                        var image = images[i];
                        int pg = i + 1;

                        double percentage = Math.Round((pg / (double)end) * 100.0);

                        // Update gui status message
                        Dispatcher.Invoke(new Action(() =>
                        {
                            TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_ConversationStatusMessage, pg, end);
                            Progress.Value = percentage;
                        }));

                        journal.Pages.Add(await PdfHelper.CreatePdfJournalPageAsync(image));
                    }

                    TextState.Text = Properties.Resources.strPDFConversationDialog_Ready;
                }

                // Free resources
                images.Dispose();

                // Save the journal and quit (only on success)
                if (await journal.SaveAsync(destinationFileName, hideStatus: true))
                    DialogResult = true;
                else
                    SetInputPanelState(true);
            }
        }

        /// <summary>
        /// https://stackoverflow.com/a/53284839/6237448
        /// </summary>
        public static async Task<PrintTicket> UploadFileAsync(string path, string url)
        {
            // we need to send a request with multipart/form-data
            var multiForm = new MultipartFormDataContent();

            // add file and directly upload it
            System.IO.FileStream fs = System.IO.File.OpenRead(path);
            multiForm.Add(new StreamContent(fs), "file", System.IO.Path.GetFileName(path));

            // send request to API 
            var client = new HttpClient();
            var response = await client.PostAsync(url, multiForm);

            if (response.IsSuccessStatusCode)
            {
                return await System.Text.Json.JsonSerializer.DeserializeAsync<PrintTicket>(await response.Content.ReadAsStreamAsync());
            }

            return null;
        }
    }

    public class PrintTicket
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("documents")]
        public List<string> Documents { get; set; } = new List<string>();

        [JsonPropertyName("status")]
        public TicketStatus Status { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Value from 1-100
        /// </summary>
        [JsonPropertyName("percentage")]
        public int Percentage { get; set; }

        [JsonPropertyName("error_message")]
        public string ErorrMessage { get; set; }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("date_time_added")]
        public DateTime DateTimeAdded { get; set; }

        [JsonIgnore]
        public string TempPath => System.IO.Path.Combine(System.IO.Path.GetTempPath(), ID);

        public override string ToString()
        {
            return Name;
        }
    }
}