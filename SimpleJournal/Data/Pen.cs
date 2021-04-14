namespace SimpleJournal.Data
{
    /// <summary>
    /// Represents a pencil in simplejournal
    /// </summary>
    public class Pen
    {
        public Color FontColor;

        public double Size;

        public Pen(Color fontColor, double size)
        {
            this.Size = size;
            this.FontColor = fontColor;
        }

        public Pen() : this(new Color(), Consts.StrokeSizes[0].Height)
        { }
    }

    public class Color
    {
        public byte A;

        public byte R;

        public byte G;

        public byte B;

        public Color(byte a, byte r, byte g, byte b)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public Color(byte r, byte g, byte b)  : this(255, r, g, b)
        { }

        // Black
        public Color() : this(255, 0, 0, 0)
        {

        }
    }
}
