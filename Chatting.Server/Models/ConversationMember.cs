namespace Chatting.Server.Models;

public class ConversationMember
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public Guid? LastReadMessageId { get; set; }
    public bool IsMuted { get; set; }
    public string? Nickname { get; set; }
    public DateTime? ClearedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public Conversation Conversation { get; set; } = null!;
    public User User { get; set; } = null!;
}
