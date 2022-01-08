using SimpleJournal.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleJournal.Data
{
    [Serializable]
    public class Journal
    {
        private static bool isSaving = false;
        private static object sync = new object();

        [XmlIgnore]
        private bool wasSavedAlready = false;

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

        public static async Task<Journal> LoadJournalAsync(string path, bool quiet = false)
        {
            try
            {
                // If we have the old file format (load it using serialization)
                if (!ZipFileHelper.IsZipFile(path))
                {
                    var result = Serialization.Serialization.Read<Journal>(path, Serialization.Serialization.Mode.XML);
                    if (result != null)
                        return result;
                }
                else
                {
                    // ... otherwise load it as a zip file
                    bool fail = false;
                    using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open))
                    {
                        using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Read, false))
                        {
                            Dictionary<int, JournalPage> journalPages = new Dictionary<int, JournalPage>();
                            Dictionary<int, PdfJournalPage> pdfJournalPages = new Dictionary<int, PdfJournalPage>();
                            Dictionary<int, byte[]> images = new Dictionary<int, byte[]>();
                            Journal journal = new Journal();

                            foreach (var currentEntry in zipArchive.Entries)
                            {
                                byte[] data = await currentEntry.ReadZipEntryAsync();
                                string pageNumber = currentEntry.Name.Replace("page", string.Empty).Replace(".png", string.Empty).Replace(".binp", string.Empty).Replace(".bin", string.Empty);

                                if (currentEntry.Name == "info.bin")
                                    journal = Serialization.Serialization.ReadBytes<Journal>(data, Serialization.Serialization.Mode.Binary);
                                else if (int.TryParse(pageNumber, out int page))
                                {
                                    if (currentEntry.Name.EndsWith(".png"))
                                    {
                                        // image for pdf page
                                        images.Add(page, data);
                                    }
                                    else if (currentEntry.Name.EndsWith(".binp"))
                                    {
                                        // pdf page
                                        pdfJournalPages.Add(page, Serialization.Serialization.ReadBytes<PdfJournalPage>(data, Serialization.Serialization.Mode.Binary));
                                    }
                                    else if (currentEntry.Name.EndsWith(".bin"))
                                    {
                                        // normal page
                                        journalPages.Add(page, Serialization.Serialization.ReadBytes<JournalPage>(data, Serialization.Serialization.Mode.Binary));
                                    }
                                }
                                else
                                {
                                    fail = true;
                                    break;
                                }
                            }

                            if (fail)
                                throw new ArgumentException("pageParser failed");

                            // Merge dictonaries together
                            // Step 1) Add images to PdFJournalPages (if any)
                            foreach (var image in images.Keys)
                            {
                                var currentPage = pdfJournalPages[image];
                                currentPage.PageBackground = images[image];
                            }

                            // Step 2) Merge pages in the correct order (huh?)
                            int oldCount = journalPages.Count + pdfJournalPages.Count;
                            int count = 0;
                            journal.Pages.Clear();
                            for (int i = 0; i < oldCount; i++)
                            {
                                if (pdfJournalPages.ContainsKey(i))
                                    journal.Pages.Add(pdfJournalPages[i]);
                                else if (journalPages.ContainsKey(i))
                                    journal.Pages.Add(journalPages[i]);

                                count++;
                            }

                            if (oldCount != count)
                            {
                                // ToDO: *** something is wrong!!
                            }


                            return journal;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!quiet)
                    MessageBox.Show($"{Properties.Resources.strFailedToLoadJournal}: {e.Message}", Properties.Resources.strFailedToLoadJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new Journal();
        }

        public static EventHandler OnSaving;
        public static EventHandler OnSavingFinished;

        public async Task<bool> UpdateJournalInfoAsync(string filePath)
        {
            if (!wasSavedAlready)
                return await SaveAsync(filePath, true);
            else
            {
                try
                {
                    // Update Journal
                    using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                    {
                        using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Read, false))
                        {
                            var entry = zipArchive.GetEntry("info.bin");

                            using (var stream = entry.Open())
                            {
                                var jrn = new Journal() { ProcessID = this.ProcessID, IsBackup = this.IsBackup, OriginalPath = this.OriginalPath };
                                // This is just for counting the pages
                                foreach (var page in Pages)
                                    jrn.Pages.Add(new JournalPage());

                                var data = Serialization.Serialization.SaveToBytes(jrn, Serialization.Serialization.Mode.Binary);
                                await stream.WriteAsync(data, 0, data.Length);
                            }
                        }
                    }

                    return true;
                }
                catch { }
            }

            return false;
        }

        public async Task<bool> SaveAsync(string filePath, bool quiet = false)
        {
            lock (sync)
            {
                if (isSaving)
                    return false;
                else
                    isSaving = true;
            }

            bool retVal = false;

            try
            {
                if (!quiet)
                    OnSaving?.Invoke(this, EventArgs.Empty);

                if (wasSavedAlready)
                {
                    // ToDo: *** Only update the archive and (eventually delete pages/images, add pages)
                    // If the file exists already and is valid zip file we do not need to re-create the entire file, but open
                    // it as as ZipArchiveMode.Update and only update the bin files
                    // But remember to ensure that if a PdfJournalPage gets deleted, we need to delete the file from the zip archive also!
                }

                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    // or update
                    using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Create, false))
                    {
                        int pgCount = 0;
                        foreach (var page in Pages)
                        {
                            if (page is PdfJournalPage pdf)
                            {
                                var pageEntry = zipArchive.CreateEntry($"pages/page{pgCount}.binp", System.IO.Compression.CompressionLevel.Optimal);

                                // Backgorund will be saved seperatly 
                                PdfJournalPage pdfJournalPage = new PdfJournalPage() { Data = pdf.Data, JournalResources = pdf.JournalResources, Orientation = pdf.Orientation, PageFormat = pdf.PageFormat, PaperPattern = pdf.PaperPattern };
                                pdfJournalPage.PageBackground = null;

                                using (System.IO.Stream stream = pageEntry.Open())
                                {
                                    var data = Serialization.Serialization.SaveToBytes(pdfJournalPage, Serialization.Serialization.Mode.Binary);
                                    await stream.WriteAsync(data, 0, data.Length);
                                }

                                // Save background
                                var imgEntry = zipArchive.CreateEntry($"pages/page{pgCount}.png", System.IO.Compression.CompressionLevel.Optimal);
                                using (System.IO.Stream stream = imgEntry.Open())
                                {
                                    await stream.WriteAsync(pdf.PageBackground, 0, pdf.PageBackground.Length);
                                }
                            }
                            else
                            {
                                var pageEntry = zipArchive.CreateEntry($"pages/page{pgCount}.bin", System.IO.Compression.CompressionLevel.Optimal);
                                using (System.IO.Stream stream = pageEntry.Open())
                                {
                                    var data = Serialization.Serialization.SaveToBytes(page, Serialization.Serialization.Mode.Binary);
                                    await stream.WriteAsync(data, 0, data.Length);
                                }
                            }

                            pgCount++;
                        }

                        // Write amount of pages to load
                        var info = zipArchive.CreateEntry("info.bin");
                        using (System.IO.Stream stream = info.Open())
                        {
                            var jrn = new Journal() { ProcessID = this.ProcessID, IsBackup = this.IsBackup, OriginalPath = this.OriginalPath };

                            // This is just for counting the pages
                            foreach (var page in Pages)
                                jrn.Pages.Add(new JournalPage());

                            var data = Serialization.Serialization.SaveToBytes(jrn, Serialization.Serialization.Mode.Binary);
                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }
                }

                if (!wasSavedAlready)
                    wasSavedAlready = true;

                if (!quiet)
                    OnSavingFinished?.Invoke(this, EventArgs.Empty);

                retVal = true;

                // Old method - XML Serialization then Binary Serialization, Now ZIP!
                //Serialization.Serialization.Save(filePath, this, Serialization.Serialization.Mode.Binary);
            }
            catch (Exception e)
            {
                retVal = false;

                if (!quiet)
                    MessageBox.Show($"{Properties.Resources.strFailedToSaveJournal} {e.Message}", Properties.Resources.strFailedToSaveJournalTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!quiet)
                OnSavingFinished?.Invoke(this, EventArgs.Empty);

            lock (sync)
                isSaving = false;

            return retVal;
        }
    }
}
