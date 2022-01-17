using ImageMagick;
using SimpleJournal.Data;
using SimpleJournal.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaction logic for PDFConversationDialog.xaml
    /// </summary>
    public partial class PDFConversationDialog : Window
    {
        private readonly string sourceFileName = string.Empty;
        private string destinationFileName = string.Empty;

        public string DestinationFileName => destinationFileName;

        public PDFConversationDialog(string sourceFileName)
        {
            InitializeComponent();
            this.sourceFileName = sourceFileName;
            TextSource.Text = sourceFileName;
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
    }
}