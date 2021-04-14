using System.Windows;
using System.Windows.Controls;

namespace SimpleJournal.Controls
{
    public class AnimatedScrollViewer : ScrollViewer
    {
        public static DependencyProperty ScrollOffsetProperty = DependencyProperty.Register("ScrollOffset", typeof(double), typeof(AnimatedScrollViewer), new PropertyMetadata(new PropertyChangedCallback(OnTestPropertyChanged)));

        public static void OnTestPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is AnimatedScrollViewer asv)
                asv?.ScrollToVerticalOffset(asv.ScrollOffset);
        }

        public double ScrollOffset
        {
            get => (double)GetValue(ScrollOffsetProperty);
            set => SetValue(ScrollOffsetProperty, value);
        }
    }
}