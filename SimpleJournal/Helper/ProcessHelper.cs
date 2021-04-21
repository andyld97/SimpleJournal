using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SimpleJournal.Helper
{
    public static class ProcessHelper
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static int CurrentProcID => Process.GetCurrentProcess().Id;

        public static int SimpleJournalProcessCount => Process.GetProcesses().Where(p => p.ProcessName.Contains("SimpleJournal")).Count();

        public static bool IsProcessActiveByTaskId(int taskID)
        {
            var process = Process.GetProcesses().Where(p => p.Id == taskID).FirstOrDefault();
            if (process == null)
                return false;

            return process.ProcessName.Contains("SimpleJournal");
        }

        public static bool BringProcessToFront(int taskID)
        {
            var process = Process.GetProcesses().Where(p => p.Id == taskID).FirstOrDefault();
            if (process == null)
                return false;

            try
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                    SetForegroundWindow(process.MainWindowHandle);

                return true;
            }
            catch
            {

            }

            return false;
        }
    }
}
