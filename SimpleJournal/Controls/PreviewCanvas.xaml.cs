using SimpleJournal.Common;
using SimpleJournal.Documents.UI.Controls;
using SimpleJournal.Documents.UI.Controls.Paper;
using SimpleJournal.Documents.UI.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using Orientation = SimpleJournal.Common.Orientation;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für PreviewCanvas.xaml
    /// </summary>
    public partial class PreviewCanvas : UserControl
    {
        private DrawingCanvas currentCanvas;
        private PaperType currentPaperType = PaperType.Blanco;
        private IPaper paper;
        private readonly DrawingAttributes attributes = new DrawingAttributes();
        private DrawingAttributes old;
        private bool writing;
        private bool enabledWriting;

        /// <summary>
        /// Is not implemented yet, because currently there are no other formats than A4
        /// </summary>
        public Format Format { get; set; }

        public PaperType PaperType
        {
            get => currentPaperType;
            set
            {
                if (currentPaperType != value)
                {
                    currentPaperType = value;

                    // For preview canvas no landsacpe is required
                    Orientation orientation = Orientation.Portrait;

                    // Switch canvas
                    switch (currentPaperType)
                    {
                        case PaperType.Blanco: paper = new Blanco(orientation); break;
                        case PaperType.Chequered: paper = new Chequered(orientation); break;
                        case PaperType.Ruled: paper = new Ruled(orientation); break;
                        case PaperType.Dotted: paper = new Dotted(orientation); break;
                    }

                    // Debug means that this is a preview canvas and e.g. Change will not be affected
                    paper.SetDebug();

                    currentCanvas = paper.Canvas;

                    if (IsInRulerMode)
                    {
                        currentCanvas.EditingMode = InkCanvasEditingMode.None;
                        currentCanvas.SetRulerMode(RulerMode.Normal);
                    }

                    gridPaperContainer.Children.Clear();
                    gridPaperContainer.Children.Add(paper as UIElement);
                }
            }
        }

        public DrawingCanvas Canvas => currentCanvas;

        public IPaper Paper => paper;

        /// <summary>
        /// Enables switch between writing and text marker
        /// </summary>
        public bool EnableWriting
        {
            get => enabledWriting;
            set
            {
                if (value != enabledWriting)
                {
                    btnWrite.Visibility = (value ? Visibility.Visible : Visibility.Collapsed);
                    enabledWriting = value;
                }
            }
        }

        public bool IsInRulerMode { get; set; }

        public DrawingAttributes DrawingAttributes
        {
            get => (writing ? old : currentCanvas.DefaultDrawingAttributes);
            set
            {
                if (writing) 
                    old = value;
                else 
                    currentCanvas.DefaultDrawingAttributes = value;
            }
        }
        
        public PreviewCanvas()
        {
            InitializeComponent();
            PaperType = PaperType.Ruled;
        }

        public void AddChild(UIElement element)
        {
            this.currentCanvas.Children.Add(element);
        }

        public void ClearCanvas()
        {
            if (currentCanvas != null)
            {
                // Only remove childrens if this preview canvas is not used to highlighting.
                // This is necessary to ensure that the text added to this canvas will not be removed if the user clicks on "Clear"
                if (!DrawingAttributes.IsHighlighter)
                    currentCanvas.Children.ClearAll(currentCanvas);

                currentCanvas.Strokes = new StrokeCollection();
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();  
        }

        private void BtnWrite_Click(object sender, RoutedEventArgs e)
        {
            if (writing)
            {
                btnWrite.Content = Properties.Resources.strWrite;
                currentCanvas.DefaultDrawingAttributes = old;
                writing = false;
            }
            else
            {
                btnWrite.Content = Properties.Resources.strHighlight;
                old = currentCanvas.DefaultDrawingAttributes.Clone();
                currentCanvas.DefaultDrawingAttributes = new DrawingAttributes();
                writing = true;
            }
        }
    }
}
