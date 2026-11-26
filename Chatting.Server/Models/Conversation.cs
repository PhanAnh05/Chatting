using Chatting.Shared.Enums;

namespace Chatting.Server.Models;

public class Conversation
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string? Title { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? ChatGroupId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ChatGroup? ChatGroup { get; set; }
    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Call> Calls { get; set; } = new List<Call>();
}
