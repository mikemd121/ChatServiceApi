using Microsoft.AspNetCore.Mvc;

namespace ChatServiceApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportChatController : ControllerBase
    {
        private readonly IChatQueueService _chatService;

        public SupportChatController(IChatQueueService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] string username)
        {
            var result = await _chatService.StartChatAsync(username);
            return Ok(result);
        }

        [HttpPost("ping")]
        public IActionResult Ping([FromBody] string username)
        {
            var result = _chatService.Ping(username);
            return Ok(result);
        }
    }
}
