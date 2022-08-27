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
    /// Interaktionslogik für Ruled.xaml
    /// </summary>
    public partial class Ruled : UserControl, IPaper 
    {
        private IPattern pattern;

        public Ruled(Orientation orientation)
        {
            InitializeComponent();
            Orientation = orientation; 

            if (Orientation == Common.Orientation.Landscape)
            {
                // Swap width and height
                (Height, Width) = (Width, Height);
            }
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Ruled;

        public DrawingCanvas Canvas => this.canvas;

        public Orientation Orientation { get; set; }

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Ruled ruled = new Ruled(Orientation);

            if (isReadonly)
                ruled.Canvas.EditingMode = InkCanvasEditingMode.None;
            ruled.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                ruled.Canvas.Children.Add(UIHelper.CloneElement(child));

            ruled.ApplyPattern(pattern);

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

        public void ApplyPattern(IPattern pattern)
        {
            this.pattern = pattern;
             if (pattern is RuledPattern rp)
            {
                var brush = FindResource("RuledBrush") as DrawingBrush;
                brush.Viewport = new System.Windows.Rect(0, 0, rp.ViewOffset, rp.ViewOffset);
                var g =  brush.Drawing as GeometryDrawing;
                var grp = g.Geometry as GeometryGroup;
                var lg = grp.Children[0] as LineGeometry;
                lg.StartPoint = new System.Windows.Point(0, 0);
                lg.EndPoint = new System.Windows.Point(rp.ViewOffset, 0);
                g.Pen.Brush = new SolidColorBrush(rp.Color.ToColor());
                g.Pen.Thickness = rp.StrokeWidth;
            }
        }
    }
}
