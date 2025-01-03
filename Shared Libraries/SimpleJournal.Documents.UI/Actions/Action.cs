﻿using SimpleJournal.Common;
using SimpleJournal.Documents.UI.Controls;

namespace SimpleJournal.Documents.UI.Actions
{
    /// <summary>
    /// Describes a user action on the canvas: Add/Remove Child/Stroke
    /// </summary>
    public abstract class Action
    {
        public ActionType ActionType { get; } = ActionType.Default;

        public Action(ActionType type)
        {
            this.ActionType = type;
        }

        public abstract void Apply(DrawingCanvas canvas);

        public abstract void Reverse(DrawingCanvas canvas);
    }
}
