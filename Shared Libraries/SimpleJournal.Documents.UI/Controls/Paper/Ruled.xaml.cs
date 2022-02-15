using SimpleJournal.Common;
using System.Windows.Controls;
using SimpleJournal.Documents.UI.Controls;
using SimpleJournal.Documents.UI.Helper;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    /// <summary>
    /// Interaktionslogik für Ruled.xaml
    /// </summary>
    public partial class Ruled : UserControl, IPaper 
    {
        public Ruled()
        {
            InitializeComponent();
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Ruled;

        public DrawingCanvas Canvas => this.canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Ruled ruled = new Ruled();

            if (isReadonly)
                ruled.Canvas.EditingMode = InkCanvasEditingMode.None;
            ruled.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                ruled.Canvas.Children.Add(UIHelper.CloneElement(child));

            return ruled;
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
