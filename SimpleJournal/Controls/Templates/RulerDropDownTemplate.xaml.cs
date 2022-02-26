using SimpleJournal.Data;
using SimpleJournal.Common;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleJournal.Documents.UI;
using SimpleJournal.Documents.UI.Helper;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für RulerDropDownTemplate.xaml
    /// </summary>
    public partial class RulerDropDownTemplate : DropDownTemplate
    {
        public delegate void onChangedRulerMode(RulerMode mode);
        public event onChangedRulerMode OnChangedRulerMode;

        private readonly bool isInitalized = false;

        public RulerDropDownTemplate()
        {
            InitializeComponent();

            lstBoxChooseRulerMode.SelectedIndex = (int)Settings.Instance.RulerStrokeMode;
            isInitalized = true;
        }

        public void SetColor(System.Windows.Media.Color c)
        {
            rectDashedStroke.Stroke = lineDottetStroke.Stroke = rectFullStroke.Stroke = new SolidColorBrush(c);
            lstBoxChooseRulerMode.Background = new SolidColorBrush(c.BuildConstrastColor());
        }

        private void LstBoxChooseRulerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitalized)
                return;
            if (lstBoxChooseRulerMode.SelectedIndex == -1)
                return;

            OnChangedRulerMode?.Invoke((RulerMode)lstBoxChooseRulerMode.SelectedIndex);
            this.CloseDropDown();
        }

        public override void OnDropDownOpened()
        {
            
        }
    }
}
