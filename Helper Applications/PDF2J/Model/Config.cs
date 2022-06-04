using System.Text.Json.Serialization;

namespace PDF2J.Model
{
    /// <summary>
    /// Contains PDF2J Configuration Settings
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The url where pdf2j is available under (must be specified fully like the default value)<br/>
        /// To make this API publicly available there are two options:<br/><br/>
        /// 1. Use localhost and an nginx (or any other) reverse proxy<br/>
        /// 2. Use http://0.0.0.0 as url, then no reverse proxy is required!<br/><br/>
        /// But don't forget to open <see cref="Port"/> in your firewall!<br/>
        /// </summary>
        [JsonPropertyName("url")]
        public string WebAddress { get; set; } = "http://localhost";

        /// <summary>
        /// On this port the API will listen to incoming request (can be a value from 1-2^16, except ports which are occupied by default)
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; } = 5290;

        /// <summary>
        /// The path where failed tickets get copied to, this
        /// should be a seperate folder. A so called "Report" has<br/><br/>
        /// - a unique id (the guid of the ticket)<br/>
        /// - exception stacktrace (log.txt)<br/><br/>
        /// and the document itself "doc.pdf"
        /// </summary>
        [JsonPropertyName("report_path")]
        public string ReportPath { get; set; }

        /// <summary>
        /// Can be specified to notify a webhook (can be empty) <br/>
        /// The url will be called if<br/><br/>
        /// - a conversation has finished successfully<br/>
        /// - a converstaion has failed<br/><br/>
        /// The endpoint gets called with a message parameter containg the actual message (urlencoded)<br/>
        /// e.g. http://test.de/?message=xyz
        /// </summary>
        [JsonPropertyName("webhook_url")]
        public string WebHookUrl { get; set; }

        /// <summary>
        /// If true failed tickets get saved to <see cref="ReportPath"/> <br/>
        /// This settings also impacts the Webhook!
        /// </summary>
        [JsonPropertyName("report_failed_tickets")]
        public bool ReportFailedTickets { get; set; }
    }
}