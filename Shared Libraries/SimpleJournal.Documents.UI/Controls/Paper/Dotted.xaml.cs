using SimpleJournal.Common;
using System.Windows.Controls;
using SimpleJournal.Documents.UI.Helper;
using Orientation = SimpleJournal.Common.Orientation;
using SimpleJournal.Documents.Pattern;
using System.Windows.Media;
using SimpleJournal.Documents.UI.Extensions;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    /// <summary>
    /// Interaction logic for Dottet.xaml
    /// </summary>
    public partial class Dotted : UserControl, IPaper
    {
        public Dotted(Orientation orientation)
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

        public PaperType Type => PaperType.Dotted;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Dotted dotted = new Dotted(Orientation);

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

        public void ApplyPattern(IPattern pattern)
        {
            if (pattern is DottedPattern dp)
            {
                var brush = FindResource("DottetBrush") as DrawingBrush;

                brush.Viewport = new System.Windows.Rect(0, 0, dp.ViewPort, dp.ViewPort);
                var gd = (brush.Drawing as GeometryDrawing);
                var gg = gd.Geometry as GeometryGroup;
                var eg = (gg.Children[0] as EllipseGeometry);
                eg.RadiusX =  eg.RadiusY = dp.Radius;

                gd.Pen.Thickness = dp.StrokeWidth;
                gd.Pen.Brush = new SolidColorBrush(dp.Color.ToColor());
            }
        }
    }
}
