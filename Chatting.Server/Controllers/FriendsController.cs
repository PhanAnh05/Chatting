using Chatting.Server.Data;
using Chatting.Server.Hubs;
using Chatting.Server.Models;
using Chatting.Server.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly IHubContext<ChatHub> _hub;
    private readonly PresenceTracker _presence;

    public FriendsController(ChatDbContext db, IHubContext<ChatHub> hub, PresenceTracker presence)
    {
        _db = db;
        _hub = hub;
        _presence = presence;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/friends — List all friends
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FriendDto>>>> GetFriends()
    {
        Guid me = CurrentUserId;

        List<Friendship> friendships = await _db.Friendships
            .Include(f => f.User1)
            .Include(f => f.User2)
            .Where(f => f.User1Id == me || f.User2Id == me)
            .ToListAsync();

        List<FriendDto> result = [];
        foreach (Friendship fs in friendships)
        {
            User friend = fs.User1Id == me ? fs.User2 : fs.User1;

            // Find direct conversation
            Guid? directConvId = await _db.Conversations
                .Where(c => c.Type == ConversationType.Direct
                         && c.Members.Any(m => m.UserId == me)
                         && c.Members.Any(m => m.UserId == friend.Id))
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync();

            if (!directConvId.HasValue)
            {
                Conversation conv = new()
                {
                    Id = Guid.NewGuid(),
                    Type = ConversationType.Direct,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Conversations.Add(conv);
                _db.ConversationMembers.AddRange(
                    new ConversationMember { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = friend.Id, JoinedAt = DateTime.UtcNow },
                    new ConversationMember { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = me, JoinedAt = DateTime.UtcNow });
                await _db.SaveChangesAsync();
                directConvId = conv.Id;
            }

            result.Add(new FriendDto
            {
                UserId = friend.Id,
                Username = friend.Username,
                Email = friend.Email,
                AvatarUrl = friend.AvatarUrl,
                Status = friend.Status,
                IsOnline = _presence.IsOnline(friend.Id),
                CreatedAt = fs.CreatedAt,
                DirectConversationId = directConvId
            });
        }

        return Ok(ApiResponse<List<FriendDto>>.Ok(result));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/friends/search?keyword=...
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<UserSearchResultDto>>>> Search([FromQuery] string keyword)
    {
        Guid me = CurrentUserId;
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(ApiResponse<List<UserSearchResultDto>>.Fail("Keyword không được để trống."));

        string kw = keyword.Trim().ToLowerInvariant();

        List<User> users = await _db.Users
            .Where(u => u.Id != me && u.IsActive &&
                       (u.Username.ToLower().Contains(kw) || u.Email.ToLower().Contains(kw)))
            .Take(20)
            .ToListAsync();

        // Pre-fetch relationships
        List<Guid> userIds = users.Select(u => u.Id).ToList();

        HashSet<Guid> friendIds = (await _db.Friendships
            .Where(f => (f.User1Id == me && userIds.Contains(f.User2Id)) ||
                        (f.User2Id == me && userIds.Contains(f.User1Id)))
            .Select(f => f.User1Id == me ? f.User2Id : f.User1Id)
            .ToListAsync()).ToHashSet();

        List<FriendRequest> requests = await _db.FriendRequests
            .Where(r => r.Status == FriendRequestStatus.Pending &&
                       ((r.SenderId == me && userIds.Contains(r.ReceiverId)) ||
                        (r.ReceiverId == me && userIds.Contains(r.SenderId))))
            .ToListAsync();

        List<UserSearchResultDto> result = users.Select(u =>
        {
            FriendRequest? sentReq = requests.FirstOrDefault(r => r.SenderId == me && r.ReceiverId == u.Id);
            FriendRequest? receivedReq = requests.FirstOrDefault(r => r.ReceiverId == me && r.SenderId == u.Id);
            return new UserSearchResultDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Status = u.Status,
                IsFriend = friendIds.Contains(u.Id),
                HasSentRequest = sentReq is not null,
                HasReceivedRequest = receivedReq is not null,
                PendingRequestId = sentReq?.Id ?? receivedReq?.Id,
                IsOnline = _presence.IsOnline(u.Id)
            };
        }).ToList();

        return Ok(ApiResponse<List<UserSearchResultDto>>.Ok(result));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/friends/requests/incoming
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("requests/incoming")]
    public async Task<ActionResult<ApiResponse<List<FriendRequestDto>>>> GetIncomingRequests()
    {
        Guid me = CurrentUserId;
        List<FriendRequest> requests = await _db.FriendRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Where(r => r.ReceiverId == me && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<FriendRequestDto>>.Ok(requests.Select(MapRequestDto).ToList()));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/friends/requests/outgoing
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("requests/outgoing")]
    public async Task<ActionResult<ApiResponse<List<FriendRequestDto>>>> GetOutgoingRequests()
    {
        Guid me = CurrentUserId;
        List<FriendRequest> requests = await _db.FriendRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Where(r => r.SenderId == me && r.Status == FriendRequestStatus.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<List<FriendRequestDto>>.Ok(requests.Select(MapRequestDto).ToList()));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/friends/requests — Send friend request
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("requests")]
    public async Task<ActionResult<ApiResponse<FriendRequestDto>>> SendRequest([FromBody] SendFriendRequestDto dto)
    {
        Guid me = CurrentUserId;
        if (dto.ReceiverId == me)
            return BadRequest(ApiResponse<FriendRequestDto>.Fail("Không thể kết bạn với chính mình."));

        User? receiver = await _db.Users.FindAsync(dto.ReceiverId);
        if (receiver is null || !receiver.IsActive)
            return NotFound(ApiResponse<FriendRequestDto>.Fail("Người dùng không tồn tại."));

        bool alreadyFriends = await _db.Friendships.AnyAsync(
            f => (f.User1Id == me && f.User2Id == dto.ReceiverId) ||
                 (f.User1Id == dto.ReceiverId && f.User2Id == me));
        if (alreadyFriends)
            return BadRequest(ApiResponse<FriendRequestDto>.Fail("Hai người đã là bạn bè."));

        bool existsPending = await _db.FriendRequests.AnyAsync(
            r => r.Status == FriendRequestStatus.Pending &&
                 ((r.SenderId == me && r.ReceiverId == dto.ReceiverId) ||
                  (r.SenderId == dto.ReceiverId && r.ReceiverId == me)));
        if (existsPending)
            return BadRequest(ApiResponse<FriendRequestDto>.Fail("Đã có lời mời kết bạn đang chờ xử lý."));

        FriendRequest req = new()
        {
            Id = Guid.NewGuid(),
            SenderId = me,
            ReceiverId = dto.ReceiverId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _db.FriendRequests.Add(req);
        await _db.SaveChangesAsync();

        // Load sender for DTO
        User sender = (await _db.Users.FindAsync(me))!;
        req.Sender = sender;
        req.Receiver = receiver;

        FriendRequestDto outDto = MapRequestDto(req);

        // Notify receiver via SignalR
        foreach (string connId in _presence.GetConnectionIds(dto.ReceiverId))
            await _hub.Clients.Client(connId).SendAsync("NewFriendRequest", outDto);

        return Ok(ApiResponse<FriendRequestDto>.Ok(outDto, "Đã gửi lời mời kết bạn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/friends/requests/{id}/accept
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("requests/{id:guid}/accept")]
    public async Task<ActionResult<ApiResponse>> AcceptRequest(Guid id)
    {
        Guid me = CurrentUserId;
        FriendRequest? req = await _db.FriendRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (req is null || req.ReceiverId != me)
            return NotFound(ApiResponse.Fail("Lời mời không tồn tại."));
        if (req.Status != FriendRequestStatus.Pending)
            return BadRequest(ApiResponse.Fail("Lời mời đã được xử lý."));

        req.Status = FriendRequestStatus.Accepted;
        req.RespondedAt = DateTime.UtcNow;

        // Create friendship (canonical: smaller GUID is User1)
        Guid u1 = req.SenderId < me ? req.SenderId : me;
        Guid u2 = req.SenderId < me ? me : req.SenderId;
        Friendship friendship = new()
        {
            Id = Guid.NewGuid(),
            User1Id = u1,
            User2Id = u2,
            CreatedAt = DateTime.UtcNow
        };
        _db.Friendships.Add(friendship);

        // Create or find direct conversation
        Guid? existingConvId = await _db.Conversations
            .Where(c => c.Type == ConversationType.Direct
                     && c.Members.Any(m => m.UserId == req.SenderId)
                     && c.Members.Any(m => m.UserId == me))
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();

        Guid convId;
        if (existingConvId.HasValue)
        {
            convId = existingConvId.Value;
        }
        else
        {
            Conversation conv = new()
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Direct,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Conversations.Add(conv);
            _db.ConversationMembers.AddRange(
                new ConversationMember { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = req.SenderId, JoinedAt = DateTime.UtcNow },
                new ConversationMember { Id = Guid.NewGuid(), ConversationId = conv.Id, UserId = me, JoinedAt = DateTime.UtcNow });
            convId = conv.Id;
        }

        await _db.SaveChangesAsync();

        FriendRequestDto reqDto = MapRequestDto(req);

        // Notify both parties
        foreach (string connId in _presence.GetConnectionIds(req.SenderId))
            await _hub.Clients.Client(connId).SendAsync("FriendRequestAccepted", reqDto);
        foreach (string connId in _presence.GetConnectionIds(me))
            await _hub.Clients.Client(connId).SendAsync("FriendRequestAccepted", reqDto);

        // Notify new conversation
        await _hub.Clients.Group($"conv_{convId}").SendAsync("NewConversationCreated", new { ConversationId = convId });

        return Ok(ApiResponse.Ok("Đã chấp nhận lời mời kết bạn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/friends/requests/{id}/reject
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("requests/{id:guid}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectRequest(Guid id)
    {
        Guid me = CurrentUserId;
        FriendRequest? req = await _db.FriendRequests.FirstOrDefaultAsync(r => r.Id == id);

        if (req is null || req.ReceiverId != me)
            return NotFound(ApiResponse.Fail("Lời mời không tồn tại."));
        if (req.Status != FriendRequestStatus.Pending)
            return BadRequest(ApiResponse.Fail("Lời mời đã được xử lý."));

        req.Status = FriendRequestStatus.Rejected;
        req.RespondedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Notify sender
        foreach (string connId in _presence.GetConnectionIds(req.SenderId))
            await _hub.Clients.Client(connId).SendAsync("FriendRequestRejected", new { RequestId = id });

        return Ok(ApiResponse.Ok("Đã từ chối lời mời kết bạn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/friends/requests/{id} — Cancel outgoing request
    // ─────────────────────────────────────────────────────────────────────────
    [HttpDelete("requests/{id:guid}")]
    public async Task<ActionResult<ApiResponse>> CancelRequest(Guid id)
    {
        Guid me = CurrentUserId;
        FriendRequest? req = await _db.FriendRequests.FirstOrDefaultAsync(r => r.Id == id);

        if (req is null || req.SenderId != me)
            return NotFound(ApiResponse.Fail("Lời mời không tồn tại."));
        if (req.Status != FriendRequestStatus.Pending)
            return BadRequest(ApiResponse.Fail("Chỉ có thể hủy lời mời đang chờ xử lý."));

        req.Status = FriendRequestStatus.Canceled;
        req.RespondedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        foreach (string connId in _presence.GetConnectionIds(req.ReceiverId))
            await _hub.Clients.Client(connId).SendAsync("FriendRequestCanceled", new { RequestId = id });

        return Ok(ApiResponse.Ok("Đã hủy lời mời kết bạn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/friends/{friendUserId} — Unfriend
    // ─────────────────────────────────────────────────────────────────────────
    [HttpDelete("{friendUserId:guid}")]
    public async Task<ActionResult<ApiResponse>> Unfriend(Guid friendUserId)
    {
        Guid me = CurrentUserId;
        Friendship? fs = await _db.Friendships.FirstOrDefaultAsync(
            f => (f.User1Id == me && f.User2Id == friendUserId) ||
                 (f.User1Id == friendUserId && f.User2Id == me));

        if (fs is null)
            return NotFound(ApiResponse.Fail("Hai người không phải bạn bè."));

        _db.Friendships.Remove(fs);
        await _db.SaveChangesAsync();

        foreach (string connId in _presence.GetConnectionIds(friendUserId))
            await _hub.Clients.Client(connId).SendAsync("FriendRemoved", new { UserId = me });

        return Ok(ApiResponse.Ok("Đã xóa bạn bè."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static FriendRequestDto MapRequestDto(FriendRequest r) => new()
    {
        Id = r.Id,
        SenderId = r.SenderId,
        SenderUsername = r.Sender?.Username ?? string.Empty,
        SenderAvatarUrl = r.Sender?.AvatarUrl,
        ReceiverId = r.ReceiverId,
        ReceiverUsername = r.Receiver?.Username ?? string.Empty,
        ReceiverAvatarUrl = r.Receiver?.AvatarUrl,
        Status = r.Status,
        CreatedAt = r.CreatedAt
    };
}
