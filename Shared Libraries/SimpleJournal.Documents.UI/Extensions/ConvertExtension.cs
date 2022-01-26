using SimpleJournal.Common.Controls;
using SimpleJournal.Documents.Resources;
using SimpleJournal.Documents.UI.Helper;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SimpleJournal.Documents.UI.Extensions
{
    public static class ConvertExtension
    {
        /// <summary>
        /// Takes UIElement, e.g. the children from InkCanvas and converts them to journalResources
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static JournalResource ConvertFromUIElement(this UIElement element)
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

        public static UIElement ConvertToUIElement(this JournalResource journalResource)
        {
            if (journalResource.JournalResourceType == Common.JournalResourceType.Image)
                return ConvertImageToUiElement(journalResource as JournalImage);
            else if (journalResource.JournalResourceType == Common.JournalResourceType.Text)
                return ConvertTextToUiElement(journalResource as JournalText);
            else if (journalResource.JournalResourceType == Common.JournalResourceType.Shape)
                return ConvertShapeToUiElement(journalResource as JournalShape);
            else if (journalResource.JournalResourceType == Common.JournalResourceType.Plot)
                return ConvertPlotToUiElement(journalResource as JournalPlot);

            return null;
        }

        #region Image
        public static UIElement ConvertImageToUiElement(this JournalImage image)
        {
            Image img = new Image() { Source = ImageHelper.LoadImageFromBase64(Convert.ToBase64String(image.Data)), Stretch = (image.IsUniform ? Stretch.Uniform : Stretch.Fill) };
            img.SetValue(InkCanvas.LeftProperty, image.Left);
            img.SetValue(InkCanvas.TopProperty, image.Top);
            img.Width = image.Width;
            img.Height = image.Height;
            img.RenderTransform = new RotateTransform(image.RotationAngle);
            img.RenderTransformOrigin = new Point(0.5, 0.5);
            Canvas.SetZIndex(img, image.ZIndex);

            return img;
        }

        public static JournalResource ConvertImage(this Image img)
        {
            JournalImage ji = new JournalImage
            {
                Data = ImageHelper.ExportImage((BitmapSource)img.Source),
                Left = (double)img.GetValue(InkCanvas.LeftProperty),
                Top = (double)img.GetValue(InkCanvas.TopProperty),
                ZIndex = Canvas.GetZIndex(img),
                Width = img.Width,
                Height = img.Height,
                IsUniform = (img.Stretch == Stretch.Uniform)
            };

            if (img.RenderTransform != null && img.RenderTransform is RotateTransform rt)
                ji.RotationAngle = (int)rt.Angle;

            return ji;
        }
        #endregion

        #region Shape
        public static UIElement ConvertShapeToUiElement(JournalShape journalShape)
        {
            try
            {
                Shape shape = null;

                if (journalShape.Data != null && journalShape.Data.Length > 0)
                {
                    // Old base64 logic
                    shape = (Shape)XamlReader.Parse(Encoding.Default.GetString(Convert.FromBase64String(Encoding.Default.GetString(journalShape.Data))));
                }
                else
                {
                    // New shape CDATA logic
                    shape = (Shape)XamlReader.Parse(journalShape.ShapeXML);
                }

                if (shape != null)
                {
                    Canvas.SetZIndex(shape, journalShape.ZIndex);
                    return shape;
                }
            }
            catch (Exception)
            {
                // Silence is golden
                return new UIElement();
            }

            return new UIElement();
        }

        public static JournalResource ConvertShape(this Shape shape)
        {
            JournalShape js = new JournalShape();

            string shapeData = XamlWriter.Save(shape);
            // js.Data = Encoding.Default.GetBytes(Convert.ToBase64String(Encoding.Default.GetBytes(shapeData)));
            js.Data = Array.Empty<byte>();
            js.ShapeXML = shapeData;
            js.ZIndex = Canvas.GetZIndex(shape);


            return js;
        }
        #endregion

        #region Text
        public static UIElement ConvertTextToUiElement(this JournalText journalText)
        {
            TextBlock text = new TextBlock() { Text = Encoding.Default.GetString(journalText.Data) };
            text.TextWrapping = TextWrapping.Wrap;
            text.SetValue(InkCanvas.LeftProperty, journalText.Left);
            text.SetValue(InkCanvas.TopProperty, journalText.Top);
            text.Width = journalText.Width;
            text.Height = journalText.Height;
            text.FontSize = journalText.FontSize;
            text.FontFamily = new System.Windows.Media.FontFamily(journalText.FontFamily);
            text.Foreground = new SolidColorBrush(journalText.TextColor.ToColor());

            if (journalText.IsBold)
                text.FontWeight = FontWeights.Bold;

            if (journalText.IsItalic)
                text.FontStyle = FontStyles.Italic;

            if (journalText.IsUnderlined)
                text.TextDecorations.Add(TextDecorations.Underline);

            if (journalText.IsStrikeout)
                text.TextDecorations.Add(TextDecorations.Strikethrough);

            text.RenderTransform = new RotateTransform(journalText.RotationAngle);
            text.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetZIndex(text, journalText.ZIndex);

            return text;
        }

        public static JournalResource ConvertText(this TextBlock text)
        {
            JournalText jt = new JournalText
            {
                Data = Encoding.Default.GetBytes(text.Text),
                Left = (double)text.GetValue(InkCanvas.LeftProperty),
                Top = (double)text.GetValue(InkCanvas.TopProperty),
                ZIndex = Canvas.GetZIndex(text),
                Width = text.Width,
                Height = text.Height,

                // Set speical button
                FontSize = text.FontSize,
                FontFamily = text.FontFamily.ToString()
            };

            var foregroundBrush = (text.Foreground as SolidColorBrush);
            jt.TextColor = foregroundBrush.Color.ToColor();
            jt.IsBold = (text.FontWeight == FontWeights.Bold);
            jt.IsItalic = (text.FontStyle == FontStyles.Italic);

            bool containsAll = true;
            foreach (var deco in TextDecorations.Underline)
            {
                if (!text.TextDecorations.Contains(deco))
                {
                    containsAll = false;
                    break;
                }
            }

            if (containsAll && text.TextDecorations.Count != 0)
                jt.IsUnderlined = true;

            containsAll = true;
            foreach (var deco in TextDecorations.Strikethrough)
            {
                if (!text.TextDecorations.Contains(deco))
                {
                    containsAll = false;
                    break;
                }
            }

            if (containsAll && text.TextDecorations.Count != 0)
                jt.IsStrikeout = true;

            if (text.RenderTransform != null && text.RenderTransform is RotateTransform rt)
                jt.RotationAngle = (int)rt.Angle;

            return jt;
        }
        #endregion

        #region Plot
        public static UIElement ConvertPlotToUiElement(this JournalPlot journalPlot)
        {
            var plt = new Plot()
            {
                DrawingDirection = journalPlot.PlotDirection,
                DrawingMode = journalPlot.PlotMode,
            };

            plt.SetValue(InkCanvas.LeftProperty, journalPlot.Left);
            plt.SetValue(InkCanvas.TopProperty, journalPlot.Top);
            plt.Width = journalPlot.Width;
            plt.Height = journalPlot.Height;
            plt.StrokeThickness = journalPlot.StrokeThickness;
            plt.Foreground = journalPlot.StrokeColor.ToColor();
            Canvas.SetZIndex(plt, journalPlot.ZIndex);

            plt.RenderTransform = new RotateTransform(journalPlot.RotationAngle);
            plt.RenderTransformOrigin = new Point(0.5, 0.5);

            return plt;
        }     

        public static JournalResource ConvertPlot(this Plot plot)
        {
            JournalPlot js = new JournalPlot
            {
                Left = (double)plot.GetValue(InkCanvas.LeftProperty),
                Top = (double)plot.GetValue(InkCanvas.TopProperty),
                PlotDirection = plot.DrawingDirection,
                PlotMode = plot.DrawingMode,
                StrokeThickness = plot.StrokeThickness,
                StrokeColor = plot.Foreground.ToColor(),
                Width = plot.Width,
                Height = plot.Height,
                ZIndex = Canvas.GetZIndex(plot)
            };

            if (plot.RenderTransform != null && plot.RenderTransform is RotateTransform rt)
                js.RotationAngle = (int)rt.Angle;

            return js;
        }

        #endregion
    }
}