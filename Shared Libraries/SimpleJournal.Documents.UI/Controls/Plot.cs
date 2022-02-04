using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleJournal.Common.Controls
{
    public class Plot : Canvas
    {
        private readonly string pathUP = "M 0 8 L 4 0 L 8 8 Z";
        private readonly string pathRight = "M 0 0 L 8 4 L 0 8 Z";
        private readonly string pathLeft = "M 0 4 L 8 0 L 8 8 Z";

        public PlotMode DrawingMode { get; set; } = PlotMode.Positive;

        public Direction DrawingDirection { get; set; } = Direction.Left;

        public double StrokeThickness { get; set; } = 1;

        public Color Foreground { get; set; } = Colors.Black;

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

            Children.Clear();

            if (DrawingMode == PlotMode.Negative)
            {
                // Vertical line
                Line verticalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness,
                    X1 = width / 2.0,
                    X2 = width / 2.0,
                    Y1 = space,
                    Y2 = height - space
                };

                // Horizontal line
                Line horizontalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness,
                    X1 = space,
                    X2 = width - space,
                    Y1 = height / 2.0,
                    Y2 = height / 2.0
                };

                Children.Add(verticalLine);
                Children.Add(horizontalLine);

                Path pathTriangleTop = new Path()
                {
                    Data = PathGeometry.Parse(pathUP),
                    Stroke = new SolidColorBrush(Foreground),
                    Fill = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness + 1
                };

                Path pathTriangleRight = new Path()
                {
                    Data = PathGeometry.Parse(pathRight),
                    Stroke = new SolidColorBrush(Foreground),
                    Fill = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness + 1
                };

                pathTriangleRight.SetValue(LeftProperty, width - (double)space);
                pathTriangleRight.SetValue(TopProperty, (height / 2.0) - 4.0);
                Children.Add(pathTriangleRight);

                pathTriangleTop.SetValue(LeftProperty, (width / 2.0) - 4.0);
                pathTriangleTop.SetValue(TopProperty, (double)space - 1.0);
                Children.Add(pathTriangleTop);
            }
            else if (DrawingMode == PlotMode.AxisXPosNegYPos)
            {
                // Vertical line
                Line verticalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness,
                    X1 = width / 2.0,
                    X2 = width / 2.0,
                    Y1 = space,
                    Y2 = height - space
                };

                // Horizontal line
                Line horizontalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness,
                    X1 = space,
                    X2 = width - space,
                    Y1 = height,
                    Y2 = height
                };

                Children.Add(verticalLine);
                Children.Add(horizontalLine);

                Path pathTriangleTop = new Path()
                {
                    Data = PathGeometry.Parse(pathUP),
                    Stroke = new SolidColorBrush(Foreground),
                    Fill = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness + 1
                };

                Path pathTriangleRight = new Path()
                {
                    Data = PathGeometry.Parse(pathRight),
                    Stroke = new SolidColorBrush(Foreground),
                    Fill = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness + 1
                };

                pathTriangleRight.SetValue(LeftProperty, width - (double)space);
                pathTriangleRight.SetValue(TopProperty, height - 4.0);
                Children.Add(pathTriangleRight);

                pathTriangleTop.SetValue(LeftProperty, (width / 2.0) - 4.0);
                pathTriangleTop.SetValue(TopProperty, (double)space - 1.0);
                Children.Add(pathTriangleTop);
            }
            else
            {
                // Vertical line
                Line verticalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Foreground),
                    StrokeThickness = this.StrokeThickness,
                    StrokeEndLineCap = PenLineCap.Square,
                    Y1 = space,
                    Y2 = height - space
                };

                if (DrawingDirection == Direction.Left)
                    verticalLine.X1 = verticalLine.X2 = space;
                else
                    verticalLine.X1 = verticalLine.X2 = width - space;

                // Horizontal line
                Line horizontalLine = new Line()
                {
                    Stroke = new SolidColorBrush(Foreground),
                    StrokeThickness = this.StrokeThickness,
                    X1 = space,
                    X2 = width - space,
                    Y1 = height - space,
                    Y2 = height - space,
                    StrokeEndLineCap = PenLineCap.Square
                };

                Path pathTriangleTop = new Path()
                {
                    Data = PathGeometry.Parse(pathUP),
                    Stroke = new SolidColorBrush(Foreground),
                    Fill = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness + 1
                };

                pathTriangleTop.SetValue(LeftProperty, verticalLine.X1 - 4.0);
                pathTriangleTop.SetValue(TopProperty, verticalLine.Y1);
                Children.Add(pathTriangleTop);

                Path pathTrianglelLeftRight = new Path()
                {
                    Data = PathGeometry.Parse(DrawingDirection != Direction.Right ? pathRight : pathLeft),
                    Stroke = new SolidColorBrush(Foreground),
                    Fill = new SolidColorBrush(Foreground),
                    StrokeThickness = StrokeThickness + 1
                };

                if (DrawingDirection != Direction.Left)
                    pathTrianglelLeftRight.SetValue(LeftProperty, (double)space);
                else
                    pathTrianglelLeftRight.SetValue(LeftProperty, width - space);

                pathTrianglelLeftRight.SetValue(TopProperty, height - space - 4.0);
                Children.Add(pathTrianglelLeftRight);
                Children.Add(verticalLine);
                Children.Add(horizontalLine);
            }
        }
    }
}