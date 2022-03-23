using SimpleJournal.Common;
using System.Xml.Serialization;

namespace SimpleJournal.Documents.Resources
{
    /// <summary>
    /// A journal image resouce
    /// </summary>
    public class JournalImage : JournalResource
    {
        [XmlElement("type")]
        public override JournalResourceType JournalResourceType => JournalResourceType.Image;

        public int RotationAngle { get; set; } = 0;

        public Common.Stretch Stretch { get; set; } = Stretch.Fill;
    }
}
