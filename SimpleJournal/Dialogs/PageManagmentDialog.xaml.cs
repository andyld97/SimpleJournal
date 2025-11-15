using SimpleJournal.Helper;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für PageManagmentDialog.xaml
    /// </summary>
    public partial class PageManagmentDialog : Window
    {
        public PageManagmentDialog()
        {
            InitializeComponent();
            HideMinimizeButton(this);

            PageManagmentControl.ModuleClosed += delegate (object sender, bool e)
            {
                DialogResult = e;
            };
        }

        #region Remove Minimize-Button

        private const int GWL_STYLE = -16, WS_MAXIMIZEBOX = 0x10000, WS_MINIMIZEBOX = 0x20000;

    

        /// <summary>
        /// Hides the Minimize and Maximize buttons in a Window. Must be called in the constructor.
        /// </summary>
        /// <param name="window">The Window whose Minimize/Maximize buttons will be hidden.</param>
        public static void HideMinimizeButton(Window window)
        {
            window.SourceInitialized += (s, e) => 
            {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                int currentStyle = NativeMethods.GetWindowLong(hwnd, GWL_STYLE);

                _ = NativeMethods.SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
            };
        }

        #endregion
    }
}