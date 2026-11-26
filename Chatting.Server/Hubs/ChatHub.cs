using Chatting.Server.Data;
using Chatting.Server.Models;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Chatting.Server.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Group calls use a LAN WebRTC mesh. This is deliberately in-memory: media
    // sessions are transient and a server restart should terminate them.
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, byte>> GroupCallParticipants = new();
    private readonly PresenceTracker _presence;
    private readonly ChatDbContext _db;

    public ChatHub(PresenceTracker presence, ChatDbContext db)
    {
        _presence = presence;
        _db = db;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Connection lifecycle
    // ─────────────────────────────────────────────────────────────────────────
    public override async Task OnConnectedAsync()
    {
        Guid userId = GetUserId();
        _presence.UserConnected(userId, Context.ConnectionId);

        // Join all conversation groups
        List<Guid> convIds = await _db.ConversationMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.ConversationId)
            .ToListAsync();

        foreach (Guid cid in convIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, ConvGroup(cid));

        // Update user status
        User? user = await _db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.Status = UserStatus.Online;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // Notify friends of online status
        await NotifyFriendsStatusAsync(userId, UserStatus.Online);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid userId = GetUserId();
        bool fullyOffline = _presence.UserDisconnected(userId, Context.ConnectionId);

        if (fullyOffline)
        {
            User? user = await _db.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = UserStatus.Offline;
                user.LastSeenAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            // Notify friends of offline status
            await NotifyFriendsStatusAsync(userId, UserStatus.Offline);

            // Handle any ringing calls that this user was involved in
            List<Guid> joinedGroupCallIds = GroupCallParticipants
                .Where(pair => pair.Value.ContainsKey(userId))
                .Select(pair => pair.Key)
                .ToList();
            List<Call> activeCalls = await _db.Calls
                .Where(c => (c.CallerId == userId || c.ReceiverId == userId || joinedGroupCallIds.Contains(c.Id))
                         && (c.Status == CallStatus.Ringing || c.Status == CallStatus.Ongoing))
                .ToListAsync();

            List<Call> endedCalls = [];
            foreach (Call call in activeCalls)
            {
                if (await IsGroupCallAsync(call))
                {
                    if (GroupCallParticipants.TryGetValue(call.Id, out var participants) && participants.TryRemove(userId, out _))
                    {
                        GroupCallParticipantDto left = await BuildGroupParticipantAsync(call.Id, userId);
                        await Clients.Group(ConvGroup(call.ConversationId)).SendAsync("GroupCallParticipantLeft", left);

                        if (!participants.IsEmpty)
                            continue;

                        GroupCallParticipants.TryRemove(call.Id, out _);
                    }
                    else
                    {
                        // The participant list may be lost after a server restart.
                        GroupCallParticipants.TryRemove(call.Id, out _);
                    }
                }

                call.Status = CallStatus.Disconnected;
                call.EndedAt = DateTime.UtcNow;
                if (call.ConnectedAt.HasValue)
                    call.DurationSeconds = (int)(call.EndedAt.Value - call.ConnectedAt.Value).TotalSeconds;
                endedCalls.Add(call);
            }
            if (endedCalls.Count > 0)
            {
                await _db.SaveChangesAsync();
                foreach (Call call in endedCalls)
                {
                    await Clients.Group(ConvGroup(call.ConversationId)).SendAsync("CallEnded",
                        new CallEndedDto { CallId = call.Id, FinalStatus = CallStatus.Disconnected, DurationSeconds = call.DurationSeconds });
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Explicitly join a conversation SignalR group.
    /// Useful for conversations opened or created after the user connected.
    /// </summary>
    public async Task JoinConversation(Guid conversationId)
    {
        Guid userId = GetUserId();
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (isMember)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ConvGroup(conversationId));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Messaging
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Client calls this to send a message in real-time.
    /// </summary>
    public async Task SendMessage(SendMessageDto dto)
    {
        Guid userId = GetUserId();

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == dto.ConversationId && cm.UserId == userId);
        if (!isMember) return;

        User? sender = await _db.Users.FindAsync(userId);
        if (sender is null) return;

        Message message = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = dto.ConversationId,
            SenderId = userId,
            Content = dto.Type == MessageType.Text ? dto.Content : null,
            Type = dto.Type,
            Status = MessageStatus.Sent,
            ReplyToMessageId = dto.ReplyToMessageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(message);

        // Link attachments
        if (dto.AttachmentIds.Count > 0)
        {
            List<Attachment> attachments = await _db.Attachments
                .Where(a => dto.AttachmentIds.Contains(a.Id) && a.UploadedByUserId == userId && a.MessageId == null)
                .ToListAsync();
            foreach (Attachment att in attachments)
                att.MessageId = message.Id;
        }

        // Update conversation timestamp
        Conversation? conv = await _db.Conversations.FindAsync(dto.ConversationId);
        if (conv is not null) conv.UpdatedAt = DateTime.UtcNow;

        // Unhide conversation for any member who had deleted/hidden it
        List<ConversationMember> hiddenMembers = await _db.ConversationMembers
            .Where(cm => cm.ConversationId == dto.ConversationId && cm.IsDeleted)
            .ToListAsync();
        foreach (var hm in hiddenMembers)
        {
            hm.IsDeleted = false;
        }

        await _db.SaveChangesAsync();

        // Build DTO for broadcast
        List<Attachment> savedAttachments = await _db.Attachments
            .Where(a => a.MessageId == message.Id).ToListAsync();

        MessageDto? replyDto = null;
        if (dto.ReplyToMessageId.HasValue)
        {
            Message? replyMsg = await _db.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == dto.ReplyToMessageId.Value);
            if (replyMsg is not null)
                replyDto = MapMessageDto(replyMsg, []);
        }

        MessageDto outDto = MapMessageDto(message, savedAttachments, replyDto, sender);

        await Clients.Group(ConvGroup(dto.ConversationId)).SendAsync("ReceiveMessage", outDto);
    }

    /// <summary>
    /// Broadcast typing indicator.
    /// </summary>
    public async Task SendTyping(Guid conversationId, bool isTyping)
    {
        Guid userId = GetUserId();
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (!isMember) return;

        await Clients.OthersInGroup(ConvGroup(conversationId)).SendAsync("UserTyping", new
        {
            ConversationId = conversationId,
            UserId = userId,
            IsTyping = isTyping
        });
    }

    /// <summary>
    /// Mark all messages in conversation as read for the calling user.
    /// </summary>
    public async Task MarkConversationRead(Guid conversationId)
    {
        Guid userId = GetUserId();

        ConversationMember? member = await _db.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (member is null) return;

        // Get the last message id
        Message? lastMsg = await _db.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted && !m.IsRevoked)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        member.LastReadAt = DateTime.UtcNow;
        member.LastReadMessageId = lastMsg?.Id;

        // Update unread messages from others to Seen
        List<Message> unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId
                     && m.SenderId != userId
                     && m.Status != MessageStatus.Seen
                     && !m.IsDeleted && !m.IsRevoked)
            .ToListAsync();

        foreach (Message m in unread)
            m.Status = MessageStatus.Seen;

        await _db.SaveChangesAsync();

        await Clients.Group(ConvGroup(conversationId)).SendAsync("MessagesRead", new
        {
            ConversationId = conversationId,
            ReadByUserId = userId,
            ReadAt = member.LastReadAt
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Call Signaling
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Caller initiates a call.
    /// </summary>
    public async Task StartCall(StartCallRequestDto dto)
    {
        Guid callerId = GetUserId();
        User? caller = await _db.Users.FindAsync(callerId);
        if (caller is null) return;

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == dto.ConversationId && cm.UserId == callerId);
        if (!isMember) return;

        Conversation? conversation = await _db.Conversations.FindAsync(dto.ConversationId);
        bool isGroupCall = conversation?.Type == ConversationType.Group && !dto.ReceiverId.HasValue;

        Guid? receiverId = dto.ReceiverId;
        if (!isGroupCall && !receiverId.HasValue)
        {
            receiverId = await _db.ConversationMembers
                .Where(cm => cm.ConversationId == dto.ConversationId && cm.UserId != callerId)
                .Select(cm => (Guid?)cm.UserId)
                .FirstOrDefaultAsync();
        }

        Call call = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = dto.ConversationId,
            CallerId = callerId,
            ReceiverId = receiverId,
            CallType = dto.CallType,
            Status = CallStatus.Ringing,
            StartedAt = DateTime.UtcNow
        };
        _db.Calls.Add(call);
        await _db.SaveChangesAsync();

        if (isGroupCall)
            GroupCallParticipants.GetOrAdd(call.Id, _ => new())[callerId] = 0;

        CallInfoDto callInfo = await BuildCallInfoDto(call);

        // Notify specific receiver or whole group
        if (receiverId.HasValue)
        {
            foreach (string connId in _presence.GetConnectionIds(receiverId.Value))
                await Clients.Client(connId).SendAsync("IncomingCall", callInfo);
        }
        else
        {
            await Clients.OthersInGroup(ConvGroup(dto.ConversationId)).SendAsync("IncomingCall", callInfo);
        }

        // Confirm to caller
        await Clients.Caller.SendAsync("CallStarted", callInfo);
    }

    /// <summary>
    /// Receiver accepts the call.
    /// </summary>
    public async Task<bool> AcceptCall(Guid callId)
    {
        Guid userId = GetUserId();
        Call? call = await _db.Calls.FindAsync(callId);
        if (call is null) return false;
        bool isGroupCall = await IsGroupCallAsync(call);
        if (!isGroupCall && call.Status != CallStatus.Ringing) return false;
        if (isGroupCall && call.Status is not (CallStatus.Ringing or CallStatus.Ongoing)) return false;
        if (isGroupCall)
        {
            bool isMember = await _db.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == call.ConversationId && cm.UserId == userId);
            if (!isMember) return false;

            var participants = GroupCallParticipants.GetOrAdd(call.Id, _ => new());
            participants.TryAdd(userId, 0);
            call.Status = CallStatus.Ongoing;
            call.ConnectedAt ??= DateTime.UtcNow;
            await _db.SaveChangesAsync();

            List<GroupCallParticipantDto> roster = await BuildGroupParticipantsAsync(call.Id, participants.Keys.Where(id => id != userId));
            foreach (string connId in _presence.GetConnectionIds(userId))
                await Clients.Client(connId).SendAsync("GroupCallParticipants", roster);

            GroupCallParticipantDto joined = await BuildGroupParticipantAsync(call.Id, userId);
            await Clients.Group(ConvGroup(call.ConversationId)).SendAsync("GroupCallParticipantJoined", joined);
            return true;
        }

        call.Status = CallStatus.Ongoing;
        call.ConnectedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        CallInfoDto info = await BuildCallInfoDto(call);
        HashSet<string> targetConns = new();
        foreach (string connId in _presence.GetConnectionIds(call.CallerId))
            targetConns.Add(connId);
        if (call.ReceiverId.HasValue)
        {
            foreach (string connId in _presence.GetConnectionIds(call.ReceiverId.Value))
                targetConns.Add(connId);
        }

        foreach (string connId in targetConns)
            await Clients.Client(connId).SendAsync("CallAccepted", info);
        return true;
    }

    /// <summary>
    /// Receiver rejects the call.
    /// </summary>
    public async Task RejectCall(Guid callId)
    {
        Guid userId = GetUserId();
        Call? call = await _db.Calls.FindAsync(callId);
        if (call is null) return;
        if (await IsGroupCallAsync(call)) return;

        call.Status = CallStatus.Rejected;
        call.EndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        CallEndedDto endedDto = new() { CallId = callId, FinalStatus = CallStatus.Rejected };
        HashSet<string> targetConns = new();
        foreach (string connId in _presence.GetConnectionIds(call.CallerId))
            targetConns.Add(connId);
        if (call.ReceiverId.HasValue)
        {
            foreach (string connId in _presence.GetConnectionIds(call.ReceiverId.Value))
                targetConns.Add(connId);
        }

        foreach (string connId in targetConns)
            await Clients.Client(connId).SendAsync("CallEnded", endedDto);
    }

    /// <summary>
    /// Either party ends the call.
    /// </summary>
    public async Task EndCall(Guid callId)
    {
        Guid userId = GetUserId();
        Call? call = await _db.Calls.FindAsync(callId);
        if (call is null) return;

        if (await IsGroupCallAsync(call))
        {
            if (!GroupCallParticipants.TryGetValue(callId, out var participants) || !participants.TryRemove(userId, out _))
                return;

            GroupCallParticipantDto left = await BuildGroupParticipantAsync(callId, userId);
            await Clients.Group(ConvGroup(call.ConversationId)).SendAsync("GroupCallParticipantLeft", left);

            if (!participants.IsEmpty) return;

            GroupCallParticipants.TryRemove(callId, out _);
            call.Status = CallStatus.Ended;
            call.EndedAt = DateTime.UtcNow;
            if (call.ConnectedAt.HasValue)
                call.DurationSeconds = (int)(call.EndedAt.Value - call.ConnectedAt.Value).TotalSeconds;
            await _db.SaveChangesAsync();
            await Clients.Group(ConvGroup(call.ConversationId)).SendAsync("CallEnded",
                new CallEndedDto { CallId = call.Id, FinalStatus = call.Status, DurationSeconds = call.DurationSeconds });
            return;
        }

        // If ended while still ringing (unanswered/cancelled/timeout), mark as Missed
        if (call.Status == CallStatus.Ringing)
        {
            call.Status = CallStatus.Missed;
        }
        else
        {
            call.Status = CallStatus.Ended;
        }

        call.EndedAt = DateTime.UtcNow;
        if (call.ConnectedAt.HasValue)
            call.DurationSeconds = (int)(call.EndedAt.Value - call.ConnectedAt.Value).TotalSeconds;

        await _db.SaveChangesAsync();

        CallEndedDto endedDto = new()
        {
            CallId = callId,
            FinalStatus = call.Status,
            DurationSeconds = call.DurationSeconds
        };
        HashSet<string> targetConns = new();
        foreach (string connId in _presence.GetConnectionIds(call.CallerId))
            targetConns.Add(connId);
        if (call.ReceiverId.HasValue)
        {
            foreach (string connId in _presence.GetConnectionIds(call.ReceiverId.Value))
                targetConns.Add(connId);
        }

        foreach (string connId in targetConns)
            await Clients.Client(connId).SendAsync("CallEnded", endedDto);
    }

    /// <summary>
    /// Relay WebRTC signaling payload (peer-ready/offer/answer/ice-candidate/media-state)
    /// to every active connection belonging to the other call participant.
    /// </summary>
    public async Task SendWebRtcSignal(WebRtcSignalDto dto)
    {
        Guid senderId = GetUserId();
        Call? call = await _db.Calls.FindAsync(dto.CallId);
        if (call is null) return;

        if (await IsGroupCallAsync(call))
        {
            if (!GroupCallParticipants.TryGetValue(call.Id, out var groupParticipants) ||
                !groupParticipants.ContainsKey(senderId) ||
                !groupParticipants.ContainsKey(dto.TargetUserId) ||
                dto.TargetUserId == senderId)
                return;

            IReadOnlyCollection<string> groupTargetConnections = _presence.GetConnectionIds(dto.TargetUserId);
            if (groupTargetConnections.Count == 0) return;

            await Clients.Clients(groupTargetConnections).SendAsync("ReceiveWebRtcSignal", new WebRtcSignalDto
            {
                CallId = dto.CallId,
                SenderUserId = senderId,
                TargetUserId = dto.TargetUserId,
                SignalType = dto.SignalType,
                Payload = dto.Payload
            });
            return;
        }

        // Do not trust a client-supplied routing target. This keeps each SDP/ICE
        // packet on the exact one-to-one call and prevents a stale conversation
        // group membership from receiving a duplicate signal.
        Guid targetUserId;
        if (senderId == call.CallerId)
        {
            targetUserId = call.ReceiverId ?? Guid.Empty;
        }
        else if (call.ReceiverId.HasValue && senderId == call.ReceiverId.Value)
        {
            targetUserId = call.CallerId;
        }
        else
        {
            return;
        }

        if (targetUserId == Guid.Empty) return;

        IReadOnlyCollection<string> targetConns = _presence.GetConnectionIds(targetUserId);
        if (targetConns.Count == 0) return;

        await Clients.Clients(targetConns).SendAsync("ReceiveWebRtcSignal", new WebRtcSignalDto
        {
            CallId = dto.CallId,
            SenderUserId = senderId,
            TargetUserId = targetUserId,
            SignalType = dto.SignalType,
            Payload = dto.Payload
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        string? sub = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                   ?? Context.User?.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out Guid id) ? id : Guid.Empty;
    }

    private static string ConvGroup(Guid conversationId) => $"conv_{conversationId}";

    private async Task<bool> IsGroupCallAsync(Call call) =>
        call.ReceiverId is null && await _db.Conversations
            .AnyAsync(c => c.Id == call.ConversationId && c.Type == ConversationType.Group);

    private async Task<GroupCallParticipantDto> BuildGroupParticipantAsync(Guid callId, Guid userId)
    {
        User? user = await _db.Users.FindAsync(userId);
        return new GroupCallParticipantDto
        {
            CallId = callId,
            UserId = userId,
            Username = user?.Username ?? "Người dùng",
            DisplayName = user?.DisplayName,
            AvatarUrl = user?.AvatarUrl
        };
    }

    private async Task<List<GroupCallParticipantDto>> BuildGroupParticipantsAsync(Guid callId, IEnumerable<Guid> userIds)
    {
        List<Guid> ids = userIds.Distinct().ToList();
        List<User> users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        return ids.Select(id =>
        {
            User? user = users.FirstOrDefault(u => u.Id == id);
            return new GroupCallParticipantDto
            {
                CallId = callId,
                UserId = id,
                Username = user?.Username ?? "Người dùng",
                DisplayName = user?.DisplayName,
                AvatarUrl = user?.AvatarUrl
            };
        }).ToList();
    }

    private static MessageDto MapMessageDto(
        Message m,
        IEnumerable<Attachment> attachments,
        MessageDto? replyTo = null,
        User? senderOverride = null)
    {
        return new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderUsername = senderOverride?.Username ?? m.Sender?.Username ?? string.Empty,
            SenderAvatarUrl = senderOverride?.AvatarUrl ?? m.Sender?.AvatarUrl,
            Content = m.IsRevoked ? null : m.Content,
            Type = m.Type,
            Status = m.Status,
            IsEdited = m.IsEdited,
            IsRevoked = m.IsRevoked,
            IsDeleted = m.IsDeleted,
            IsPinned = m.IsPinned,
            PinnedByUserId = m.PinnedByUserId,
            PinnedAt = m.PinnedAt,
            ForwardedFromUsername = m.ForwardedFromUsername,
            ReplyToMessageId = m.ReplyToMessageId,
            ReplyToMessage = replyTo,
            Attachments = attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                OriginalFileName = a.OriginalFileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                RelativePath = a.RelativePath,
                DurationSeconds = a.DurationSeconds
            }).ToList(),
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        };
    }

    private async Task<CallInfoDto> BuildCallInfoDto(Call call)
    {
        User? caller = await _db.Users.FindAsync(call.CallerId);
        User? receiver = call.ReceiverId.HasValue ? await _db.Users.FindAsync(call.ReceiverId.Value) : null;
        return new CallInfoDto
        {
            CallId = call.Id,
            ConversationId = call.ConversationId,
            CallerId = call.CallerId,
            CallerUsername = caller?.Username ?? string.Empty,
            CallerDisplayName = caller?.DisplayName,
            CallerAvatarUrl = caller?.AvatarUrl,
            ReceiverId = call.ReceiverId,
            ReceiverUsername = receiver?.Username,
            ReceiverDisplayName = receiver?.DisplayName,
            ReceiverAvatarUrl = receiver?.AvatarUrl,
            CallType = call.CallType,
            Status = call.Status,
            StartedAt = call.StartedAt,
            ConnectedAt = call.ConnectedAt,
            EndedAt = call.EndedAt,
            DurationSeconds = call.DurationSeconds
        };
    }

    private async Task NotifyFriendsStatusAsync(Guid userId, UserStatus status)
    {
        List<Guid> friendIds = await _db.Friendships
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync();

        foreach (Guid friendId in friendIds)
        {
            foreach (string connId in _presence.GetConnectionIds(friendId))
            {
                await Clients.Client(connId).SendAsync("FriendStatusChanged", new
                {
                    UserId = userId,
                    Status = status.ToString()
                });
            }
        }
    }
}
