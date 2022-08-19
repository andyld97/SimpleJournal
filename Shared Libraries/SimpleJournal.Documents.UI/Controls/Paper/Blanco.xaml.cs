using SimpleJournal.Common;
using System.Windows.Controls;
using SimpleJournal.Documents.UI.Helper;
using Orientation = SimpleJournal.Common.Orientation;
using SimpleJournal.Documents.Pattern;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    /// <summary>
    /// Interaktionslogik für Blanco.xaml
    /// </summary>
    public partial class Blanco : UserControl, IPaper
    {
        public Blanco(Orientation orientation)
        {
            InitializeComponent();
            Orientation = orientation;

            if (Orientation == Common.Orientation.Landscape)
            {
                // Swap width and height
                (Height, Width) = (Width, Height);
            }
        }

        public Orientation Orientation { get; set; }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Blanco;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Blanco blanco = new Blanco(Orientation);

            if (isReadonly)
                blanco.Canvas.EditingMode = InkCanvasEditingMode.None;
            blanco.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                blanco.Canvas.Children.Add(UIHelper.CloneElement(child));

            return blanco;
        }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);
        }

        public void Dispose()
        {
            Content = null;
            Border = null;
            Canvas.Strokes.Clear();
            Canvas.Children.Clear();
        }

        public void ApplyPattern(IPattern pattern)
        {
            throw new NotImplementedException();
        }
    }
}