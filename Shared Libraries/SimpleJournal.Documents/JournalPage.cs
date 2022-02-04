using SimpleJournal.Common;
using System.Xml.Serialization;

namespace SimpleJournal.Documents
{
    [XmlInclude(typeof(PdfJournalPage))]
    public class JournalPage
    {
        private byte[] cache = null;

        /// <summary>
        /// Determines if there are any resources to display
        /// </summary>
        public bool HasAdditionalResources => JournalResources.Count != 0;

        /// <summary>
        /// Represents stroke collection of InkCanvas as base64 (only used for converting old documents)
        /// </summary>
        public string Base64Data
        {
            get => string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Data = Convert.FromBase64String(value);
            }
        }

        /// <summary>
        /// Additonal data e.g images, text
        /// </summary>
        public List<JournalResource> JournalResources = new List<JournalResource>();

        /// <summary>
        /// The format of the page (currently only A4 is possible)
        /// </summary>
        public Format PageFormat { get; set; } = Format.A4;

        /// <summary>
        /// The pattern of the page (i.e. chequered)
        /// </summary>
        public PaperType PaperPattern { get; set; } = PaperType.Chequeued;

        public byte[] Data
        {
            get => cache;
            set
            {
                if (value != null)
                    cache = value;
            }
        }
    }
}
