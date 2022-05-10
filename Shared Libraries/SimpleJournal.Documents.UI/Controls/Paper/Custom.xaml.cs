using SimpleJournal.Common;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleJournal.Documents.UI.Helper;
using Orientation = SimpleJournal.Common.Orientation;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    /// <summary>
    /// Interaction logic for Custom.xaml
    /// </summary>
    public partial class Custom : UserControl, IPaper
    {
        public byte[] PageBackground { get; private set; }

        public Orientation Orientation { get; set; }

        public Custom(byte[] background, Common.Orientation orientation)
        {
            InitializeComponent();
            this.Orientation = orientation;

            if (background != null)
            {
                PageBackground = background;
                // canvas.Background = new ImageBrush() { ImageSource = GeneralHelper.LoadImage(background), Stretch = Stretch.Uniform };

                ImageBackground.Source = ImageHelper.LoadImage(background);
                canvas.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (orientation == Common.Orientation.Landscape)
            {
                // Swap width and height
                double width = Canvas.Width;
                Canvas.Width = Canvas.Height;
                Canvas.Height = Width;
                (Height, Width) = (Width, Height);
            }
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Custom;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Custom custom = new Custom(PageBackground, Orientation);

            if (isReadonly)
                custom.Canvas.EditingMode = InkCanvasEditingMode.None;
            custom.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                custom.Canvas.Children.Add(UIHelper.CloneElement(child));

            return custom;
        }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);
        }

        public void Dispose()
        {
            Content = null;
            Border = null;
            Canvas.Background = null;//   ImageBackground.Source = null;
            PageBackground = null;
            Canvas.Strokes.Clear();
            Canvas.Children.Clear();
        }
    }
}
