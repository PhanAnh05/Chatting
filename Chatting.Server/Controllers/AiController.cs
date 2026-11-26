using Chatting.Server.Hubs;
using Chatting.Server.Services;
using Chatting.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly AiAssistantService _ai;
    private readonly IHubContext<ChatHub> _hub;
    private readonly PresenceTracker _presence;

    public AiController(AiAssistantService ai, IHubContext<ChatHub> hub, PresenceTracker presence)
    {
        _ai = ai;
        _hub = hub;
        _presence = presence;
    }

    private Guid Me =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/ai/conversation — Get or create AI conversation ID for current user
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("conversation")]
    public async Task<ActionResult<ApiResponse<Guid>>> GetAiConversation()
    {
        Guid convId = await _ai.GetOrCreateAiConversationAsync(Me);
        return Ok(ApiResponse<Guid>.Ok(convId));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/ai/chat — Send a message to the AI, get reply via SignalR
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> Chat([FromBody] AiChatRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(ApiResponse<MessageDto>.Fail("Tin nhắn không được để trống."));

        Guid me = Me;
        Guid conversationId = dto.ConversationId == Guid.Empty
            ? await _ai.GetOrCreateAiConversationAsync(me)
            : dto.ConversationId;

        // Ensure user's active connection(s) are in the conversation SignalR group
        var connIds = _presence.GetConnectionIds(me);
        foreach (string connId in connIds)
        {
            await _hub.Groups.AddToGroupAsync(connId, $"conv_{conversationId}");
        }

        // 1. Save and broadcast user message IMMEDIATELY so it shows up in real-time
        var userMsgDto = await _ai.SaveUserMessageAsync(me, conversationId, dto.Message);
        if (userMsgDto != null)
        {
            await _hub.Clients.Group($"conv_{conversationId}").SendAsync("ReceiveMessage", userMsgDto);
            foreach (string connId in connIds)
            {
                await _hub.Clients.Client(connId).SendAsync("ReceiveMessage", userMsgDto);
            }
        }

        // 2. Generate AI reply, save and broadcast to real-time chat
        var aiMsgDto = await _ai.GenerateAiReplyAsync(me, conversationId);
        if (aiMsgDto != null)
        {
            await _hub.Clients.Group($"conv_{conversationId}").SendAsync("ReceiveMessage", aiMsgDto);
            foreach (string connId in connIds)
            {
                await _hub.Clients.Client(connId).SendAsync("ReceiveMessage", aiMsgDto);
            }
        }

        return Ok(ApiResponse<MessageDto>.Ok(aiMsgDto ?? userMsgDto!));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DTO for AI chat request (lives in controller since it's AI-specific)
// ─────────────────────────────────────────────────────────────────────────────
public class AiChatRequestDto
{
    public Guid ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}
