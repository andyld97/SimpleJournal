using SimpleJournal.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleJournal.Actions
{
    public class ChildAddedAction : Action
    {
        private readonly UIElement child = null;

        public ChildAddedAction(UIElement child) : base(ActionType.AddedChild)
        {
            this.child = child;
        }

        public override void Apply(DrawingCanvas canvas)
        {
            // Remove added child
            // ToDo: *** Sidebar must be refreshed
            canvas.Children.Remove(child);
        }

        public override void Reverse(DrawingCanvas canvas)
        {
            canvas.Children.Add(child);
        }
    }

    public class ChildRemovedAction : Action
    {
        private readonly UIElement child = null;

        public ChildRemovedAction(UIElement child) : base(ActionType.RemovedChild)
        {
            this.child = child;
        }

        public override void Apply(DrawingCanvas canvas)
        {
            // Add removed child
            // ToDo: *** Sidebar must be refreshed
            canvas.Children.Add(child);
        }

        public override void Reverse(DrawingCanvas canvas)
        {
            canvas.Children.Remove(child);
        }
    }

    public class ShapeChangedAction : Action
    {
        private readonly UIElement handle;
        private Color cOld;
        private Color cNew;
        private readonly double thicknessOld;
        private readonly double thicknessNew;

        public ShapeChangedAction(UIElement handle, Color cOld, Color cNew, double thicknessOld, double thicknessNew) : base(ActionType.ShapeChanged)
        {
            this.handle = handle;
            this.cOld = cOld;
            this.cNew = cNew;
            this.thicknessOld = thicknessOld;
            this.thicknessNew = thicknessNew;
        }

        public override void Apply(DrawingCanvas canvas)
        {
            try
            {
                if (handle is Shape sh)
                {
                    sh.Stroke = new SolidColorBrush(cOld);
                    sh.StrokeThickness = thicknessOld;
                }
                else if (handle is Plot pl)
                {
                    pl.StrokeThickness = thicknessOld;
                    pl.Foreground = cOld;
                    pl.RenderPlot();
                }
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
                if (handle is Shape sh)
                {
                    sh.Stroke = new SolidColorBrush(cNew);
                    sh.StrokeThickness = thicknessNew;
                }
                else if (handle is Plot pl)
                {
                    pl.Foreground = cNew;
                    pl.StrokeThickness = thicknessNew;
                    pl.RenderPlot();
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
