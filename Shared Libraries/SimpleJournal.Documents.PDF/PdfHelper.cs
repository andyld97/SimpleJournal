using ImageMagick;
using SimpleJournal.Common;
using Orientation = SimpleJournal.Common.Orientation;
using SimpleJournal.Documents;

namespace SimpleJournal.Helper.PDF
{
    /// <summary>
    /// Ghostscript needs to be installed: https://ghostscript.com/releases/gsdnld.html
    /// </summary>
    public class PdfHelper
    {
        public static async Task<MagickImageCollection> ReadPDFFileAsync(string path)
        {
            var settings = new MagickReadSettings
            {
                // Settings the density to 300 dpi will create an image with a better quality
                Density = new Density(300, 300),
            };

            // Add all the pages of the PDF file to the collection
            var images = new MagickImageCollection();
            await images.ReadAsync(path, settings);
            return images;
        }

        public static async Task<PdfJournalPage?> CreatePdfJournalPageAsync(IMagickImage<ushort> image)
        {
            Orientation orientation = image.Width >= image.Height ? Orientation.Landscape : Orientation.Portrait;

            PdfJournalPage? pdfJournalPage = null;
            await Task.Run(() =>
            {
                // Resize image to A4 pixels (96 dpi)
                image.Resize(new MagickGeometry(orientation == Orientation.Portrait ? (int)Consts.A4WidthP : (int)Consts.A4WidthL, orientation == Orientation.Portrait ? Consts.A4HeightP : Consts.A4HeightL) { IgnoreAspectRatio = false });

                pdfJournalPage = new PdfJournalPage
                {
                    PageBackground = image.ToByteArray(MagickFormat.Png),
                    PaperPattern = PaperType.Custom,
                    Orientation = orientation
                };
            });

            return pdfJournalPage;
        }

        public static async Task<(bool, string)> ExportJournalAsPDF(string outputPath, List<byte[]> pages)
        {
            // This method shouldn't freeze the whole GUI (it's better already)
            State.SetAction(StateType.ExportPDF, ProgressState.Start);

            try
            {
                using (MagickImageCollection imagesToPdf = new MagickImageCollection())
                {
                    foreach (var page in pages)
                    {
                        await Task.Run(() =>
                        {
                            imagesToPdf.Add(new MagickImage(page, MagickFormat.Png));
                        });
                    }

                    await imagesToPdf.WriteAsync(outputPath, MagickFormat.Pdf);
                    imagesToPdf.Dispose();

                    return (true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                State.SetAction(StateType.ExportPDF, ProgressState.Completed);
                return (false, ex.Message);
            }
            finally
            {
                State.SetAction(StateType.ExportPDF, ProgressState.Completed);
            }
        }
    }
}
