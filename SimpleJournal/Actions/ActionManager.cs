using System;
using System.Collections.Generic;

namespace SimpleJournal.Actions
{
    /// <summary>
    /// Manages undo and redo commands for a specified DrawingCanvas
    /// </summary>
    public class ActionManager
    {
        private readonly Stack<SimpleJournal.Actions.Action> undo = new Stack<SimpleJournal.Actions.Action>();
        private readonly Stack<SimpleJournal.Actions.Action> redo = new Stack<SimpleJournal.Actions.Action>();
        private readonly DrawingCanvas owner = null;

        public bool IgnoreEvents { get; private set; } = false;

        public ActionManager(DrawingCanvas canvas)
        {
            owner = canvas ?? throw new ArgumentNullException("canvas");

            // Hang on to canvas adding/remove strokes/childs event
            owner.OnChanged += Owner_OnChanged;
        }

        private void Owner_OnChanged(System.Windows.Ink.StrokeCollection strokes, System.Windows.UIElement child, Action.Type value)
        {
            if ((strokes == null && child == null) || IgnoreEvents)
                return;

            Action action = null;

            switch (value)
            {
                case Action.Type.AddedChild: action = new ChildAddedAction(child); break;
                case Action.Type.AddedStrokes: action = new StrokeAddedAction(strokes); break;
                case Action.Type.RemovedChild: action = new ChildRemovedAction(child); break;
                case Action.Type.RemovedStrokes: action = new StrokeRemovedAction(strokes); break;
            }

            undo.Push(action);
        }

        public void AddSpecialAction<T>(List<T> actions) where T : Action
        {
            undo.Push(new MultipleChangedActions<T>(actions));
        }

        public bool CanUndo()
        {
            return undo.Count > 0;
        }

        public bool CanRedo()
        {
            return redo.Count > 0;
        }

        /// <summary>
        /// Ctrl+Z => Removes last step 
        /// </summary>
        public bool Undo()
        {
            if (!CanUndo())
                return false;

            IgnoreEvents = true;

            // Make the first item on the stack back
            SimpleJournal.Actions.Action current = undo.Pop();
            current.Apply(owner);
            redo.Push(current);

            IgnoreEvents = false;
            return true;
        }

        /// <summary>
        /// Ctrl+W => Repeat last step
        /// </summary>
        public bool Redo()
        {
            if (!CanRedo())
                return false;

            IgnoreEvents = true;

            var action = redo.Pop();
            undo.Push(action);
            action.Reverse(owner);

            IgnoreEvents = false;

            return true;
        }
    }
}
