using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJournal
{
    public static class State
    {
        public delegate void onStateChanged(string message, ProgressState state);
        public static event onStateChanged OnStateChanged;

        public static void SetAction(StateAction action, ProgressState state)
        {
            // ToDo: *** Translate message
            // Generate message
            string message = string.Empty;

            switch (action)
            {
                case StateAction.Saving: message = "Speichern ..."; break;
                case StateAction.ExportPDF: message = "PDF exportieren ..."; break;
                case StateAction.Export: message = "Journal exportieren ..."; break;
            }

            OnStateChanged?.Invoke(message, state);
        }
    }
}
