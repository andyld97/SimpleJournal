using System.Windows.Media;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für SelectDropDownTemplate.xaml
    /// </summary>
    public partial class SelectDropDownTemplate : DropDownTemplate
    {
        public delegate void onColorAndSizeChanged(Color? c, int size);
        public event onColorAndSizeChanged OnColorAndSizeChanged;

        public SelectDropDownTemplate()
        {
            InitializeComponent();
            this.colorPalette.OnColorChanged += ColorPalette_OnColorChanged;
        }

        private void ColorPalette_OnColorChanged(Color color)
        {
            OnColorAndSizeChanged?.Invoke(color, -1);
            PenSize.SetColor(color);
            CloseDropDown();
        }

        private void PenSize_SwitchtedPenSize(int size)
        {
            OnColorAndSizeChanged(null, size);
            CloseDropDown();
        }
    }
}
