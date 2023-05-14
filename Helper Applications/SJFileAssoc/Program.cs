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
                // See <uap5:ExecutionAlias Alias="SimpleJournal.exe" />
                FileAssociations.EnsureAssociationsSet("SimpleJournal", "SimpleJournal UWP");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.strFailedToSetFileAssoc_Message, ex.Message), Properties.Resources.strError, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}