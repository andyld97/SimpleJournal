using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleJournal.Data
{
    public class Journal
    {
        public List<JournalPage> Pages { get; set; } = new List<JournalPage>();

        /// <summary>
        /// Determines if this journal is a backup
        /// </summary>
        public bool IsBackup { get; set; } = false;

        /// <summary>
        /// ProcessID should only be set if this journal is open
        /// (It should not be serialized if it's empty)
        /// </summary>
        [XmlElement, DefaultValue(-1)]
        public int ProcessID { get; set; } = -1;

        /// <summary>
        /// The path where this journal (backup) was orginally saved
        /// Note: This can be empty, although it's a backup! It will only be set, if a journal was loaded while creating the backup!
        /// (It should not be serialized if it's empty)
        /// </summary>
        [XmlElement, DefaultValue("")]
        public string OriginalPath { get; set; } = "";

        public static Journal LoadJournal(string path, bool quiet = false)
        {
            try
            {
                var result = Serialization.Serialization.Read<Journal>(path, Serialization.Serialization.Mode.Normal);
                return result;
            }
            catch (Exception e)
            {
                if (!quiet)
                    MessageBox.Show($"{Properties.Resources.strFailedToLoadJournal}: {e.Message}", Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Journal();
        }

        public void Save(string filePath, bool quiet = false)
        {
            try
            {
                Serialization.Serialization.Save(filePath, this, Serialization.Serialization.Mode.Normal);
            }
            catch (Exception e)
            {
                if (!quiet)
                    MessageBox.Show($"{Properties.Resources.strFailedToSaveJournal} {e.Message}", Properties.Resources.strFailedToSaveJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
