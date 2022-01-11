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
            // Generate message
            string message = string.Empty;

            switch (action)
            {
                case StateAction.Saving: message = Properties.Resources.strStateSaving; break;
                case StateAction.ExportPDF: message = Properties.Resources.strStateExportAsPDF; break;
                case StateAction.Export: message =Properties.Resources.strStateExportAsJournal; break;
                case StateAction.Printing: message = Properties.Resources.strStatePrinting; break;
            }

            OnStateChanged?.Invoke(message, state);
        }
    }
}
