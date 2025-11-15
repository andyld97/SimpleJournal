using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SimpleJournal.Helper
{
    public static class SHA256Hash
    {
        public static async Task<string> CreateHashFromFileAsync(string sourceFilePath)
        {
            var fi = new System.IO.FileInfo(sourceFilePath);

            // If the file exceeds the limit of 1 GB, only create a partly hash
            if (fi.Length >= 1024 * 1024 * 1024)
                return await CreateHashFromFilePartlyAsync(sourceFilePath);

            return await CreateFullHashFromFileAsync(sourceFilePath);
        }

        private static async Task<string> CreateFullHashFromFileAsync(string sourceFilePath)
        {
            using SHA256 sha256Hash = SHA256.Create();
            using System.IO.FileStream fs = new System.IO.FileStream(sourceFilePath, System.IO.FileMode.Open);
            var result = await sha256Hash.ComputeHashAsync(fs);
            return result.ToHash();
        }

        private static async Task<string> CreateHashFromFilePartlyAsync(string sourceFilePath, int length = 50 * 1024 * 1024)
        {
            var bytes = await CreateHashBufferAsync(sourceFilePath, length);
            return SHA256.HashData(bytes).ToHash();
        }

        private static string ToHash(this byte[] data)
        {
            return string.Join(string.Empty, data.Select(p => p.ToString("x2")));
        }

        private static async Task<byte[]> CreateHashBufferAsync(string sourceFilePath, int length)
        {
            byte[] data;
            var fi = new System.IO.FileInfo(sourceFilePath);
            if (fi.Length < length * 3)
                data = await System.IO.File.ReadAllBytesAsync(fi.FullName);
            else
            {
                data = new byte[length * 3];

                using System.IO.FileStream fs = new System.IO.FileStream(sourceFilePath, System.IO.FileMode.Open);

                // First n bytes
                await fs.ReadExactlyAsync(data, 0, length);

                // Middle + n bytes
                fs.Seek(fs.Length / 2, System.IO.SeekOrigin.Begin);
                await fs.ReadExactlyAsync(data, length, length);

                // End - n bytes
                fs.Seek(fs.Length - length, System.IO.SeekOrigin.Begin);
                await fs.ReadExactlyAsync(data, length * 2, length);
            }

            return data;
        }
    }
}
