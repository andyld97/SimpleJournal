using SimpleJournal.Documents.UI.Extensions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleJournal.Controls
{
    /// <summary>
    /// Interaktionslogik für ColorPalette.xaml
    /// </summary>
    public partial class ColorPalette : UserControl
    {
        private bool ignoreSelectionChanged = false;

        public delegate void onColorChanged(Color color);
        public event onColorChanged OnColorChanged;

        public static Color[] StandardColorsPalette;

        static ColorPalette()
        {
            MahApps.Metro.Controls.BuildInColorPalettes.WpfColorsPalette.Remove(System.Windows.Media.Colors.Transparent);

            // Ignore first transparent entry
            StandardColorsPalette = new Color[MahApps.Metro.Controls.BuildInColorPalettes.StandardColorsPalette.Length - 1];
            for (int i = 1; i <= StandardColorsPalette.Length; i++)
                StandardColorsPalette[i - 1] = MahApps.Metro.Controls.BuildInColorPalettes.StandardColorsPalette[i];

            // Restore old SimpleJournal colors!
            StandardColorsPalette[5] = Consts.PEN_COLORS[1].ToColor();
            StandardColorsPalette[10] = Consts.PEN_COLORS[2].ToColor();
            StandardColorsPalette[14] = Consts.PEN_COLORS[3].ToColor();

         
        }

        public ColorPalette()
        {
            InitializeComponent();

            // FIX for MahAppMetro theming issues related due to Colorful-Theme
            var accentColorHover = (Color)FindResource("Fluent.Ribbon.Colors.Accent40");
            var accentColorSelected = (Color)FindResource("Fluent.Ribbon.Colors.Accent60");

            MahApps.Metro.Controls.ItemHelper.SetHoverBackgroundBrush(this, new SolidColorBrush(accentColorHover));
            MahApps.Metro.Controls.ItemHelper.SetSelectedBackgroundBrush(this, new SolidColorBrush(accentColorSelected));
            MahApps.Metro.Controls.ItemHelper.SetHoverSelectedBackgroundBrush(this, new SolidColorBrush(accentColorSelected));
        }

        public void SetSelectedColor(Color color)
        {
            ignoreSelectionChanged = true;

            DefaultColors.UnselectAll();
            AdvancedColors.UnselectAll();

            if (StandardColorsPalette.Any(p => p == color))
                DefaultColors.SelectedItem = color;
            else
            {
                Expander.IsExpanded = true;
                AdvancedColors.SelectedItem = color;
            }
            ignoreSelectionChanged = false;
        }

        private void AdvancedColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged)
                return;

            ignoreSelectionChanged = true;
            DefaultColors.UnselectAll();
            ignoreSelectionChanged = false;

            OnColorChanged?.Invoke((Color)AdvancedColors.SelectedItem);
        }

        private void DefaultColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreSelectionChanged)
                return;

            ignoreSelectionChanged = true;
            AdvancedColors.UnselectAll();
            ignoreSelectionChanged = false;

            OnColorChanged?.Invoke((Color)DefaultColors.SelectedItem);
        }
    }
}