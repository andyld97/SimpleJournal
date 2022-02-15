using ControlzEx.Theming;
using SimpleJournal.Data;
using SimpleJournal.Dialogs;
using SimpleJournal.Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SimpleJournal.Common.FileAssociations;
using SimpleJournal.Documents.UI;
using SimpleJournal.Documents.UI.Controls;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window, INotifyPropertyChanged
    {
        private bool editMode;

        public event PropertyChangedEventHandler PropertyChanged;

        public static Theme[] Themes
        {
            get
            {
                string value = Settings.Instance.UseDarkMode ? "Dark" : "Light";
                return ThemeManager.Current.Themes.Where(p => !p.DisplayName.Contains("Colorful") && p.DisplayName.Contains(value)).ToArray();
            }
        }

        public SettingsDialog()
        {
            editMode = true;
            InitializeComponent();
            LoadSettings();

            ComboBoxThemeChooser.DataContext = this;

#if DEBUG
            DebugTestButton.Visibility = Visibility.Visible;
#endif
        }

        private void LoadSettings()
        {
            editMode = true;

            chkPaperType.SelectedIndex = (int)Settings.Instance.PaperType;

            if (Settings.Instance.WindowState == WindowState.Maximized)
                chkWindowMode.SelectedIndex = 2;
            else if (Settings.Instance.WindowState == WindowState.Normal)
                chkWindowMode.SelectedIndex = 1;
            else if (Settings.Instance.WindowState == WindowState.Minimized)
                chkWindowMode.SelectedIndex = 0;

            CheckBoxActivateGlowingBrush.IsChecked = Settings.Instance.ActivateGlowingBrush;
            CheckBoxUseNewMenu.IsChecked = Settings.Instance.UseNewMenu;

#if UWP
            TabTouch.Visibility = Visibility.Collapsed;
            TabTouchUWP.Visibility = Visibility.Visible;
#else 
            TabTouch.Visibility = Visibility.Visible;
            TabTouchUWP.Visibility = Visibility.Collapsed;
#endif

            // Apply background settings
            switch (Settings.Instance.PageBackground)
            {
                case SimpleJournal.Common.Background.Default: RbDefault.IsChecked = true; break;
                case SimpleJournal.Common.Background.Blue: RbBlue.IsChecked = true; break;
                case SimpleJournal.Common.Background.Sand: RbSand.IsChecked = true; break;
                case SimpleJournal.Common.Background.Wooden1: RbWooden1.IsChecked = true; break;
                case SimpleJournal.Common.Background.Wooden2: RbWooden2.IsChecked = true; break;
                case SimpleJournal.Common.Background.Custom: RbCustom.IsChecked = true; break;
            }

            TextCustomImagePath.Text = Settings.Instance.CustomBackgroundImagePath;
            SelectTheme();

            NumericUpDownAutoSaveInteral.Value = Settings.Instance.AutoSaveIntervalMinutes;
            editMode = false;
        }

        private void SelectTheme()
        {
            // Get current selected theme
            string value = Settings.Instance.UseDarkMode ? "Dark" : "Light";
            CheckBoxDisplayMode.SelectedIndex = Settings.Instance.UseDarkMode ? 1 : 0;
            ComboBoxThemeChooser.SelectedItem = Themes.Where(p => p.DisplayName.Contains(Settings.Instance.Theme.Replace(".Colorful", string.Empty)) && p.DisplayName.Contains(value)).FirstOrDefault();
            CheckBoxThemeIsColorful.IsChecked = Settings.Instance.Theme.Contains(".Colorful");
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chkPaperType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.PaperType = (PaperType)(sender as ComboBox).SelectedIndex;
            Settings.Instance.Save();
        }

        private void chkWindowMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editMode)
                return;

            int index = this.chkWindowMode.SelectedIndex;
            if (index == 0)
                Settings.Instance.WindowState = WindowState.Minimized;
            else if (index == 1)
                Settings.Instance.WindowState = WindowState.Normal;
            else if (index == 2)
                Settings.Instance.WindowState = WindowState.Maximized;

            Settings.Instance.Save();
        }

        private void BtnOpenAssistent_Click(object sender, RoutedEventArgs e)
        {
            new SetupDialog().ShowDialog();
            LoadSettings();
        }

        private void BtnResetPens_Click(object sender, RoutedEventArgs e)
        {
            // Delete Pen.xml
            try
            {
                System.IO.File.Delete(Consts.PenSettingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"{Properties.Resources.strFailedToResetPens}{Environment.NewLine}{Environment.NewLine}{ex.Message}", SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Notify main window to refresh pens
            MainWindow.W_INSTANCE.UpdatePenButtons(reset: true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // Notify Window2 that the settings changed
            MainWindow.W_INSTANCE.ApplySettings();
        }

        private void BtnResetRecentlyOpenedDocuments_Click(object sender, RoutedEventArgs e)
        {
            RecentlyOpenedDocuments.Instance.Clear();
            RecentlyOpenedDocuments.Save();
        }

        private void NumericUpDownAutoSaveInteral_OnChanged(int oldValue, int newValue)
        {
            if (editMode)
                return;

            Settings.Instance.AutoSaveIntervalMinutes = newValue;
            Settings.Instance.Save();
        }

        private void CheckBoxDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseDarkMode = CheckBoxDisplayMode.SelectedIndex == 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Themes"));

            string value = Settings.Instance.UseDarkMode ? "Dark" : "Light";
            ComboBoxThemeChooser.ItemsSource = Themes;
            ComboBoxThemeChooser.SelectedItem = Themes.Where(p => p.DisplayName.Contains(Settings.Instance.Theme.Replace(".Colorful", string.Empty)) && p.DisplayName.Contains(value)).FirstOrDefault();

            Settings.Instance.Save();

            // Apply theming
            GeneralHelper.ApplyTheming();
        }

        private void UpdateThemeSettings()
        {
            if (ComboBoxThemeChooser.SelectedIndex == -1 || editMode)
                return;

            string theme = Themes[ComboBoxThemeChooser.SelectedIndex].Name.Replace("Light.", string.Empty).Replace("Dark.", string.Empty);

            if (CheckBoxThemeIsColorful.IsChecked.HasValue && CheckBoxThemeIsColorful.IsChecked.Value)
                theme += ".Colorful";

            Settings.Instance.Theme = theme;
            Settings.Instance.Save();

            // Apply theming
            GeneralHelper.ApplyTheming();
            MainWindow.W_INSTANCE.UpdateGlowingBrush();
        }

        private void ComboBoxThemeChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editMode || ComboBoxThemeChooser.SelectedIndex == -1)
                return;

            UpdateThemeSettings();
        }

        private void CheckBoxThemeIsColorful_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Themes"));
            UpdateThemeSettings();
        }

        private void CheckBoxThemeIsColorful_Unchecked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Themes"));
            UpdateThemeSettings();
        }

        private void CheckBoxActivateGlowingBrush_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.ActivateGlowingBrush = CheckBoxActivateGlowingBrush.IsChecked.Value;
            Settings.Instance.Save();
            UpdateThemeSettings();
        }

        private void btnSetFileAssoc_Click(object sender, RoutedEventArgs e)
        {
#if UWP
            GeneralHelper.InstallUWPFileAssoc();
#else
            FileAssociations.EnsureAssociationsSet(Consts.Executable);
#endif
        }

        private void DownloadTDM_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            GeneralHelper.OpenUri(e.Uri);
        }

        private void rb_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            if (sender is RadioButton rb)
            {
                if (rb.Name == RbDefault.Name)
                    Settings.Instance.PageBackground = SimpleJournal.Common.Background.Default;
                else if (rb.Name == RbSand.Name)
                    Settings.Instance.PageBackground = SimpleJournal.Common.Background.Sand;
                else if (rb.Name == RbBlue.Name)
                    Settings.Instance.PageBackground = SimpleJournal.Common.Background.Blue;
                else if (rb.Name == RbWooden1.Name)
                    Settings.Instance.PageBackground = SimpleJournal.Common.Background.Wooden1;
                else if (rb.Name == RbWooden2.Name)
                    Settings.Instance.PageBackground = SimpleJournal.Common.Background.Wooden2;

                if (rb.Name == RbCustom.Name)
                    Settings.Instance.PageBackground = SimpleJournal.Common.Background.Custom;

                MainWindow.W_INSTANCE.ApplyBackground();
                Settings.Instance.Save();
            }
        }

        private void ButtonSearchCustomImage_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog() { Filter = $"{Properties.Resources.strImages} (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png" })
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TextCustomImagePath.Text = ofd.FileName;
                    MainWindow.W_INSTANCE.ApplyBackground();
                }
            }
        }

        private void TextCustomImagePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.CustomBackgroundImagePath = TextCustomImagePath.Text;
            Settings.Instance.Save();
        }

        private void SettingFitToCurve_OnSettingChanged(object value)
        {
            if (DrawingCanvas.LastModifiedCanvas != null)
                DrawingCanvas.LastModifiedCanvas.DefaultDrawingAttributes.FitToCurve = (bool)value;
        }

        private void CheckBoxUseNewMenu_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseNewMenu = CheckBoxUseNewMenu.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void DebugTestButton_Click(object sender, RoutedEventArgs e)
        {
            // This is just for debugging purposes
            new UpdateDialog(Consts.StoreVersion).ShowDialog();
        }

        private void ButtonShowAutoSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!System.IO.Directory.Exists(Consts.AutoSaveDirectory))
                    System.IO.Directory.CreateDirectory(Consts.AutoSaveDirectory);
            }
            catch
            { }

            GeneralHelper.OpenUri(new Uri(Consts.AutoSaveDirectory));
        }

        private void ButtonShowBackupFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!System.IO.Directory.Exists(Consts.BackupDirectory))
                    System.IO.Directory.CreateDirectory(Consts.BackupDirectory);
            }
            catch
            { }

            GeneralHelper.OpenUri(new Uri(Consts.BackupDirectory));
        }
    }
}