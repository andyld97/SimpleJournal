using SimpleJournal.Common;
using SimpleJournal.Documents.Pattern;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    public interface IPaper : IDisposable
    {
        Format Format { get;  }

        PaperType Type { get; }

        DrawingCanvas Canvas { get;  }

        void ApplyPattern(IPattern pattern);

        Orientation Orientation { get; set; }

        PageSplitter Border { get; set; }

        void SetDebug(bool state = true);

        IPaper ClonePage(bool isReadonly);
    }
}
