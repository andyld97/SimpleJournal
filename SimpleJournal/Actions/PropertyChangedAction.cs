using System;
using System.Collections.Generic;

namespace SimpleJournal.Actions
{
    public class PropertyChangedAction : Action
    {
        private readonly object oldValue = null;
        private readonly object newValue = null;
        private readonly Action<object> applyValueToProperty = null;

        public PropertyChangedAction(object newValue, object oldValue, Action<object> applyValueToProperty) : base(Common.ActionType.PropertyChanged)
        {
            this.newValue = newValue;
            this.oldValue = oldValue;
            this.applyValueToProperty = applyValueToProperty;
        }

        public override void Apply(DrawingCanvas canvas)
        {
            applyValueToProperty?.Invoke(oldValue);
        }

        public override void Reverse(DrawingCanvas canvas)
        {
            applyValueToProperty?.Invoke(newValue);
        }
    }

    public class MultipleChangedActions<T> : Action where T : Action
    {
        private readonly List<T> actions = null;

        public MultipleChangedActions(List<T> actions) : base(Common.ActionType.PropertyChanged)
        {
            this.actions = actions;
        }

        public override void Apply(DrawingCanvas canvas)
        {
            foreach (Action a in actions)
            {
                a.Apply(canvas);
            }
        }

        public override void Reverse(DrawingCanvas canvas)
        {
            foreach (Action a in actions)
            {
                a.Reverse(canvas);
            }
        }
    }
}
