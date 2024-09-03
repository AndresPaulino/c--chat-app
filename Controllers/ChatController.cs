using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Chat API is working!");
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // Simulating fetching a chat by id
            if (id <= 0)
            {
                return BadRequest("Invalid chat ID");
            }
            return Ok($"Details for chat {id}");
        }

        [HttpPost]
        public IActionResult Create([FromBody] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("Message cannot be empty");
            }
            // Simulating chat creation
            return CreatedAtAction(nameof(GetById), new { id = 1 }, $"Created chat with message: {message}");
        }

        [HttpGet("recent")]
        public IActionResult GetRecent()
        {
            // This will respond to GET api/chat/recent
            return Ok("Recent chats");
        }
    }
}