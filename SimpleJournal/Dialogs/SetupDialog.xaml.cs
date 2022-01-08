using SimpleJournal.Controls;
using SimpleJournal.Controls.Templates;
using SimpleJournal.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Pen = SimpleJournal.Data.Pen;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für SetupDialog.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        private readonly bool isInitalized;
        private int currentPage;
        private bool clickedOnExit;
        private bool ignorePenClickedEvent = false;

        public Grid[] pagesArr;
        public Data.Pen[] pens = new Data.Pen[Consts.AMOUNT_PENS];
        private readonly PreviewCanvas[] previewCanvas;
        private readonly PenDropDownTemplate[] penTemplates = new PenDropDownTemplate[] { new PenDropDownTemplate(), new PenDropDownTemplate(), new PenDropDownTemplate(), new PenDropDownTemplate() };
        private readonly PenDropDownTemplate textMarkerTemplate = new PenDropDownTemplate();

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

            foreach (var prev in previewCanvas)
                prev.PaperType = PaperType.Chequeued;

            btnTextMarker.DropDown = textMarkerTemplate;

            textMarkerTemplate.SetTextMarker();
            textMarkerTemplate.LoadPen(new Pen(Settings.Instance.TextMarkerColor, Settings.Instance.TextMarkerSize.Width, Settings.Instance.TextMarkerSize.Height));
            textMarkerTemplate.OnChangedColorAndSize += TextMarkerTemplate_OnChangedColorAndSize;
            markerPath.Fill = new SolidColorBrush(Settings.Instance.TextMarkerColor.ToColor());

            InitalizePens();
            isInitalized = true;

            LoadSettings();
            InitalizePreviewCanvasTextMarker();
            InitalizePreviewInputGesture();
        }

        #region Text Marker

        private void TextMarkerTemplate_OnChangedColorAndSize(System.Windows.Media.Color? c, int sizeIndex)
        {
            // Apply size index & apply color and size to DrawingAttributes of previewCanvasTextMarker
            if (c.HasValue)
            {
                markerPath.Fill = new SolidColorBrush(c.Value);
                previewCanvasTextMarker.DrawingAttributes.Color = c.Value;
                Settings.Instance.TextMarkerColor = new Data.Color(c.Value);
            }

            if (sizeIndex >= 0)
            {
                var size = Consts.TextMarkerSizes[sizeIndex];
                previewCanvasTextMarker.DrawingAttributes.Width = size.Height;
                previewCanvasTextMarker.DrawingAttributes.Height = size.Width;

                Settings.Instance.TextMarkerSize = size;
            }

            Settings.Instance.Save();
            MainWindow.W_INSTANCE.UpdateTextMarkerAttributes();
        }

        private void InitalizePreviewCanvasTextMarker()
        {
            var size = Settings.Instance.TextMarkerSize;

            previewCanvasTextMarker.ClearCanvas();
            previewCanvasTextMarker.DrawingAttributes.StylusTip = System.Windows.Ink.StylusTip.Rectangle;
            previewCanvasTextMarker.DrawingAttributes.IsHighlighter = true;
            previewCanvasTextMarker.DrawingAttributes.Width = size.Height;
            previewCanvasTextMarker.DrawingAttributes.Height = size.Width;
            previewCanvasTextMarker.DrawingAttributes.Color = Settings.Instance.TextMarkerColor.ToColor();
            previewCanvasTextMarker.EnableWriting = true;
            previewCanvasTextMarker.AddChild(new TextBlock() { Text = Properties.Resources.strTextToHightlight, FontSize = 18 });
        }

        #endregion

        private void InitalizePreviewInputGesture()
        {
            previewInputGesture.Canvas.SetFreeHandDrawingMode();
            previewInputGesture.Canvas.PreviewCircleCorrection = (cmbCircleCorrection.SelectedIndex == 0);
            previewInputGesture.Canvas.PreviewRotationCorrection = (cmbRotationCorrection.SelectedIndex == 0);
        }

        #region Pens

        private int SelectedPen = 0;

        private void InitalizePens()
        {
            // Initalize pens
            pens = Pen.Instance;

            btnPen1.DropDown = penTemplates[0];
            btnPen2.DropDown = penTemplates[1];
            btnPen3.DropDown = penTemplates[2];
            btnPen4.DropDown = penTemplates[3];

            penTemplates[0].OnChangedColorAndSize += BtnPen1_OnChanged;
            penTemplates[1].OnChangedColorAndSize += BtnPen2_OnChanged;
            penTemplates[2].OnChangedColorAndSize += BtnPen3_OnChanged;
            penTemplates[3].OnChangedColorAndSize += BtnPen4_OnChanged;

            UpdatePenButtons();

            try
            {
                if (System.IO.File.Exists(Consts.PenSettingsFilePath))
                {
                    var result = Serialization.Serialization.Read<Data.Pen[]>(Consts.PenSettingsFilePath, Serialization.Serialization.Mode.XML);
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

        private void BtnPen4_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(3, c, sizeIndex);
        }

        private void BtnPen3_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(2, c, sizeIndex);
        }

        private void BtnPen2_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(1, c, sizeIndex);
        }

        private void BtnPen1_OnChanged(System.Windows.Media.Color? c, int sizeIndex)
        {
            ChangePenValues(0, c, sizeIndex);
        }

        private void SelectPen(int index)
        {
            if (!isInitalized || ignorePenClickedEvent)
                return;

            SelectedPen = index;

            DropDownToggleButton[] pensControls = new DropDownToggleButton[] { btnPen1, btnPen2, btnPen3, btnPen4 };

            ignorePenClickedEvent = true;
            foreach (var pen in pensControls)
                pen.IsChecked = false;

            pensControls[index].IsChecked = true;
            ignorePenClickedEvent = false;

            var size = Consts.StrokeSizes[Consts.StrokeSizes.IndexOf(new Size(pens[index].Width, pens[index].Height))];

            var currentDrawingAttributes = previewPensCanvas.DrawingAttributes;
            currentDrawingAttributes.Color = pens[index].FontColor.ToColor();
            currentDrawingAttributes.Width = size.Width;
            currentDrawingAttributes.Height = size.Height;
            previewPensCanvas.DrawingAttributes = currentDrawingAttributes;
        }

        private void btnPen1_Click(object sender, EventArgs e)
        {
            SelectPen(0);
        }

        private void btnPen2_Click(object sender, EventArgs e)
        {
            SelectPen(1);
        }

        private void btnPen3_Click(object sender, EventArgs e)
        {
            SelectPen(2);
        }

        private void btnPen4_Click(object sender, EventArgs e)
        {
            SelectPen(3);
        }

        private void ChangePenValues(int index, System.Windows.Media.Color? c, int sizeIndex)
        {
            var currentDrawingAttributes = previewPensCanvas.DrawingAttributes;

            if (c.HasValue)
            {
                currentDrawingAttributes.Color = c.Value;

                if (SelectedPen != -1)
                    pens[SelectedPen].FontColor = new Data.Color(c.Value.A, c.Value.R, c.Value.G, c.Value.B);
                else
                    pens[index].FontColor = new Data.Color(c.Value.A, c.Value.R, c.Value.G, c.Value.B);


                previewPensCanvas.DrawingAttributes = currentDrawingAttributes;
            }

            if (sizeIndex >= 0)
            {
                pens[index].Width = Consts.StrokeSizes[sizeIndex].Width;
                pens[index].Height = Consts.StrokeSizes[sizeIndex].Height;
                currentDrawingAttributes.Width = pens[index].Width;
                currentDrawingAttributes.Height = pens[index].Height;

                previewPensCanvas.DrawingAttributes = currentDrawingAttributes;
            }

            UpdatePenButtons();          
        }

        private void UpdatePenButtons()
        {
            pathPen1.Fill = new SolidColorBrush(pens[0].FontColor.ToColor());
            pathPen2.Fill = new SolidColorBrush(pens[1].FontColor.ToColor());
            pathPen3.Fill = new SolidColorBrush(pens[2].FontColor.ToColor());
            pathPen4.Fill = new SolidColorBrush(pens[3].FontColor.ToColor());

            penTemplates[0].LoadPen(pens[0]);
            penTemplates[1].LoadPen(pens[1]);
            penTemplates[2].LoadPen(pens[2]);
            penTemplates[3].LoadPen(pens[3]);

            Pen.Instance = pens;
            Pen.Save();
            MainWindow.W_INSTANCE.UpdatePenButtons(pens);
        }

        #endregion

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
                Serialization.Serialization.Save<Data.Pen[]>(Consts.PenSettingsFilePath, pens, Serialization.Serialization.Mode.XML);
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
            // ToDo: ***
            //Settings.Instance.TextMarkerColor = new Data.Color(colPickerTextMarker.SelectedColor.R, colPickerTextMarker.SelectedColor.G, colPickerTextMarker.SelectedColor.B);
            //Settings.Instance.TextMarkerSize = Consts.TextMarkerSizes[cmbStrokeSizeTextMarker.SelectedIndex];

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

        #region Navigation

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
                Close();
            }

            CurrentPage++;
        }

        #endregion

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
            GeneralHelper.OpenUri(new Uri(Consts.HelpUrl));
        }

        private void cmbDarkMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}