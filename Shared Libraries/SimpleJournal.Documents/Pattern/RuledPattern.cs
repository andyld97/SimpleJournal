namespace SimpleJournal.Documents.Pattern
{
    public class RuledPattern : IPattern
    {
        public double ViewOffset { get; set; } = 50;

        public double StrokeWidth { get; set; } = 0.1;

        public SimpleJournal.Common.Data.Color Color { get; set; } = new Common.Data.Color(0x00, 0x00, 0x00); // black

        public void Reset()
        {
            RuledPattern newInstance = new RuledPattern();
            ViewOffset = newInstance.ViewOffset;
            StrokeWidth = newInstance.StrokeWidth;
            Color = newInstance.Color;
        }
    }
}
