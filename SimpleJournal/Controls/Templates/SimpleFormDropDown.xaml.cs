using System.Windows.Controls.Primitives;
using static SimpleJournal.Data.Enums;

namespace SimpleJournal.Controls.Templates
{
    /// <summary>
    /// Interaktionslogik für SimpleFormDropDown.xaml
    /// </summary>
    public partial class SimpleFormDropDown : DropDownTemplate
    {
        private ShapeType shapeType = ShapeType.Rectangle;

        public delegate void onSimpleFormDropDownChanged(ShapeType shapeType);
        public event onSimpleFormDropDownChanged OnSimpleFormDropDownChanged;

        private readonly ToggleButton[] buttons;

        public SimpleFormDropDown()
        {
            InitializeComponent();

            buttons = new ToggleButton[]
            {
                btnShapeRectangle,
                btnShapeCircle,
                btnShapeTriangleEquilateral,
                btnShapeTriangleEquilateral180,
                btnShapeTriangleLeft90,
                btnShapeTriangleRight90,
                btnShapeTriangleLeft180,
                btnShapeTriangleRight180,
                btnShapeTrapeze
            };
        }

        public ShapeType ShapeType
        {
            get => shapeType;
            set
            {
                if (value != shapeType)
                {
                    shapeType = value;
                    OnSimpleFormDropDownChanged?.Invoke(value);
                }
            }
        }

        private void SwitchState(ToggleButton btn)
        {
            foreach (var button in buttons)
                button.IsChecked = false;

            if (btn != null)
                btn.IsChecked = true;
        }

        private void btnShapeRectangle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.Rectangle;
        }

        private void btnShapeCircle_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.Circle;
        }

        private void btnShapeTriangleEquilateral_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.TriangleEquiliteral;
        }

        private void btnShapeTriangleEquilateral180_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.TriangleEquiliteral180;
        }

        private void btnShapeTriangleLeft90_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.TriangleLeft90;
        }

        private void btnShapeTriangleRight90_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.TriangleRight90;
        }

        private void btnShapeTriangleLeft180_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.TriangleLeft180;
        }

        private void btnShapeTriangleRight180_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.TriangleRight180;
        }

        private void btnShapeTrapeze_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SwitchState(sender as ToggleButton);
            ShapeType = ShapeType.Trapeze;
        }
    }
}