namespace SimpleJournal.Documents.Pattern
{
    public class DottedPattern : IPattern
    {
        public double ViewPort { get; set; } = 20;

        public double Radius { get; set; } = 0.5;

        public double StrokeWidth { get; set; } = 2;

        public Common.Data.Color Color { get; set; } = new Common.Data.Color(0x00, 0x00, 0x00); // black

        public bool HasDefaultValues
        {
            get
            {
                DottedPattern dottedPattern = new DottedPattern();
                return ViewPort == dottedPattern.ViewPort && Radius == dottedPattern.Radius && StrokeWidth == dottedPattern.StrokeWidth && Color == dottedPattern.Color;
            }
        }

        public object Clone()
        {
            return new DottedPattern()
            {
                ViewPort = this.ViewPort,
                Radius = this.Radius,
                StrokeWidth = this.StrokeWidth,
                Color = this.Color,
            };
        }

        public void Reset()
        {
            var newInstance = new DottedPattern();
            ViewPort = newInstance.ViewPort;
            Radius = newInstance.Radius;
            StrokeWidth = newInstance.StrokeWidth;
            Color = newInstance.Color;
        }
    }
}
