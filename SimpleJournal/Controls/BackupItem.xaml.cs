using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für BackupItem.xaml
    /// </summary>
    public partial class BackupItem : UserControl
    {
        public delegate void onRecievedAction(bool discard, BackupDataItem item);
        public event onRecievedAction OnRecievedAction;

        public BackupItem(BackupDataItem data)
        {
            InitializeComponent();
            DataContext = data;
        }

        private void DiscardFileButton_Click(object sender, RoutedEventArgs e)
        {
            OnRecievedAction?.Invoke(true, DataContext as BackupDataItem);
        }

        private void RestoreFileButton_Click(object sender, RoutedEventArgs e)
        {
            OnRecievedAction?.Invoke(false, DataContext as BackupDataItem);
        }
    }

    public class BackupDataItem
    {
        public System.IO.FileInfo FileInfo { get; set; }

        public string Name { get => System.IO.Path.GetFileNameWithoutExtension(FileInfo.Name); set { } }

        public string Path { get; set; }
    }

    #region Converter
    public class PathToRunConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Properties.Resources.strPathHasNoOrigin;
            else
                return Properties.Resources.strPathHasOrigin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
