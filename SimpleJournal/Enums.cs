namespace SimpleJournal
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

    public enum Format
    {
        A4
    }

    public enum PaperType
    {
        Blanco,
        Chequeued,
        Ruled,
        Dotted
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

    public enum PlotMode
    {
        Positive,
        Negative
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
}
