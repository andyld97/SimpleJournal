using MahApps.Metro.Controls;
using SimpleJournal.Common;
using SimpleJournal.Modules;
using System;
using System.Windows;
using System.Windows.Interop;

namespace Dialogs
{
    /// <summary>
    /// Interaction logic for PaperPatternDialog.xaml
    /// </summary>
    public partial class PaperPatternDialog : MetroWindow
    {
        public PaperPatternDialog(PaperType paperType)
        {
            InitializeComponent();
            PaperPattern.SelectPaperType(paperType);

            // Assign module events
            var module = (PaperPattern as ITabbedModule);
            module.ModuleClosed += Module_ModuleClosed;
            module.ToggleMinimizeMaximize += Module_ToggleMinMax;
            module.Move += Module_Move;
        }


        private void Module_ModuleClosed(object? sender, bool result)
        {
            DialogResult = result;
        }

        private void Module_ToggleMinMax(object? sender, EventArgs e)
        {
            ToggleMinMax();
        }

        private void Module_Move(object? sender, EventArgs e)
        {
            Move();
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

        private void ToggleMinMax()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
        }
        #endregion
    }
}
