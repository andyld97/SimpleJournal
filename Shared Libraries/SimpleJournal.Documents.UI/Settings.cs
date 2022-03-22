using SimpleJournal.Common;
using SimpleJournal.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Size = SimpleJournal.Common.Data.Size;

namespace SimpleJournal.Documents.UI
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
        public Common.Data.Color TextMarkerColor { get; set; } = new Common.Data.Color(255, 255, 0); // yellow

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
        /// Defines the last selected mode to select in the DropDown
        /// </summary>
        public PlotMode LastSelectedPlotMode { get; set; } = PlotMode.Positive;

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
        /// Determines if the Fluent.Ribbon Backstackage Menu (true) is used, or the default application menu
        /// </summary>
        public bool UseNewMenu { get; set; } = true;

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
        public string Theme { get; set; } = "Cobalt"; // Cobalt is the default one used in SJ

        /// <summary>
        /// Determines if the message (inserting an ui element) will be ignored or not!
        /// </summary>
        public bool DoesNotShowInsertHint { get; set; } = false;

        /// <summary>
        /// Determines if the glowing brush of the mainwindow is disabled or not
        /// </summary>
        public bool ActivateGlowingBrush { get; set; } = true;

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

        /// <summary>
        /// Determines if the touch screen will be deactivated in foreground and activated in background
        /// </summary>
        public bool DisableTouchScreenIfInForeground { get; set; } = false;

        /// <summary>
        /// This is just to remember the checkbox in PDFConversationDialog
        /// </summary>
        public bool UseOnlineConversation { get; set; } = false;

        /// <summary>
        /// If true the strokes drawn with a ruler getting straight horizontal or vertical (only if the offset is little enough)
        /// </summary>
        public bool UseRulerCompensation { get; set; } = true;

        /// <summary>
        /// If true the object bar will be transparent
        /// </summary>
        public bool UseObjectBarTransparency { get; set; } = true;

        public Settings()
        {

        }

        public static Settings Load()
        {
            try
            {
                var result = Serialization.Read<Settings>(path, Serialization.Mode.XML);
                if (result != null)
                    return result;
            }
            catch (Exception e)
            {
                 MessageBox.Show($"{SharedResources.Resources.strFailedToLoadSettings}{Environment.NewLine}{Environment.NewLine}{e.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Settings();
        }

        public void Save()
        {
            try
            {
                Serialization.Save<Settings>(path, this, Serialization.Mode.XML);
            }
            catch (Exception e)
            {
                 MessageBox.Show($"{SharedResources.Resources.strFailedToSaveSettings}{Environment.NewLine}{Environment.NewLine}{e.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}