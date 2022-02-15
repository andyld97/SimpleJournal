using SimpleJournal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleJournal.Documents.UI.Controls
{
    /// <summary>
    /// Interaction logic for PageSplitter.xaml
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

        private void ButtonDotted_Click(object sender, RoutedEventArgs e)
        {
            OnPageAdded?.Invoke(this, PaperType.Dotted);
        }
    }
}
