namespace SimpleJournal.Documents.UI.Data
{
    public class ShapeInfo
    {
        public System.Windows.Media.Color BackgroundColor;

        public System.Windows.Media.Color BorderColor;

        public int BorderWidth;

        public int Angle;

        public ShapeInfo()
        {

        }

        public ShapeInfo(System.Windows.Media.Color background, System.Windows.Media.Color borderColor, int borderWidth, int angle)
        {
            this.BackgroundColor = background;
            this.BorderColor = borderColor;
            this.BorderWidth = borderWidth;
            this.Angle = angle;
        }
    }
}
