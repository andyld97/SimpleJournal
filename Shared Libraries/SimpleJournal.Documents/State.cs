using SimpleJournal.Common;

namespace SimpleJournal.Documents
{
    public static class State
    {
        private static bool isInitalized = false;
        private static string[] langItems = null;

        public delegate void onStateChanged(string message, ProgressState state);
        public static event onStateChanged OnStateChanged;

        public static void Initalize(string[] langItems)
        {
            State.langItems = langItems;
            isInitalized = true;
        }

        public static void SetAction(StateType action, ProgressState state)
        {
            if (!isInitalized)
                return;

            // Generate message
            string message = string.Empty;

            switch (action)
            {
                case StateType.Saving: message = langItems[0]; break;
                case StateType.ExportPDF: message = langItems[1]; break;
                case StateType.Export: message = langItems[2]; break;
                case StateType.Printing: message = langItems[3]; break;
            }

            OnStateChanged?.Invoke(message, state);
        }
    }
}