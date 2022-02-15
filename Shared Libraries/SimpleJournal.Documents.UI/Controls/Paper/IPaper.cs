using SimpleJournal.Common;
using System;
using SimpleJournal.Documents.UI.Controls;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    public interface IPaper : IDisposable
    {
        Format Format { get;  }

        PaperType Type { get; }

        DrawingCanvas Canvas { get;  }

        PageSplitter Border { get; set; }

        void SetDebug(bool state = true);

        IPaper ClonePage(bool isReadonly);
    }
}
