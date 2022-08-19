using MahApps.Metro.Controls;
using SimpleJournal.Documents.UI.Controls.Paper;
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


        #region Chequered
        private void SliderChequredStrokeWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            if (ChequeredPreview.Paper is Chequered chq)
            {
                chq.ApplyStrokeThickness(e.NewValue);
            }
        }

        private void SliderChequeredIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            if (ChequeredPreview.Paper is Chequered chq)
            {
                chq.ApplyIntensity(e.NewValue);
            }
        }

        private void SliderChequeredOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitalized)
                return;

            if (ChequeredPreview.Paper is Chequered chq)
            {
                SliderChequeredIntensity.Value = e.NewValue - 4;
                chq.ApplyOffset(e.NewValue);
            }
        }

        #endregion


    }
}
