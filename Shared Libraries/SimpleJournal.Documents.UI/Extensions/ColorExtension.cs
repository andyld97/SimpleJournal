using SimpleJournal.Common.Data;

namespace SimpleJournal.Documents.UI.Extensions
{
    public static class ColorExtension
    {
        public static System.Windows.Media.Color ToColor(this Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color ToColor(this System.Windows.Media.Color color)
        {
            return new Color(color.A, color.R, color.G, color.B);
        }

        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return null;

            try
            {
                return ((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex)).ToColor();
            }
            catch
            {
                return null;
            }
        }
    }
}
