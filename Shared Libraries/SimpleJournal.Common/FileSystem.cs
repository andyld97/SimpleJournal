using System.Security.Cryptography;
using System.Text;

namespace SimpleJournal.Common
{
    public static class FileSystem
    {
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
    }
}