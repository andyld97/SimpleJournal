using SimpleJournal.Common;
using System.ComponentModel;
using System.Xml.Serialization;
using SimpleJournal.Common.Helper;
using SimpleJournal.Documents.Pattern;

namespace SimpleJournal.Documents
{
    public class Journal
    {
        private static bool isSaving = false;
        private static readonly object sync = new object();

        public delegate void onErrorOccurred(string message, string scope);
        public static event onErrorOccurred OnErrorOccurred = null!;

        [XmlIgnore]
        private bool wasSavedAlready = false;

        /// <summary>
        /// [MultiPageDocument - PDF]: The previous document index to load if the user wants to navigate backwards
        /// </summary>
        [XmlElement, DefaultValue(null)]
        public int? PreviousDocumentIndex { get; set; } = null;

        /// <summary>
        /// [MultiPageDocument - PDF]: The next document index to load if the user wants to navigate forwards
        /// </summary>
        [XmlElement, DefaultValue(null)]
        public int? NextDocumentIndex { get; set; } = null;

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
        /// The path where this journal (backup) was originally saved
        /// Note: This can be empty, although it's a backup! It will only be set, if a journal was loaded while creating the backup!
        /// (It should not be serialized if it's empty)
        /// </summary>
        [XmlElement, DefaultValue("")]
        public string OriginalPath { get; set; } = "";

        #region Pattern
        public ChequeredPattern ChequeredPattern { get; set; } = null!;

        public RuledPattern RuledPattern { get; set; } = null!;

        public DottedPattern DottedPattern { get; set; } = null!;
        #endregion

        #region Load, Save, Update Methods

        /// <summary>
        /// Loads the journal, but only with the meta information
        /// </summary>
        /// <param name="path">The path where the journal is located</param>
        /// <returns></returns>
        public static async Task<Journal> LoadJournalMetaAsync(string path)
        {
            if (!ZipFileHelper.IsZipFile(path))
                return Serialization.Read<Journal>(path, Serialization.Mode.XML);

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Read, false))
                    {
                        var entry = zipArchive.GetEntry("journal.xml");
                        var data = await entry.ReadZipEntryAsync();

                        return Serialization.ReadBytes<Journal>(data, Serialization.Mode.XML);
                    }
                }
            }
            catch
            {

            }

            return null;
        }

        /// <summary>
        /// Loads the full journal (zip file)
        /// </summary>
        /// <param name="path">The path where the journal is located</param>
        /// <param name="backupDirectory">The directory where old documents will be stored</param>
        /// <param name="quiet">If true, no error message is shown</param>
        /// <returns></returns>
        public static async Task<Journal> LoadJournalAsync(string path, string backupDirectory, bool quiet = false)
        {
            try
            {
                // If we have the old file format (create a backup and load it using serialization)
                if (!ZipFileHelper.IsZipFile(path))
                {
                    try
                    {
                        // Ensure that the backup directory exists
                        if (!System.IO.Directory.Exists(backupDirectory))
                            System.IO.Directory.CreateDirectory(backupDirectory);
                    }
                    catch
                    {

                    }

                    try
                    {
                        // Create a backup (for older versions of sj)
                        if (!string.IsNullOrEmpty(backupDirectory))
                            System.IO.File.Copy(path, System.IO.Path.Combine(backupDirectory, System.IO.Path.GetFileName(path)));
                    }
                    catch
                    {

                    }

                    string xmlData = await System.IO.File.ReadAllTextAsync(path, System.Text.Encoding.UTF8);
                    // Correct old documents misspelled pattern!
                    xmlData = xmlData.Replace("<PaperPattern>Chequeued</PaperPattern>", "<PaperPattern>Chequered</PaperPattern>");

                    var result = Serialization.ReadString<Journal>(xmlData, System.Text.Encoding.UTF8);
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
                                string pageNumber = currentEntry.Name.Replace("page", string.Empty).Replace(".png", string.Empty).Replace(".pdf", string.Empty).Replace(".xml", string.Empty);

                                if (currentEntry.Name == "journal.xml")
                                {
                                    string xmlData = System.Text.Encoding.UTF8.GetString(data);
                                    // Correct old documents misspelled pattern!
                                    xmlData = xmlData.Replace("<PaperPattern>Chequeued</PaperPattern>", "<PaperPattern>Chequered</PaperPattern>");
                                    journal = Serialization.ReadString<Journal>(xmlData, System.Text.Encoding.UTF8);
                                }
                                else if (int.TryParse(pageNumber, out int page))
                                {
                                    if (currentEntry.Name.EndsWith(".png"))
                                    {
                                        // image for pdf page
                                        images.Add(page, data);
                                    }
                                    else if (currentEntry.Name.EndsWith(".pdf"))
                                    {
                                        // pdf page
                                        pdfJournalPages.Add(page, Serialization.ReadBytes<PdfJournalPage>(data, Serialization.Mode.XML));
                                    }
                                    else if (currentEntry.Name.EndsWith(".xml"))
                                    {
                                        // normal page
                                        string xmlData = System.Text.Encoding.UTF8.GetString(data);

                                        // Correct old documents misspelled pattern!
                                        xmlData = xmlData.Replace("<PaperPattern>Chequeued</PaperPattern>", "<PaperPattern>Chequered</PaperPattern>");
                                        journalPages.Add(page, Serialization.ReadString<JournalPage>(xmlData, System.Text.Encoding.UTF8));
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

                            // Merge dictionaries
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
                                throw new Exception("Invalid or corrupt file, cannot load data!");

                            return journal;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!quiet)
                    OnErrorOccurred?.Invoke(e.Message, "load");
            }

            return null;
        }

        public async Task<bool> UpdateJournalMetaAsync(string filePath, bool forceUpdate)
        {
            if (!wasSavedAlready && !forceUpdate)
                return await SaveAsync(filePath, true);
            else
            {
                try
                {
                    // Update Journal
                    using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
                    {
                        using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Update, false))
                        {
                            var entry = zipArchive.GetEntry("journal.xml");

                            // Delete entry first to prevent that no old content will stay there!
                            if (entry != null)
                                entry.Delete();

                            entry = zipArchive.CreateEntry("journal.xml");

                            using var stream = entry.Open();
                            var jrn = new Journal()
                            {
                                ProcessID = this.ProcessID,
                                IsBackup = this.IsBackup,
                                OriginalPath = this.OriginalPath,
                                PreviousDocumentIndex = this.PreviousDocumentIndex,
                                NextDocumentIndex = this.NextDocumentIndex,
                                ChequeredPattern = (ChequeredPattern)this.ChequeredPattern?.Clone(),
                                DottedPattern = (DottedPattern)this.DottedPattern?.Clone(),
                                RuledPattern = (RuledPattern)this.RuledPattern?.Clone(),
                            };

                            // This is just for counting the pages
                            foreach (var page in Pages)
                                jrn.Pages.Add(new JournalPage());

                            var data = Serialization.SaveToBytes(jrn, Serialization.Mode.XML);
                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }

                    return true;
                }
                catch { }
            }

            return false;
        }

        public async Task<bool> SaveAsync(string filePath, bool quiet = false, bool hideStatus = false)
        {
            lock (sync)
            {
                if (isSaving)
                    return false;
                else
                    isSaving = true;
            }

            bool retVal;

            try
            {
                if (!quiet && !hideStatus)
                    State.SetAction(StateType.Saving, ProgressState.Start);

                if (wasSavedAlready)
                {
                    // ToDo: *** Only update the archive and (eventually delete pages/images, add pages)
                    // If the file exists already and is valid zip file we do not need to re-create the entire file, but open
                    // it as ZipArchiveMode.Update and only update the bin files
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
                                var pageEntry = zipArchive.CreateEntry($"pages/page{pgCount}.pdf", System.IO.Compression.CompressionLevel.Optimal);

                                // Background will be saved separately 
                                PdfJournalPage pdfJournalPage = new PdfJournalPage
                                {
                                    Data = pdf.Data,
                                    JournalResources = pdf.JournalResources,
                                    Orientation = pdf.Orientation,
                                    PageFormat = pdf.PageFormat,
                                    PaperPattern = pdf.PaperPattern,
                                    PageBackground = null
                                };

                                using (System.IO.Stream stream = pageEntry.Open())
                                {
                                    var data = Serialization.SaveToBytes(pdfJournalPage, Serialization.Mode.XML);
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
                                var pageEntry = zipArchive.CreateEntry($"pages/page{pgCount}.xml", System.IO.Compression.CompressionLevel.Optimal);
                                using (System.IO.Stream stream = pageEntry.Open())
                                {
                                    var data = Serialization.SaveToBytes(page, Serialization.Mode.XML);
                                    await stream.WriteAsync(data, 0, data.Length);
                                }
                            }

                            pgCount++;
                        }

                        // Write journal infos
                        var info = zipArchive.CreateEntry("journal.xml");
                        using (System.IO.Stream stream = info.Open())
                        {
                            var jrn = new Journal()
                            {
                                ProcessID = this.ProcessID,
                                IsBackup = this.IsBackup,
                                OriginalPath = this.OriginalPath,
                                PreviousDocumentIndex = this.PreviousDocumentIndex,
                                NextDocumentIndex = this.NextDocumentIndex,
                                ChequeredPattern = (ChequeredPattern)this.ChequeredPattern?.Clone(),
                                DottedPattern = (DottedPattern)this.DottedPattern?.Clone(),
                                RuledPattern = (RuledPattern)this.RuledPattern?.Clone(),
                            };

                            // This is just for counting the pages
                            foreach (var page in Pages)
                                jrn.Pages.Add(new JournalPage());

                            var data = Serialization.SaveToBytes(jrn, Serialization.Mode.XML);
                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }
                }

                if (!wasSavedAlready)
                    wasSavedAlready = true;

                if (!quiet && !hideStatus)
                    State.SetAction(StateType.Saving, ProgressState.Completed);

                retVal = true;

                // Old method - only XML Serialization
                //Serialization.Serialization.Save(filePath, this, Serialization.Serialization.Mode.XML);
            }
            catch (Exception e)
            {
                retVal = false;

                if (!quiet)
                    OnErrorOccurred?.Invoke(e.Message, "save");
            }

            if (!quiet)
                State.SetAction(StateType.Saving, ProgressState.Completed);

            lock (sync)
                isSaving = false;

            return retVal;
        }

        #endregion
    }
}