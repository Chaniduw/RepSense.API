using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepSense.API.Services;

namespace RepSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly CoachChatService _coachChatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(CoachChatService coachChatService, ILogger<ChatController> logger)
        {
            _coachChatService = coachChatService;
            _logger = logger;
        }

        /// <summary>
        /// Fitness coach reply using OpenAI and the current user's MongoDB data.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CoachChatRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { Message = "Message is required." });
            }

            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }

            try
            {
                var reply = await _coachChatService.GetCoachReplyAsync(userId, request.Message.Trim(), cancellationToken);
                return Ok(new { message = reply });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("OpenAI API key", StringComparison.Ordinal))
            {
                return StatusCode(503, new { Message = "Coach is unavailable: OpenAI API key is not configured." });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "OpenAI request failed");
                return StatusCode(502, new { Message = "Failed to reach AI service.", Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Coach chat failed");
                return StatusCode(500, new { Message = "Failed to get coach reply." });
            }
        }
    }

    public class CoachChatRequest
    {
        public required string Message { get; set; }
    }
}
