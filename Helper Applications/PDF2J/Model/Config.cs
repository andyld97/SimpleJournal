using System.Text.Json.Serialization;

namespace PDF2J.Model
{
    /// <summary>
    /// Contains PDF2J Configuration Settings
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The path where failed tickets get copied to, this
        /// should be a seperate folder. A so called "Report" has
        /// - a unique id (the guid of the ticket)
        /// - exception stacktrace (log.txt)
        /// and the document itself "doc.pdf"
        /// </summary>
        [JsonPropertyName("report_path")]
        public string ReportPath { get; set; }

        /// <summary>
        /// Can be specified to notify a webhook (can be empty)
        /// The url will be called if
        /// - a conversation has finished successfully
        /// - a converstaion has failed
        /// The endpoint gets called with a message parameter containg the actual message (urlencoded)
        /// e.g. http://test.de/?message=xyz
        /// </summary>
        [JsonPropertyName("webhook_url")]
        public string WebHookUrl { get; set; }

        /// <summary>
        /// If true failed tickets get saved to <see cref="ReportPath"/>
        /// This settings also impacts the Webhook!
        /// </summary>
        [JsonPropertyName("report_failed_tickets")]
        public bool ReportFailedTickets { get; set; }
    }
}