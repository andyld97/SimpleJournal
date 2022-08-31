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
using Orientation = SimpleJournal.Common.Orientation;
using System.Windows.Data;
using System.Globalization;
using SimpleJournal.Documents.UI.Helper;
using SimpleJournal.Documents.Pattern;

namespace SimpleJournal.Modules
{
    /// <summary>
    /// Interaction logic for PageManagmentModule.xaml
    /// </summary>
    public partial class PageManagmentModule : UserControl, IModule
    {
        #region Private Members
        private List<IPaper> pages = null;
        private double scaleFactor = 1.0;
        private double[] zoomValues = { 0.2, 0.5, 1.0, 1.2, 1.5, 1.8, 2.0 };
        private bool isInitalized = false;
        private bool ignoreCheckedChanged = false;
        private int changesMade = 0;
        private ToggleButton[] toggleButtons = null;
        private static Dictionary<UIElement, string> toolTips = new Dictionary<UIElement, string>();
        private Window owner;

        private ChequeredPattern chequeredPattern;
        private DottedPattern dottedPattern;
        private RuledPattern ruledPattern;
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
                    return PaperType.Chequered;
                else if (ToggleButtonDotted.IsChecked.Value)
                    return PaperType.Dotted;

                return PaperType.Ruled;
            }
        }

        public EventHandler<bool> ModuleClosed { get; set; }

        public EventHandler<string> TitleChanged { get; set; }

        public string Title => string.Empty;

        public Common.Data.Size WindowSize => null;

        #endregion

        #region Ctor

        public PageManagmentModule()
        {
            InitializeComponent();
            ResetHoverText();
        }

        public void Initalize(List<IPaper> pages, ChequeredPattern chequeredPattern, DottedPattern dottedPattern, RuledPattern ruledPattern, Window owner)
        {
            this.owner = owner;
            this.chequeredPattern = chequeredPattern;
            this.dottedPattern = dottedPattern;
            this.ruledPattern = ruledPattern;

            toggleButtons = new ToggleButton[] { ToggleButtonDotted, ToggleButtonChequered, ToggleButtonRuled, ToggleButtonBlanko };
            this.pages = pages;

            RefreshListView();

            switch (Settings.Instance.PaperType)
            {
                case PaperType.Blanco: ToggleButtonBlanko.IsChecked = true; break;
                case PaperType.Chequered: ToggleButtonChequered.IsChecked = true; break;
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
                ButtonDeletePage,
                ButtonRotatePage,

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
                { ButtonRotatePage, Properties.Resources.strPageManagmentDialogButtonRotatePage}
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
            ListViewPages.ItemsSource = null;
            ListViewPages.ItemsSource = pages;
            MouseDown += PageManagmentControl_MouseDown;
        }

        private void PageManagmentControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Fix for triggering re-initalization of this control!
            // No mouse down events will be redirected to the parent of this control.
            e.Handled = true;
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

            TextInfoLabel.Text = $"{Properties.Resources.strHint}: {result}!";
        }

        private void OnMouseLeave(object sender)
        {
            ResetHoverText();
        }

        private void ResetHoverText()
        {
            TextInfoLabel.Text = Properties.Resources.strPageManagmentDialog_HoverDefaultText;
        }

        private IPaper CreateEmptyTemplate(PaperType paperType, Orientation orientation)
        {
            IPaper template = null;
            switch (paperType)
            {
                case PaperType.Blanco: template = new Blanco(orientation); break;
                case PaperType.Chequered:
                    {
                        template = new Chequered(orientation);
                        template.ApplyPattern(chequeredPattern);
                    }
                    break;
                case PaperType.Ruled:
                    {
                        template = new Ruled(orientation);
                        template.ApplyPattern(ruledPattern);
                    }
                    break;
                case PaperType.Dotted:
                    {
                        template = new Dotted(orientation);
                        template.ApplyPattern(dottedPattern);
                    }
                    break;
            }

            // Make sure canvas is non editable
            template.Canvas.EditingMode = InkCanvasEditingMode.None;
            return template;
        }

        private void InsertPageBeforeIndex(PaperType type, Orientation orientation)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                // Insert with CurrentSelectedPaperType
                var template = CreateEmptyTemplate(type, orientation);
                pages.Insert(idx, template);
                RefreshListView();

                ListViewPages.SelectedIndex = idx;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ListViewPages), ListViewPages);
                changesMade++;
            }
        }

        private void InsertPageAfterIndex(PaperType type, Orientation orientation)
        {
            int idx = ListViewPages.SelectedIndex;
            if (idx >= 0)
            {
                // Insert with CurrentSelectedPaperType
                var template = CreateEmptyTemplate(type, orientation);
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
                    case PaperType.Chequered: tb = ToggleButtonChequered; break;
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
                    var nPage = CreateEmptyTemplate(CurrentSelectedPaperType, page.Orientation);

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
                ModuleClosed?.Invoke(this, false);
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
                        jp = new JournalPage { PaperPattern = page.Type, Orientation = page.Orientation };

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
            ModuleClosed?.Invoke(this, true);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ModuleClosed?.Invoke(this, false);
        }

        #endregion

        #region Delete/Clear Page

        private void DeletePage()
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

        private void ClearPage()
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

        private void ButtonDeletePage_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewPages.SelectedItem == null)
                return;

            ShowContextMenu(sender, e);
        }

        private void MenuButtonDeletePage_Click(object sender, RoutedEventArgs e)
        {
            DeletePage();
        }

        private void MenuButtonClearPage_Click(object sender, RoutedEventArgs e)
        {
            ClearPage();
        }

        #endregion

        private void ShowContextMenu(object sender, RoutedEventArgs e, PaperType type, bool before)
        {
            var btn = sender as FrameworkElement;
            ContextMenu contextMenu = btn.ContextMenu;
            contextMenu.PlacementTarget = btn;
            contextMenu.IsOpen = true;
            contextMenu.Tag = (type, before);
            e.Handled = true;
        }

        private void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            var btn = sender as FrameworkElement;
            ContextMenu contextMenu = btn.ContextMenu;
            contextMenu.PlacementTarget = btn;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }


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
            ShowContextMenu(sender, e, PaperType.Chequered, true);
        }

        private void ButtonInsertPageBeforeIndexDotted_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Dotted, true);
        }

        private void ButtonInsertPageBeforeIndexRuled_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Ruled, true);
        }

        private void ButtonInsertPageBeforeIndexBlanko_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Blanco, true);
        }
        #endregion

        #region InsertPageAfter
        private void ButtonInsertPageAfterIndexBlanko_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Blanco, false);
        }

        private void ButtonInsertPageAfterIndexDotted_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Dotted, false);
        }

        private void ButtonInsertPageAfterIndexRuled_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Ruled, false);
        }

        private void ButtonInsertPageAfterIndexChequered_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Chequered, false);
        }
        #endregion

        private void MenuButtonPortrait_Click(object sender, RoutedEventArgs e)
        {
            HandleMenuButtonClick(sender, Orientation.Portrait);
        }

        private void MenuButtonLandscape_Click(object sender, RoutedEventArgs e)
        {
            HandleMenuButtonClick(sender, Orientation.Landscape);
        }

        private void HandleMenuButtonClick(object sender, Orientation orientation)
        {
            if (sender is MenuItem rm && rm.Parent is ContextMenu cm && cm.Tag is (PaperType pt, bool before))
            {
                if (before)
                    InsertPageBeforeIndex(pt, orientation);
                else
                    InsertPageAfterIndex(pt, orientation);
            }
        }

        #endregion

        private void ButtonRotatePage_Click(object sender, RoutedEventArgs e)
        {
            int index = ListViewPages.SelectedIndex;

            if (index == -1)
                return;

            if (MessageBox.Show(Properties.Resources.strPageManagmentDialog_RotatePage_Message, Properties.Resources.strPageManagmentDialog_RotatePage_Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            var pg = pages[index];
            if (pg.Orientation == Orientation.Portrait)
                pg.Orientation = Orientation.Landscape;
            else
                pg.Orientation = Orientation.Portrait;

            RefreshListView();
            ListViewPages.SelectedIndex = index;
            changesMade++;
        }
    }

    #region Converter

    public class PageToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IPaper jp)
            {                
                string icon = jp.Type.ToString().ToLower();

                if (jp.Orientation == Orientation.Landscape)
                    icon += "_landscape";

                icon += ".png";

                return ImageHelper.LoadImage(new Uri($"pack://application:,,,/SimpleJournal.SharedResources;component/icons/pages/{icon}"));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is IPaper paper && values[1] is ItemCollection ic)
                return System.Convert.ToString(ic.IndexOf(paper) + 1);

            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}