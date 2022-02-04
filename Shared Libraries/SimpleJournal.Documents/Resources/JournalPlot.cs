using SimpleJournal.Common;
using SimpleJournal.Common.Data;

namespace SimpleJournal.Documents.Resources
{
    public class JournalPlot : JournalResource
    {
        public override JournalResourceType JournalResourceType => JournalResourceType.Plot;

        public Direction PlotDirection { get; set; } = Direction.Left;

        public PlotMode PlotMode { get; set; } = PlotMode.Positive;

        public double RotationAngle { get; set; } = 0;

        public double StrokeThickness { get; set; } = 1;

        public Color StrokeColor { get; set; } = new Color();
    }
}