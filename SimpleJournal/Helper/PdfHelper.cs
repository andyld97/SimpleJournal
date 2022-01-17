using ImageMagick;
using SimpleJournal.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        public static async Task<PdfJournalPage> CreatePdfJournalPageAsync(IMagickImage<ushort> image)
        {
            Orientation orientation = image.Width >= image.Height ? Orientation.Landscape : Orientation.Portrait;

            PdfJournalPage pdfJournalPage = null;
            await Task.Run(() =>
            {
                // Resize image to A4 pixels (96 dpi)
                image.Resize(new MagickGeometry(orientation == Orientation.Portrait ? Consts.A4WidthP : Consts.A4WidthL, orientation == Orientation.Portrait ? Consts.A4HeightP : Consts.A4HeightL) { IgnoreAspectRatio = false });

                pdfJournalPage = new PdfJournalPage
                {
                    PageBackground = image.ToByteArray(MagickFormat.Png),
                    PaperPattern = PaperType.Custom,
                    Orientation = orientation
                };
            });

            return pdfJournalPage;
        }

        public static async Task ExportJournalAsPDF(string outputPath, List<IPaper> pages)
        {
            // This method shouldn't freeze the whole gui (it's better already)
            State.SetAction(StateAction.ExportPDF, ProgressState.Start);      

            try
            {
                using (MagickImageCollection imagesToPdf = new MagickImageCollection())
                {
                    foreach (var page in pages)
                    {
                        // BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                        PngBitmapEncoder encoder = new PngBitmapEncoder();

                        RenderTargetBitmap rtb = GeneralHelper.RenderToBitmap(page as UserControl, 1.0, new SolidColorBrush(Colors.White));
                        encoder.Frames.Add(BitmapFrame.Create(rtb));

                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            encoder.Save(ms);
                            // ms.Seek(0, System.IO.SeekOrigin.Begin);

                            await Task.Run(() =>
                            {
                                imagesToPdf.Add(new MagickImage(ms.ToArray(), MagickFormat.Png));
                            });
                        }

                        rtb.Clear();
                        rtb = null;
                        encoder.Frames.Clear();
                        encoder = null;
                    }

                    await imagesToPdf.WriteAsync(outputPath, MagickFormat.Pdf);
                    imagesToPdf.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Properties.Resources.strFailedToExportJournalAsPDF}\n\n{Properties.Resources.strPDFConversationDialog_GhostscriptMessage}: {ex.Message}", Properties.Resources.strError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            State.SetAction(StateAction.ExportPDF, ProgressState.Completed);
        }
    }
}
