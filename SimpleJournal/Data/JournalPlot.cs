using SimpleJournal.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SimpleJournal.Data.Enums;

namespace SimpleJournal.Data
{
    public class JournalPlot : JournalResource
    {
        public override Type JournalResourceType => Type.Plot;

        public Direction PlotDirection { get; set; } = Direction.Left;

        public PlotMode PlotMode { get; set; } = PlotMode.Positive;

        public int ZIndex { get; set; }

        public double RotationAngle { get; set; } = 0;

        public double StrokeThickness { get; set; } = 1;

        public byte ForegroundA = 255, ForegroundR = 0, ForegroundB = 0, ForegroundG = 0;

        public override UIElement ConvertToUiElement()
        {
            var plt = new Plot()
            {
                DrawingDirection = PlotDirection,
                DrawingMode = PlotMode,
            };

            plt.SetValue(DrawingCanvas.LeftProperty, Left);
            plt.SetValue(DrawingCanvas.TopProperty, Top);
            plt.Width = Width;
            plt.Height = Height;
            plt.StrokeThickness = this.StrokeThickness;
            plt.Foreground = System.Windows.Media.Color.FromArgb(ForegroundA, ForegroundR, ForegroundG, ForegroundB);
            Canvas.SetZIndex(plt, ZIndex);

            plt.RenderTransform = new RotateTransform(RotationAngle);
            plt.RenderTransformOrigin = new Point(0.5, 0.5);

            return plt;
        }
    }
}