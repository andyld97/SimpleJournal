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
        // ToDo: *** Print (finished, or failed) tickets should be automatically deleted by a timer (maybe 1 hour)
        public static List<PrintTicket> PrintTickets = new List<PrintTicket>();
        private static System.Timers.Timer workingTimer = new System.Timers.Timer();
        
        private static bool isRunning = false;
        private static object sync = new object();
        private static PrintTicket currentTicket = null;

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
                currentTicket = ticket;
                string docPath = System.IO.Path.Combine(ticket.TempPath, "doc.pdf");
                string newFileName = $"{System.IO.Path.GetFileNameWithoutExtension(ticket.Name)}.journal";

                PdfConverter pdfConverter = new PdfConverter(docPath, System.IO.Path.Combine(ticket.TempPath, newFileName), ticket.ConversationOptions);
                pdfConverter.ProgressChanged += PdfConverter_ProgressChanged;
                pdfConverter.JournalHasFewerPagesThenRequired += PdfConverter_JournalHasFewerPagesThenRequired;
                pdfConverter.Completed += PdfConverter_Completed;

                var result = await pdfConverter.ConvertAsync();

                // Unassign events
                pdfConverter.ProgressChanged -= PdfConverter_ProgressChanged;
                pdfConverter.JournalHasFewerPagesThenRequired -= PdfConverter_JournalHasFewerPagesThenRequired;
                pdfConverter.Completed -= PdfConverter_Completed;
            }

            lock (sync)
            {
                currentTicket = null;
                isRunning = false;
            }
        }

        private static void PdfConverter_Completed(bool success, Exception ex, string destinationFileName)
        {
            if (success)
            {
                currentTicket.IsCompleted = true;
                currentTicket.Status = TicketStatus.Completed;
                logger.LogInformation($"[{currentTicket.Name}] Completed!");
            }
            else
            {
                logger.LogError($"[{currentTicket.Name}] Failed: {ex.Message}");

                currentTicket.Status = TicketStatus.Failed;
                currentTicket.ErorrMessage = ex.ToString();
            }
        }

        private static bool PdfConverter_JournalHasFewerPagesThenRequired(int firstPage, int maxPages)
        {
            // No user interaction possible; always return true
            return true;
        }

        private static void PdfConverter_ProgressChanged(PdfAction status, int progress, int currentPage, int maxPages, string journal)
        {
            if (status == PdfAction.Reading) 
            {
                logger.LogInformation($"[{currentTicket.Name}] Prepearing ...");
                currentTicket.Status = TicketStatus.Prepearing;
            }
            else if (status == PdfAction.PagesALL_WritingPage || status == PdfAction.PageRange_WritingPage)
            {
                currentTicket.Status = TicketStatus.InProgress;
                currentTicket.Percentage = progress;
                logger.LogInformation($"[{currentTicket.Name}] Converting page {currentPage} from {maxPages} ...");
            }
            else if (status == PdfAction.Saving)
            {
                currentTicket.Documents.Add(journal);
                currentTicket.Status = TicketStatus.Saving;
                logger.LogInformation($"[{currentTicket.Name}] Saving {journal} ...");
            }
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
