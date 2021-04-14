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
        #region Color Definitons
        private readonly Color[] colors = new Color[]
        {
            Colors.White,
            Colors.Black,
            Color.FromRgb(238,236,225),
            Color.FromRgb(31, 73, 125),
            Color.FromRgb(79,129,189),
            Color.FromRgb(192,80,77),
            Color.FromRgb(155,187,89),
            Color.FromRgb(128,100,162),
            Color.FromRgb(75,172,198),
            Color.FromRgb(247,150,70),

            // Second line
            Color.FromRgb(242,242,242),
            Color.FromRgb(128,128,128),
            Color.FromRgb(221,217,195),
            Color.FromRgb(198,217,241),
            Color.FromRgb(220,230,242),
            Color.FromRgb(242,220,219),
            Color.FromRgb(235,241,222),
            Color.FromRgb(230,224,236),
            Color.FromRgb(219,238,244),
            Color.FromRgb(253,234,218),

            // Third line
            Color.FromRgb(217,217,217),
            Color.FromRgb(89,89,89),
            Color.FromRgb(196,189,151),
            Color.FromRgb(142,180,227),
            Color.FromRgb(185,205,229),
            Color.FromRgb(230,185,184),
            Color.FromRgb(215,228,189),
            Color.FromRgb(204,193,218),
            Color.FromRgb(183,222,232),
            Color.FromRgb(252,213,181),

            // Fourth line
            Color.FromRgb(191,191,191),
            Color.FromRgb(64,64,64),
            Color.FromRgb(148,138,84),
            Color.FromRgb(85,142,213),
            Color.FromRgb(149,179,215),
            Color.FromRgb(217,150,148),
            Color.FromRgb(195,214,155),
            Color.FromRgb(179,162,199),
            Color.FromRgb(147,205,221),
            Color.FromRgb(250,192,144),

            // Fifth line
            Color.FromRgb(166,166,166),
            Color.FromRgb(38,38,38),
            Color.FromRgb(74,69,42),
            Color.FromRgb(23,55,94),
            Color.FromRgb(55,96,146),
            Color.FromRgb(149,55,53),
            Color.FromRgb(119,147,60),
            Color.FromRgb(96,74,123),
            Color.FromRgb(49,133,156),
            Color.FromRgb(228,108,10),

            // Sixt line
            Color.FromRgb(128,128,128),
            Color.FromRgb(13,13,13),
            Color.FromRgb(30,28,17),
            Color.FromRgb(15,37,63),
            Color.FromRgb(37,64,97),
            Color.FromRgb(99,37,35),
            Color.FromRgb(79,98,40),
            Color.FromRgb(64,49,82),
            Color.FromRgb(33,89,104),
            Color.FromRgb(152,72,7),
   };

        private readonly Color[] standartColors = new Color[]
        {
            Color.FromRgb(192,0,0),
            Color.FromRgb(255,0,0),
            Color.FromRgb(255,192,0),
            Color.FromRgb(255,255,0),
            Color.FromRgb(146,208,80),
            Color.FromRgb(0,176,80),
            Color.FromRgb(0,176,240),
            Color.FromRgb(0,112,192),
            Color.FromRgb(0,32,96),
            Color.FromRgb(112,48,160),
        };
        #endregion

        public delegate void onColorChanged(Color color);
        public event onColorChanged OnColorChanged;

        public ColorPalette()
        {
            InitializeComponent();

            // Add colors dynamically
            // So we have 60 colors to add to grid
            int currentColumn = 0;
            int currentRow = 0;
            for (int i = 0; i < 60; i++)
            {
                Button template = GenerateTemplate();
                template.Background = new SolidColorBrush(colors[i]);
                template.Click += Template_Click;

                // Add to colors grid and make sure that just every second row and column is used.
                // 0 1 2 3 4
                // C X C X C
                if (currentColumn + 2 > 18)
                {
                    currentColumn = 0;
                    currentRow += 2;
                }
                else if (i != 0)
                    currentColumn += 2;

                var grd = colorsGrid;
                grd.Children.Add(template);
                template.SetValue(Grid.RowProperty, currentRow);
                template.SetValue(Grid.ColumnProperty, currentColumn);
            }

            currentColumn = 0;
            // Now apply standart colors
            for (int i = 0; i < 10; i++)
            {
                Button template = GenerateTemplate();
                template.Background = new SolidColorBrush(standartColors[i]);
                template.Click += Template_Click;

                var grd = gridStandartColors;
                grd.Children.Add(template);
                template.SetValue(Grid.ColumnProperty, currentColumn);
                currentColumn += 2;
            }
        }

        public Button GenerateTemplate()
        {
            Button template = new Button
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0.6),
                Style = Application.Current.Resources["buttonWithoutSelection"] as Style
            };
            return template;
        }

        private void Template_Click(object sender, RoutedEventArgs e)
        {
            var col = ((sender as Button).Background as SolidColorBrush).Color;
            OnColorChanged?.Invoke(col);           
        }
    }
}