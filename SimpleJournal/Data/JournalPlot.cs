using SimpleJournal.Controls;
using SimpleJournal.Common;
using SimpleJournal.Common.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Data
{
    public class JournalPlot : JournalResource
    {
        public override Type JournalResourceType => Type.Plot;

        public Direction PlotDirection { get; set; } = Direction.Left;

        public PlotMode PlotMode { get; set; } = PlotMode.Positive;

        public double RotationAngle { get; set; } = 0;

        public double StrokeThickness { get; set; } = 1;

        public Color StrokeColor { get; set; } = new Color();

        public override UIElement ConvertToUiElement()
        {
            var plt = new Plot()
            {
                DrawingDirection = PlotDirection,
                DrawingMode = PlotMode,
            };

            plt.SetValue(InkCanvas.LeftProperty, Left);
            plt.SetValue(InkCanvas.TopProperty, Top);
            plt.Width = Width;
            plt.Height = Height;
            plt.StrokeThickness = this.StrokeThickness;
            plt.Foreground = StrokeColor.ToColor();
            Canvas.SetZIndex(plt, ZIndex);

            plt.RenderTransform = new RotateTransform(RotationAngle);
            plt.RenderTransformOrigin = new Point(0.5, 0.5);

            return plt;
        }
    }
}