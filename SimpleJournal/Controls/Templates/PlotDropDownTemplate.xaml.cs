using SimpleJournal.Data;
using SimpleJournal.Common;
using SimpleJournal.Documents.UI;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für PlotDropDownTemplate.xaml
    /// </summary>
    public partial class PlotDropDownTemplate : Templates.DropDownTemplate
    {
        public delegate void onPlotModeChanged(PlotMode plotMode);
        public event onPlotModeChanged OnPlotModeChanged;

        private readonly bool isInitalized = false;

        public PlotDropDownTemplate()
        {
            InitializeComponent();
            isInitalized = true;

            CmbChoosePlotMode.SelectedIndex = (int)Settings.Instance.LastSelectedPlotMode;
        }

        private void CmbChoosePlotMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInitalized)
                return;

            PlotMode mode = (PlotMode)CmbChoosePlotMode.SelectedIndex;

            Settings.Instance.LastSelectedPlotMode = mode;
            Settings.Instance.Save();

            OnPlotModeChanged?.Invoke(mode);
        }
    }
}