using ControlzEx.Theming;
using SimpleJournal.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaction logic for DesignSettings.xaml
    /// </summary>
    public partial class DesignSettings : UserControl
    {
        private bool isInitalized = false;

        public static Theme[] Themes
        {
            get
            {
                string value = Settings.Instance.UseDarkMode ? "Dark" : "Light";
                return ThemeManager.Current.Themes.Where(p => !p.DisplayName.Contains("Colorful") && p.DisplayName.Contains(value)).ToArray();
            }
        }

        public DesignSettings()
        {
            InitializeComponent();
            ComboBoxThemeChooser.DataContext = this;
            SelectTheme();
            isInitalized = true;
        }

        private void UpdateTheme()
        {
            if (!isInitalized)
                return;

            string theme = Themes[ComboBoxThemeChooser.SelectedIndex].Name.Replace("Light.", string.Empty).Replace("Dark.", string.Empty);
            bool isColorful = CheckBoxThemeIsColorful.IsChecked.HasValue && CheckBoxThemeIsColorful.IsChecked.Value;
            if (isColorful)
                theme += ".Colorful";

            string themeName = $"{(CheckBoxDisplayMode.SelectedIndex == 1 ? ThemeManager.BaseColorDarkConst : ThemeManager.BaseColorLightConst)}.{theme}"; //.Colorful";
            ThemeManager.Current.ChangeTheme(this, themeName);

            if (isColorful)
                GridTitleBar.Background = (SolidColorBrush)FindResource("Fluent.Ribbon.Brushes.AccentBaseColorBrush");
            else
                GridTitleBar.Background = (SolidColorBrush)FindResource("WhiteBrush");
        }

        private void SelectTheme()
        {
            // Get current selected theme
            string value = Settings.Instance.UseDarkMode ? "Dark" : "Light";
            CheckBoxDisplayMode.SelectedIndex = Settings.Instance.UseDarkMode ? 1 : 0;
            ComboBoxThemeChooser.SelectedItem = Themes.Where(p => p.DisplayName.Contains(Settings.Instance.Theme.Replace(".Colorful", string.Empty)) && p.DisplayName.Contains(value)).FirstOrDefault();
            CheckBoxThemeIsColorful.IsChecked = Settings.Instance.Theme.Contains(".Colorful");
        }

        private void ComboBoxThemeChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTheme();
        }

        private void CheckBoxDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTheme();
        }

        private void CheckBoxThemeIsColorful_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateTheme();
        }
    }
}