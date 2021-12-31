using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    public class CustomListBoxItem : ListBoxItem 
    {
        public UIElement AssociativeRelation { get; } = null;

        public Viewbox Viewport { get; } = null;

        public CustomListBoxItem(UIElement associativeRelation, Viewbox viewPort)
        {
            this.AssociativeRelation = associativeRelation;
            this.Viewport = viewPort;

            Refresh();
        }

        public void Refresh()
        {
            // TextBlock has it's own custom icons, so no refresh required
            if (AssociativeRelation is TextBlock)
                return;

            var element = GeneralHelper.CloneElement(AssociativeRelation);
            element.RenderTransform = new RotateTransform();
            Viewport.Child = element;
        }
    }
}
