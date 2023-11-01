using System.Text.Json.Serialization;

namespace SimpleJournal.Documents.PDF
{
    public class PdfConversationOptions
    {
        [JsonPropertyName("pages_per_journal")]
        public int PagesPerJournal { get; set; } = Consts.MaxPDFPagesPerJournal;

        [JsonPropertyName("use_page_range")]
        public bool UsePageRange { get; set; }

        /// <summary>
        /// One-based index of the start page (only used if UsePageRange is set to true)
        /// </summary>
        [JsonPropertyName("start_page")]
        public int StartPage { get; set; } = 0;

        /// <summary>
        /// This is required for detecting incompatible versions
        /// </summary>
        [JsonPropertyName("sj_version")]
        public string CurrentSimpleJounalVersion { get; set; }

        /// <summary>
        /// One-based index of the last page (only used if UsePageRange is set to true)
        /// </summary>
        [JsonPropertyName("last_page")]
        public int LastPage { get; set; } = Consts.MaxPDFPagesPerJournal;
    }
}