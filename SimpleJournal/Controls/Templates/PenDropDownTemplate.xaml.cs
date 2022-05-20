using SimpleJournal.Documents.UI.Extensions;
using System.Collections.Generic;
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

        private int FindIndex(List<Common.Data.Size> list, Common.Data.Size size)
        {
            int index = 0;
            bool notFound = true;
            foreach (var item in list)
            {
                if (item.Width == size.Width && item.Height == size.Height)
                {
                    notFound = false;
                    break;
                }
                else
                    index += 1;
            }

            if (notFound)
                return -1;

            return index;
        }

        public void LoadPen(Data.Pen p)
        {
            int index;

            if (isTextMarker)
                index = FindIndex(Documents.UI.Consts.TextMarkerSizes, new Common.Data.Size(p.Width, p.Height));
            else
                index = FindIndex(Documents.UI.Consts.StrokeSizes, new Common.Data.Size(p.Width, p.Height));

            var fontColor = p.FontColor.ToColor();
            PenSize.SetColor(fontColor);
            PenSize.SetIndex(index);
            colorPalette.SetSelectedColor(fontColor);
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
