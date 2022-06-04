using SimpleJournal.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using SimpleJournal.Documents.PDF;
using SimpleJournal.Data;
using System.ComponentModel;
using SimpleJournal.Documents.UI;
using System.Threading.Tasks;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaction logic for PDFConversationDialog.xaml
    /// </summary>
    public partial class PDFConversationDialog : Window
    {
        #region Pivate Members
        private readonly string sourceFileName = string.Empty;
        private string destinationFileName = string.Empty;

        private PdfConverter currentConverter = null;

        private bool isInitialized = false;

        private readonly Timer timer = new Timer() { Interval = 1000 };
        private PrintTicket currentTicket = null;

        #endregion

        public string DestinationFileName => destinationFileName;

        private static string PDF2JApiUrl
        {
            get
            {
                if (Settings.Instance.UseSelfHostedPDF2JApi && !string.IsNullOrEmpty(Settings.Instance.SelfHostedPDF2JApiUrl))
                    return Settings.Instance.SelfHostedPDF2JApiUrl;

                return Consts.ConverterAPIUrl;
            }
        }

        #region Ctor
        public PDFConversationDialog(string sourceFileName)
        {
            InitializeComponent();
            this.sourceFileName = sourceFileName;
            TextSource.Text = sourceFileName;

            CheckUseOnlineConverter.IsChecked = Settings.Instance.UseOnlineConversation;
            ExpanderHelp.IsExpanded = Settings.Instance.PDFConverstaionDialogIsHelpExpanded;
            CheckUseSelfHostedAPI.IsChecked = Settings.Instance.UseSelfHostedPDF2JApi;
            TextUrl.Text = Settings.Instance.SelfHostedPDF2JApiUrl;

            // If there is no connection, uncheck online converstaion (must be done here, to prevent this dialog from saving it to settings)
            if (!GeneralHelper.IsConnectedToInternet())
                CheckUseOnlineConverter.IsChecked = false;

            timer.Tick += Timer_Tick;
            isInitialized = true;
        }
        #endregion

        #region Dialog Buttons/Events

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
            currentConverter = null;
            bool useAllPages = RadioAllPages.IsChecked.Value;
            bool useOnlineConverstation = CheckUseOnlineConverter.IsChecked.Value; 
            int pageFrom = NumPageFrom.Value;
            int pageTo = NumPageTo.Value;

            var options = new PdfConversationOptions()
            {
                UsePageRange = !useAllPages,
                StartPage = pageFrom,
                LastPage = pageTo,
                CurrentSimpleJounalVersion = Consts.NormalVersion.ToString(4),
            };

            // Validate input params
            if (string.IsNullOrEmpty(destinationFileName) || string.IsNullOrEmpty(sourceFileName) || !System.IO.File.Exists(sourceFileName))
            {
                MessageBox.Show(this, Properties.Resources.strPDFConversationDialog_InvalidInputMessage, SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Further validation
            if (!useAllPages && pageFrom > pageTo)
            {
                MessageBox.Show(this, Properties.Resources.strPDFConversationDialog_InvalidInputMessage, SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (useOnlineConverstation)
            {
                SetInputPanelState(false);

                // Upload file to API
                PrintTicket printTicket = null;
                string errorMessage = string.Empty;

                try
                {
                    printTicket = await GeneralHelper.UploadFileAsync(sourceFileName, $"{PDF2JApiUrl}/api/pdf/upload", System.Text.Json.JsonSerializer.Serialize(options));
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }

                if (printTicket == null)
                {
                    SetInputPanelState(true);
                    MessageBox.Show($"{Properties.Resources.strPDFConversationDialog_ConversationGeneralError}{Environment.NewLine}{Environment.NewLine}{errorMessage}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    currentTicket = printTicket;
                    SetInputPanelState(false);
                    Progress.IsIndeterminate = true;
                    TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Ticket_InQueue, printTicket.Name);
                    timer.Start();
                }
            }
            else
            {
                PdfConverter pdfConverter = new PdfConverter(sourceFileName, destinationFileName, options);
                currentConverter = pdfConverter;

                pdfConverter.JournalHasFewerPagesThenRequired += PdfConverter_JournalHasFewerPagesThenRequired;
                pdfConverter.Completed += PdfConverter_Completed;
                pdfConverter.ProgressChanged += PdfConverter_ProgressChanged;

                SetInputPanelState(false);

                await pdfConverter.ConvertAsync();

                // Unassign events after completion
                pdfConverter.ProgressChanged -= PdfConverter_ProgressChanged;
                pdfConverter.JournalHasFewerPagesThenRequired -= PdfConverter_JournalHasFewerPagesThenRequired;
                pdfConverter.Completed -= PdfConverter_Completed;
            }
        }
        #endregion

        #region Converter Events

        private void PdfConverter_ProgressChanged(PdfAction status, int progress, int currentPage, int maxPages, string journal)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (status != PdfAction.Reading)
                {
                    if (Progress.IsIndeterminate)
                        Progress.IsIndeterminate = false;

                    Progress.Value = progress;
                }
                else
                {
                    Progress.IsIndeterminate = true;
                    TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_ReadingPDFDocument, journal);
                }

                if (status == PdfAction.PagesALL_WritingPage)
                    TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_WritingPageStatus, currentPage, maxPages, journal);
                else if (status == PdfAction.PageRange_WritingPage)
                    TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_ConversationStatusMessage, currentPage, maxPages);
                else if (status == PdfAction.Saving)
                    TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Saving, journal);
            }));
        }

        private void PdfConverter_Completed(bool success, Exception ex, string destinationFileName)
        {
            if (success)
            {
                DialogResult = true;
                this.destinationFileName = destinationFileName;
                return;
            }

            // Show error messages (if any)
            if (ex != null)
                MessageBox.Show(this, $"{Properties.Resources.strPDFConversationDialog_GhostscriptMessage}\n\n{ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(Properties.Resources.strPDFConversationDialog_ConversationGeneralError, SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);

            SetInputPanelState(true);
        }

        private bool PdfConverter_JournalHasFewerPagesThenRequired(int firstPage, int maxPages)
        {
            string message = string.Format(Properties.Resources.strPDFCOnversationDialog_TooFewPagesMessage, maxPages, firstPage, maxPages);

            if (MessageBox.Show(this, message, Properties.Resources.strSure, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                PanelInput.Visibility = Visibility.Visible;
                PanelProgress.Visibility = Visibility.Collapsed;
                PanelInput.IsEnabled = true;
                return false;
            }

            return true;
        }

        private async Task CancelOrDeleteTicketAsync(string operation = "cancel")
        {
            try
            {
                using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) })
                {
                    // Send a message to the api that the ticket should be canceld!
                    await client.GetAsync($"{PDF2JApiUrl}/api/ticket/{currentTicket?.ID}/{operation}");
                }
            }
            catch
            {
                // ignore
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (timer.Enabled)
            {
                timer.Stop();
                await CancelOrDeleteTicketAsync();
            }

            currentConverter?.Cancel();
        }

        private void CheckUseOnlineConverter_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.UseOnlineConversation = CheckUseOnlineConverter.IsChecked.Value;
            Settings.Instance.Save();
        }

        #endregion

        #region Online Converstaion

        private async void Timer_Tick(object sender, EventArgs e)
        {
            if (currentTicket == null)
            {
                timer.Stop();
                return;
            }

            // Refresh ticket information
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{PDF2JApiUrl}/api/ticket/{currentTicket.ID}";

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
                            TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Ticket_Preparing, currentTicket.Name);
                        else if (currentTicket.Status == TicketStatus.InProgress)
                            TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Ticket_InProgress, currentTicket.Name);
                        else if (currentTicket.Status == TicketStatus.Saving)
                            TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Ticket_Saving, currentTicket.Name);
                        else if (currentTicket.Status == TicketStatus.OnHold)
                            TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_Ticket_OnHold, currentTicket.Name);
                        else
                        {
                            TextState.Text = currentTicket.Status.ToString();

                            if (currentTicket.Status == TicketStatus.Failed)
                            {
                                timer.Stop();
                                Progress.Value = 0;
                                Progress.IsIndeterminate = false;
                                SetInputPanelState(true);
                                MessageBox.Show(currentTicket.ErorrMessage, SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    });

                    if (currentTicket.IsCompleted && currentTicket.Status == TicketStatus.Completed)
                    {
                        timer.Stop();
                        Progress.Value = 100;

                        try
                        {
                            // Download file(s)
                            int counter = 1;
                            string parentDirectory = System.IO.Path.GetDirectoryName(destinationFileName);
                            string journalFileName = System.IO.Path.GetFileNameWithoutExtension(destinationFileName);
                            foreach (var journal in currentTicket.Documents)
                            {
                                string newFileName = System.IO.Path.Combine(parentDirectory, $"{journalFileName}.{counter}.journal");

                                // Assign the first document to open
                                if (counter == 1 && currentTicket.Documents.Count > 1)
                                    destinationFileName = newFileName;

                                var response = await client.GetAsync($"{PDF2JApiUrl}/api/ticket/{currentTicket.ID}/download/{counter - 1}");
                                if (response.IsSuccessStatusCode)
                                {
                                    var str = await response.Content.ReadAsStreamAsync();

                                    using (System.IO.FileStream fs = new System.IO.FileStream(currentTicket.Documents.Count == 1 ? destinationFileName : newFileName, System.IO.FileMode.Create))
                                    {
                                        await str.CopyToAsync(fs);
                                    }
                                }
                                else
                                    throw new Exception("Http Status Code: " + response.StatusCode);

                                counter++;
                            }

                            // Send a message to the api that the ticket can be deleted
                            await CancelOrDeleteTicketAsync("delete");
                        }
                        catch (Exception ex)
                        {
                            timer.Stop();
                            SetInputPanelState(true);
                            MessageBox.Show($"{Properties.Resources.strPDFConversationDialog_ConversationGeneralError}{Environment.NewLine}{Environment.NewLine}{ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                SetInputPanelState(true);
                MessageBox.Show($"{Properties.Resources.strPDFConversationDialog_ConversationGeneralError}{Environment.NewLine}{Environment.NewLine}{ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void CheckUseSelfHostedAPI_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.UseSelfHostedPDF2JApi = CheckUseSelfHostedAPI.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.PDFConverstaionDialogIsHelpExpanded = ExpanderHelp.IsExpanded;
            Settings.Instance.Save();
        }

        private void TextUrl_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!isInitialized)
                return;

            Settings.Instance.SelfHostedPDF2JApiUrl = TextUrl.Text;
            Settings.Instance.Save();
        }
    }
}