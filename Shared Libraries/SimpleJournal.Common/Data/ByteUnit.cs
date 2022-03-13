using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleJournal.Common.Data
{
    public class ByteUnit
    {
        public double Length { get; set; }

        public Unit Type { get; set; }

        public ByteUnit(double length, Unit type)
        {
            Length = length;
            Type = type;
        }

        public static ByteUnit Calculate(double value)
        {
            // Get right unit prefix
            int index = 1;
            double nValue = value;           

            while (nValue > 1024.0)
            {
                nValue /= 1024.0;
                index++;
            }

            return new ByteUnit(Math.Round(nValue, 2), (Unit)index);
        }

        public static ByteUnit Calculate(ByteUnit source, Unit toConvert)
        {
            // Calculate difference:
            int difference = (int)source.Type - (int)toConvert;
            return new ByteUnit(Math.Round(difference < 0 ? source.Length / Math.Pow(1024, (int)Math.Abs(difference)) : source.Length * Math.Pow(1024, (int)Math.Abs(difference)), 2), toConvert);
        }

        public override string ToString()
        {
            return $"{Length:G} {Type}";
        }

        public string ToString(bool inSeconds)
        {
            return $"{ToString()}/s";
        }
    }
}