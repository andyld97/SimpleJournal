using System.Windows;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für WaitingDialog.xaml
    /// </summary>
    public partial class WaitingDialog : Window
    {
        private readonly string text = Properties.Resources.strWatingText;

        public WaitingDialog(string documentName)
        {
            InitializeComponent();

            Title = string.Format(text, documentName);
        }

        public void SetProgress(double value, int pageFrom, int pageTo)
        {
            Dispatcher.Invoke(() => {

                ProgressTotal.IsIndeterminate = false;
                ProgressTotal.Maximum = 1;
                ProgressTotal.Value = value;

                TextCurrentStatus.Text = string.Format(Properties.Resources.strWaitingDialogReadingPage, pageFrom, pageTo);
            });            
        }
    }
}
