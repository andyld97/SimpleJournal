using SimpleJournal.Documents.UI.Helper;
using System.Windows;
using System.Windows.Controls;

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
            if (AssociativeRelation is TextBlock text)
            {
                ToolTip = text.Text;
                return;
            }

            var element = UIHelper.CloneElement(AssociativeRelation);

            /* This leads to missing rotation in the sidebar, but the question is why, because
             * actually we want to see the rotation also in the sidebar, I think.
             * If this behavior should be changed, we can just set element.RenderTransform to null */
            // element.RenderTransform = new RotateTransform();

            Viewport.Child = element;
        }
    }
}
