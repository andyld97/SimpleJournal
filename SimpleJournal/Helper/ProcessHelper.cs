using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SimpleJournal.Helper
{
    public static class ProcessHelper
    {
        public static int CurrentProcID => Environment.ProcessId;

        public static int SimpleJournalProcessCount => Process.GetProcesses().Where(p => p.ProcessName.Contains("SimpleJournal")).Count();

        public static bool IsProcessActiveByTaskId(int taskID)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == taskID);
            if (process == null)
                return false;

            return process.ProcessName.Contains("SimpleJournal");
        }

        public static bool BringProcessToFront(int taskID)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.Id == taskID);
            if (process == null)
                return false;

            try
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                    NativeMethods.SetForegroundWindow(process.MainWindowHandle);

                return true;
            }
            catch
            {

            }

            return false;
        }
    }
}
