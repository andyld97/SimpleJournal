using SimpleJournal.Controls;
using SimpleJournal.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für SetupDialog.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        private readonly bool isInitalized = false;
        private int currentPage = 0;
        private bool clickedOnExit = false;

        public Grid[] pagesArr = null;
        public Data.Pen[] pens = new Data.Pen[Consts.AMOUNT_PENS];
        private int currentlySelectedPen = 0;
        private readonly PreviewCanvas[] previewCanvas = null;

        public SetupDialog()
        {
            InitializeComponent();
            pagesArr = new Grid[] { page1, page2, page3, page4, page5, page6, page7 };
            previewCanvas = new PreviewCanvas[]
            {
                formatPreviewCanvas,
                preasurePreviewCanvas,
                previewPensCanvas,
                previewCanvasTextMarker,
                previewInputGesture
            };

            previewCanvasTextMarker.DrawingAttributes.IsHighlighter = true;

            InitalizePens();
            isInitalized = true;

            // Make sure first pen is also applied to preview canvas6
            cmbPens.SelectedIndex = 1;
            cmbPens.SelectedIndex = 0;

            colPickerTextMarker.SelectedColor = Consts.TextMarkerColor;
            cmbStrokeSizeTextMarker.SelectedIndex = 0;

            LoadSettings();
            InitalizePreviewCanvasTextMarker();
            InitalizePreviewInputGesture();
        }

        private void InitalizePreviewInputGesture()
        {
            previewInputGesture.Canvas.SetFreeHandDrawingMode();
            previewInputGesture.Canvas.PreviewCircleCorrection = (cmbCircleCorrection.SelectedIndex == 0);
            previewInputGesture.Canvas.PreviewRotationCorrection = (cmbRotationCorrection.SelectedIndex == 0);
        }

        private void InitalizePreviewCanvasTextMarker()
        {
            var size = Consts.TextMarkerSizes[cmbStrokeSizeTextMarker.SelectedIndex];

            previewCanvasTextMarker.DrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Rectangle;
            previewCanvasTextMarker.DrawingAttributes.IsHighlighter = true;
            previewCanvasTextMarker.DrawingAttributes.Width = size.Height;
            previewCanvasTextMarker.DrawingAttributes.Height = size.Width;
            previewCanvasTextMarker.DrawingAttributes.Color = Colors.Yellow;
            previewCanvasTextMarker.EnableWriting = true;
            previewCanvasTextMarker.AddChild(new TextBlock() { Text = Properties.Resources.strTextToHightlight, FontSize = 18 });
        }

        private void InitalizePens()
        {
            // Initalize pens
            for (int i = 0; i < pens.Length; i++)
            {
                pens[i] = new Data.Pen(Consts.PEN_COLORS[i], Consts.StrokeSizes[0].Height);
            }

            try
            {
                if (System.IO.File.Exists(Consts.PenSettingsFilePath))
                {
                    var result = Serialization.Serialization.Read<Data.Pen[]>(Consts.PenSettingsFilePath, Serialization.Serialization.Mode.Normal);
                    if (result != null)
                        pens = result;
                }
                else
                {
                    SavePenSettings();
                }
            }
            catch
            {
                // Do something?
            }
        }

        #region Settings: Load/Save

        private void LoadSettings()
        {
            cmbFormat.SelectedIndex = (int)Settings.Instance.PaperType;
            cmbHDResolution.SelectedIndex = (Settings.Instance.UserHasSelectedHDScreen ? 0 : 1);
            cmbPreasure.SelectedIndex = (Settings.Instance.UsePreasure ? 0 : 1);
            cmbCircleCorrection.SelectedIndex = (Settings.Instance.UseCircleCorrection ? 0 : 1);
            cmbRotationCorrection.SelectedIndex = (Settings.Instance.UseRotateCorrection ? 0 : 1);
            cmbFitToFurve.SelectedIndex = (Settings.Instance.UseFitToCurve ? 0 : 1);
            cmbHDResolution.SelectedIndex = (Settings.Instance.UserHasSelectedHDScreen ? 0 : 1);
            cmbDarkMode.SelectedIndex = (Settings.Instance.UseDarkMode ? 0 : 1);
        }

        private void SavePenSettings()
        {
            try
            {
                Serialization.Serialization.Save<Data.Pen[]>(Consts.PenSettingsFilePath, pens, Serialization.Serialization.Mode.Normal);
            }
            catch
            { }
        }

        private void SaveSettings()
        {
            // Save all items to settings
            if (cmbHDResolution.SelectedIndex == 0)
            {
                // YES // HD 
                Settings.Instance.EnlargeScrollbar = false;
                Settings.Instance.DisplaySidebarAutomatically = true;
            }
            else
            {
                // No HD
                Settings.Instance.EnlargeScrollbar = true;
                Settings.Instance.DisplaySidebarAutomatically = false;
            }

            // Apply text marker attributes
            Settings.Instance.TextMarkerColor = new Data.Color(colPickerTextMarker.SelectedColor.R, colPickerTextMarker.SelectedColor.G, colPickerTextMarker.SelectedColor.B);
            Settings.Instance.TextMarkerSize = Consts.TextMarkerSizes[cmbStrokeSizeTextMarker.SelectedIndex];

            //Settings.Instance.PaperFormat = Format.A4;
            Settings.Instance.PaperType = (PaperType)cmbFormat.SelectedIndex;
            Settings.Instance.UsePreasure = (cmbPreasure.SelectedIndex == 0);
            Settings.Instance.UseCircleCorrection = (cmbCircleCorrection.SelectedIndex == 0);
            Settings.Instance.UseRotateCorrection = (cmbRotationCorrection.SelectedIndex == 0);
            Settings.Instance.UseFitToCurve = (cmbFitToFurve.SelectedIndex == 0);
            Settings.Instance.UserHasSelectedHDScreen = (cmbHDResolution.SelectedIndex == 0);
            Settings.Instance.UseDarkMode = (cmbDarkMode.SelectedIndex == 0);

            Settings.Instance.Save();
            GeneralHelper.ApplyTheming();
        }
        #endregion

        public int CurrentPage
        {
            get => currentPage;
            set
            {
                if (value == currentPage)
                    return;
                else if (value >= pagesArr.Length - 1)
                {
                    currentPage = pagesArr.Length - 1;
                    btnForwards.Content = Properties.Resources.strFinish;
                }
                else if (value < 0)
                {
                    currentPage = 0;
                    btnForwards.Content = Properties.Resources.strNext;
                }
                else
                {
                    currentPage = value;
                    btnForwards.Content = Properties.Resources.strNext;
                }

                for (int i = 0; i < pagesArr.Length; i++)
                {
                    if (i != currentPage)
                        pagesArr[i].Visibility = Visibility.Collapsed;
                    else
                        pagesArr[i].Visibility = Visibility.Visible;
                }
            }
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, Properties.Resources.strSetupExitDialog, Properties.Resources.strSetupExitDialogTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                clickedOnExit = true;
                this.Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (clickedOnExit)
                e.Cancel = false;
            else
            {
                if (MessageBox.Show(this, Properties.Resources.strSetupExitDialog, Properties.Resources.strSetupExitDialogTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
            }
        }

        private void btnBackwards_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
        }

        private void btnForwards_Click(object sender, RoutedEventArgs e)
        {
            if (btnForwards.Content.ToString() == Properties.Resources.strFinish)
            {
                SaveSettings();

                // Notify Window2 that the settings changed
                MainWindow.W_INSTANCE.ApplySettings();

                clickedOnExit = true;
                this.Close();
            }

            CurrentPage++;
        }

        private void CmbFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
            {
                foreach (PreviewCanvas pc in previewCanvas)
                    pc.PaperType = (PaperType)cmbFormat.SelectedIndex;

                InitalizePreviewCanvasTextMarker();
                InitalizePreviewInputGesture();
            }
        }

        private void CmbPreasure_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
            {
                bool ignorePressure = (cmbPreasure.SelectedIndex == 1);
                foreach (PreviewCanvas pc in previewCanvas)
                    pc.DrawingAttributes.IgnorePressure = ignorePressure;
            }
        }

        private void cmbFitToFurve_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (isInitalized)
            {
                bool useFitToCurve = cmbFitToFurve.SelectedIndex == 0;
                foreach (PreviewCanvas pc in previewCanvas)
                    pc.DrawingAttributes.FitToCurve = useFitToCurve;
            }
        }

        private void ColorPicker_ColorChanged(System.Windows.Media.Color c)
        {
            foreach (PreviewCanvas pc in previewCanvas)
            {
                pc.DrawingAttributes.Color = c;
            }
            pens[currentlySelectedPen].FontColor = new Data.Color(c.A, c.R, c.G, c.B);
            SavePenSettings();
        }

        private void CmbStrokeSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
            {
                var size = Consts.StrokeSizes[cmbStrokeSize.SelectedIndex].Width;
                foreach (PreviewCanvas pc in previewCanvas.Except(new List<PreviewCanvas>() { previewCanvasTextMarker }))
                    pc.DrawingAttributes.Width = pc.DrawingAttributes.Height = size;

                pens[currentlySelectedPen].Size = size;
                SavePenSettings();
            }
        }

        private void CmbPens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
            {
                // Apply values to comboxbox
                currentlySelectedPen = cmbPens.SelectedIndex;

                Data.Pen currentPen = pens[currentlySelectedPen];

                colorPicker.SelectedColor = currentPen.FontColor.ToColor();
                cmbStrokeSize.SelectedIndex = Consts.StrokeSizes.IndexOf(new Size(currentPen.Size, currentPen.Size));

                foreach (PreviewCanvas pc in previewCanvas.Except(new List<PreviewCanvas>() { previewCanvasTextMarker }))
                {
                    pc.DrawingAttributes.Width = pc.DrawingAttributes.Height = currentPen.Size;
                    pc.DrawingAttributes.Color = currentPen.FontColor.ToColor();
                }
            }
        }

        private void CmbStrokeSizeTextMarker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
            {
                var size = Consts.TextMarkerSizes[cmbStrokeSizeTextMarker.SelectedIndex];
                previewCanvasTextMarker.DrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Rectangle;
                previewCanvasTextMarker.DrawingAttributes.Width = size.Height;
                previewCanvasTextMarker.DrawingAttributes.Height = size.Width;
            }
        }

        private void ColPickerTextMarker_ColorChanged(System.Windows.Media.Color c)
        {
            previewCanvasTextMarker.DrawingAttributes.Color = c;
        }

        private void CmbRotationCorrection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
                InitalizePreviewInputGesture();
        }

        private void CmbCircleCorrection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitalized)
                InitalizePreviewInputGesture();
        }

        private void LnkHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://code-a-software.net/simplejournal/index.php?page=help");
            }
            catch
            {

            }
        }

        private void cmbDarkMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}