using ImageMagick;
using SimpleJournal.Data;
using SimpleJournal.Helper;
using System;
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

        private async void ButtonConvert_Click(object sender, RoutedEventArgs e)
        {
            // Validate input params
            if (string.IsNullOrEmpty(destinationFileName) || string.IsNullOrEmpty(sourceFileName) || !System.IO.File.Exists(sourceFileName))
            {
                MessageBox.Show(this, Properties.Resources.strPDFConversationDialog_InvalidInputMessage, Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PanelInput.Visibility = Visibility.Hidden;
            PanelProgress.Visibility = Visibility.Visible;
            PanelInput.IsEnabled = false;
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
                PanelInput.Visibility = Visibility.Visible;
                PanelProgress.Visibility = Visibility.Collapsed;
                PanelInput.IsEnabled = true;
                return;
            }

            // Create a journal
            Journal journal = new Journal();

            if (images != null)
            {
                Progress.IsIndeterminate = false;
                int count = images.Count;

                for (int i = 0; i < count; i++)
                {
                    var image = images[i];
                    int pg = i + 1;
             
                    double percentage = Math.Round((pg / (double)count) * 100.0);

                    // Update gui status message
                    Dispatcher.Invoke(new Action(() => 
                    {
                        TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_ConversationStatusMessage, pg, count);
                        Progress.Value = percentage;
                    }));

                    // Create a journal page using image
                    await Task.Run(() => 
                    {
                        PdfJournalPage pdfJournalPage = new PdfJournalPage
                        {
                            PageBackground = image.ToByteArray(MagickFormat.Png),
                            PaperPattern = PaperType.Custom
                        };

                        journal.Pages.Add(pdfJournalPage);
                    });                  
                }

                TextState.Text = Properties.Resources.strPDFConversationDialog_Ready;
            }
            
            // Free resources
            images.Dispose();

            // Save the journal and quit (only on success)
            if (journal.Save(destinationFileName))
                DialogResult = true;
            else
            {
                PanelInput.Visibility = Visibility.Visible;
                PanelProgress.Visibility = Visibility.Collapsed;
                PanelInput.IsEnabled = true;
            }
        }
    }
}