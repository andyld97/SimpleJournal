using System.Windows;
using System.Windows.Input;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window
    {
        public TextInputWindow()
        {
            InitializeComponent();
        }

        public string Result => txtResult.Text;

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            dismissDialog();
        }

        private void txtResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                dismissDialog();
        }

        private void dismissDialog()
        {
            if (!string.IsNullOrEmpty(Result))
                this.DialogResult = true;
            else
                MessageBox.Show(this, Properties.Resources.strPleaseEnterValidText, Properties.Resources.strEmptyText, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
