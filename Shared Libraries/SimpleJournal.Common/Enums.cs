namespace SimpleJournal.Common
{
    public enum Tools
    {
        Pencil1 = 1,
        Pencil2 = 2,
        Pencil3 = 3,
        Pencil4 = 4,
        Select = 5,
        RubberStrokes = 6,
        RubberFree = 7,
        Ruler = 8,
        Recognization = 9,
        TextMarker = 10,
        FreeHandPolygon = 11,
        Form = 12,
        CooardinateSystem = 13
    }

    /// <summary>
    /// Type of this resource
    /// </summary>
    public enum JournalResourceType
    {
        /// <summary>
        /// Image type
        /// </summary>
        Image = 0,

        /// <summary>
        /// Text type
        /// </summary>
        Text = 1,

        /// <summary>
        /// A rectangle, circle, ellipse, poligon  or something else which derrivered from shape
        /// </summary>
        Shape = 2,

        /// <summary>
        /// Simple plot
        /// </summary>
        Plot = 3
    }

    public enum Format
    {
        A4
    }

    public enum Orientation
    {
        Landscape,
        Portrait, // default
    }

    public enum PaperType
    {
        Blanco,
        Chequeued,
        Ruled,
        Dotted,
        Custom
    }

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

    public enum ShapeType
    {
        Rectangle,
        Circle,
        TriangleEquiliteral,
        TriangleLeft90,
        TriangleRight90,
        TriangleLeft180,
        TriangleRight180,
        TriangleEquiliteral180,
        Trapeze
    }

    public enum Operation
    {
        Shape,
        Text,
        TextSearch
    }

    public enum PlotMode
    {
        Positive,
        Negative,
        AxisXPosNegYPos
    }

    public enum Direction
    {
        Left,
        Right
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

    public enum ExportMode
    {
        AllPages,
        CurrentPage,
        SelectedPageRange,
        SinglePage
    }

    public enum ProgressState
    {
        Start,
        Completed,
    }

    public enum StateType
    {
        Saving,
        ExportPDF,
        Export,
        Printing,
    }

    public enum ActionType
    {
        AddedChild,
        RemovedChild,
        AddedStrokes,
        RemovedStrokes,
        PropertyChanged,
        StrokeChanged,
        ShapeChanged,
        Default
    }

    public enum TicketStatus
    {
        OnHold,
        Prepearing,
        InProgress,
        Failed,
        Saving,
        Completed,
        Canceld
    }

    public enum PdfAction
    {
        Reading,
        PageRange_WritingPage,
        PagesALL_WritingPage,
        Saving
    }

    public enum Unit
    {
        B = 1,
        KB = 2,
        MB = 3,
        GB = 4,
        TB = 5
    }

    public enum Stretch
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    }
}