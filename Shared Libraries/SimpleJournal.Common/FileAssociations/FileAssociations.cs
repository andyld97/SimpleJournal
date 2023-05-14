using Microsoft.Win32;
using System;
//using System.Windows.Forms;

namespace SimpleJournal.Common.FileAssociations
{
    public class FileAssociations
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        public static readonly string FileAssociationIconsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "journal", "icon.ico");

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static void EnsureAssociationsSet(string executable, string progId = "SimpleJournal")
        {
            // var filePath = Process.GetCurrentProcess().MainModule.FileName;
            //var filePath = @"explorer shell:appsFolder\26590AndreasLeopold.SimpleJournal_77rg7g3vwn6wg!SimpleJournal";
            //var filePath = "26590AndreasLeopold.SimpleJournal_77rg7g3vwn6wg";
            var filePath = executable;

            EnsureAssociationsSet(new FileAssociation
            {
                Extension = ".journal",
                ProgId = progId,
                FileTypeDescription = "Journal",
                ExecutableFilePath = filePath
            });

        }

        public static void EnsureAssociationsSet(params FileAssociation[] associations)
        {
            bool madeChanges = false;

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\SimpleJournal");
            }
            catch
            {

            }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\SimpleJournal UWP");
            }
            catch
            { }

            foreach (var association in associations)
            {
                madeChanges |= SetAssociation(
                    association.Extension,
                    association.ProgId,
                    association.FileTypeDescription,
                    association.ExecutableFilePath);
            }

            if (madeChanges)
            {
                SHChangeNotify(SHCNE_ASSOCCHANGED, 0x2000, IntPtr.Zero, IntPtr.Zero); // 
            }
        }

        public static bool SetAssociation(string extension, string progId, string fileTypeDescription, string applicationFilePath)
        {
            bool madeChanges = false;
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + extension, progId);
            madeChanges |= SetKeyDefaultValue(@"Software\Classes\" + progId, fileTypeDescription);
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\shell\open\command", "\"" + applicationFilePath + "\" \"%1\"");
            madeChanges |= SetKeyDefaultValue($@"Software\Classes\{progId}\DefaultIcon", FileAssociationIconsPath);
            return madeChanges;
        }


        private static bool SetKeyDefaultValue(string keyPath, string value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key.GetValue(null) as string != value)
                    {
                        key.SetValue(null, value);
                        return true;
                    }
                }
            }
            catch (Exception)
            {

            }

            return false;
        }
    }
}
