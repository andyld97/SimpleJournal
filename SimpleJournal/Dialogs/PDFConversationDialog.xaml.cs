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

            bool useAllPages = RadioAllPages.IsChecked.Value;
            int pageFrom = NumPageFrom.Value;
            int pageTo = NumPageTo.Value;

            if (!useAllPages && pageFrom > pageTo)
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

            // ToDo: *** Limit pages to 100 (if more split the document into multiple documents ..100, ..200)

            if (images != null)
            {
                Progress.IsIndeterminate = false;
                int count = images.Count;

                if (pageTo > count)
                {
                    int nPageTo = Math.Min(100, count);
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
                        TextState.Text = string.Format(Properties.Resources.strPDFConversationDialog_ConversationStatusMessage, pg, end );
                        Progress.Value = percentage;
                    }));

                    Orientation orientation = image.Width >= image.Height ? Orientation.Landscape : Orientation.Portrait;

                    // Create a journal page using image
                    await Task.Run(() => 
                    {
                        PdfJournalPage pdfJournalPage = new PdfJournalPage
                        {
                            PageBackground = image.ToByteArray(MagickFormat.Png),
                            PaperPattern = PaperType.Custom,
                            Orientation = orientation
                        };

                        journal.Pages.Add(pdfJournalPage);
                    });                  
                }

                TextState.Text = Properties.Resources.strPDFConversationDialog_Ready;
            }
            
            // Free resources
            images.Dispose();

            // Save the journal and quit (only on success)
            if (await journal.SaveAsync(destinationFileName))
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