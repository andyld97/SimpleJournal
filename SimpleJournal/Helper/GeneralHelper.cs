using ControlzEx.Theming;
using Newtonsoft.Json;
using SimpleJournal.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SimpleJournal.Common.Helper;
using SimpleJournal.Documents.PDF;
using System.Threading.Tasks;
using System.Net.Http;
using SimpleJournal.Documents.UI;
using Newtonsoft.Json.Linq;
using Notifications;

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

        #region Theming

        public static string GetCurrentTheme()
        {
            return $"{(Settings.Instance.UseDarkMode ? ThemeManager.BaseColorDarkConst : ThemeManager.BaseColorLightConst)}.{Settings.Instance.Theme}"; //.Colorful";
        }

        public static void ApplyTheming()
        {
            Color 
                sidebarColor, linkColor, tabControlBackgroundColor, tabItemBackground, tabItemSelectedBackground;

            static Color ToColor(string hex) => (Color)ColorConverter.ConvertFromString(hex);
            static Brush ToBrush(string hex) => new SolidColorBrush(ToColor(hex));

            string transparency = Settings.Instance.UseObjectBarTransparency ? "AF" : "FF";

            if (Settings.Instance.UseDarkMode)
            {
                sidebarColor = ToColor($"#{transparency}252525");
                linkColor = Colors.White;
                tabControlBackgroundColor = Colors.Black;
                
                tabItemBackground = ToColor("#AF282828");
                tabItemSelectedBackground = ToColor("#282828");

                // Scrollbar / Dark
                App.Current.Resources["ScrollBarButtonBackgroundBrush"] = ToBrush("#FF2B2B2B");
                App.Current.Resources["ScrollbarThumb"] = ToBrush("#FF383838");
                App.Current.Resources["ScrollBarButtonHighlightBackgroundBrush"] = ToBrush("#FF3C3C3C");
                App.Current.Resources["ScrollBarButtonArrowForegroundBrush"] = ToBrush("#FFD0D0D0");
                App.Current.Resources["ScrollBarTrackBrush"] = ToBrush("#FF1C1C1C");
            }
            else
            {
                sidebarColor = ToColor($"#{transparency}CECACA");
                linkColor = Colors.MediumBlue;
                tabControlBackgroundColor = ToColor($"#DADADA");

                tabItemBackground = ToColor("#D7D7D7");
                tabItemSelectedBackground = ToColor("#F9F9F9");

                // Scrollbar / Light
                App.Current.Resources["ScrollBarButtonBackgroundBrush"] = ToBrush("#FFE6E6E6");
                App.Current.Resources["ScrollbarThumb"] = ToBrush("#FFB5B5B5");
                App.Current.Resources["ScrollBarButtonHighlightBackgroundBrush"] = ToBrush("#FFDADADA");
                App.Current.Resources["ScrollBarButtonArrowForegroundBrush"] = ToBrush("#FF4A4A4A");
                App.Current.Resources["ScrollBarTrackBrush"] = ToBrush("#FFF5F5F5");
            }

            // Apply own theming colors
            App.Current.Resources["Item.SidebarBackgroundColor"] = new SolidColorBrush(sidebarColor);
            App.Current.Resources["Link.Foreground"] = new SolidColorBrush(linkColor);
            App.Current.Resources["TabControl.Background"] = new SolidColorBrush(tabControlBackgroundColor);
            App.Current.Resources["TabItemBackground"] = new SolidColorBrush(tabItemBackground);
            App.Current.Resources["TabItemSelectedBackground"] = new SolidColorBrush(tabItemSelectedBackground);

            var theme = ThemeManager.Current.GetTheme(GetCurrentTheme()); // ThemeManager.Current.DetectTheme(Application.Current);
            if (theme != null)
            {
                if (Settings.Instance.UseDarkMode)
                {
                    var fixedBlack = ToColor("#FF252525");
                    theme.Resources["Fluent.Ribbon.Colors.White"] = fixedBlack;
                    theme.Resources["Fluent.Ribbon.Brushes.White"] = new SolidColorBrush(fixedBlack);                
                }

                ThemeManager.Current.ChangeTheme(Application.Current, theme);
                return;
            }

            ThemeManager.Current.ChangeTheme(Application.Current, GetCurrentTheme());
        }

        #endregion

        #region .NET Auto detect/Autoupdate
        /// <summary>
        /// Determines the win-x86 or win-x64 platform
        /// </summary>
        /// <returns>win-x86 or winx64</returns>
        public static string DeterminePlatform()
        {
            string platform = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            return $"win-{platform}";
        }

        /// <summary>
        /// Determines the dotnet download link for <see cref="Consts.CompiledDotnetVersion"/>
        /// </summary>
        /// <returns></returns>
        public static async Task<string> DetermineDotnetDesktpRuntimeDownloadLink()
        {
            // Determine link
            using (HttpClient client = new HttpClient())
            {
                string releasesJson = await client.GetStringAsync(Consts.DotnetReleaseInfoUrl);

                var root = JsonConvert.DeserializeObject<JObject>(releasesJson);
                var releases = root["releases"].Value<JArray>();

                var release = releases.FirstOrDefault(p => p.Value<string>("release-version") == Consts.CompiledDotnetVersion.ToString(3));
                if (release != null)
                {
                    var files = release.Value<JObject>("windowsdesktop").Value<JArray>("files");
                    var file = files.FirstOrDefault(f => f.Value<string>("rid") == DeterminePlatform());

                    return file.Value<string>("url");
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Downloads the <see cref="Consts.CompiledDotnetVersion"/>-Version, starts the setup and then quits SimpleJournal (a manual restart is required)
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateNETCoreVersionAsync()
        {
            if (MessageBox.Show(Properties.Resources.strDotnetUpdateSetup_PrepareMessage, SharedResources.Resources.strAttention, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            string url;

            try
            {
                url = await GeneralHelper.DetermineDotnetDesktpRuntimeDownloadLink();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strDotnetUpdateSetup_FailedToDetermineDownloadUrl, System.Environment.Version.ToString(3), ex.Message), SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string platform = GeneralHelper.DeterminePlatform();
            string fileName = $"windowsdesktop-runtime-{Consts.CompiledDotnetVersion}-{platform}.exe";
            string localFilePath = System.IO.Path.Combine(FileSystemHelper.GetDownloadsPath(), fileName);
            var dialog = new UpdateDownloadDialog(string.Format(Properties.Resources.strDotnetUpdateSetup_Downloading, fileName), url, string.Empty) { LocalFilePath = localFilePath };
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                try
                {
                    System.Diagnostics.Process.Start(localFilePath);

                    // Exit to make sure user can easily update without problems
                    Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(Properties.Resources.strDotnetUpdate_FailedToOpenSetup, localFilePath, ex.Message), SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

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
        /// Create a screenshot of an UI element
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
        /// Shutdowns the app and stops all services! This should always be called when the app should exit.
        /// </summary>
        public static void Shutdown()
        {
            NotificationService.NotificationServiceInstance?.Stop();
            Application.Current.Shutdown();
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

        /// <summary>
        /// https://stackoverflow.com/a/53284839/6237448
        /// </summary>
        public static async Task<PrintTicket> UploadFileAsync(string path, string url, string json)
        {
            var multiForm = new MultipartFormDataContent();
            multiForm.Headers.Add("options", json);
            multiForm.Headers.Add("language", Properties.Resources.strLang);

            // Add file and directly upload it
            System.IO.FileStream fs = System.IO.File.OpenRead(path);
            multiForm.Add(new StreamContent(fs), "file", System.IO.Path.GetFileName(path));

            // Send request to API 
            using HttpClient client = new HttpClient();
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
            string executable = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SjFileAssoc.exe");            
           
            if (System.IO.File.Exists(executable))
            {
                try
                {
                    string fileVersion = FileVersionInfo.GetVersionInfo(executable).FileVersion;

                    if (fileVersion == "0.6.4.0")
                        return true;
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
            // TODO: *** This should be re-worked sometimes, it should:
            // Parse the version and compare >= instead of string comparision for a specific version
            // And it shouldn't be done on every start, thats only really necessary if you have both SJ Version (Store|Normal) installed and if you want
            // to ensure that the file association is always set to the store version.

            // Maybe there is even a better way to create the file association on UWP without starting another application
            // or without admin rights!

            InstallApplicationIconForFileAssociation();

            if (InstallFileAssoc())
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