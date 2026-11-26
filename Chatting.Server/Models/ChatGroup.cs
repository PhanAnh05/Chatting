namespace Chatting.Server.Models;

public class ChatGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool AllowMembersToAdd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public User CreatedByUser { get; set; } = null!;
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public Conversation? Conversation { get; set; }
}
