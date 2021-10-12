using Microsoft.Win32;
using SimpleJournal.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaction logic for ExportControl.xaml
    /// </summary>
    public partial class ExportControl : UserControl, IDialog, INotifyPropertyChanged
    {
        private ObservableCollection<IPaper> pages = null;
        private ExportMode selectedExportMode = ExportMode.AllPages;
        private IPaper currentPage;
        private bool isInitalized;
        private string title;
        private Window owner;

        public event PropertyChangedEventHandler PropertyChanged;
        private List<Expander> expanders = new List<Expander>();

        public ExportMode SelectedExportMode
        {
            get => selectedExportMode;
            set
            {
                if (value != selectedExportMode)
                {
                    selectedExportMode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedExportMode"));
                }
            }
        }

        public string Title
        {
            get => title;
            set
            {
                if (value != title)
                {
                    title = value;
                    TitleChanged?.Invoke(this, value);
                }
            }
        }

        public EventHandler<bool> DialogClosed { get; set; }

        public EventHandler<string> TitleChanged { get; set; }

        public ExportControl()
        {
            InitializeComponent();
        }

        public void Initalize(ObservableCollection<IPaper> pages, IPaper currentPage, Window owner)
        {
            DataContext = this;
            this.currentPage = currentPage;
            this.pages = pages;
            this.owner = owner;

            UpDownFrom.Maximum = pages.Count;
            UpDownTo.Maximum = pages.Count;
            UpDownTo.Value = pages.Count;
            UpDownFrom.Value = 1;
            UpDownSinglePage.Maximum = pages.Count;
            UpDownSinglePage.Value = 1;

            UpDownFrom.OnChanged += UpDown_OnChanged;
            UpDownTo.OnChanged += UpDown_OnChanged;
            UpDownSinglePage.OnChanged += UpDown_OnChanged;
            CheckBoxExportAsJournal.IsChecked = Settings.Instance.ExportAsJournal;
            isInitalized = true;

            RenderPreview();
        }

        private void UpDown_OnChanged(int oldValue, int newValue)
        {
            RenderPreview();
        }

        private void RenderPreview()
        {
            if (!isInitalized)
                return;

            Pages.Children.Clear();
            expanders.Clear();

            var range = GetPageRange();

            if (ValidatePageRange(range))
            {
                for (int i = range.Item1 - 1; i < range.Item2; i++)
                {
                    var page = pages[i];
                    int pageIndex = pages.IndexOf(page) + 1;
                    var frame = new Frame() { Content = PageHelper.ClonePage(page, true) };
                    var expander = new Expander()
                    {
                        Header = $"{Properties.Resources.strPage} {pageIndex}",
                        Foreground = new SolidColorBrush(Colors.Black),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Width = page.Canvas.Width
                    };
                    expander.IsExpanded = false;
                    expander.Content = frame;

                    expanders.Add(expander);
                    Pages.Children.Add(expander);
                }

                TextInvalidSettings.Text = string.Empty;
            }
            else
                TextInvalidSettings.Text = Properties.Resources.strExportDialogInvalidSettings;
        }

        private (int, int) GetPageRange()
        {
            switch (SelectedExportMode)
            {
                case ExportMode.AllPages: return (1, pages.Count);
                case ExportMode.CurrentPage: return (pages.IndexOf(currentPage) + 1, pages.IndexOf(currentPage) + 1);
                case ExportMode.SelectedPageRange: return (UpDownFrom.Value, UpDownTo.Value);
                case ExportMode.SinglePage: return (UpDownSinglePage.Value, UpDownSinglePage.Value);
            }

            return (0, 0);
        }

        private bool ValidatePageRange((int, int) range)
        {
            bool result = false;

            switch (SelectedExportMode)
            {
                case ExportMode.AllPages: result = true; break;
                case ExportMode.CurrentPage: result = true; break;
                case ExportMode.SelectedPageRange:
                    {
                        if (!(range.Item1 <= 0 || range.Item2 > pages.Count || range.Item2 <= 0 || range.Item2 < range.Item1))
                            result = true;
                    }
                    break;
                case ExportMode.SinglePage:
                    {
                        if (range.Item1 == range.Item2 && (!(range.Item1 < 1 || range.Item2 > pages.Count)))
                            result = true;
                    }
                    break;
            }

            return result;
        }

        private void CmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RenderPreview();
        }

        private void DisplayInvalidInput()
        {
            MessageBox.Show(owner, Properties.Resources.strInvalidInput, Properties.Resources.strInvalidInputTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Export

        private bool Export(int from, int to)
        {
            SaveFileDialog ofd = new SaveFileDialog() { Filter = $"{Properties.Resources.strImages}|*.png;*.tif;*.bmp;*.jpg;*.jpeg" };
            var result = ofd.ShowDialog();

            if (result.HasValue && result.Value)
                return ExportImages(ofd.FileName, from, to);

            return false;
        }

        private bool ExportImages(string path, int from, int to)
        {
            MainGrid.IsEnabled = false;
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

                if (!ExportPageAsImage(pages[from].Canvas, path, from + 1))
                    return false;

                Title = $"{Properties.Resources.strExport} ...";
            }
            else
            {
                for (int i = from; i <= to; i++)
                {
                    string nPath = path.Replace(oldExt, string.Empty);
                    int displayNumber = i + 1;

                    nPath += $".{displayNumber}{oldExt}";

                    if (!ExportPageAsImage(pages[i].Canvas, nPath, displayNumber))
                        return false;

                    Title = $"{Properties.Resources.strExport} ... {Properties.Resources.strPage} {displayNumber}/{to + 1}";
                }
                Title = $"{Properties.Resources.strExport} ({Properties.Resources.strPage} {to + 1}/{to + 1})";
            }

            MainGrid.IsEnabled = true;
            Title = Properties.Resources.strExportPages;
            MessageBox.Show(owner, Properties.Resources.strExportFinished, Properties.Resources.strSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        private bool ExportJournal(int from, int to)
        {
            SaveFileDialog ofd = new SaveFileDialog() { Filter = $"{Properties.Resources.strJournalFile}|*.journal;" };
            var dialogResult = ofd.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                MainGrid.IsEnabled = false;

                Journal journal = new Journal();
                for (int i = from; i <= to; i++)
                {
                    var currentCanvas = pages[i].Canvas;

                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(owner, $"{Properties.Resources.strExportFailed} {i + 1}: {ex.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    int displayNumber = i + 1;
                    Title = $"{Properties.Resources.strExport} ... {Properties.Resources.strPage} {displayNumber}/{to + 1}";
                }
                Title = $"{Properties.Resources.strExport} ({Properties.Resources.strPage} {to + 1}/{to + 1})";

                journal.Save(ofd.FileName);
                MainGrid.IsEnabled = true;
                Title = Properties.Resources.strExportPages;
                MessageBox.Show(owner, Properties.Resources.strExportFinished, Properties.Resources.strSuccess, MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// See https://stackoverflow.com/a/19534008/6237448
        /// </summary>
        /// <param name="element"></param>
        /// <param name="scale"></param>
        /// <param name="background"></param>
        /// <returns></returns>
        public static RenderTargetBitmap RenderToBitmap(UIElement element, double scale, Brush background)
        {
            var renderWidth = (int)(element.RenderSize.Width * scale);
            var renderHeight = (int)(element.RenderSize.Height * scale);

            var renderTarget = new RenderTargetBitmap(renderWidth, renderHeight, 96, 96, PixelFormats.Default);
            var sourceBrush = new VisualBrush(element);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            var rect = new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(background, null, rect);
                drawingContext.DrawRectangle(sourceBrush, null, rect);
            }

            renderTarget.Render(drawingVisual);

            return renderTarget;
        }

        private bool ExportPageAsImage(DrawingCanvas canvas, string path, int page)
        {
            try
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                RenderTargetBitmap rtb = RenderToBitmap(canvas, 1.0, new SolidColorBrush(Colors.White));

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

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(owner, $"{Properties.Resources.strExportFailed} {page}: {e.Message}", Properties.Resources.strFailure, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        #endregion

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var range = GetPageRange();
            if (!ValidatePageRange(range))
            {
                DisplayInvalidInput();
                return;
            }

            bool exportAsJournal = CheckBoxExportAsJournal.IsChecked.Value;
            bool result = false;
            int currentPageIdx = pages.IndexOf(currentPage);

            switch (SelectedExportMode)
            {
                case ExportMode.AllPages: result = (exportAsJournal ? ExportJournal(0, pages.Count - 1) : Export(0, pages.Count - 1)); break;
                case ExportMode.CurrentPage: result = (exportAsJournal ? ExportJournal(currentPageIdx, currentPageIdx) : Export(currentPageIdx, currentPageIdx)); break;
                case ExportMode.SinglePage:
                case ExportMode.SelectedPageRange: result = (exportAsJournal ? ExportJournal(range.Item1 - 1, range.Item2 - 1) : Export(range.Item1 - 1, range.Item2 - 1)); break;
            }

            // Update title (reset)
            Title = Properties.Resources.strExportPages;

            if (result)
                DialogClosed?.Invoke(this, true);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            expanders.ForEach(ex => ex.IsExpanded = false);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            expanders.ForEach(ex => ex.IsExpanded = true);
        }

        private void CheckBoxExportAsJournal_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized)
                return;

            Settings.Instance.ExportAsJournal = CheckBoxExportAsJournal.IsChecked.Value;
            Settings.Instance.Save();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogClosed?.Invoke(this, false);
        }
    }

    #region Converter
    public class SelectedModeToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExportMode exportMode)
                return (int)exportMode;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
                return (ExportMode)i;

            return ExportMode.AllPages;
        }
    }


    public class SelectedModeToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExportMode mode && int.TryParse(parameter.ToString(), out int index) && (int)mode == index)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
