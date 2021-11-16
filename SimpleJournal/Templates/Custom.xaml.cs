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
    public partial class Custom : Page, IPaper
    {
        public byte[] PageBackground { get; private set; }

        public Custom(byte[] background)
        {
            InitializeComponent();

            if (background != null)
            {
                PageBackground = background;
                // canvas.Background = new ImageBrush() { ImageSource = GeneralHelper.LoadImage(background), Stretch = Stretch.UniformToFill };

                ImageBackground.Source = GeneralHelper.LoadImage(background);
                canvas.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Custom;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Custom custom = new Custom(PageBackground);

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
    }
}
