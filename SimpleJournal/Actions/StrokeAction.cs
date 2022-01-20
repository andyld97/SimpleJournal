using System;
using System.Windows.Ink;

namespace SimpleJournal.Actions
{
    public class StrokeAddedAction : Action
    {
        private readonly StrokeCollection strokes = null;

        public StrokeAddedAction(StrokeCollection strokes) : base(ActionType.AddedStrokes)
        {
            this.strokes = strokes ?? throw new ArgumentNullException("strokes");
        }

        public override void Apply(DrawingCanvas canvas)
        {
            // Remove added strokes
            try
            {
                if (canvas.Strokes != null && canvas.Strokes.Count > 0)
                    canvas.Strokes.Remove(strokes);
            }
            catch (Exception)
            {
                // Make sure app doesn't crashes
            }
        }

        public override void Reverse(DrawingCanvas canvas)
        {
            if (canvas.Strokes != null && canvas.Strokes.Count > 0)
                canvas.Strokes.Add(strokes);
            else
                canvas.Strokes = new StrokeCollection(strokes);
        }
    }

    public class StrokeRemovedAction : Action
    {
        private readonly StrokeCollection strokes = null;

        public StrokeRemovedAction(StrokeCollection strokes) : base(ActionType.RemovedStrokes)
        {
            this.strokes = strokes ?? throw new ArgumentNullException("strokes");
        }

        public override void Apply(DrawingCanvas canvas)
        {
            // Add removed strokes
            if (canvas.Strokes != null && canvas.Strokes.Count > 0)
                canvas.Strokes.Add(strokes);
            else
                canvas.Strokes = new StrokeCollection(strokes);

        }

        public override void Reverse(DrawingCanvas canvas)
        {
            try
            {
                canvas.Strokes.Remove(strokes);
            }
            catch (Exception)
            {
                // Make sure app won't crash 
            }
        }
    }

    public class StrokesChangedAction : Action
    {
        private readonly Stroke st;
        private readonly DrawingAttributes oldAttributes;
        private readonly DrawingAttributes newAttributes;

        public StrokesChangedAction(Stroke st, DrawingAttributes oldAttr, DrawingAttributes newAttr) : base(ActionType.AddedChild)
        {
            this.st = st;
            this.oldAttributes = oldAttr;
            this.newAttributes = newAttr;
        }

        public override void Apply(DrawingCanvas canvas)
        {
            try
            {
                st.DrawingAttributes = oldAttributes;
            }
            catch
            {
                // ignore
            }
        }

        public override void Reverse(DrawingCanvas canvas)
        {
            try
            {
                st.DrawingAttributes = newAttributes;
            }
            catch
            {
                // ignore
            }
        }
    }
}
