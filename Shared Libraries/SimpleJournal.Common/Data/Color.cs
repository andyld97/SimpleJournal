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

        public static bool operator ==(Color c1, Color c2)
        {
            return c1.A == c2.A &&
                   c1.R == c2.R &&
                   c1.G == c2.G &&
                   c2.B == c2.B;
        }

        public static bool operator !=(Color c1, Color c2)
        {
            return c1 != c2;
        }
    }
}
