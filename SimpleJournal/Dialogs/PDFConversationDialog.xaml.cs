﻿using SimpleJournal.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using SimpleJournal.Documents.PDF;
using SimpleJournal.Data;
using System.ComponentModel;

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

        #region Ctor
        public PDFConversationDialog(string sourceFileName)
        {
            InitializeComponent();
            this.sourceFileName = sourceFileName;
            TextSource.Text = sourceFileName;

            CheckUseOnlineConverter.IsChecked = Settings.Instance.UseOnlineConversation;

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
            };

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
                SetInputPanelState(false);

                // Upload file
                // ToDo: ** Try-Catch
                var printTicket = await GeneralHelper.UploadFileAsync(sourceFileName, $"{Consts.ConverterAPIUrl}/api/pdf/upload", System.Text.Json.JsonSerializer.Serialize(options));

                if (printTicket == null)
                {
                    MessageBox.Show(Properties.Resources.strPDFConversationDialog_ConversationGeneralError, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(this, $"{Properties.Resources.strPDFConversationDialog_GhostscriptMessage}\n\n{ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(Properties.Resources.strPDFConversationDialog_ConversationGeneralError, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);

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

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (timer.Enabled)
            {
                timer.Stop();
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Send a message to the api that the ticket should be canceld!
                        await client.GetAsync($"{Consts.ConverterAPIUrl}/api/ticket/{currentTicket?.ID}/cancel");
                    }
                }
                catch
                {
                    // ignore
                }
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
                            MessageBox.Show(currentTicket.ErorrMessage, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
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

                            var response = await client.GetAsync($"{Consts.ConverterAPIUrl}/api/ticket/{currentTicket.ID}/download/{journal}");
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

                        try
                        {
                            // Send a message to the api that the ticket can be deleted
                            await client.GetAsync($"{Consts.ConverterAPIUrl}/api/ticket/{currentTicket.ID}/delete");
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                    catch (Exception ex)
                    {
                        SetInputPanelState(false);
                        MessageBox.Show($"{Properties.Resources.strPDFConversationDialog_ConversationGeneralError}{Environment.NewLine}{Environment.NewLine}{ex.Message}" , Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    DialogResult = true;
                }
            }
        }

        #endregion
    }
}