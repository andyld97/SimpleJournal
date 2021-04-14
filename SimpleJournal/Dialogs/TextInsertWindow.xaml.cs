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

        private void btnChangeFont_Click(object sender, RoutedEventArgs e)
        {
            // ToDo: Implement later
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            dismissDialog();
        }

        private void dismissDialog()
        {
            if (!string.IsNullOrEmpty(Result))
                this.DialogResult = true;
            else
                MessageBox.Show(this, Properties.Resources.strPleaseEnterValidText, Properties.Resources.strEmptyText, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtResult_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                dismissDialog();
        }
    }
}
