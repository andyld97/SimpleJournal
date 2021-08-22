namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für PlotDropDownTemplate.xaml
    /// </summary>
    public partial class PlotDropDownTemplate : Templates.DropDownTemplate
    {
        public delegate void onPlotModeChanged(PlotMode plotMode);
        public event onPlotModeChanged OnPlotModeChanged;

        public PlotDropDownTemplate()
        {
            InitializeComponent();
        }

        private void CheckBoxNegativePlot_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            OnPlotModeChanged?.Invoke(CheckBoxNegativePlot.IsChecked.Value ? PlotMode.Negative : PlotMode.Positive);
        }
    }
}