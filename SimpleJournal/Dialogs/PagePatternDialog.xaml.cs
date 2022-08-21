using MahApps.Metro.Controls;
using SimpleJournal.Documents.Pattern;
using SimpleJournal.Documents.UI.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace Dialogs
{
    /// <summary>
    /// Interaction logic for PagePatternDialog.xaml
    /// </summary>
    public partial class PagePatternDialog : MetroWindow
    {
        private readonly TextBlock[] headers;
        private bool isInitalized = false;

        public PagePatternDialog()
        {
            InitializeComponent();
            headers = new TextBlock[] { TabHeaderChequered, TabHeaderDotted, TabHeaderRuled, TabHeaderHelp };

            foreach (var tb in TabControl.Items)
            {
                var tabItem = (tb as TabItem);
                tabItem.ApplyTemplate();                
                var grid = tabItem.Template.FindName("PART_Header", tabItem) as Grid;
                grid.PreviewMouseDown += Grid_PreviewMouseDown;               
            }

            isInitalized = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var header in headers)
                header.FontWeight = FontWeights.Normal;

            headers[(sender as TabControl).SelectedIndex].FontWeight = FontWeights.Bold;
        }  

        private void ToggleMinMax()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
        }

        #region Move
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Move()
        {
            ReleaseCapture();
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            SendMessage(windowHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
        #endregion

        #region Mouse Move
        private void HandleMouseMove(MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (e.ClickCount == 2)
            {
                ToggleMinMax();
                e.Handled = true;
                return;
            }

            Move();
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            HandleMouseMove(e);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TabPanel)
                HandleMouseMove(e);
        }

        #endregion

        #region Paper Pattern
        #region Chequered
        private ChequeredPattern chequeredPattern = new ChequeredPattern();

        private void SliderChequredStrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            chequeredPattern.StrokeWidth = e.NewValue;
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void SliderChequeredIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            chequeredPattern.ViewPort = e.NewValue;
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void SliderChequeredOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            SliderChequeredIntensity.Value = e.NewValue - 4;
            chequeredPattern.ViewOffset = e.NewValue;
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void ChequeredColorPicker_ColorChanged(System.Windows.Media.Color c)
        {
            if (!isInitalized)
                return;

            chequeredPattern.Color = c.ToColor();
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);
        }

        private void ButtonResetChequered_Click(object sender, RoutedEventArgs e)
        {
            chequeredPattern.Reset();
            ChequeredPreview.Paper.ApplyPattern(chequeredPattern);

            isInitalized = false;

            SliderChequeredOffset.Value = chequeredPattern.ViewOffset;
            SliderChequeredIntensity.Value = chequeredPattern.ViewPort;
            SliderChequredStrokeWidth.Value = chequeredPattern.StrokeWidth;
            ChequeredColorPicker.SelectedColor = chequeredPattern.Color.ToColor();

            isInitalized = true;
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

            dottedPattern.ViewPort = SliderViewPort.Value;
            DottedPreview.Paper.ApplyPattern(dottedPattern);
        }


        private void DottedColorPicker_ColorChanged(System.Windows.Media.Color c)
        {
            if (!isInitalized)
                return;

            dottedPattern.Color = c.ToColor();
            DottedPreview.Paper.ApplyPattern(dottedPattern);
        }

        private void ButtonResetDotted_Click(object sender, RoutedEventArgs e)
        {
            dottedPattern.Reset();
            DottedPreview.Paper.ApplyPattern(dottedPattern);

            isInitalized = false;

            SliderDottedRadius.Value = dottedPattern.Radius;
            SliderViewPort.Value = dottedPattern.ViewPort;
            SliderChequredStrokeWidth.Value = dottedPattern.StrokeWidth;
            DottedColorPicker.SelectedColor = dottedPattern.Color.ToColor();

            isInitalized = true;
        }

        #endregion

        #region Ruled
        private readonly RuledPattern ruledPattern = new RuledPattern();


        private void SliderRuledOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            ruledPattern.ViewOffset = SliderRuledOffset.Value;
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

        private void ButtonResetRuled_Click(object sender, RoutedEventArgs e)
        {
            ruledPattern.Reset();
            RuledPreview.Paper.ApplyPattern(ruledPattern);

            isInitalized = false;

            SliderRuledOffset.Value = ruledPattern.ViewOffset;
            SliderRuledStrokeWdith.Value = ruledPattern.StrokeWidth;
            RuledColorPicker.SelectedColor = ruledPattern.Color.ToColor();

            isInitalized = true;
        }

        #endregion
        #endregion
    }
}
