using Fluent;
using Microsoft.Win32;
using SimpleJournal.Actions;
using SimpleJournal.Controls;
using SimpleJournal.Controls.Templates;
using SimpleJournal.Data;
using SimpleJournal.Dialogs;
using SimpleJournal.Helper;
using SimpleJournal.Shared;
using SimpleJournal.Templates;
#if !UWP
using SJFileAssoc;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static SimpleJournal.Data.Enums;
using Pen = SimpleJournal.Data.Pen;

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
        private bool startSetupDialog = false;
        private bool preventPageBoxSelectionChanged = false;
        private bool forceOpenSidebar = false;
        private double currentScaleFactor = 1.0;
        private Tools currentTool = Tools.Pencil1;
        private Tools lastSelectedPencil = Tools.Pencil1;
        private TextAnalyser textAnalyserInstance = null;
        private string currentJournalName = string.Empty;

        /// <summary>
        /// The timer which runs in the background to make a backup
        /// </summary>
        private readonly DispatcherTimer autoSaveBackupTimer = new DispatcherTimer(DispatcherPriority.Background);

        public int CurrentPages => CurrentJournalPages.Count;

        // Buttons
        private readonly Fluent.ToggleButton[] toggleButtons = { };
        private readonly DropDownToggleButton[] dropDownToggleButtons = { };

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
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            // Display last opened files
            RefreshRecentlyOpenedFiles();
            RecentlyOpenedDocuments.DocumentsChanged += delegate ()
            {
                RefreshRecentlyOpenedFiles();
            };

            CurrentJournalPages.CollectionChanged += IPages_CollectionChanged;
            Page page = GeneratePage();
            DrawingCanvas.LastModifiedCanvas = AddPage(page);
            cmbPages.SelectedIndex = 0;

            CurrentDrawingAttributes = (page as IPaper).Canvas.DefaultDrawingAttributes;

#if UWP
            GeneralHelper.InstallApplicationIconForFileAssociation();
            if (GeneralHelper.InstallFileAssoc())
            {
                var tempFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "SjFileAssoc.exe");
                if (System.IO.File.Exists(tempFile))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(tempFile);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
#endif

            // Register file association
            if (!Settings.Instance.FirstStart)
            {
                Settings.Instance.FirstStart = true;
                Settings.Instance.Save();

#if !UWP
                FileAssociations.EnsureAssociationsSet();
#endif

                // Display Setup Dialog
                startSetupDialog = true;
            }

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
            this.PreviewKeyDown += (s, e) =>
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
                else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    // Save
                    btnSaveProject_Click(null, null);
                }
            };

            // Boot with fullscreen
            left = this.Left;
            top = this.Top;
            width = this.Width;
            height = this.Height;

            // Maximized = Full screen
            // Normal = Maximized
            // Minimized = Normal
            if (Settings.Instance.WindowState == WindowState.Maximized)
                ToggleFullscreen();
            else if (Settings.Instance.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;

            // Call other init methods
            RefreshVerticalScrollbarSize();
            UpdateTextMarkerAttributes();

            DrawingCanvas.ChildElementsSelected += DrawingCanvas_ChildElementsSelected;

            // Apply default size to textmarker
            currentTextMarkerAttributes.Height = Consts.TextMarkerSizes[0].Width;
            currentTextMarkerAttributes.Width = Consts.TextMarkerSizes[0].Height;

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
            RefreshSizeBar();
            UpdateGlowingBrush();

#if !UWP
            GeneralHelper.RemoveUpdaterIfAny();
            GeneralHelper.SearchForUpdates();            
#endif

            DrawingCanvas.OnChangedDocumentState += DrawingCanvas_OnChangedDocumentState;

            var screen = WpfScreen.GetScreenFrom(this);
            if (screen != null)
            {
                var bounds = screen.DeviceBounds;
                // ToDo: *** Fix #6
                if (bounds.Width > bounds.Height)
                {
                    this.Width = Math.Min(1130, bounds.Width);
                    this.Height = Math.Min(800, bounds.Height);
                }
                else
                {
                    this.Height = Math.Min(800, bounds.Height);
                    this.Width = bounds.Width;
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
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (startSetupDialog)
            {
                startSetupDialog = false;
                new SetupDialog().ShowDialog();
            }

            // Check if command line parameter was passed
            if (!string.IsNullOrEmpty(App.Path) && System.IO.File.Exists(App.Path))
                await LoadJournal(App.Path);

            if (Settings.Instance.UseAutoSave)
                ShowRecoverAutoBackupFileDialog();

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

        #region AutoSave - Backup

        private string lastBackupFileName = string.Empty;

        private void ShowRecoverAutoBackupFileDialog()
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

                System.IO.FileInfo[] autoSaveFiles = new System.IO.DirectoryInfo(Consts.AutoSaveDirectory).GetFiles();
                var backupFiles = autoSaveFiles.Where(f => f.Name.EndsWith(".journal"));

                foreach (var journalFile in backupFiles)
                {
                    try
                    {
                        Journal j = Journal.LoadJournal(journalFile.FullName, true);
                        showRecoverDialog |= j.IsBackup && !ProcessHelper.IsProcessActiveByTaskId(j.ProcessID);

                        j.Pages.Clear();
                        j = null;

                        // If it is true => then no more search needed
                        if (showRecoverDialog)
                            break;
                    }
                    catch
                    {
                        // ignore
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
                    this.DeleteAutoSaveBackup();

                    // Bring this window to the front
                    this.Topmost = true;
                    this.Topmost = false;
                    this.Activate();
                }
                else
                    this.DeleteAutoSaveBackup();
            }

            // Start timer just after the dialog is appeard (or not)
            autoSaveBackupTimer.Interval = TimeSpan.FromMinutes(Settings.Instance.AutoSaveIntervalMinutes);
            autoSaveBackupTimer.Tick += AutoSaveBackupTimer_Tick;
            autoSaveBackupTimer.Start();
        }

        private void AutoSaveBackupTimer_Tick(object sender, EventArgs e)
        {
            CreateBackup();
        }

        private void CreateBackup()
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
            bool result = SaveJournal(path, true);
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
        }

        public void DeleteAutoSaveBackup(bool onClosing = false)
        {
            // Delete the backup with this ProcessID
            // Get the backup with this task id - it's obiously the last one - this instace -  has created!
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
                    if (new System.IO.DirectoryInfo(Consts.AutoSaveDirectory).GetFiles().Count() == 0)
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
        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //MessageBox.Show($"{Properties.Resources.strUnexceptedFailure}{Environment.NewLine}{Environment.NewLine}{e.Exception.Message}", Properties.Resources.strUnexceptedFailureTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = string.Empty;

            if (e.ExceptionObject != null && e.ExceptionObject is Exception ex)
            {
                message = ex.ToString();

                if (ex.InnerException != null)
                    message += Environment.NewLine + Environment.NewLine + ex.InnerException.ToString();
            }
            MessageBox.Show($"{Properties.Resources.strUnexceptedFailure}{Environment.NewLine}{Environment.NewLine}{message}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.strUnexceptedFailureLine1}", Properties.Resources.strUnexceptedFailureTitle, MessageBoxButton.OK, MessageBoxImage.Error);


            // Try at least to create a backup - if SJ crashes - the user can restore the backup and everything is fine
            CreateBackup();
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
            foreach (FrameworkElement child in this.pages.Children)
            {
                if (child != null && child is Frame && counter == index)
                {
                    var paper = (child as Frame).Content as IPaper;
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
                else if (child is Frame)
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
                this.pnlSidebar.Visibility = Visibility.Hidden;
                return;
            }

            // Display ui elements in sidebar
            this.pnlItems.Items.Clear();
            this.pnlSidebar.Visibility = Visibility.Visible;

            foreach (UIElement element in DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)))
            {
                string text = Properties.Resources.strUnknown;

                if (element is Shape shape)
                {
                    double w = Math.Round(shape.Width, 1);
                    double h = Math.Round(shape.Height, 1);

                    if (element is Polygon pol)
                    {
                        int edge = ConvexHull.GetConvexHull(pol.Points.ToList()).Count;
                        if (edge == 3)
                            text = Properties.Resources.strTriangle;
                        else if (edge == 4)
                            text = Properties.Resources.strQuad;
                        else if (edge == 5)
                            text = Properties.Resources.strPentagon;
                        else if (edge == 6)
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
                    text = Properties.Resources.strText;
                else if (element is Plot)
                    text = Properties.Resources.strPlot;

                Viewbox v = new Viewbox
                {
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.Both,
                    Child = GeneralHelper.CloneElement(element),
                };

                var item = new CustomListBoxItem(element, v)
                {
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Height = Consts.SidebarListBoxItemHeight
                };

                v.Width = Consts.SidebarListBoxItemViewboxSize;
                v.Height = Consts.SidebarListBoxItemViewboxSize;

                StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Background = new SolidColorBrush(Colors.Transparent);
                panel.Children.Add(v);

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
                this.pnlItems.Items.Add(item);
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
                this.pnlSidebar.Visibility = Visibility.Hidden;
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
                this.pnlSidebar.Visibility = Visibility.Hidden;
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
                    this.pnlSidebar.Visibility = Visibility.Hidden;

            }
            else
                MessageBox.Show(this, Properties.Resources.strNoObjectsToDelete, Properties.Resources.strEmptySelection, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Private Methods

        public void UpdateTextMarkerAttributes(bool reset = false)
        {
            if (reset)
            {
                // Default values
                currentTextMarkerAttributes.Width = new Settings().TextMarkerSize.Height; // Consts.TEXT_MARKER_WIDTH;
                currentTextMarkerAttributes.Height = new Settings().TextMarkerSize.Width; // Consts.TEXT_MARKER_HEIGHT;
                currentTextMarkerAttributes.StylusTip = StylusTip.Rectangle;
                currentTextMarkerAttributes.Color = new Settings().TextMarkerColor.ToColor(); //Consts.TEXT_MARKER_COLOR;

                Settings.Instance.TextMarkerSize = Consts.TextMarkerSizes[0];
                Settings.Instance.TextMarkerColor = new Data.Color(Consts.TextMarkerColor.A, Consts.TextMarkerColor.R, Consts.TextMarkerColor.G, Consts.TextMarkerColor.B);
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
            textMarkerTemplate.LoadPen(new Pen(new Data.Color(currentTextMarkerAttributes.Color), Settings.Instance.TextMarkerSize.Width, Settings.Instance.TextMarkerSize.Height));

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
            btnPen1.DropDown = penTemplates[0];
            btnPen2.DropDown = penTemplates[1];
            btnPen3.DropDown = penTemplates[2];
            btnPen4.DropDown = penTemplates[3];

            penTemplates[0].OnChangedColorAndSize += BtnPen1_OnChanged;
            penTemplates[1].OnChangedColorAndSize += BtnPen2_OnChanged;
            penTemplates[2].OnChangedColorAndSize += BtnPen3_OnChanged;
            penTemplates[3].OnChangedColorAndSize += BtnPen4_OnChanged;

            penTemplates[0].LoadPen(currentPens[0]);
            penTemplates[1].LoadPen(currentPens[1]);
            penTemplates[2].LoadPen(currentPens[2]);
            penTemplates[3].LoadPen(currentPens[3]);

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

        private void AddNewPage(PaperType paperType)
        {
            AddPage(GeneratePage(paperType));
            RefreshInsertIcon();
            ScrollToPage(CurrentJournalPages.Count - 1);
            DrawingCanvas.Change = true;
        }

        private void RefreshInsertIcon()
        {
            string resourceImageName = string.Empty;
            switch (Settings.Instance.PaperTypeLastInserted)
            {
                case PaperType.Blanco: resourceImageName = "addblankopage.png"; break;
                case PaperType.Chequeued: resourceImageName = "addchequeredpage.png"; break;
                case PaperType.Ruled: resourceImageName = "addruledpage.png"; break;
                case PaperType.Dotted: resourceImageName = "adddotted.png"; break;
            }

            // Switch icon to paperType
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri($"pack://application:,,,/SimpleJournal;component/resources/{resourceImageName}");
            bitmapImage.EndInit();

            ButtonInsertNewPageIcon.Source = bitmapImage;
        }

        private void AddPageDropDownTemplate_AddPage(PaperType paperType)
        {
            AddNewPage(paperType);
        }

        private void SimpleFormDropDown_OnSimpleFormDropDownChanged(ShapeType shapeType)
        {
            ApplyToAllCanvas((DrawingCanvas dc) => {
                dc.SetFormMode(shapeType);
            });
        }

        private void PlotDropDownTemplate_OnPlotModeChanged(PlotMode plotMode)
        {
            ApplyToAllCanvas((DrawingCanvas dc) => {
                dc.SetPlotMode(plotMode);
            });
        }

        private void SelectDropDownTemplate_OnColorAndSizeChanged(System.Windows.Media.Color? c, int size)
        {
            // If strokes and child elements are selected change their colors
            List<Actions.Action> changedActions = new List<Actions.Action>();

            foreach (Stroke st in DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes())
            {
                DrawingAttributes old = st.DrawingAttributes.Clone();
                if (c != null && c.HasValue)
                    st.DrawingAttributes.Color = c.Value;

                if (size != -1)
                {
                    st.DrawingAttributes.Width = Consts.StrokeSizes[size].Width;
                    st.DrawingAttributes.Height = Consts.StrokeSizes[size].Height;
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
                    double newSize = (size >= 0 ? Consts.StrokeSizes[size].Width : oldSize);
                    System.Windows.Media.Color oldColor = (sh.Stroke as SolidColorBrush).Color;
                    System.Windows.Media.Color newColor = (c != null && c.HasValue ? c.Value : oldColor);

                    changedActions.Add(new ShapeChangedAction(sh, oldColor, newColor, oldSize, newSize));
                    if (c != null && c.HasValue)
                        sh.Stroke = new SolidColorBrush(c.Value);
                    if (size >= 0)
                        sh.StrokeThickness = Consts.StrokeSizes[size].Width;
                }
                else if (element is Plot plot)
                {
                    double oldSize = plot.StrokeThickness;
                    double newSize = (size >= 0 ? Consts.StrokeSizes[size].Width : oldSize);
                    System.Windows.Media.Color oldColor = plot.Foreground;
                    System.Windows.Media.Color newColor = (c != null && c.HasValue ? c.Value : oldColor);

                    changedActions.Add(new ShapeChangedAction(plot, plot.Foreground, newColor, oldSize, newSize));

                    if (c != null && c.HasValue)
                        plot.Foreground = c.Value;
                    if (size >= 0)
                        plot.StrokeThickness = Math.Round((Consts.StrokeSizes[size].Width + Consts.StrokeSizes[size].Height) / 2.0);

                    plot.RenderPlot();
                }
            }

            DrawingCanvas.LastModifiedCanvas.Manager.AddSpecialAction<Actions.Action>(changedActions);
            DrawingCanvas.Change = true;
            RefreshSideBar();
        }

        private void RulerDropDownTemplate_OnChangedRulerMode(Settings.RulerMode mode)
        {
            ApplyToAllCanvas((DrawingCanvas dc) => {
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
            // Do not divide with zero!
            if (mainScrollView.ScrollableHeight == 0)
                return 0;

            double totalHeight = mainScrollView.ExtentHeight;
            double scrollPercentage = mainScrollView.VerticalOffset / mainScrollView.ScrollableHeight;
            double result = ((totalHeight * scrollPercentage) / totalHeight) * CurrentPages;

            return (int)result;
        }

        private void RefreshVerticalScrollbarSize()
        {
            ScrollViewer scrollViewer = mainScrollView;
            scrollViewer.ApplyTemplate();
            ScrollBar scrollBar = (ScrollBar)scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer);
            scrollBar.Width = (Settings.Instance.EnlargeScrollbar ? Consts.ScrollBarExtendedWidth : Consts.ScrollBarDefaultWidth);
        }

        private Page GeneratePage(PaperType? paperType = null)
        {
            Page pageContent = null;
            PaperType paperPattern = Settings.Instance.PaperType;

            if (paperType.HasValue)
                paperPattern = paperType.Value;

            switch (paperPattern)
            {
                case PaperType.Blanco: pageContent = new Blanco(); break;
                case PaperType.Chequeued: pageContent = new Chequered(); break;
                case PaperType.Ruled: pageContent = new Ruled(); break;
                case PaperType.Dotted: pageContent = new Dotted(); break;
            }

            IPaper page = pageContent as IPaper;
            // Apply properties and events to the new canvas
            page.Canvas.EditingMode = currentkInkMode;
            page.Canvas.DefaultDrawingAttributes = CurrentDrawingAttributes;
            page.Canvas.OnCopyPositionIsKnown += Canvas_OnCopyPositionIsKnown;
            page.Canvas.OnInsertPositionIsKnown += Canvas_OnInsertPositionIsKnown;
            page.Canvas.SelectionChanged += Canvas_SelectionChanged;
            page.Canvas.Children.CollectionChanged += Children_CollectionChanged;
            page.Canvas.RemoveElementFromSidebar += delegate (List<UIElement> temp) {

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
                page.Canvas.SetRulerMode(Settings.RulerMode.Normal);
            else if (currentTool == Tools.FreeHandPolygon)
                page.Canvas.SetFreeHandPolygonMode(polygonDropDownTemplate);
            else if (currentTool == Tools.Form)
                page.Canvas.SetFormMode(simpleFormDropDown.ShapeType);
            else if (currentTool == Tools.CooardinateSystem)
                page.Canvas.SetPlotMode();
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

        private DrawingCanvas AddPage(Page page, int index = -1)
        {
            Frame elementToAdd = new Frame() { Content = page };
            elementToAdd.Background = new SolidColorBrush(Colors.Transparent);
            // This is needed to have something to click on for scrolling
            elementToAdd.Background = new SolidColorBrush(Colors.Transparent);
            var paper = page as IPaper;

            if (index == -1)
            {
                CurrentJournalPages.Add(paper);
                this.pages.Children.Add(elementToAdd);
            }
            else
                this.pages.Children.Insert(index, elementToAdd);

            double offset = mainScrollView.VerticalOffset;


            PageSplitter pageSplitter = new PageSplitter();
            pageSplitter.OnPageAdded += delegate (PageSplitter owner, PaperType type)
            {
                var newPage = GeneratePage(type);
                int pageIndex = pages.Children.IndexOf(elementToAdd);

                // Adjust page index
                if (pageIndex != -1)
                    pageIndex += 2;

                AddPage(newPage, pageIndex);
                CurrentJournalPages.Insert(CurrentJournalPages.IndexOf(paper) + 1, newPage as IPaper);

                RefreshPages();
            };

            if (index == -1)
                this.pages.Children.Add(pageSplitter);
            else
                this.pages.Children.Insert(index + 1, pageSplitter);

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
                for (int i = 0; i < currentPens.Length; i++)
                {
                    currentPens[i] = new Pen(Consts.PEN_COLORS[i], Consts.StrokeSizes[0].Width, Consts.StrokeSizes[0].Height);
                }

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

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Properties.Resources.strHelpLnk);
            }
            catch
            { }
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

        private bool ignoreToggleButtonHandling = false;

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

            var result = Consts.RubberSizes[index];
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

        public void ApplySettings()
        {
            isInitalized = false;
            RefreshVerticalScrollbarSize();
            CurrentDrawingAttributes.IgnorePressure = !Settings.Instance.UsePreasure;
            CurrentDrawingAttributes.FitToCurve = Settings.Instance.UseFitToCurve;

            UpdatePenButtons();

            // Refresh Text marker
            UpdateTextMarkerAttributes();

            // Refresh recently openend documents
            RefreshRecentlyOpenedFiles();

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
                    target.SetPlotMode();
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
                    currentPens[SelectedPen].FontColor = new Data.Color(c.Value.A, c.Value.R, c.Value.G, c.Value.B);
                else
                    currentPens[index].FontColor = new Data.Color(c.Value.A, c.Value.R, c.Value.G, c.Value.B);

                rulerDropDownTemplate.SetColor(c.Value);
            }

            if (sizeIndex >= 0)
            {
                currentPens[index].Width = Consts.StrokeSizes[sizeIndex].Width;
                currentPens[index].Height = Consts.StrokeSizes[sizeIndex].Height;
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
                var textMakerResult = Consts.TextMarkerSizes[sizeIndex];
                Settings.Instance.TextMarkerSize = textMakerResult;
                Settings.Instance.Save();
                currentTextMarkerAttributes.Width = textMakerResult.Height;
                currentTextMarkerAttributes.Height = textMakerResult.Width;
            }

            if (c != null)
            {
                Settings.Instance.TextMarkerColor = new Data.Color(c.Value);
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
            //TouchHelper.SetTouchState(false);
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveProject();
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

        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Print();
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
            InsertImageFromClipboard();
        }

        #endregion

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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized && !preventPageBoxSelectionChanged && cmbPages.SelectedIndex != -1)
            {
                int pTarget = cmbPages.SelectedIndex;
                ScrollToPage(pTarget);
            }
        }

        private void ScrollToPage(int pTarget)
        {
            double resultOffset = (pTarget == 0 ? 0 : pTarget * (new Chequered().Height * currentScaleFactor) + ((pTarget - 1) * Consts.SpaceBetweenPages * currentScaleFactor));
            if (pTarget != 0)
                mainScrollView.ScrollToVerticalOffset(resultOffset + (Consts.SpaceBetweenPages * currentScaleFactor));
            else
                mainScrollView.ScrollToVerticalOffset(0.0);
        }

        private void SaveProject()
        {
            if (string.IsNullOrEmpty(currentJournalPath))
            {
                SaveFileDialog dialog = new SaveFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal", Title = Properties.Resources.strSave };
                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    SaveJournal(dialog.FileName);
                    this.UpdateTitle(System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));
                }
            }
            else
                SaveJournal(currentJournalPath);
        }

        private void Print()
        {
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
                int pageCount = 1;
                for (int i = from; i <= to; i++)
                {
                    pd.PrintVisual(CurrentJournalPages[i].Canvas, $"{Properties.Resources.strPrinting} {pageCount++}");
                }
            }
        }

        private void InsertImageFromClipboard()
        {
            if (System.Windows.Clipboard.ContainsImage())
            {
                var img = new Image { Source = System.Windows.Clipboard.GetImage() };
                img.Width = Consts.InsertImageWidth;
                img.Height = Consts.InsertImageHeight;

                InsertUIElement(img);
            }
            else
            {
                MessageBox.Show(Properties.Resources.strUnsupportedFormat, Properties.Resources.strUnssportedFormatTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertImageFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = $"{Properties.Resources.strImages}|*.png;*.tif;*.bmp;*.jpg;*.jpeg" };
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                Image image = new Image
                {
                    Width = Consts.InsertImageWidth,
                    Height = Consts.InsertImageHeight,
                    Source = new BitmapImage(new Uri(ofd.FileName, UriKind.Absolute))
                };

                InsertUIElement(image);
            }
        }

        private void btnDisplaySidebar_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)).ToList().Count > 0)
            {
                forceOpenSidebar = true;
                DrawingCanvas.LastModifiedCanvas.Select(new UIElement[] { DrawingCanvas.LastModifiedCanvas.Children.Omit(typeof(Line)).ToList()[0] });
            }
        }

        private void btnCreateForm_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes() != null && DrawingCanvas.LastModifiedCanvas.GetSelectedStrokes().Count > 0 && !DrawingCanvas.LastModifiedCanvas.ConvertSelectedStrokesToShape())
                MessageBox.Show(this, Properties.Resources.strNoShapeRecognized, Properties.Resources.strNoShapeRecognizedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnSaveProject_Click(object sender, RoutedEventArgs e)
        {
            SaveProject();
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal", Title = Properties.Resources.strSaveAs };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                SaveJournal(dialog.FileName);
                this.UpdateTitle(System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));
            }
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

        private async void BtnManagePages_Click(object sender, RoutedEventArgs e)
        {
            var pgmd = new PageManagmentDialog(CurrentJournalPages.ToList());
            var userResult = pgmd.ShowDialog();

            if (userResult.HasValue && userResult.Value)
            {
                // Apply result
                ClearJournal();

                double currentScrollOffset = this.mainScrollView.VerticalOffset;
                var dialog = new WaitingDialog(System.IO.Path.GetFileNameWithoutExtension(currentJournalTitle), 1) { Owner = this };
                dialog.Show();

                this.IsEnabled = false;
                isInitalized = false;

                try
                {
                    int countPages = 0;
                    double progress = 0;

                    // Load pages to iPages
                    foreach (var page in pgmd.Result)
                    {
                        DrawingCanvas canvas = null;
                        canvas = AddPage(GeneratePage(page.PaperPattern));

                        if (countPages == 0)
                        {
                            // Set last modified canvas to this, because the old is non existing any more
                            DrawingCanvas.LastModifiedCanvas = canvas;
                        }

                        progress = (countPages++ + 1) / (double)pgmd.Result.Count;
                        dialog.SetProgress(progress, countPages, pgmd.Result.Count);

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
                                        JournalResource.AddJournalResourceToCanvas(jr, canvas);
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
                    this.IsEnabled = true;
                    dialog.Close();
                    isInitalized = true;

                    // Make sure that user will be asked to save
                    DrawingCanvas.Change = true;

                    // Apply old offset
                    this.mainScrollView.ScrollToVerticalOffset(currentScrollOffset);
                }
            }
        }

        private void BtnInsertNewPage_Click(object sender, EventArgs e)
        {
            AddNewPage(Settings.Instance.PaperTypeLastInserted);
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
                OpenFileDialog ofd = new OpenFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal|{Properties.Resources.strImages}|*.png;*.tif;*.bmp;*.jpg;*.jpeg" }; // .journal or images
                var result = ofd.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    pnlSidebar.Visibility = Visibility.Hidden;
                    await LoadJournal(ofd.FileName);
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

                    Frame result = null;
                    foreach (Frame frame in pages.Children.OfType<Frame>())
                    {
                        if ((frame.Content as IPaper).Canvas.Equals(canvasToRemove))
                        {
                            result = frame;
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

        private async void test_Click(object sender, RoutedEventArgs e)
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
                if (this.pnlSidebar.IsVisible)
                {
                    this.pnlSidebar.Visibility = Visibility.Collapsed;
                    if (can.Children.Count > 0)
                        can.Select(new UIElement[] { can.Children[0] });
                }

                // Deleting strokes is also a change!
                DrawingCanvas.Change = true;
            }
        }

        #region Exit
        private bool closedButtonWasPressed = false;

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas.Change)
            {
                var result = MessageBox.Show(this, Properties.Resources.strSaveChanges, Properties.Resources.strSaveChangesTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.No)
                {
                    closedButtonWasPressed = true;
                    Close();
                }
                else if (result == MessageBoxResult.Yes)
                {
                    this.btnSaveProject_Click(sender, e);
                    closedButtonWasPressed = true;
                    Close();
                }
            }
            else
            {
                closedButtonWasPressed = true;
                Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

#pragma warning disable CS0219 // Variable ist zugewiesen, der Wert wird jedoch niemals verwendet
            bool close = false;
#pragma warning restore CS0219 // Variable ist zugewiesen, der Wert wird jedoch niemals verwendet
            if (!closedButtonWasPressed && DrawingCanvas.Change)
            {
                // Create a backup before closing - in case of windows will kill the app if the user will not awnser the dialog
                CreateBackup();

                // Ask 
                var result = MessageBox.Show(this, Properties.Resources.strSaveChanges, Properties.Resources.strSaveChangesTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
                else if (result == MessageBoxResult.No)
                {
                    DeleteAutoSaveBackup(true);
                    e.Cancel = false;
                    close = true;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    this.btnSaveProject_Click(null, null);
                    DeleteAutoSaveBackup(true);
                    e.Cancel = false;
                    close = true;
                }
            }
            else
            {
                DeleteAutoSaveBackup(true);
                close = true;
            }

#if !UWP
            if (close && Settings.Instance.UseTouchScreenDisabling)
            {
                if (ProcessHelper.SimpleJournalProcessCount > 1)
                    return;

                TouchHelper.SetTouchState(true);
            }
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

            btnZoom100.IsChecked =
            btnZoom120.IsChecked =
            btnZoom150.IsChecked =
            btnZoom180.IsChecked =
            btnZoom200.IsChecked = false;

            if (scale == 1.0)
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
            // This is for prevent jumping while switching zoom (see User Story #4)
            mainScrollView.ScrollToVerticalOffset((mainScrollView.VerticalOffset / currentScaleFactor) * scale);
            currentScaleFactor = scale;

            // Save scale to settings
            Settings.Instance.Zoom = (int)(scale * 100);
            Settings.Instance.Save();
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
            if (!DetermineIfMouseIsInChildFrame(pages.Children, p))
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
            if (!DetermineIfMouseIsInChildFrame(pages.Children, p) && p1.X != double.MinValue && p1.Y != double.MinValue)
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

        private bool DetermineIfMouseIsInChildFrame(UIElementCollection collection, Point ps)
        {
            foreach (UIElement uIElement in collection)
            {
                if (uIElement is Frame f && f.Content is Page p && p is IPaper ip)
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
        private bool SaveJournal(string path, bool saveAsBackup = false)
        {
            try
            {
                Journal journal = new Journal { ProcessID = ProcessHelper.CurrentProcID };

                if (saveAsBackup)
                {
                    // Claim this journal as a backup - the document needs also a process ID to determine if this is an active backup (see 112 in Azure for details)
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
                        JournalPage jp = new JournalPage { PaperPattern = paper.Type };
                        jp.SetData(ms.ToArray());

                        // Check for additional ressources
                        if (currentCanvas.Children.Count > 0)
                        {
                            foreach (UIElement element in currentCanvas.Children)
                            {
                                var result = JournalResource.ConvertFromUIElement(element);
                                if (result != null)
                                    jp.JournalResources.Add(result);
                            }
                        }
                        journal.Pages.Add(jp);
                    }
                }

                journal.Save(path);

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

        private void ClearJournal()
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

        private async Task LoadJournal(string fileName)
        {
            if (fileName.EndsWith(".journal"))
            {
                var dialog = new WaitingDialog(System.IO.Path.GetFileNameWithoutExtension(fileName), 1) { Owner = this };
                try
                {
                    var currentJournal = Journal.LoadJournal(fileName);

                    if (currentJournal == null)
                    {
                        MessageBox.Show(Properties.Resources.strFailedToLoadJournalFromNetwork, Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (currentJournal.IsBackup)
                    {
                        MessageBox.Show(Properties.Resources.strBackupFileCannotBeOpened, Properties.Resources.strBackupFileCannotBeOpenedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (currentJournal.ProcessID > -1)
                    {
                        // A process id is set.
                        if (ProcessHelper.IsProcessActiveByTaskId(currentJournal.ProcessID))
                        {
                            MessageBox.Show(Properties.Resources.strJournalIsAlreadyOpened, Properties.Resources.strJournalIsAlreadyOpenedTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                            // Try to focus the instance where it is
                            ProcessHelper.BringProcessToFront(currentJournal.ProcessID);
                            return;
                        }
                    }

                    // Okay: User has decided to load / discard old document => remove the process ID
                    if (!string.IsNullOrEmpty(currentJournalPath))
                    {
                        // If a path is set remove the process id
                        try
                        {
                            var journal = Journal.LoadJournal(currentJournalPath);
                            journal.ProcessID = -1;
                            journal.Save(currentJournalPath);
                            currentJournalPath = string.Empty;
                        }
                        catch
                        {
                            // ignroe
                        }
                    }

                    currentJournalPath = fileName;

                    this.IsEnabled = false;
                    isInitalized = false;
                    dialog.Show();

                    ClearJournal();
                    RecentlyOpenedDocuments.AddDocument(fileName);

                    int countPages = 0;
                    double progress = 0;

                    foreach (JournalPage jp in currentJournal.Pages)
                    {
                        DrawingCanvas canvas = null;
                        canvas = AddPage(GeneratePage(jp.PaperPattern));

                        if (countPages == 0)
                        {
                            // Set last modified canvas to this, because the old is non existing any more
                            DrawingCanvas.LastModifiedCanvas = canvas;
                        }

                        progress = (countPages++ + 1) / (double)currentJournal.Pages.Count;
                        dialog.SetProgress(progress, countPages, currentJournal.Pages.Count());

                        StrokeCollection strokes = null;
                        await Task.Run(() =>
                        {
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                            {
                                ms.Write(jp.Data, 0, jp.Data.Length);
                                ms.Position = 0;
                                strokes = new StrokeCollection(ms);
                            }
                        }).ContinueWith(new Action<Task>((Task t) =>
                        {
                            Application.Current.Dispatcher.Invoke(new System.Action(() =>
                            {
                                canvas.Strokes = strokes;

                                if (jp.HasAdditionalResources)
                                {
                                    foreach (JournalResource jr in jp.JournalResources)
                                        JournalResource.AddJournalResourceToCanvas(jr, canvas);
                                }
                            }));
                        }));
                    }

                    // Make sure curerntTool is forced after loading
                    SwitchTool(currentTool, true);

                    // Set title due to new document
                    this.UpdateTitle(System.IO.Path.GetFileNameWithoutExtension(fileName));

                    // Delete old auto save files
                    DeleteAutoSaveBackup();

                    // Set process id to document and save it to make sure other instances cannot load this journal
                    currentJournal.ProcessID = ProcessHelper.CurrentProcID;
                    currentJournal.Save(fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"{Properties.Resources.strFailedToLoadJournal} {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}", Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    this.IsEnabled = true;
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
        #endregion

        #region Export

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new ExportDialog(CurrentJournalPages, CurrentJournalPages[cmbPages.SelectedIndex]).ShowDialog();
        }

        #endregion

        #region Copy / Paste
        private Data.Clipboard clipboard = new Data.Clipboard();
        private bool waitingForClickToPaste = false;
        private Tools pasteBackupTool = Tools.Pencil1;
        private Rect selctionBounds = new Rect();

        private void BtnCopy_Click_1(object sender, RoutedEventArgs e)
        {
            selctionBounds = DrawingCanvas.LastModifiedCanvas.GetSelectionBounds();

            // Fill clipboard
            clipboard = new Data.Clipboard();

            var elements = DrawingCanvas.LastModifiedCanvas.GetSelectedElements();
            foreach (UIElement elem in elements)
            {
                var point = GeneralHelper.ConvertTransformToProperties(elem, DrawingCanvas.LastModifiedCanvas);
                var result = GeneralHelper.CloneElement(elem);

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
                target.UnSetCopyMode();
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

        public void InsertUIElement(UIElement elem)
        {
            if (elem == null)
                return;

            if (elem is Image i)
                i.Stretch = Stretch.Fill;

            insertBackupTool = currentTool;
            ShowInsertHint();

            insertClipboard = elem;
            ApplyToAllCanvas(new Action<DrawingCanvas>((DrawingCanvas target) =>
            {
                target.SetInsertMode();
            }));
        }

        private void ShowInsertHint()
        {
            if (!Settings.Instance.DoesNotShowInsertHint)
            {
                if (MessageBox.Show(Properties.Resources.strHintInsert, Properties.Resources.strHintTitle, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Settings.Instance.DoesNotShowInsertHint = true;
                    Settings.Instance.Save();
                }
            }
        }

        private void btnInsertText_Click(object sender, RoutedEventArgs e)
        {
            TextInsertWindow dialog = new TextInsertWindow();
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                var textblock = new TextBlock() { Text = dialog.Result, TextWrapping = System.Windows.TextWrapping.Wrap };
                textblock.Width = Consts.InsertTextWidth;
                textblock.Height = Consts.InsertTextHeight;

                InsertUIElement(textblock);
            }
        }

        private void btnInsertFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            InsertImageFromClipboard();
        }


        private void btnInsertImage_Click(object sender, RoutedEventArgs e)
        {
            InsertImageFromFile();
        }

        private void btnInsertDate_Click(object sender, RoutedEventArgs e)
        {
            string toInsert = $"{Properties.Resources.strDate} {DateTime.Now.ToString(Properties.Resources.strDateFormat)}";

            var textblock = new TextBlock() { Text = toInsert, TextWrapping = System.Windows.TextWrapping.Wrap };

            textblock.Width = Consts.InsertTextWidth;
            textblock.Height = Consts.InsertTextHeight;
            textblock.FontSize = Consts.DefaultTextSize;

            InsertUIElement(textblock);
        }

        #endregion

        #region Sidebar Handling

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

                    GeneralHelper.ConvertTransformToProperties(sh, DrawingCanvas.LastModifiedCanvas);

                    int angle = 0;
                    if (sh.RenderTransform is RotateTransform rt)
                        angle = (int)rt.Angle;

                    objShapeSettings.Load(new ShapeInfo((sh.Fill == null ? Colors.Transparent : (sh.Fill as SolidColorBrush).Color), (sh.Stroke as SolidColorBrush).Color, (int)sh.StrokeThickness, angle));
                }
                else if (item is Image im)
                {
                    objImgSettings.Visibility = Visibility.Visible;
                    objShapeSettings.Visibility = Visibility.Collapsed;
                    objTextSettings.Visibility = Visibility.Collapsed;

                    GeneralHelper.ConvertTransformToProperties(im, DrawingCanvas.LastModifiedCanvas);
                }
                else if (item is TextBlock tb)
                {
                    objTextSettings.Visibility = Visibility.Visible;
                    objShapeSettings.Visibility = Visibility.Collapsed;
                    objImgSettings.Visibility = Visibility.Collapsed;

                    GeneralHelper.ConvertTransformToProperties(tb, DrawingCanvas.LastModifiedCanvas);

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

        private void ObjImgSettings_Changed(int angle)
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
                    im.RenderTransform = new RotateTransform(angle);
                    im.RenderTransformOrigin = center;
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

        #region Background

        public void ApplyBackground()
        {
            if (Settings.Instance.PageBackground == Settings.Background.Default)
            {
                mainScrollView.Background = Consts.DefaultBackground;
                return;
            }

            try
            {
                string imageFileName = string.Empty;

                switch (Settings.Instance.PageBackground)
                {
                    case Settings.Background.Default: mainScrollView.Background = Consts.DefaultBackground; break;
                    case Settings.Background.Blue: imageFileName = "blue"; break;
                    case Settings.Background.Sand: imageFileName = "sand"; break;
                    case Settings.Background.Wooden1: imageFileName = "wooden-1"; break;
                    case Settings.Background.Wooden2: imageFileName = "wooden-2"; break;
                } 

                if (Settings.Instance.PageBackground != Settings.Background.Custom)
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

            Console.WriteLine("MainWindow activated ....");
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


            Console.WriteLine("MainWindow deactivated ....");
#if !UWP

            // Enable touch screen when windows gets deactivated ...
            if (Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(true);

#endif
        }

        private void RibbonWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(true);
            else if ((WindowState == WindowState.Normal || WindowState == WindowState.Maximized) && Settings.Instance.DisableTouchScreenIfInForeground)
                TouchHelper.SetTouchState(false);
        }

        #endregion
    }

    #region Converters

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
    #endregion
}
