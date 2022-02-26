using SimpleJournal.Documents.UI;
using System.Windows;
using System.Windows.Media;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für PolygonDropDownTemplate.xaml
    /// </summary>
    public partial class PolygonDropDownTemplate : DropDownTemplate, IPolygonTemplateInfo
    {
        private Color borderColor = Colors.Black;
        private Color backgroundColor = Colors.Transparent;

        public Color BorderColor
        {
            get => borderColor;
            set => borderColor = value;
        }

        public Color BackgroundColor
        {
            get => backgroundColor;
            set => backgroundColor = value;
        }


        public PolygonDropDownTemplate()
        {
            InitializeComponent();

            backgroundColorPicker.SetBorderToRectangle();
            borderBackgroundColorPicker.SetBorderToRectangle();
        }

        private void BorderBackgroundColorPicker_ColorChanged(Color c)
        {
            this.BorderColor = c;
        }

        private void BackgroundColorPicker_ColorChanged(Color c)
        {
            this.BackgroundColor = c;
        }

        private void ChkBackgroundTransparent_Checked(object sender, RoutedEventArgs e)
        {
            this.BackgroundColor = Colors.Transparent;
        }

        private void ChkBackgroundTransparent_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}