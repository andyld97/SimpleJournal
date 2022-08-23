namespace SimpleJournal.Documents.Pattern
{
    public interface IPattern : ICloneable
    {
        void Reset();

        bool HasDefaultValues { get; }
    }
}