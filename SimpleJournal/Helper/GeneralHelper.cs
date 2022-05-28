using ControlzEx.Theming;
using Newtonsoft.Json;
using SimpleJournal.Controls;
using SimpleJournal.Data;
using SimpleJournal.Dialogs;
using SimpleJournal.Common;
using SimpleJournal.Documents.UI.Controls;
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
using SimpleJournal.Documents;
using SimpleJournal.Common.Helper;
using SimpleJournal.Documents.UI.Extensions;
using SimpleJournal.Documents.PDF;
using System.Threading.Tasks;
using System.Net.Http;
using SimpleJournal.Documents.UI;

namespace SimpleJournal
{
    public static class GeneralHelper
    {
        public static IEnumerable<T> Omit<T>(this IEnumerable<T> iterator, Type toOmit)
        {
            return iterator.Where(p => p.GetType() != toOmit);
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

        public static string GetCurrentTheme()
        {
            return $"{(Settings.Instance.UseDarkMode ? ThemeManager.BaseColorDarkConst : ThemeManager.BaseColorLightConst)}.{Settings.Instance.Theme}"; //.Colorful";
        }

        public static void ApplyTheming()
        {
            System.Windows.Media.Color sidebarColor;
            System.Windows.Media.Color linkColor;

            string transparency = Settings.Instance.UseObjectBarTransparency ? "AF" : "FF";

            if (Settings.Instance.UseDarkMode)
            {
                sidebarColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString($"#{transparency}252525");
                linkColor = System.Windows.Media.Colors.White;
            }
            else
            {
                sidebarColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString($"#{transparency}CECACA");
                linkColor = System.Windows.Media.Colors.MediumBlue;
            }

            // Apply own theming colors
            App.Current.Resources["Item.SidebarBackgroundColor"] = new SolidColorBrush(sidebarColor);
            App.Current.Resources["Link.Foreground"] = new SolidColorBrush(linkColor);

            ThemeManager.Current.ChangeTheme(Application.Current, GetCurrentTheme());
        }   

        #region RTB


        /// <summary>
        /// See https://stackoverflow.com/a/19534008/6237448
        /// </summary>
        /// <param name="element"></param>
        /// <param name="scale"></param>
        /// <param name="background"></param>
        /// <returns></returns>
        public static RenderTargetBitmap RenderToBitmap(UIElement element, double scale, Brush background)
        {
            var renderWidth = (int)(element.RenderSize.Width * scale);
            var renderHeight = (int)(element.RenderSize.Height * scale);

            var renderTarget = new RenderTargetBitmap(renderWidth, renderHeight, 96, 96, PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(element);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            var rect = new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

            using (drawingContext)
            {
                if (scale != 1.0)
                    drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(background, null, rect);
                drawingContext.DrawRectangle(sourceBrush, null, rect);
            }

            renderTarget.Render(drawingVisual);

            return renderTarget;
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

   
        #endregion

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

        /// <summary>
        /// https://stackoverflow.com/a/53284839/6237448
        /// </summary>
        public static async Task<PrintTicket> UploadFileAsync(string path, string url, string json)
        {
            var multiForm = new MultipartFormDataContent();
            multiForm.Headers.Add("options", json);

            // Add file and directly upload it
            System.IO.FileStream fs = System.IO.File.OpenRead(path);
            multiForm.Add(new StreamContent(fs), "file", System.IO.Path.GetFileName(path));

            // Send request to API 
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(url, multiForm);

                if (response.IsSuccessStatusCode)
                    return await System.Text.Json.JsonSerializer.DeserializeAsync<PrintTicket>(await response.Content.ReadAsStreamAsync());
                else
                {
                    string content = string.Empty;
                    try
                    {
                        content = await response.Content.ReadAsStringAsync();
                    }
                    catch
                    { }

                    if (!string.IsNullOrEmpty(content))
                        throw new Exception($"Http Status Code: {response.StatusCode}{Environment.NewLine}{Environment.NewLine}{content}");
                    else
                        throw new Exception($"Http Status Code: {response.StatusCode}");
                }
            }
        }

        public static bool IsConnectedToInternet()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var result = Task.Run(() => client.GetAsync(Consts.Google204Url)).Result;
                    if (result.IsSuccessStatusCode)
                        return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static void SearchForUpdates()
        {
            // Search for updates
            if (GeneralHelper.IsConnectedToInternet())
            {
                // If a new update is available
                // Display all changes from current version till new version (changelog is enough)

                // 1) Get current version
                var currentVersion = Consts.NormalVersion;

                // 2) Download version
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string versionJSON = Task.Run(() => client.GetStringAsync(Consts.VersionUrl)).Result;
                        dynamic versions = JsonConvert.DeserializeObject(versionJSON);

                        Version onlineVersion = Version.Parse(versions.current.normal.Value);

                        var result = onlineVersion.CompareTo(currentVersion);
                        if (result > 0)
                        {
                            // There is a new version
                            UpdateDialog ud = new UpdateDialog(onlineVersion);
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
            string directoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal");
            string execuatable = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SjFileAssoc.exe");            
           
            if (System.IO.File.Exists(execuatable))
            {
                try
                {
                    string fileVersion = FileVersionInfo.GetVersionInfo(execuatable).FileVersion;

                    if (fileVersion == "0.5.1.0")
                        return false;
                }
                catch
                {

                }
            }

            try
            {
                FileSystemHelper.ExtractZipFile(Properties.Resources.SJFileAssoc, directoryPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strFailedToExtractSJFileAssoc, ex.Message), SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return true;
        }

        public static bool InstallUWPFileAssoc()
        {
            GeneralHelper.InstallApplicationIconForFileAssociation();

            if (GeneralHelper.InstallFileAssoc())
            {
                var executableSJFileAssocFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SjFileAssoc.exe");
                if (System.IO.File.Exists(executableSJFileAssocFile))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(executableSJFileAssocFile);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format(SharedResources.Resources.strFailedToSetFileAssoc_Message, ex.Message), SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            return false;
        }
    }
}