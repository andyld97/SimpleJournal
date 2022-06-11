using SimpleJournal.Common;
using SimpleJournal.Documents.UI.Extensions;
using SimpleJournal.Documents.UI.Helper;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Controls
{
    /// <summary>
    /// Interaction logic for NotificationDisplay.xaml
    /// </summary>
    public partial class NotificationDisplay : UserControl
    {
        public NotificationDisplay(Notification notification)
        {
            InitializeComponent();
            DataContext = notification;

            if (notification.Type == NotificationType.Info)
                Border.Background = Border.FindResource("InfoBrush") as Brush;
            else if (notification.Type == NotificationType.Warning)
                Border.Background = Border.FindResource("WarningBrush") as Brush;
            else if (notification.Type == NotificationType.Error)
                Border.Background = Border.FindResource("ErrorBrush") as Brush;

            TextMessage.Inlines.Clear();
            var tooltip = new TextBlock();
            foreach (var inline in notification.Message)
            {
                TextMessage.Inlines.Add(inline.ConvertInline());
                tooltip.Inlines.Add(inline.ConvertInline());
            }
            TextMessage.ToolTip = tooltip;


            PanelActions.Children.Clear();
            int counter = 0;

            TextBlock textBlock = new TextBlock() { Foreground = new SolidColorBrush(Colors.WhiteSmoke) };
            foreach (var action in notification.UserInteractions)
            {               
                var link = new Hyperlink(new Run(action.Description) { Foreground = new SolidColorBrush(Colors.WhiteSmoke) })
                {
                    Tag = action,
                    NavigateUri = new Uri("http://google.de"), // only a placeholder
                    Foreground = new SolidColorBrush(Colors.WhiteSmoke),
                };
                link.RequestNavigate += Link_RequestNavigate;

                textBlock.Inlines.Add(new Run("• "));
                textBlock.Inlines.Add(link);

                if (counter != notification.UserInteractions.Count - 1)                
                    textBlock.Inlines.Add(new LineBreak());
                
                counter++;
            }

            PanelActions.Children.Add(textBlock);
        }

        private async void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if ((sender as Hyperlink).Tag is UserInteraction ui)
                await ui.ExecuteAsync();
        }
    }

    #region Converter
    public class NotficationImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationType nt)
            {
               var uri =  ImageHelper.GeneratePackUri(false, "resources/notifications", nt.ToString().ToLower() + ".png");
                return ImageHelper.LoadImage(uri);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
