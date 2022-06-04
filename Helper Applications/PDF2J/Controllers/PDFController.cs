using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SimpleJournal.Common;
using static SimpleJournal.Common.Language;
using SimpleJournal.Documents.PDF;
using System;
using System.Threading.Tasks;
using SimpleJournal.Common.Helper;

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
            
            // Get language (if any)
            string language = "en";
            if (Request.Headers.TryGetValue("language", out StringValues sv))
                language = sv.ToString().GetSupportedLangauge();

            // Get options
            PdfConversationOptions options = null;
            if (Request.Headers.TryGetValue("options", out StringValues opt))
            {
                try
                {
                    options = System.Text.Json.JsonSerializer.Deserialize<PdfConversationOptions>(opt.ToString());
                }
                catch (Exception ex)
                {
                    return BadRequest($"Failed to read options: {ex.Message}");
                }
            }

            // Create a new printing ticket
            PrintTicket ticket = new PrintTicket
            {
                Name = file.FileName,
                Status = TicketStatus.OnHold,
                DateTimeAdded = DateTime.Now,
                ConversationOptions = options
            };

            // Check version            
            bool isVersionTooOld;
            if (Version.TryParse(ticket.ConversationOptions.CurrentSimpleJounalVersion, out var version) && version < Program.MinSJVersionRequired)
                isVersionTooOld = true;
            else
                isVersionTooOld = true;

            if (isVersionTooOld)
                BadRequest(Properties.Resources.ResourceManager.GetString(nameof(Properties.Resources.strVersionTooOld), language));

            // Create the the working directory for this printing ticket
            FileSystemHelper.TryCreateDirectory(ticket.TempPath);

            // Copy the file to temp
            try
            {
                using System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(ticket.TempPath, "doc.pdf"), System.IO.FileMode.OpenOrCreate);
                using var source = file.OpenReadStream();
                await source.CopyToAsync(fs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to copy file: {ex.Message}");
            }

            // Add the printing ticket to list
            lock (Program.PrintTickets)
                Program.PrintTickets.Add(ticket);

            return Ok(ticket);
        }
    }
}