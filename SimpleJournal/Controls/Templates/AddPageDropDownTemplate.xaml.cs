using SimpleJournal.Common;
using System.Windows.Controls;
using SimpleJournal.Documents.UI;
using Orientation = SimpleJournal.Common.Orientation;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für AddPageDropDownTemplate.xaml
    /// </summary>
    public partial class AddPageDropDownTemplate : DropDownTemplate
    {
        public delegate void addPage(PaperType paperType, Orientation orientation);
        public event addPage AddPage;
        private bool ignoreSelectionChanged = false;

        public AddPageDropDownTemplate()
        {
            InitializeComponent();
        }

        public override void OnDropDownOpened()
        {
            base.OnDropDownOpened();

            int index;
            if (Settings.Instance.PaperTypeLastInserted == PaperType.Chequered) index = 0;
            else if (Settings.Instance.PaperTypeLastInserted == PaperType.Dotted) index = 2;
            else if (Settings.Instance.PaperTypeLastInserted == PaperType.Ruled) index = 4;
            else index = 6;

            if (Settings.Instance.OrientationLastInserted == Orientation.Landscape)
                index++;

            ignoreSelectionChanged = true;
            ListViewAddPages.SelectedIndex = index;
            ignoreSelectionChanged = false;
        }

        private void RaiseAddPage(PaperType paperType, Orientation orientation)
        {
            Settings.Instance.PaperTypeLastInserted = paperType;
            Settings.Instance.OrientationLastInserted = orientation;
            Settings.Instance.Save();

            CloseDropDown();
            AddPage?.Invoke(Settings.Instance.PaperTypeLastInserted, orientation);
        }

        private void ListViewAddPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ListViewAddPages.SelectedIndex;
            if (ignoreSelectionChanged || index == -1)
                return;            

            // 0|1
            // 2|3
            // 4|5
            // 6|7

            Orientation orientation = (index % 2 == 0 ? Orientation.Portrait : Orientation.Landscape);
            PaperType paperType = PaperType.Chequered;

            if (index == 0 || index == 1) paperType = PaperType.Chequered;
            if (index == 2 || index == 3) paperType = PaperType.Dotted;
            if (index == 4 || index == 5) paperType = PaperType.Ruled;
            if (index == 6 || index == 7) paperType = PaperType.Blanco;

            RaiseAddPage(paperType, orientation);
        }
    }
}