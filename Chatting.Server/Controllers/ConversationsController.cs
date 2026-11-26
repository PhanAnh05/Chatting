using Chatting.Server.Data;
using Chatting.Server.Hubs;
using Chatting.Server.Models;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly IHubContext<ChatHub> _hub;
    private readonly PresenceTracker _presence;

    public ConversationsController(ChatDbContext db, IHubContext<ChatHub> hub, PresenceTracker presence)
    {
        _db = db;
        _hub = hub;
        _presence = presence;
    }

    private Guid Me =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/conversations
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ConversationDto>>>> GetConversations()
    {
        Guid me = Me;

        List<Guid> convIds = await _db.ConversationMembers
            .Where(cm => cm.UserId == me)
            .Select(cm => cm.ConversationId)
            .ToListAsync();

        List<Conversation> conversations = await _db.Conversations
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Where(c => convIds.Contains(c.Id))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        List<ConversationDto> result = [];

        foreach (Conversation c in conversations)
        {
            ConversationMember? myMember = c.Members.FirstOrDefault(m => m.UserId == me);
            if (myMember is null) continue;

            // Last message visible to 'me'
            Message? last = await _db.Messages
                .Where(m => m.ConversationId == c.Id && !m.IsDeleted
                         && !_db.MessageHiddenForUsers.Any(h => h.MessageId == m.Id && h.UserId == me)
                         && (myMember.ClearedAt == null || m.CreatedAt > myMember.ClearedAt.Value))
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            // If 'me' marked conversation as deleted and no new message arrived after ClearedAt:
            if (myMember.IsDeleted && last == null)
            {
                continue; // Hide from conversation list for 'me'
            }

            // Unread count
            DateTime? myLastRead = myMember.LastReadAt;
            int unread = await _db.Messages
                .CountAsync(m => m.ConversationId == c.Id
                              && m.SenderId != me
                              && !m.IsDeleted
                              && !_db.MessageHiddenForUsers.Any(h => h.MessageId == m.Id && h.UserId == me)
                              && (myMember.ClearedAt == null || m.CreatedAt > myMember.ClearedAt.Value)
                              && (myLastRead == null || m.CreatedAt > myLastRead));

            // Build title/avatar for Direct
            string title = c.Title ?? string.Empty;
            string? avatar = c.AvatarUrl;
            if (c.Type == ConversationType.Direct)
            {
                ConversationMember? other = c.Members.FirstOrDefault(m => m.UserId != me);
                if (other is not null)
                {
                    title = string.IsNullOrWhiteSpace(other.Nickname)
                        ? (string.IsNullOrWhiteSpace(other.User.DisplayName) ? other.User.Username : other.User.DisplayName)
                        : other.Nickname;
                    avatar = other.User.AvatarUrl;
                }
            }

            result.Add(new ConversationDto
            {
                Id = c.Id,
                Type = c.Type,
                Title = title,
                AvatarUrl = avatar,
                UpdatedAt = c.UpdatedAt,
                ChatGroupId = c.ChatGroupId,
                UnreadCount = unread,
                LastMessage = last is null ? null : MapMsgDto(last, null, []),
                Members = c.Members.Select(m => new ConversationMemberDto
                {
                    UserId = m.UserId,
                    Username = m.User.Username,
                    Nickname = m.Nickname,
                    ProfileDisplayName = m.User.DisplayName,
                    Bio = m.User.Bio,
                    AvatarUrl = m.User.AvatarUrl,
                    Status = m.User.Status,
                    IsOnline = _presence.IsOnline(m.UserId),
                    JoinedAt = m.JoinedAt,
                    LastReadAt = m.LastReadAt
                }).ToList()
            });
        }

        return Ok(ApiResponse<List<ConversationDto>>.Ok(result));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/conversations/{id} — Get single conversation details
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> GetConversation(Guid id)
    {
        Guid me = Me;

        Conversation? c = await _db.Conversations
            .Include(c => c.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == id && c.Members.Any(m => m.UserId == me));

        if (c is null)
            return NotFound(ApiResponse<ConversationDto>.Fail("Không tìm thấy cuộc trò chuyện."));

        ConversationMember? myMember = c.Members.FirstOrDefault(m => m.UserId == me);
        if (myMember is not null && myMember.IsDeleted)
        {
            myMember.IsDeleted = false;
            await _db.SaveChangesAsync();
        }

        Message? last = await _db.Messages
            .Where(m => m.ConversationId == c.Id && !m.IsDeleted
                     && !_db.MessageHiddenForUsers.Any(h => h.MessageId == m.Id && h.UserId == me)
                     && (myMember == null || myMember.ClearedAt == null || m.CreatedAt > myMember.ClearedAt.Value))
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        string title = c.Title ?? string.Empty;
        string? avatar = c.AvatarUrl;
        if (c.Type == ConversationType.Direct)
        {
            ConversationMember? other = c.Members.FirstOrDefault(m => m.UserId != me);
            if (other is not null)
            {
                title = string.IsNullOrWhiteSpace(other.Nickname) ? other.User.Username : other.Nickname;
                avatar = other.User.AvatarUrl;
            }
        }

        return Ok(ApiResponse<ConversationDto>.Ok(new ConversationDto
        {
            Id = c.Id,
            Type = c.Type,
            Title = title,
            AvatarUrl = avatar,
            UpdatedAt = c.UpdatedAt,
            ChatGroupId = c.ChatGroupId,
            UnreadCount = 0,
            LastMessage = last is null ? null : MapMsgDto(last, null, []),
            Members = c.Members.Select(m => new ConversationMemberDto
            {
                UserId = m.UserId,
                Username = m.User.Username,
                Nickname = m.Nickname,
                ProfileDisplayName = m.User.DisplayName,
                Bio = m.User.Bio,
                AvatarUrl = m.User.AvatarUrl,
                Status = m.User.Status,
                IsOnline = _presence.IsOnline(m.UserId),
                JoinedAt = m.JoinedAt,
                LastReadAt = m.LastReadAt
            }).ToList()
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/conversations/{id}/nickname
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPut("{id:guid}/nickname")]
    public async Task<ActionResult<ApiResponse>> SetNickname(Guid id, [FromBody] SetNicknameRequestDto dto)
    {
        Guid me = Me;
        ConversationMember? actorMember = await _db.ConversationMembers
            .Include(cm => cm.User)
            .FirstOrDefaultAsync(cm => cm.ConversationId == id && cm.UserId == me);
        if (actorMember is null) return Forbid();

        ConversationMember? targetMember = await _db.ConversationMembers
            .Include(cm => cm.User)
            .FirstOrDefaultAsync(cm => cm.ConversationId == id && cm.UserId == dto.TargetUserId);
        if (targetMember is null) return BadRequest(ApiResponse.Fail("Người dùng không thuộc cuộc trò chuyện này."));

        string? nickname = string.IsNullOrWhiteSpace(dto.Nickname) ? null : dto.Nickname.Trim();
        if (nickname?.Length > 100) return BadRequest(ApiResponse.Fail("Biệt danh tối đa 100 ký tự."));
        targetMember.Nickname = nickname;

        string targetLabel = targetMember.UserId == me ? "chính mình" : targetMember.User.Username;
        string content = nickname is null
            ? $"{actorMember.User.Username} đã xóa biệt danh của {targetLabel}"
            : $"{actorMember.User.Username} đã đặt biệt danh cho {targetLabel} là {nickname}";
        Message systemMessage = new()
        {
            Id = Guid.NewGuid(), ConversationId = id, SenderId = me, Content = content,
            Type = MessageType.System, Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(systemMessage);
        Conversation? conversation = await _db.Conversations.FindAsync(id);
        if (conversation is not null) conversation.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        MessageDto messageDto = MapMsgDto(systemMessage, null, [], actorMember.User);
        await _hub.Clients.Group($"conv_{id}").SendAsync("NicknameUpdated", id, dto.TargetUserId, nickname, messageDto);
        return Ok(ApiResponse.Ok("Đã cập nhật biệt danh."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/conversations/{id} — Clear conversation history for current user
    // ─────────────────────────────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteConversation(Guid id)
    {
        Guid me = Me;

        ConversationMember? member = await _db.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == id && cm.UserId == me);

        if (member is null)
            return NotFound(ApiResponse.Fail("Không tìm thấy cuộc trò chuyện."));

        member.ClearedAt = DateTime.UtcNow;
        member.IsDeleted = true;
        member.LastReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Đã xóa cuộc trò chuyện khỏi danh sách của bạn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/conversations/{id}/messages?pageIndex=1&pageSize=30&keyword=
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<PagedResult<MessageDto>>>> GetMessages(
        Guid id,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 30,
        [FromQuery] string? keyword = null)
    {
        Guid me = Me;
        ConversationMember? myMember = await _db.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == id && cm.UserId == me);
        if (myMember is null)
            return Forbid();

        IQueryable<Message> query = _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Include(m => m.ReplyToMessage).ThenInclude(r => r!.Sender)
            .Where(m => m.ConversationId == id
                     && !_db.MessageHiddenForUsers.Any(h => h.MessageId == m.Id && h.UserId == me));

        // Filter out messages prior to ClearedAt for current user
        if (myMember.ClearedAt.HasValue)
        {
            query = query.Where(m => m.CreatedAt > myMember.ClearedAt.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(m => m.Type != MessageType.System
                                  && m.Content != null
                                  && m.Content.Contains(keyword));

        int total = await query.CountAsync();

        List<Message> messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        messages.Reverse(); // oldest-first for display

        List<Guid> messageIds = messages.Select(m => m.Id).ToList();
        List<MessageReaction> reactions = messageIds.Count == 0
            ? []
            : await _db.MessageReactions.Where(r => messageIds.Contains(r.MessageId)).ToListAsync();

        List<MessageDto> dtos = messages.Select(m =>
        {
            MessageDto? replyDto = m.ReplyToMessage is null ? null : MapMsgDto(m.ReplyToMessage, null, []);
            MessageDto dto = MapMsgDto(m, replyDto, m.Attachments);
            dto.Reactions = BuildReactionSummary(reactions.Where(r => r.MessageId == m.Id), me);
            return dto;
        }).ToList();

        return Ok(ApiResponse<PagedResult<MessageDto>>.Ok(new PagedResult<MessageDto>
        {
            Items = dtos,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/conversations/messages/{id}/pin — Pin or unpin a message
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPut("messages/{id:guid}/pin")]
    public async Task<ActionResult<ApiResponse<MessagePinChangedDto>>> SetMessagePinned(
        Guid id,
        [FromBody] SetMessagePinDto dto)
    {
        Guid me = Me;
        Message? message = await _db.Messages.FirstOrDefaultAsync(m => m.Id == id);
        if (message is null) return NotFound(ApiResponse<MessagePinChangedDto>.Fail("Không tìm thấy tin nhắn."));

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == message.ConversationId && cm.UserId == me);
        if (!isMember) return Forbid();
        if (message.Type == MessageType.System || message.IsRevoked || message.IsDeleted)
            return BadRequest(ApiResponse<MessagePinChangedDto>.Fail("Không thể ghim tin nhắn này."));

        DateTime changedAt = DateTime.UtcNow;
        message.IsPinned = dto.IsPinned;
        message.PinnedByUserId = dto.IsPinned ? me : null;
        message.PinnedAt = dto.IsPinned ? changedAt : null;
        await _db.SaveChangesAsync();

        MessagePinChangedDto update = new()
        {
            MessageId = message.Id,
            ConversationId = message.ConversationId,
            IsPinned = message.IsPinned,
            PinnedByUserId = message.PinnedByUserId,
            PinnedAt = message.PinnedAt,
            ChangedAt = changedAt
        };
        await _hub.Clients.Group($"conv_{message.ConversationId}").SendAsync("MessagePinUpdated", update);
        return Ok(ApiResponse<MessagePinChangedDto>.Ok(update,
            dto.IsPinned ? "Đã ghim tin nhắn." : "Đã bỏ ghim tin nhắn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/conversations/messages/{id}/forward — Forward to conversations
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("messages/{id:guid}/forward")]
    public async Task<ActionResult<ApiResponse<List<MessageDto>>>> ForwardMessage(
        Guid id,
        [FromBody] ForwardMessageDto dto)
    {
        Guid me = Me;
        List<Guid> targetIds = dto.TargetConversationIds?.Distinct().Where(targetId => targetId != Guid.Empty).ToList() ?? [];
        if (targetIds.Count == 0)
            return BadRequest(ApiResponse<List<MessageDto>>.Fail("Hãy chọn cuộc trò chuyện muốn chuyển tiếp."));
        if (targetIds.Count > 20)
            return BadRequest(ApiResponse<List<MessageDto>>.Fail("Mỗi lần chỉ có thể chuyển tiếp đến tối đa 20 cuộc trò chuyện."));

        Message? source = await _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (source is null) return NotFound(ApiResponse<List<MessageDto>>.Fail("Không tìm thấy tin nhắn."));

        bool canReadSource = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == source.ConversationId && cm.UserId == me);
        if (!canReadSource) return Forbid();
        if (source.Type == MessageType.System || source.IsRevoked || source.IsDeleted)
            return BadRequest(ApiResponse<List<MessageDto>>.Fail("Không thể chuyển tiếp tin nhắn này."));

        List<Guid> accessibleTargets = await _db.ConversationMembers
            .Where(cm => targetIds.Contains(cm.ConversationId) && cm.UserId == me)
            .Select(cm => cm.ConversationId)
            .Distinct()
            .ToListAsync();
        if (accessibleTargets.Count != targetIds.Count) return Forbid();

        List<Conversation> conversations = await _db.Conversations
            .Where(c => targetIds.Contains(c.Id))
            .ToListAsync();
        if (conversations.Count != targetIds.Count)
            return BadRequest(ApiResponse<List<MessageDto>>.Fail("Một số cuộc trò chuyện không còn tồn tại."));

        User? forwarder = await _db.Users.FindAsync(me);
        if (forwarder is null) return Unauthorized();

        string originalSenderName = source.ForwardedFromUsername
            ?? (string.IsNullOrWhiteSpace(source.Sender.DisplayName) ? source.Sender.Username : source.Sender.DisplayName);
        DateTime now = DateTime.UtcNow;
        List<(Message Message, List<Attachment> Attachments)> forwardedMessages = [];
        foreach (Conversation conversation in conversations)
        {
            Message forwarded = new()
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderId = me,
                Content = source.Content,
                Type = source.Type,
                Status = MessageStatus.Sent,
                ForwardedFromUsername = originalSenderName,
                CreatedAt = now,
                UpdatedAt = now
            };
            List<Attachment> copiedAttachments = source.Attachments.Select(attachment => new Attachment
            {
                Id = Guid.NewGuid(),
                MessageId = forwarded.Id,
                UploadedByUserId = me,
                OriginalFileName = attachment.OriginalFileName,
                StoredFileName = attachment.StoredFileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                RelativePath = attachment.RelativePath,
                DurationSeconds = attachment.DurationSeconds,
                CreatedAt = now
            }).ToList();

            _db.Messages.Add(forwarded);
            _db.Attachments.AddRange(copiedAttachments);
            conversation.UpdatedAt = now;
            forwardedMessages.Add((forwarded, copiedAttachments));
        }

        List<ConversationMember> hiddenMembers = await _db.ConversationMembers
            .Where(cm => targetIds.Contains(cm.ConversationId) && cm.IsDeleted)
            .ToListAsync();
        foreach (ConversationMember member in hiddenMembers)
            member.IsDeleted = false;

        await _db.SaveChangesAsync();

        List<MessageDto> results = forwardedMessages
            .Select(item => MapMsgDto(item.Message, null, item.Attachments, forwarder))
            .ToList();
        foreach (MessageDto message in results)
            await _hub.Clients.Group($"conv_{message.ConversationId}").SendAsync("ReceiveMessage", message);

        return Ok(ApiResponse<List<MessageDto>>.Ok(results,
            $"Đã chuyển tiếp đến {results.Count} cuộc trò chuyện."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/conversations/messages/{id}/reactions — Toggle a reaction
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("messages/{id:guid}/reactions")]
    public async Task<ActionResult<ApiResponse<MessageReactionsChangedDto>>> ToggleMessageReaction(
        Guid id,
        [FromBody] SetMessageReactionDto dto)
    {
        Guid me = Me;
        string emoji = dto.Emoji?.Trim() ?? string.Empty;
        string[] allowedEmojis = ["👍", "❤️", "😂", "😮", "😢", "🎉"];
        if (!allowedEmojis.Contains(emoji, StringComparer.Ordinal))
            return BadRequest(ApiResponse<MessageReactionsChangedDto>.Fail("Cảm xúc không được hỗ trợ."));

        Message? message = await _db.Messages.FindAsync(id);
        if (message is null || message.IsDeleted || message.IsRevoked)
            return NotFound(ApiResponse<MessageReactionsChangedDto>.Fail("Tin nhắn không tồn tại."));
        if (message.Type == MessageType.System)
            return BadRequest(ApiResponse<MessageReactionsChangedDto>.Fail("Không thể thả cảm xúc vào thông báo hệ thống."));

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == message.ConversationId && cm.UserId == me);
        if (!isMember) return Forbid();

        bool isHiddenForMe = await _db.MessageHiddenForUsers
            .AnyAsync(h => h.MessageId == id && h.UserId == me);
        if (isHiddenForMe)
            return NotFound(ApiResponse<MessageReactionsChangedDto>.Fail("Tin nhắn không tồn tại."));

        MessageReaction? current = await _db.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == id && r.UserId == me && r.Emoji == emoji);
        if (current is null)
        {
            _db.MessageReactions.Add(new MessageReaction
            {
                MessageId = id,
                UserId = me,
                Emoji = emoji,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            _db.MessageReactions.Remove(current);
        }

        await _db.SaveChangesAsync();

        List<MessageReaction> reactions = await _db.MessageReactions
            .Where(r => r.MessageId == id)
            .ToListAsync();
        MessageReactionsChangedDto update = new()
        {
            MessageId = id,
            ConversationId = message.ConversationId,
            Reactions = BuildReactionSummary(reactions, me)
        };

        await _hub.Clients.Group($"conv_{message.ConversationId}")
            .SendAsync("MessageReactionsUpdated", update);

        return Ok(ApiResponse<MessageReactionsChangedDto>.Ok(update));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/conversations/{id}/shared-content — Get shared media, files, links, voice
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{id:guid}/shared-content")]
    public async Task<ActionResult<ApiResponse<SharedContentDto>>> GetSharedContent(Guid id)
    {
        Guid me = Me;
        ConversationMember? myMember = await _db.ConversationMembers
            .FirstOrDefaultAsync(cm => cm.ConversationId == id && cm.UserId == me);
        if (myMember is null)
            return Forbid();

        IQueryable<Message> query = _db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Where(m => m.ConversationId == id && !m.IsDeleted && !m.IsRevoked
                     && !_db.MessageHiddenForUsers.Any(h => h.MessageId == m.Id && h.UserId == me));

        if (myMember.ClearedAt.HasValue)
        {
            query = query.Where(m => m.CreatedAt > myMember.ClearedAt.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var result = new SharedContentDto();
        var linkRegex = new Regex(@"(https?://[^\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (var m in messages)
        {
            // 1. Attachments & Media / Files / Voice
            if (m.Attachments.Count > 0)
            {
                foreach (var att in m.Attachments)
                {
                    string ext = Path.GetExtension(att.OriginalFileName).ToLowerInvariant();
                    string ct = (att.ContentType ?? string.Empty).ToLowerInvariant();

                    bool isImage = m.Type == MessageType.Image ||
                                   ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" ||
                                   ct.StartsWith("image/");

                    bool isVideo = m.Type == MessageType.Video ||
                                   ext is ".mp4" or ".mov" or ".avi" or ".mkv" or ".webm" ||
                                   ct.StartsWith("video/");

                    bool isVoice = m.Type == MessageType.Voice ||
                                   ext is ".wav" or ".ogg" or ".m4a" ||
                                   ct.StartsWith("audio/");

                    if (isImage || isVideo)
                    {
                        result.Media.Add(new SharedMediaItemDto
                        {
                            AttachmentId = att.Id,
                            MessageId = m.Id,
                            FileName = att.OriginalFileName,
                            FileSize = att.FileSize,
                            ContentType = att.ContentType ?? string.Empty,
                            RelativePath = att.RelativePath,
                            IsVideo = isVideo,
                            SenderId = m.SenderId,
                            SenderUsername = m.Sender?.Username ?? string.Empty,
                            CreatedAt = m.CreatedAt
                        });
                    }
                    else if (isVoice)
                    {
                        result.VoiceMessages.Add(new SharedVoiceItemDto
                        {
                            AttachmentId = att.Id,
                            MessageId = m.Id,
                            FileName = att.OriginalFileName,
                            DurationSeconds = att.DurationSeconds ?? 0,
                            SenderId = m.SenderId,
                            SenderUsername = m.Sender?.Username ?? string.Empty,
                            CreatedAt = m.CreatedAt
                        });
                    }
                    else
                    {
                        // File / Document
                        result.Files.Add(new SharedFileItemDto
                        {
                            AttachmentId = att.Id,
                            MessageId = m.Id,
                            FileName = att.OriginalFileName,
                            FileSize = att.FileSize,
                            ContentType = att.ContentType ?? string.Empty,
                            RelativePath = att.RelativePath,
                            SenderId = m.SenderId,
                            SenderUsername = m.Sender?.Username ?? string.Empty,
                            CreatedAt = m.CreatedAt
                        });
                    }
                }
            }

            // 2. Links extracted from message content
            if (!string.IsNullOrWhiteSpace(m.Content))
            {
                var matches = linkRegex.Matches(m.Content);
                foreach (Match match in matches)
                {
                    string url = match.Value.TrimEnd('.', ',', ';', '!', '?', ')');
                    if (Uri.TryCreate(url, UriKind.Absolute, out _))
                    {
                        result.Links.Add(new SharedLinkItemDto
                        {
                            MessageId = m.Id,
                            Url = url,
                            SenderId = m.SenderId,
                            SenderUsername = m.Sender?.Username ?? string.Empty,
                            CreatedAt = m.CreatedAt
                        });
                    }
                }
            }
        }

        return Ok(ApiResponse<SharedContentDto>.Ok(result));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/conversations/messages — Send via REST
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("messages")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] SendMessageDto dto)
    {
        Guid me = Me;
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == dto.ConversationId && cm.UserId == me);
        if (!isMember) return Forbid();

        User? sender = await _db.Users.FindAsync(me);
        if (sender is null) return Unauthorized();

        Message message = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = dto.ConversationId,
            SenderId = me,
            Content = dto.Type == MessageType.Text ? dto.Content : null,
            Type = dto.Type,
            Status = MessageStatus.Sent,
            ReplyToMessageId = dto.ReplyToMessageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(message);

        if (dto.AttachmentIds.Count > 0)
        {
            List<Attachment> attachments = await _db.Attachments
                .Where(a => dto.AttachmentIds.Contains(a.Id) && a.UploadedByUserId == me && a.MessageId == null)
                .ToListAsync();
            foreach (Attachment a in attachments)
                a.MessageId = message.Id;
        }

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

        List<Attachment> saved = await _db.Attachments.Where(a => a.MessageId == message.Id).ToListAsync();
        MessageDto? replyDto = null;
        if (dto.ReplyToMessageId.HasValue)
        {
            Message? rm = await _db.Messages.Include(m => m.Sender).FirstOrDefaultAsync(m => m.Id == dto.ReplyToMessageId.Value);
            if (rm is not null) replyDto = MapMsgDto(rm, null, []);
        }

        MessageDto outDto = MapMsgDto(message, replyDto, saved, sender);
        await _hub.Clients.Group($"conv_{dto.ConversationId}").SendAsync("ReceiveMessage", outDto);

        return Ok(ApiResponse<MessageDto>.Ok(outDto));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/conversations/messages/{id} — Edit message
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPut("messages/{id:guid}")]
    public async Task<ActionResult<ApiResponse>> EditMessage(Guid id, [FromBody] EditMessageDto dto)
    {
        Guid me = Me;
        Message? msg = await _db.Messages.FindAsync(id);

        if (msg is null || msg.IsDeleted || msg.IsRevoked)
            return NotFound(ApiResponse.Fail("Tin nhắn không tồn tại."));
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == msg.ConversationId && cm.UserId == me);
        if (!isMember) return Forbid();
        if (msg.SenderId != me)
            return Forbid();
        if (msg.Type != MessageType.Text)
            return BadRequest(ApiResponse.Fail("Chỉ có thể sửa tin nhắn văn bản."));

        msg.Content = dto.Content;
        msg.IsEdited = true;
        msg.Status = MessageStatus.Edited;
        msg.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"conv_{msg.ConversationId}").SendAsync("MessageEdited", new MessageEditedDto
        {
            MessageId = id,
            ConversationId = msg.ConversationId,
            NewContent = dto.Content,
            EditedAt = msg.UpdatedAt
        });

        return Ok(ApiResponse.Ok("Đã chỉnh sửa tin nhắn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/conversations/messages/{id}/revoke — Revoke for everyone
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("messages/{id:guid}/revoke")]
    public async Task<ActionResult<ApiResponse>> RevokeMessage(Guid id)
    {
        Guid me = Me;
        Message? msg = await _db.Messages.FindAsync(id);
        if (msg is null || msg.IsDeleted) return NotFound(ApiResponse.Fail("Tin nhắn không tồn tại."));

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == msg.ConversationId && cm.UserId == me);
        if (!isMember) return Forbid();

        // Check if sender or group leader
        bool canRevoke = msg.SenderId == me;
        if (!canRevoke)
        {
            // Check leader in group conversation
            Conversation? conv = await _db.Conversations
                .Include(c => c.ChatGroup).ThenInclude(g => g!.Members)
                .FirstOrDefaultAsync(c => c.Id == msg.ConversationId);

            if (conv?.Type == ConversationType.Group && conv.ChatGroupId.HasValue)
            {
                canRevoke = await _db.GroupMembers.AnyAsync(
                    gm => gm.ChatGroupId == conv.ChatGroupId && gm.UserId == me && gm.Role == GroupRole.Leader);
            }
        }

        if (!canRevoke) return Forbid();

        bool wasPinned = msg.IsPinned;
        msg.IsRevoked = true;
        msg.IsPinned = false;
        msg.PinnedByUserId = null;
        msg.PinnedAt = null;
        msg.Content = null;
        msg.Status = MessageStatus.Revoked;
        msg.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (wasPinned)
        {
            await _hub.Clients.Group($"conv_{msg.ConversationId}").SendAsync("MessagePinUpdated", new MessagePinChangedDto
            {
                MessageId = id,
                ConversationId = msg.ConversationId,
                IsPinned = false,
                ChangedAt = msg.UpdatedAt
            });
        }

        await _hub.Clients.Group($"conv_{msg.ConversationId}").SendAsync("MessageRevoked", new MessageStateChangedDto
        {
            MessageId = id,
            ConversationId = msg.ConversationId,
            ChangedAt = msg.UpdatedAt
        });

        return Ok(ApiResponse.Ok("Đã thu hồi tin nhắn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/conversations/messages/{id} — Hide for current user
    // ─────────────────────────────────────────────────────────────────────────
    [HttpDelete("messages/{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteMessage(Guid id)
    {
        Guid me = Me;
        Message? msg = await _db.Messages.FindAsync(id);
        if (msg is null || msg.IsDeleted || msg.IsRevoked)
            return NotFound(ApiResponse.Fail("Tin nhắn không tồn tại."));

        bool isConversationMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == msg.ConversationId && cm.UserId == me);
        if (!isConversationMember) return Forbid();

        bool alreadyHidden = await _db.MessageHiddenForUsers
            .AnyAsync(h => h.MessageId == id && h.UserId == me);
        if (!alreadyHidden)
        {
            _db.MessageHiddenForUsers.Add(new MessageHiddenForUser
            {
                MessageId = id,
                UserId = me,
                HiddenAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        await _hub.Clients.Clients(_presence.GetConnectionIds(me)).SendAsync("MessageHiddenForMe", new MessageStateChangedDto
        {
            MessageId = id,
            ConversationId = msg.ConversationId,
            ChangedAt = DateTime.UtcNow
        });

        return Ok(ApiResponse.Ok("Đã xóa tin nhắn ở phía bạn."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static MessageDto MapMsgDto(
        Message m,
        MessageDto? replyTo,
        IEnumerable<Attachment> attachments,
        User? senderOverride = null) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        SenderUsername = senderOverride?.Username ?? m.Sender?.Username ?? string.Empty,
        SenderAvatarUrl = senderOverride?.AvatarUrl ?? m.Sender?.AvatarUrl,
        Content = m.IsRevoked || m.IsDeleted ? null : m.Content,
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
        Attachments = m.IsRevoked || m.IsDeleted
            ? []
            : attachments.Select(a => new AttachmentDto
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

    private static List<MessageReactionSummaryDto> BuildReactionSummary(
        IEnumerable<MessageReaction> reactions,
        Guid currentUserId) => reactions
        .GroupBy(r => r.Emoji)
        .OrderBy(g => g.Min(r => r.CreatedAt))
        .Select(g => new MessageReactionSummaryDto
        {
            Emoji = g.Key,
            Count = g.Count(),
            ReactedByMe = g.Any(r => r.UserId == currentUserId),
            ReactedUserIds = g.Select(r => r.UserId).ToList()
        })
        .ToList();
}
