using SimpleJournal.Data;
using SimpleJournal.Documents.UI;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaction logic for SettingsItemControl.xaml
    /// </summary>
    public partial class SettingsItemControl : UserControl
    {
        private bool ignoreEvents;

        #region Properties
        public string Description { get; set; }

        public string SettingName { get; set; }

        public string SettingPropertyName { get; set; }

        public bool UseBorder { get; set; } = true;

        #endregion

        public delegate void onSettingChanged(object value);
        public event onSettingChanged OnSettingChanged;

        public SettingsItemControl()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += SettingsItemControl_Loaded;
        }

        private void SettingsItemControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ignoreEvents = true;
            chkValue.IsChecked = GetValue<bool>();
            ignoreEvents = false;
        }

        private T GetValue<T>()
        {
            var propertyInfo = typeof(Settings).GetProperty(SettingPropertyName);
            if (propertyInfo != null)
                return (T)(object)propertyInfo.GetValue(Settings.Instance);

            return default;
        }

        private void SetValue<T>(T value)
        {
            var propertyInfo = typeof(Settings).GetProperty(SettingPropertyName);
            if (propertyInfo != null)
                propertyInfo.SetValue(Settings.Instance, value);

            OnSettingChanged?.Invoke(value);
            Settings.Instance.Save();
        }

        private void chkValue_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ignoreEvents)
                return;

            SetValue(chkValue.IsChecked.Value);
        }

        private void TextDescription_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Enable toggle checkbox also on the description text
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                chkValue.IsChecked = !chkValue.IsChecked.Value;
        }
    }

    #region Converter

    public class UseBorderToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return 1.0;

            return 0.0;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UseBorderToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new Thickness(5);

            return new Thickness(5, 2, 5, 5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}