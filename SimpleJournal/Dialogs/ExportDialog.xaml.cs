using Microsoft.Win32;
using SimpleJournal.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        private readonly ObservableCollection<IPaper> pages = null;

        public ExportDialog(ObservableCollection<IPaper> pages)
        {
            this.pages = pages;
            InitializeComponent();

            rdAllPages.Content = $"{Properties.Resources.strExportAllPages} ({pages.Count})";

            txtFrom.Text = "1";
            txtTo.Text = pages.Count().ToString();
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            if (!chkExportAsJournal.IsChecked.Value)
            {
                if (rdAllPages.IsChecked.Value)
                {
                    export(0, pages.Count - 1);
                }
                else if (rdSinglePage.IsChecked.Value)
                {
                    if (int.TryParse(txtSinglePage.Text, out int value))
                    {
                        if (value < 1 || value > pages.Count())
                            displayInvalidInput();
                        else
                        {
                            export(value - 1, value - 1);
                        }
                    }
                    else
                        displayInvalidInput();
                }
                else if (rdArea.IsChecked.Value)
                {
                    if (int.TryParse(txtFrom.Text, out int valueFrom) && int.TryParse(txtTo.Text, out int valueTo))
                    {
                        if ((valueFrom <= 0 || valueTo > pages.Count || valueTo <= 0 || valueTo < valueFrom))
                        {
                            displayInvalidInput();
                        }
                        else
                        {
                            export(valueFrom - 1, valueTo - 1);
                        }
                    }
                    else
                        displayInvalidInput();
                }
            }
            else
            {
                // Export as jorunal
                if (rdAllPages.IsChecked.Value)
                {
                    exportJournal(0, pages.Count - 1);
                }
                else if (rdSinglePage.IsChecked.Value)
                {
                    if (int.TryParse(txtSinglePage.Text, out int value))
                    {
                        if (value < 1 || value > pages.Count())
                            displayInvalidInput();
                        else
                        {
                            exportJournal(value - 1, value - 1);
                        }
                    }
                    else
                        displayInvalidInput();
                }
                else if (rdArea.IsChecked.Value)
                {
                    if (int.TryParse(txtFrom.Text, out int valueFrom) && int.TryParse(txtTo.Text, out int valueTo))
                    {
                        if ((valueFrom <= 0 || valueTo > pages.Count || valueTo <= 0 || valueTo < valueFrom))
                        {
                            displayInvalidInput();
                        }
                        else
                        {
                            exportJournal(valueFrom - 1, valueTo - 1);
                        }
                    }
                    else
                        displayInvalidInput();
                }
            }
        }
           

        private void displayInvalidInput()
        {
            MessageBox.Show(this, Properties.Resources.strInvalidInput, Properties.Resources.strInvalidInputTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void export(int from, int to)
        {
            SaveFileDialog ofd = new SaveFileDialog() { Filter = $"{Properties.Resources.strImages}|*.png;*.tif;*.bmp;*.jpg;*.jpeg" };
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
            {
                exportImages(ofd.FileName, from, to);
            }
        }

        private void exportImages(string path, int from, int to)
        {
            this.mainPanel.IsEnabled = false;
            if (path.EndsWith(".png") || path.EndsWith(".tif") || path.EndsWith(".bmp") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
            {
                // ignore
            }
            else
                path += ".png";

            string oldExt = System.IO.Path.GetExtension(path);

            if (from == to)
            {
                // Single page
                path = path.Replace(oldExt, string.Empty);
                path += $".{from + 1}{oldExt}";

                exportPageAsImage(pages[from].Canvas, path, from + 1);

                this.Title = $"{Properties.Resources.strExport} ...";
            }
            else
            {
                for (int i = from; i <= to; i++)
                {
                    string nPath = path.Replace(oldExt, string.Empty);
                    int displayNumber = i + 1;

                    nPath += $".{displayNumber}{oldExt}";

                    exportPageAsImage(pages[i].Canvas, nPath, displayNumber);

                    this.Title = $"{Properties.Resources.strExport} ... {Properties.Resources.strPage} {displayNumber}/{to + 1}";
                }
                this.Title = $"{Properties.Resources.strExport} ({Properties.Resources.strPage} {to + 1}/{to + 1})";
            }

            this.mainPanel.IsEnabled = true;
            this.Title = Properties.Resources.strExportPages;
            MessageBox.Show(this, Properties.Resources.strExportFinished, Properties.Resources.strSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void exportJournal(int from, int to)
        {
            SaveFileDialog ofd = new SaveFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal;" };
            var dialogResult = ofd.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                this.mainPanel.IsEnabled = false;

                Journal journal = new Journal();
                for (int i = from; i <= to; i++)
                {
                    var currentCanvas = pages[i].Canvas;

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        currentCanvas.Strokes.Save(ms);
                        JournalPage journalPage = new JournalPage();
                        journalPage.SetData(ms.ToArray());

                        // Check for additional ressources
                        if (currentCanvas.Children.Count > 0)
                        {
                            foreach (UIElement element in currentCanvas.Children)
                            {
                                var result = JournalResource.ConvertFromUIElement(element);
                                if (result != null)
                                    journalPage.JournalResources.Add(result);
                            }
                        }
                        journal.Pages.Add(journalPage);
                    }

                    int displayNumber = i + 1;
                    this.Title = $"{Properties.Resources.strExport} ... {Properties.Resources.strPage} {displayNumber}/{to + 1}";
                }
                this.Title = $"{Properties.Resources.strExport} ({Properties.Resources.strPage} {to + 1}/{to + 1})";

                journal.Save(ofd.FileName);
                this.mainPanel.IsEnabled = true;
                this.Title = Properties.Resources.strExportPages;
                MessageBox.Show(this, Properties.Resources.strExportFinished, Properties.Resources.strSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void exportPageAsImage(DrawingCanvas canvas, string path, int page)
        {
            try
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96.0, 96.0, PixelFormats.Default);
                rtb.Render(canvas);

                encoder.Frames.Add(BitmapFrame.Create(rtb));
                using (System.IO.FileStream fs = System.IO.File.Open(path, System.IO.FileMode.Create))
                {
                    encoder.Save(fs);
                    fs.Dispose();
                }

                rtb.Clear();
                rtb = null;
                encoder.Frames.Clear();
                encoder = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                MessageBox.Show(this, $"{Properties.Resources.strExportFailed} {page}: {e.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtFrom_TextChanged(object sender, TextChangedEventArgs e)
        {


        }
        private void TxtTo_TextChanged(object sender, TextChangedEventArgs e)
        {
   

        }

        private void TxtSinglePage_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
