using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using PDF2J.Model;
using SimpleJournal.Common;
using SimpleJournal.Common.Helper;
using SimpleJournal.Documents.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using WebhookAPI;

namespace PDF2J
{
    public class Program
    {
        public static List<PrintTicket> PrintTickets = new List<PrintTicket>();
        private static System.Timers.Timer workingTimer = new System.Timers.Timer();
        private static System.Timers.Timer cleanUPTimer = new System.Timers.Timer();

        public static Config GlobalConfig;
        public static Webhook Webhook;
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// This version must only be changed if there are changes due to the document format!
        /// </summary>
        public static readonly Version MinSJVersionRequired = new Version(0, 5, 1, 0);
        
        private static bool isRunning = false;
        private static object sync = new object();
        private static PrintTicket currentTicket = null;
        private static PdfConverter currentPdfConverter;
        private static ILogger logger;

        public static void Main(string[] args)
        {
            // Read config json (if any) [https://stackoverflow.com/a/28700387/6237448]
            string configPath = System.IO.Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "config.json");
            string preInitalizationLogMessage = string.Empty;
            if (System.IO.File.Exists(configPath))
            {
                string configJson = System.IO.File.ReadAllText(configPath);
                GlobalConfig = System.Text.Json.JsonSerializer.Deserialize<Config>(configJson);
                preInitalizationLogMessage = "Successfully initialized config file!";

                if (!string.IsNullOrEmpty(GlobalConfig.WebHookUrl))
                    Webhook = new Webhook(GlobalConfig.WebHookUrl, "PDF2J");
            }
            else
            {
                preInitalizationLogMessage = "No config file found ..."; 
                GlobalConfig = new Config();
            }

            var host = CreateHostBuilder(args).Build();
            logger = host.Services.GetService<ILogger<Program>>();

            if (!string.IsNullOrEmpty(preInitalizationLogMessage))
                logger.LogInformation(preInitalizationLogMessage);

            workingTimer = new System.Timers.Timer() { Interval = TimeSpan.FromSeconds(1).TotalMilliseconds };
            workingTimer.Elapsed += WorkingTimer_Elapsed;
            workingTimer.Start();

            cleanUPTimer = new System.Timers.Timer() { Interval = TimeSpan.FromHours(1).TotalMilliseconds };
            cleanUPTimer.Elapsed += CleanUPTimer_Elapsed;
            cleanUPTimer.Start();

            host.Run();
        }

        private static void CleanUPTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            var ticketsToRemove = PrintTickets.Where(t => (t.Status == TicketStatus.Failed || t.Status == TicketStatus.Completed) && t.DateTimeAdded.AddHours(1) >= now).ToList();

            foreach (var ticket in ticketsToRemove)
            {
                try
                {
                    PrintTickets.Remove(ticket);
                    logger.LogInformation($"[CleanUP] Ticket {ticket.Name} was removed! (Added: {ticket.DateTimeAdded.ToLongDateString()} @ {ticket.DateTimeAdded.ToLongTimeString()})");
                }
                catch { }
            }

            ticketsToRemove.Clear();
        }

        public static void CancelTicket(string id)
        {
            if (currentTicket.ID == id)
                currentPdfConverter.Cancel();
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

                currentPdfConverter = new PdfConverter(docPath, System.IO.Path.Combine(ticket.TempPath, newFileName), ticket.ConversationOptions);
                currentPdfConverter.ProgressChanged += PdfConverter_ProgressChanged;
                currentPdfConverter.JournalHasFewerPagesThenRequired += PdfConverter_JournalHasFewerPagesThenRequired;
                currentPdfConverter.Completed += PdfConverter_Completed;

                var result = await currentPdfConverter.ConvertAsync();

                // Detach events
                currentPdfConverter.ProgressChanged -= PdfConverter_ProgressChanged;
                currentPdfConverter.JournalHasFewerPagesThenRequired -= PdfConverter_JournalHasFewerPagesThenRequired;
                currentPdfConverter.Completed -= PdfConverter_Completed;
                currentPdfConverter = null;
            }

            lock (sync)
            {
                currentTicket = null;
                isRunning = false;
            }
        }

        private static async void PdfConverter_Completed(bool success, Exception ex, string destinationFileName)
        {
            if (success)
            {
                currentTicket.IsCompleted = true;
                currentTicket.Status = TicketStatus.Completed;
                logger.LogInformation($"[{currentTicket.Name}] Completed!");

                await Webhook?.PostWebHookAsync(Webhook.LogLevel.Info, $"[PDF2J] Successfully converted a pdf document \"{currentTicket.Name}\"", "Conversation");
            }
            else
            {
                logger.LogError($"[{currentTicket.Name}] Failed: {ex.Message}");

                currentTicket.Status = TicketStatus.Failed;
                currentTicket.ErorrMessage = ex.ToString() ?? string.Empty;

                if (GlobalConfig.ReportFailedTickets && !string.IsNullOrWhiteSpace(GlobalConfig.ReportPath))
                {
                    // Report failed ticket
                    string path = System.IO.Path.Combine(GlobalConfig.ReportPath, currentTicket.ID);
                    FileSystemHelper.TryCreateDirectory(path);

                    System.IO.File.Copy(System.IO.Path.Combine(currentTicket.TempPath, "doc.pdf"), System.IO.Path.Combine(path, "doc.pdf"));
                    System.IO.File.WriteAllText(System.IO.Path.Combine(path, "log.txt"), currentTicket.ErorrMessage);

                    await Webhook?.PostWebHookAsync(Webhook.LogLevel.Error, $"[PDF2J] Failed to convert a pdf document \"{currentTicket.Name}\". Ticket-ID: \"{currentTicket.ID}\"", "Conversation");
                }
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
                logger.LogInformation($"[{currentTicket.Name}] Preparing ...");
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
                    webBuilder.UseUrls($"{GlobalConfig.WebAddress}:{GlobalConfig.Port}");
                });
    }
}