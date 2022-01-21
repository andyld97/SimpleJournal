using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace PDF2J.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetTicket(string id)
        {
            var result = Program.PrintTickets.FirstOrDefault(t => t.ID == id);

            if (result == null)
                return BadRequest($"Ticket {id} not found!");

            return Ok(result);
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
    }
}
