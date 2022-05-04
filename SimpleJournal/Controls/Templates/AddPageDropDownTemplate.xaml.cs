using SimpleJournal.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public AddPageDropDownTemplate()
        {
            InitializeComponent();
        }

        public override void OnDropDownOpened()
        {
            base.OnDropDownOpened();

            int index;
            if (Settings.Instance.PaperTypeLastInserted == PaperType.Chequeued) index = 0;
            else if (Settings.Instance.PaperTypeLastInserted == PaperType.Dotted) index = 1;
            else if (Settings.Instance.PaperTypeLastInserted == PaperType.Ruled) index = 2;
            else index = 3;

            if (Settings.Instance.OrientationLastInserted == Orientation.Portrait)
                ListBoxPageType.SelectedIndex = index;
            else
                ListBoxPageTypeLandsacpe.SelectedIndex = index;
        }

        private void ListBoxPageType_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
            {
                int index = ListBoxPageType.Items.IndexOf(item);

                ListBoxPageTypeLandsacpe.UnselectAll();

                if (index >= 0)
                    RaiseAddPage(index, Orientation.Portrait);
            }
        }

        private void ListBoxPageTypeLandsacpe_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
            {
                int index = ListBoxPageTypeLandsacpe.Items.IndexOf(item);

                ListBoxPageType.UnselectAll();

                if (index >= 0)
                    RaiseAddPage(index, Orientation.Landscape);
            }
        }

        private void RaiseAddPage(int index, Orientation orientation)
        {
            switch (index)
            {
                case 0: Settings.Instance.PaperTypeLastInserted = PaperType.Chequeued; break;
                case 1: Settings.Instance.PaperTypeLastInserted = PaperType.Dotted; break;
                case 2: Settings.Instance.PaperTypeLastInserted = PaperType.Ruled; break;
                case 3: Settings.Instance.PaperTypeLastInserted = PaperType.Blanco; break;
            }
            Settings.Instance.OrientationLastInserted = orientation;
            Settings.Instance.Save();

            CloseDropDown();
            AddPage?.Invoke(Settings.Instance.PaperTypeLastInserted, orientation);
        }
    }
}