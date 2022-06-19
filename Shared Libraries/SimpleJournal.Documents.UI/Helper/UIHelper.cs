using SimpleJournal.Common;
using SimpleJournal.Documents.UI.Controls;
using SimpleJournal.Documents.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace SimpleJournal.Documents.UI.Helper
{
    public static class UIHelper
    {
        public static StrokeCollection Reverse(this StrokeCollection sc)
        {
            if (sc == null)
                return null;

            StrokeCollection result = new StrokeCollection();
            for (int i = sc.Count - 1; i >= 0; i--)
                result.Add(sc[i]);

            return result;
        }

        public static Point ToPoint(this StylusPoint point)
        {
            return new Point(point.X, point.Y);
        }

        public static Size ToSize(this Common.Data.Size size)
        {
            return new Size(size.Width, size.Height);
        }

        public static double Angle(this Point pt, Point other)
        {
            double a = Math.Abs(pt.X - other.X);
            double result = Math.Acos(a / Math.Sqrt(Math.Pow(a, 2) + Math.Pow(Math.Abs(pt.Y - other.Y), 2)));

            if (other.X < pt.X)
                return Angle(other, pt) - 180;

            if (other.Y < pt.Y)
                result = -result;

            return result * (180 / Math.PI);
        }

        public static System.Windows.Media.Color BuildConstrastColor(this System.Windows.Media.Color color)
        {
            // Counting the perceptive luminance - human eye favors green color... 
            var l = 0.2126 * color.ScR + 0.7152 * color.ScG + 0.0722 * color.ScB;
            return l < 0.5 ? Colors.White : Colors.Black;
        }

        public static void ClearAll(this ObservableCollection<UIElement> collection, DrawingCanvas canvas)
        {
            List<UIElement> childs = new List<UIElement>();
            foreach (UIElement child in collection)
                childs.Add(child);
            foreach (UIElement child in childs)
                canvas.Children.Remove(child);
        }

        public static double Distance(this Point p1)
        {
            return Distance(p1, new Point(0, 0));
        }

        public static double Distance(this Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public static (Point, Point) SortPoints(this Point p1, Point p2)
        {
            //  if (p1.Distance() < p2.Distance())
            return (p1, p2);
            //else
            //  return (p2, p1);
        }


        /// <summary>
        /// Determines if an element is visible to the user in relation to it's container (e.g. ScrollViewer)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        /// <summary>
        /// Creates a clone of input element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static UIElement CloneElement(UIElement element)
        {
            if (element == null)
                return null;

            if (element is Image oldImage)
            {
                var img = new Image() { Source = ((Image)element).Source.Clone() };
                img.Width = oldImage.Width;
                img.Height = oldImage.Height;

                // Remember to also apply coordinates for an image (and rendertransform also ...)
                img.SetValue(InkCanvas.LeftProperty, element.GetValue(InkCanvas.LeftProperty));
                img.SetValue(InkCanvas.TopProperty, element.GetValue(InkCanvas.TopProperty));
                img.RenderTransform = oldImage.RenderTransform.Clone();

                return img;
            }

            string s = XamlWriter.Save(element);
            StringReader stringReader = new StringReader(s);
            XmlReader xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());
            var copy = (UIElement)XamlReader.Load(xmlReader);

            if (copy is System.Windows.Controls.TextBox txt)
            {
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
            }

            return copy;
        }

        public static Point DeterminePointFromUIElement(UIElement element, DrawingCanvas can)
        {
            try
            {
                Visual visualcanvas = (Visual)can;
                Visual visualRec = (Visual)element;
                GeneralTransform gf = visualRec.TransformToVisual(visualcanvas);

                return gf.Transform(new Point(0, 0));
            }
            catch
            {
                return new Point();
            }
        }

        public static void BringToFront(this UIElement element, DrawingCanvas canvas)
        {
            try
            {
                int currentIndex = Canvas.GetZIndex(element);
                int maxZ = 0;

                for (int i = 0; i < canvas.Children.Count; i++)
                {
                    if (canvas.Children[i] is not null && canvas.Children[i] != element)
                        maxZ = Math.Max(maxZ, Canvas.GetZIndex(canvas.Children[i]));
                }
                Canvas.SetZIndex(element, Math.Min(++maxZ, int.MaxValue));
            }
            catch (Exception)
            { }
        }

        public static void BringToFront(this IEnumerable<UIElement> elements, DrawingCanvas canvas)
        {
            // Determine max index
            int zMax = 0;
            for (int i = 0; i < canvas.Children.Count; i++)
                zMax = Math.Max(zMax, Canvas.GetZIndex(canvas.Children[i]));

            int newZ = Math.Min(zMax + 1, int.MaxValue);
            foreach (var element in elements)
                Canvas.SetZIndex(element, newZ);
        }

        public static void AddJournalResourceToCanvas(JournalResource resource, DrawingCanvas ink)
        {
            ink.LoadChildren(resource.ConvertToUIElement());
        }

        /// <summary>
        /// Removes orignal renderTransform and replace it that there is just a RenderTransform and normal properties
        /// </summary>
        /// <param name="element"></param>
        /// <param name="can"></param>
        /// <returns></returns>
        public static Point ConvertTransformToProperties(UIElement element, DrawingCanvas can)
        {
            try
            {
                FrameworkElement fe = (FrameworkElement)element;

                if (fe.Parent == null)
                    return new Point();

                // Get rotation angle from rendertransform of the element
                var v = new Vector(0, 1);
                Vector rotated = Vector.Multiply(v, element.RenderTransform.Value);
                double angleBetween = Vector.AngleBetween(v, rotated);

                // Correct negative angles
                if (angleBetween < 0)
                    angleBetween = 360 - Math.Abs(angleBetween);

                // Get current position of elem in the InkCanvas
                Point point = DeterminePointFromUIElement(element, can);

                // Reset old transform to replace it with roation angle with origin (0,0)!
                if (element.RenderTransform is not RotateTransform)
                {
                    element.RenderTransform = new RotateTransform(angleBetween);
                    element.SetValue(InkCanvas.LeftProperty, point.X);
                    element.SetValue(InkCanvas.TopProperty, point.Y);
                    // IMPORTANT: ORIGIN AT (0,0)
                }
                else
                {
                    if (element.RenderTransform is RotateTransform rt)
                        rt.Angle = angleBetween;
                }

                return point;
            }
            catch (Exception e)
            {
                 MessageBox.Show($"{SharedResources.Resources.strFailedToTransformShape} {Environment.NewLine}{Environment.NewLine}{e.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Point();
        }

        private static double CalculeAngle(Point start, Point arrival)
        {
            var deltaX = Math.Pow((arrival.X - start.X), 2);
            var deltaY = Math.Pow((arrival.Y - start.Y), 2);

            var radian = Math.Atan2((arrival.Y - start.Y), (arrival.X - start.X));
            var angle = (radian * (180 / Math.PI) + 360) % 360;

            return angle;
        }

        /// <summary>
        /// Counts all edges of a polygon (
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static int CountEdges(this Polygon polygon)
        {
            /*
             * Concept: Use polygon.Points.Count as start value, then check for points which do not build another edge.
             * If a point has the same angle to it's previous point, it can be removed (count--)
             * 
             * - Start with a lastAngle of the last to the first point
             * - Due to inaccuracies add a correction factor of 1
             */

            int count = polygon.Points.Count;
            int edgeCounter = count;
            int lastAngle = (int)CalculeAngle(polygon.Points.LastOrDefault(), polygon.Points.FirstOrDefault());

            for (int i = 0; i < count; i++)
            {
                Point e1 = polygon.Points[i];
                Point e2 = polygon.Points[(i + 1) % count];

                int angle = (int)CalculeAngle(e1, e2);
                if (Math.Abs(angle - lastAngle) <= 1)
                    edgeCounter--;

                lastAngle = angle;
            }

            return edgeCounter;
        }

        /// <summary>
        /// Determines if an ellipse is a circle
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static bool IsCricle(this Ellipse el)
        {
            return (el != null && Math.Round(el.Width) == Math.Round(el.Height));
        }

        /// <summary>
        /// Get bounds from a child of a visual
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Rect BoundsRelativeTo(this FrameworkElement child, Visual parent)
        {
            try
            {
                GeneralTransform gt = child.TransformToAncestor(parent);
                return gt.TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
            }
            catch
            { }

            return new Rect();
        }

        /// <summary>
        /// Recursively finds the specified named parent in a control hierarchy
        /// </summary>
        /// <typeparam name="T">The type of the targeted Find</typeparam>
        /// <param name="child">The child control to start with</param>
        /// <param name="parentName">The name of the parent to find</param>
        /// <returns></returns>
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null)
                return null;

            T foundParent = null;
            var currentParent = VisualTreeHelper.GetParent(child);

            do
            {
                var frameworkElement = currentParent as FrameworkElement;
                if (frameworkElement is T)
                {
                    foundParent = (T)currentParent;
                    break;
                }

                currentParent = VisualTreeHelper.GetParent(currentParent);

            } while (currentParent != null);

            return foundParent;
        }
    }
}
