namespace SimpleJournal.Documents.Pattern
{
    public class RuledPattern : IPattern
    {
        public double ViewOffset { get; set; } = 50;

        public double StrokeWidth { get; set; } = 0.1;

        public SimpleJournal.Common.Data.Color Color { get; set; } = new Common.Data.Color(0x00, 0x00, 0x00); // black

        public bool HasDefaultValues
        {
            get
            {
                RuledPattern ruledPattern = new RuledPattern();
                return ViewOffset == ruledPattern.ViewOffset && StrokeWidth == ruledPattern.StrokeWidth && Color == ruledPattern.Color;
            }
        }

        public object Clone()
        {
            return new RuledPattern()
            {
                ViewOffset = this.ViewOffset,
                StrokeWidth = this.StrokeWidth,
                Color = this.Color
            };
        }

        public void Reset()
        {
            RuledPattern newInstance = new RuledPattern();
            ViewOffset = newInstance.ViewOffset;
            StrokeWidth = newInstance.StrokeWidth;
            Color = newInstance.Color;
        }
    }
}
