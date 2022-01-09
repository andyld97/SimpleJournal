using ImageMagick;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleJournal.Helper
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
                Density = new Density(300, 300)
            };

            // Add all the pages of the pdf file to the collection
            var images = new MagickImageCollection();
            await images.ReadAsync(path, settings);
            return images;
        }

        public static async Task ExportJournalAsPDF(string outputPath, List<IPaper> pages)
        {
            State.SetAction(StateAction.ExportPDF, ProgressState.Start);
            // ToDo: *** Error Handling
            // This method shouldn't freeze the whole gui (it's better already)

            using (MagickImageCollection imagesToPdf = new MagickImageCollection())
            {
                foreach (var page in pages)
                {
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();

                    RenderTargetBitmap rtb = GeneralHelper.RenderToBitmap(page.Canvas, 1.0, new SolidColorBrush(Colors.White));
                    encoder.Frames.Add(BitmapFrame.Create(rtb));

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        encoder.Save(ms);
                        ms.Seek(0, System.IO.SeekOrigin.Begin);

                        await Task.Run(() => {

                            imagesToPdf.Add(new MagickImage(ms));
                        });                        
                    }

                    rtb.Clear();
                    rtb = null;
                    encoder.Frames.Clear();
                    encoder = null;
                }

                await imagesToPdf.WriteAsync(outputPath);
                imagesToPdf.Dispose();
            }

            State.SetAction(StateAction.ExportPDF, ProgressState.Completed);
        }
    }
}
