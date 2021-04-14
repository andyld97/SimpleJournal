using System.Windows;
using System.Windows.Controls;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für PageSplitter.xaml
    /// </summary>
    public partial class PageSplitter : UserControl
    {
        public delegate void onPageAdded(PageSplitter sender, PaperType paperType);
        public event onPageAdded OnPageAdded;

        public PageSplitter()
        {
            InitializeComponent();
        }

        private void ButtonChequered_Click(object sender, RoutedEventArgs e)
        {
            OnPageAdded?.Invoke(this, PaperType.Chequeued);
        }

        private void ButtonRuled_Click(object sender, RoutedEventArgs e)
        {
            OnPageAdded?.Invoke(this, PaperType.Ruled);
        }

        private void ButtonBlanko_Click(object sender, RoutedEventArgs e)
        {
            OnPageAdded?.Invoke(this, PaperType.Blanco);
        }
    }
}
