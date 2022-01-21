using System.IO;
using System.Windows.Media.Imaging;

namespace SimpleJournal.Documents.UI.Helper
{
    public static class ImageHelper
    {
        public delegate void onErrorOccured(string message, string scope);
        public static event onErrorOccured OnErrorOccured;

        public static byte[] ExportImage(BitmapSource bi)
        {
            try
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bi));
                    encoder.Save(memStream);
                    return memStream.ToArray();
                }
            }
            catch (Exception e)
            {
                OnErrorOccured?.Invoke(e.Message, "export");
                return Array.Empty<byte>();
            }
        }

        public static BitmapImage LoadImageFromBase64(string data)
        {
            return LoadImage(Convert.FromBase64String(data));
        }

        public static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            try
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
            catch (Exception e)
            {
                OnErrorOccured?.Invoke(e.Message, "load");
                return null;
            }
        }

        public static BitmapImage LoadImage(Uri url)
        {
            if (url == null) return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = url;
                image.StreamSource = null;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception e)
            {
                OnErrorOccured?.Invoke(e.Message, "load");
                return null;
            }
        }

    }
}
