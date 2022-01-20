using SimpleJournal.Common;
using System;

namespace SimpleJournal.Data
{
    public class PdfJournalPage : JournalPage
    {
        public PdfJournalPage()
        {
            // Set to custom (bc a pdf page can only have custom as pattern!)
            PaperPattern = PaperType.Custom;
        }

        public byte[] PageBackground { get; set; }

        public Orientation Orientation { get; set; }    
    }
}