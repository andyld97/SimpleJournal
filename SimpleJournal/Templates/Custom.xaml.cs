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
                canvas.Background = new ImageBrush() { ImageSource = LoadImage(background) };
            }
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
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
