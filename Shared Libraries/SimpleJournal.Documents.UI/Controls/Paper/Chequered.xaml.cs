using SimpleJournal.Common;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleJournal.Documents.UI.Helper;
using Orientation = SimpleJournal.Common.Orientation;
using System.Windows;
using System.Reflection;
using SimpleJournal.Documents.UI.Extensions;
using SimpleJournal.Documents.Pattern;

namespace SimpleJournal.Documents.UI.Controls.Paper
{
    /// <summary>
    /// Interaktionslogik für Chequered.xaml
    /// </summary>
    public partial class Chequered : UserControl, IPaper
    {
        private IPattern pattern;

        public Chequered(Orientation orientation)
        {
            InitializeComponent();
            Orientation = orientation;

            // Load the correct drawing brush for the canvas
            if (Settings.Instance.UseOldChequeredPattern)
            {
                if (FindResource("OldChequeredBrush") is DrawingBrush drawingBrush)
                    canvas.Background = drawingBrush;
            }
            else
            {
                if (FindResource("CurrentChequeredBrush") is DrawingBrush drawingBrush)
                    canvas.Background = drawingBrush;
            }

            if (Orientation == Common.Orientation.Landscape)
            {
                // Swap width and height
                (Height, Width) = (Width, Height);
            }
        }

        #region ApplyBackgroundBrushSettings

        public void ApplyPattern(IPattern pattern)
        {
            this.pattern = pattern;
            if (pattern is ChequeredPattern chequeredPattern)
            {
                DrawingBrush brush = (DrawingBrush)FindResource(Settings.Instance.UseOldChequeredPattern ? "OldChequeredBrush" : "CurrentChequeredBrush");

                ApplyIntensity(brush, chequeredPattern.ViewPort);
                ApplyOffset(brush, chequeredPattern.ViewOffset);
                ApplyLineColor(brush, chequeredPattern.Color);
                ApplyStrokeThickness(brush, chequeredPattern.StrokeWidth);        
            }
        } 

        private void ApplyStrokeThickness(DrawingBrush brush, double value)
        {
            var g = brush.Drawing as GeometryDrawing;
            g.Pen.Thickness = value;
        }

        private void ApplyIntensity(DrawingBrush brush, double value)
        {
            var g = brush.Drawing as GeometryDrawing;
            var grp = g.Geometry as GeometryGroup;

            var lg1 = (grp.Children[0] as LineGeometry);
            lg1.StartPoint = new Point(0, value);
            lg1.EndPoint = new Point(value, value);

            var lg2 = (grp.Children[1] as LineGeometry);
            lg2.StartPoint = new Point(0, 0);
            lg2.EndPoint = new Point(0, value);
        }

        private void ApplyOffset(DrawingBrush brush, double value)
        {
            brush.Viewport = new Rect(0, 0, value, value);
        }

        private void ApplyLineColor(DrawingBrush brush, SimpleJournal.Common.Data.Color color)
        {
            (brush.Drawing as GeometryDrawing).Pen.Brush = new SolidColorBrush(color.ToColor());
        }

        #endregion

        public Orientation Orientation { get; set; }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Chequered;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public IPaper ClonePage(bool isReadonly)
        {
            Chequered chq = new Chequered(Orientation);

            if (isReadonly)
                chq.Canvas.EditingMode = InkCanvasEditingMode.None;
            chq.Canvas.Strokes = Canvas.Strokes.Clone();
            foreach (var child in Canvas.Children)
                chq.Canvas.Children.Add(UIHelper.CloneElement(child));

            chq.ApplyPattern(pattern);

            return chq;
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
