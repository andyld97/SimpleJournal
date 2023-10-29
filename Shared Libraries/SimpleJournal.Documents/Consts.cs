namespace SimpleJournal.Documents
{
    public static class Consts
    {
        public const int A4WidthP = 794;
        public const int A4HeightP = 1123;
        public const int A4WidthL = 1123;
        public const int A4HeightL = 794;

#if DEBUG
        public const int MaxPDFPagesPerJournal = 20;
#else
        public const int MaxPDFPagesPerJournal = 100;
#endif
    }
}