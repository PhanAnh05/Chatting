using Chatting.Shared.Enums;

namespace Chatting.Shared.DTOs;

public class ConversationDto
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? ChatGroupId { get; set; }
    public List<ConversationMemberDto> Members { get; set; } = new();
}

public class ConversationMemberDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? ProfileDisplayName { get; set; }
    public string? Bio { get; set; }
    public string DisplayName => string.IsNullOrWhiteSpace(Nickname)
        ? (string.IsNullOrWhiteSpace(ProfileDisplayName) ? Username : ProfileDisplayName)
        : Nickname;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public bool IsOnline { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
}

public class SetNicknameRequestDto
{
    public Guid TargetUserId { get; set; }
    public string? Nickname { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string? Content { get; set; }
    public MessageType Type { get; set; }
    public MessageStatus Status { get; set; }
    public bool IsEdited { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPinned { get; set; }
    public Guid? PinnedByUserId { get; set; }
    public DateTime? PinnedAt { get; set; }
    public string? ForwardedFromUsername { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public MessageDto? ReplyToMessage { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<MessageReactionSummaryDto> Reactions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MessageReactionSummaryDto
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool ReactedByMe { get; set; }
    public List<Guid> ReactedUserIds { get; set; } = new();
}

public class SetMessageReactionDto
{
    public string Emoji { get; set; } = string.Empty;
}

public class MessageReactionsChangedDto
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public List<MessageReactionSummaryDto> Reactions { get; set; } = new();
}

public class SendMessageDto
{
    public Guid ConversationId { get; set; }
    public string? Content { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid? ReplyToMessageId { get; set; }
    public List<Guid> AttachmentIds { get; set; } = new();
}

public class EditMessageDto
{
    public string Content { get; set; } = string.Empty;
}

public class MessageEditedDto
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public string NewContent { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
}

public class SetMessagePinDto
{
    public bool IsPinned { get; set; }
}

public class MessagePinChangedDto
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public bool IsPinned { get; set; }
    public Guid? PinnedByUserId { get; set; }
    public DateTime? PinnedAt { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class ForwardMessageDto
{
    public List<Guid> TargetConversationIds { get; set; } = new();
}

public class MessageStateChangedDto
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? RelativePath { get; set; }
    public int? DurationSeconds { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage => (PageIndex * PageSize) < TotalCount;
}

public class SharedContentDto
{
    public List<SharedMediaItemDto> Media { get; set; } = new();
    public List<SharedFileItemDto> Files { get; set; } = new();
    public List<SharedLinkItemDto> Links { get; set; } = new();
    public List<SharedVoiceItemDto> VoiceMessages { get; set; } = new();
}

public class SharedMediaItemDto
{
    public Guid AttachmentId { get; set; }
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? RelativePath { get; set; }
    public bool IsVideo { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SharedFileItemDto
{
    public Guid AttachmentId { get; set; }
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? RelativePath { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SharedLinkItemDto
{
    public Guid MessageId { get; set; }
    public string Url { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SharedVoiceItemDto
{
    public Guid AttachmentId { get; set; }
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
