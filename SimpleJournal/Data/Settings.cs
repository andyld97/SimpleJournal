using System;
using System.Windows;
using System.Windows.Media;

namespace SimpleJournal.Data
{
    public class Settings
    {
#if !UWP
        private static readonly string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Settings.xml");
#else
        private static readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "Settings.xml");        
#endif

        static Settings()
        {
#if UWP 
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
#endif
        }


        public static readonly Settings Instance = Settings.Load();

        /// <summary>
        /// Determine the type of the paper.
        /// </summary>
        public PaperType PaperType { get; set; } = PaperType.Chequeued;

        /// <summary>
        /// Determine the type of the paper which was used while inserting a page
        /// </summary>
        public PaperType PaperTypeLastInserted { get; set; } = PaperType.Chequeued;

        /// <summary>
        /// Determines the format of the paper. Currently only A4 is supported
        /// </summary>
        public Format PaperFormat { get; set; } = Format.A4;

        /// <summary>
        /// Determines in which state SimpleJournal schould start
        /// </summary>
        public WindowState WindowState { get; set; } = WindowState.Minimized;

        /// <summary>
        /// Determines if the scrollbar will be bigger
        /// </summary>
        public bool EnlargeScrollbar { get; set; } = false;

        /// <summary>
        /// Determines if the sidebar will be displayed automatically
        /// </summary>
        public bool DisplaySidebarAutomatically { get; set; } = true;

        /// <summary>
        /// Determines if the pressure from the stylus is used
        /// </summary>
        public bool UsePreasure { get; set; } = true;
    
        /// <summary>
        /// Determines whether the app not has been started yet
        /// </summary>
        public bool FirstStart { get; set; } = false;

        /// <summary>
        /// Determines how much percent the canvas are scaled. 
        /// </summary>
        public int Zoom { get; set; } = 100;

        /// <summary>
        /// Size of the text marker
        /// </summary>
        public Size TextMarkerSize { get; set; } = Consts.TextMarkerSizes[0];

        /// <summary>
        /// The color of the text marker 
        /// </summary>
        public Color TextMarkerColor { get; set; } = new Color(Colors.Yellow.R, Colors.Yellow.G, Colors.Yellow.B);

        /// <summary>
        /// If true then all forms which would be converted will have a rotation of 0 degrees.
        /// </summary>
        public bool UseRotateCorrection { get; set; } = true;

        /// <summary>
        /// If true makes every ellipse to circle
        /// </summary>
        public bool UseCircleCorrection { get; set; } = true;

        /// <summary>
        /// How the stroke of the ruler looks like
        /// </summary>
        public RulerMode RulerStrokeMode { get; set; } = RulerMode.Normal;

        /// <summary>
        /// If this settings is true, the old chequered pattern must be used!
        /// </summary>
        public bool UseOldChequeredPattern { get; set; } = false;

        /// <summary>
        /// If true, every AutoSaveIntervalMS of time a backup will be created
        /// </summary>
        public bool UseAutoSave { get; set; } = true;

        /// <summary>
        /// If true "FitToCurve" will be activated in the drawing attributes
        /// </summary>
        public bool UseFitToCurve { get; set; } = false;
        
        /// <summary>
        /// The interval (in minutes) which will be used for AutoSave - default value is every 5 minutes
        /// </summary>
        public int AutoSaveIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// The first setting for the setup dialog!
        /// </summary>
        public bool UserHasSelectedHDScreen { get; set; } = true;

        /// <summary>
        /// Determines if the darkmode is active or not
        /// </summary>
        public bool UseDarkMode { get; set; } = false;

        /// <summary>
        /// The theme which is used in SimpleJournal (hard to describe, because Theme is enogugh)
        /// </summary>
        public string Theme { get; set; } = "Cobalt"; // cobalt is the default one used in SJ

        /// <summary>
        /// Determines if the message (inserting an ui element) will be ignored or not!
        /// </summary>
        public bool DoesNotShowInsertHint { get; set; } = false;

        /// <summary>
        /// Determines if the glowing brush of the mainwindow is disabled or not
        /// </summary>
        public bool ActivateGlowingBrush { get; set; } = false;

        /// <summary>
        /// Determines if the touch screen will be disabled at startup and enabled at closing
        /// </summary>
        public bool UseTouchScreenDisabling { get; set; } = false;

        /// <summary>
        /// The background of all pages
        /// </summary>
        public Background PageBackground { get; set; } = Background.Default;

        /// <summary>
        /// If PageBackground is set to custom, this will the path for a custom image
        /// </summary>
        public string CustomBackgroundImagePath { get; set; } = string.Empty;

        /// <summary>
        /// If true, the scroll direction is inverted
        /// </summary>
        public bool UseNaturalScrolling { get; set; } = false;

        /// <summary>
        /// Determines if the checkbox in the ExportDialog is checked or not 
        /// </summary>
        public bool ExportAsJournal { get; set; } = false;

        public enum RulerMode : int
        {
            /// <summary>
            /// Just a normal Stroke which will be added to the StrokeCollection
            /// </summary>
            Normal = 0,

            /// <summary>
            /// It's a line like this: ------- --------- --------
            /// </summary>
            Dashed = 1,

            /// <summary>
            /// /// It's a line like this: .......................
            /// </summary>
            Dottet = 2,
        }

        public enum Background : int
        {
            /// <summary>
            /// The normal default background
            /// </summary>
            Default = 0,

            /// <summary>
            /// A wooden texture as background
            /// </summary>
            Wooden1 = 1,

            /// <summary>
            /// Another wooden texture as background
            /// </summary>
            Wooden2 = 2,

            /// <summary>
            /// Sand from a beach as background
            /// </summary>
            Sand = 3,

            /// <summary>
            /// A simple blue background, which looks like water
            /// </summary>
            Blue = 4,


            /// <summary>
            /// A custom defined background
            /// </summary>
            Custom = 100
        }

        public Settings()
        {

        }

        public static Settings Load()
        {
            try
            {
                var result = Serialization.Serialization.Read<Settings>(path, Serialization.Serialization.Mode.Normal);
                if (result != null)
                    return result;
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToLoadSettings}{Environment.NewLine}{Environment.NewLine}{e.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Settings();
        }

        public void Save()
        {
            try
            {
                Serialization.Serialization.Save<Settings>(path, this, Serialization.Serialization.Mode.Normal);
            }
            catch (Exception e)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToSaveSettings}{Environment.NewLine}{Environment.NewLine}{e.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
