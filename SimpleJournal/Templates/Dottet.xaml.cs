using SimpleJournal.Controls;
using System.Windows.Controls;

namespace SimpleJournal.Templates
{
    /// <summary>
    /// Interaction logic for Dottet.xaml
    /// </summary>
    public partial class Dottet : Page, IPaper
    {
        public Dottet()
        {
            InitializeComponent();
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Dottet;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);
        }
    }
}
