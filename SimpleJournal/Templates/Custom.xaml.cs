using ImageMagick;
using SimpleJournal.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleJournal.Templates
{
    /// <summary>
    /// Interaction logic for Custom.xaml
    /// </summary>
    public partial class Custom : UserControl, IPaper
    {
        public byte[] PageBackground { get; private set; }

        public Orientation Orientation { get; private set; }

        public Custom(byte[] background, Orientation orientation)
        {
            InitializeComponent();
            this.Orientation = orientation;

            if (background != null)
            {
                PageBackground = background;
                // canvas.Background = new ImageBrush() { ImageSource = GeneralHelper.LoadImage(background), Stretch = Stretch.UniformToFill };

                ImageBackground.Source = GeneralHelper.LoadImage(background);
                canvas.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (orientation == Orientation.Landscape)
            {
                // Swap width and height
                double temp = Width;
                Width = Height;
                Height = temp;
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
                custom.Canvas.Children.Add(GeneralHelper.CloneElement(child));

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
            ImageBackground.Source = null;
            PageBackground = null;
            Canvas.Strokes.Clear();
            Canvas.Children.Clear();
        }
    }
}
