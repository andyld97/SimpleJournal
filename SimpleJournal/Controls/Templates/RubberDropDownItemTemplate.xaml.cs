using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für RubberDropDownItemTemplate.xaml
    /// </summary>
    public partial class RubberDropDownItemTemplate : DropDownTemplate
    {
        private readonly ToggleButton[] buttons = new ToggleButton[] { };

        public delegate void onChangedRubber(int sizeIndex, bool? rectangle);
        public event onChangedRubber OnChangedRubber;

        public RubberDropDownItemTemplate()
        {
            InitializeComponent();

            buttons = new ToggleButton[]
            {
                btnSize1,
                btnSize2,
                btnSize3,
                btnSize4,
                btnSize5,
            };

            // Default shape is a rectangle
            changeShapes(true);
        }

        private void BtnSize1_Click(object sender, RoutedEventArgs e)
        {
            fireOnChange(0);
        }

        private void BtnSize2_Click(object sender, RoutedEventArgs e)
        {
            fireOnChange(1);
        }

        private void BtnSize3_Click(object sender, RoutedEventArgs e)
        {
            fireOnChange(2);
        }

        private void BtnSize4_Click(object sender, RoutedEventArgs e)
        {
            fireOnChange(3);
        }

        private void BtnSize5_Click(object sender, RoutedEventArgs e)
        {
            fireOnChange(4);
        }

        private void fireOnChange(int sizeIndex = - 1, bool? rectangle = null)
        {
            // Display correct toggle button state
            if (sizeIndex != -1)
            {
                foreach (ToggleButton btn in buttons)
                    btn.IsChecked = false;
                buttons[sizeIndex].IsChecked = true;
            }

            // Later in the event handler in Window2.xaml.cs do this for all canvas:
            //applyRubberSizeAndShape(canvas);
            OnChangedRubber?.Invoke(sizeIndex, rectangle);

            CloseDropDown();
        }

        private void BtnShapeCircle_Click(object sender, RoutedEventArgs e)
        {
            btnShapeCircle.IsChecked = true;
            btnShapeRectangle.IsChecked = false;

            fireOnChange(-1, false);

            // Also changes shapes from togglebuttons
            changeShapes(false);
        }

        private void BtnShapeRectangle_Click(object sender, RoutedEventArgs e)
        {
            btnShapeCircle.IsChecked = false;
            btnShapeRectangle.IsChecked = true;

            fireOnChange(-1, true);

            // Also changes shapes from togglebuttons
            changeShapes(true);
        }

        private void changeShapes(bool rectangle)
        {
            foreach (ToggleButton tb in buttons)
            {
                if (tb.Content is Shape sh)
                {
                    if (rectangle && !(sh is Rectangle))
                    {
                        // Switch to rectangle
                        tb.Content = new Rectangle() { Width = sh.Width, Height = sh.Height, Stroke = sh.Stroke, StrokeThickness = sh.StrokeThickness, Fill = sh.Fill };
                    }
                    else if (!rectangle && !(sh is Ellipse))
                    {
                        // Switch to ellipse
                        tb.Content = new Ellipse() { Width = sh.Width, Height = sh.Height, Stroke = sh.Stroke, StrokeThickness = sh.StrokeThickness, Fill = sh.Fill };
                    }
                }
            }
        }
    }
}
