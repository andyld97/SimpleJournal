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
        private bool isCanceled = false;
        private readonly PdfConversationOptions options;

        #endregion

        #region Events
        public delegate void OnProgressChanged(PdfAction status, int progress, int currentPage, int maxPages, string journal);
        public event OnProgressChanged ProgressChanged;

        public delegate void OnCompleted(bool success, Exception ex, string destinationFileName);
        public event OnCompleted Completed;

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
            isCanceled = true;
        }

        public async Task<List<Journal>> ConvertAsync()
        {
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

            if (isCanceled)
                return null;

            // Limit pages to Consts.MaxPDFPagesPerJournal (if more split the document into multiple documents 1..100, 101..200, ...)
            Journal currentJournal = new Journal();
            int pageCount = DeterminePageCount(options, images.Count);

            if (pageCount > options.PagesPerJournal)
            {
                // More than one journal is required
                // Calculate the amount of journals required
                int n = (int)Math.Ceiling(pageCount / (double)options.PagesPerJournal);
                string firstFileName = string.Empty;

                List<Journal> journals = new List<Journal>() { new Journal() };
                currentJournal = journals.FirstOrDefault();

                int journalCounter = 1;
                int progress = 0;
                int sI = (options.UsePageRange ? options.StartPage - 1 : 0);
                int eI = (options.UsePageRange ? options.LastPage - 1 : pageCount - 1);

                for (int p = sI; p <= eI; p++)
                {
                    if (isCanceled)
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

                    double percentage = Math.Round(((progress + 1) / (double)pageCount) * 100.0);
                    int maxPages = (journals.Count == n ? pageCount % options.PagesPerJournal : options.PagesPerJournal);

                    ProgressChanged?.Invoke(PdfAction.PagesALL_WritingPage, (int)percentage, (progress % options.PagesPerJournal) + 1, maxPages, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{journalCounter}.journal");
                    progress++;
                }

                int counter = 1;
                int journalCount = journals.Count;

                foreach (var journal in journals)
                {
                    if (isCanceled)
                        break;

                    // Page navigation in SimpleJournal PDF document
                    if (journalCount > 1)
                    {
                        if (counter == 1)
                        {
                            // First page
                            journal.PreviousDocumentIndex = null;
                            journal.NextDocumentIndex = counter + 1;
                        }
                        else if (counter == journalCount)
                        {
                            // Last page
                            journal.PreviousDocumentIndex = counter - 1;
                            journal.NextDocumentIndex = null;
                        }
                        else
                        {
                            // Page in between
                            journal.PreviousDocumentIndex = counter - 1;
                            journal.NextDocumentIndex = counter + 1;
                        }
                    }

                    // Generate fileName
                    string newFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(destinationFileName), System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{counter}.journal");

                    if (string.IsNullOrEmpty(firstFileName))
                        firstFileName = newFileName;

                    ProgressChanged?.Invoke(PdfAction.Saving, 100, 0, 0, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + $".{counter}.journal");

                    await journal.SaveAsync(newFileName, hideStatus: true);
                    counter++;
                }

                if (isCanceled)
                    return null;

                Completed?.Invoke(true, null, firstFileName);
                return journals;

            }
            else
            {
                // Only one journal is required!
                if (isCanceled)
                    return null;

                // Create a new journal
                Journal journal = new Journal();

                if (images != null)
                {
                    if (isCanceled)
                        return null;

                    int sI = (options.UsePageRange ? options.StartPage - 1 : 0);
                    int eI = (options.UsePageRange ? options.LastPage - 1 : pageCount - 1);
                    int progress = 0;

                    for (int i = sI; i <= eI; i++)
                    {
                        if (isCanceled)
                            break;

                        var image = images[i];
                        int currentPage = i + 1;

                        double percentage = Math.Round((progress / (double)pageCount) * 100.0);

                        ProgressChanged?.Invoke(PdfAction.PageRange_WritingPage, (int)percentage, currentPage, pageCount, string.Empty);
                        journal.Pages.Add(await PdfHelper.CreatePdfJournalPageAsync(image));
                        progress++;
                    }
                }

                if (isCanceled)
                    return null;

                // Free resources
                images.Dispose();

                if (isCanceled)
                    return null;

                ProgressChanged?.Invoke(PdfAction.Saving, 100, 0, 0, System.IO.Path.GetFileNameWithoutExtension(destinationFileName) + ".journal");

                if (isCanceled)
                    return null;

                // Save the journal and quit (only on success)
                if (await journal.SaveAsync(destinationFileName, hideStatus: true))
                {
                    if (isCanceled)
                        return null;

                    Completed?.Invoke(true, null, destinationFileName);
                    return new List<Journal>() { journal };
                }
                else
                {
                    if (isCanceled)
                        return null;

                    Completed?.Invoke(false, null, string.Empty);
                }
            }

            return null;
        }

        private int DeterminePageCount(PdfConversationOptions options, int pdfPages)
        {
            int pageCount = pdfPages;

            if (options.UsePageRange)
            {
                int tmp = 0;

                int sp = options.StartPage - 1;
                int lp = options.LastPage;

                if (sp > pdfPages - 1 || sp > lp || lp > pdfPages - 1)
                    throw new ArgumentException("Page range is invalid for this document!");

                for (int p = sp; p < lp; p++)
                    tmp++;

                pageCount = tmp;
            }

            return pageCount;
        }
    }
}