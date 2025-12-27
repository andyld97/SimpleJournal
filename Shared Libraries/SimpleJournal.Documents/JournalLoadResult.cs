namespace SimpleJournal.Documents
{
    public class JournalLoadResult
    {
        public Journal Journal { get; set; }

        public LoadState State { get; set; }    

        public bool SupressErrorMessage { get; set; } = false;  

        public string ErrorMessage { get; set; } 

        public JournalLoadResult(Journal journal, LoadState state)
        {
            Journal = journal;
            State = state;
        }   

        public JournalLoadResult(LoadState state)
        {
            State = state;
        }

        public JournalLoadResult()
        {
            State = LoadState.UnknownError;
        }   
    }

    public enum LoadState
    {
        Success,
        InvalidOrCorruptFile,
        IncompatibleVersion,   
        UnsupportedVersionOrCorrupt,
        UnknownError
    }
}
