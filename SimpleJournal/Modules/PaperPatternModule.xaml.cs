using Helper;
using SimpleJournal.Common;
using SimpleJournal.Documents.Pattern;
using SimpleJournal.Documents.UI;
using SimpleJournal.Documents.UI.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using Size = SimpleJournal.Common.Data.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace SimpleJournal.Modules
{
    /// <summary>
    /// Interaction logic for PaperPatternModule.xaml
    /// </summary>
    public partial class PaperPatternModule : UserControl, ITabbedModule
    {
        private readonly TextBlock[] headers;
        private bool isInitalized = false;

        #region Interface Impl
        public EventHandler<bool> ModuleClosed { get; set; }

        public EventHandler<string> TitleChanged { get; set; }

        public EventHandler ToggleWindowState { get; set; }

        public EventHandler Move { get; set; }

        public TabControl TabControl => MainTabControl;

        public Grid MainGrid => GridMainContent;

        public bool CanToggleWindowState => true;

        public string Title => Properties.Resources.strSettings_EditPaperPattern;

        public Size WindowSize => new Common.Data.Size(800, 600);
        #endregion

        public PaperPatternModule()
        {
            InitializeComponent();
            headers = new TextBlock[] { TabHeaderChequered, TabHeaderDotted, TabHeaderRuled, TabHeaderHelp };

            // ChequeredPattern has Gray as default color
            ChequeredColorPicker.SelectedColor = System.Windows.Media.Colors.Gray;

            // Load pattern from settings (if any)
            if (Settings.Instance.ChequeredPattern != null)
            {
                chequeredPattern = (ChequeredPattern)Settings.Instance.ChequeredPattern.Clone();
                ApplyChequeredPattern(chequeredPattern);
            }

            if (Settings.Instance.DottedPattern != null)
            {
                dottedPattern = (DottedPattern)Settings.Instance.DottedPattern.Clone();
                ApplyDottedPattern(dottedPattern);
            }

            if (Settings.Instance.RuledPattern != null)
            {
                ruledPattern = (RuledPattern)Settings.Instance.RuledPattern.Clone();
                ApplyRuledPattern(ruledPattern);
            }

            RefreshCM();

            ModuleHelper.ApplyTabbedFeatures(this);
            isInitalized = true;
        }

        public void SelectPaperType(PaperType paperType)
        {
            if (paperType == PaperType.Chequered)
                TabControl.SelectedIndex = 0;
            else if (paperType == PaperType.Dotted)
                TabControl.SelectedIndex = 1;
            else if (paperType == PaperType.Ruled)
                TabControl.SelectedIndex = 2;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitalized)
                return;

            foreach (var header in headers)
                header.FontWeight = FontWeights.Normal;

            headers[(sender as TabControl).SelectedIndex].FontWeight = FontWeights.Bold;
        }

        #region Paper Pattern

        #region Chequered
        private readonly ChequeredPattern chequeredPattern = new ChequeredPattern();

        private void SliderChequredStrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            chequeredPattern.StrokeWidth = e.NewValue;
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        /*
         * Important Notice! Sharpness/Offset are swapped here, because I misinterpreted these
         * values, so SliderChequeredIntensity is actually the offset and SliderChequeredOffset is actually the intensity/sharpness
         * 
         */
        private void SliderChequeredIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            chequeredPattern.ViewPort = e.NewValue;
            SliderChequeredOffset.Value = e.NewValue + 4;
            RefreshCM();
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void SliderChequeredOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            chequeredPattern.ViewOffset = e.NewValue;
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void RefreshCM()
        {
            TextChequeredCM.Text = Math.Round(SliderChequeredIntensity.Value.PxToCm(), 2) + " cm";
            TextDottedCM.Text = Math.Round(SliderDottedViewPort.Value.PxToCm(), 2) + " cm";
            TextRuledCM.Text = Math.Round(SliderRuledOffset.Value.PxToCm(), 2) + " cm";
        }

        private void ChequeredColorPicker_ColorChanged(System.Windows.Media.Color c)
        {
            if (!isInitalized)
                return;

            chequeredPattern.Color = c.ToColor();
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void ApplyChequeredPattern(ChequeredPattern pattern)
        {
            ChequeredPreview.Paper.ApplyPattern(pattern);

            isInitalized = false;

            SliderChequeredIntensity.Value = pattern.ViewPort;
            SliderChequeredOffset.Value = pattern.ViewOffset;
            SliderChequredStrokeWidth.Value = pattern.StrokeWidth;
            ChequeredColorPicker.SelectedColor = pattern.Color.ToColor();
            RefreshCM();

            isInitalized = true;
        }

        private void ButtonResetChequered_Click(object sender, RoutedEventArgs e)
        {
            chequeredPattern.Reset();
            ApplyChequeredPattern(chequeredPattern);
        }

        #endregion

        #region Dotted

        private readonly DottedPattern dottedPattern = new DottedPattern();

        private void SliderDottedRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            dottedPattern.Radius = SliderDottedRadius.Value;
            DottedPreview.Paper.ApplyPattern(dottedPattern);
        }
        private void SliderDottedStrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            dottedPattern.StrokeWidth = SliderDottedStrokeWidth.Value;
            DottedPreview.Paper.ApplyPattern(dottedPattern);
        }

        private void SliderViewPort_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            dottedPattern.ViewPort = SliderDottedViewPort.Value;
            RefreshCM();
            DottedPreview.Paper.ApplyPattern(dottedPattern);
        }


        private void DottedColorPicker_ColorChanged(System.Windows.Media.Color c)
        {
            if (!isInitalized)
                return;

            dottedPattern.Color = c.ToColor();
            DottedPreview.Paper.ApplyPattern(dottedPattern);
        }

        private void ApplyDottedPattern(DottedPattern dottedPattern)
        {
            DottedPreview.Paper.ApplyPattern(dottedPattern);

            isInitalized = false;

            SliderDottedRadius.Value = dottedPattern.Radius;
            SliderDottedViewPort.Value = dottedPattern.ViewPort;
            SliderDottedStrokeWidth.Value = dottedPattern.StrokeWidth;
            DottedColorPicker.SelectedColor = dottedPattern.Color.ToColor();
            RefreshCM();

            isInitalized = true;
        }

        private void ButtonResetDotted_Click(object sender, RoutedEventArgs e)
        {
            dottedPattern.Reset();
            ApplyDottedPattern(dottedPattern);
        }

        #endregion

        #region Ruled
        private readonly RuledPattern ruledPattern = new RuledPattern();

        private void SliderRuledOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            ruledPattern.ViewOffset = SliderRuledOffset.Value;
            RefreshCM();
            RuledPreview.Paper.ApplyPattern(ruledPattern);
        }

        private void SliderRuledStrokeWdith_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            ruledPattern.StrokeWidth = SliderRuledStrokeWdith.Value;
            RuledPreview.Paper.ApplyPattern(ruledPattern);
        }

        private void RuledColorPicker_ColorChanged(System.Windows.Media.Color c)
        {
            if (!isInitalized)
                return;

            ruledPattern.Color = c.ToColor();
            RuledPreview.Paper.ApplyPattern(ruledPattern);
        }

        private void ApplyRuledPattern(RuledPattern ruledPattern)
        {
            RuledPreview.Paper.ApplyPattern(ruledPattern);

            isInitalized = false;

            SliderRuledOffset.Value = ruledPattern.ViewOffset;
            SliderRuledStrokeWdith.Value = ruledPattern.StrokeWidth;
            RuledColorPicker.SelectedColor = ruledPattern.Color.ToColor();
            RefreshCM();

            isInitalized = true;
        }

        private void ButtonResetRuled_Click(object sender, RoutedEventArgs e)
        {
            ruledPattern.Reset();
            ApplyRuledPattern(ruledPattern);
        }

        #endregion

        #endregion

        #region Confirm/Cancel

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            ModuleClosed?.Invoke(sender, false);
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            if (!chequeredPattern.HasDefaultValues)
                Settings.Instance.ChequeredPattern = chequeredPattern;
            else
                Settings.Instance.ChequeredPattern = null;

            if (!dottedPattern.HasDefaultValues)
                Settings.Instance.DottedPattern = dottedPattern;
            else
                Settings.Instance.DottedPattern = null;

            if (!ruledPattern.HasDefaultValues)
                Settings.Instance.RuledPattern = ruledPattern;
            else
                Settings.Instance.RuledPattern = null;

            Settings.Instance.Save();
            MainWindow.W_INSTANCE.ApplySettings();

            ModuleClosed?.Invoke(sender, true);
        }
        #endregion
    }
}