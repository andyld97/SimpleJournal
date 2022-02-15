using SimpleJournal.Data;
using SimpleJournal.Documents.UI.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für TextDesign.xaml
    /// </summary>
    public partial class TextDesign : UserControl
    {
        private TextData currentData = new TextData();
        private bool isInitalized = false;

        public delegate void onChanged(TextData data);
        public event onChanged OnChanged;

        public TextDesign()
        {
            InitializeComponent();

            textColorPicker.SetBorderToRectangle();
        }

        public void Load(TextData data)
        {
            this.currentData = data;
            isInitalized = false;

            numAngle.Value = data.Angle;
            numTextSize.Value = (int)data.FontSize;
            txtContent.Text = data.Content;
            textColorPicker.SelectedColor = data.FontColor;
            cmbFontChooser.Select(new FontFamily(data.FontFamily));
            btnBold.IsChecked = data.IsBold;
            btnItalic.IsChecked = data.IsItalic;
            btnUnderline.IsChecked = data.IsUnderlined;
            btnStrikeThrough.IsChecked = data.IsStrikeout;

            isInitalized = true;
        }

        private void NumTextSize_OnChanged(int oldValue, int newValue)
        {
            if (!isInitalized)
                return;

            currentData.FontSize = newValue;
            OnChanged?.Invoke(currentData);
        }

        private void NumAngle_OnChanged(int oldValue, int newValue)
        {
            if (!isInitalized)
                return;

            currentData.Angle = newValue;
            OnChanged?.Invoke(currentData);
        }

        private void TextColorPicker_ColorChanged(Color c)
        {
            if (!isInitalized)
                return;

            currentData.FontColor = c;
            OnChanged?.Invoke(currentData);
        }

        private void CmbFontChooser_OnFontChanged(FontFamily family)
        {
            if (!isInitalized)
                return;

            currentData.FontFamily = family.ToString();
            OnChanged?.Invoke(currentData);
        }

        private void TxtContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitalized)
                return;

            currentData.Content = txtContent.Text;
            OnChanged?.Invoke(currentData);
        }

        private void BtnBold_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized)
                return;

            currentData.IsBold = btnBold.IsChecked.Value;
            OnChanged?.Invoke(currentData);
        }

        private void BtnItalic_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized)
                return;

            currentData.IsItalic = btnItalic.IsChecked.Value;
            OnChanged?.Invoke(currentData);
        }

        private void BtnItalic_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized)
                return;

            currentData.IsItalic = btnItalic.IsChecked.Value;
            OnChanged?.Invoke(currentData);
        }

        private void BtnUnderline_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized)
                return;

            currentData.IsUnderlined = btnUnderline.IsChecked.Value;
            OnChanged?.Invoke(currentData);
        }

        private void BtnStrikeThrough_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized)
                return;

            currentData.IsStrikeout = btnStrikeThrough.IsChecked.Value;
            OnChanged?.Invoke(currentData);
        }
    }
}
