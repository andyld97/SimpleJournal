using SimpleJournal.Common.FileAssociations;
using System;

namespace SJFileAssoc
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            FileAssociations.EnsureAssociationsSet("SimpleJournal UWP", "SimpleJournal UWP");
        }
    }
}