using ChatApp.Api.Models;
using ChatApp.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ChatApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatDbContext context, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }

            _logger.LogInformation($"Received message from {User.Identity?.Name}: {message.Content}");

            message.Sender = User.Identity?.Name ?? "Anonymous";
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Saved message to database. Sending to hub...");

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Sender, message.Content);

            _logger.LogInformation($"Message sent to hub.");

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();
            if (message.Sender != User.Identity.Name)
                return Forbid();
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}