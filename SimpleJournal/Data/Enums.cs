using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJournal.Data
{
    public static class Enums
    {
        public enum ShapeType
        {
            Rectangle,
            Circle,
            TriangleEquiliteral,
            TriangleLeft90,
            TriangleRight90,
            TriangleLeft180,
            TriangleRight180,
            TriangleEquiliteral180,
            Trapeze
        }

        public enum PlotMode
        {
            Positive,
            Negative
        }

        public enum Direction
        {
            Left,
            Right
        }
    }
}
