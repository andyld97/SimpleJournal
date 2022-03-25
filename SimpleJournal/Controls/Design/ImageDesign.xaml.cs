using SimpleJournal.Common;
using System.Windows.Controls;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für ImageDesign.xaml
    /// </summary>
    public partial class ImageDesign : UserControl
    {
        public delegate void changed(int? angle, Stretch? stretch);
        public event changed OnChanged;

        private bool ignoreEvents = false;

        public ImageDesign()
        {
            InitializeComponent();
        }

        public void Load(int angle, System.Windows.Media.Stretch stretch)
        {
            ignoreEvents = true;

            // Assign values
            numAngle.Value = angle;
            ComboBoxStretch.SelectedIndex = (stretch == System.Windows.Media.Stretch.Fill ? 0 : 1);

            ignoreEvents = false;
        }

        private void NumAngle_OnChanged(int oldValue, int newValue)
        {
            if (ignoreEvents)
                return;

            OnChanged?.Invoke(newValue, null);
        }

        private void ComboBoxStretch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreEvents)
                return;

            Stretch stretch = (ComboBoxStretch.SelectedIndex == 0 ? Stretch.Fill : Stretch.Uniform);
            OnChanged?.Invoke(null, stretch);
        }
    }
}
