using System.Windows;
using System.Windows.Input;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window
    {
        public string Result => txtResult.Text;

        public TextInputWindow()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DismissDialog();
        }

        private void txtResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DismissDialog();
        }

        private void DismissDialog()
        {
            if (!string.IsNullOrEmpty(Result))
                this.DialogResult = true;
            else
                MessageBox.Show(this, Properties.Resources.strPleaseEnterValidText, Properties.Resources.strEmptyText, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
