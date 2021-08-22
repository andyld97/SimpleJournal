﻿namespace SimpleJournal.Data
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

        public Pen() : this(new Color(), Consts.StrokeSizes[0].Width, Consts.StrokeSizes[0].Height)
        { }

        public static Pen[] Load()
        {
            // Initalize pens
            Pen[] pens = new Pen[Consts.AMOUNT_PENS];
            for (int i = 0; i < Consts.AMOUNT_PENS; i++)
                pens[i] = new Pen(Consts.PEN_COLORS[i], Consts.StrokeSizes[0].Width, Consts.StrokeSizes[1].Height);

            try
            {
                if (System.IO.File.Exists(Consts.PenSettingsFilePath))
                {
                    var result = Serialization.Serialization.Read<Pen[]>(Consts.PenSettingsFilePath, Serialization.Serialization.Mode.Normal);
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
                Serialization.Serialization.Save(Consts.PenSettingsFilePath, Pen.Instance, Serialization.Serialization.Mode.Normal);
            }
            catch
            { }
        }

        public static Pen[] Save(Pen[] pens)
        {
            try
            {
                Serialization.Serialization.Save(Consts.PenSettingsFilePath, pens, Serialization.Serialization.Mode.Normal);
            }
            catch
            { }

            return pens;
        }
    }
}
