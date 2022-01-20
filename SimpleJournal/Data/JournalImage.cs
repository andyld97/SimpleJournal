using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Data
{
    /// <summary>
    /// A journal image resouce
    /// </summary>
    public class JournalImage : JournalResource
    {
        public override Type JournalResourceType => Type.Image;

        public int RotationAngle { get; set; } = 0;

        public bool IsUniform { get; set; } = true;

        public override UIElement ConvertToUiElement()
        {
            Image img = new Image() { Source = GeneralHelper.LoadImageFromBase64(Convert.ToBase64String(this.Data)), Stretch = (IsUniform ? Stretch.Uniform : Stretch.Fill) };
            img.SetValue(InkCanvas.LeftProperty, this.Left);
            img.SetValue(InkCanvas.TopProperty, this.Top);
            img.Width = this.Width;
            img.Height = this.Height;
            img.RenderTransform = new RotateTransform(RotationAngle);
            img.RenderTransformOrigin = new Point(0.5, 0.5);
            Canvas.SetZIndex(img, ZIndex);

            return img;
        }
    }
}
