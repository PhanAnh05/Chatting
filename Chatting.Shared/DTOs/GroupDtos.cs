using Chatting.Shared.Enums;

namespace Chatting.Shared.DTOs;

public class GroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool AllowMembersToAdd { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
    public Guid? ConversationId { get; set; }
}

public class GroupMemberDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public GroupRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsOnline { get; set; }
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public List<Guid> MemberUserIds { get; set; } = new();
    public bool AllowMembersToAdd { get; set; }
}

public class UpdateGroupDto
{
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? AllowMembersToAdd { get; set; }
}

public class AddGroupMembersDto
{
    public List<Guid> UserIds { get; set; } = new();
}

public class GroupMembersAddedDto
{
    public Guid GroupId { get; set; }
    public Guid ConversationId { get; set; }
    public List<Guid> AddedUserIds { get; set; } = new();
    public Guid AddedByUserId { get; set; }
    public string AddedByUsername { get; set; } = string.Empty;
}

public class GroupMemberRemovedDto
{
    public Guid GroupId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid RemovedUserId { get; set; }
    public string GroupName { get; set; } = string.Empty;
}

public class GroupMemberChangedDto
{
    public Guid GroupId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateMemberRoleDto
{
    public Guid UserId { get; set; }
    public GroupRole Role { get; set; }
}
