using Chatting.Shared.Enums;

namespace Chatting.Server.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public UserStatus Status { get; set; } = UserStatus.Offline;
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
    public ICollection<Friendship> Friendships1 { get; set; } = new List<Friendship>();
    public ICollection<Friendship> Friendships2 { get; set; } = new List<Friendship>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<ConversationMember> ConversationMemberships { get; set; } = new List<ConversationMember>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Call> InitiatedCalls { get; set; } = new List<Call>();
    public ICollection<Call> ReceivedCalls { get; set; } = new List<Call>();
}
