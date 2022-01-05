﻿using SimpleJournal.Controls;
using SimpleJournal.Templates;
using System;
using System.Windows.Controls;

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

    public static class PageHelper
    {
        public static IPaper ClonePage(IPaper page, bool isReadonly)
        {
            IPaper template = null;
            switch (page.Type)
            {
                case PaperType.Blanco: template = new Blanco(); break;
                case PaperType.Chequeued: template = new Chequered(); break;
                case PaperType.Ruled: template = new Ruled(); break;
                case PaperType.Dotted: template = new Dotted(); break;
                case PaperType.Custom: return (page as Custom).ClonePage(isReadonly); break;
            }

            // Make sure canvas is non editable
            if (isReadonly)
                template.Canvas.EditingMode = InkCanvasEditingMode.None;
            template.Canvas.Strokes = page.Canvas.Strokes.Clone();
            foreach (var child in page.Canvas.Children)
                template.Canvas.Children.Add(GeneralHelper.CloneElement(child));

            return template;
        }
    }
}
