using ImageMagick;
using SimpleJournal.Common;
using SimpleJournal.Helper.PDF;

namespace SimpleJournal.Documents.PDF
{
    public class PdfConverter
    {
        #region Private Members
        private readonly string sourceFilePath;
        private readonly string destinationFileName;
        private bool isCanceld = false;
        private readonly PdfConversationOptions options;

        #endregion

        #region Events
        public delegate void OnProgressChanged(PdfAction status, int progress, int currentPage, int maxPages, string journal);
        public event OnProgressChanged ProgressChanged;

        public delegate void OnCompleted(bool success, Exception ex, string destinationFileName);
        public event OnCompleted Completed;

        public delegate bool OnJournalHasFewerPagesThenRequired(int firstPage, int maxPages);

        /// <summary>
        /// If a page range is sepcified and the journal hasn't enough pages, this should return true if you want to use all pages instead. Otherwise the conversation will be canceld.
        /// </summary>
        public event OnJournalHasFewerPagesThenRequired JournalHasFewerPagesThenRequired;

        #endregion

        #region Ctor
        public PdfConverter(string sourceFilePath, string destinationFileName) : this(sourceFilePath, destinationFileName, null)
        { }

        public PdfConverter(string sourceFilePath, string destinationFileName, PdfConversationOptions pdfConversationOptions)
        {
            this.sourceFilePath = sourceFilePath;
            this.destinationFileName = destinationFileName;

            if (pdfConversationOptions == null)
                options = new PdfConversationOptions();
            else
                options = pdfConversationOptions;
        }
        #endregion

        public void Cancel()
        {
            isCanceld = true;
        }

        public async Task<List<Journal>> ConvertAsync()
        {
            int pageFrom = options.StartPage;
            int pageTo = options.LastPage;

            // Read PDF Document
            MagickImageCollection images = null;
            try
            {
                ProgressChanged?.Invoke(PdfAction.Reading, 0, 0, 0, System.IO.Path.GetFileName(sourceFilePath));
                images = await PdfHelper.ReadPDFFileAsync(sourceFilePath);
            }
            catch (Exception ex)
            {
                Completed?.Invoke(false, ex, string.Empty);
                return null;
            }

            if (isCanceld)
                return null;

            // Limit pages to Consts.MaxPDFPagesPerJournal (if more split the document into multiple documents ..100, ..200)
            Journal currentJournal = new Journal();

            if (images.Count > options.PagesPerJournal)
            {
                // Calculate the amount of journals required
                int n = (int)Math.Ceiling(images.Count / (double)options.PagesPerJournal);
                string firstFileName = string.Empty;

                List<Journal> journals = new List<Journal>() { new Journal() };
                currentJournal = journals.FirstOrDefault();

                int journalCounter = 1;
                for (int p = 0; p < images.Count; p++)
                {
                    if (isCanceld)
                        return null;

                    var image = images[p];
                    var page = await PdfHelper.CreatePdfJournalPageAsync(image);

                    if (currentJournal.Pages.Count < options.PagesPerJournal)
                        currentJournal.Pages.Add(page);
                    else
                    {
                        currentJournal = new Journal();
                        currentJournal.Pages.Add(page);
                        journals.Add(currentJournal);
                        journalCounter++;
                    }

                    double percentage = Math.Round(((p + 1) / (double)images.Count) * 100.0);
                    int maxPages = (journals.Count == n ? images.Count % options.PagesPerJournal : options.PagesPerJournal);
                    ProgressChanged?.Invoke(PdfAction.PagesALL_WritingPage, (int)percentage, (p % options.PagesPerJournal) + 1, maxPages, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{journalCounter}.journal");
                }

                int counter = 1;
                foreach (var journal in journals)
                {
                    if (isCanceld)
                        break;

                    // Generate fileName
                    string newFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destinationFileName), System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{counter}.journal");

                    if (string.IsNullOrEmpty(firstFileName))
                        firstFileName = newFileName;

                    ProgressChanged?.Invoke(PdfAction.Saving, 100, 0, 0, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{counter}.journal");

                    await journal.SaveAsync(newFileName, hideStatus: true);
                    counter++;
                }

                if (isCanceld)
                    return null;

                Completed?.Invoke(true, null, firstFileName);
                return journals;
            }
            else
            {
                if (isCanceld)
                    return null;

                // Create a new journal
                Journal journal = new Journal();

                if (images != null)
                {
                    // Progress.IsIndeterminate = false;
                    int count = images.Count;

                    if (options.UsePageRange && pageTo > count)
                    {
                        int nPageTo = Math.Min(options.PagesPerJournal, count);
                      
                        var result = JournalHasFewerPagesThenRequired?.Invoke(pageFrom, nPageTo);
                        if (result.HasValue && !result.Value)
                            return null;
                        else
                            pageTo = nPageTo;                                                
                    }

                    if (isCanceld)
                        return null;

                    int start = (!options.UsePageRange ? 0 : pageFrom - 1);
                    int end = (!options.UsePageRange ? count : pageTo);

                    for (int i = start; i < end; i++)
                    {
                        if (isCanceld)
                            break;

                        var image = images[i];
                        int currentPage = i + 1;

                        double percentage = Math.Round((currentPage / (double)end) * 100.0);

                        ProgressChanged?.Invoke(PdfAction.PageRange_WritingPage, (int)percentage, currentPage, end, string.Empty);
                        journal.Pages.Add(await PdfHelper.CreatePdfJournalPageAsync(image));
                    }
                }

                if (isCanceld)
                    return null;

                // Free resources
                images.Dispose();

                if (isCanceld)
                    return null;

                ProgressChanged?.Invoke(PdfAction.Saving, 100, 0, 0, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + ".journal");

                if (isCanceld)
                    return null;

                // Save the journal and quit (only on success)
                if (await journal.SaveAsync(destinationFileName, hideStatus: true))
                {
                    if (isCanceld)
                        return null;

                    Completed?.Invoke(true, null, System.IO.Path.GetFileName(destinationFileName));
                    return new List<Journal>() { journal }; 
                }
                else 
                {
                    if (isCanceld)
                        return null;

                    Completed?.Invoke(false, null, string.Empty);
                }
            }

            return null;
        }
    }
}