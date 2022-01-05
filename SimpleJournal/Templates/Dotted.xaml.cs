using SimpleJournal.Controls;
using System.Windows.Controls;

namespace SimpleJournal.Templates
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
                dotted.Canvas.Children.Add(GeneralHelper.CloneElement(child));

            return dotted;
        }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);
        }
    }
}
