using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Data
{
    [Serializable]
    public class JournalText : JournalResource
    {
        public override Type JournalResourceType => Type.Text;

        public double FontSize = 12;

        public string FontFamily = "Times New Roman";

        public byte A, R, G, B;

        public int RotationAngle = 0;

        public bool IsBold = false;

        public bool IsItalic = false;

        public bool IsUnderlined = false;

        public bool IsStrikeout = false;

        public int ZIndex { get; set; } = 0;

        public override UIElement ConvertToUiElement()
        {
            TextBlock text = new TextBlock() { Text = Encoding.Default.GetString(Data) };
            text.TextWrapping = TextWrapping.Wrap;
            text.SetValue(InkCanvas.LeftProperty, this.Left);
            text.SetValue(InkCanvas.TopProperty, this.Top);
            text.Width = this.Width;
            text.Height = this.Height;
            text.FontSize = FontSize;
            text.FontFamily = new System.Windows.Media.FontFamily(FontFamily);
            text.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(A, R, G, B));

            if (IsBold)
                text.FontWeight = FontWeights.Bold;

            if (IsItalic)
                text.FontStyle = FontStyles.Italic;

            if (IsUnderlined)
                text.TextDecorations.Add(TextDecorations.Underline);

            if (IsStrikeout)
                text.TextDecorations.Add(TextDecorations.Strikethrough);

            text.RenderTransform = new RotateTransform(RotationAngle);
            text.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetZIndex(text, ZIndex);

            return text;
        }
    }
}
