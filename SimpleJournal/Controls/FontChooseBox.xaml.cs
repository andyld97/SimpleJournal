using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für FontChooseBox.xaml
    /// </summary>
    public partial class FontChooseBox : UserControl
    {
        public delegate void onFontChanged(FontFamily family);
        public event onFontChanged OnFontChanged;

        public FontChooseBox()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cmbFont.SelectedItem;
            if (item is FontFamily ff)
                OnFontChanged?.Invoke(ff);
        }

        public void Select(FontFamily ff)
        {
            cmbFont.SelectedItem = ff;
        }
    }
}
