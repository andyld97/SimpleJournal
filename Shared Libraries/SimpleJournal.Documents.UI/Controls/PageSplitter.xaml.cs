using SimpleJournal.Common;
using System.Windows;
using System.Windows.Controls;
using Orientation = SimpleJournal.Common.Orientation;

namespace SimpleJournal.Documents.UI.Controls
{
    /// <summary>
    /// Interaction logic for PageSplitter.xaml
    /// </summary>
    public partial class PageSplitter : UserControl
    {
        public delegate void onPageAdded(PageSplitter sender, PaperType paperType, Orientation orientation);
        public event onPageAdded OnPageAdded;

        public PageSplitter()
        {
            InitializeComponent();
        }

        private void ButtonChequered_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Chequeued);
        }

        private void ButtonRuled_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e,  PaperType.Ruled);
        }

        private void ButtonBlanko_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Blanco);
        }

        private void ButtonDotted_Click(object sender, RoutedEventArgs e)
        {
            ShowContextMenu(sender, e, PaperType.Dotted);  
        }

        private void ShowContextMenu(object sender, RoutedEventArgs e, PaperType type)
        {
            var btn = sender as TransparentImageButton;
            ContextMenu contextMenu = btn.ContextMenu;
            contextMenu.PlacementTarget = btn;
            contextMenu.IsOpen = true;
            contextMenu.Tag = type;
            e.Handled = true;
        }

        private void RaisePageAddedEvent(PaperType paperType, Orientation orientation)
        {
            OnPageAdded?.Invoke(this, paperType, orientation);
        }

        private void MenuButtonPortrait_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem rm && rm.Parent is ContextMenu cm && cm.Tag is PaperType pt)
                RaisePageAddedEvent(pt, Orientation.Portrait);
        }

        private void MenuButtonLandscape_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem rm && rm.Parent is ContextMenu cm && cm.Tag is PaperType pt)
                RaisePageAddedEvent(pt, Orientation.Landscape);
        }
    }
}
