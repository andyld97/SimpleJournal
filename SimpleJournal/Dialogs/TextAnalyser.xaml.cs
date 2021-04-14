using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für TextAnalyser.xaml
    /// </summary>
    public partial class TextAnalyser : Window
    {
        private List<IPaper> pages = null;
        
        public TextAnalyser()
        {
            InitializeComponent();        
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
                        string[] result = DrawingCanvas.StartAnalyzingProcess(strokes, DrawingCanvas.Operation.Text);
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
