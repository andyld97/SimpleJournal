using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleJournal.Common.Helper
{
    public static class FileSystemHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);

        public static string GetDownloadsPath()
        {
            string downloads;
            try
            {
                SHGetKnownFolderPath(KnownFolder.Downloads, 0, IntPtr.Zero, out downloads);
                return downloads;
            }
            catch
            {
                // Use the temp path in case if there is a problem 
                return System.IO.Path.GetTempPath();
            }

        }

        public static bool TryDeleteFile(string fileName)
        {
            try
            {
                System.IO.File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryWriteAllBytes(string fileName, byte[] data)
        {
            try
            {
                System.IO.File.WriteAllBytes(fileName, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string BuildSHA1FromFile(string fileName)
        {
            try
            {
                byte[] data = System.IO.File.ReadAllBytes(fileName);
                return SHA1(data);
            }
            catch
            {
                // ignore
            }

            return string.Empty;
        }

        private static string SHA1(byte[] input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(input);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static void ExtractZipFile(byte[] data, string targetDirectoryPath)
        {
            if (!System.IO.Directory.Exists(targetDirectoryPath))
                System.IO.Directory.CreateDirectory(targetDirectoryPath);

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
            using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read, false))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    // Get the target file path and the parent directory
                    string targetPath = System.IO.Path.Combine(targetDirectoryPath, entry.FullName);
                    string parentDirectory = System.IO.Path.GetDirectoryName(targetPath);

                    // Create parent directory (if it doesn't exists)
                    if (!System.IO.Directory.Exists(parentDirectory))
                        System.IO.Directory.CreateDirectory(parentDirectory);

                    // Ignore empty folder entries
                    if (string.IsNullOrEmpty(entry.Name) && entry.Length == 0)
                        continue;

                    // Write the file to disk
                    using (System.IO.FileStream stream = new System.IO.FileStream(targetPath, System.IO.FileMode.Create))
                    using (var e = entry.Open())
                        e.CopyTo(stream);
                }
            }
        }

        public static async Task ExtractZipFileAsync(byte[] data, string targetDirectoryPath)
        {
            if (!System.IO.Directory.Exists(targetDirectoryPath))
                System.IO.Directory.CreateDirectory(targetDirectoryPath);

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data))
            using (System.IO.Compression.ZipArchive zipArchive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read, false))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    // Get the target file path and the parent directory
                    string targetPath = System.IO.Path.Combine(targetDirectoryPath, entry.FullName);
                    string parentDirectory = System.IO.Path.GetDirectoryName(targetPath);

                    // Create parent directory (if it doesn't exists)
                    if (!System.IO.Directory.Exists(parentDirectory))
                        System.IO.Directory.CreateDirectory(parentDirectory);

                    // Ignore empty folder entries
                    if (string.IsNullOrEmpty(entry.Name) && entry.Length == 0)
                        continue;

                    // Write the file to disk
                    using (System.IO.FileStream stream = new System.IO.FileStream(targetPath, System.IO.FileMode.Create))
                    using (var e = entry.Open())
                        await e.CopyToAsync(stream);
                }
            }
        }
    }

    public static class KnownFolder
    {
        public static readonly Guid Downloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
    }
}