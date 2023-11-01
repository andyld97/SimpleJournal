using SimpleJournal.Common;

namespace SimpleJournal.Documents
{
    public class PdfJournalPage : JournalPage
    {
        public PdfJournalPage()
        {
            // Set to custom (because a PDF page can only have custom as pattern!)
            PaperPattern = PaperType.Custom;
        }

        public byte[] PageBackground { get; set; }   
    }
}