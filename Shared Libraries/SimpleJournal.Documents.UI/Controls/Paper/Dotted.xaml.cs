using SimpleJournal.Common;
using System.Windows.Controls;
using SimpleJournal.Documents.UI.Controls;
using SimpleJournal.Documents.UI.Helper;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    /// <summary>
    /// Interaction logic for Dottet.xaml
    /// </summary>
    public partial class Dotted : UserControl, IPaper
    {
        public Dotted()
        {
            InitializeComponent();
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Dotted;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Dotted dotted = new Dotted();

            if (isReadonly)
                dotted.Canvas.EditingMode = InkCanvasEditingMode.None;
            dotted.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                dotted.Canvas.Children.Add(UIHelper.CloneElement(child));

            return dotted;
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
    }
}
