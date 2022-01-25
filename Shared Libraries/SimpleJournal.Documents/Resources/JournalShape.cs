using SimpleJournal.Common;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleJournal.Documents.Resources
{
    public class JournalShape : JournalResource
    {
        private static readonly XmlDocument dummyDoc = new XmlDocument();

        public override JournalResourceType JournalResourceType => JournalResourceType.Shape;

        [XmlElement("shape")]
        public XmlCDataSection ShapeCData
        {
            get => dummyDoc.CreateCDataSection(ShapeXML);
            set => ShapeXML = value?.Data;
        }

        [XmlIgnore]
        public string ShapeXML { get; set; }
    }
}
