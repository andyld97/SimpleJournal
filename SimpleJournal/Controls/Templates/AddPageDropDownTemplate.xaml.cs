using SimpleJournal.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für AddPageDropDownTemplate.xaml
    /// </summary>
    public partial class AddPageDropDownTemplate : DropDownTemplate
    {
        public delegate void addPage(PaperType paperType);
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

            ListBoxPageType.SelectedIndex = index;
        }
        private void ListBoxPageType_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
                {
                    int index = ListBoxPageType.Items.IndexOf(item);

                    if (index >= 0)
                        Run(index);
                }
            }
        }

        private void Run(int index)
        {
            switch (index)
            {
                case 0: Settings.Instance.PaperTypeLastInserted = PaperType.Chequeued; break;
                case 1: Settings.Instance.PaperTypeLastInserted = PaperType.Dotted; break;
                case 2: Settings.Instance.PaperTypeLastInserted = PaperType.Ruled; break;
                case 3: Settings.Instance.PaperTypeLastInserted = PaperType.Blanco; break;
            }
            Settings.Instance.Save();

            CloseDropDown();
            AddPage?.Invoke(Settings.Instance.PaperTypeLastInserted);
        }
    }
}
