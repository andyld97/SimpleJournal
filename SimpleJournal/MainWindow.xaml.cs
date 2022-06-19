using Controls;
using Fluent;
using Microsoft.Win32;
using Notifications;
using SimpleJournal.Common;
using SimpleJournal.Controls;
using SimpleJournal.Controls.Templates;
using SimpleJournal.Data;
using SimpleJournal.Dialogs;
using SimpleJournal.Documents;
using SimpleJournal.Documents.UI;
using SimpleJournal.Documents.UI.Actions;
using SimpleJournal.Documents.UI.Controls;
using SimpleJournal.Documents.UI.Controls.Paper;
using SimpleJournal.Documents.UI.Data;
using SimpleJournal.Documents.UI.Extensions;
using SimpleJournal.Documents.UI.Helper;
using SimpleJournal.Helper;
using SimpleJournal.Helper.PDF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Xps;
using Action = SimpleJournal.Documents.UI.Actions.Action;
using Clipboard = SimpleJournal.Documents.UI.Data.Clipboard;
using Orientation = SimpleJournal.Common.Orientation;
using Pen = SimpleJournal.Data.Pen;
using Stretch = System.Windows.Media.Stretch;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für Window2.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public ObservableCollection<IPaper> CurrentJournalPages { get; private set; } = new ObservableCollection<IPaper>();
        public static MainWindow W_INSTANCE = null;

        #region Private Members
        private InkCanvasEditingMode currentkInkMode = InkCanvasEditingMode.Ink;
        private readonly DrawingAttributes CurrentDrawingAttributes = new DrawingAttributes() { FitToCurve = Settings.Instance.UseFitToCurve, IsHighlighter = false };
        private readonly DrawingAttributes currentTextMarkerAttributes = new DrawingAttributes() { IsHighlighter = true };
        private string currentJournalPath = string.Empty;
        private string currentJournalTitle = string.Empty;
        private bool isFullScreen = false;
        private bool isInitalized = false;
        private bool arePensInitalized = false;
        private bool startSetupDialog = false;
        private bool preventPageBoxSelectionChanged = false;
        private bool forceOpenSidebar = false;
        private bool ignoreToggleButtonHandling = false;
        private double currentScaleFactor = 1.0;
        private Tools currentTool = Tools.Pencil1;
        private Tools lastSelectedPencil = Tools.Pencil1;
        private PlotMode plotMode;
        private TextAnalyser textAnalyserInstance = null;
        private string currentJournalName = string.Empty;

        /// <summary>
        /// The timer which runs in the background to make a backup
        /// </summary>
        private readonly DispatcherTimer autoSaveBackupTimer = new DispatcherTimer(DispatcherPriority.Background);

        public int CurrentPages => CurrentJournalPages.Count;

        // Buttons
        private readonly Fluent.ToggleButton[] toggleButtons = Array.Empty<Fluent.ToggleButton>();
        private readonly DropDownToggleButton[] dropDownToggleButtons = Array.Empty<DropDownToggleButton>();

        // Templates
        private readonly PenDropDownTemplate[] penTemplates = new PenDropDownTemplate[] { new PenDropDownTemplate(), new PenDropDownTemplate(), new PenDropDownTemplate(), new PenDropDownTemplate() };
        private readonly PenDropDownTemplate textMarkerTemplate = new PenDropDownTemplate();
        private readonly RubberDropDownItemTemplate rubberDropDownTemplate = new RubberDropDownItemTemplate();
        private readonly RulerDropDownTemplate rulerDropDownTemplate = new RulerDropDownTemplate();
        private readonly SelectDropDownTemplate selectDropDownTemplate = new SelectDropDownTemplate();
        private readonly PolygonDropDownTemplate polygonDropDownTemplate = new PolygonDropDownTemplate();
        private readonly AddPageDropDownTemplate addPageDropDownTemplate = new AddPageDropDownTemplate();
        private readonly SimpleFormDropDown simpleFormDropDown = new SimpleFormDropDown();
        private readonly PlotDropDownTemplate plotDropDownTemplate = new PlotDropDownTemplate();

        // Ruler members
        private readonly InkCanvasEditingMode old = InkCanvasEditingMode.Ink;
        private bool rulerWasSelected = false;

        // Rubber members
        private int rubberSizeIndex = 0;
        private bool rubberIsRectangle = true;

        // Old data for restoring windows from fullscreen
        private double left, top, width, height;
        private bool wasMaximized = false;

        // Sidebar members
        private UIElement[] elements = null;
        public bool preventSelection = false;
        private bool preventSelectionChanged = false;
        private readonly StrokeCollection selectedStrokes = new StrokeCollection();

        // Pens
        public Pen[] currentPens = new Data.Pen[Consts.AMOUNT_PENS];

#endregion

        #region Private Properties
        private int SelectedPen
        {
            get
            {
                int result = -1;
                switch (currentTool)
                {
                    case Tools.Pencil1: result = 0; break;
                    case Tools.Pencil2: result = 1; break;
                    case Tools.Pencil3: result = 2; break;
                    case Tools.Pencil4: result = 3; break;
                }
                return result;
            }
        }
#endregion

        #region Constructor
        static MainWindow()
        {
            DrawingCanvas.HideSidebar += DrawingCanvas_HideSidebar;
            DrawingCanvas.PreventSelection += DrawingCanvas_PreventSelection;
        }

        private static void DrawingCanvas_PreventSelection(object sender, EventArgs e)
        {
            if (MainWindow.W_INSTANCE.IsSideBarVisible)
                MainWindow.W_INSTANCE.preventSelection = false;
        }

        private static void DrawingCanvas_HideSidebar(object sender, int e)
        {
            if (e > 0 && MainWindow.W_INSTANCE.IsSideBarVisible)
                MainWindow.W_INSTANCE.preventSelection = false;
            else if (MainWindow.W_INSTANCE.IsSideBarVisible)
                MainWindow.W_INSTANCE.pnlSidebar.Visibility = Visibility.Collapsed;
        }

        public MainWindow()
        {
            W_INSTANCE = this;
            isInitalized = false;
            InitializeComponent();

            var dpi = VisualTreeHelper.GetDpi(this);
            if (dpi.PixelsPerInchX == 96 && WpfScreen.Primary.DeviceBounds.Width >= 1920 && WpfScreen.Primary.DeviceBounds.Height >= 1080)
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

            UpdateTitle(Properties.Resources.strNewJournal);
            isInitalized = true;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            // Display last opened files
            RefreshRecentlyOpenedFiles();
            RecentlyOpenedDocuments.DocumentsChanged += delegate ()
            {
                RefreshRecentlyOpenedFiles();
            };

            CurrentJournalPages.CollectionChanged += IPages_CollectionChanged;
            var page = GeneratePage();
            DrawingCanvas.LastModifiedCanvas = AddPage(page);
            cmbPages.SelectedIndex = 0;

            CurrentDrawingAttributes = (page as IPaper).Canvas.DefaultDrawingAttributes;

            // Register file association
            if (!Settings.Instance.FirstStart)
            {
                Settings.Instance.FirstStart = true;
                Settings.Instance.Save();

                // ToDo: *** Move this out of first start (so that this is checked on every start?)
#if !UWP
                string executable = Consts.Executable;
                try
                {
                    SimpleJournal.Common.FileAssociations.FileAssociations.EnsureAssociationsSet(executable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(SharedResources.Resources.strFailedToSetFileAssoc_Message, ex.Message), SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Display Setup Dialog
                startSetupDialog = true;
#endif
            }

#if UWP
            // Do this everytime to ensure the newest SJFileAssoc Update gets applied!
            GeneralHelper.InstallUWPFileAssoc();
#endif



            toggleButtons = new Fluent.ToggleButton[]
            {
                btnRubberGrob,
                btnRecogniziation,
            };

            dropDownToggleButtons = new DropDownToggleButton[]
            {
                btnRuler,
                btnRubberFine,
                btnPen1,
                btnPen2,
                btnPen3,
                btnPen4,
                btnTextMarker,
                btnSelect,
                btnFreeHandPolygon,
                btnInsertSimpleForm,
                btnInsertPlot
            };

            // Handle keydown
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.F11)
                {
                    ToggleFullscreen();
                }
                else if (e.Key == Key.O && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    // Open
                    btnOpen_Click(null, null);
                }
            };


            // Handle events
            PageManagementControl.DialogClosed += async delegate (object semder, bool e)
            {
                if (e)
                    await ApplyPageManagmentDialog(PageManagementControl.Result);

                MenuBackstage.IsOpen = false;
            };

            ExportControl.DialogClosed += delegate (object sender, bool e)
            {
                MenuBackstage.IsOpen = false;
            };

            ExportControl.TitleChanged += delegate (object sender, string e)
            {
                if (e == Properties.Resources.strExportPages)
                    TextExportStatus.Text = string.Empty;
                else
                    TextExportStatus.Text = e;
            };

            State.Initalize(new[] {
                Properties.Resources.strStateSaving,
                Properties.Resources.strStateExportAsPDF,
                Properties.Resources.strStateExportAsJournal,
                Properties.Resources.strStatePrinting,
            });

            State.OnStateChanged += delegate (string message, ProgressState state)
            {
                MainStatusBar.Visibility = (state == ProgressState.Start ? Visibility.Visible : Visibility.Collapsed);
                TextStatusBar.Text = message;
            };

            Journal.OnErrorOccured += delegate (string message, string scope)
            {
                if (scope == "load")
                    MessageBox.Show($"{Properties.Resources.strFailedToLoadJournal} {message}", Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                else if (scope == "save")
                    MessageBox.Show($"{Properties.Resources.strFailedToSaveJournal} {message}", Properties.Resources.strFailedToSaveJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            };

            ImageHelper.OnErrorOccured += delegate (string message, string scope)
            {
                if (scope == "export")
                    MessageBox.Show($"{Properties.Resources.strFailedToSaveImage} {message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
                else if (scope == "load")
                    MessageBox.Show($"{Properties.Resources.strFailedToLoadImage} {message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Boot with fullscreen
            left = Left;
            top = Top;
            width = Width;
            height = Height;

            // Maximized = Full screen
            // Normal = Maximized
            // Minimized = Normal
            if (Settings.Instance.WindowState == WindowState.Maximized)
                ToggleFullscreen();
            else if (Settings.Instance.WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;

            // Call other init methods
            RefreshVerticalScrollbarSize();
            UpdateTextMarkerAttributes();
            UpdateNotificationToolBarAndButton();

            DrawingCanvas.ChildElementsSelected += DrawingCanvas_ChildElementsSelected;

            // Apply default size to textmarker
            var defaultSize = Documents.UI.Consts.TextMarkerSizes[0];
            currentTextMarkerAttributes.Height = defaultSize.Width;
            currentTextMarkerAttributes.Width = defaultSize.Height;

            // Apply default zoom factor
            ZoomByScale(Settings.Instance.Zoom / 100.0);

            currentPens = Data.Pen.Instance;

            // Apply pens to gui
            UpdatePenButtons();
            UpdateTextMarker();
            UpdateDropDownButtons();

            // Make sure the insert button has the right page icon
            RefreshInsertIcon();

            // Apply first and selected pen
            CurrentDrawingAttributes.Color = currentPens[0].FontColor.ToColor();
            CurrentDrawingAttributes.Width = currentPens[0].Width;
            CurrentDrawingAttributes.Height = currentPens[0].Height;
            plotMode = Settings.Instance.LastSelectedPlotMode;
            RefreshSizeBar();
            UpdateGlowingBrush();
            UpdateMenu();

#if !UWP
            GeneralHelper.SearchForUpdates();            
#endif

            DrawingCanvas.OnChangedDocumentState += DrawingCanvas_OnChangedDocumentState;

            var screen = WpfScreen.GetScreenFrom(this);
            if (screen != null)
            {
                var bounds = screen.DeviceBounds;
                if (bounds.Width > bounds.Height)
                {
                    Width = Math.Min(1130, bounds.Width);
                    Height = Math.Min(800, bounds.Height);
                }
                else
                {
                    Height = Math.Min(800, bounds.Height);
                    Width = bounds.Width;
                }
            }

            ApplyBackground();
        }

        private void DrawingCanvas_OnChangedDocumentState(bool value)
        {
            UpdateTitle(string.Empty, value);
        }

        private void RefreshRecentlyOpenedFiles()
        {
            ListRecentlyOpenedDocuments.ItemsSource = null;
            ListRecentlyOpenedDocuments.Items.Clear();
            ListRecentlyOpenedDocuments.ItemsSource = RecentlyOpenedDocuments.Instance;

            RecentlyOpenedDocumentsBackstage.ItemsSource = null;
            RecentlyOpenedDocumentsBackstage.Items.Clear();
            RecentlyOpenedDocumentsBackstage.ItemsSource = RecentlyOpenedDocuments.Instance;

            int entries = RecentlyOpenedDocuments.Instance.Count;
            string text = entries > 0 ? entries.ToString() : Properties.Resources.strRecentlyOpenedDocuments_No;

            MenuAppRecentlyOpendedFilesCount.Text = text;
            MenuBackstageRecentlyOpendedFilesCount.Text = text;
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Start service
            NotificationService.NotificationServiceInstance = new NotificationService();
            RefreshNotifications(); // do it manually here, because otheriwse already added notifications won't get displayed!
            NotificationService.NotificationServiceInstance.OnNotificationAdded += NotificationServiceInstance_OnNotificationAdded;
            NotificationService.NotificationServiceInstance.OnNotifcationRemoved += NotificationServiceInstance_OnNotifcationRemoved;
            NotificationService.NotificationServiceInstance.Start();;

            if (startSetupDialog)
            {
                startSetupDialog = false;
                new SetupDialog().ShowDialog();
            }

            // Check if command line parameter was passed
            if (!string.IsNullOrEmpty(App.Path) && System.IO.File.Exists(App.Path))
                await LoadJournal(App.Path);

            if (Settings.Instance.UseAutoSave)
                await ShowRecoverAutoBackupFileDialog();

#if !UWP

            if (Settings.Instance.UseTouchScreenDisabling && ProcessHelper.SimpleJournalProcessCount == 1)
            {
                // If there are more than one process, don't start it
                TouchHelper.SetTouchState(false);
            }

#endif
        }


        private void IPages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshPages();
        }
        #endregion

        #region Notifications

        // ToDo!!!!

        private void NotificationServiceInstance_OnNotifcationRemoved(Notification notification)
        {
            Dispatcher.Invoke(() =>
            {
                RefreshNotifications();
            });
        }

        private void NotificationServiceInstance_OnNotificationAdded(Notification notification)
        {
            Dispatcher.Invoke(() => 
            {
                RefreshNotifications();
            });
        }

        private void RefreshNotifications()
        {
            if (NotificationService.NotificationServiceInstance == null)
                return;

            int count = NotificationService.NotificationServiceInstance.Notifications.Count;

            if (count > 0)
            {
                btnToggleNotification.Header = count.ToString();
                btnToggleNotification.SizeDefinition = new RibbonControlSizeDefinition("middle");
            }
            else
            {
                btnToggleNotification.Header = string.Empty;
                btnToggleNotification.SizeDefinition = new RibbonControlSizeDefinition("small");
            }

            ListNotifications.Children.Clear();
            foreach (var notification in NotificationService.NotificationServiceInstance?.Notifications)
                ListNotifications.Children.Add(new NotificationDisplay(notification) { Margin = new Thickness(3, 4, 3, 4) });
        }

        public void UpdateNotificationToolBarAndButton()
        {
            if (Settings.Instance.HideNotificationToolBar)
            {
                btnToggleNotification.IsChecked = false;
                btnToggleNotification.Visibility = Visibility.Collapsed;

                if (NotificationService.NotificationServiceInstance != null && NotificationService.NotificationServiceInstance.IsRunning)
                    NotificationService.NotificationServiceInstance?.Stop();
            }
            else
            {
                btnToggleNotification.Visibility = Visibility.Visible;

                if (NotificationService.NotificationServiceInstance != null && !NotificationService.NotificationServiceInstance.IsRunning)
                    NotificationService.NotificationServiceInstance.Start();
            }
        }

        private void ButtonCloseNotificationsPanel_Click(object sender, RoutedEventArgs e)
        {
            btnToggleNotification.IsChecked = false;
        }
        #endregion

        #region AutoSave - Backup

        private string lastBackupFileName = string.Empty;

        private async Task ShowRecoverAutoBackupFileDialog()
        {
            // Show this dialog only if there a backup files!
            bool showRecoverDialog = false;
            bool autoSaveDirectoryExists = System.IO.Directory.Exists(Consts.AutoSaveDirectory);

            if (autoSaveDirectoryExists)
            {
                // Okay, so more than one process is open
                // Check if all existing backups are active, 
                // if there is a backup which is flagged with non-existent task-id, than
                // it's really a backup - so we need to recover it

                // Reset this value, because if it's true it will never be false again
                showRecoverDialog = false;

                IEnumerable<System.IO.FileInfo> autoSaveFiles = new System.IO.DirectoryInfo(Consts.AutoSaveDirectory).EnumerateFiles();
                var backupFiles = autoSaveFiles.Where(f => f.Name.EndsWith(".journal"));

                foreach (var journalFile in backupFiles)
                {
                    try
                    {
                        Journal j = await Journal.LoadJournalMetaAsync(journalFile.FullName);
                        if (j == null)
                        {
                            // delete invalid files
                            try
                            {
                                System.IO.File.Delete(journalFile.FullName);
                            }
                            catch
                            {

                            }

                            continue;
                        }

                        showRecoverDialog |= j.IsBackup && !ProcessHelper.IsProcessActiveByTaskId(j.ProcessID);

                        j.Pages.Clear();
                        j = null;

                        // If it is true => then no more search needed
                        if (showRecoverDialog)
                            break;
                    }
                    catch
                    {

                    }
                }
            }

            if (showRecoverDialog && !App.IgnoreBackups)
            {
                RecoverAutoBackupFileDialog rabfd = new RecoverAutoBackupFileDialog();

                bool? result = rabfd.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    // If result was successfully, there is a new file, so we can delete the old backup file
                    DeleteAutoSaveBackup();

                    // Bring this window to the front
                    Topmost = true;
                    Topmost = false;
                    Activate();
                }
                else
                    DeleteAutoSaveBackup();
            }

            // Start timer just after the dialog is appeard (or not)
            autoSaveBackupTimer.Interval = TimeSpan.FromMinutes(Settings.Instance.AutoSaveIntervalMinutes);
            autoSaveBackupTimer.Tick += AutoSaveBackupTimer_Tick;
            autoSaveBackupTimer.Start();
        }

        private async void AutoSaveBackupTimer_Tick(object sender, EventArgs e)
        {
            await CreateBackup();
        }

        private async Task CreateBackup()
        {
            if (!Settings.Instance.UseAutoSave)
            {
                autoSaveBackupTimer.Stop();
                return;
            }

            // Do autosave 
            if (!System.IO.Directory.Exists(Consts.AutoSaveDirectory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(Consts.AutoSaveDirectory);
                }
                catch
                {
                    return;
                }
            }

            string backupName;
            if (!string.IsNullOrEmpty(currentJournalPath))
                backupName = $"Backup {ProcessHelper.CurrentProcID} ({System.IO.Path.GetFileNameWithoutExtension(currentJournalPath)}) ";
            else
                backupName = $"Backup {ProcessHelper.CurrentProcID} - ";
            backupName += $"{DateTime.Now.ToString(Properties.Resources.strAutoSaveDateTimeFileFormat)}.journal";

            string path = System.IO.Path.Combine(Consts.AutoSaveDirectory, backupName);
            bool result = await SaveJournal(path, true);
            if (!result)
            {
                try
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                catch
                {

                }
            }

            // ToDo: *** Slighty notification that autosave failed (if result == FALSE)?

            // Delete the old backup only if the new one was successfull
            if (!string.IsNullOrEmpty(lastBackupFileName) && result)
            {
                try
                {
                    System.IO.File.Delete(lastBackupFileName);
                }
                catch
                {
                    // ignore
                }
            }

            lastBackupFileName = path;
            return;
        }

        public void DeleteAutoSaveBackup(bool onClosing = false)
        {
            // Delete the backup with this ProcessID
            // Get the backup with this task id - it's obviously the last one, this instace has created!
            if (!string.IsNullOrEmpty(lastBackupFileName) && System.IO.File.Exists(lastBackupFileName))
            {
                try
                {
                    System.IO.File.Delete(lastBackupFileName);
                }
                catch
                {

                }
            }

            // Clear last backup path
            lastBackupFileName = null;

            // Only the last instance can set the value
            if (onClosing && ProcessHelper.SimpleJournalProcessCount == 1)
            {
                try
                {
                    // Only delete empty directory (0 files and false (no recursive))
                    if (new System.IO.DirectoryInfo(Consts.AutoSaveDirectory).GetFiles().Length == 0)
                        System.IO.Directory.Delete(Consts.AutoSaveDirectory, false);
                }
                catch
                {
                    // ignore
                }
            }
        }

        #endregion

        #region Error Handling

        // ToDO: *** https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/23
        // Related:  https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/27
        // Related:  https://github.com/dotnet/runtime/discussions/64942
        // Currently only Dispatcher_UnhandledException gets called due to DotnetRuntimeBootstrapper
        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // MessageBox.Show(e.Exception.ToString());
            // MessageBox.Show($"{Properties.Resources.strUnexceptedFailure}{Environment.NewLine}{Environment.NewLine}{e.Exception.Message}", Properties.Resources.strUnexceptedFailureTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = string.Empty;

            if (e.ExceptionObject != null && e.ExceptionObject is Exception ex)
            {
                // Shorten the output length of the MessageBox in production code!
#if DEBUG
                message = ex.ToString();

                if (ex.InnerException != null)
                    message += Environment.NewLine + Environment.NewLine + ex.InnerException.ToString();
#else
                message = ex.Message;

                if (ex.InnerException != null)
                    message += Environment.NewLine + Environment.NewLine + ex.InnerException.Message;
#endif
            }

            MessageBox.Show($"{Properties.Resources.strUnexceptedFailure}{Environment.NewLine}{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.strUnexceptedFailureLine1}", Properties.Resources.strUnexceptedFailureTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            // Try at least to create a backup - if SJ crashes - the user can restore the backup and everything should be fine though.
            await CreateBackup();
        }
#endregion

        #region Determine which Canvas is the last modifed while scrolling

        private void mainScrollView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!isInitalized)
                return;

            int index = (int)CalculateCurrentPageIndexOnScrollPosition();
            preventPageBoxSelectionChanged = true;
            cmbPages.SelectedIndex = index;
            preventPageBoxSelectionChanged = false;

            int counter = 0;
            foreach (FrameworkElement child in pages.Children)
            {
                if (child != null && child is UserControl && child is IPaper paper && counter == index)
                {
                    var can = paper.Canvas;

                    if (DrawingCanvas.LastModifiedCanvas != can)
                    {
                        DrawingCanvas.LastModifiedCanvas = can;

                        if (pnlSidebar.IsVisible)
                        {
                            if (can.Children.Count > 0)
                            {
                                can.Select(new UIElement[] { can.Children.First() });
                                DrawingCanvas_ChildElementsSelected(DrawingCanvas.LastModifiedCanvas.GetSelectedElements().ToArray());
                                pnlSidebar.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pnlSidebar.Visibility = Visibility.Collapsed;
                                elements = null;
                                pnlItems.Items.Clear();
                            }
                        }
                        else if (DrawingCanvas.LastModifiedCanvas.GetSelectedElements().Count > 0)
                        {
                            // Reactive siderbar
                            DrawingCanvas_ChildElementsSelected(DrawingCanvas.LastModifiedCanvas.GetSelectedElements().ToArray());
                            pnlSidebar.Visibility = Visibility.Visible;
                        }
                        break;
                    }
                    break;
                }
                else if (child is IPaper)
                    counter++;
            }
        }
#endregion

        #region Sidebar Handling

        public bool IsSideBarVisible => pnlSidebar.IsVisible;

        private void DrawingCanvas_ChildElementsSelected(UIElement[] elements)
        {
            if (preventSelection || (!Settings.Instance.DisplaySidebarAutomatically && !forceOpenSidebar))
                return;

            if (forceOpenSidebar)
                forceOpenSidebar = false;

            this.elements = elements;

            // Show sidebar only if there're elements to show (and also ignore lines)
            if (elements.Length == 0 || elements.Where(p => p is Line).Count() == elements.Length)
            {
                pnlSidebar.Visibility = Visibility.Hidden;
                return;
            }

            // Display ui elements in sidebar
            pnlItems.Items.Clear();
            pnlSidebar.Visibility = Visibility.Visible;

            foreach (UIElement element in DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)))
            {
                string text = Properties.Resources.strUnknown;
                bool isTextBlock = false;

                if (element is Shape shape)
                {
                    double w = Math.Round(shape.Width, 1);
                    double h = Math.Round(shape.Height, 1);

                    if (element is Polygon pol)
                    {
                        // Either pol.Points.Count nor the ConvexHull builds the correct amount of points!
                        int edges = pol.CountEdges();
                        if (edges == 3)
                            text = Properties.Resources.strTriangle;
                        else if (edges == 4)
                            text = Properties.Resources.strQuad;
                        else if (edges == 5)
                            text = Properties.Resources.strPentagon;
                        else if (edges == 6)
                            text = Properties.Resources.strHexagon;
                        else
                            text = Properties.Resources.strPolygon;
                    }
                    else if (element is Ellipse el)
                    {
                        if (w == h)
                            text = Properties.Resources.strCircle;
                        else
                            text = Properties.Resources.strEllipse;
                    }
                    else if (element is Rectangle)
                    {
                        if (w == h)
                            text = Properties.Resources.strSquare;
                        else
                            text = Properties.Resources.strRectangle;
                    }
                    else
                        text = Properties.Resources.strShape;
                }
                else if (element is Image)
                    text = Properties.Resources.strImage;
                else if (element is TextBlock)
                {
                    text = Properties.Resources.strText;
                    isTextBlock = true;
                }
                else if (element is Plot)
                    text = Properties.Resources.strPlot;

                Viewbox previewViewBox = new Viewbox
                {
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.Both
                };

                if (!isTextBlock)
                    previewViewBox.Child = UIHelper.CloneElement(element);
                else
                {
                    var img = new Image
                    {
                        Source = ImageHelper.LoadImage(new Uri($"pack://application:,,,/SimpleJournal;component/resources/text.png")),
                        Width = Consts.SidebarListBoxItemViewboxSize,
                        Height = Consts.SidebarListBoxItemViewboxSize
                    };
                    previewViewBox.Child = img;
                }

                var item = new CustomListBoxItem(element, previewViewBox)
                {
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Height = Consts.SidebarListBoxItemHeight
                };

                previewViewBox.Width = Consts.SidebarListBoxItemViewboxSize;
                previewViewBox.Height = Consts.SidebarListBoxItemViewboxSize;

                StackPanel panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
                panel.Background = new SolidColorBrush(Colors.Transparent);
                panel.Children.Add(previewViewBox);

                var textBlock = new TextBlock()
                {
                    Text = text,
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                textBlock.FontWeight = FontWeights.Bold;
                textBlock.SetResourceReference(TextBlock.ForegroundProperty, "BlackBrush"); // new Binding("BlackBrush"));
                panel.Children.Add(textBlock);

                item.Content = panel;
                pnlItems.Items.Add(item);
            }

            preventSelection = true;
            preventSelectionChanged = true;
            pnlItems.SelectedItems.Clear();

            foreach (UIElement ui in elements)
            {
                int index = DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)).ToList().IndexOf(ui);
                if (index != -1)
                    pnlItems.SelectedItems.Add(pnlItems.Items[index]);
            }

            if (pnlItems.Items.Count == 0)
                pnlSidebar.Visibility = Visibility.Hidden;
            preventSelection = false;
            preventSelectionChanged = false;
        }

        private void pnlItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (preventSelectionChanged)
                return;

            int delta = e.RemovedItems.Count;
            if (delta == 1 && pnlItems.SelectedItems.Count == 0)
            {
                preventSelectionChanged = true;
                pnlItems.SelectedItems.Add(e.RemovedItems[0]);
                preventSelectionChanged = false;
            }

            if (pnlItems.Items.Count != 0 && pnlItems.SelectedIndex != -1)
            {
                List<UIElement> selection = new List<UIElement>();
                foreach (var item in pnlItems.SelectedItems)
                {
                    int index = pnlItems.Items.IndexOf(item);

                    if (index != -1)
                        selection.Add(DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)).ToList()[index]);
                }
                preventSelection = true;
                DrawingCanvas.LastModifiedCanvas.Select(selection);
                preventSelection = false;
            }
            else if (pnlItems.SelectedItems.Count == 0 && pnlItems.Items.Count == 0)
            {
                // Hide sidebar
                pnlSidebar.Visibility = Visibility.Hidden;
            }
        }

        private void btnSwitchSidebarOff_Click(object sender, RoutedEventArgs e)
        {
            pnlSidebar.Visibility = Visibility.Collapsed;
        }

        private void btnDeleteSidebarItem_Click(object sender, RoutedEventArgs e)
        {
            if (pnlItems.SelectedItems.Count > 0)
            {
                preventSelectionChanged = true;
                List<CustomListBoxItem> customItems = new List<CustomListBoxItem>();
                foreach (CustomListBoxItem li in pnlItems.SelectedItems)
                    customItems.Add(li);

                foreach (CustomListBoxItem li in customItems)
                {
                    // Lines are not displayed in the sidebar, so ignore them
                    // But lines are not added to the sidebar, so they are not relevant anymore.
                    DrawingCanvas.LastModifiedCanvas.Children.Remove(li.AssociativeRelation);
                    pnlItems.Items.Remove(li);
                }

                preventSelectionChanged = false;

                if (pnlItems.Items.Count == 0)
                    pnlSidebar.Visibility = Visibility.Hidden;

            }
            else
                MessageBox.Show(this, Properties.Resources.strNoObjectsToDelete, Properties.Resources.strEmptySelection, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Canvas_SelectionChanged(object sender, EventArgs e)
        {
            var items = pnlItems.SelectedItems;
            if (items.Count == 0 || items.Count > 1)
            {
                objImgSettings.Visibility = Visibility.Collapsed;
                objShapeSettings.Visibility = Visibility.Collapsed;
                objTextSettings.Visibility = Visibility.Collapsed;
            }
            else
            {
                var item = (items[0] as CustomListBoxItem).AssociativeRelation;

                if (item is Shape sh)
                {
                    objShapeSettings.Visibility = Visibility.Visible;
                    objImgSettings.Visibility = Visibility.Collapsed;
                    objTextSettings.Visibility = Visibility.Collapsed;

                    UIHelper.ConvertTransformToProperties(sh, DrawingCanvas.LastModifiedCanvas);

                    int angle = 0;
                    if (sh.RenderTransform is RotateTransform rt)
                        angle = (int)rt.Angle;
                    if (sh.RenderTransform is TransformGroup tg && tg.Children.Count == 2 && tg.Children.LastOrDefault() is RotateTransform _rt)
                        angle = (int)_rt.Angle;

                    objShapeSettings.Load(new ShapeInfo((sh.Fill == null ? Colors.Transparent : (sh.Fill as SolidColorBrush).Color), (sh.Stroke as SolidColorBrush).Color, (int)sh.StrokeThickness, angle));
                }
                else if (item is Image im)
                {
                    objImgSettings.Visibility = Visibility.Visible;
                    objShapeSettings.Visibility = Visibility.Collapsed;
                    objTextSettings.Visibility = Visibility.Collapsed;

                    int angle = 0;
                    if (im.RenderTransform is RotateTransform rt)
                        angle = (int)rt.Angle;

                    UIHelper.ConvertTransformToProperties(im, DrawingCanvas.LastModifiedCanvas);
                    objImgSettings.Load(angle, im.Stretch);
                }
                else if (item is TextBlock tb)
                {
                    objTextSettings.Visibility = Visibility.Visible;
                    objShapeSettings.Visibility = Visibility.Collapsed;
                    objImgSettings.Visibility = Visibility.Collapsed;

                    UIHelper.ConvertTransformToProperties(tb, DrawingCanvas.LastModifiedCanvas);

                    int angle = 0;
                    if (tb.RenderTransform != null && tb.RenderTransform is RotateTransform rt)
                        angle = (int)rt.Angle;

                    var jrText = tb.ConvertText() as JournalText;
                    objTextSettings.Load(new TextData()
                    {
                        Angle = angle,
                        Content = tb.Text,
                        FontColor = (tb.Foreground as SolidColorBrush).Color,
                        FontSize = tb.FontSize,
                        FontFamily = tb.FontFamily.ToString(),
                        IsBold = jrText.IsBold,
                        IsItalic = jrText.IsItalic,
                        IsStrikeout = jrText.IsStrikeout,
                        IsUnderlined = jrText.IsUnderlined
                    });
                }
                else
                {
                    objImgSettings.Visibility = Visibility.Collapsed;
                    objShapeSettings.Visibility = Visibility.Collapsed;
                    objTextSettings.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ObjTextSettings_OnChanged(TextData data)
        {
            var can = DrawingCanvas.LastModifiedCanvas;
            DrawingCanvas.Change = true;

            if (can.GetSelectedElements().Count > 0)
            {
                var item = (pnlItems.Items[pnlItems.SelectedIndex] as CustomListBoxItem).AssociativeRelation;

                if (item is TextBlock tb)
                {
                    tb.TextDecorations.Clear();

                    // UnDo/ReDo Idea. Works but see Bug 39 in DevOps
                    /* DrawingCanvas.LastModifiedCanvas.Manager.RunSpecialAction<PropertyChangedAction>(new List<PropertyChangedAction>() {
                         new PropertyChangedAction(data.Content, tb.Text, (object s) => { tb.Text = s.ToString(); })
                     });*/

                    tb.Text = data.Content;
                    tb.FontFamily = new FontFamily(data.FontFamily);
                    tb.FontSize = data.FontSize;
                    tb.Foreground = new SolidColorBrush(data.FontColor);
                    tb.RenderTransform = new RotateTransform(data.Angle);
                    tb.RenderTransformOrigin = new Point(0.5, 0.5);

                    if (data.IsBold)
                        tb.FontWeight = FontWeights.Bold;
                    else
                        tb.FontWeight = FontWeights.Normal;

                    if (data.IsItalic)
                        tb.FontStyle = FontStyles.Italic;
                    else
                        tb.FontStyle = FontStyles.Normal;

                    if (data.IsUnderlined)
                        tb.TextDecorations.Add(TextDecorations.Underline);

                    if (data.IsStrikeout)
                        tb.TextDecorations.Add(TextDecorations.Strikethrough);
                }

                (pnlItems.Items[pnlItems.SelectedIndex] as CustomListBoxItem).Refresh();
            }
        }

        private void ObjShapeSettings_OnChanged_1(ShapeInfo info)
        {
            var can = DrawingCanvas.LastModifiedCanvas;
            DrawingCanvas.Change = true;
            Point center = new Point(0.5, 0.5);

            if (can.GetSelectedElements().Count > 0)
            {
                var item = (pnlItems.Items[pnlItems.SelectedIndex] as CustomListBoxItem).AssociativeRelation;

                if (item is Shape sh)
                {
                    if (item is Ellipse el && el.IsCricle())
                    {
                        // Don't rotate a circle
                    }
                    else
                    {
                        if (sh.RenderTransform is TransformGroup grp && grp.Children.LastOrDefault() is RotateTransform rt)
                            rt.Angle = info.Angle;

                        if (sh.RenderTransform != null)
                            (sh.RenderTransform as RotateTransform).Angle = info.Angle;
                        else 
                            sh.RenderTransform = new RotateTransform(info.Angle);
                        sh.RenderTransformOrigin = center;
                    }
                    sh.Fill = new SolidColorBrush(info.BackgroundColor);
                    sh.Stroke = new SolidColorBrush(info.BorderColor);
                    sh.StrokeThickness = info.BorderWidth;
                }

                (pnlItems.Items[pnlItems.SelectedIndex] as CustomListBoxItem).Refresh();
            }
        }

        private void ObjImgSettings_Changed(int? angle, Common.Stretch? stretch)
        {
            // Rotate image with it's origin in the center
            var can = DrawingCanvas.LastModifiedCanvas;
            DrawingCanvas.Change = true;
            Point center = new Point(0.5, 0.5);

            if (can.GetSelectedElements().Count > 0)
            {
                var item = (pnlItems.Items[pnlItems.SelectedIndex] as CustomListBoxItem).AssociativeRelation;

                if (item is Image im)
                {
                    if (angle != null)
                    {
                        im.RenderTransform = new RotateTransform(angle.Value);
                        im.RenderTransformOrigin = center;
                    }

                    if (stretch != null)
                        im.Stretch = stretch.Value.ConvertStretch();
                }

                (pnlItems.Items[pnlItems.SelectedIndex] as CustomListBoxItem).Refresh();
            }
        }

        private void ButtonBringToFront_Click(object sender, RoutedEventArgs e)
        {
            var can = DrawingCanvas.LastModifiedCanvas;

            if (can == null || can.GetSelectedElements() == null)
                return;

            var elements = can.GetSelectedElements();

            if (elements != null && elements.Count == 1)
                elements.FirstOrDefault().BringToFront(DrawingCanvas.LastModifiedCanvas);
            else
                elements.BringToFront(DrawingCanvas.LastModifiedCanvas);
        }

        #endregion

        #region Private Methods

        public void UpdateTextMarkerAttributes(bool reset = false)
        {
            if (reset)
            {
                // Default values
                var defaultSettings = new Settings();
                currentTextMarkerAttributes.Width = defaultSettings.TextMarkerSize.Height; // Consts.TEXT_MARKER_WIDTH;
                currentTextMarkerAttributes.Height = defaultSettings.TextMarkerSize.Width; // Consts.TEXT_MARKER_HEIGHT;
                currentTextMarkerAttributes.StylusTip = StylusTip.Rectangle;
                currentTextMarkerAttributes.Color = defaultSettings.TextMarkerColor.ToColor(); //Consts.TEXT_MARKER_COLOR;

                Settings.Instance.TextMarkerSize = Documents.UI.Consts.TextMarkerSizes[0];
                Settings.Instance.TextMarkerColor = new Common.Data.Color(Consts.TextMarkerColor.A, Consts.TextMarkerColor.R, Consts.TextMarkerColor.G, Consts.TextMarkerColor.B);
                Settings.Instance.Save();
            }
            else
            {
                currentTextMarkerAttributes.Width = Settings.Instance.TextMarkerSize.Height; // Consts.TEXT_MARKER_WIDTH;
                currentTextMarkerAttributes.Height = Settings.Instance.TextMarkerSize.Width; // Consts.TEXT_MARKER_HEIGHT;
                currentTextMarkerAttributes.StylusTip = StylusTip.Rectangle;
                currentTextMarkerAttributes.Color = Settings.Instance.TextMarkerColor.ToColor(); //Consts.TEXT_MARKER_COLOR;
            }

            markerPath.Fill = new SolidColorBrush(currentTextMarkerAttributes.Color);
            markerPath.Stroke = Brushes.Black;
            markerPath.StrokeThickness = Consts.MarkerPathStrokeThickness;
            textMarkerTemplate.LoadPen(new Pen(currentTextMarkerAttributes.Color.ToColor(), Settings.Instance.TextMarkerSize.Width, Settings.Instance.TextMarkerSize.Height));

            if (currentTool == Tools.TextMarker)
            {
                ApplyToAllCanvas((DrawingCanvas dc) =>
                {
                    dc.DefaultDrawingAttributes = currentTextMarkerAttributes;
                });
            }
        }

        public void UpdateDropDownButtons()
        {
            if (!arePensInitalized)
            {
                arePensInitalized = true;

                btnPen1.DropDown = penTemplates[0];
                btnPen2.DropDown = penTemplates[1];
                btnPen3.DropDown = penTemplates[2];
                btnPen4.DropDown = penTemplates[3];

                penTemplates[0].OnChangedColorAndSize += BtnPen1_OnChanged;
                penTemplates[1].OnChangedColorAndSize += BtnPen2_OnChanged;
                penTemplates[2].OnChangedColorAndSize += BtnPen3_OnChanged;
                penTemplates[3].OnChangedColorAndSize += BtnPen4_OnChanged;

                textMarkerTemplate.SetTextMarker();
                textMarkerTemplate.LoadPen(new Pen(Settings.Instance.TextMarkerColor, Settings.Instance.TextMarkerSize.Width, Settings.Instance.TextMarkerSize.Height));
                textMarkerTemplate.OnChangedColorAndSize += BtnTextMarker_OnChanged;
                btnTextMarker.DropDown = textMarkerTemplate;

                btnRubberFine.DropDown = rubberDropDownTemplate;
                rubberDropDownTemplate.OnChangedRubber += BtnRubberFine_OnChangedRubber;

                btnRuler.DropDown = rulerDropDownTemplate;
                rulerDropDownTemplate.OnChangedRulerMode += RulerDropDownTemplate_OnChangedRulerMode;
                rulerDropDownTemplate.SetColor(currentPens[0].FontColor.ToColor());

                btnSelect.DropDown = selectDropDownTemplate;
                selectDropDownTemplate.OnColorAndSizeChanged += SelectDropDownTemplate_OnColorAndSizeChanged;

                btnFreeHandPolygon.DropDown = polygonDropDownTemplate;
                btnInsertNewPage.DropDown = addPageDropDownTemplate;
                btnInsertSimpleForm.DropDown = simpleFormDropDown;
                simpleFormDropDown.OnSimpleFormDropDownChanged += SimpleFormDropDown_OnSimpleFormDropDownChanged;
                btnInsertPlot.DropDown = plotDropDownTemplate;
                plotDropDownTemplate.OnPlotModeChanged += PlotDropDownTemplate_OnPlotModeChanged;
                addPageDropDownTemplate.AddPage += AddPageDropDownTemplate_AddPage;

                // polygonDropDownTemplate.OnChanged ...
            }

            penTemplates[0].LoadPen(currentPens[0]);
            penTemplates[1].LoadPen(currentPens[1]);
            penTemplates[2].LoadPen(currentPens[2]);
            penTemplates[3].LoadPen(currentPens[3]);
        }

        private void AddNewPage(PaperType paperType, Orientation orientation)
        {
            AddPage(GeneratePage(paperType, orientation: orientation));
            RefreshInsertIcon();
            ScrollToPage(CurrentJournalPages.Count - 1);
            DrawingCanvas.Change = true;
        }

        private void RefreshInsertIcon()
        {
            string resourceImageName = string.Empty;
            // Switch icon to paperType
            switch (Settings.Instance.PaperTypeLastInserted)
            {
                case PaperType.Blanco: resourceImageName = "addblancopage"; break;
                case PaperType.Chequered: resourceImageName = "addchequeredpage"; break;
                case PaperType.Ruled: resourceImageName = "addruledpage"; break;
                case PaperType.Dotted: resourceImageName = "adddottedpage"; break;
            }

            // In case of a new PaperType is not 100% supported/implemented
            if (string.IsNullOrEmpty(resourceImageName))
                return;

            if (Settings.Instance.OrientationLastInserted == Orientation.Landscape)
                resourceImageName += "_landscape";

            resourceImageName += ".png";

            try
            {
                ButtonInsertNewPageIcon.Source = ImageHelper.LoadImage(new Uri($"pack://application:,,,/SimpleJournal.SharedResources;component/icons/pages/{resourceImageName}"));
            }
            catch
            {
                // ignore - it's just an image which is empty then.
            }
        }

        private void AddPageDropDownTemplate_AddPage(PaperType paperType, Orientation orientation)
        {
            AddNewPage(paperType, orientation);
            DrawingCanvas.Change = true;
        }

        private void SimpleFormDropDown_OnSimpleFormDropDownChanged(ShapeType shapeType)
        {
            ApplyToAllCanvas((DrawingCanvas dc) =>
            {
                dc.SetFormMode(shapeType);
            });
        }

        private void PlotDropDownTemplate_OnPlotModeChanged(PlotMode plotMode)
        {
            ApplyToAllCanvas((DrawingCanvas dc) =>
            {
                this.plotMode = plotMode;
                dc.SetPlotMode(plotMode);
            });
        }

        private void SelectDropDownTemplate_OnColorAndSizeChanged(System.Windows.Media.Color? c, int size)
        {
            // If strokes and child elements are selected change their colors
            List<Action> changedActions = new List<Action>();

            foreach (Stroke st in DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes())
            {
                DrawingAttributes old = st.DrawingAttributes.Clone();
                if (c != null && c.HasValue)
                    st.DrawingAttributes.Color = c.Value;

                if (size != -1)
                {
                    var selectedSize = Documents.UI.Consts.StrokeSizes[size];
                    st.DrawingAttributes.Width = selectedSize.Width;
                    st.DrawingAttributes.Height = selectedSize.Height;
                }

                DrawingAttributes newAttr = st.DrawingAttributes.Clone();
                changedActions.Add(new StrokesChangedAction(st, old, newAttr));
            }

            // Add line/shape actions
            foreach (UIElement element in DrawingCanvas.LastModifiedCanvas.GetSelectedElements())
            {
                if (element is Shape sh)
                {
                    double oldSize = sh.StrokeThickness;
                    double newSize = (size >= 0 ? Documents.UI.Consts.StrokeSizes[size].Width : oldSize);
                    System.Windows.Media.Color oldColor = (sh.Stroke as SolidColorBrush).Color;
                    System.Windows.Media.Color newColor = (c != null && c.HasValue ? c.Value : oldColor);

                    changedActions.Add(new ShapeChangedAction(sh, oldColor, newColor, oldSize, newSize));
                    if (c != null && c.HasValue)
                        sh.Stroke = new SolidColorBrush(c.Value);
                    if (size >= 0)
                        sh.StrokeThickness = Documents.UI.Consts.StrokeSizes[size].Width;
                }
                else if (element is Plot plot)
                {
                    double oldSize = plot.StrokeThickness;
                    double newSize = (size >= 0 ? Documents.UI.Consts.StrokeSizes[size].Width : oldSize);
                    System.Windows.Media.Color oldColor = plot.Foreground;
                    System.Windows.Media.Color newColor = (c != null && c.HasValue ? c.Value : oldColor);

                    changedActions.Add(new ShapeChangedAction(plot, plot.Foreground, newColor, oldSize, newSize));

                    if (c != null && c.HasValue)
                        plot.Foreground = c.Value;
                    if (size >= 0)
                        plot.StrokeThickness = Math.Round((Documents.UI.Consts.StrokeSizes[size].Width + Documents.UI.Consts.StrokeSizes[size].Height) / 2.0);

                    plot.RenderPlot();
                }
            }

            DrawingCanvas.LastModifiedCanvas.Manager.AddSpecialAction<Action>(changedActions);
            DrawingCanvas.Change = true;
            RefreshSideBar();
        }

        private void RulerDropDownTemplate_OnChangedRulerMode(RulerMode mode)
        {
            ApplyToAllCanvas((DrawingCanvas dc) =>
            {
                dc.SetRulerMode(mode);
            });

            Settings.Instance.RulerStrokeMode = mode;
            Settings.Instance.Save();
        }

        private void RefreshPages()
        {
            cmbPages.Items.Clear();

            int counter = 1;
            foreach (var page in CurrentJournalPages)
                cmbPages.Items.Add(new TextBlock() { Text = $"{Properties.Resources.strPage} {counter++}", HorizontalAlignment = HorizontalAlignment.Center });

            // Select page which could be selected
            int pIndex = CalculateCurrentPageIndexOnScrollPosition();
            preventPageBoxSelectionChanged = true;
            cmbPages.SelectedIndex = pIndex;
            preventPageBoxSelectionChanged = false;
        }

        private int CalculateCurrentPageIndexOnScrollPosition()
        {
            // Do not divide by zero!
            if (mainScrollView.ScrollableHeight == 0)
                return 0;

            double totalHeight = mainScrollView.ExtentHeight;
            double scrollPercentage = mainScrollView.VerticalOffset / mainScrollView.ScrollableHeight;
            double result = ((totalHeight * scrollPercentage) / totalHeight) * CurrentPages;

            return (int)result;
        }

        private void ScrollToPage(int pTarget)
        {
            double cumulatedHeight = 0;

            // Cumulate size height foreach page (because each page can have a different height due to landscape/portrait)
            for (int i = 0; i < pTarget; i++)
                cumulatedHeight += CurrentJournalPages[i].Canvas.ActualHeight * currentScaleFactor;

            // Add space
            cumulatedHeight += ((pTarget - 1) * Consts.SpaceBetweenPages * currentScaleFactor);

            double resultOffset = (pTarget == 0 ? 0 : cumulatedHeight);

            if (pTarget != 0)
                mainScrollView.ScrollToVerticalOffset(resultOffset + (Consts.SpaceBetweenPages * currentScaleFactor));
            else
                mainScrollView.ScrollToVerticalOffset(0.0);
        }

        private void RefreshVerticalScrollbarSize()
        {
            ScrollViewer scrollViewer = mainScrollView;
            scrollViewer.ApplyTemplate();
            ScrollBar scrollBar = (ScrollBar)scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer);
            scrollBar.Width = (Settings.Instance.EnlargeScrollbar ? Consts.ScrollBarExtendedWidth : Consts.ScrollBarDefaultWidth);
        }

        private UserControl GeneratePage(PaperType? paperType = null, byte[] background = null, Orientation orientation = Orientation.Portrait)
        {
            UserControl pageContent = null;
            PaperType paperPattern = Settings.Instance.PaperType;

            if (paperType.HasValue)
                paperPattern = paperType.Value;

            switch (paperPattern)
            {
                case PaperType.Blanco: pageContent = new Blanco(orientation); break;
                case PaperType.Chequered: pageContent = new Chequered(orientation); break;
                case PaperType.Ruled: pageContent = new Ruled(orientation); break;
                case PaperType.Dotted: pageContent = new Dotted(orientation); break;
                case PaperType.Custom: pageContent = new Custom(background, orientation); break;
            }

            IPaper page = pageContent as IPaper;
            // Apply properties and events to the new canvas
            page.Canvas.EditingMode = currentkInkMode;
            page.Canvas.DefaultDrawingAttributes = CurrentDrawingAttributes;
            page.Canvas.OnCopyPositionIsKnown += Canvas_OnCopyPositionIsKnown;
            page.Canvas.OnInsertPositionIsKnown += Canvas_OnInsertPositionIsKnown;
            page.Canvas.SelectionChanged += Canvas_SelectionChanged;
            page.Canvas.Children.CollectionChanged += Children_CollectionChanged;
            page.Canvas.RemoveElementFromSidebar += delegate (List<UIElement> temp)
            {
                List<CustomListBoxItem> toRemove = new List<CustomListBoxItem>();
                foreach (CustomListBoxItem item in pnlItems.Items)
                {
                    if (temp.Contains(item.AssociativeRelation))
                    {
                        toRemove.Add(item);
                    }
                }
                foreach (CustomListBoxItem item in toRemove)
                    pnlItems.Items.Remove(item);
            };

            if (waitingForClickToPaste)
                page.Canvas.SetCopyMode();

            if (insertClipboard != null)
                page.Canvas.SetInsertMode(); // Something to insert

            if (currentTool == Tools.Ruler)
                page.Canvas.SetRulerMode(Settings.Instance.RulerStrokeMode);
            else if (currentTool == Tools.FreeHandPolygon)
                page.Canvas.SetFreeHandPolygonMode(polygonDropDownTemplate);
            else if (currentTool == Tools.Form)
                page.Canvas.SetFormMode(simpleFormDropDown.ShapeType);
            else if (currentTool == Tools.CooardinateSystem)
                page.Canvas.SetPlotMode(plotMode);
            else if (currentTool == Tools.TextMarker)
                page.Canvas.DefaultDrawingAttributes = currentTextMarkerAttributes;

            // Also apply rubber apperance to new page
            else if (currentTool == Tools.RubberFree || currentTool == Tools.RubberStrokes)
                ApplyRubberSizeAndShape(page.Canvas);
            else if (currentTool == Tools.Recognization)
                page.Canvas.SetFreeHandDrawingMode();

            return pageContent;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TextObjectCounter.Text = DrawingCanvas.LastModifiedCanvas.Children.Count.ToString();
        }

        private DrawingCanvas AddPage(UserControl page, int index = -1)
        {
            var elementToAdd = page;
            var paper = page as IPaper;

            if (index == -1)
            {
                CurrentJournalPages.Add(paper);
                pages.Children.Add(elementToAdd);
            }
            else
                pages.Children.Insert(index, elementToAdd);

            double offset = mainScrollView.VerticalOffset;


            PageSplitter pageSplitter = new PageSplitter();
            pageSplitter.OnPageAdded += delegate (PageSplitter owner, PaperType type, Orientation orientation)
            {
                var newPage = GeneratePage(type, orientation: orientation);
                int pageIndex = pages.Children.IndexOf(elementToAdd);

                // Adjust page index
                if (pageIndex != -1)
                    pageIndex += 2;

                AddPage(newPage, pageIndex);
                CurrentJournalPages.Insert(CurrentJournalPages.IndexOf(paper) + 1, newPage as IPaper);

                RefreshPages();
                DrawingCanvas.Change = true;
            };

            if (index == -1)
                pages.Children.Add(pageSplitter);
            else
                pages.Children.Insert(index + 1, pageSplitter);

            mainScrollView.ScrollToVerticalOffset(offset);

            if (index == -1)
                RefreshPages();

            paper.Border = pageSplitter;
            return paper.Canvas;
        }

        private void UpdateTitle(string journal, bool change = false)
        {
            if (string.IsNullOrEmpty(journal))
                journal = currentJournalName;
            else
                currentJournalName = journal;

            if (string.IsNullOrEmpty(journal))
                journal = Properties.Resources.strNewJournal;

            currentJournalTitle = journal;
            Title = $"SimpleJournal - {journal}";
            if (change)
                Title += " *";
        }

        public void UpdateGlowingBrush()
        {
            if (Settings.Instance.ActivateGlowingBrush)
                GlowBrush = new SolidColorBrush((System.Windows.Media.Color)FindResource("Fluent.Ribbon.Colors.AccentColor60"));
            else
                GlowBrush = null;

            NonActiveBorderBrush = GlowBrush;
        }

        // Muste be public for accessing via singleton from the settings
        public void UpdatePenButtons(Pen[] pens = null, bool reset = false)
        {
            if (reset)
            {
                // Initalize pens
                var defaultSize = Documents.UI.Consts.StrokeSizes[0];
                for (int i = 0; i < currentPens.Length; i++)
                    currentPens[i] = new Pen(Consts.PEN_COLORS[i], defaultSize.Width, defaultSize.Height);

                // Initalize text marker (reset)
                UpdateTextMarkerAttributes(true);
            }

            // Save applied values to make sure they are persisted
            if (pens == null)
                Pen.Instance = currentPens;
            else
            {
                currentPens = pens;
                Pen.Instance = pens;
            }

            Pen.Save();

            // Also load and refresh pens
            for (int i = 0; i < Consts.AMOUNT_PENS; i++)
                penTemplates[i].LoadPen(currentPens[i]);

            // Refresh pathes displayed in the menu
            Path[] pathes = new Path[] { pathPen1, pathPen2, pathPen3, pathPen4 };
            for (int i = 0; i < currentPens.Length; i++)
            {
                Path currentPath = pathes[i];

                currentPath.Fill = new SolidColorBrush(currentPens[i].FontColor.ToColor());
                currentPath.Stroke = Brushes.Black;
                currentPath.StrokeThickness = 0.4;
            }

            // Also if currentTool is a selected pen or the text-marker the DrawingAttributes for all canvas needs to be updated!!!!
            if (currentTool is Tools.Pencil1 or Tools.Pencil2 or Tools.Pencil3 or Tools.Pencil4)
            {
                var pen = Pen.Instance[(int)currentTool - 1];
                CurrentDrawingAttributes.Color = pen.FontColor.ToColor();
                CurrentDrawingAttributes.Width = pen.Width;
                CurrentDrawingAttributes.Height = pen.Height;
                ApplyToAllCanvas(p => p.DefaultDrawingAttributes = CurrentDrawingAttributes);

                UpdateDropDownButtons();

                if (reset)
                {
                    // In this case pen bar was resetted so we need to apply tool again to force canvas to apply to default pen
                    SwitchTool(Tools.Pencil1, true);
                }
            }
        }

        private void UpdateTextMarker()
        {
            markerPath.Fill = new SolidColorBrush(currentTextMarkerAttributes.Color);
            markerPath.Stroke = Brushes.Black;
            markerPath.StrokeThickness = 0.4;
        }

        private void RefreshSizeBar()
        {
            if (SelectedPen == -1)
                return;

            isInitalized = false;
            isInitalized = true;
        }

        private void RefreshSideBar()
        {
            DrawingCanvas_ChildElementsSelected(DrawingCanvas.LastModifiedCanvas.GetSelectedElements().ToArray());

            try
            {
                if (DrawingCanvas.LastModifiedCanvas.GetSelectedElements().Omit(typeof(Line)).ToList().Count == 1 && DrawingCanvas.LastModifiedCanvas.GetSelectedElements().Omit(typeof(Line)).ToList()[0] is Shape sh)
                    objShapeSettings.borderColorPicker.SelectedColor = (sh.Stroke as SolidColorBrush).Color;
            }
            catch
            {
                // just to be sure
            }
        }

        private void ToggleFullscreen()
        {
            if (!isFullScreen)
            {
                left = this.Left;
                top = this.Top;
                width = this.Width;
                height = this.Height;

                wasMaximized = this.WindowState == WindowState.Maximized;

                // Make full screeen
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }
            else
            {
                this.WindowState = wasMaximized ? WindowState.Maximized : WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                // Restore
                this.Left = left;
                this.Top = top;
                this.Width = width;
                this.Height = height;
            }

            isFullScreen = !isFullScreen;
        }

        private void ApplyToAllCanvas(Action<DrawingCanvas> action)
        {
            foreach (IPaper paper in CurrentJournalPages)
                action.Invoke(paper.Canvas);
        }

        private void SetStateForToggleButton<T>(T bt, Tools tool) where T : UIElement
        {
            ignoreToggleButtonHandling = true;

            foreach (Fluent.ToggleButton b in toggleButtons)
                b.IsChecked = false;

            foreach (DropDownToggleButton dtb in dropDownToggleButtons)
                dtb.IsChecked = false;

            if (typeof(T) == typeof(Fluent.ToggleButton))
                ((Fluent.ToggleButton)(object)bt).IsChecked = true;
            else if (typeof(T) == typeof(DropDownToggleButton))
                ((DropDownToggleButton)(object)bt).IsChecked = true;

            ignoreToggleButtonHandling = false;

            // One pencil must be active (except for rubber and text marker)
            if ((tool == Tools.RubberFree) || (tool == Tools.RubberStrokes) || (tool == Tools.TextMarker))
                return;

            if (tool == Tools.Pencil1)
            {
                //btnPen1.IsChecked = true;
                lastSelectedPencil = tool;
            }
            else if (tool == Tools.Pencil2)
            {
                //btnPen2.IsChecked = true;
                lastSelectedPencil = tool;
            }
            else if (tool == Tools.Pencil3)
            {
                //btnPen3.IsChecked = true;
                lastSelectedPencil = tool;
            }
            else if (tool == Tools.Pencil4)
            {
                //btnPen4.IsChecked = true;
                lastSelectedPencil = tool;
            }
            else
            {
                // Select lastSelectedPencil
                SelectPen(lastSelectedPencil);
            }
        }

        private void SelectPen(Tools pen)
        {
            if (pen == Tools.Pencil1 || pen == Tools.Pencil2 || pen == Tools.Pencil3 || pen == Tools.Pencil4)
                UpdatePenButtons();
        }

        private void DeselectRuler()
        {
            if (rulerWasSelected)
            {
                rulerWasSelected = false;

                Action<InkCanvas> setEditMode = new Action<InkCanvas>((InkCanvas target) =>
                {
                    target.EditingMode = old;
                    (target as DrawingCanvas).UnsetRulerMode();
                });
                ApplyToAllCanvas(setEditMode);
                currentkInkMode = old;
            }
        }

        private void ApplyRubberSizeAndShape(DrawingCanvas canvas)
        {
            int index = rubberSizeIndex;
            bool rectangle = rubberIsRectangle;

            var result = Documents.UI.Consts.RubberSizes[index];
            StylusShape shape;
            if (rectangle)
                shape = new RectangleStylusShape(result.Width, result.Height);
            else
                shape = new EllipseStylusShape(result.Width, result.Height);

            canvas.EraserShape = shape;
            var before = canvas.EditingMode;
            canvas.EditingMode = InkCanvasEditingMode.Ink;
            canvas.EditingMode = before;
        }

        private void UpdateMenu()
        {
            if (Settings.Instance.UseNewMenu)
            {
                MenuBackstage.Visibility = Visibility.Visible;
                MenuApplication.Visibility = Visibility.Collapsed;
            }
            else
            {
                MenuBackstage.Visibility = Visibility.Collapsed;
                MenuApplication.Visibility = Visibility.Visible;
            }
        }

        public void ApplySettings()
        {
            isInitalized = false;

            UpdateMenu();
            RefreshVerticalScrollbarSize();
            CurrentDrawingAttributes.IgnorePressure = !Settings.Instance.UsePreasure;
            CurrentDrawingAttributes.FitToCurve = Settings.Instance.UseFitToCurve;

            UpdatePenButtons();

            // Refresh Text marker
            UpdateTextMarkerAttributes();

            // Refresh recently openend documents
            RefreshRecentlyOpenedFiles();

            // Refresh notifcationbar button
            UpdateNotificationToolBarAndButton();

            // Refresh ruler mode
            rulerDropDownTemplate.lstBoxChooseRulerMode.SelectedIndex = (int)(Settings.Instance.RulerStrokeMode);

            if (CurrentJournalPages.Count == 1 && CurrentJournalPages[0].Canvas.Strokes.Count == 0 && CurrentJournalPages[0].Canvas.Children.Count == 0)
            {
                // Switch to new format
                var newPage = GeneratePage();
                CurrentJournalPages.Clear();
                this.pages.Children.Clear();
                DrawingCanvas.LastModifiedCanvas = AddPage(newPage);
                DrawingCanvas.Change = false;
            }

            isInitalized = true;
        }
#endregion

        #region Toolbar Handling / Private Event Handling

        #region Tool Handling

        private InkCanvasEditingMode ConvertTool(Tools tool)
        {
            switch (tool)
            {
                case Tools.TextMarker:
                case Tools.Recognization:
                case Tools.Pencil1:
                case Tools.Pencil2:
                case Tools.Pencil3:
                case Tools.Pencil4:
                    currentkInkMode = InkCanvasEditingMode.Ink; break;
                case Tools.RubberFree: currentkInkMode = InkCanvasEditingMode.EraseByPoint; break;
                case Tools.RubberStrokes: currentkInkMode = InkCanvasEditingMode.EraseByStroke; break;
                case Tools.Ruler: currentkInkMode = InkCanvasEditingMode.None; break;
                case Tools.Select: currentkInkMode = InkCanvasEditingMode.Select; break;
                case Tools.Form: currentkInkMode = InkCanvasEditingMode.None; break;
                case Tools.CooardinateSystem: currentkInkMode = InkCanvasEditingMode.None; break;
            }
            return currentkInkMode;
        }

        public void SwitchTool(Tools currentTool, bool force = false)
        {
            // Nothing to do here
            if (currentTool == this.currentTool && !force)
                return;

            Tools oldTool = this.currentTool;
            InkCanvasEditingMode currentMode = ConvertTool(currentTool);
            this.currentTool = currentTool;

            // Disapply old tools 
            if (oldTool == Tools.Ruler && currentTool != Tools.Ruler)
            {
                rulerWasSelected = false;

                Action<DrawingCanvas> setEditMode = new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.EditingMode = currentMode;
                    target.UnsetRulerMode();
                });
                ApplyToAllCanvas(setEditMode);
            }
            else if (oldTool == Tools.FreeHandPolygon && currentTool != Tools.FreeHandPolygon)
            {
                Action<DrawingCanvas> setEditMode = new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.EditingMode = currentMode;
                    target.UnsetFreeHandPolygonMode();
                });
                ApplyToAllCanvas(setEditMode);

                btnBack.IsEnabled = btnForward.IsEnabled = true;
            }
            else if (oldTool == Tools.TextMarker)
            {
                // Restore old attributes
                CurrentDrawingAttributes.IsHighlighter = false;

                Action<DrawingCanvas> setEditMode = new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.EditingMode = currentMode;
                    target.DefaultDrawingAttributes = CurrentDrawingAttributes;
                });
                ApplyToAllCanvas(setEditMode);

                UpdateTextMarker();
            }
            else if (oldTool == Tools.Recognization)
            {
                Action<DrawingCanvas> setEditMode = new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.EditingMode = currentMode;
                    target.UnsetFreeHandMode();
                });
                ApplyToAllCanvas(setEditMode);
            }
            else if (oldTool == Tools.Form)
            {
                Action<DrawingCanvas> setEditMode = new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.EditingMode = currentMode;
                    target.UnsetFormMode();
                });
                ApplyToAllCanvas(setEditMode);
            }
            else if (oldTool == Tools.CooardinateSystem)
            {
                Action<DrawingCanvas> setEditMode = new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.EditingMode = currentMode;
                    target.UnsetPlotMode();
                });
                ApplyToAllCanvas(setEditMode);
            }

            // Apply new tools
            this.btnRemoveSelectedStrokes.IsEnabled = false;
            btnCopy.IsEnabled = false;

            if (currentTool != Tools.TextMarker)
            {
                if (currentTool == Tools.Select)
                {
                    this.btnRemoveSelectedStrokes.IsEnabled = true;
                    btnCopy.IsEnabled = true;
                }

                // If pencil was selected apply values to toolbar
                if (currentTool == Tools.Pencil1 || currentTool == Tools.Pencil2 || currentTool == Tools.Pencil3 || currentTool == Tools.Pencil4)
                {
                    int index = SelectedPen;
                    var currentPen = currentPens[index];

                    // Disable controls reactions because of changing these values
                    RefreshSizeBar();
                    UpdatePenButtons();
                    UpdateTextMarker();
                }
            }
            else if (currentTool == Tools.TextMarker)
            {
                UpdatePenButtons();
                UpdateTextMarker();
            }

            Action<DrawingCanvas> apply = new Action<DrawingCanvas>((DrawingCanvas target) =>
            {
                target.EditingMode = currentMode;

                if (currentTool == Tools.Recognization)
                    target.SetFreeHandDrawingMode();
                else if (currentTool == Tools.TextMarker)
                    target.DefaultDrawingAttributes = currentTextMarkerAttributes;
                else if (currentTool == Tools.Ruler)
                    target.SetRulerMode(Settings.Instance.RulerStrokeMode);
                else if (currentTool == Tools.FreeHandPolygon)
                    target.SetFreeHandPolygonMode(polygonDropDownTemplate);
                else if (currentTool == Tools.Form)
                    target.SetFormMode();
                else if (currentTool == Tools.CooardinateSystem)
                    target.SetPlotMode(plotMode);
                else if (currentTool == Tools.RubberStrokes)
                {
                    // Reset rubber size
                    var can = new InkCanvas();
                    target.EraserShape = new RectangleStylusShape(can.EraserShape.Width, can.EraserShape.Height);
                }

                if (SelectedPen != -1)
                {
                    // Apply 
                    CurrentDrawingAttributes.Color = currentPens[SelectedPen].FontColor.ToColor();
                    CurrentDrawingAttributes.Width = currentPens[SelectedPen].Width;
                    CurrentDrawingAttributes.Height = currentPens[SelectedPen].Height;
                }

            });
            ApplyToAllCanvas(apply);
        }

        private void ChangePenValues(int index, System.Windows.Media.Color? c, int sizeIndex)
        {
            if (c.HasValue)
            {
                CurrentDrawingAttributes.Color = c.Value;

                if (SelectedPen != -1)
                    currentPens[SelectedPen].FontColor = new Common.Data.Color(c.Value.A, c.Value.R, c.Value.G, c.Value.B);
                else
                    currentPens[index].FontColor = new Common.Data.Color(c.Value.A, c.Value.R, c.Value.G, c.Value.B);

                rulerDropDownTemplate.SetColor(c.Value);
            }

            if (sizeIndex >= 0)
            {
                var selectedSize = Documents.UI.Consts.StrokeSizes[sizeIndex];
                currentPens[index].Width = selectedSize.Width;
                currentPens[index].Height = selectedSize.Height;
                CurrentDrawingAttributes.Width = currentPens[index].Width;
                CurrentDrawingAttributes.Height = currentPens[index].Height;

                ApplyToAllCanvas(new Action<InkCanvas>((InkCanvas canvas) =>
                {
                    canvas.DefaultDrawingAttributes = CurrentDrawingAttributes;
                }));
            }

            UpdatePenButtons();
        }

        private void btnPen1_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling || !isInitalized)
                return;

            SetStateForToggleButton(sender as DropDownToggleButton, Tools.Pencil1);
            rulerDropDownTemplate.SetColor(currentPens[0].FontColor.ToColor());
            SwitchTool(Tools.Pencil1);
        }

        private void btnPen2_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling || !isInitalized)
                return;

            SetStateForToggleButton(sender as DropDownToggleButton, Tools.Pencil2);
            rulerDropDownTemplate.SetColor(currentPens[1].FontColor.ToColor());
            SwitchTool(Tools.Pencil2);
        }

        private void btnPen3_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling || !isInitalized)
                return;

            SetStateForToggleButton(sender as DropDownToggleButton, Tools.Pencil3);
            rulerDropDownTemplate.SetColor(currentPens[2].FontColor.ToColor());
            SwitchTool(Tools.Pencil3);
        }

        private void btnPen4_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling || !isInitalized)
                return;

            SetStateForToggleButton(sender as DropDownToggleButton, Tools.Pencil4);
            rulerDropDownTemplate.SetColor(currentPens[3].FontColor.ToColor());
            SwitchTool(Tools.Pencil4);
        }

        private void BtnPen1_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(0, c, sizeIndex);
        }

        private void BtnPen2_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(1, c, sizeIndex);
        }

        private void BtnPen3_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(2, c, sizeIndex);
        }

        private void BtnPen4_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(3, c, sizeIndex);
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnSelect, Tools.Select);
            SwitchTool(Tools.Select);
        }

        private void btnRuler_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnRuler, Tools.Ruler);
            SwitchTool(Tools.Ruler);
        }

        private void BtnFreeHandPolygon_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnFreeHandPolygon, Tools.FreeHandPolygon);
            SwitchTool(Tools.FreeHandPolygon);
        }

        private void btnRecogniziation_Click(object sender, RoutedEventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnRecogniziation, Tools.Recognization);
            SwitchTool(Tools.Recognization);
        }

        private void btnTextMarker_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton<DropDownToggleButton>(btnTextMarker, Tools.TextMarker);
            SwitchTool(Tools.TextMarker);
        }

        private void BtnTextMarker_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            if (sizeIndex != -1)
            {
                var textMakerResult = Documents.UI.Consts.TextMarkerSizes[sizeIndex];
                Settings.Instance.TextMarkerSize = textMakerResult;
                Settings.Instance.Save();
                currentTextMarkerAttributes.Width = textMakerResult.Height;
                currentTextMarkerAttributes.Height = textMakerResult.Width;
            }

            if (c != null)
            {
                Settings.Instance.TextMarkerColor = c.Value.ToColor();
                Settings.Instance.Save();
                currentTextMarkerAttributes.Color = c.Value;
            }

            if (c != null || sizeIndex != -1)
            {
                ApplyToAllCanvas(new Action<InkCanvas>((InkCanvas canvas) =>
                {
                    canvas.DefaultDrawingAttributes = currentTextMarkerAttributes;
                }));

                UpdateTextMarker();
            }
        }

        private void BtnRubberFine_OnChangedRubber(int sizeIndex, bool? rectangle)
        {
            if (sizeIndex != -1)
                rubberSizeIndex = sizeIndex;

            if (rectangle.HasValue)
                rubberIsRectangle = rectangle.Value;

            if (sizeIndex >= 0 || rectangle.HasValue)
            {
                ApplyToAllCanvas((DrawingCanvas dc) =>
                {
                    ApplyRubberSizeAndShape(dc);
                });
            }
        }

        private void btnRubberGrob_Click(object sender, RoutedEventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnRubberGrob, Tools.RubberStrokes);
            SwitchTool(Tools.RubberStrokes);
        }

        private void btnRubberFine_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnRubberFine, Tools.RubberFree);
            SwitchTool(Tools.RubberFree);
        }

        private void btnInsertSimpleForm_Click_1(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnInsertSimpleForm, Tools.Form);
            SwitchTool(Tools.Form);
        }

        private void btnCooardinateSystem_Click(object sender, EventArgs e)
        {
            if (ignoreToggleButtonHandling)
                return;

            SetStateForToggleButton(btnInsertPlot, Tools.CooardinateSystem);
            SwitchTool(Tools.CooardinateSystem);
        }


#endregion

        #region Event Handling / Menu

        #region Commands

        private void DisableTouchScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // TouchHelper.SetTouchState(false);
        }

        private async void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await SaveProject(false);
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void UndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingCanvas.LastModifiedCanvas.Undo();
        }

        private void UndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DrawingCanvas.LastModifiedCanvas == null)
                e.CanExecute = false;
            else
                e.CanExecute = DrawingCanvas.LastModifiedCanvas.CanUndo();
        }

        private void RedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DrawingCanvas.LastModifiedCanvas.Redo();
        }

        private void RedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DrawingCanvas.LastModifiedCanvas == null)
                e.CanExecute = false;
            else
                e.CanExecute = DrawingCanvas.LastModifiedCanvas.CanRedo();
        }

        private async void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Print();
        }

        private void PrintCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool run;
            if (DrawingCanvas.Change)
            {
                var result = MessageBox.Show(this, Properties.Resources.strWantToCreateNewJournal, Properties.Resources.strWantToLoadNewJournalTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                run = (result == MessageBoxResult.Yes);
            }
            else
                run = true;

            if (run)
            {
                // No journal is loaded currently
                pages.Children.Clear();
                CurrentJournalPages.Clear();
                pnlSidebar.Visibility = Visibility.Hidden;

                var page = GeneratePage();
                AddPage(page);

                // Set last modified canvas to this, because the old is non existing any more
                DrawingCanvas.LastModifiedCanvas = (page as IPaper).Canvas as DrawingCanvas;
                SwitchTool(currentTool, true);

                UpdateTitle(Properties.Resources.strNewJournal);
                currentJournalPath = string.Empty;
            }
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertImageFromFile();
        }

        private void InsertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertFromClipboardCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = System.Windows.Forms.Clipboard.ContainsImage();
        }

        private void InsertFromClipboardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InsertFromClipboard();
        }

#endregion

        private void Backstage_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MenuBackstage.IsOpen)
                MenuBackstageTabControl.SelectedIndex = 0;
        }

        private void BackstageTabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ApplicationCommands.New.Execute(null, null);
            MenuBackstage.IsOpen = false;
        }

        private void ImageCloseStatusBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainStatusBar.Visibility = Visibility.Collapsed;
        }

        private void btnFeedback_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog ad = new AboutDialog();
            ad.ShowFeedbackPage().ShowDialog();
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        private async void ListRecentlyOpenedDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListRecentlyOpenedDocuments.SelectedItem is Document d)
            {
                // Check if file exists
                if (!System.IO.File.Exists(d.Path))
                {
                    if (MessageBox.Show(this, Properties.Resources.strSelectedDocumentNotExisting, Properties.Resources.strSelectedDocumentNotExistingTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        RecentlyOpenedDocuments.Remove(d.Path);
                }
                else if (AskForOpeningAfterModifying())
                    await LoadJournal(d.Path);

                ListRecentlyOpenedDocuments.SelectedItem = null;
            }
        }

        private async void RecentlyOpenedDocumentsBackstage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecentlyOpenedDocumentsBackstage.SelectedItem is Document d)
            {
                // Check if file exists
                if (!System.IO.File.Exists(d.Path))
                {
                    if (MessageBox.Show(this, Properties.Resources.strSelectedDocumentNotExisting, Properties.Resources.strSelectedDocumentNotExistingTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        RecentlyOpenedDocuments.Remove(d.Path);
                }
                else if (AskForOpeningAfterModifying())
                {
                    MenuBackstage.IsOpen = false;
                    await Task.Delay(500).ContinueWith(delegate (Task t)
                    {
                        Dispatcher.Invoke(new System.Action(async () =>
                        {
                            await LoadJournal(d.Path);
                        }));
                    });
                }

                RecentlyOpenedDocumentsBackstage.SelectedItem = null;
                MenuBackstage.IsOpen = false;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized && !preventPageBoxSelectionChanged && cmbPages.SelectedIndex != -1)
            {
                int pTarget = cmbPages.SelectedIndex;
                ScrollToPage(pTarget);
            }
        }

        private void ButtonPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            int index = cmbPages.SelectedIndex - 1;
            if (index < 0)
                index = CurrentPages - 1;

            ScrollToPage(index);
        }

        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {
            int index = (cmbPages.SelectedIndex + 1) % CurrentPages;
            ScrollToPage(index);
        }

        private async Task<bool> SaveProject(bool forceNewPath)
        {
            bool resultSaving = false;

            if (string.IsNullOrEmpty(currentJournalPath) || forceNewPath)
            {
                SaveFileDialog dialog = new SaveFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal", Title = Properties.Resources.strSave };
                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    resultSaving = await SaveJournal(dialog.FileName);
                    UpdateTitle(System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));
                }
            }
            else
                resultSaving = await SaveJournal(currentJournalPath);

            return resultSaving;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            await SaveProject(true);
        }

        /// <summary>
        /// This button is currently hidden, because using a pdf printer results in a better quality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuButtonBackstageExportPdf_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filter = Properties.Resources.strPDFFilter, Title = Properties.Resources.strPDFDialogTitle };
            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                List<byte[]> data = new List<byte[]>();

                foreach (var page in CurrentJournalPages)
                {
                    // BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    PngBitmapEncoder encoder = new PngBitmapEncoder();

                    RenderTargetBitmap rtb = GeneralHelper.RenderToBitmap(page as UserControl, 1.0, new SolidColorBrush(Colors.White));
                    encoder.Frames.Add(BitmapFrame.Create(rtb));

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        encoder.Save(ms);
                        data.Add(ms.ToArray());
                    }

                    rtb.Clear();
                    rtb = null;
                    encoder.Frames.Clear();
                    encoder = null;
                }

                var result = await PdfHelper.ExportJournalAsPDF(dialog.FileName, data);

                if (!result.Item1)
                    MessageBox.Show($"{Properties.Resources.strFailedToExportJournalAsPDF}\n\n{Properties.Resources.strPDFConversationDialog_GhostscriptMessage}: {result.Item2}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Print()
        {
            /*
            Prevent printing through printQueue instead create a pdf file directly                    
            SaveFileDialog dialog = new SaveFileDialog() { Filter = Properties.Resources.strPDFFilter, Title = Properties.Resources.strPDFDialogTitle };
            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                List<IPaper> pages = new List<IPaper>();
                for (int i = from; i <= to; i++)
                   pages.Add(CurrentJournalPages[i]);
                await PdfHelper.ExportJournalAsPDF(dialog.FileName, pages);
            }

            // Old print method but with orientation
            /*for (int i = from; i <= to; i++)
            {
                var page = CurrentJournalPages[i].ClonePage(true);
                if (page is Custom c && c.Orientation == Orientation.Landscape)
                    pd.PrintTicket.PageOrientation = PageOrientation.Landscape;
                else
                    pd.PrintTicket.PageOrientation = PageOrientation.Portrait;

                pd.PrintVisual((UserControl)page, $"Printing page {i}/{to}");
            }
            */

            PrintDialog pd = new PrintDialog() { MinPage = 1, MaxPage = (uint)CurrentJournalPages.Count, UserPageRangeEnabled = true, SelectedPagesEnabled = false, CurrentPageEnabled = true };
            int from = 0; int to = CurrentJournalPages.Count - 1;
            var result = pd.ShowDialog();

            switch (pd.PageRangeSelection)
            {
                case PageRangeSelection.AllPages:
                    {
                        from = 0;
                        to = CurrentJournalPages.Count - 1;
                    }
                    break;
                case PageRangeSelection.CurrentPage:
                    {
                        from = to = CalculateCurrentPageIndexOnScrollPosition();
                    }
                    break;
                case PageRangeSelection.UserPages:
                    {
                        from = pd.PageRange.PageFrom - 1;
                        to = pd.PageRange.PageTo - 1;
                    }
                    break;
            }

            if (result.HasValue && result.Value)
            {
                // Determine if we need to print a pdf or not
                bool printToFilePDF = (pd.PrintQueue.Name.ToLower().Contains("pdf"));

                if (printToFilePDF && pd.PrintQueue.Name.Contains("Microsoft"))
                {
                    if (MessageBox.Show(Properties.Resources.strMicrosoftPrintToPDFWarning, Properties.Resources.strSure, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return;
                }

                // https://stackoverflow.com/a/10139076/6237448
                // https://stackoverflow.com/questions/8230090/printdialog-with-landscape-and-portrait-pages
                // For later: Find out a way to notify the user when printing is done
                State.SetAction(StateType.Printing, ProgressState.Start);
                await Task.Delay(1);

                try
                {
                    var pageSize = new Size(Documents.Consts.A4WidthP, Documents.Consts.A4HeightP);

                    List<IPaper> pages = new List<IPaper>();

                    for (int i = from; i <= to; i++)
                        pages.Add(CurrentJournalPages[i]);

                    var document = new FixedDocument();
                    XpsDocumentWriter xpsWriter = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                    xpsWriter.WritingCompleted += delegate (object sender, System.Windows.Documents.Serialization.WritingCompletedEventArgs e)
                    {
                        State.SetAction(StateType.Printing, ProgressState.Completed);
                    };

                    foreach (var item in pages)
                    {
                        var ui = item.Canvas;

                        if (item.Orientation == Orientation.Landscape)
                            pageSize = new Size(Documents.Consts.A4WidthL, Documents.Consts.A4HeightP);

                        // Create FixedPage
                        var fixedPage = new FixedPage
                        {
                            Width = pageSize.Width,
                            Height = pageSize.Height
                        };

                        // Add visual, measure/arrange page.
                        fixedPage.Children.Add((UIElement)item.ClonePage(true));
                        fixedPage.Measure(pageSize);
                        fixedPage.Arrange(new Rect(new Point(), pageSize));
                        fixedPage.UpdateLayout();

                        // Add page to document
                        var pageContent = new PageContent();
                        ((IAddChild)pageContent).AddChild(fixedPage);
                        document.Pages.Add(pageContent);

                        await Task.Delay(1);
                    }

                    // Set the job description
                    pd.PrintQueue.CurrentJobSettings.Description = currentJournalName;

                    xpsWriter.WritingPrintTicketRequired += (s, e) =>
                    {
                        var page = pages[e.Sequence - 1];
                        Orientation orientation = page.Orientation;

                        if (page is Custom c)
                            orientation = c.Orientation;

                        e.CurrentPrintTicket = new System.Printing.PrintTicket();
                        if (orientation == Orientation.Landscape)
                            e.CurrentPrintTicket.PageOrientation = PageOrientation.Landscape;
                        else
                            e.CurrentPrintTicket.PageOrientation = PageOrientation.Portrait;
                    };

                    // Use xpsWriter directly instead of pd.PrintDocument - this way, we can print multiple orientations in one printing ticket!!!
                    // pd.PrintDocument(document.DocumentPaginator, "My Document");               
                    xpsWriter.WriteAsync(document);
                }
                catch (Exception ex)
                {
                    State.SetAction(StateType.Printing, ProgressState.Completed);
                    MessageBox.Show($"{Properties.Resources.strPrintingError}: {ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnDisplaySidebar_Click(object sender, RoutedEventArgs e)
        {
            var elements = DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)).ToList();

            if (elements.Count > 0)
            {
                forceOpenSidebar = true;
                DrawingCanvas.LastModifiedCanvas.Select(new UIElement[] { elements.FirstOrDefault() });

                // If the sidebar is opened manually, ensure that the select tool is properly selected!
                SetStateForToggleButton(btnSelect, Tools.Select);
                SwitchTool(Tools.Select, true);
            }
        }

        private void btnCreateForm_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes() != null && DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes().Count > 0 && !DrawingCanvas.LastModifiedCanvas.ConvertSelectedStrokesToShape())
                MessageBox.Show(this, Properties.Resources.strNoShapeRecognized, Properties.Resources.strNoShapeRecognizedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            else
                RefreshSideBar();
        }

        private void btnClearPage_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.LastModifiedCanvas.Strokes != null || DrawingCanvas.LastModifiedCanvas.Children.Count > 0)
            {
                if (MessageBox.Show(this, Properties.Resources.strWantToClearPage, Properties.Resources.strWantToClearPageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (DrawingCanvas.LastModifiedCanvas.Strokes != null && DrawingCanvas.LastModifiedCanvas.Strokes.Count > 0)
                        DrawingCanvas.LastModifiedCanvas.Strokes.Clear();

                    // This is a workaround for clearing childrens because .Clear() doesn't work
                    DrawingCanvas.LastModifiedCanvas.Children.ClearAll(DrawingCanvas.LastModifiedCanvas);
                    DrawingCanvas.LastModifiedCanvas.Select(new StrokeCollection());

                    // But also make sure to reselect old tool
                    SwitchTool(currentTool, true);

                    // Make sidebar invisible, because there are no objects to display, because the page was cleaned
                    if (pnlSidebar.IsVisible)
                        pnlSidebar.Visibility = Visibility.Hidden;

                    // Make sure, that the user will be asked to save if he/she closes SJ
                    DrawingCanvas.Change = true;
                }
            }
        }

        private void BtnInsertNewPage_Click(object sender, EventArgs e)
        {
            AddNewPage(Settings.Instance.PaperTypeLastInserted, Settings.Instance.OrientationLastInserted);
        }

        private bool AskForOpeningAfterModifying()
        {
            bool run = false;
            if (DrawingCanvas.Change)
            {
                var res = MessageBox.Show(this, Properties.Resources.strWantToLoadNewJournal, Properties.Resources.strWantToLoadNewJournalTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    run = true;
            }
            else
                run = true;

            return run;
        }

        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (AskForOpeningAfterModifying())
            {
                OpenFileDialog ofd = new OpenFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal;*.pdf" }; // .journal or pdf
                var result = ofd.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    pnlSidebar.Visibility = Visibility.Hidden;

                    if (System.IO.Path.GetExtension(ofd.FileName).ToLower().Contains("pdf"))
                    {
                        PDFConversationDialog pdfConversationDialog = new PDFConversationDialog(ofd.FileName);
                        bool? res = pdfConversationDialog.ShowDialog();

                        if (res.HasValue && res.Value)
                            await LoadJournal(pdfConversationDialog.DestinationFileName);

                        return;
                    }

                    await LoadJournal(ofd.FileName);
                }
            }
        }

        private async void MenuButtonImportPDF_Click(object sender, RoutedEventArgs e)
        {
            if (AskForOpeningAfterModifying())
            {
                OpenFileDialog ofd = new OpenFileDialog() { Filter = $"{Properties.Resources.strPDFFile}|*.pdf" };
                var result = ofd.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    pnlSidebar.Visibility = Visibility.Hidden;

                    PDFConversationDialog pdfConversationDialog = new PDFConversationDialog(ofd.FileName);
                    bool? res = pdfConversationDialog.ShowDialog();

                    if (res.HasValue && res.Value)
                        await LoadJournal(pdfConversationDialog.DestinationFileName);
                }
            }
        }

        private void btnDeletePage_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPages.Items.Count > 1 && cmbPages.SelectedIndex != -1)
            {
                if (MessageBox.Show(this, Properties.Resources.strWantToDeletePage, Properties.Resources.strWantToDeletePageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // One page must stay there anyway
                    int index = cmbPages.SelectedIndex;

                    var canvasToRemove = CurrentJournalPages[index].Canvas;
                    var borderToRemove = CurrentJournalPages[index].Border;

                    UserControl result = null;
                    foreach (UserControl control in pages.Children.OfType<UserControl>())
                    {
                        if (control is IPaper p && p.Canvas.Equals(canvasToRemove))
                        {
                            result = control;
                            break;
                        }
                    }

                    UIElement[] toRemove = new UIElement[] { result, borderToRemove };
                    foreach (UIElement element in toRemove)
                        pages.Children.Remove(element);

                    CurrentJournalPages.RemoveAt(index);
                    RefreshPages();

                    this.pnlSidebar.Visibility = Visibility.Hidden;
                    DrawingCanvas.Change = true;
                }
            }
            else
            {
                MessageBox.Show(this, Properties.Resources.strJournalNeedsOnePageMinimum, Properties.Resources.strJournalNeedsOnePageMinimumTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnTextDetection_Click(object sender, RoutedEventArgs e)
        {
            if (textAnalyserInstance == null)
            {
                textAnalyserInstance = new TextAnalyser();
                textAnalyserInstance.Closed += TextAnalyserInstance_Closed;
                textAnalyserInstance.Show();
            }
            else
            {
                // Bring window to front
                textAnalyserInstance.Topmost = true;
                textAnalyserInstance.Topmost = false;
                textAnalyserInstance.Focus();
            }

            await textAnalyserInstance.AnalyzePages(CurrentJournalPages.ToList());
        }

        private void TextAnalyserInstance_Closed(object sender, EventArgs e)
        {
            textAnalyserInstance = null;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            TextInputWindow tiw = new TextInputWindow { Title = Properties.Resources.strSearch };
            var cachedResult = tiw.ShowDialog();
            int totalResults = 0;

            if (cachedResult.HasValue && cachedResult.Value)
            {
                foreach (IPaper paper in CurrentJournalPages)
                    totalResults += paper.Canvas.Search(tiw.Result);

                if (totalResults == 0)
                    MessageBox.Show(this, $"{tiw.Result} {Properties.Resources.strTextNotFound}", Properties.Resources.strTextNotFoundTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog settingsWindow = new SettingsDialog() { Owner = this };
            settingsWindow.ShowDialog();

            if (!Settings.Instance.UseAutoSave)
            {
                // Deactive AutoSave Timer
                autoSaveBackupTimer.Stop();
            }
            else
            {
                var newTime = TimeSpan.FromMinutes(Settings.Instance.AutoSaveIntervalMinutes);
                // Restart timer if interval changed
                if (autoSaveBackupTimer.Interval.Ticks != newTime.Ticks)
                {
                    autoSaveBackupTimer.Stop();
                    autoSaveBackupTimer.Interval = newTime;
                    autoSaveBackupTimer.Start();
                }
            }

            RefreshVerticalScrollbarSize();
        }

        private void btnRemoveSelectedStrokes_Click(object sender, RoutedEventArgs e)
        {
            var selectedStrokes = DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes();
            if (selectedStrokes != null && selectedStrokes.Count > 0)
                DrawingCanvas.Change = true;
            DrawingCanvas.LastModifiedCanvas.Strokes.Remove(selectedStrokes);

            var can = DrawingCanvas.LastModifiedCanvas;

            if (can.GetSelectedElements() != null && can.GetSelectedElements().Count > 0)
            {
                var lst = can.GetSelectedElements();
                List<UIElement> tmpList = new List<UIElement>();
                foreach (UIElement element in lst)
                    tmpList.Add(element);

                foreach (UIElement element in tmpList)
                    can.Children.Remove(element);

                // Refresh list
                if (pnlSidebar.IsVisible)
                {
                    pnlSidebar.Visibility = Visibility.Collapsed;
                    if (can.Children.Count > 0)
                        can.Select(new UIElement[] { can.Children[0] });
                }

                // Deleting strokes is also a change!
                DrawingCanvas.Change = true;
            }
        }

#region Exit
        private bool closedButtonWasPressed = false;

        private async void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.Change)
            {
                var result = MessageBox.Show(this, Properties.Resources.strSaveChanges, Properties.Resources.strSaveChangesTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.No)
                {
                    closedButtonWasPressed = true;
                    NotificationService.NotificationServiceInstance?.Stop();
                    Close();
                }
                else if (result == MessageBoxResult.Yes)
                {
                    await SaveProject(false);
                    closedButtonWasPressed = true;
                    NotificationService.NotificationServiceInstance?.Stop();
                    Close();
                }
            }
            else
            {
                closedButtonWasPressed = true;
                NotificationService.NotificationServiceInstance?.Stop();
                Close();
            }
        }

        private bool cancelClosing = false;

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (cancelClosing)
            {
                cancelClosing = false;
                return;
            }

#if !UWP
            bool close = false;
#endif

            if (!closedButtonWasPressed && DrawingCanvas.Change)
            {
                // Create a backup before closing - in case of windows will kill the app if the user will not answer the dialog
                // PROBLEM: CreateBackup take too much time (compressing), so before the backup is finished the app gets closed!
                // https://stackoverflow.com/questions/45310056/wpf-child-form-onclosing-event-and-await/45325710#45325710
                // In general: For large documents the user shouldn't wait till the large backup has finished!

                // So Fire and Forget is the solution here
                // But it needs to be done on the gui/app thread, because gui elements are accessed!
                // https://stackoverflow.com/questions/5613951/simplest-way-to-do-a-fire-and-forget-method-in-c-sharp-4-0/5616348#5616348
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Factory.StartNew(() => Application.Current.Dispatcher.Invoke(() => CreateBackup()));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                // Ask 
                var result = MessageBox.Show(this, Properties.Resources.strSaveChanges, Properties.Resources.strSaveChangesTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
                else if (result == MessageBoxResult.No)
                {
                    NotificationService.NotificationServiceInstance?.Stop();
                    DeleteAutoSaveBackup(true);
                    e.Cancel = false;
#if !UWP
                    close = true;
#endif
                }
                else if (result == MessageBoxResult.Yes)
                {
                    // Ensure that closing is canceld to successfully saving the document, if it's finished "Close()" is
                    // called again, but this event is ignored then and the app gets closed - related to https://stackoverflow.com/a/49013345/6237448
                    e.Cancel = true;
                    cancelClosing = true;

                    await SaveProject(false);
                    DeleteAutoSaveBackup(true);

                    e.Cancel = false;
#if !UWP
                    close = true;
#else
                    GeneralHelper.Shutdown();
#endif
                }
            }
            else
            {
                NotificationService.NotificationServiceInstance?.Stop();
                DeleteAutoSaveBackup(true);
#if !UWP
                close = true;
#endif
            }

#if !UWP
            if (close && Settings.Instance.UseTouchScreenDisabling && ProcessHelper.SimpleJournalProcessCount == 1)
                TouchHelper.SetTouchState(true);
                
            if (close && cancelClosing)
                GeneralHelper.Shutdown();
#endif
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }
#endregion

#endregion

        #endregion

        #region Zoom

        private void ZoomByScale(double scale)
        {
            ScaleTransform lt = new ScaleTransform
            {
                ScaleX = scale,
                ScaleY = scale,
                // CenterY = (mainScrollView.ScrollOffset / mainScrollView.VerticalOffset)
            };

            btnZoom80.IsChecked =
            btnZoom100.IsChecked =
            btnZoom120.IsChecked =
            btnZoom150.IsChecked =
            btnZoom180.IsChecked =
            btnZoom200.IsChecked = false;

            if (scale == 0.8)
                btnZoom80.IsChecked = true;
            else if (scale == 1.0)
                btnZoom100.IsChecked = true;
            else if (scale == 1.2)
                btnZoom120.IsChecked = true;
            else if (scale == 1.5)
                btnZoom150.IsChecked = true;
            else if (scale == 1.8)
                btnZoom180.IsChecked = true;
            else if (scale == 2.0)
                btnZoom200.IsChecked = true;

            pages.LayoutTransform = lt;

            // This is for prevent jumping while switching zoom
            mainScrollView.ScrollToVerticalOffset((mainScrollView.VerticalOffset / currentScaleFactor) * scale);

            currentScaleFactor = scale;

            // Save scale to settings
            Settings.Instance.Zoom = (int)(scale * 100);
            Settings.Instance.Save();
        }

        private void btnZoom80_Click(object sender, RoutedEventArgs e)
        {
            ZoomByScale(0.8);
        }

        private void btnZoom100_Click(object sender, RoutedEventArgs e)
        {
            ZoomByScale(1.0);
        }

        private void btnZoom200_Click(object sender, RoutedEventArgs e)
        {
            ZoomByScale(2.0);
        }

        private void btnZoom120_Click(object sender, RoutedEventArgs e)
        {
            ZoomByScale(1.2);
        }

        private void btnZoom150_Click(object sender, RoutedEventArgs e)
        {
            ZoomByScale(1.5);
        }

        private void btnZoom180_Click(object sender, RoutedEventArgs e)
        {
            ZoomByScale(1.8);
        }
#endregion

        #region Scroll Handling
        private Point p1 = new Point();
        private readonly Stopwatch timer = new Stopwatch();
        private Storyboard sbScrollViewerAnimation = new Storyboard();
        private bool handledMouseUp = false;

        private void Pages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(pages);
            if (!DetermineIfMouseIsInChildIPaper(pages.Children, p))
            {
                timer.Stop();
                timer.Reset();
                timer.Start();

                p1 = p;
                handledMouseUp = true;
            }
        }

        private void Pages_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(pages);
            if (!DetermineIfMouseIsInChildIPaper(pages.Children, p) && p1.X != double.MinValue && p1.Y != double.MinValue)
            {
                double deltaO = Math.Abs(p.Y - p1.Y) * 4;
                handledMouseUp = false;

                if (p.Y > p1.Y)
                    deltaO = -deltaO;

                // Natural scrolling. If enabled just do deltaO *= -1;
                if (Settings.Instance.UseNaturalScrolling)
                    deltaO *= -1;

                ScrollToOffset(deltaO, p1);

                // Reset point
                p1 = new Point(double.MinValue, double.MinValue);
            }
        }

        private void Pages_MouseLeave(object sender, MouseEventArgs e)
        {
            if (handledMouseUp)
                Pages_PreviewMouseUp(sender, new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left));
        }

        private bool DetermineIfMouseIsInChildIPaper(UIElementCollection collection, Point ps)
        {
            foreach (UIElement uIElement in collection)
            {
                // Prevent PageSplitter from triggering scrolling event!
                if (uIElement is PageSplitter prg && prg.BoundsRelativeTo(pages).Contains(ps))
                    return true;

                if (uIElement is UserControl p && p is IPaper ip)
                {
                    var bounds = ip.Canvas.BoundsRelativeTo(pages);
                    if (bounds.Contains(ps))
                        return true;
                }
            }

            return false;
        }

        private void ScrollToOffset(double offset, Point p1)
        {
            if (double.IsInfinity(offset) || p1.Y == 0)
                return;

            DoubleAnimation verticalAnimation = new DoubleAnimation
            {
                From = mainScrollView.VerticalOffset,
                To = mainScrollView.VerticalOffset + offset,
                Duration = new Duration(TimeSpan.FromMilliseconds(400))
            };

            // Debug: Console.WriteLine($"From: {verticalAnimation.From}. To: {verticalAnimation.To}. Delta: {offset}. P1 = ({p1.X},{p1.Y}). P2 = ({p2.X},{p2.Y})");

            sbScrollViewerAnimation = new Storyboard();
            sbScrollViewerAnimation.Children.Add(verticalAnimation);

            Storyboard.SetTarget(verticalAnimation, mainScrollView);
            Storyboard.SetTargetProperty(verticalAnimation, new PropertyPath(AnimatedScrollViewer.ScrollOffsetProperty));
            mainScrollView.BeginStoryboard(sbScrollViewerAnimation);
        }

#endregion

        #region Internal Save and Load
        private async Task<bool> SaveJournal(string path, bool saveAsBackup = false)
        {
            try
            {
                Journal journal = new Journal { ProcessID = ProcessHelper.CurrentProcID };

                if (saveAsBackup)
                {
                    // Claim this journal as a backup - the document needs also a process ID to determine if this is an active backup
                    journal.IsBackup = true;
                    journal.ProcessID = ProcessHelper.CurrentProcID;

                    // Assign original file path (if any)
                    if (!string.IsNullOrEmpty(currentJournalPath))
                        journal.OriginalPath = currentJournalPath;

                    // Don't make a backup when there is nothing to back-up.
                    if (!DrawingCanvas.Change || (CurrentJournalPages.Count == 1 && CurrentJournalPages[0].Canvas.Strokes.Count == 0 && CurrentJournalPages[0].Canvas.Children.Count == 0))
                        return true;
                }

                foreach (IPaper paper in CurrentJournalPages)
                {
                    var currentCanvas = paper.Canvas;

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        currentCanvas.Strokes.Save(ms);

                        JournalPage jp = new JournalPage();
                        if (paper is Custom custom)
                            jp = new PdfJournalPage() { PageBackground = custom.PageBackground };

                        jp.Orientation = paper.Orientation;
                        jp.PaperPattern = paper.Type;
                        jp.Data = ms.ToArray();

                        // Check for additional ressources
                        if (currentCanvas.Children.Count > 0)
                        {
                            foreach (UIElement element in currentCanvas.Children)
                            {
                                var result = element.ConvertFromUIElement();
                                if (result != null)
                                    jp.JournalResources.Add(result);
                            }
                        }
                        journal.Pages.Add(jp);
                    }
                }

                await journal.SaveAsync(path, quiet: saveAsBackup, hideStatus: saveAsBackup);

                if (!saveAsBackup)
                {
                    RecentlyOpenedDocuments.AddDocument(path);
                    RefreshRecentlyOpenedFiles();

                    currentJournalPath = path;
                    DrawingCanvas.Change = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (!saveAsBackup)
                    MessageBox.Show(this, $"{Properties.Resources.strFailedToSaveJournal} {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}", Properties.Resources.strFailedToSaveJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        private async Task LoadJournal(string fileName)
        {
            if (fileName.EndsWith(".journal"))
            {
                var dialog = new WaitingDialog(System.IO.Path.GetFileNameWithoutExtension(fileName)) { Owner = this };
                try
                {
                    if (!System.IO.File.Exists(fileName))
                        throw new Exception(string.Format(Properties.Resources.strFileNotFound, fileName));

                    dialog.Show();
                    Journal currentJournal = await Journal.LoadJournalAsync(fileName, Consts.BackupDirectory);

                    if (currentJournal == null)
                    {
                        dialog.Close();
                        MessageBox.Show(Properties.Resources.strFailedToLoadJournalFromNetwork, Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (currentJournal.IsBackup)
                    {
                        dialog.Close();
                        MessageBox.Show(Properties.Resources.strBackupFileCannotBeOpened, Properties.Resources.strBackupFileCannotBeOpenedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (currentJournal.ProcessID > -1)
                    {
                        // A process id is set.
                        if (ProcessHelper.IsProcessActiveByTaskId(currentJournal.ProcessID))
                        {
                            if (currentJournal.ProcessID != Environment.ProcessId)
                            {
                                MessageBox.Show(Properties.Resources.strJournalIsAlreadyOpened, Properties.Resources.strJournalIsAlreadyOpenedTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                                // Try to focus the instance where it is
                                ProcessHelper.BringProcessToFront(currentJournal.ProcessID);
                                dialog.Close();
                                return;
                            }
                            else
                            {
                                dialog.Close();
                                MessageBox.Show(Properties.Resources.strJournalIsAlreadyOpenedInTheSameWindow, Properties.Resources.strJournalIsAlreadyOpenedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    // Okay: User has decided to load / discard old document => remove the process ID
                    if (!string.IsNullOrEmpty(currentJournalPath))
                    {
                        // If a path is set remove the process id
                        var journal = await Journal.LoadJournalMetaAsync(currentJournalPath);
                        if (journal != null)
                        {
                            journal.ProcessID = -1;
                            await journal.UpdateJournalMetaAsync(currentJournalPath, true);
                            currentJournalPath = string.Empty;
                        }
                    }

                    currentJournalPath = fileName;

                    IsEnabled = false;
                    isInitalized = false;

                    ClearJournal();
                    RecentlyOpenedDocuments.AddDocument(fileName);

                    int pageCount = 0;
                    double progress = 0;

                    foreach (JournalPage jp in currentJournal.Pages)
                    {
                        DrawingCanvas canvas = null;
                        byte[] background = null;
                        Orientation orientation = jp.Orientation;

                        if (jp is PdfJournalPage pdf)
                            background = pdf.PageBackground;

                        canvas = AddPage(GeneratePage(jp.PaperPattern, background, orientation));

                        if (pageCount == 0)
                        {
                            // Set last modified canvas to this, because the old is non existing any more
                            DrawingCanvas.LastModifiedCanvas = canvas;
                        }

                        progress = (pageCount++ + 1) / (double)currentJournal.Pages.Count;
                        dialog.SetProgress(progress, pageCount, currentJournal.Pages.Count);

                        // Delay 1ms to ensure the dialog will be displayed correctly
                        await Task.Delay(1);

                        StrokeCollection strokes = null;
                        await Task.Run(() =>
                        {
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                            {
                                if (jp.Data != null)
                                {
                                    ms.Write(jp.Data, 0, jp.Data.Length);
                                    ms.Position = 0;
                                    strokes = new StrokeCollection(ms);
                                }
                                else
                                    strokes = new StrokeCollection();
                            }
                        }).ContinueWith(new Action<Task>((Task t) =>
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                canvas.Strokes = strokes;

                                if (jp.HasAdditionalResources)
                                {
                                    foreach (JournalResource jr in jp.JournalResources)
                                        UIHelper.AddJournalResourceToCanvas(jr, canvas);
                                }
                            }));
                        }));
                    }

                    // Make sure curerntTool is forced after loading
                    SwitchTool(currentTool, true);

                    // Set title due to new document
                    UpdateTitle(System.IO.Path.GetFileNameWithoutExtension(fileName));

                    // Delete old auto save files
                    DeleteAutoSaveBackup();

                    // Set process id to document and save it to make sure other instances cannot load this journal
                    currentJournal.ProcessID = ProcessHelper.CurrentProcID;

                    await currentJournal.UpdateJournalMetaAsync(fileName, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"{Properties.Resources.strFailedToLoadJournal} {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}", Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsEnabled = true;
                    dialog.Close();
                    isInitalized = true;
                }
            }
            else
            {
                MessageBox.Show(this, Properties.Resources.strWrongFileFormat, Properties.Resources.strWrongFileFormatTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            DrawingCanvas.Change = false;
        }

        private void ClearJournalOld()
        {
            List<IPaper> toClear = new List<IPaper>();
            foreach (IPaper page in CurrentJournalPages)
            {
                page.Canvas.Strokes = new StrokeCollection();
                page.Canvas.Children.ClearAll(page.Canvas);
                toClear.Add(page);
            }
            foreach (IPaper page in toClear)
                CurrentJournalPages.Remove(page);
            pages.Children.Clear();
            CurrentJournalPages.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void ClearJournal()
        {
            List<UIElement> toRemove = new List<UIElement>();

            foreach (var item in pages.Children)
            {
                if (item is IPaper page)
                {
                    page.Dispose();
                    toRemove.Add(item as UIElement);
                }
                else if (item is PageSplitter ps)
                    toRemove.Add(ps);
            }

            foreach (var item in toRemove)
                pages.Children.Remove(item);

            pages.Children.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            CurrentJournalPages.Clear();
            UpdateLayout();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }


#endregion

        #region Pagemanagment Dialog

        private async Task ApplyPageManagmentDialog(List<JournalPage> result)
        {
            // Apply result
            ClearJournal();

            double currentScrollOffset = this.mainScrollView.VerticalOffset;
            var dialog = new WaitingDialog(System.IO.Path.GetFileNameWithoutExtension(currentJournalTitle)) { Owner = this };
            dialog.Show();

            IsEnabled = false;
            isInitalized = false;

            try
            {
                int countPages = 0;
                double progress = 0;

                // Load pages to iPages
                foreach (var page in result)
                {
                    DrawingCanvas canvas = null;
                    byte[] background = null;
                    Orientation orientation = page.Orientation;

                    if (page is PdfJournalPage pdf)
                        background = pdf.PageBackground;

                    canvas = AddPage(GeneratePage(page.PaperPattern, background, orientation));

                    if (countPages == 0)
                    {
                        // Set last modified canvas to this, because the old is non existing any more
                        DrawingCanvas.LastModifiedCanvas = canvas;
                    }

                    progress = (countPages++ + 1) / (double)result.Count;
                    dialog.SetProgress(progress, countPages, result.Count);

                    // Delay 1ms to ensure the dialog will be displayed correctly
                    await Task.Delay(1);

                    StrokeCollection strokes = null;
                    await Task.Run(() =>
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            ms.Write(page.Data, 0, page.Data.Length);
                            ms.Position = 0;
                            strokes = new StrokeCollection(ms);
                        }
                    }).ContinueWith(new Action<Task>((Task t) =>
                    {
                        Application.Current.Dispatcher.Invoke(new System.Action(() =>
                        {
                            canvas.Strokes = strokes;

                            if (page.HasAdditionalResources)
                            {
                                foreach (JournalResource jr in page.JournalResources)
                                    UIHelper.AddJournalResourceToCanvas(jr, canvas);
                            }
                        }));
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"{Properties.Resources.strFailedToLoadJournal} {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}", Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
                dialog.Close();
                isInitalized = true;

                // Make sure that user will be asked to save
                DrawingCanvas.Change = true;

                // Apply old offset
                mainScrollView.ScrollToVerticalOffset(currentScrollOffset);
            }
        }

        private async Task ShowPageManagmentDialog()
        {
            var pgmd = new PageManagmentDialog();
            pgmd.PageManagmentControl.Initalize(CurrentJournalPages.ToList(), pgmd);
            var userResult = pgmd.ShowDialog();

            if (userResult.HasValue && userResult.Value)
                await ApplyPageManagmentDialog(pgmd.PageManagmentControl.Result);
        }

        private async void btnManagePages_Click(object sender, RoutedEventArgs e)
        {
            await ShowPageManagmentDialog();
        }

        private async void btnPageManagment_Click(object sender, RoutedEventArgs e)
        {
            await ShowPageManagmentDialog();
        }

        private void MenuBackstageEditPages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PageManagementControl.Initalize(CurrentJournalPages.ToList(), this);
        }

#endregion

        #region Export

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var exportDialog = new ExportDialog();

            exportDialog.exportControl.Initalize(CurrentJournalPages, CurrentJournalPages[cmbPages.SelectedIndex], exportDialog);
            exportDialog.ShowDialog();
        }

        private void MenuBackstageExport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextExportStatus.Text = string.Empty;
            ExportControl.Initalize(CurrentJournalPages, CurrentJournalPages[cmbPages.SelectedIndex], this);
        }

#endregion

        #region Copy / Paste
        private Clipboard clipboard = new Clipboard();
        private bool waitingForClickToPaste = false;
        private Tools pasteBackupTool = Tools.Pencil1;
        private Rect selctionBounds = new Rect();

        private void BtnCopy_Click_1(object sender, RoutedEventArgs e)
        {
            selctionBounds = DrawingCanvas.LastModifiedCanvas.GetSelectionBounds();

            // Fill clipboard
            clipboard = new Clipboard();

            var elements = DrawingCanvas.LastModifiedCanvas.GetSelectedElements();
            foreach (UIElement elem in elements)
            {
                var point = UIHelper.ConvertTransformToProperties(elem, DrawingCanvas.LastModifiedCanvas);
                var result = UIHelper.CloneElement(elem);

                clipboard.Children.Add(result);
                clipboard.ChildPoints.Add(point);
            }

            var strokes = DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes();
            clipboard.Strokes = strokes.Clone();
        }

        private void BtnPaste_Click_1(object sender, RoutedEventArgs e)
        {
            if (!clipboard.IsEmpty)
            {
                waitingForClickToPaste = true;
                pasteBackupTool = currentTool;

                ShowInsertHint();

                ApplyToAllCanvas(new Action<DrawingCanvas>((DrawingCanvas target) =>
                {
                    target.SetCopyMode();
                }));
            }
            else
            {
                MessageBox.Show(this, Properties.Resources.strEmptyClipboard, Properties.Resources.strEmptyClipboardTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Canvas_OnCopyPositionIsKnown(Point e)
        {
            waitingForClickToPaste = false;
            pasteElements(e);

            // Restore old tool
            SwitchTool(pasteBackupTool, true);

            ApplyToAllCanvas(new Action<DrawingCanvas>((DrawingCanvas target) =>
            {
                target.UnsetCopyMode();
            }));
        }

        private void pasteElements(Point pa)
        {
            Point referencePoint = new Point(selctionBounds.Left, selctionBounds.Top);

            // Paste
            int counter = 0;
            foreach (UIElement child in clipboard.Children)
            {
                Point newPoint = new Point(pa.X + Math.Abs(referencePoint.X - clipboard.ChildPoints[counter].X), pa.Y + Math.Abs(referencePoint.Y - clipboard.ChildPoints[counter].Y));

                child.SetValue(InkCanvas.LeftProperty, newPoint.X);
                child.SetValue(InkCanvas.TopProperty, newPoint.Y);

                DrawingCanvas.LastModifiedCanvas.Children.Add(child);
                counter++;
            }

            StrokeCollection nCollection = new StrokeCollection();
            foreach (Stroke st in clipboard.Strokes)
            {
                StylusPointCollection spc = new StylusPointCollection();
                foreach (Point p in st.StylusPoints)
                {
                    Point newPoint = new Point(pa.X + Math.Abs(referencePoint.X - p.X), pa.Y + Math.Abs(referencePoint.Y - p.Y));
                    spc.Add(new StylusPointCollection(new Point[] { newPoint }));
                }
                nCollection.Add(new Stroke(spc)
                {
                    DrawingAttributes = st.DrawingAttributes.Clone()
                });
            }
            DrawingCanvas.LastModifiedCanvas.Strokes.Add(nCollection);
            clipboard.Renew();
        }

#endregion

        #region Insert

        private UIElement insertClipboard = null;
#pragma warning disable IDE0052 // Ungelesene private Member entfernen
        private Point lastInsertPosition;
#pragma warning restore IDE0052 // Ungelesene private Member entfernen
        private Tools insertBackupTool = Tools.Pencil1;

        private void Canvas_OnInsertPositionIsKnown(Point e)
        {
            DrawingCanvas.LastModifiedCanvas.Children.Add(insertClipboard);
            insertClipboard.SetValue(InkCanvas.LeftProperty, e.X);
            insertClipboard.SetValue(InkCanvas.TopProperty, e.Y);
            lastInsertPosition = e;

            insertClipboard.RenderTransform = new RotateTransform(0) { };

            // Restore old tool
            SwitchTool(insertBackupTool, true);

            ApplyToAllCanvas(new Action<DrawingCanvas>((DrawingCanvas target) =>
            {
                target.UnsetInsertMode();
            }));

            // Select inserted object!
            DrawingCanvas.LastModifiedCanvas.Select(null, new UIElement[] { insertClipboard });
            SetStateForToggleButton(btnSelect, Tools.Select);
            SwitchTool(Tools.Select, true);

            insertClipboard = null;
        }

        private void InsertText(string text)
        {
            // Determine foreground from last selected pencil
            SolidColorBrush foreground = new SolidColorBrush(Pen.Instance[(int)lastSelectedPencil - 1].FontColor.ToColor());

            var textblock = new TextBlock
            {
                Text = text,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                Width = Consts.InsertTextWidth,
                Height = Consts.InsertTextHeight,
                Foreground = foreground
            };

            InsertUIElement(textblock);
        }

        public void InsertUIElement(UIElement elem)
        {
            if (elem == null)
                return;

            insertBackupTool = currentTool;
            ShowInsertHint();

            insertClipboard = elem;
            ApplyToAllCanvas(new Action<DrawingCanvas>((DrawingCanvas target) =>
            {
                target.SetInsertMode();
            }));
        }

        private void InsertFromClipboard()
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                var img = new Image
                {
                    Source = System.Windows.Clipboard.GetImage(),
                    Width = Consts.InsertImageWidth,
                    Height = Consts.InsertImageHeight,
                    Stretch = Settings.Instance.InsertImageStretchFormat.ConvertStretch()
                };

                InsertUIElement(img);
            }
            else if (System.Windows.Clipboard.ContainsText())
                InsertText(System.Windows.Clipboard.GetText());
            else
                MessageBox.Show(Properties.Resources.strUnsupportedFormat, Properties.Resources.strUnssportedFormatTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void InsertImageFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = $"{Properties.Resources.strImages}|*.png;*.tif;*.bmp;*.jpg;*.jpeg" };
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                try
                {
                    Image image = new Image
                    {
                        Width = Consts.InsertImageWidth,
                        Height = Consts.InsertImageHeight,
                        Source = new BitmapImage(new Uri(ofd.FileName, UriKind.Absolute)),
                        Stretch = Settings.Instance.InsertImageStretchFormat.ConvertStretch()
                    };

                    InsertUIElement(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Properties.Resources.strFailedToInsertImage} {ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static void ShowInsertHint()
        {
            if (!Settings.Instance.DoesNotShowInsertHint && MessageBox.Show(Properties.Resources.strHintInsert, Properties.Resources.strHintTitle, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Settings.Instance.DoesNotShowInsertHint = true;
                Settings.Instance.Save();
            }
        }

        private void btnInsertFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            InsertFromClipboard();
        }

        private void btnInsertImage_Click(object sender, RoutedEventArgs e)
        {
            InsertImageFromFile();
        }

        private void btnInsertText_Click(object sender, RoutedEventArgs e)
        {
            TextInsertWindow dialog = new TextInsertWindow();
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
                InsertText(dialog.Result);
        }

        private void btnInsertDate_Click(object sender, RoutedEventArgs e)
        {
            string toInsert = $"{Properties.Resources.strDate} {DateTime.Now.ToString(Properties.Resources.strDateFormat)}";
            InsertText(toInsert);
        }

#endregion

        #region Background

        public void ApplyBackground()
        {
            if (Settings.Instance.PageBackground == SimpleJournal.Common.Background.Default)
            {
                mainScrollView.Background = Consts.DefaultBackground;
                return;
            }

            try
            {
                string imageFileName = string.Empty;

                switch (Settings.Instance.PageBackground)
                {
                    case SimpleJournal.Common.Background.Default: mainScrollView.Background = Consts.DefaultBackground; break;
                    case SimpleJournal.Common.Background.Blue: imageFileName = "blue"; break;
                    case SimpleJournal.Common.Background.Sand: imageFileName = "sand"; break;
                    case SimpleJournal.Common.Background.Wooden1: imageFileName = "wooden-1"; break;
                    case SimpleJournal.Common.Background.Wooden2: imageFileName = "wooden-2"; break;
                }

                if (Settings.Instance.PageBackground != SimpleJournal.Common.Background.Custom)
                {
                    string uri = $"pack://application:,,,/SimpleJournal;component/resources/backgrounds/{imageFileName}.jpg";
                    ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(uri))) { Stretch = Stretch.UniformToFill };
                    mainScrollView.Background = imageBrush;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Settings.Instance.CustomBackgroundImagePath))
                    {
                        ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(Settings.Instance.CustomBackgroundImagePath))) { Stretch = Stretch.UniformToFill };
                        mainScrollView.Background = imageBrush;
                    }
                    else
                        mainScrollView.Background = Consts.DefaultBackground;
                }
            }
            catch
            {
                // fallback
                mainScrollView.Background = Consts.DefaultBackground;
            }
        }
        #endregion

        #region General Events / Touch

        private void RibbonWindow_Activated(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                return;

            System.Diagnostics.Debug.WriteLine("MainWindow activated ....");
#if !UWP

            // Disable touch screen when window get reactivated ...
            if (Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(false);

#endif
        }

        private void RibbonWindow_Deactivated(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                return;

            System.Diagnostics.Debug.WriteLine("MainWindow deactivated ....");
#if !UWP

            // Enable touch screen when windows gets deactivated ...
            if (Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(true);

#endif
        }

        private void RibbonWindow_StateChanged(object sender, EventArgs e)
        {
#if !UWP
            if (WindowState == WindowState.Minimized && Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(true);
            else if ((WindowState == WindowState.Normal || WindowState == WindowState.Maximized) && Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(false);
#endif
        }

#endregion
    }

    #region Converter
    public class SelectedIndexToColumnSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int result && result != -1)
                return 1;

            return 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedIndexToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int result && result != -1)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
#endregion
}