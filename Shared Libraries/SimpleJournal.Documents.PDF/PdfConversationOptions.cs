using System.Text.Json.Serialization;

namespace SimpleJournal.Documents.PDF
{
    public class PdfConversationOptions
    {
        private int lastPage = Consts.MaxPDFPagesPerJournal;

        [JsonPropertyName("pages_per_journal")]
        public int PagesPerJournal { get; set; } = Consts.MaxPDFPagesPerJournal;

        [JsonPropertyName("use_page_range")]
        public bool UsePageRange { get; set; }

        /// <summary>
        /// Zero based index of the start page (only used if UsePageRange is set to true)
        /// </summary>
        [JsonPropertyName("start_page")]
        public int StartPage { get; set; } = 0;

        /// <summary>
        /// Zero base index of the last page (only used if UsePageRange is set to true)
        /// </summary>
        [JsonPropertyName("last_page")]
        public int LastPage
        {
            get => lastPage;
            set
            {
                if (value > Consts.MaxPDFPagesPerJournal)
                    throw new ArgumentException($"A converted journal cannot have more than {Consts.MaxPDFPagesPerJournal} pages!");

                lastPage = value;
            }
        }
    }
}
