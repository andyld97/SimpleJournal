using System.Windows;
using System.Windows.Media;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für DropDownItemTemplate.xaml
    /// </summary>
    public partial class PenDropDownTemplate : Templates.DropDownTemplate
    {
        private bool isTextMarker;

        #region Events
        public delegate void onChangedColorAndSize(Color? c, int sizeIndex);
        public event onChangedColorAndSize OnChangedColorAndSize;
        #endregion   

        #region Ctor
        public PenDropDownTemplate()
        {
            InitializeComponent();
            colorPalette.OnColorChanged += ColorPalette_OnColorChanged;
        }
        #endregion

        public void SetTextMarker()
        {
            isTextMarker = true;
            PenSize.SetTextMarker();
        }

        public void LoadPen(Data.Pen p)
        {
            int index;

            if (isTextMarker)
                index = Consts.TextMarkerSizes.IndexOf(new Size(p.Width, p.Height));
            else
                index = Consts.StrokeSizes.IndexOf(new Size(p.Width, p.Height));

            PenSize.SetColor(p.FontColor.ToColor());
            PenSize.SetIndex(index);
        }

        private void ColorPalette_OnColorChanged(Color color)
        {
            OnChangedColorAndSize?.Invoke(color, -1);
            PenSize.SetColor(color);
            CloseDropDown();
        }

        private void SwitchSize(int button)
        {          
            // Fire event
            OnChangedColorAndSize?.Invoke(null, button);
            CloseDropDown();
        }

        private void PenSize_SwitchtedPenSize(int size)
        {
            SwitchSize(size);
        }
    }
}
