using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace PDF2J.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ILogger<TicketController> _logger;

        public TicketController(ILogger<TicketController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult GetTicket(string id)
        {
            var result = Program.PrintTickets.FirstOrDefault(t => t.ID == id);

            if (result == null)
                return BadRequest($"Ticket {id} not found!");

            return Ok(result);
        }

        [HttpGet("{id}/cancel")]
        public IActionResult CancelTicket(string id)
        {
            var result = Program.PrintTickets.FirstOrDefault(t => t.ID == id);

            if (result == null)
                return BadRequest($"Ticket {id} not found!");

            result.Status = SimpleJournal.Common.TicketStatus.Canceld;
            _logger.LogInformation($"Ticket {result.Name} was canceld by the user!");

            Program.CancelTicket(id);
            return DeleteTicket(id);
        }

        [HttpGet("{id}/download/{document}")]
        public IActionResult DownloadDocument(string id, string document)
        {
            var result = Program.PrintTickets.FirstOrDefault(t => t.ID == id);

            if (result == null)
                return BadRequest($"Ticket {id} not found!");

            if (string.IsNullOrEmpty(document))
                return BadRequest("Invalid document specified!");

            return Ok(new System.IO.FileStream(System.IO.Path.Combine(result.TempPath, document), System.IO.FileMode.Open));
        }

        [HttpGet("all")]
        public IActionResult GetAllTickets()
        {
            return Ok(Program.PrintTickets.Select(p => p.ID).ToList());
        }

        [HttpGet("{id}/delete")]
        public IActionResult DeleteTicket(string id)
        {
            var result = Program.PrintTickets.FirstOrDefault(t => t.ID == id);

            if (result == null)
                return BadRequest($"Ticket {id} not found!");

            try
            {
                if (System.IO.Directory.Exists(result.TempPath))
                    System.IO.Directory.Delete(result.TempPath);
            }
            catch
            {
                // ignore
            }

            Program.PrintTickets.Remove(result);
            _logger.LogInformation($"[{result.Name}] Ticket \"{result.ID}\" got deleted!");

            return Ok();
        }
    }
}
