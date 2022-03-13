using SimpleJournal.Common.FileAssociations;
using System;
using System.Windows.Forms;

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
            try
            {
                FileAssociations.EnsureAssociationsSet("SimpleJournal UWP", "SimpleJournal UWP");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set file association: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}