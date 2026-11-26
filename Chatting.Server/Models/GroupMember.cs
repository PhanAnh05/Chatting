using Chatting.Shared.Enums;

namespace Chatting.Server.Models;

public class GroupMember
{
    public Guid Id { get; set; }
    public Guid ChatGroupId { get; set; }
    public Guid UserId { get; set; }
    public GroupRole Role { get; set; } = GroupRole.Member;
    public DateTime JoinedAt { get; set; }

    // Navigation
    public ChatGroup ChatGroup { get; set; } = null!;
    public User User { get; set; } = null!;
}
