namespace SimpleJournal.Documents.Pattern
{
    public class ChequeredPattern : IPattern
    {
        public double ViewPort { get; set; } = 18;

        public double ViewOffset { get; set; } = 22;

        public double StrokeWidth { get; set; } = 0.5;

        public Common.Data.Color Color { get; set; } = new Common.Data.Color(0x80, 0x80, 0x80); // gray

        public bool HasDefaultValues
        {
            get
            {
                ChequeredPattern chequeredPattern = new ChequeredPattern();
                return ViewPort == chequeredPattern.ViewPort && ViewOffset == chequeredPattern.ViewOffset && StrokeWidth == chequeredPattern.StrokeWidth && Color == chequeredPattern.Color;
            }
        }

        public object Clone()
        {
            return new ChequeredPattern()
            {
                ViewPort = this.ViewPort,
                ViewOffset = this.ViewOffset,
                StrokeWidth = this.StrokeWidth,
                Color = this.Color
            };
        }

        public void Reset()
        {
            var newInstance = new ChequeredPattern();

            ViewPort = newInstance.ViewPort;
            ViewOffset = newInstance.ViewOffset;
            StrokeWidth = newInstance.StrokeWidth;
            Color = newInstance.Color;
        }
    }
}
