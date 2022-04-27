using SimpleJournal.Data;
using SimpleJournal.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SimpleJournal.Documents;
using SimpleJournal.Documents.UI.Extensions;
using SimpleJournal.Documents.UI;
using SimpleJournal.Documents.UI.Controls.Paper;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaction logic for PageManagmentControl.xaml
    /// </summary>
    public partial class PageManagmentControl : UserControl, IDialog
    {  
        #region Private Members
        private List<IPaper> pages = null;
        private double scaleFactor = 1.0;
        private double[] zoomValues = { 1.0, 1.2, 1.5, 1.8, 2.0 };
        private bool isInitalized = false;
        private bool ignoreCheckedChanged = false;
        private int changesMade = 0;
        private ToggleButton[] toggleButtons = null;
        private static Dictionary<UIElement, string> toolTips = new Dictionary<UIElement, string>();
        private Window owner;
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
                else if (ToggleButtonDotted.IsChecked.Value)
                    return PaperType.Dotted;

                return PaperType.Ruled;
            }
        }

        public EventHandler<bool> DialogClosed { get; set; }

        public EventHandler<string> TitleChanged { get; set; }

        #endregion

        #region Ctor

        public PageManagmentControl()
        {
            InitializeComponent();
        }

        public void Initalize(List<IPaper> pages, Window owner)
        {
            this.owner = owner;
            toggleButtons = new ToggleButton[] { ToggleButtonDotted, ToggleButtonChequered, ToggleButtonRuled, ToggleButtonBlanko };
            this.pages = pages;

            RefreshListView();

            switch (Settings.Instance.PaperType)
            {
                case PaperType.Blanco: ToggleButtonBlanko.IsChecked = true; break;
                case PaperType.Chequeued: ToggleButtonChequered.IsChecked = true; break;
                case PaperType.Ruled: ToggleButtonRuled.IsChecked = true; break;
                case PaperType.Dotted: ToggleButtonDotted.IsChecked = true; break;
            }

            // Select first page at start
            if (pages.Count > 0)
                ListViewPages.SelectedIndex = 0;

            isInitalized = true;

            UIElement[] btns = new UIElement[] {
                ToggleButtonBlanko,
                ToggleButtonChequered,
                ToggleButtonRuled,
                ToggleButtonDotted,
                btnIncreaseZoom,
                btnDecreaseZoom,
                ButtonClearPage,
                ButtonDeletePage,

                ButtonInsertPageBeforeIndexChequered,
                ButtonInsertPageBeforeIndexDotted,
                ButtonInsertPageBeforeIndexBlanko,
                ButtonInsertPageBeforeIndexRuled,

                ButtonInsertPageAfterIndexChequered,
                ButtonInsertPageAfterIndexDotted,
                ButtonInsertPageAfterIndexBlanko,
                ButtonInsertPageAfterIndexRuled,

                ButtonMovePageUp,
                ButtonMovePageDown,

                ButtonInsertTop,
                ButtonInsertDown
            };

            toolTips = new Dictionary<UIElement, string>()
            {
                { ToggleButtonBlanko, Properties.Resources.strPageManagmentToggleButtonBlanko },
                { ToggleButtonChequered, Properties.Resources.strPageManagmentToggleButtonChequered },
                { ToggleButtonRuled, Properties.Resources.strPageManagmentToggleButtonRuled },
                { ToggleButtonDotted, Properties.Resources.strPageManagmentToggleButtonDotted },
                { btnIncreaseZoom, Properties.Resources.strPageManagmentButtonIncreaseZoom },
                { btnDecreaseZoom, Properties.Resources.strPageManagmentButtonDecreaseZoom },
                { ButtonClearPage, Properties.Resources.strPageManagmentButtonClearPage },
                { ButtonDeletePage, Properties.Resources.strPageManagmentButtonDeletePage },
                { ButtonInsertPageBeforeIndexChequered, Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexChequered },
                { ButtonInsertPageBeforeIndexDotted, Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexDotted },
                { ButtonInsertPageBeforeIndexBlanko, Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexBlanko },
                { ButtonInsertPageBeforeIndexRuled, Properties.Resources.strPageManagmentButtonInsertPageBeforeIndexRuled },
                { ButtonInsertPageAfterIndexChequered, Properties.Resources.strPageManagmentButtonInsertPageAfterIndexChequered },
                { ButtonInsertPageAfterIndexDotted, Properties.Resources.strPageManagmentButtonInsertPageAfterIndexDotted },
                { ButtonInsertPageAfterIndexBlanko, Properties.Resources.strPageManagmentButtonInsertPageAfterIndexBlanko },
                { ButtonInsertPageAfterIndexRuled, Properties.Resources.strPageManagmentButtonInsertPageAfterIndexRuled },
                { ButtonMovePageUp, Properties.Resources.strPageManagmentButtonMovePageUp },
                { ButtonMovePageDown, Properties.Resources.strPageManagmentButtonMovePageDown },
                { ButtonInsertTop, Properties.Resources.strPageManagmentButtonInsertTop },
                { ButtonInsertDown, Properties.Resources.strPageManagmentButtonInsertBottom },
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

            if (displayFrame.Child is IPaper paper)
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
            string result = toolTips[sender as UIElement];

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
                case PaperType.Dotted: template = new Dotted(); break;
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
                changesMade++;
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
                changesMade++;
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
                var template = page.ClonePage(true);
                displayFrame.Child = template as UserControl;
                ignoreCheckedChanged = true;

                if (template.Type == PaperType.Custom)
                    return;

                ToggleButton tb = null;
                switch (template.Type)
                {
                    case PaperType.Blanco: tb = ToggleButtonBlanko; break;
                    case PaperType.Chequeued: tb = ToggleButtonChequered; break;
                    case PaperType.Ruled: tb = ToggleButtonRuled; break;
                    case PaperType.Dotted: tb = ToggleButtonDotted; break;
                }

                tb.IsChecked = true;
                foreach (var button in toggleButtons.Where(p => p != tb))
                    button.IsChecked = false;

                ignoreCheckedChanged = false;
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitalized || ignoreCheckedChanged || CurrentSelectedPaperType == PaperType.Custom)
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
                    changesMade++;
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
            if (changesMade == 0)
            {
                // No changes made, so just quit the dialog directly (with a false dialog result)
                DialogClosed?.Invoke(this, false);
                return;
            }
            IsEnabled = false;

            // Convert pages to JournalPages for Result
            try
            {
                List<JournalPage> resultPages = new List<JournalPage>();

                foreach (var page in pages)
                {
                    JournalPage jp = null;
                    if (page is Custom pdf)
                        jp = new PdfJournalPage { PaperPattern = PaperType.Custom, PageBackground = pdf.PageBackground, Orientation = pdf.Orientation };
                    else
                        jp = new JournalPage { PaperPattern = page.Type };

                    using (MemoryStream ms = new MemoryStream())
                    {
                        page.Canvas.Strokes.Save(ms);
                        jp.Data = ms.ToArray();

                        // Check for additional ressources
                        if (page.Canvas.Children.Count > 0)
                        {
                            foreach (UIElement element in page.Canvas.Children)
                            {
                                var result = element.ConvertFromUIElement();
                                if (result != null)
                                    jp.JournalResources.Add(result);
                            }
                        }
                    }

                    resultPages.Add(jp);
                }


                Result = resultPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strPageManagmentControl_SaveError, ex.Message), SharedResources.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
                IsEnabled = true;
                return;
            }

            IsEnabled = true;
            DialogClosed?.Invoke(this, true);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogClosed?.Invoke(this, false);
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
                    if (MessageBox.Show(owner, Properties.Resources.strWantToDeletePage, Properties.Resources.strWantToDeletePageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                        changesMade++;
                    }
                }
                else
                    MessageBox.Show(owner, Properties.Resources.strJournalNeedsOnePageMinimum, Properties.Resources.strJournalNeedsOnePageMinimumTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClearPage_Click(object sender, RoutedEventArgs e)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                if (MessageBox.Show(owner, Properties.Resources.strWantToClearPage, Properties.Resources.strWantToClearPageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var page = pages[idx];
                    page.Canvas.Strokes.Clear();
                    page.Canvas.Children.Clear();

                    RefreshListView();
                    ListViewPages.SelectedIndex = idx;
                    FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
                    changesMade++;
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
                changesMade++;
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
                changesMade++;
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
                changesMade++;
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
                changesMade++;
            }
        }
        #endregion

        #region InsertPageBefore
        private void ButtonInsertPageBeforeIndexChequered_Click(object sender, RoutedEventArgs e)
        {
            InsertPageBeforeIndex(PaperType.Chequeued);
        }

        private void ButtonInsertPageBeforeIndexDotted_Click(object sender, RoutedEventArgs e)
        {
            InsertPageBeforeIndex(PaperType.Dotted);
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

        private void ButtonInsertPageAfterIndexDotted_Click(object sender, RoutedEventArgs e)
        {
            InsertPageAfterIndex(PaperType.Dotted);
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