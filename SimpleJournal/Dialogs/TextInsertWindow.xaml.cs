using System.Windows;
using System.Windows.Input;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für TextInsertWindow.xaml
    /// </summary>
    public partial class TextInsertWindow : Window
    {
        public string Result => txtResult.Text;

        public TextInsertWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DismissDialog();
        }

        private void DismissDialog()
        {
            if (!string.IsNullOrEmpty(Result))
                DialogResult = true;
            else
                MessageBox.Show(this, Properties.Resources.strPleaseEnterValidText, Properties.Resources.strEmptyText, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DismissDialog();
        }
    }
}
