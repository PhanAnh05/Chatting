using Chatting.Shared.Enums;

namespace Chatting.Shared.DTOs;

public class UserSearchResultDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public bool IsFriend { get; set; }
    public bool HasSentRequest { get; set; }
    public bool HasReceivedRequest { get; set; }
    public Guid? PendingRequestId { get; set; }
    public bool IsOnline { get; set; }
}

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public Guid ReceiverId { get; set; }
    public string ReceiverUsername { get; set; } = string.Empty;
    public string? ReceiverAvatarUrl { get; set; }
    public FriendRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendFriendRequestDto
{
    public Guid ReceiverId { get; set; }
}

public class FriendDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? DirectConversationId { get; set; }
}
