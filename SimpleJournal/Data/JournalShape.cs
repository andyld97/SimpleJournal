using System;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace SimpleJournal.Data
{
    [Serializable]
    public class JournalShape : JournalResource
    {
        public override Type JournalResourceType => Type.Shape;

        public override UIElement ConvertToUiElement()
        {
            try
            {
                Shape shape = (Shape)XamlReader.Parse(Encoding.Default.GetString(Convert.FromBase64String(Encoding.Default.GetString(this.Data))));

                return shape;
            }
            catch (Exception)
            {
                // Silence is golden
                return new UIElement();
            }
        }
    }
}
