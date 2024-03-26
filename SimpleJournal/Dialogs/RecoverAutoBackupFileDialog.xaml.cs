using SimpleJournal.Controls;
using SimpleJournal.Data;
using SimpleJournal.Documents;
using SimpleJournal.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleJournal.Dialogs
{
    /// <summary>
    /// Interaktionslogik für RecoverAutoBackupFileDialog.xaml
    /// </summary>
    public partial class RecoverAutoBackupFileDialog : Window
    {
        private bool ignoreOnClosingEvent = false;

        private List<System.IO.FileInfo> BackupFiles { get; set; } = [];

        private List<BackupDataItem> BackupItems { get; set; } = [];

        public RecoverAutoBackupFileDialog()
        {
            InitializeComponent();

            Loaded += RecoverAutoBackupFileDialog_Loaded;
        }

        private async void RecoverAutoBackupFileDialog_Loaded(object sender, RoutedEventArgs e)
        {
            BackupFiles = await GetAllBackupFiles();

            if (BackupFiles.Count > 0)
            {
                var sortedBackupFiles = BackupFiles.OrderBy(p => p.LastWriteTime);

                // Add all files to listBox
                foreach (var file in sortedBackupFiles)
                {
                    Journal journal = await Journal.LoadJournalMetaAsync(file.FullName);
                    var bdi = new BackupDataItem()
                    {
                        FileInfo = file,
                        Path = journal.OriginalPath
                    };
                    var bi = new BackupItem(bdi);
                    bi.OnRecievedAction += Bi_OnRecievedAction;
                    ListBackupFiles.Items.Add(bi);
                    BackupItems.Add(bdi);

                    journal.Pages.Clear();
                    journal = null;
                }

                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ButtonRecoverAll), ButtonRecoverAll);
            }
            else
            {
                RunAutoSaveRecoverInfo.Text = Properties.Resources.strAutoSaveNoBackupFileFound;
                RunAutoSaveRecoverInfo.Foreground = new SolidColorBrush(Colors.Red);
                ButtonDiscardAll.Content = Properties.Resources.strOK;
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(ButtonDiscardAll), ButtonDiscardAll);
                ButtonRecoverAll.Visibility = Visibility.Hidden;
            }

            RunLoading.Text = string.Empty;
        }

        private async void Bi_OnRecievedAction(bool discard, BackupDataItem item)
        {
            if (discard)
            {
                DiscardFile(item, false);

                // If there are no more files, close the dialog
                if (ListBackupFiles.Items.Count == 0)
                {
                    ignoreOnClosingEvent = true;
                    this.DialogResult = true;
                }
            }
            else
            {
                await RecoverFile(item, CheckBoxOpenBackupsAfterRestore.IsChecked.Value, false);

                // If there are no more files, close the dialog
                if (ListBackupFiles.Items.Count == 0)
                {
                    ignoreOnClosingEvent = true;
                    this.DialogResult = true;
                }
            }
        }

        private async Task<List<System.IO.FileInfo>> GetAllBackupFiles()
        {
            List<System.IO.FileInfo> backupFiles = [];

            if (!System.IO.Directory.Exists(Consts.AutoSaveDirectory))
                return backupFiles;

            var files = new System.IO.DirectoryInfo(Consts.AutoSaveDirectory).EnumerateFiles().Where(f => f.Name.EndsWith(".journal"));

            foreach (var file in files)
            {
                try
                {
                    var journal = await Journal.LoadJournalMetaAsync(file.FullName);
                    if (journal.IsBackup)
                    {
                        int pID = journal.ProcessID;

                        // Check if this Process ID is active - if yes, ignore this file
                        if (ProcessHelper.IsProcessActiveByTaskId(pID))
                            continue;

                        // otherwise add it to the list of backup files
                        backupFiles.Add(file);
                        journal.Pages.Clear();
                        journal = null;
                    }
                }
                catch
                {

                }
            }

            return backupFiles;
        }

        private bool AskUserForDiscardingBackupFile()
        {
            // If there is no backup file - nothing which can be discarded
            if (BackupFiles.Count == 0)
                return true;

            // Ask the user if he/she wants to discard this file
            if (System.Windows.MessageBox.Show(Properties.Resources.strDiscardQuestionAll, Properties.Resources.strDiscardTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                return true;

            return false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ignoreOnClosingEvent)
                return;

            bool result = !AskUserForDiscardingBackupFile();

            if (!result)
            {
                // Discard all - using force (no msgbox)
                DiscardAll(true);
            }

            e.Cancel = result;
            base.OnClosing(e);
        }

        #region Recover / Discard File(s)

        private bool DiscardFile(BackupDataItem backupDataItem, bool silent)
        {
            if (silent || System.Windows.MessageBox.Show(Properties.Resources.strDiscardQuestion, Properties.Resources.strDiscardTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    System.IO.File.Delete(backupDataItem.FileInfo.FullName);
                    ListBackupFiles.Items.RemoveAt(BackupItems.IndexOf(backupDataItem));
                    BackupItems.Remove(backupDataItem);
                    return true;
                }
                catch
                {
                    if (!silent)
                        System.Windows.MessageBox.Show(Properties.Resources.strFailedToDeleteFile, Properties.Resources.strFailedToDeleteFileTitle);
                }
            }

            return false;
        }

        private async Task<string> RecoverFile(BackupDataItem backupDataItem, bool openAfterRecover, bool silent, string folderPath = "")
        {
            string pathToRecover = string.Empty;
            if (!string.IsNullOrEmpty(backupDataItem.Path))
                pathToRecover = backupDataItem.Path;
            else if (!string.IsNullOrEmpty(folderPath))
                pathToRecover = System.IO.Path.Combine(folderPath, backupDataItem.FileInfo.Name);
            else
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        pathToRecover = System.IO.Path.Combine(folderBrowserDialog.SelectedPath, backupDataItem.FileInfo.Name);
                    else
                        return string.Empty;
                }
            }

            try
            {
                // Load journal
                var journal = await Journal.LoadJournalAsync(backupDataItem.FileInfo.FullName, Consts.BackupDirectory, true);
                if (journal == null)
                    return string.Empty;

                journal.IsBackup = false;

                // Save it to another path
                await journal.SaveAsync(pathToRecover, true);

                journal.Pages.Clear();
                journal = null;

                // Delete the backup file                    
                System.IO.File.Delete(backupDataItem.FileInfo.FullName);

                // If the directory is empty, it could be deleted.
                // But I don't think this is important - because it will be deleted anyway at the end of all SJ instances!

                // Remove the file
                ListBackupFiles.Items.RemoveAt(BackupItems.IndexOf(backupDataItem));
                BackupItems.Remove(backupDataItem);

                // Only open it, if checkbox is set
                if (!openAfterRecover)
                    return pathToRecover;

                // Finally open it
                OpenRecoveredFile(pathToRecover, silent);
            }
            catch (Exception ex)
            {
                if (!silent)
                    System.Windows.MessageBox.Show($"{Properties.Resources.strFailedToRecoverJournal} {ex.Message}", Properties.Resources.strFailedToRecoverJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return string.Empty;
        }

        private bool OpenRecoveredFile(string path, bool silent)
        {
            string args = $"\"{path}\" /ignorebackups";

            try
            {
#if UWP
#if DEBUG
                string sjPath = @"C:\Users\andre\source\Workspaces\SimpleJournal\SimpleJournal\bin\Debug\SimpleJournal.exe";
#else
                string sjPath = "SimpleJournal.exe";
#endif
                System.Diagnostics.Process.Start(sjPath, args);
#else                
                // Location is only the .NET 6 DLL but not the EXE File
                System.Diagnostics.Process.Start(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SimpleJournal.exe"), args);
#endif

                return true;
            }
            catch (Exception ex)
            {
                if (!silent)
                    System.Windows.MessageBox.Show($"{Properties.Resources.strFailedToOpenNewSJInstance} {ex.Message}", Properties.Resources.strFailedToOpenNewSJInstanceTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        private void DiscardAll(bool force)
        {
            if (force || System.Windows.MessageBox.Show(Properties.Resources.strDiscardQuestionAll, Properties.Resources.strDiscardTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool result = false;
                foreach (var item in new List<BackupDataItem>(BackupItems))
                    result |= DiscardFile(item, true);

                if (result)
                {
                    ignoreOnClosingEvent = true;
                    this.DialogResult = false;
                }
                else
                {
                    System.Windows.MessageBox.Show(Properties.Resources.strFailedToDeleteSomeBackups, Properties.Resources.strFailedToDeleteSomeBackupsTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonDiscardAll_Click(object sender, RoutedEventArgs e)
        {
            if (BackupFiles.Count == 0)
            {
                ignoreOnClosingEvent = true;
                this.DialogResult = false;
                return;
            }

            DiscardAll(false);
                
        }

        private async void ButtonRecoverAll_Click(object sender, RoutedEventArgs e)
        {
            // Determine if there are backups with no origin - we need to show an FolderBrowserDialog,
            // but not in RecoverFile each time, but rather once (yet)
            string path = string.Empty;

            if (BackupItems.Any(x => string.IsNullOrEmpty(x.Path)))
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        path = folderBrowserDialog.SelectedPath;
                    else
                        return;
                }
            }

            List<string> filesToOpen = [];
            List<BackupDataItem> temp = [.. BackupItems];

            foreach (var item in temp)
            {
                // Pass path to make sure opening the FolderBrowserDialog only once!
                string pathToRecover = await RecoverFile(item, false, true, path);
                filesToOpen.Add(pathToRecover);
            }

            // Open all files AFTER restoring not WHILE restoring
            if (CheckBoxOpenBackupsAfterRestore.IsChecked.Value)
            {
                foreach (var item in filesToOpen)
                {
                    // Open the file - and only continue if it has successfully opened another instance with the recovered file
                    if (!OpenRecoveredFile(item, true))
                    {
                        System.Windows.MessageBox.Show(Properties.Resources.strFailedToOpenNewSJInstance, Properties.Resources.strFailedToOpenNewSJInstanceTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }

                    // Make sure not all instances start parallel - to make sure they won't reset the settings
                    Thread.Sleep(1000);
                }
            }

            ignoreOnClosingEvent = true;
            this.DialogResult = true;
        }

        #endregion

        private void ButtonDisplayDirectoryInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(Consts.AutoSaveDirectory))
            {
                try
                {
                    System.Diagnostics.Process.Start(Consts.AutoSaveDirectory);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"{Properties.Resources.strFailedToOpenAutoBackupDirectory}{ex.Message}", Properties.Resources.strFailedToOpenAutoBackupDirectoryTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(Properties.Resources.strAutoBackupDirectoryDoesNotExists, Properties.Resources.strAutoBackupDirectoryDoesNotExistsTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}