using SimpleJournal.Common.Data;
using System.Windows.Media;

namespace SimpleJournal.Documents.UI
{
    public static class Consts
    {
        public const int TrapezeOffset = 30;
        public const int DefaultLineStrokeDashOffset = 1;
        public const int RulerCompensationOffset = 10;
        public static readonly DoubleCollection LineStrokeDottedDashArray = new DoubleCollection() { 0.03, 2 };
        public static readonly DoubleCollection LineStrokeDashedDashArray = new DoubleCollection() { 4, 3 };
        public static readonly string AnalyzerPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Analyzer.exe");

        #region Pen and Stroke Sizes
        /// <summary>
        /// The sizes for the stylus
        /// </summary>
        public static readonly List<Size> StrokeSizes = new List<Size>()
        {
            // Default size of InkCanvas
            new Size(1, 1),
            new Size(2.0031496062992127, 2.0031496062992127),
            new Size(3, 3),
            new Size(6, 6),
            new Size(9, 9),
        };

        /// <summary>
        /// The sizes for the rubber
        /// </summary>
        public static readonly List<Size> RubberSizes = new List<Size>()
        {
            new Size(8, 8),
            new Size(15, 15),
            new Size(30, 30),
            new Size(35, 35),
            new Size(40, 40),
        };

        /// <summary>
        /// The sizes for the textmarker
        /// </summary>
        public static readonly List<Size> TextMarkerSizes = new List<Size>()
        {
            new Size(15, 20),
            new Size(20, 20),
            new Size(30, 20),
            new Size(40, 20),
            new Size(50, 20),
        };

        #endregion
    }
}