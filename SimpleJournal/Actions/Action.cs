namespace SimpleJournal.Actions
{
    /// <summary>
    /// Describes a user action on the canvas: Add/Remove Child/Stroke
    /// </summary>
    public abstract class Action
    {
        public Type ActionType { get; } = Type.Default;

        public enum Type
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

        public Action(Type type)
        {
            this.ActionType = type;
        }

        public abstract void Apply(DrawingCanvas canvas);

        public abstract void Reverse(DrawingCanvas canvas);
    }
}
