using SimpleJournal.Common;
using SimpleJournal.Documents.UI.Controls;
using SimpleJournal.Documents.UI.Controls.Paper;
using SimpleJournal.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für TextAnalyser.xaml
    /// </summary>
    public partial class TextAnalyser : Window
    {
        private List<IPaper> pages = null;

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_SYSMENU = 0x80000;

        public TextAnalyser()
        {
            InitializeComponent();        
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowInteropHelper wih = new WindowInteropHelper(this);
            IntPtr hWnd = wih.Handle;
            Int32 windowStyle = NativeMethods.GetWindowLongPtr(hWnd, GWL_STYLE);

            // MinSysbutton disabled
            _ = NativeMethods.SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MINIMIZEBOX);
        }

        public async Task AnalyzePages(List<IPaper> pages)
        {
            this.pages = pages;

            txtText.Text = string.Empty;
            int pageCounter = 1;

            foreach (IPaper page in pages)
            {
                var strokes = page.Canvas.Strokes;
                await Task.Run(() =>
                {
                    if (strokes.Count != 0)
                    {
                        string[] result = DrawingCanvas.StartAnalyzingProcess(strokes, Operation.Text);
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            txtText.Text += $"{Properties.Resources.strPage} {pageCounter}:\n\n";
                            foreach (string line in result)
                            {
                                txtText.Text += line + "\n";
                            }
                            txtText.Text += "\n\n";
                        }));

                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            txtText.Text += $"{Properties.Resources.strPage} {pageCounter}:\n\n";
                        }));
                    }
                   pageCounter++;
               });
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Title = Properties.Resources.strOCRTitle;
                this.txtText.IsEnabled = true;
                this.btnCopy.IsEnabled = this.btnOk.IsEnabled = true;
            }));
        }
 
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtText.Text);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
