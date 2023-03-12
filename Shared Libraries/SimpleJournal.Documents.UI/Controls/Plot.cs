using SimpleJournal.Common;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleJournal.Documents.UI.Controls
{
    public class Plot : Canvas
    {
        private readonly string pathUP = "M 0 8 L 4 0 L 8 8 Z";
        private readonly string pathRight = "M 0 0 L 8 4 L 0 8 Z";
        private readonly string pathLeft = "M 0 4 L 8 0 L 8 8 Z";

        #region Properties

        public PlotMode DrawingMode { get; set; } = PlotMode.Positive;

        public Direction DrawingDirection { get; set; } = Direction.Left;

        public double StrokeThickness { get; set; } = 1;

        public Color Foreground { get; set; } = Colors.Black;

        #endregion

        #region Controls

        // PlotMode.Negative:
        private Line verticalLine = new Line();
        private Line horizontalLine = new Line();

        private Path pathTriangleTop = new Path();
        private Path pathTriangleRight = new Path();

        // PlotMode.AxisXPosNegYPos
        private Line verticalLineAXPNYP = new Line();
        private Line horizontalLineAXPNYP = new Line();
        private Path pathTriangleTopAXPNYP = new Path();
        private Path pathTriangleRightAXPNYP = new Path();

        // PlotMode Else
        private Line verticalLineOther = new Line();
        private Line horizontalLineOther = new Line();
        private Path pathTriangleTopOther = new Path();
        private Path pathTrianglelLeftRightOther = new Path();
        #endregion

        public Plot()
        {
            Loaded += Coard_Loaded;
            SizeChanged += Coard_SizeChanged;
        }

        private void Coard_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            RenderPlot();
        }

        private void Coard_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            RenderPlot();
        }

        public void RenderPlot(double space = 0.0)
        {
            double width = ActualWidth;
            double height = ActualHeight;

            // Children.Clear();

            if (DrawingMode == PlotMode.Negative)
            {
                // Vertical line
                verticalLine.Stroke = new SolidColorBrush(Foreground);
                verticalLine.StrokeThickness = StrokeThickness;
                verticalLine.X1 = width / 2.0;
                verticalLine.X2 = width / 2.0;
                verticalLine.Y1 = space;
                verticalLine.Y2 = height - space;

                // Horizontal line
                horizontalLine.Stroke = new SolidColorBrush(Foreground);
                horizontalLine.StrokeThickness = StrokeThickness;
                horizontalLine.X1 = space;
                horizontalLine.X2 = width - space;
                horizontalLine.Y1 = height / 2.0;
                horizontalLine.Y2 = height / 2.0;

                if (!Children.Contains(verticalLine))
                    Children.Add(verticalLine);

                if (!Children.Contains(horizontalLine))
                    Children.Add(horizontalLine);

                pathTriangleTop.Data = PathGeometry.Parse(pathUP);
                pathTriangleTop.Stroke = new SolidColorBrush(Foreground);
                pathTriangleTop.Fill = new SolidColorBrush(Foreground);
                pathTriangleTop.StrokeThickness = StrokeThickness + 1;

                pathTriangleRight.Data = PathGeometry.Parse(pathRight);
                pathTriangleRight.Stroke = new SolidColorBrush(Foreground);
                pathTriangleRight.Fill = new SolidColorBrush(Foreground);
                pathTriangleRight.StrokeThickness = StrokeThickness + 1;

                pathTriangleRight.SetValue(LeftProperty, width - (double)space);
                pathTriangleRight.SetValue(TopProperty, (height / 2.0) - 4.0);
                if (!Children.Contains(pathTriangleRight))
                    Children.Add(pathTriangleRight);

                pathTriangleTop.SetValue(LeftProperty, (width / 2.0) - 4.0);
                pathTriangleTop.SetValue(TopProperty, (double)space - 1.0);
                if (!Children.Contains(pathTriangleTop))
                    Children.Add(pathTriangleTop);
            }
            else if (DrawingMode == PlotMode.AxisXPosNegYPos)
            {
                // Vertical line
                verticalLineAXPNYP.Stroke = new SolidColorBrush(Foreground);
                verticalLineAXPNYP.StrokeThickness = StrokeThickness;
                verticalLineAXPNYP.X1 = width / 2.0;
                verticalLineAXPNYP.X2 = width / 2.0;
                verticalLineAXPNYP.Y1 = space;
                verticalLineAXPNYP.Y2 = height - space;

                // Horizontal line
                horizontalLineAXPNYP.Stroke = new SolidColorBrush(Foreground);
                horizontalLineAXPNYP.StrokeThickness = StrokeThickness;
                horizontalLineAXPNYP.X1 = space;
                horizontalLineAXPNYP.X2 = width - space;
                horizontalLineAXPNYP.Y1 = height;
                horizontalLineAXPNYP.Y2 = height;

                if (!Children.Contains(verticalLineAXPNYP))
                    Children.Add(verticalLineAXPNYP);

                if (!Children.Contains(horizontalLineAXPNYP))
                    Children.Add(horizontalLineAXPNYP);

                pathTriangleTopAXPNYP.Data = PathGeometry.Parse(pathUP);
                pathTriangleTopAXPNYP.Stroke = new SolidColorBrush(Foreground);
                pathTriangleTopAXPNYP.Fill = new SolidColorBrush(Foreground);
                pathTriangleTopAXPNYP.StrokeThickness = StrokeThickness + 1;

                pathTriangleRightAXPNYP.Data = PathGeometry.Parse(pathRight);
                pathTriangleRightAXPNYP.Stroke = new SolidColorBrush(Foreground);
                pathTriangleRightAXPNYP.Fill = new SolidColorBrush(Foreground);
                pathTriangleRightAXPNYP.StrokeThickness = StrokeThickness + 1;
                pathTriangleRightAXPNYP.SetValue(LeftProperty, width - (double)space);
                pathTriangleRightAXPNYP.SetValue(TopProperty, height - 4.0);
                if (!Children.Contains(pathTriangleRightAXPNYP))
                    Children.Add(pathTriangleRightAXPNYP);

                pathTriangleTopAXPNYP.SetValue(LeftProperty, (width / 2.0) - 4.0);
                pathTriangleTopAXPNYP.SetValue(TopProperty, (double)space - 1.0);
                if (!Children.Contains(pathTriangleTopAXPNYP))
                    Children.Add(pathTriangleTopAXPNYP);
            }
            else
            {
                // Vertical line
                verticalLineOther.Stroke = new SolidColorBrush(Foreground);
                verticalLineOther.StrokeThickness = this.StrokeThickness;
                verticalLineOther.StrokeEndLineCap = PenLineCap.Square;
                verticalLineOther.Y1 = space;
                verticalLineOther.Y2 = height - space;

                if (DrawingDirection == Direction.Left)
                    verticalLineOther.X1 = verticalLineOther.X2 = space;
                else
                    verticalLineOther.X1 = verticalLineOther.X2 = width - space;

                // Horizontal line
                horizontalLineOther.Stroke = new SolidColorBrush(Foreground);
                horizontalLineOther.StrokeThickness = this.StrokeThickness;
                horizontalLineOther.X1 = space;
                horizontalLineOther.X2 = width - space;
                horizontalLineOther.Y1 = height - space;
                horizontalLineOther.Y2 = height - space;
                horizontalLineOther.StrokeEndLineCap = PenLineCap.Square;

                pathTriangleTopOther.Data = PathGeometry.Parse(pathUP);
                pathTriangleTopOther.Stroke = new SolidColorBrush(Foreground);
                pathTriangleTopOther.Fill = new SolidColorBrush(Foreground);
                pathTriangleTopOther.StrokeThickness = StrokeThickness + 1;

                pathTriangleTopOther.SetValue(LeftProperty, verticalLineOther.X1 - 4.0);
                pathTriangleTopOther.SetValue(TopProperty, verticalLineOther.Y1);

                if (!Children.Contains(pathTriangleTopOther))
                    Children.Add(pathTriangleTopOther);

                pathTrianglelLeftRightOther.Data = PathGeometry.Parse(DrawingDirection != Direction.Right ? pathRight : pathLeft);
                pathTrianglelLeftRightOther.Stroke = new SolidColorBrush(Foreground);
                pathTrianglelLeftRightOther.Fill = new SolidColorBrush(Foreground);
                pathTrianglelLeftRightOther.StrokeThickness = StrokeThickness + 1;

                if (DrawingDirection != Direction.Left)
                    pathTrianglelLeftRightOther.SetValue(LeftProperty, (double)space);
                else
                    pathTrianglelLeftRightOther.SetValue(LeftProperty, width - space);

                pathTrianglelLeftRightOther.SetValue(TopProperty, height - space - 4.0);
                if (!Children.Contains(pathTrianglelLeftRightOther))
                    Children.Add(pathTrianglelLeftRightOther);
                
                if (!Children.Contains(verticalLineOther)) 
                    Children.Add(verticalLineOther);

                if (!Children.Contains(horizontalLineOther))
                    Children.Add(horizontalLineOther);
            }
        }
    }
}