using SimpleJournal.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SimpleJournal.Data
{
    /// <summary>
    /// Represents a journal resource to save and load journals
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(JournalImage))]
    [XmlInclude(typeof(JournalText))]
    [XmlInclude(typeof(JournalShape))]
    [XmlInclude(typeof(JournalPlot))]
    public abstract class JournalResource
    {
        [XmlIgnore]
        protected byte[] cache = null;

        /// <summary>
        /// Type of this resource
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Image type
            /// </summary>
            Image = 0,

            /// <summary>
            /// Text type
            /// </summary>
            Text = 1,

            /// <summary>
            /// A rectangle, circle, ellipse, poligon  or something else which derrivered from shape
            /// </summary>
            Shape = 2,

            /// <summary>
            /// Simple plot
            /// </summary>
            Plot = 3
        }

        /// <summary>
        /// Represents the X-Coardinate
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Represents the Y-Coardinate
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Represents the size.width of this resource if any
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Represents the size.height of this resource if any
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Journal-Data in Base64-String representation (only used for converting old documents)
        /// </summary>
        public string DataBase64
        {
            get => string.Empty;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Data = Convert.FromBase64String(value);
            }
        }

        public abstract Type JournalResourceType { get; }

        public virtual byte[] Data
        {
            get => cache;
            set => cache = value;
        }

        public abstract UIElement ConvertToUiElement();

        public static void AddJournalResourceToCanvas(JournalResource resource, DrawingCanvas ink)
        {
            ink.LoadChildren(resource.ConvertToUiElement());
        }

        /// <summary>
        /// Takes UIElement, e.g. the children from InkCanvas and converts them to journalResources
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static JournalResource ConvertFromUIElement(UIElement element)
        {
            if (element is Image img)
                return img.ConvertImage();
            else if (element is TextBlock text)
                return text.ConvertText();
            else if (element is Shape shape)
                return shape.ConvertShape();
            else if (element is Plot plot)
                return plot.ConvertPlot();

            return null;
        }
    }
}
