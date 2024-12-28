using SimpleJournal.Common;
using System.Text.Json.Serialization;

namespace SimpleJournal.Documents.PDF
{
    public class PrintTicket
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("documents")]
        public List<string> Documents { get; set; } = [];

        [JsonPropertyName("status")]
        public TicketStatus Status { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Value from 1-100
        /// </summary>
        [JsonPropertyName("percentage")]
        public int Percentage { get; set; }

        [JsonPropertyName("error_message")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("date_time_added")]
        public DateTime DateTimeAdded { get; set; }

        [JsonPropertyName("conversation_options")]
        public PdfConversationOptions ConversationOptions { get; set; } = new PdfConversationOptions();

        [JsonIgnore]
        public string TempPath => System.IO.Path.Combine(System.IO.Path.GetTempPath(), ID);

        public override string ToString()
        {
            return Name;
        }
    }
}