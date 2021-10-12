namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog
    {
        public ExportDialog()
        {
            InitializeComponent();

            exportControl.TitleChanged += delegate (object sender, string e)
            {
                Title = e;
            };

            exportControl.DialogClosed += delegate (object sender, bool e)
            {
                DialogResult = e;
            };
        }
    }
}