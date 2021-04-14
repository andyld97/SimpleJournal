using SimpleJournal.Controls;

namespace SimpleJournal
{
    public interface IPaper
    {
        Format Format { get;  }

        PaperType Type { get; }

        DrawingCanvas Canvas { get;  }

        PageSplitter Border { get; set; }

        void SetDebug(bool state = true);
    }

    public enum Format
    {
        A4
    }

    public enum PaperType
    {
        Blanco,
        Chequeued,
        Ruled
    }
}
