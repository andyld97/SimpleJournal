using SimpleJournal.Common;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    public interface IPaper : IDisposable
    {
        Format Format { get;  }

        PaperType Type { get; }

        DrawingCanvas Canvas { get;  }

        Orientation Orientation { get; set; }

        PageSplitter Border { get; set; }

        void SetDebug(bool state = true);

        IPaper ClonePage(bool isReadonly);
    }
}
