using SimpleJournal.Controls;
using System.Windows.Controls;

namespace SimpleJournal.Templates
{
    /// <summary>
    /// Interaktionslogik für Ruled.xaml
    /// </summary>
    public partial class Ruled : Page, IPaper 
    {
        public Ruled()
        {
            InitializeComponent();
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Ruled;

        public DrawingCanvas Canvas => this.canvas;

        public PageSplitter Border { get; set; }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);
        }
    }
}
