using SimpleJournal.Actions;
using SimpleJournal.Controls;
using SimpleJournal.Controls.Templates;
using SimpleJournal.Data;
using SimpleJournal.Common;
using SimpleJournal.Common.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Action = SimpleJournal.Actions.Action;

namespace SimpleJournal
{
    public class DrawingCanvas : InkCanvas
    {
        #region Public Static Members
        public static DrawingCanvas LastModifiedCanvas = null;
        private static bool change = false;

        public delegate void onChangedDocumentState(bool value);
        public static event onChangedDocumentState OnChangedDocumentState;

        public static bool Change
        {
            get => change;
            set
            {
                if (change != value)
                {
                    change = value;
                    OnChangedDocumentState?.Invoke(value);
                }
            }
        }
        #endregion

        #region Private Members

        private PolygonDropDownTemplate polygonDropDownTemplate = null;
        private bool isInFreeHandDrawingMode = false;
        //private bool isInFreeHandWritingMode = false;
        private bool isInFreeHandPolygonMode = false;
        private bool handleCollectionChanged = true;
        private bool notifiyActionManagerOnCollectionChanged = true;
        private bool preventCallingStrokesChangedTwice = false;
        private bool isPreview = false;

        /// <summary>
        /// Mode: Paste of copied objects
        /// </summary>
        private bool isInCopyMode = false;

        /// <summary>
        /// Mode: Insert a new child generated, e.g. image, text
        /// </summary>
        private bool isInsertMode = false;
        private bool isInFormMode = false;
        private ShapeType ShapeType = ShapeType.Rectangle;
        private PlotMode plotMode = PlotMode.Positive;
        private bool isInPlotMode = false;

        private Point p1 = new Point();
        private Point p2 = new Point();
        private int pointCounter = 0;
        private RulerMode rulerMode = RulerMode.Normal;
        private Line line = new Line();
        private Shape currentShape = new Rectangle();
        private Plot plot = new Plot();
       

        // Members for Ruler
        private StylusPointCollection pointCollection = new StylusPointCollection();
        private Stroke currentStroke = null;

        #endregion

        #region Pubic Properties
        /// <summary>
        /// Gets or sets the stroke collection of this canvas
        /// This property overrides the canvas property when you set the strokeCollection the event StrokesChanged will ALWAYS be applied also too
        /// So this is a fix when a journal is loaded.
        /// </summary>
        public new StrokeCollection Strokes
        {
            get
            {
                return base.Strokes;
            }
            set
            {
                if (value != base.Strokes)
                {
                    base.Strokes = value;

                    // Refresh event 
                    base.Strokes.StrokesChanged += Strokes_StrokesChanged;
                }
            }
        }

        public new ObservableCollection<UIElement> Children { get; } = new ObservableCollection<UIElement>();

        public bool IsInRulerMode { get; private set; } = false;

        public bool PreviewCircleCorrection { get; set; } = false;

        public bool PreviewRotationCorrection { get; set; } = false;
        #endregion

        #region Events
        public delegate void onChanged(StrokeCollection strokes, UIElement child, ActionType value);
        public event onChanged OnChanged;

        public delegate void childElementsSelected(UIElement[] elements);
        public static event childElementsSelected ChildElementsSelected;
        #endregion

        public ActionManager Manager { get; } = null;

        public DrawingCanvas() : this(false)
        { }

        public DrawingCanvas(bool isPreview = false)
        {
            this.isPreview = isPreview;
            RequestBringIntoView += DrawingCanvas_RequestBringIntoView;

            if (LastModifiedCanvas == null)
                LastModifiedCanvas = this;

            AllowDrop = true;
            Strokes.StrokesChanged += Strokes_StrokesChanged;
            SelectionChanged += DrawingCanvas_SelectionChanged;

            // Try to get more shapeness
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            // Manager of actions manges forward and backward actions
            Manager = new ActionManager(this);
            Children.CollectionChanged += Childrens_CollectionChanged;
        }

        public void Dispose()
        {
            // Unassign all events to ensure gc can clean up 
            OnChanged = null;
            OnChangedDocumentState = null;
            OnInsertPositionIsKnown = null;
            OnCopyPositionIsKnown = null;
            ChildElementsSelected = null;

            RequestBringIntoView -= DrawingCanvas_RequestBringIntoView;
            Strokes.StrokesChanged -= Strokes_StrokesChanged;
            SelectionChanged -= DrawingCanvas_SelectionChanged;
            Children.CollectionChanged -= Childrens_CollectionChanged;
            base.Strokes.StrokesChanged -= Strokes_StrokesChanged;
        }


        public void SetDebug(bool state = true)
        {
            isPreview = state;
        }

        #region Event Handling SelectionChanged/CollectionChanged
        private void Childrens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // A new child was added => So set Change to true to make sure that on closing will ask the user for saving
            if (!isPreview)
                Change = true;

            if (!handleCollectionChanged)
                return;

            // Notify manger 
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var ui = (UIElement)item;
                    base.Children.Add(ui);
                }

                if (MainWindow.W_INSTANCE.IsSideBarVisible)
                    MainWindow.W_INSTANCE.preventSelection = false;

                if (notifiyActionManagerOnCollectionChanged)
                    OnChanged.Invoke(GetSelectedStrokes(), (UIElement)e.NewItems[0], ActionType.AddedChild);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var ui = (UIElement)item;
                    base.Children.Remove(ui);
                }

                if (base.Children.Count > 0 && MainWindow.W_INSTANCE.IsSideBarVisible)
                    MainWindow.W_INSTANCE.preventSelection = false;
                else if (MainWindow.W_INSTANCE.IsSideBarVisible)
                    MainWindow.W_INSTANCE.pnlSidebar.Visibility = Visibility.Collapsed;

                if (notifiyActionManagerOnCollectionChanged)
                    OnChanged.Invoke(GetSelectedStrokes(), (UIElement)e.OldItems[0], ActionType.RemovedChild);
            }
        }

        private void DrawingCanvas_SelectionChanged(object sender, EventArgs e)
        {
            // Show sidebar
            // If a child is selected then show it in the sidebar
            ChildElementsSelected?.Invoke(GetSelectedElements().ToArray());
        }

        #endregion

        #region Public Methods (For setting canvas-drawing-mode)

        /// <summary>
        /// Call this on load, because childs were added can't be removed with undo
        /// </summary>
        /// <param name="child"></param>
        public void LoadChildren(UIElement child)
        {
            base.Children.Add(child);
            handleCollectionChanged = false;
            Children.Add(child);
            handleCollectionChanged = true;
        }

        public void SetRulerMode(RulerMode mode)
        {
            rulerMode = mode;
            pointCounter = 0;
            IsInRulerMode = true;
        }

        public void SetFormMode(ShapeType shapeType)
        {
            pointCounter = 0;
            isInFormMode = true;
            ShapeType = shapeType;
            pointCollection.Clear();
            currentShape = null;
        }

        public void SetFormMode()
        {
            isInFormMode = true;

        }

        public void UnsetFormMode()
        {
            isInFormMode = false;
        }

        public void SetPlotMode(PlotMode plotMode)
        {
            isInPlotMode = true;
            this.plotMode = plotMode;
        }

        public void UnsetPlotMode()
        {
            isInPlotMode = false;
        }

        public void SetFreeHandPolygonMode(PolygonDropDownTemplate polygonDropDownTemplate)
        {
            this.polygonDropDownTemplate = polygonDropDownTemplate;

            isInFreeHandPolygonMode = true;
            polyline = new Polyline();
            polylinePoints = new PointCollection();
            polyline.StrokeThickness = 2;
            polyline.Stroke = Brushes.Black;
            notifiyActionManagerOnCollectionChanged = false;
            Children.Add(polyline);
            notifiyActionManagerOnCollectionChanged = true;

            EditingMode = InkCanvasEditingMode.None;
        }

        public void UnsetFreeHandPolygonMode()
        {
            isInFreeHandPolygonMode = false;

            try
            {
                notifiyActionManagerOnCollectionChanged = false;
                Children.Remove(startPointEllipse);
                Children.Remove(polyline);
                notifiyActionManagerOnCollectionChanged = true;
            }
            catch
            {
                // ignore
            }
        }

        public void SetCopyMode()
        {
            isInCopyMode = true;
            EditingMode = InkCanvasEditingMode.None;
        }

        public void UnSetCopyMode()
        {
            isInCopyMode = false;
        }

        public void SetInsertMode()
        {
            isInsertMode = true;
            EditingMode = InkCanvasEditingMode.None;
        }

        public void UnsetInsertMode()
        {
            isInsertMode = false;
        }

        public void UnsetRulerMode()
        {
            IsInRulerMode = false;
            pointCollection = new StylusPointCollection();

            if (pointCounter == 1)
            {
                this.Children.Remove(line);
                DrawingCanvas.LastModifiedCanvas = this;
                DrawingCanvas.Change = true;
            }

            pointCounter = 0;
            line = new Line();
        }

        public void SetFreeHandDrawingMode()
        {
            isInFreeHandDrawingMode = true;
            // isInFreeHandWritingMode = false;
        }

        public void SetFreeHandWritingMode()
        {
            // This method is never used
            isInFreeHandDrawingMode = false;
            // isInFreeHandWritingMode = true;
        }

        public void UnsetFreeHandMode()
        {
            isInFreeHandDrawingMode = false;
            // isInFreeHandWritingMode = false;
        }

        public bool CanUndo() => Manager != null && Manager.CanUndo();

        public bool CanRedo() => Manager != null && Manager.CanRedo();

        public void Undo()
        {
            if (Manager != null && Manager.Undo())
                DrawingCanvas.Change = true;
        }

        public void Redo()
        {
            if (Manager != null && Manager.Redo())
                DrawingCanvas.Change = true;
        }

        public int Search(string text)
        {
            string[] searchResult = StartAnalyzingProcess(Strokes, Operation.TextSearch, new string[] { Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(text)) });
            // This process call should return serialized strokes which should be selected
            int strokesFound = 0;

            foreach (string toParse in searchResult)
            {
                string[] numbers = toParse.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                StrokeCollection toSelected = new StrokeCollection();
                foreach (string number in numbers)
                {
                    int result = int.Parse(number);
                    if (result != -1)
                        toSelected.Add(Strokes[result]);
                }
                if (toSelected.Count != 0)
                    strokesFound++;
                Select(toSelected);
                break;
            }

            return strokesFound;
        }

        public bool ConvertSelectedStrokesToShape()
        {
            int count = Children.Count;
            var sc = GetSelectedStrokes().Clone();
            StartConvertingProcess(GetSelectedStrokes());

            if (Children.Count == count)
            {
                // Recreate strokes
                Strokes.Add(sc);
                return false;
            }

            return true;
        }

        #endregion

        #region Shape and Text Recognization

        public enum Operation
        {
            Shape,
            Text,
            TextSearch
        }

        public static string[] StartAnalyzingProcess(StrokeCollection sc, Operation operation, string[] additionalArguments = null)
        {
            List<string> lines = new List<string>();

            try
            {
                // Convert StrokeCollection to byte[] to convert byte[] to base64 encoded string to pass to process
                byte[] result = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    sc.Save(ms);
                    result = ms.ToArray();
                }

                // Generate and format arguments
                string base64EncodedStrokes = Convert.ToBase64String(result);

                // Write it to a temporary file
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SimpleJournal", "Analyser.temp");
                if (!System.IO.Directory.Exists(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SimpleJournal")))
                {
                    try
                    {
                        var pt = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SimpleJournal");
                        System.IO.Directory.CreateDirectory(pt);
                    }
                    catch
                    {
                        // ignore
                    }
                }
                System.IO.File.WriteAllText(tempPath, base64EncodedStrokes);

                string args = string.Empty;
                string operationArgument = "d";

                // Determine string from operation enum
                switch (operation)
                {
                    case Operation.Shape: operationArgument = "d"; break;
                    case Operation.Text: operationArgument = "w"; break;
                    case Operation.TextSearch: operationArgument = "ts"; break;
                }

                if (additionalArguments != null)
                {
                    foreach (string arg in additionalArguments)
                        args += $"{arg} ";
                }

                // Maybe add additionalArguments to argumentChain
                string argumetChain = operationArgument; // $"{base64EncodedStrokes} {operationArgument}";
                if (!string.IsNullOrEmpty(args))
                    argumetChain += $" {args}";

                // Pass argumentChain to other process (x86)
                // process will do analysis => and return the result (serialized shape)
                string path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Analyzer.exe");
                Process analyzingProcess = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = argumetChain,
                        CreateNoWindow = false,
                        FileName = path,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };

                analyzingProcess.Start();
                while (!analyzingProcess.StandardOutput.EndOfStream)
                {
                    string currentLine = analyzingProcess.StandardOutput.ReadLine();
                    if (string.IsNullOrEmpty(currentLine))
                        continue;
                    else
                        lines.Add(currentLine);
                }

                string error = analyzingProcess.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Properties.Resources.strErrorMessageAnalyzingStrokes, e.Message), Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return lines.ToArray();
        }

        private void StartConvertingProcess(StrokeCollection sc)
        {
            string[] result = StartAnalyzingProcess(sc, Operation.Shape);

            if (result.Length == 0)
            {
                preventCallingStrokesChangedTwice = true;
                Strokes.Remove(sc);
                preventCallingStrokesChangedTwice = false;
                return;
            }

            string encodedLine = System.Text.Encoding.Default.GetString(Convert.FromBase64String(result[0])).Replace(Environment.NewLine, string.Empty);
            string[] data = encodedLine.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            // 0 is serialized Shape
            // 1 is serialized Rect
            if (data.Length != 2)
            {
                preventCallingStrokesChangedTwice = true;
                Strokes.Remove(sc);
                preventCallingStrokesChangedTwice = false;
                return;
            }

            Rect location = Serialization.ReadString<Rect>(data[1].Trim(), System.Text.Encoding.Default);
            StringReader stringReader = new StringReader(data[0]);
            XmlReader xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());
            var elem = (UIElement)XamlReader.Load(xmlReader);

            if (elem is Shape sh)
            {
                sh.StrokeThickness = DefaultDrawingAttributes.Width;
                sh.Stroke = new SolidColorBrush(DefaultDrawingAttributes.Color);

                if (isPreview)
                {
                    if (PreviewCircleCorrection && (sh is Ellipse el))
                    {
                        if (el.Width != el.Height)
                        {
                            double center = (el.Width + el.Height) / 2;
                            el.Width = center;
                            el.Height = center;
                        }
                    }

                    if (PreviewRotationCorrection)
                    {
                        sh.RenderTransform = new RotateTransform(0);
                        sh.SetValue(InkCanvas.LeftProperty, location.Left);
                        sh.SetValue(InkCanvas.TopProperty, location.Top);
                    }
                }
                else
                {
                    if (Settings.Instance.UseCircleCorrection && (sh is Ellipse))
                    {
                        var el = sh as Ellipse;
                        if (el.Width != el.Height)
                        {
                            double center = (el.Width + el.Height) / 2;
                            el.Width = center;
                            el.Height = center;
                        }
                    }

                    if (Settings.Instance.UseRotateCorrection)
                    {
                        sh.RenderTransform = new RotateTransform(0);
                        sh.SetValue(InkCanvas.LeftProperty, location.Left);
                        sh.SetValue(InkCanvas.TopProperty, location.Top);
                    }
                }
            }

            // Make sure not to call this event twice
            preventCallingStrokesChangedTwice = true;
            Strokes.Remove(sc);
            preventCallingStrokesChangedTwice = false;

            Children.Add(elem);
            bool childAdded = true;

            if (!childAdded)
            {
                preventCallingStrokesChangedTwice = true;
                Strokes.Remove(sc);
                preventCallingStrokesChangedTwice = false;
            }
        }

        private void Strokes_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (preventCallingStrokesChangedTwice)
                return;

            // isInFreeHandWritingMode is currently not supported
            if (isInFreeHandDrawingMode)
                StartConvertingProcess(e.Added);
            else if (!IsInRulerMode)
            {
                if (e.Added.Count > 0)
                {
                    OnChanged?.Invoke(e.Added, null, ActionType.AddedStrokes);
                }
                if (e.Removed.Count > 0)
                {
                    OnChanged?.Invoke(e.Removed, null, ActionType.RemovedStrokes);
                }
            }
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            base.OnStrokeCollected(e);
            LastModifiedCanvas = this;

            if (!isPreview)
                Change = true;
        }

        protected override void OnStrokeErased(RoutedEventArgs e)
        {
            base.OnStrokeErased(e);
            LastModifiedCanvas = this;

            if (!isPreview)
                Change = true;
        }

        #endregion

        #region Prevent Jumping from ScrollViewer
        private void DrawingCanvas_RequestBringIntoView(object sender, System.Windows.RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        #region Ruler, Forms, Plot u {Paste, Insert} Handling
        public delegate void onCopyPositionIsKnown(Point e);
        public event onCopyPositionIsKnown OnCopyPositionIsKnown;

        public delegate void onInsertPositionIsKnown(Point e);
        public event onInsertPositionIsKnown OnInsertPositionIsKnown;

        #region Polygon Drawing

        private Polyline polyline;
        private PointCollection polylinePoints;
        private bool drawOnMove = false;
        private Ellipse startPointEllipse = new Ellipse();

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (isInFreeHandPolygonMode)
            {
                if (Children.Contains(startPointEllipse) && GeneralHelper.BoundsRelativeTo(startPointEllipse, this).Contains(e.GetPosition(this)))
                {
                    notifiyActionManagerOnCollectionChanged = false;
                    Children.Remove(startPointEllipse);
                    Children.Remove(polyline);
                    notifiyActionManagerOnCollectionChanged = true;

                    Polygon tmpPolygon = new Polygon
                    {
                        StrokeThickness = DefaultDrawingAttributes.Height,
                        Stroke = new SolidColorBrush(polygonDropDownTemplate.BorderColor),
                        Points = polylinePoints.Clone()
                    };
                    polylinePoints.Clear();

                    // Lowest X and lowest Y Points needed to be calculated to get the actual point in the canvas
                    double lowestX = polyline.Points[0].X;
                    double lowestY = polyline.Points[0].Y;

                    for (int i = 1; i < polyline.Points.Count; i++)
                    {
                        Point current = polyline.Points[i];

                        if (current.X < lowestX)
                            lowestX = current.X;

                        if (current.Y < lowestY)
                            lowestY = current.Y;
                    }

                    tmpPolygon.SetValue(DrawingCanvas.LeftProperty, lowestX);
                    tmpPolygon.SetValue(DrawingCanvas.TopProperty, lowestY);
                    tmpPolygon.Stretch = Stretch.Fill;

                    drawOnMove = false;
                    tmpPolygon.Fill = new SolidColorBrush(polygonDropDownTemplate.BackgroundColor);

                    Children.Add(tmpPolygon);

                    // Reset all values
                    SetFreeHandPolygonMode(polygonDropDownTemplate);
                }
                else
                {
                    polylinePoints.Add(e.GetPosition(this));
                    polyline.Points = polylinePoints.Clone();
                    polyline.Stroke = new SolidColorBrush(polygonDropDownTemplate.BorderColor);
                    polyline.StrokeThickness = DefaultDrawingAttributes.Height;

                    if (polyline.Points.Count == 1)
                    {
                        startPointEllipse = new Ellipse
                        {
                            Width = 10,
                            Height = 10,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            Fill = new SolidColorBrush { Color = polygonDropDownTemplate.BackgroundColor }
                        };
                        startPointEllipse.Margin = new Thickness(left: polyline.Points[0].X - startPointEllipse.Width / 2, top: polyline.Points[0].Y - startPointEllipse.Height / 2, right: 0, bottom: 0);
                        notifiyActionManagerOnCollectionChanged = false;
                        Children.Add(startPointEllipse);
                        notifiyActionManagerOnCollectionChanged = true;
                    }

                    drawOnMove = true;
                }
            }
        }
        #endregion

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var pos = e.GetPosition(this);

            if (isInCopyMode)
            {
                isInCopyMode = false;
                // Restore EditingMode is not necessary, the caller needs to handle this

                OnCopyPositionIsKnown?.Invoke(pos);
                return;
            }

            if (isInsertMode)
            {
                isInsertMode = false;
                // Restore EditingMode is not necessary, the caller needs to handle this

                OnInsertPositionIsKnown?.Invoke(pos);
                return;
            }


            if (!IsInRulerMode && !isInFormMode && !isInPlotMode)
                return;

            if (pointCounter == 0)
            {
                p1 = pos;
                pointCollection.Add(new StylusPoint(p1.X, p1.Y));
                pointCounter++;
            }
            else if (pointCounter == 1)
            {
                p2 = pos;

                if (!isInFormMode && !isInPlotMode)
                {
                    // Add stroke
                    if (pointCollection.Count == 2)
                        pointCollection.RemoveAt(1);
                    pointCollection.Add(new StylusPoint(p2.X, p2.Y));

                    if (Settings.Instance.UseRulerCompensation)
                    {
                        var firstPoint = pointCollection.FirstOrDefault();
                        var secondPoint = pointCollection.LastOrDefault();

                        double xDiff = Math.Abs(firstPoint.X - secondPoint.X);
                        double yDiff = Math.Abs(firstPoint.Y - secondPoint.Y);

                        if (xDiff > yDiff)
                        {
                            // horizontal
                            if (yDiff <= Consts.RulerCompensationOffset)
                            { 
                                secondPoint.Y = firstPoint.Y;
                                pointCollection[pointCollection.Count - 1] = secondPoint;
                            }
                        }
                        else if (yDiff > xDiff)
                        {
                            // vertical
                            if (xDiff <= Consts.RulerCompensationOffset)
                            {
                                secondPoint.X = firstPoint.X;
                                pointCollection[pointCollection.Count - 1] = secondPoint;
                            }
                        }
                    }

                    if (rulerMode == RulerMode.Normal)
                    {
                        Strokes.Remove(currentStroke);
                        var stroke = new System.Windows.Ink.Stroke(pointCollection) { DrawingAttributes = DefaultDrawingAttributes.Clone() };
                        Strokes.Add(stroke);

                        // Make sure Action-Manager is notfiyed about this change, because he can't be notifyed when the user is moving the mouse,
                        // because then everytime when a stroke is added and removed it will be notified.
                        OnChanged?.Invoke(new StrokeCollection() { stroke }, null, ActionType.AddedStrokes);

                        // Make sure save dialog will also ask only if a stroke was drown in ruler mode
                        DrawingCanvas.Change = true;
                    }
                    else
                        OnChanged?.Invoke(null, line, ActionType.AddedChild);

                    // Reset ruler
                    pointCounter = 0;
                    p1 = new Point();
                    p2 = new Point();
                    pointCollection = new StylusPointCollection();
                    currentStroke = null;
                    line = new Line();
                }
                else if (isInFormMode)
                {
                    // Remove MouseLeftButton event after adding the control finally (to ensure the ruler works properly again on the border of this control - after adding the control, the events are properly working again)
                    Children.Last().MouseLeftButtonDown -= Control_MouseLeftButtonDown;
                    OnChanged?.Invoke(null, Children.Last(), ActionType.AddedChild);

                    // Add rectangle
                    pointCounter = 0;
                    p1 = new Point();
                    p2 = new Point();
                    pointCollection = new StylusPointCollection();
                    currentShape = null;
                    DrawingCanvas.Change = true;
                }
                else if (isInPlotMode)
                {
                    // Remove MouseLeftButton event after adding the control finally (to ensure the ruler works properly again on the border of this control - after adding the control, the events are properly working again)
                    Children.Last().MouseLeftButtonDown -= Control_MouseLeftButtonDown;
                    OnChanged?.Invoke(null, Children.Last(), ActionType.AddedChild);

                    // Add rectangle
                    pointCounter = 0;
                    p1 = new Point();
                    p2 = new Point();
                    pointCollection = new StylusPointCollection();

                    plot = new Plot();
                    DrawingCanvas.Change = true;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (drawOnMove && isInFreeHandPolygonMode)
            {
                polyline.Points = polylinePoints.Clone();
                polyline.Points.Add(e.GetPosition(this));
                return;
            }

            if (!IsInRulerMode && !isInFormMode && !isInPlotMode)
                return;

            var pos = e.GetPosition(this);
            if (pointCounter == 1 && e.LeftButton == MouseButtonState.Released)
            {
                var p2 = e.GetPosition(this);
                if (pointCollection.Count == 2)
                    pointCollection.RemoveAt(1);

                pointCollection.Add(new StylusPoint(p2.X, p2.Y));

                if (isInPlotMode)
                {
                    notifiyActionManagerOnCollectionChanged = false;
                    Children.Remove(plot);

                    plot = new Plot
                    {
                        StrokeThickness = Math.Round(DefaultDrawingAttributes.Width + DefaultDrawingAttributes.Height / 2.0),
                        Foreground = DefaultDrawingAttributes.Color
                    };

                    Point pm1 = new Point(pointCollection.First().X, pointCollection.First().Y);
                    Point pm2 = new Point(pointCollection[1].X, pointCollection[1].Y);

                    double xDiff = pm1.X - pm2.X;
                    double yDiff = pm1.Y - pm2.Y;

                    plot.SetValue(DrawingCanvas.LeftProperty, (xDiff >= 0 ? pm2.X : pm1.X));
                    plot.SetValue(DrawingCanvas.TopProperty, (yDiff >= 0 ? pm2.Y : pm1.Y));
                    plot.Width = Math.Abs(xDiff);
                    plot.Height = Math.Abs(yDiff);

                    plot.DrawingDirection = (xDiff < 0 ? Direction.Left : Direction.Right);
                    plot.DrawingMode = plotMode;

                    // Ensure that if the mouse/pointer lands on the rectangle, that the event will be redirected to OnMouseLeftButtonDown!
                    plot.MouseLeftButtonDown += Control_MouseLeftButtonDown;

                    Children.Add(plot);
                    notifiyActionManagerOnCollectionChanged = true;
                }
                else if (!isInFormMode)
                {
                    if (rulerMode == RulerMode.Normal)
                    {
                        if (currentStroke == null)
                        {
                            currentStroke = new Stroke(pointCollection) { DrawingAttributes = DefaultDrawingAttributes.Clone() };
                            Strokes.Add(currentStroke);
                        }
                        else
                        {
                            Strokes.Remove(currentStroke);
                            Strokes.Add(currentStroke);
                        }
                    }
                    else
                    {
                        notifiyActionManagerOnCollectionChanged = false;
                        if (!Children.Contains(line))
                            Children.Add(line);
                        notifiyActionManagerOnCollectionChanged = true;

                        var points = pointCollection[0].ToPoint().SortPoints(pointCollection[1].ToPoint());

                        line.X1 = points.Item1.X;
                        line.Y1 = points.Item1.Y;
                        line.X2 = points.Item2.X;
                        line.Y2 = points.Item2.Y;

                        // *** Workaround to make sure the line can be easily accessed via user (no big selection rectangle)
                        line.Stretch = Stretch.Fill;
                        line.SetValue(InkCanvas.LeftProperty, Math.Min(line.X1, line.X2) - 1);
                        line.SetValue(InkCanvas.TopProperty, Math.Min(line.Y1, line.Y2) - line.StrokeThickness / 2 - 1);

                        // *** </Workaround>
                        switch (rulerMode)
                        {
                            case RulerMode.Dottet:
                                {
                                    line.StrokeDashArray = Consts.LineStrokeDottetDashArray;
                                    line.StrokeDashCap = PenLineCap.Round;
                                }
                                break;
                            case RulerMode.Dashed:
                                {
                                    line.StrokeDashArray = Consts.LineStrokeDashedDashArray;
                                    line.StrokeDashCap = PenLineCap.Flat;
                                }
                                break;
                        }

                        line.StrokeDashOffset = Consts.DefaultLineStrokeDashOffset;
                        line.Stroke = new SolidColorBrush(DefaultDrawingAttributes.Color);
                        line.StrokeThickness = DefaultDrawingAttributes.Height;
                    }
                }
                else
                {
                    notifiyActionManagerOnCollectionChanged = false;
                    bool isPolygon = false;
                    Children.Remove(currentShape);
                    
                    switch (ShapeType)
                    {
                        case ShapeType.Rectangle: currentShape = new Rectangle(); break;
                        case ShapeType.Circle: currentShape = new Ellipse(); break;
                        case ShapeType.TriangleEquiliteral:
                        case ShapeType.TriangleEquiliteral180:
                        case ShapeType.TriangleLeft180:
                        case ShapeType.TriangleRight180:
                        case ShapeType.TriangleLeft90:
                        case ShapeType.TriangleRight90:
                        case ShapeType.Trapeze:
                            {
                                currentShape = new Polygon();
                                isPolygon = true;
                            }
                            break;
                    }

          
                    Point pm1 = new Point(pointCollection.First().X, pointCollection.First().Y);
                    Point pm2 = new Point(pointCollection[1].X, pointCollection[1].Y);

                    double xDiff = pm1.X - pm2.X;
                    double yDiff = pm1.Y - pm2.Y;
                    double w = Math.Abs(xDiff);
                    double h = Math.Abs(yDiff);

                    currentShape.SetValue(InkCanvas.LeftProperty, (xDiff >= 0 ? pm2.X : pm1.X));
                    currentShape.SetValue(InkCanvas.TopProperty, (yDiff >= 0 ? pm2.Y : pm1.Y));
                    currentShape.Stroke = new SolidColorBrush(DefaultDrawingAttributes.Color);
                    currentShape.StrokeThickness = DefaultDrawingAttributes.Width + DefaultDrawingAttributes.Height / 2.0;

                    if (currentShape is Polygon poly)
                        poly.Points.Clear();

                    if (isPolygon && currentShape is Polygon polygon)
                    {                        
                        switch (ShapeType)
                        {
                            case ShapeType.TriangleEquiliteral:
                                {
                                    polygon.Points.Add(new Point(w / 2.0, 0));
                                    polygon.Points.Add(new Point(0, h));
                                    polygon.Points.Add(new Point(w, h));
                                }
                                break;
                            case ShapeType.TriangleEquiliteral180:
                                {
                                    polygon.Points.Add(new Point(0, 0));
                                    polygon.Points.Add(new Point(w, 0));
                                    polygon.Points.Add(new Point(w / 2.0, h));
                                }
                                break;
                            case ShapeType.TriangleLeft90:
                                {
                                    polygon.Points.Add(new Point(0, 0));
                                    polygon.Points.Add(new Point(0, h));
                                    polygon.Points.Add(new Point(w, h));
                                }
                                break;
                            case ShapeType.TriangleRight90:
                                {
                                    polygon.Points.Add(new Point(w, 0));
                                    polygon.Points.Add(new Point(0, h));
                                    polygon.Points.Add(new Point(w, h));
                                }
                                break;
                            case ShapeType.TriangleLeft180:
                                {
                                    polygon.Points.Add(new Point(0, 0));
                                    polygon.Points.Add(new Point(w, 0));
                                    polygon.Points.Add(new Point(0, h));
                                }
                                break;
                            case ShapeType.TriangleRight180:
                                {
                                    polygon.Points.Add(new Point(0, 0));
                                    polygon.Points.Add(new Point(w, 0));
                                    polygon.Points.Add(new Point(w, h));
                                }
                                break;
                            case ShapeType.Trapeze:
                                {
                                    polygon.Points.Add(new Point(0, h));
                                    polygon.Points.Add(new Point(Consts.TrapezeOffset, 0));
                                    polygon.Points.Add(new Point(w, 0));
                                    polygon.Points.Add(new Point(w - Consts.TrapezeOffset, h));
                                }
                                break;
                        }
                    }

                    if (!isPolygon)
                    {
                        currentShape.Width = w;
                        currentShape.Height = h;
                    }

                    // Ensure that if the mouse/pointer lands on the rectangle, that the event will be redirected to OnMouseLeftButtonDown!
                    currentShape.MouseLeftButtonDown += Control_MouseLeftButtonDown;

                    Children.Add(currentShape);
                    notifiyActionManagerOnCollectionChanged = true;
                }
            }
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
        }

        #endregion

        #region Try Handling Image Resizing Problem / Entf-Key implementation

        public delegate void removeElementFromSidebar(List<UIElement> elem);
        public event removeElementFromSidebar RemoveElementFromSidebar;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete && GetSelectedElements().Count > 0)
            {
                e.Handled = true;
                // Handle to make sure object will disapear from object bar (side bar)
                List<UIElement> temp = new List<UIElement>();
                foreach (UIElement child in GetSelectedElements())
                {
                    temp.Add(child);
                }
                foreach (UIElement elem in temp)
                    Children.Remove(elem);

                RemoveElementFromSidebar?.Invoke(temp);
            }
            else
                base.OnPreviewKeyDown(e);
        }

        protected override void OnSelectionMoving(InkCanvasSelectionEditingEventArgs e)
        {
            base.OnSelectionMoving(e);
            DrawingCanvas.Change = true;

            List<PropertyChangedAction> actions = new List<PropertyChangedAction>();

            // Idea:
            // Get current positions and calculate the new ones to get all necessary values for the PropertyChangedAction
            foreach (UIElement element in GetSelectedElements())
            {
                Point currentPosition = GeneralHelper.DeterminePointFromUIElement(element, this);
                Point selectionStart = new Point(e.OldRectangle.Left, e.OldRectangle.Top);
                Point newPosition = new Point(e.NewRectangle.X + Math.Abs(selectionStart.X - currentPosition.X), e.NewRectangle.Y + Math.Abs(selectionStart.Y - currentPosition.Y));

                PropertyChangedAction pca = new PropertyChangedAction(newPosition, currentPosition, new Action<object>((object value) =>
                {
                    if (value is Point p)
                    {
                        element.SetValue(InkCanvas.LeftProperty, p.X);
                        element.SetValue(InkCanvas.TopProperty, p.Y);
                    }
                }));

                // Add pca to action manager
                actions.Add(pca);
            }

            foreach (Stroke str in GetSelectedStrokes())
            {
                Point currentPosition = new Point(str.GetBounds().Left, str.GetBounds().Top);
                Point selectionStart = new Point(e.OldRectangle.Left, e.OldRectangle.Top);
                Point newPosition = new Point(e.NewRectangle.X + Math.Abs(selectionStart.X - currentPosition.X), e.NewRectangle.Y + Math.Abs(selectionStart.Y - currentPosition.Y));

                PropertyChangedAction pca = new PropertyChangedAction(newPosition, currentPosition, new Action<object>((object value) =>
                {
                    if (value is Point p)
                    {
                        StylusPointCollection collection = new StylusPointCollection();
                        Point pos = new Point(str.GetBounds().Left, str.GetBounds().Top);

                        foreach (StylusPoint pt in str.StylusPoints)
                        {
                            collection.Add(new StylusPoint()
                            {
                                PressureFactor = pt.PressureFactor,
                                X = p.X + Math.Abs(pos.X - pt.X),
                                Y = p.Y + Math.Abs(pos.Y - pt.Y)
                            });
                        }
                        str.StylusPoints = collection;
                    }
                }));

                // Add pca to action manager
                actions.Add(pca);
            }

            Manager.AddSpecialAction(actions);
        }
        #endregion
    }
}
