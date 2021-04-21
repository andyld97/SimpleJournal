using ControlzEx.Theming;
using SimpleJournal.Data;
using SimpleJournal.Dialogs;
using SJFileAssoc;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window, INotifyPropertyChanged
    {
        private bool editMode = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public Theme[] Themes
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

#if UWP
            btnSetFileAssocText.Text = string.Empty;
            btnSetFileAssocPoint.Text = string.Empty;
            CheckBoxDisableTouchScreen.IsEnabled = false;
#endif
        }

        private void LoadSettings()
        {
            editMode = true;

            chkPaperType.SelectedIndex = (int)Settings.Instance.PaperType;
            chkScrollbar.IsChecked = Settings.Instance.EnlargeScrollbar;

            if (Settings.Instance.WindowState == WindowState.Maximized)
                chkWindowMode.SelectedIndex = 2;
            else if (Settings.Instance.WindowState == WindowState.Normal)
                chkWindowMode.SelectedIndex = 1;
            else if (Settings.Instance.WindowState == WindowState.Minimized)
                chkWindowMode.SelectedIndex = 0;

            chkDisplaySidebarAutomatically.IsChecked = Settings.Instance.DisplaySidebarAutomatically;
            chkUseInputPreasure.IsChecked = Settings.Instance.UsePreasure;
            useCircleCorrect.IsChecked = Settings.Instance.UseCircleCorrection;
            useRotationCorrect.IsChecked = Settings.Instance.UseRotateCorrection;
            CheckBoxOldPattern.IsChecked = Settings.Instance.UseOldChequeredPattern;
            chkUseFitToCurve.IsChecked = Settings.Instance.UseFitToCurve;
            CheckBoxActivateGlowingBrush.IsChecked = Settings.Instance.ActivateGlowingBrush;
            CheckBoxDisableTouchScreen.IsChecked = Settings.Instance.UseTouchScreenDisabling;
            chkScrollbarNatural.IsChecked = Settings.Instance.UseNaturalScrolling;

            // Apply background settings
            switch (Settings.Instance.PageBackground)
            {
                case Settings.Background.Default: RbDefault.IsChecked = true; break;
                case Settings.Background.Blue: RbBlue.IsChecked = true; break;
                case Settings.Background.Sand: RbSand.IsChecked = true; break;
                case Settings.Background.Wooden1: RbWooden1.IsChecked = true; break;
                case Settings.Background.Wooden2: RbWooden2.IsChecked = true; break;
                case Settings.Background.Custom: RbCustom.IsChecked = true; break;
            }

            TextCustomImagePath.Text = Settings.Instance.CustomBackgroundImagePath;

            SelectTheme();

            NumericUpDownAutoSaveInteral.Value = Settings.Instance.AutoSaveIntervalMinutes;
            CheckBoxUseAutoSave.IsChecked = Settings.Instance.UseAutoSave;                        

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

        private void txtScrollbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                chkScrollbar.IsChecked = !chkScrollbar.IsChecked;
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
                System.IO.File.Delete(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pen.xml"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"{Properties.Resources.strFailedToResetPens}{Environment.NewLine}{Environment.NewLine}{ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Notify main window to refresh pens
            Window2.W_INSTANCE.UpdatePenButtons(true);
        }

        private void ChkUseInputPreasure_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UsePreasure = chkUseInputPreasure.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void ChkDisplaySidebarAutomatically_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.DisplaySidebarAutomatically = chkDisplaySidebarAutomatically.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void ChkScrollbar_Checked_1(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.EnlargeScrollbar = chkScrollbar.IsChecked.Value;
            Settings.Instance.Save();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // Notify Window2 that the settings changed
            Window2.W_INSTANCE.ApplySettings();
        }

        private void UseRotationCorrect_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseRotateCorrection = useRotationCorrect.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void UseCircleCorrect_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseCircleCorrection = useCircleCorrect.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void BtnResetRecentlyOpenedDocuments_Click(object sender, RoutedEventArgs e)
        {
            RecentlyOpenedDocuments.Instance.Clear();
            RecentlyOpenedDocuments.Save();
        }

        private void CheckBoxOldPattern_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseOldChequeredPattern = CheckBoxOldPattern.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void NumericUpDownAutoSaveInteral_OnChanged(int oldValue, int newValue)
        {
            if (editMode)
                return;

            Settings.Instance.AutoSaveIntervalMinutes = newValue;
            Settings.Instance.Save();
        }

        private void CheckBoxUseAutoSave_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseAutoSave = CheckBoxUseAutoSave.IsChecked.Value;
            Settings.Instance.Save();
        }


        private void chkUseFitToCurve_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            if (DrawingCanvas.LastModifiedCanvas != null)
                DrawingCanvas.LastModifiedCanvas.DefaultDrawingAttributes.FitToCurve = chkUseFitToCurve.IsChecked.Value;

            Settings.Instance.UseFitToCurve = chkUseFitToCurve.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void CheckBoxDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseDarkMode = CheckBoxDisplayMode.SelectedIndex == 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Themes"));

            string value = Settings.Instance.UseDarkMode ? "Dark" : "Light";
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
            Window2.W_INSTANCE.UpdateGlowingBrush();
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

        private void CheckBoxDisableTouchScreen_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseTouchScreenDisabling = CheckBoxDisableTouchScreen.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void btnSetFileAssoc_Click(object sender, RoutedEventArgs e)
        {
            FileAssociations.EnsureAssociationsSet();
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
                    Settings.Instance.PageBackground = Settings.Background.Default;
                else if (rb.Name == RbSand.Name)
                    Settings.Instance.PageBackground = Settings.Background.Sand;
                else if (rb.Name == RbBlue.Name)
                    Settings.Instance.PageBackground = Settings.Background.Blue;
                else if (rb.Name == RbWooden1.Name)
                    Settings.Instance.PageBackground = Settings.Background.Wooden1;
                else if (rb.Name == RbWooden2.Name)
                    Settings.Instance.PageBackground = Settings.Background.Wooden2;

                if (rb.Name == RbCustom.Name)
                    Settings.Instance.PageBackground = Settings.Background.Custom;

                Window2.W_INSTANCE.ApplyBackground();
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
                    Window2.W_INSTANCE.ApplyBackground();
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

        private void chkScrollbarNatural_Checked(object sender, RoutedEventArgs e)
        {
            if (editMode)
                return;

            Settings.Instance.UseNaturalScrolling = chkScrollbarNatural.IsChecked.Value;
            Settings.Instance.Save();
        }
    }
}