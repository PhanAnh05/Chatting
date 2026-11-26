using Chatting.Server.Data;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/calls")]
[Authorize]
public class CallsController : ControllerBase
{
    private readonly ChatDbContext _db;

    public CallsController(ChatDbContext db)
    {
        _db = db;
    }

    private Guid Me =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/calls?conversationId={id}&search={text}&filter={filter}&pageIndex=1&pageSize=50
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CallInfoDto>>>> GetCallHistory(
        [FromQuery] Guid? conversationId,
        [FromQuery] string? search,
        [FromQuery] string? filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50)
    {
        Guid me = Me;

        IQueryable<Models.Call> query = _db.Calls
            .Include(c => c.Caller)
            .Include(c => c.Receiver)
            .Where(c => c.CallerId == me || c.ReceiverId == me);

        if (conversationId.HasValue)
            query = query.Where(c => c.ConversationId == conversationId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToLower();
            query = query.Where(c =>
                (c.Caller.Username != null && c.Caller.Username.ToLower().Contains(s)) ||
                (c.Caller.DisplayName != null && c.Caller.DisplayName.ToLower().Contains(s)) ||
                (c.Receiver != null && c.Receiver.Username != null && c.Receiver.Username.ToLower().Contains(s)) ||
                (c.Receiver != null && c.Receiver.DisplayName != null && c.Receiver.DisplayName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            string f = filter.Trim().ToLower();
            if (f == "incoming")
            {
                query = query.Where(c => c.ReceiverId == me);
            }
            else if (f == "outgoing")
            {
                query = query.Where(c => c.CallerId == me);
            }
            else if (f == "missed")
            {
                query = query.Where(c => (c.ReceiverId == me && c.Status == CallStatus.Missed) || c.Status == CallStatus.Missed);
            }
        }

        int total = await query.CountAsync();

        List<Models.Call> calls = await query
            .OrderByDescending(c => c.StartedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        List<CallInfoDto> dtos = calls.Select(c => new CallInfoDto
        {
            CallId = c.Id,
            ConversationId = c.ConversationId,
            CallerId = c.CallerId,
            CallerUsername = c.Caller.Username,
            CallerDisplayName = c.Caller.DisplayName,
            CallerAvatarUrl = c.Caller.AvatarUrl,
            ReceiverId = c.ReceiverId,
            ReceiverUsername = c.Receiver?.Username,
            ReceiverDisplayName = c.Receiver?.DisplayName,
            ReceiverAvatarUrl = c.Receiver?.AvatarUrl,
            CallType = c.CallType,
            Status = c.Status,
            StartedAt = c.StartedAt,
            ConnectedAt = c.ConnectedAt,
            EndedAt = c.EndedAt,
            DurationSeconds = c.DurationSeconds
        }).ToList();

        return Ok(ApiResponse<PagedResult<CallInfoDto>>.Ok(new PagedResult<CallInfoDto>
        {
            Items = dtos,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/calls/{callId}
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{callId:guid}")]
    public async Task<ActionResult<ApiResponse<CallInfoDto>>> GetCall(Guid callId)
    {
        Guid me = Me;
        Models.Call? call = await _db.Calls
            .Include(c => c.Caller)
            .Include(c => c.Receiver)
            .FirstOrDefaultAsync(c => c.Id == callId);

        if (call is null) return NotFound(ApiResponse<CallInfoDto>.Fail("Cuộc gọi không tồn tại."));

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == call.ConversationId && cm.UserId == me);
        if (!isMember) return Forbid();

        return Ok(ApiResponse<CallInfoDto>.Ok(new CallInfoDto
        {
            CallId = call.Id,
            ConversationId = call.ConversationId,
            CallerId = call.CallerId,
            CallerUsername = call.Caller.Username,
            CallerDisplayName = call.Caller.DisplayName,
            CallerAvatarUrl = call.Caller.AvatarUrl,
            ReceiverId = call.ReceiverId,
            ReceiverUsername = call.Receiver?.Username,
            ReceiverDisplayName = call.Receiver?.DisplayName,
            ReceiverAvatarUrl = call.Receiver?.AvatarUrl,
            CallType = call.CallType,
            Status = call.Status,
            StartedAt = call.StartedAt,
            ConnectedAt = call.ConnectedAt,
            EndedAt = call.EndedAt,
            DurationSeconds = call.DurationSeconds
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/calls/missed — Missed calls for current user
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("missed")]
    public async Task<ActionResult<ApiResponse<List<CallInfoDto>>>> GetMissedCalls()
    {
        Guid me = Me;
        List<Models.Call> calls = await _db.Calls
            .Include(c => c.Caller)
            .Include(c => c.Receiver)
            .Where(c => c.ReceiverId == me && c.Status == CallStatus.Missed)
            .OrderByDescending(c => c.StartedAt)
            .Take(50)
            .ToListAsync();

        return Ok(ApiResponse<List<CallInfoDto>>.Ok(calls.Select(c => new CallInfoDto
        {
            CallId = c.Id,
            ConversationId = c.ConversationId,
            CallerId = c.CallerId,
            CallerUsername = c.Caller.Username,
            CallerDisplayName = c.Caller.DisplayName,
            CallerAvatarUrl = c.Caller.AvatarUrl,
            ReceiverId = c.ReceiverId,
            ReceiverUsername = c.Receiver?.Username,
            ReceiverDisplayName = c.Receiver?.DisplayName,
            ReceiverAvatarUrl = c.Receiver?.AvatarUrl,
            CallType = c.CallType,
            Status = c.Status,
            StartedAt = c.StartedAt,
            ConnectedAt = c.ConnectedAt,
            EndedAt = c.EndedAt,
            DurationSeconds = c.DurationSeconds
        }).ToList()));
    }
}
