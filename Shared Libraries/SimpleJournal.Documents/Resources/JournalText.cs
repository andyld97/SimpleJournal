using SimpleJournal.Common;
using SimpleJournal.Common.Data;

namespace SimpleJournal.Documents
{
    public class JournalText : JournalResource
    {
        public override JournalResourceType JournalResourceType => JournalResourceType.Text;

        public double FontSize = 12;

        public string FontFamily = "Times New Roman";

        public Color TextColor { get; set; } = new Color();

        public int RotationAngle = 0;

        public bool IsBold = false;

        public bool IsItalic = false;

        public bool IsUnderlined = false;

        public bool IsStrikeout = false;
    }
}
