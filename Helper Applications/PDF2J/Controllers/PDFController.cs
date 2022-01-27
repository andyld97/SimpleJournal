using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleJournal.Common;
using SimpleJournal.Documents.PDF;
using System;
using System.Threading.Tasks;

namespace PDF2J.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PDFController : ControllerBase
    {
        private readonly ILogger<PDFController> _logger;

        public PDFController(ILogger<PDFController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync([FromQuery] IFormFile file)
        {
            if (file == null)
                return BadRequest("invalid file");

            string optionsJson = Request.Headers["options"].ToString();

            // Create a new printing ticket
            PrintTicket ticket = new PrintTicket() { Name = file.FileName, Status = TicketStatus.OnHold, DateTimeAdded = DateTime.Now };
            ticket.ConversationOptions = System.Text.Json.JsonSerializer.Deserialize<PdfConversationOptions>(optionsJson);
     
            // Create the the working directory for this printing ticket
            try
            {
                System.IO.Directory.CreateDirectory(ticket.TempPath);
            }
            catch
            { }

            // Copy the file to temp
            using (System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(ticket.TempPath, "doc.pdf"), System.IO.FileMode.OpenOrCreate))
            {
                using (var source = file.OpenReadStream())
                {
                    await source.CopyToAsync(fs);
                }
            }

            // Add the printing ticket to list
            lock (Program.PrintTickets)
                Program.PrintTickets.Add(ticket);

            return Ok(ticket);
        }
    }
}
