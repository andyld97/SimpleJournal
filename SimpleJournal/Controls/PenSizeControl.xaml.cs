using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für PenSizeControl.xaml
    /// </summary>
    public partial class PenSizeControl : UserControl
    {
        public delegate void switchedPenSize(int size);
        public event switchedPenSize SwitchtedPenSize;
        private readonly ToggleButton[] buttons = null;

        public bool IsToggleable { get; set; } = true;

        public PenSizeControl()
        {
            InitializeComponent();
            buttons = new ToggleButton[] { btnSize1, btnSize2, btnSize3, btnSize4, btnSize5 };
        }

        public void SetIndex(int index)
        {
            // Prevent multiple selection
            foreach (var btn in buttons)
                btn.IsChecked = false;

            if (index != -1 && index < buttons.Length && IsToggleable)
                buttons[index].IsChecked = true;
        }

        public void SetTextMarker()
        {
            int i = 1;
            foreach (ToggleButton btn in buttons)
            {
                if (btn.Content is Ellipse el)
                {
                    btn.Content = new Rectangle() { Width = 10, Height = 4 * i, Margin = el.Margin, Fill = el.Fill, Stroke = el.Stroke, StrokeThickness = el.StrokeThickness };
                    i++;
                }
            }
        }

        public void SetColor(Color color)
        {
            foreach (ToggleButton toggle in buttons)
            {
                if (toggle.Content is Ellipse es)
                {
                    es.Fill = new SolidColorBrush(color);
                    es.StrokeThickness = 0.5;
                }
                else if (toggle.Content is Rectangle rect)
                {
                    rect.Fill = new SolidColorBrush(color);
                    rect.StrokeThickness = 0.5;
                }
            }
        }

        private void RaiseEvent(int button)
        {
            foreach (var toggle in buttons)
                toggle.IsChecked = false;

            if (IsToggleable)
                buttons[button].IsChecked = true;

            SwitchtedPenSize?.Invoke(button);
        }

        private void BtnSize1_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(0);
        }

        private void BtnSize2_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(1);
        }

        private void BtnSize3_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(2);
        }

        private void BtnSize4_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(3);
        }

        private void BtnSize5_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(4);
        }
    }
}
