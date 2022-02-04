namespace SimpleJournal.Common.Data
{
    public class Color
    {
        public byte A { get; set; }

        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public Color(byte r, byte g, byte b) : this(255, r, g, b)
        { }

        // Black
        public Color() : this(255, 0, 0, 0)
        { }
    }
}
