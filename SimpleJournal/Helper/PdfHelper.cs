using ImageMagick;
using SimpleJournal.Data;
using System.Threading.Tasks;

namespace SimpleJournal.Helper
{
    public class PdfHelper
    {
        public static async Task<Journal> ConvertPDFToJournal(string path)
        {
            // Ghostscript needs to be installed: https://ghostscript.com/releases/gsdnld.html

            Journal journal = new Journal();

            var settings = new MagickReadSettings
            {
                // Settings the density to 300 dpi will create an image with a better quality
                Density = new Density(300, 300)
            };

            using (var images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                await images.ReadAsync(path, settings);

                foreach (var image in images)
                {
                    PdfJournalPage pdfJournalPage = new PdfJournalPage
                    {
                        PageBackground = image.ToByteArray(MagickFormat.Png),
                        PaperPattern = PaperType.Custom
                    };
                    journal.Pages.Add(pdfJournalPage);

                }
            }

            return journal;
        }
    }
}
