namespace SimpleJournal.Common.Data
{
    public class Run
    {
        public string Text { get; set; }

        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }

        public bool IsUnderline { get; set; }

        public double? FontSize { get; set; } = null;

        public string FontFamily { get; set; }

        public Data.Color Foreground { get; set; }

        public bool IsNewLine { get; set; }

        public Run()
        { }

        public Run(string text)
        {
            Text = text;
        }
    }

    public class LineBreak : Run
    {
        public LineBreak()
        {
            IsNewLine = true;
        }
    }
}
