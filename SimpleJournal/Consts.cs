using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SimpleJournal
{
    public static class Consts
    {
        public const int LastRecentlyOpenedDocuments = 10;
        public const double SpaceBetweenPages = 75D;
        public const int TrapezeOffset = 30;
        public const double MarkerPathStrokeThickness = 0.4;
        public const double DefaultTextSize = 15.0;

        public const int A4WidthP = 794;
        public const int A4HeightP = 1123;
        public const int A4WidthL = 1123;
        public const int A4HeightL = 794;

        public static readonly DoubleCollection LineStrokeDottetDashArray = new DoubleCollection() { 0.03, 2 };
        public static readonly DoubleCollection LineStrokeDashedDashArray = new DoubleCollection() { 4, 3 };
        public const int DefaultLineStrokeDashOffset = 1;

        public const int MaxPDFPagesPerJournal = 100;

        public static readonly SolidColorBrush DefaultBackground = new SolidColorBrush((System.Windows.Media.Color)(ColorConverter.ConvertFromString("#E1E1E1")));

        public static readonly string ChangelogUrl = "https://simplejournal.ca-soft.net/chg.php?lang={0}&dark={1}";
        public static readonly string DonwloadUrl = "https://simplejournal.ca-soft.net/download.php";
        public static readonly string VersionUrl = "https://simplejournal.ca-soft.net/versions.json";
        public static readonly string FeedbackUrl = "https://simplejournal.ca-soft.net/feedback.php?name={0}&mail={1}&content={2}";
        public static readonly string HomePageUrl = "https://simplejournal.ca-soft.net";
        public static readonly string HelpUrl = "https://simplejournal.ca-soft.net/faq";
        public static readonly string GhostScriptDownloadUrl = "https://ghostscript.com/releases/gsdnld.html";
        public static readonly string ConverterAPIUrl = "http://cas-server2.ddns.net:8080";

        public static readonly string UpdaterExe = "7244a3f048e82af354eb3cfa3089a3035ff8768f";
        public static readonly string UpdateSystemDotNetDotControllerDotdll = "bbc7224ccc544651d9d844f309721091860f0f92";

        public static readonly Version StoreVersion = new Version("1.501.0.0");
        public static readonly Version NormalVersion = typeof(Consts).Assembly.GetName().Version;

        #region Pens
        public const int AMOUNT_PENS = 4;

        public static Common.Data.Color[] PEN_COLORS = new Common.Data.Color[] 
        {
            new Common.Data.Color(0, 0, 0), // black
            new Common.Data.Color(180, 30, 40), // red
            new Common.Data.Color(30, 190, 20), // green
            new Common.Data.Color(30, 40, 210), // blue
            new Common.Data.Color(127, 127, 127)
        };

        #endregion


        public static readonly string WebView2CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "WebView2");

#if !UWP
        public static readonly string PenSettingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pen.xml");

        /// <summary>
        /// Used for old xml journals
        /// </summary>
        public static readonly string BackupDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleJournal", "Backup");
        public static readonly string AutoSaveDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleJournal", "AutoSave");
        public static readonly string TouchExecutable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Touch.exe");
        public static readonly string Executable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleJournal.exe");

        public static readonly string SetTouchOn = "/on";
        public static readonly string SetTouchOff = "/off";
#else
        public static readonly string PenSettingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "Pen.xml");
        public static readonly string AutoSaveDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "AutoSave");
        /// <summary>
        /// Used for old xml journals
        /// </summary>
        public static readonly string BackupDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "Backup");
#endif


#if UWP
        static Consts()
        {
            try
            {
                var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pen.xml");
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
            catch
            {
                // ignore
            }
        }
#endif

        #region Scrollbar
        public const int ScrollBarDefaultWidth = 17;
        public const int ScrollBarExtendedWidth = 30;
        #endregion

        #region Insert
        public const double InsertImagePositionX = 200D;
        public const double InsertImagePositionY = 200D;
        public const double InsertImageWidth = 200D;
        public const double InsertImageHeight = 200D;
        public const double InsertTextWidth = 150D;
        public const double InsertTextHeight = 50D;
        #endregion

        #region TextMarker
        public const double TextMarkerHeight = 30.0;
        public const double TextMarkerWidth = 20.0;
        public static readonly Color TextMarkerColor = Colors.Yellow;
        #endregion

        #region Sidebar
        public const double SidebarListBoxItemHeight = 50;
        public const double SidebarListBoxItemViewboxSize = 40;
        #endregion

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