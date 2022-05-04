using SimpleJournal.Documents.UI.Helper;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleJournal.Documents.UI.Controls
{
    /// <summary>
    /// Interaction logic for TransparentImageButton.xaml
    /// </summary>
    public partial class TransparentImageButton : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<RoutedEventArgs> Click;

        private Uri image;
        private readonly SolidColorBrush NormalBrush = new SolidColorBrush() { Opacity = 0.6, Color = Colors.White };
        private readonly SolidColorBrush HoverBrush = new SolidColorBrush() { Opacity = 0.6, Color = Colors.LightBlue };

        public Uri Image
        {
            get => image;
            set
            {
                if (value != image)
                {
                    image = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            }
        }

        public TransparentImageButton()
        {
            InitializeComponent();
            DataContext = this;
            MouseEnter += TransparentImageButton_MouseEnter;
            MouseLeave += TransparentImageButton_MouseLeave;
        }

        private void TransparentImageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Background = NormalBrush;
        }

        private void TransparentImageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Background = HoverBrush;
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var evt = new RoutedEventArgs(e.RoutedEvent);
                Click?.Invoke(this, evt);
                e.Handled = evt.Handled;
            }
        }

        private void Image_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
         
        }
    }

    public class UriToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Uri uri)
                return ImageHelper.LoadImage(uri);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
