using System.Windows.Controls;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für ImageDesign.xaml
    /// </summary>
    public partial class ImageDesign : UserControl
    {
        public delegate void changed(int angle);
        public event changed OnChanged;

        public ImageDesign()
        {
            InitializeComponent();
        }

        private void NumAngle_OnChanged(int oldValue, int newValue)
        {
            OnChanged?.Invoke(newValue);
        }
    }
}
