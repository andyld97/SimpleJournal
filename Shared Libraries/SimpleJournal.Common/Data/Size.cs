namespace SimpleJournal.Common.Data
{
    public class Size
    {
        public double Width { get; set; }   

        public double Height { get; set; }

        public Size()
        { }

        public Size(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
