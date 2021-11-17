using ImageMagick;
using SimpleJournal.Data;
using System.Threading.Tasks;

namespace SimpleJournal.Helper
{
    public class PdfHelper
    {
        /// <summary>
        /// Ghostscript needs to be installed: https://ghostscript.com/releases/gsdnld.html
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<MagickImageCollection> ReadPDFFileAsync(string path)
        {
            var settings = new MagickReadSettings
            {
                // Settings the density to 300 dpi will create an image with a better quality
                Density = new Density(300, 300)
            };

            // Add all the pages of the pdf file to the collection
            var images = new MagickImageCollection();
            await images.ReadAsync(path, settings);
            return images;

        }
    }
}
