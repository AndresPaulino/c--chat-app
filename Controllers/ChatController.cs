using ChatApp.Api.Models;
using ChatApp.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ChatDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _context.Messages.ToListAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast the message to all clients
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Sender, message.Content);

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}