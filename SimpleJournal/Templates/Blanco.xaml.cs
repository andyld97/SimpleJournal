using SimpleJournal.Controls;
using System.Windows.Controls;

namespace SimpleJournal
{
    /// <summary>
    /// Interaktionslogik für Blanco.xaml
    /// </summary>
    public partial class Blanco : Page, IPaper
    {
        public Blanco()
        {
            InitializeComponent();
        }

        public Format Format => Format.A4;

        public PaperType Type => PaperType.Blanco;

        public DrawingCanvas Canvas => canvas;

        public PageSplitter Border { get; set; }

        public void SetDebug(bool state = true)
        {
            Canvas.SetDebug(state);   
        }
    }
}
