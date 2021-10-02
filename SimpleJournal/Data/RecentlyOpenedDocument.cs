using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleJournal.Data
{
    public class RecentlyOpenedDocuments
    {
#if !UWP
        private static readonly string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "RecentlyOpenedDocuments.xml");
#else
        private static readonly string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal", "RecentlyOpenedDocuments.xml");
#endif

        public static List<Document> Instance = RecentlyOpenedDocuments.Load();

        public delegate void documentsChanged();
        public static documentsChanged DocumentsChanged;

        static RecentlyOpenedDocuments()
        {
#if UWP 
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleJournal");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
#endif
        }        

        private static List<Document> Load()
        {
            List<Document> result = null;
            try
            {
                result = Serialization.Serialization.Read<List<Document>>(path, Serialization.Serialization.Mode.Normal);
            }
            catch
            {
                // Silence is golden
            }


            if (result == null)
            {
                result = new List<Document>();
                Save(result);
            }

            return result;
        } 

        public static void Save()
        {
            Save(Instance);
        }

        private static void Save(List<Document> docuemnts)
        {
            try
            {
                Serialization.Serialization.Save<List<Document>>(path, docuemnts, Serialization.Serialization.Mode.Normal);
            }
            catch
            {
                // Silence is golden
            }
        }

        public static void AddDocument(string filePath)
        {
            // Check if documents existis
            var result = Instance.Where(d => d.Path == filePath).FirstOrDefault();

            if (result == null)
                Instance.Insert(0, new Document() { Path = filePath, LastModified = DateTime.Now });
            else
            {
                // Place it to top
                Instance.Remove(result);
                Instance.Insert(0, result);
                result.LastModified = DateTime.Now;
            }

            // Limit to value of recently used docuemnts
            if (Instance.Count > Consts.LastRecentlyOpenedDocuments)
                Instance.RemoveAt(Instance.Count - 1);

            Save(Instance);
            DocumentsChanged?.Invoke();
        }

        public static void Remove(string fileName)
        {
            Instance.Remove(Instance.Where(f => f.Path == fileName).FirstOrDefault());
            Save(Instance);

            DocumentsChanged?.Invoke();
        }
    }

    public class Document
    {
        public string Path { get; set; }

        public DateTime LastModified { get; set; }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    }
}
