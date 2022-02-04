using System.Xml.Serialization;
using SimpleJournal.Common;
using SimpleJournal.Documents.Resources;

namespace SimpleJournal.Documents
{
    /// <summary>
    /// Represents a journal resource to save and load journals
    /// </summary>
    [XmlInclude(typeof(JournalImage))]
    [XmlInclude(typeof(JournalText))]
    [XmlInclude(typeof(JournalShape))]
    [XmlInclude(typeof(JournalPlot))]
    public abstract class JournalResource
    {
        [XmlIgnore]
        protected byte[] cache = null;       

        /// <summary>
        /// Represents the X-Coardinate
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Represents the Y-Coardinate
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Represents the size.width of this resource if any
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Represents the size.height of this resource if any
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Represents the z order of this resource
        /// </summary>
        public int ZIndex { get; set; } = 0;

        /// <summary>
        /// Journal-Data in Base64-String representation (only used for converting old documents)
        /// </summary>
        public string DataBase64
        {
            get => string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Data = Convert.FromBase64String(value);
            }
        }

        [XmlElement("type")]
        public abstract JournalResourceType JournalResourceType { get; }

        public virtual byte[] Data
        {
            get => cache;
            set => cache = value;
        }  
    }
}
