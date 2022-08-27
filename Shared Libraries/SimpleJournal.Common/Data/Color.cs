using System.Data;
using System.Numerics;

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
            if (c1 is null)
                return c2 is null;

            return c1.Equals(c2);
        }

        public static bool operator !=(Color c1, Color c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return obj is Color c2 ?
                   A == c2.A &&
                   R == c2.R &&
                   G == c2.G &&
                   B == c2.B : false;
        }

        public override int GetHashCode()
        {
            return (A, R, G, B).GetHashCode();
        }
    }
}
