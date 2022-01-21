using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleJournal.Common;
using SimpleJournal.Documents;
using SimpleJournal.Documents.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PDF2J
{
    public class Program
    {
        public static List<PrintTicket> PrintTickets = new List<PrintTicket>();
        private static System.Timers.Timer workingTimer = new System.Timers.Timer();
        
        private static bool isRunning = false;
        private static object sync = new object();

        private  static ILogger logger;

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            logger = host.Services.GetService<ILogger<Program>>();

            workingTimer = new System.Timers.Timer() { Interval = TimeSpan.FromSeconds(1).TotalMilliseconds };
            workingTimer.Elapsed += WorkingTimer_Elapsed;
            workingTimer.Start();

            host.Run();
        }

        private static async void WorkingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (sync)
            {
                if (isRunning)
                    return;
                else
                    isRunning = true;
            }

            // Get the next printing ticket
            var ticket = PrintTickets.FirstOrDefault(t => t.Status == TicketStatus.OnHold);

            if (ticket != null)
            {
                // Let the work begin
                try
                {
                    logger.LogInformation($"[{ticket.Name}] Prepearing ...");
                    ticket.Status = TicketStatus.Prepearing;
                    string docPath = System.IO.Path.Combine(ticket.TempPath, "doc.pdf");

                    var settings = new MagickReadSettings
                    {
                        // Settings the density to 300 dpi will create an image with a better quality
                        Density = new Density(300, 300)
                    };

                    Journal journal = new Journal();
                    using (ImageMagick.MagickImageCollection magickImages = new ImageMagick.MagickImageCollection())
                    {
                        await magickImages.ReadAsync(docPath, settings);
                        ticket.Status = TicketStatus.InProgress;
                        for (int p = 0; p < magickImages.Count; p++)
                        {
                            logger.LogInformation($"[{ticket.Name}] Converting page {p + 1} from {magickImages.Count} ...");
                            var page = await CreatePdfJournalPageAsync(magickImages[p]);
                            journal.Pages.Add(page);

                            // Update ticket
                            ticket.Percentage = (int)(((p + 1) / (double)magickImages.Count) * 100.0);
                        }
                    }

                    ticket.Status = TicketStatus.Saving;
                    logger.LogInformation($"[{ticket.Name}] Saving {ticket}.journal ...");
                    await journal.SaveAsync(System.IO.Path.Combine(ticket.TempPath, $"{ticket.Name}.journal"));

                    ticket.Documents.Add($"{ticket.Name}.journal");
                    ticket.Status = TicketStatus.Completed;
                    logger.LogInformation($"[{ticket.Name}] Completed!");
                    ticket.IsCompleted = true;
                }
                catch (Exception ex)
                {
                    logger.LogError($"[{ticket.Name}] Failed: {ex.Message}");

                    ticket.Status = TicketStatus.Failed;
                    ticket.ErorrMessage = ex.ToString();
                }
            }

            lock (sync)
                isRunning = false;
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:5290");
                });
    }
}
