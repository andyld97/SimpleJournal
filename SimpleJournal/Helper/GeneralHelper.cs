using ControlzEx.Theming;
using Newtonsoft.Json;
using SimpleJournal.Controls;
using SimpleJournal.Data;
using SimpleJournal.Dialogs;
using SimpleJournal.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace SimpleJournal
{
    public static class GeneralHelper
    {
        public static IEnumerable<T> Omit<T>(this IEnumerable<T> iterator, Type toOmit)
        {
            return iterator.Where(p => p.GetType() != toOmit);
        }

        public static StrokeCollection Reverse(this StrokeCollection sc)
        {
            if (sc == null)
                return null;

            StrokeCollection result = new StrokeCollection();
            for (int i = sc.Count - 1; i >= 0; i--)
                result.Add(sc[i]);

            return result;
        }

        public static void Move<T>(this List<T> lst, int from, int to)
        {
            if (from >= 0 && from < lst.Count && to >= 0 && to < lst.Count)
            {
                var element = lst[from];

                lst.RemoveAt(from);
                lst.Insert(to, element);
            }
            else
                throw new ArgumentOutOfRangeException("from or to must be in the range of length!");
        }

        public static Point ToPoint(this StylusPoint point)
        {
            return new Point(point.X, point.Y);
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

        public static System.Windows.Media.Color ConstrastColor(this System.Windows.Media.Color color)
        {
            // Counting the perceptive luminance - human eye favors green color... 
            var l = 0.2126 * color.ScR + 0.7152 * color.ScG + 0.0722 * color.ScB;
            return l < 0.5 ? Colors.White : Colors.Black;
        }

        public static string GetCurrentTheme()
        {
            return $"{(Settings.Instance.UseDarkMode ? ThemeManager.BaseColorDarkConst : ThemeManager.BaseColorLightConst)}.{Settings.Instance.Theme}"; //.Colorful";
        }

        public static void ApplyTheming()
        {
            System.Windows.Media.Color sidebarColor;
            System.Windows.Media.Color linkColor;

            if (Settings.Instance.UseDarkMode)
            {
                sidebarColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF252525");
                linkColor = System.Windows.Media.Colors.White;
            }
            else
            {
                sidebarColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#AFCECACA");
                linkColor = System.Windows.Media.Colors.MediumBlue;
            }

            // Apply own theming colors
            App.Current.Resources["Item.SidebarBackgroundColor"] = new SolidColorBrush(sidebarColor);
            App.Current.Resources["Link.Foreground"] = new SolidColorBrush(linkColor);

            ThemeManager.Current.ChangeTheme(Application.Current, GetCurrentTheme());
        }

        public static System.Windows.Media.Color ToColor(this Data.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
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

        public static byte[] ExportImage(BitmapSource bi)
        {
            try
            {
                MemoryStream memStream = new MemoryStream();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bi));
                encoder.Save(memStream);
                return memStream.ToArray();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToSaveImage} {e.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
                return new byte[] { };
            }
        }

        public static BitmapImage ImageFromBase64(string data)
        {
            return LoadImage(Convert.FromBase64String(data));
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            try
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToLoadImage} {e.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static JournalResource ConvertImage(this Image img)
        {
            JournalImage ji = new JournalImage();
            ji.SetData(GeneralHelper.ExportImage((BitmapSource)img.Source));
            ji.Left = (double)img.GetValue(InkCanvas.LeftProperty);
            ji.Top = (double)img.GetValue(InkCanvas.TopProperty);
            ji.ZIndex = Canvas.GetZIndex(img);
            ji.Width = img.Width;
            ji.Height = img.Height;
            ji.IsUniform = (img.Stretch == Stretch.Uniform);

            if (img.RenderTransform != null && img.RenderTransform is RotateTransform rt)
                ji.RotationAngle = (int)rt.Angle;

            return ji;
        }

        public static JournalResource ConvertShape(this Shape shape)
        {
            JournalShape js = new JournalShape();

            string shapeData = XamlWriter.Save(shape);
            js.SetData(Encoding.Default.GetBytes(Convert.ToBase64String(Encoding.Default.GetBytes(shapeData))));

            return js;
        }

        public static JournalResource ConvertPlot(this Plot plot)
        {
            JournalPlot js = new JournalPlot
            {
                Left = (double)plot.GetValue(DrawingCanvas.LeftProperty),
                Top = (double)plot.GetValue(DrawingCanvas.TopProperty),
                PlotDirection = plot.DrawingDirection,
                PlotMode = plot.DrawingMode,
                StrokeThickness = plot.StrokeThickness,
                ForegroundA = plot.Foreground.A,
                ForegroundB = plot.Foreground.B,
                ForegroundG = plot.Foreground.G,
                ForegroundR = plot.Foreground.R,
                Width = plot.Width,
                Height = plot.Height,
                ZIndex = Canvas.GetZIndex(plot)
            };

            if (plot.RenderTransform != null && plot.RenderTransform is RotateTransform rt)
                js.RotationAngle = (int)rt.Angle;

            return js;
        }

        /// <summary>
        /// Opens the default system browser with the requested uri
        /// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp/67838115#67838115
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool OpenUri(Uri uri)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"\"{uri}\"");
                return true;
            }
            catch (Exception)
            {
                // ignore 
                return false;
            }
        }

        public static JournalResource ConvertText(this TextBlock text)
        {
            JournalText jt = new JournalText();
            jt.SetData(Encoding.Default.GetBytes(text.Text));
            jt.Left = (double)text.GetValue(InkCanvas.LeftProperty);
            jt.Top = (double)text.GetValue(InkCanvas.TopProperty);
            jt.ZIndex = Canvas.GetZIndex(text);
            jt.Width = text.Width;
            jt.Height = text.Height;

            // Set speical button
            jt.FontSize = text.FontSize;
            jt.FontFamily = text.FontFamily.ToString();
            var foregroundBrush = (text.Foreground as SolidColorBrush);

            jt.A = foregroundBrush.Color.A;
            jt.R = foregroundBrush.Color.R;
            jt.G = foregroundBrush.Color.G;
            jt.B = foregroundBrush.Color.B;

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

            if (containsAll && text.TextDecorations.Count() != 0)
                jt.IsStrikeout = true;

            if (text.RenderTransform != null && text.RenderTransform is RotateTransform rt)
                jt.RotationAngle = (int)rt.Angle;

            return jt;
        }

        /// <summary>
        /// Create a screenshof of UI element
        /// </summary>
        /// <param name="element">The element to copy.</param>
        public static RenderTargetBitmap CreateScreenshotOfElement(FrameworkElement element)
        {
            double width = double.IsNaN(element.Width) ? element.ActualWidth : element.Width;
            double height = double.IsNaN(element.Height) ? element.ActualHeight : element.Height;

            RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(element);
                dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            }
            bmpCopied.Render(dv);
            return bmpCopied;
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
                var img = new Image() { Source = (element as Image).Source.Clone() };
                img.RenderTransform = new RotateTransform(0);
                img.Width = oldImage.Width;
                img.Height = oldImage.Height;
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
                    if (canvas.Children[i] is UIElement && canvas.Children[i] != element)
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

                // Get current position of elem in the InkCanvas
                Point point = DeterminePointFromUIElement(element, can);

                // Reset old transform to replace it with roation angle with origin (0,0)!
                if (!(element.RenderTransform is RotateTransform))
                {
                    element.RenderTransform = null;
                    element.SetValue(InkCanvas.LeftProperty, point.X);
                    element.SetValue(InkCanvas.TopProperty, point.Y);
                    element.RenderTransform = new RotateTransform(angleBetween);
                    // IMPORTANT: ORIGIN AT (0,0)
                }

                return point;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToTransformShape} {Environment.NewLine}{Environment.NewLine}{e.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Point();
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

        public static void RemoveUpdaterIfAny()
        {
            string pathUpdaterExe = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");
            string pathUpdateSystemDotNetDotControllerDotdll = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "updateSystemDotNet.Controller.dll");

            if (System.IO.File.Exists(pathUpdaterExe) && FileSystem.BuildSHA1FromFile(pathUpdaterExe) == Consts.UpdaterExe)
                FileSystem.TryDeleteFile(pathUpdaterExe);

            if (System.IO.File.Exists(pathUpdateSystemDotNetDotControllerDotdll) && FileSystem.BuildSHA1FromFile(pathUpdateSystemDotNetDotControllerDotdll) == Consts.UpdateSystemDotNetDotControllerDotdll)
                FileSystem.TryDeleteFile(pathUpdateSystemDotNetDotControllerDotdll);
        }

        public static bool IsConnectedToInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SearchForUpdates()
        {
            // Search for updates
            if (GeneralHelper.IsConnectedToInternet())
            {
                // If a new update is available
                // Display all changes from current version till new version (changelog is enough)

                // 1) Get current version
                var version = Consts.NormalVersion;

                // 2) Download version
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        string versionJSON = wc.DownloadString(Consts.VersionUrl);
                        dynamic versions = JsonConvert.DeserializeObject(versionJSON);

                        Version current = Version.Parse(versions.current.normal.Value);

                        var result = current.CompareTo(version);
                        if (result > 0)
                        {
                            // There is a new version
                            UpdateDialog ud = new UpdateDialog(current);
                            ud.ShowDialog();
                        }
                        else if (result < 0)
                        {
                            // Online version is older than this version (impossible case)
                        }
                        else
                        {
                            // equal
                        }
                    }
                }
                catch (Exception)
                {
                    // ignore failed to get updates
                }
            }
        }

#if UWP 
        public static readonly string FileAssociationIconsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "icon.ico");
#endif

        public static bool InstallApplicationIconForFileAssociation()
        {
#if !UWP
            return false;
#endif

#if UWP
            if (System.IO.File.Exists(FileAssociationIconsPath))
                return false;

            try
            {
                string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal");
                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);

                using (FileStream fs = new FileStream(FileAssociationIconsPath, FileMode.Create))
                {
                    Properties.Resources.journalicon.Save(fs);
                }

                return true;
            }
            catch (Exception)
            {
                // Silence is golden
                return false;
            }
#endif
        }

        public static bool InstallFileAssoc()
        {
#if !UWP
            return false;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            Dictionary<string, byte[]> resourcesToDeploy = new Dictionary<string, byte[]>()
#pragma warning restore CS0162 // Unreachable code detected
            {
                {  System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SjFileAssoc.exe"),   Properties.Resources.SJFileAssoc },
                {  System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SJFileAssoc.exe.config"),  System.Text.Encoding.Default.GetBytes(Properties.Resources.SJFileAssoc_exe) },
                {  System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SimpleJournal.Shared.dll"),   Properties.Resources.SimpleJournal_Shared },
            };

            // If the file exists and if it's up to date no need to install
            if (System.IO.File.Exists(resourcesToDeploy.Keys.FirstOrDefault()) && FileVersionInfo.GetVersionInfo(resourcesToDeploy.Keys.First()).FileVersion == "1.0.1.0")
                return false;
            else if (System.IO.File.Exists(resourcesToDeploy.Keys.FirstOrDefault()))
                FileSystem.TryDeleteFile(resourcesToDeploy.Keys.FirstOrDefault());

            bool result = true;
            foreach (var file in resourcesToDeploy)
                result &= FileSystem.TryWriteAllBytes(file.Key, file.Value);

            return result;
        }

        public static bool InstallUWPFileAssoc()
        {
            GeneralHelper.InstallApplicationIconForFileAssociation();
            if (GeneralHelper.InstallFileAssoc())
            {
                var tempFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SjFileAssoc.exe");
                if (System.IO.File.Exists(tempFile))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(tempFile);
                        return true;
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return false;
        }
    }
}