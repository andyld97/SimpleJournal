using SimpleJournal.Common;
using SimpleJournal.Common.Data;
using System.Linq;
using Size = SimpleJournal.Common.Data.Size;

namespace SimpleJournal.Data
{
    /// <summary>
    /// Represents a pencil in SimpleJournal
    /// </summary>
    public class Pen
    {
        public static Pen[] Instance = Load();

        public Color FontColor { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public Pen(Color fontColor, double width, double height)
        {
            FontColor = fontColor;
            Width = width;
            Height = height;
        }

        public Pen() : this(new Color(), Documents.UI.Consts.StrokeSizes.FirstOrDefault())
        { }

        public Pen(Color fontColor, Size size) : this(fontColor, size.Width, size.Height)
        { }

        public static Pen[] Load()
        {
            // Initialize pens
            Pen[] pens = new Pen[Consts.AMOUNT_PENS];
            var firstSize = Documents.UI.Consts.StrokeSizes.FirstOrDefault();
            for (int i = 0; i < Consts.AMOUNT_PENS; i++)
                pens[i] = new Pen(Consts.PEN_COLORS[i], firstSize);

            try
            {
                if (System.IO.File.Exists(Consts.PenSettingsFilePath))
                {
                    var result = Serialization.Read<Pen[]>(Consts.PenSettingsFilePath, Serialization.Mode.XML);
                    if (result != null)
                        return result;
                }
            }
            catch
            { }

            return Save(pens);
        }

        public static void Save()
        {
            try
            {
                Serialization.Save(Consts.PenSettingsFilePath, Pen.Instance, Serialization.Mode.XML);
            }
            catch
            { }
        }

        public static Pen[] Save(Pen[] pens)
        {
            try
            {
                Serialization.Save(Consts.PenSettingsFilePath, pens, Serialization.Mode.XML);
            }
            catch
            { }

            return pens;
        }
    }
}