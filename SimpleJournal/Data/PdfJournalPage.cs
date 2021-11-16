using System;

namespace SimpleJournal.Data
{
    [Serializable]
    public class PdfJournalPage : JournalPage
    {
        public byte[] PageBackground { get; set; }
    }
}