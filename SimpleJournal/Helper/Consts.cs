﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SimpleJournal
{
    public static class Consts
    {
        public const int LAST_RECENTLY_OPENED_DOCUMENT_LIMIT = 10;
        public const double SPACE_BETWEEN_SITES = 75D;
        public const int TRAPEZE_OFFSET = 30;
        public const double MARKER_PATH_STROKE_THICKNESS = 0.4;
        public const double DEFAULT_TEXT_SIZE = 15.0;

        public static readonly DoubleCollection LINE_STROKE_DOTTET_DASH_ARRAY = new DoubleCollection() { 0.03, 2 };
        public static readonly DoubleCollection LINE_STROKE_DASHED_DASH_ARRAY = new DoubleCollection() { 4, 3 };
        public const int DEFAULT_LINE_STROKE_DASH_OFFSET = 1;

        public static readonly SolidColorBrush DEFAULT_BACKGROUND_BRUSH = new SolidColorBrush((System.Windows.Media.Color)(ColorConverter.ConvertFromString("#E1E1E1")));

        public static readonly string CHANGELOG_URL = "https://simplejournal.ca-soft.net/chg.php?lang={0}&dark={1}";
        public static readonly string DOWNLOAD_URL = "https://code-a-software.net/simplejournal/download.php";
        public static readonly string VERSION_URL = "https://code-a-software.net/simplejournal/versions.json";
        public static readonly string FEEDBACK_URL = "https://code-a-software.net/simplejournal/feedback.php?name={0}&mail={1}&content={2}";
        public static readonly string HOMEPAGE_URL = "https://simplejournal.ca-soft.net";

        public static readonly string UpdaterExe = "7244a3f048e82af354eb3cfa3089a3035ff8768f";
        public static readonly string UpdateSystemDotNetDotControllerDotdll = "bbc7224ccc544651d9d844f309721091860f0f92";

        public static readonly Version StoreVersion = new Version("1.431.0.0");
        public static readonly Version NormalVersion = typeof(Consts).Assembly.GetName().Version;

        #region Pens
        public static int AMOUNT_PENS = 4;

        public static Data.Color[] PEN_COLORS = new Data.Color[] {
            new Data.Color(0, 0, 0), // black
            new Data.Color(180, 30, 40), // red
            new Data.Color(30, 190, 20), // green
            new Data.Color(30, 40, 210), // blue
            new Data.Color(127, 127, 127)
        };

        #endregion

#if !UWP
        public static readonly string PenSettingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pen.xml");
        public static readonly string AutoSaveDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SimpleJournal", "AutoSave");
#else

        public static readonly string PenSettingsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "Pen.xml");
        public static readonly string AutoSaveDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "AutoSave");
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
        public const int SCROLLBAR_DEFAULT_WIDTH = 17;
        public const int SCROLLBAR_EXTENDED_WIDTH = 30;
#endregion

        #region Insert
        public const double INSERT_IMAGE_X = 200D;
        public const double INSERT_IMAGE_Y = 200D;
        public const double INSERT_IMAGE_WIDTH = 200D;
        public const double INSERT_IMAGE_HEIGHT = 200D;
        public const double INSERT_TEXT_WIDTH = 50D;
        public const double INSERT_TEXT_HEIGHT = 50D;
#endregion

        #region TextMarker
        public const double TEXT_MARKER_HEIGHT = 30.0;
        public const double TEXT_MARKER_WIDTH = 20.0;
        public static readonly Color TEXT_MARKER_COLOR = Colors.Yellow;
        #endregion

        #region Sidebar
        public const double SIDEBAR_LISTBOX_ITEM_HEIGHT = 50;
        public const double SIDEBAR_LISTBOX_ITEM_VIEWBOX_SIZE = 40;
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