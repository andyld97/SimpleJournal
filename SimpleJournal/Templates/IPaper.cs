using SimpleJournal.Controls;
using SimpleJournal.Common;
using System;

namespace SimpleJournal
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
