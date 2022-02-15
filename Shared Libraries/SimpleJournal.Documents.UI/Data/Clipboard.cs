using SimpleJournal.Documents.UI.Helper;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;

namespace SimpleJournal.Documents.UI.Data
{
    public class Clipboard
    {
        /// <summary>
        /// Elements like shapes, images ...
        /// </summary>
        public List<UIElement> Children { get; set; } = new List<UIElement>();

        /// <summary>
        /// Position of elements
        /// </summary>
        public List<Point> ChildPoints { get; set; } = new List<Point>();

        /// <summary>
        /// Drawn strokes from user
        /// </summary>
        public StrokeCollection Strokes { get; set; } = new StrokeCollection();

        /// <summary>
        /// Determines if the clipboard is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (Children == null && ChildPoints == null && Strokes == null)
                    return true;

                return (ChildPoints.Count == 0 && Children.Count == 0 && Strokes.Count == 0);
            }
        }

        /// <summary>
        /// Clears everything out of the clipboard
        /// </summary>
        public void Clear()
        {
            Children.Clear();
            ChildPoints.Clear();
            Strokes = new StrokeCollection();
        }

        public void Renew()
        {
            Strokes = Strokes.Clone();
            List<UIElement> newElements = new List<UIElement>();
            foreach (UIElement child in Children)
                newElements.Add(UIHelper.CloneElement(child));

            Children.Clear();
            Children = newElements;
        }
    }
}
