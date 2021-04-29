using SimpleJournal.Data;
using SimpleJournal.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für PageManagmentDialog.xaml
    /// </summary>
    public partial class PageManagmentDialog : Window
    {
        #region Private Members
        private readonly List<IPaper> pages = null;
        private double scaleFactor = 1.0;
        private readonly double[] zoomValues = { 1.0, 1.2, 1.5, 1.8, 2.0 };
        private readonly bool isInitalized = false;
        private bool ignoreCheckedChanged = false;
        private readonly ToggleButton[] toggleButtons = null;
        #endregion

        #region Properties

        /// <summary>
        /// The journal pages collection which is the result of the user input of this dialog
        /// </summary>
        public List<JournalPage> Result { get; private set; }

        /// <summary>
        /// The currently selected paper type (determined by the togglebutton which is checked)
        /// </summary>
        private PaperType CurrentSelectedPaperType
        {
            get
            {
                if (ToggleButtonBlanko.IsChecked.Value)
                    return PaperType.Blanco;
                else if (ToggleButtonChequered.IsChecked.Value)
                    return PaperType.Chequeued;
                else
                    return PaperType.Ruled;
            }
        }

        #endregion

        #region Remove Minimize-Button

        private const int GWL_STYLE = -16, WS_MAXIMIZEBOX = 0x10000, WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        /// <summary>
        /// Hides the Minimize and Maximize buttons in a Window. Must be called in the constructor.
        /// </summary>
        /// <param name="window">The Window whose Minimize/Maximize buttons will be hidden.</param>
        public static void HideMinimizeButton(Window window)
        {
            window.SourceInitialized += (s, e) => {
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                int currentStyle = GetWindowLong(hwnd, GWL_STYLE);

                SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX);
            };
        }

        #endregion

        #region Ctor
        public PageManagmentDialog(List<IPaper> pages)
        {
            InitializeComponent();
            HideMinimizeButton(this);
            toggleButtons = new ToggleButton[] { ToggleButtonBlanko, ToggleButtonChequered, ToggleButtonRuled };
            this.pages = pages;

            RefreshListView();

            switch (Settings.Instance.PaperType)
            {
                case PaperType.Blanco: ToggleButtonBlanko.IsChecked = true; break;
                case PaperType.Chequeued: ToggleButtonChequered.IsChecked = true; break;
                case PaperType.Ruled: ToggleButtonRuled.IsChecked = true; break;
            }

            // Select first page at start
            if (pages.Count > 0)
                ListViewPages.SelectedIndex = 0;

            isInitalized = true;

            UIElement[] btns = new UIElement[] {
                ToggleButtonBlanko,
                ToggleButtonChequered,
                ToggleButtonRuled,
                btnIncreaseZoom,
                btnDecreaseZoom,
                ButtonClearPage,
                ButtonDeletePage,

                ButtonInsertPageBeforeIndexChequered,
                ButtonInsertPageBeforeIndexBlanko,
                ButtonInsertPageBeforeIndexRuled,

                ButtonInsertPageAfterIndexChequered,
                ButtonInsertPageAfterIndexBlanko,
                ButtonInsertPageAfterIndexRuled,

                ButtonMovePageUp,
                ButtonMovePageDown,

                ButtonInsertTop,
                ButtonInsertDown
            };

            // Add events for buttons (tooltip)
            foreach (var element in btns)
            {
                element.MouseMove += delegate (object sender, MouseEventArgs e) { OnMouseHover(sender); };
                element.MouseLeave += delegate (object sender, MouseEventArgs e) { OnMouseLeave(sender); };
            }

            // Make sure zoom label will display the correct zoom value
            TextZoomLabel.Text = string.Format(Properties.Resources.strPageManagmentZoomLabel, scaleFactor * 100);
        }
        #endregion

        #region Private Methods
        private void RefreshListView()
        {
            ListViewPages.Items.Clear();

            int pageCount = 0;
            foreach (var page in pages)
                ListViewPages.Items.Add($"{Properties.Resources.strPage} {++pageCount}");
        }

        private void ZoomByScale(double scale)
        {
            // Calculate originX and originY
            double centerX = 0.0;
            double centerY = 0.0;

            if (displayFrame.Content is IPaper paper)
            {
                centerX = paper.Canvas.ActualWidth * 0.5;
                centerY = paper.Canvas.ActualHeight * 0.5;
            }

            displayFrame.LayoutTransform = new ScaleTransform(scale, scale, centerX, centerY);
            displayFrameScrollViewer.UpdateLayout();
            displayFrameScrollViewer.ScrollToVerticalOffset((displayFrameScrollViewer.VerticalOffset / scaleFactor) * scale);
            TextZoomLabel.Text = string.Format(Properties.Resources.strPageManagmentZoomLabel, scale * 100);
            scaleFactor = scale;
        }

        private void OnMouseHover(object sender)
        {
            // Display a kind of a tooltip at the bottom, right gray bar (when hovering buttons to explain them)

            string result = string.Empty;
            if (sender == ToggleButtonBlanko)
                result = Properties.Resources.strPageManagmentToggleButtonBlanko;
            else if (sender == ToggleButtonChequered)
                result = Properties.Resources.strPageManagmentToggleButtonChequered;
            else if (sender == ToggleButtonRuled)
                result = Properties.Resources.strPageManagmentToggleButtonRuled;
            else if (sender == btnIncreaseZoom)
                result = Properties.Resources.strPageManagmentButtonIncreaseZoom;
            else if (sender == btnDecreaseZoom)
                result = Properties.Resources.strPageManagmentButtonDecreaseZoom;
            else if (sender == ButtonClearPage)
                result = Properties.Resources.strPageManagmentButtonClearPage;
            else if (sender == ButtonDeletePage)
                result = Properties.Resources.strPageManagmentButtonDeletePage;
            else if (sender == ButtonInsertPageBeforeIndexChequered)
                result = Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexChequered;
            else if (sender == ButtonInsertPageBeforeIndexBlanko)
                result = Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexBlanko;
            else if (sender == ButtonInsertPageBeforeIndexRuled)
                result = Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexRuled;
            else if (sender == ButtonInsertPageAfterIndexChequered)
                result = Properties.Resources.strPageManagmentButtonInsertPageAfterIndexChequered;
            else if (sender == ButtonInsertPageAfterIndexBlanko)
                result = Properties.Resources.strPageManagmentButtonInsertPageAfterIndexBlanko;
            else if (sender == ButtonInsertPageAfterIndexRuled)
                result = Properties.Resources.strPageManagmentButtonInsertPageAfterIndexRuled;
            else if (sender == ButtonMovePageUp)
                result = Properties.Resources.strPageManagmentButtonMovePageUp;
            else if (sender == ButtonMovePageDown)
                result = Properties.Resources.strPageManagmentButtonMovePageDown;
            else if (sender == ButtonInsertTop)
                result = Properties.Resources.strPageManagmentButtonInsertTop;
            else if (sender == ButtonInsertDown)
                result = Properties.Resources.strPageManagmentButtonInsertBottom;

            TextInfoLabel.Text = $"{Properties.Resources.strHint}: {result}";
            TextInfoLabel.Visibility = Visibility.Visible;
        }

        private void OnMouseLeave(object sender)
        {
            TextInfoLabel.Visibility = Visibility.Hidden;
        }

        private IPaper CreateEmptyTemplate(PaperType paperType)
        {
            IPaper template = null;
            switch (paperType)
            {
                case PaperType.Blanco: template = new Blanco(); break;
                case PaperType.Chequeued: template = new Chequered(); break;
                case PaperType.Ruled: template = new Ruled(); break;
            }

            // Make sure canvas is non editable
            template.Canvas.EditingMode = InkCanvasEditingMode.None;
            return template;
        }

        private void InsertPageBeforeIndex(PaperType type)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                // Insert with CurrentSelectedPaperType
                var template = CreateEmptyTemplate(type);
                pages.Insert(idx, template);
                RefreshListView();

                ListViewPages.SelectedIndex = idx;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
            }
        }

        private void InsertPageAfterIndex(PaperType type)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                // Insert with CurrentSelectedPaperType
                var template = CreateEmptyTemplate(type);
                pages.Insert(idx + 1, template);
                RefreshListView();

                ListViewPages.SelectedIndex = idx + 1;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
            }
        }
        #endregion

        #region Events 

        #region ListViewPages_IndexChanged / ToggleButtonChanged
        private void ListViewPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewPages.SelectedIndex != -1)
            {
                var page = pages[ListViewPages.SelectedIndex];
                var template = PageHelper.ClonePage(page, true);
                displayFrame.Content = template;
                ignoreCheckedChanged = true;

                ToggleButton tb = null;
                switch (template.Type)
                {
                    case PaperType.Blanco: tb = ToggleButtonBlanko; break;
                    case PaperType.Chequeued: tb = ToggleButtonChequered; break;
                    case PaperType.Ruled: tb = ToggleButtonRuled; break;
                }

                tb.IsChecked = true;
                foreach (var button in toggleButtons.Where(p => p != tb))
                    button.IsChecked = false;

                ignoreCheckedChanged = false;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized || ignoreCheckedChanged)
                return;

            var tbSender = (sender as ToggleButton);

            ignoreCheckedChanged = true;

            if (!tbSender.IsChecked.Value)
                tbSender.IsChecked = true;

            foreach (var tb in toggleButtons.Where(p => p != tbSender))
                tb.IsChecked = false;

            if (ListViewPages.SelectedIndex >= 0)
            {
                // Change current page
                int idx = ListViewPages.SelectedIndex;
                var page = pages[idx];
                if (page.Type != CurrentSelectedPaperType)
                {
                    // Refresh page 
                    var nPage = CreateEmptyTemplate(CurrentSelectedPaperType);

                    // Move strokes
                    nPage.Canvas.Strokes = page.Canvas.Strokes.Clone();

                    // Move uiElements
                    List<UIElement> uIElementsToMove = new List<UIElement>();
                    foreach (var element in page.Canvas.Children)
                        uIElementsToMove.Add(element);

                    foreach (var element in uIElementsToMove)
                    {
                        page.Canvas.Children.Remove(element);
                        nPage.Canvas.Children.Add(element);
                    }

                    pages[idx] = nPage;
                    RefreshListView();
                    ListViewPages.SelectedIndex = idx;
                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
                }
            }
            ignoreCheckedChanged = false;
        }
        #endregion

        #region Zoom Buttons
        private void BtnDecreaseZoom_Click(object sender, RoutedEventArgs e)
        {
            int index = (zoomValues.ToList().IndexOf(scaleFactor) - 1);
            if (index < 0)
                index = zoomValues.Length - 1;
            ZoomByScale(zoomValues[index]);
        }

        private void BtnIncreaseZoom_Click(object sender, RoutedEventArgs e)
        {
            int index = (zoomValues.ToList().IndexOf(scaleFactor) + 1) % zoomValues.Length;
            ZoomByScale(zoomValues[index]);           
        }
        #endregion

        #region OK / Cancel

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            // Convert pages to JournalPages for Result
            List<JournalPage> resultPages = new List<JournalPage>();

            foreach (var page in pages)
            {
                JournalPage jp = new JournalPage { PaperPattern = page.Type };

                using (MemoryStream ms = new MemoryStream())
                {
                    page.Canvas.Strokes.Save(ms);                    
                    jp.SetData(ms.ToArray());

                    // Check for additional ressources
                    if (page.Canvas.Children.Count > 0)
                    {
                        foreach (UIElement element in page.Canvas.Children)
                        {
                            var result = JournalResource.ConvertFromUIElement(element);
                            if (result != null)
                                jp.JournalResources.Add(result);
                        }
                    }
                }

                resultPages.Add(jp);
                Result = resultPages;
            }

            this.IsEnabled = true;            
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #endregion

        #region Delete/Clear Page

        private void ButtonDeletePage_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                if (pages.Count - 1 >= 1)
                {
                    if (MessageBox.Show(this, Properties.Resources.strWantToDeletePage, Properties.Resources.strWantToDeletePageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        pages.RemoveAt(idx);
                        RefreshListView();

                        // Select the next index to prevent jumping to start (so that the user can contiune work from this index)
                        // Check if index + 1 is in range
                        if (idx + 1 < pages.Count)
                            ListViewPages.SelectedIndex = idx + 1;
                        else if (idx - 1 < pages.Count)
                            ListViewPages.SelectedIndex = idx - 1;
                        else // We don't need to check for pages.Count == 0, because a journal must consist at least of one page!
                            ListViewPages.SelectedIndex = 0;

                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
                    }
                }
                else
                {
                    MessageBox.Show(this, Properties.Resources.strJournalNeedsOnePageMinimum, Properties.Resources.strJournalNeedsOnePageMinimumTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonClearPage_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                if (MessageBox.Show(this, Properties.Resources.strWantToClearPage, Properties.Resources.strWantToClearPageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var page = pages[idx];
                    page.Canvas.Strokes.Clear();
                    page.Canvas.Children.Clear();

                    RefreshListView();
                    ListViewPages.SelectedIndex = idx;
                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
                }
            }
        }
        #endregion

        #region Insert Top/Down
        private void ButtonInsertDown_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                // Insert at the bottom
                pages.Move(idx, pages.Count - 1);
                RefreshListView();

                ListViewPages.SelectedIndex = pages.Count - 1;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
            }
        }

        private void ButtonInsertTop_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                // Insert at the bottom
                pages.Move(idx, 0);
                RefreshListView();

                ListViewPages.SelectedIndex = 0;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
            }
        }

        #endregion

        #region MovePageUp/Down
        private void ButtonMovePageUp_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx > 0)
            {
                pages.Move(idx, idx - 1);
                RefreshListView();

                ListViewPages.SelectedIndex = idx - 1;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
            }
        }

        private void ButtonMovePageDown_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0 && idx != pages.Count - 1)
            {
                pages.Move(idx, idx + 1);
                RefreshListView();

                ListViewPages.SelectedIndex = idx + 1;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
            }
        }
        #endregion

        #region InsertPageBefore
        private void ButtonInsertPageBeforeIndexChequered_Click(object sender, RoutedEventArgs e)
        {
            InsertPageBeforeIndex(PaperType.Chequeued);
        }

        private void ButtonInsertPageBeforeIndexRuled_Click(object sender, RoutedEventArgs e)
        {
            InsertPageBeforeIndex(PaperType.Ruled);
        }

        private void ButtonInsertPageBeforeIndexBlanko_Click(object sender, RoutedEventArgs e)
        {
            InsertPageBeforeIndex(PaperType.Blanco);
        }
        #endregion

        #region InsertPageAfter
        private void ButtonInsertPageAfterIndexBlanko_Click(object sender, RoutedEventArgs e)
        {
            InsertPageAfterIndex(PaperType.Blanco);
        }

        private void ButtonInsertPageAfterIndexRuled_Click(object sender, RoutedEventArgs e)
        {
            InsertPageAfterIndex(PaperType.Ruled);
        }

        private void ButtonInsertPageAfterIndexChequered_Click(object sender, RoutedEventArgs e)
        {
            InsertPageAfterIndex(PaperType.Chequeued);
        }
        #endregion

        #endregion
    }
}