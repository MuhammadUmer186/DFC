using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using RestaurantSystem.DTOs;
using RestaurantSystem.Services.Ai;

namespace RestaurantSystem.Controllers
{
    [ApiController]
    [Route("api/ai/assistant")]
    [Authorize(Roles = "SuperAdmin,Admin,MainAdmin")]
    [EnableRateLimiting("ai")]
    public class AiAssistantController : ControllerBase
    {
        private readonly InsightsAssistantService _service;
        private readonly AiFeatureOptions _features;

        public AiAssistantController(InsightsAssistantService service, IOptions<AiFeatureOptions> features)
        {
            _service = service;
            _features = features.Value;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(AskAssistantRequest request)
        {
            if (!_features.InsightsAssistantEnabled) return StatusCode(503, new { message = "The business insights assistant is currently disabled" });
            if (string.IsNullOrWhiteSpace(request.Question)) return BadRequest(new { message = "Question is required" });

            var (userId, userName, role) = CurrentUser();
            var result = await _service.AskAsync(request.ConversationId, request.Question, userId, userName, role);

            if (!result.Success)
                return StatusCode(502, new { message = result.Error ?? "The assistant could not answer that question", conversationId = result.ConversationId });

            return Ok(new AskAssistantResponse { ConversationId = result.ConversationId, Answer = result.Answer, ToolsUsed = result.ToolsUsed });
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var (userId, _, _) = CurrentUser();
            var conversations = await _service.GetConversationsAsync(userId);
            return Ok(conversations.Select(c => new ConversationSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt,
                LastMessageAt = c.LastMessageAt
            }));
        }

        [HttpGet("conversations/{id}")]
        public async Task<IActionResult> GetConversation(int id)
        {
            var (userId, _, _) = CurrentUser();
            var conversation = await _service.GetConversationAsync(id, userId);
            if (conversation == null) return NotFound();

            return Ok(new ConversationDetailDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Messages = conversation.Messages.Select(m => new ConversationMessageDto
                {
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToList()
            });
        }

        private (int userId, string userName, string role) CurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out var userId);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            return (userId, userName, role);
        }
    }
}
