using Chatting.Server.Data;
using Chatting.Server.Hubs;
using Chatting.Server.Models;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly IHubContext<ChatHub> _hub;
    private readonly PresenceTracker _presence;

    public GroupsController(ChatDbContext db, IHubContext<ChatHub> hub, PresenceTracker presence)
    {
        _db = db;
        _hub = hub;
        _presence = presence;
    }

    private Guid Me =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/groups — Create group
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GroupDto>>> CreateGroup([FromBody] CreateGroupDto dto)
    {
        Guid me = Me;
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(ApiResponse<GroupDto>.Fail("Tên nhóm không được để trống."));

        // Ensure creator is in member list
        if (!dto.MemberUserIds.Contains(me))
            dto.MemberUserIds.Add(me);

        if (dto.MemberUserIds.Count < 2)
            return BadRequest(ApiResponse<GroupDto>.Fail("Nhóm phải có ít nhất 2 thành viên."));

        // Verify all users exist
        List<User> members = await _db.Users
            .Where(u => dto.MemberUserIds.Contains(u.Id) && u.IsActive)
            .ToListAsync();
        if (members.Count != dto.MemberUserIds.Count)
            return BadRequest(ApiResponse<GroupDto>.Fail("Một số người dùng không tồn tại."));

        // Create group
        ChatGroup group = new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            CreatedByUserId = me,
            AllowMembersToAdd = dto.AllowMembersToAdd,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.ChatGroups.Add(group);

        // Create GroupMembers
        List<GroupMember> groupMembers = dto.MemberUserIds.Select(uid => new GroupMember
        {
            Id = Guid.NewGuid(),
            ChatGroupId = group.Id,
            UserId = uid,
            Role = uid == me ? GroupRole.Leader : GroupRole.Member,
            JoinedAt = DateTime.UtcNow
        }).ToList();
        _db.GroupMembers.AddRange(groupMembers);

        // Create Conversation for group
        Conversation conv = new()
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.Group,
            Title = group.Name,
            ChatGroupId = group.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Conversations.Add(conv);

        // Create ConversationMembers
        List<ConversationMember> convMembers = dto.MemberUserIds.Select(uid => new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            UserId = uid,
            JoinedAt = DateTime.UtcNow
        }).ToList();
        _db.ConversationMembers.AddRange(convMembers);

        // System message
        Message sysMsg = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            SenderId = me,
            Content = $"Nhóm \"{group.Name}\" đã được tạo.",
            Type = MessageType.System,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(sysMsg);

        await _db.SaveChangesAsync();

        // Join each currently connected member to the new conversation and notify
        // every device directly; nobody has joined the hub group yet.
        foreach (Guid memberId in dto.MemberUserIds.Distinct())
        {
            foreach (string connectionId in _presence.GetConnectionIds(memberId))
            {
                await _hub.Groups.AddToGroupAsync(connectionId, $"conv_{conv.Id}");
                await _hub.Clients.Client(connectionId).SendAsync("NewConversationCreated", new { ConversationId = conv.Id });
            }
        }

        GroupDto outDto = await BuildGroupDto(group.Id);
        return Ok(ApiResponse<GroupDto>.Ok(outDto, "Tạo nhóm thành công."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/groups/{groupId}
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{groupId:guid}")]
    public async Task<ActionResult<ApiResponse<GroupDto>>> GetGroup(Guid groupId)
    {
        Guid me = Me;
        bool isMember = await _db.GroupMembers.AnyAsync(gm => gm.ChatGroupId == groupId && gm.UserId == me);
        if (!isMember) return Forbid();

        return Ok(ApiResponse<GroupDto>.Ok(await BuildGroupDto(groupId)));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/groups/{groupId} — Update name/avatar
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPut("{groupId:guid}")]
    public async Task<ActionResult<ApiResponse>> UpdateGroup(Guid groupId, [FromBody] UpdateGroupDto dto)
    {
        Guid me = Me;
        bool isLeader = await _db.GroupMembers.AnyAsync(
            gm => gm.ChatGroupId == groupId && gm.UserId == me && gm.Role == GroupRole.Leader);
        if (!isLeader) return Forbid();

        ChatGroup? group = await _db.ChatGroups.FindAsync(groupId);
        if (group is null) return NotFound(ApiResponse.Fail("Nhóm không tồn tại."));

        if (!string.IsNullOrWhiteSpace(dto.Name)) group.Name = dto.Name.Trim();
        if (dto.AvatarUrl is not null) group.AvatarUrl = dto.AvatarUrl;
        if (dto.AllowMembersToAdd.HasValue) group.AllowMembersToAdd = dto.AllowMembersToAdd.Value;
        group.UpdatedAt = DateTime.UtcNow;

        // Update conversation title
        Conversation? conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ChatGroupId == groupId);
        if (conv is not null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Name)) conv.Title = dto.Name.Trim();
            conv.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        if (conv is not null)
        {
            await BroadcastSystemMessage(conv.Id, me, $"Thông tin nhóm đã được cập nhật.");
            await _hub.Clients.Group($"conv_{conv.Id}").SendAsync("GroupUpdated", new
            {
                GroupId = groupId,
                group.Name,
                group.AvatarUrl,
                group.AllowMembersToAdd
            });
        }

        return Ok(ApiResponse.Ok("Đã cập nhật thông tin nhóm."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/groups/{groupId}/members — Add members
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("{groupId:guid}/members")]
    public async Task<ActionResult<ApiResponse>> AddMembers(Guid groupId, [FromBody] AddGroupMembersDto dto)
    {
        Guid me = Me;
        ChatGroup? group = await _db.ChatGroups.FindAsync(groupId);
        if (group is null) return NotFound(ApiResponse.Fail("Nhóm không tồn tại."));

        GroupMember? actor = await _db.GroupMembers.FirstOrDefaultAsync(gm => gm.ChatGroupId == groupId && gm.UserId == me);
        if (actor is null || (actor.Role != GroupRole.Leader && !group.AllowMembersToAdd)) return Forbid();

        Conversation? conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ChatGroupId == groupId);

        List<Guid> existingIds = await _db.GroupMembers
            .Where(gm => gm.ChatGroupId == groupId)
            .Select(gm => gm.UserId)
            .ToListAsync();

        List<Guid> newIds = dto.UserIds.Except(existingIds).ToList();
        if (newIds.Count == 0)
            return BadRequest(ApiResponse.Fail("Tất cả người dùng đã là thành viên."));

        List<User> newUsers = await _db.Users
            .Where(u => newIds.Contains(u.Id) && u.IsActive).ToListAsync();

        foreach (User u in newUsers)
        {
            _db.GroupMembers.Add(new GroupMember
            {
                Id = Guid.NewGuid(),
                ChatGroupId = groupId,
                UserId = u.Id,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow
            });

            if (conv is not null)
            {
                _db.ConversationMembers.Add(new ConversationMember
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    UserId = u.Id,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();

        if (conv is not null)
        {
            User? actorUser = await _db.Users.FindAsync(me);
            string actorName = string.IsNullOrWhiteSpace(actorUser?.DisplayName) ? actorUser?.Username ?? "Thành viên" : actorUser.DisplayName;
            string names = string.Join(", ", newUsers.Select(u => string.IsNullOrWhiteSpace(u.DisplayName) ? u.Username : u.DisplayName));

            // Join new members before broadcasting the system message so they see
            // who added them immediately on every connected device.
            foreach (User addedUser in newUsers)
            {
                foreach (string connectionId in _presence.GetConnectionIds(addedUser.Id))
                {
                    await _hub.Groups.AddToGroupAsync(connectionId, $"conv_{conv.Id}");
                }
            }

            await BroadcastSystemMessage(conv.Id, me, $"{actorName} đã thêm {names} vào nhóm.");
            await _hub.Clients.Group($"conv_{conv.Id}").SendAsync("MembersAdded", new GroupMembersAddedDto
            {
                GroupId = groupId,
                ConversationId = conv.Id,
                AddedUserIds = newIds,
                AddedByUserId = me,
                AddedByUsername = actorName
            });

            foreach (User addedUser in newUsers)
            {
                foreach (string connectionId in _presence.GetConnectionIds(addedUser.Id))
                {
                    await _hub.Clients.Client(connectionId).SendAsync("NewConversationCreated", new { ConversationId = conv.Id });
                }
            }
        }

        return Ok(ApiResponse.Ok("Đã thêm thành viên vào nhóm."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/groups/{groupId}/members/{userId} — Remove member
    // ─────────────────────────────────────────────────────────────────────────
    [HttpDelete("{groupId:guid}/members/{userId:guid}")]
    public async Task<ActionResult<ApiResponse>> RemoveMember(Guid groupId, Guid userId)
    {
        Guid me = Me;
        bool isLeader = await _db.GroupMembers.AnyAsync(
            gm => gm.ChatGroupId == groupId && gm.UserId == me && gm.Role == GroupRole.Leader);
        if (!isLeader) return Forbid();
        ChatGroup? group = await _db.ChatGroups.FindAsync(groupId);
        if (group is null) return NotFound(ApiResponse.Fail("Nhóm không tồn tại."));
        if (userId == me) return BadRequest(ApiResponse.Fail("Trưởng nhóm không thể tự xóa mình. Hãy dùng chức năng Rời nhóm."));

        GroupMember? gm = await _db.GroupMembers
            .FirstOrDefaultAsync(g => g.ChatGroupId == groupId && g.UserId == userId);
        if (gm is null) return NotFound(ApiResponse.Fail("Thành viên không tồn tại."));

        List<string> removedConnectionIds = _presence.GetConnectionIds(userId).ToList();
        _db.GroupMembers.Remove(gm);

        Conversation? conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ChatGroupId == groupId);
        if (conv is not null)
        {
            ConversationMember? cm = await _db.ConversationMembers
                .FirstOrDefaultAsync(m => m.ConversationId == conv.Id && m.UserId == userId);
            if (cm is not null) _db.ConversationMembers.Remove(cm);
        }

        await _db.SaveChangesAsync();

        User? removedUser = await _db.Users.FindAsync(userId);
        if (conv is not null && removedUser is not null)
        {
            GroupMemberRemovedDto removedEvent = new()
            {
                GroupId = groupId,
                ConversationId = conv.Id,
                RemovedUserId = userId,
                GroupName = group.Name
            };

            foreach (string connectionId in removedConnectionIds)
            {
                await _hub.Clients.Client(connectionId).SendAsync("YouWereRemovedFromGroup", removedEvent);
                await _hub.Groups.RemoveFromGroupAsync(connectionId, $"conv_{conv.Id}");
            }

            await BroadcastSystemMessage(conv.Id, me, $"{removedUser.Username} đã bị xóa khỏi nhóm.");
            await _hub.Clients.Group($"conv_{conv.Id}").SendAsync("MemberRemoved", removedEvent);
        }

        return Ok(ApiResponse.Ok("Đã xóa thành viên khỏi nhóm."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/groups/{groupId}/members/role — Change role
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPut("{groupId:guid}/members/role")]
    public async Task<ActionResult<ApiResponse>> UpdateRole(Guid groupId, [FromBody] UpdateMemberRoleDto dto)
    {
        Guid me = Me;
        bool isLeader = await _db.GroupMembers.AnyAsync(
            gm => gm.ChatGroupId == groupId && gm.UserId == me && gm.Role == GroupRole.Leader);
        if (!isLeader) return Forbid();

        GroupMember? target = await _db.GroupMembers
            .FirstOrDefaultAsync(gm => gm.ChatGroupId == groupId && gm.UserId == dto.UserId);
        if (target is null) return NotFound(ApiResponse.Fail("Thành viên không tồn tại trong nhóm."));

        if (dto.Role == GroupRole.Leader)
        {
            // Demote current leader
            GroupMember? currentLeader = await _db.GroupMembers
                .FirstOrDefaultAsync(gm => gm.ChatGroupId == groupId && gm.UserId == me);
            if (currentLeader is not null) currentLeader.Role = GroupRole.Member;
        }

        target.Role = dto.Role;
        await _db.SaveChangesAsync();

        Conversation? conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ChatGroupId == groupId);
        User? targetUser = await _db.Users.FindAsync(dto.UserId);
        if (conv is not null && targetUser is not null)
        {
            string roleName = dto.Role == GroupRole.Leader ? "Trưởng nhóm" : "Thành viên";
            await BroadcastSystemMessage(conv.Id, me, $"{targetUser.Username} đã được gán vai trò {roleName}.");
            await _hub.Clients.Group($"conv_{conv.Id}").SendAsync("MemberRoleChanged", new
            {
                GroupId = groupId,
                UserId = dto.UserId,
                NewRole = dto.Role.ToString()
            });
        }

        return Ok(ApiResponse.Ok("Đã cập nhật vai trò thành viên."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/groups/{groupId}/leave — Leave group
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("{groupId:guid}/leave")]
    public async Task<ActionResult<ApiResponse>> LeaveGroup(Guid groupId)
    {
        Guid me = Me;
        GroupMember? myMembership = await _db.GroupMembers
            .FirstOrDefaultAsync(gm => gm.ChatGroupId == groupId && gm.UserId == me);
        if (myMembership is null) return NotFound(ApiResponse.Fail("Bạn không phải thành viên nhóm này."));

        List<GroupMember> remaining = await _db.GroupMembers
            .Where(gm => gm.ChatGroupId == groupId && gm.UserId != me)
            .OrderBy(gm => gm.JoinedAt)
            .ToListAsync();

        // If leader and others remain, transfer leadership
        if (myMembership.Role == GroupRole.Leader && remaining.Count > 0)
        {
            remaining[0].Role = GroupRole.Leader;
        }

        List<string> leavingConnectionIds = _presence.GetConnectionIds(me).ToList();
        _db.GroupMembers.Remove(myMembership);

        Conversation? conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ChatGroupId == groupId);
        if (conv is not null)
        {
            ConversationMember? cm = await _db.ConversationMembers
                .FirstOrDefaultAsync(m => m.ConversationId == conv.Id && m.UserId == me);
            if (cm is not null) _db.ConversationMembers.Remove(cm);
        }

        // If no one left, we can optionally delete the group; here we just mark it
        await _db.SaveChangesAsync();

        User? me_user = await _db.Users.FindAsync(me);
        if (conv is not null && me_user is not null)
        {
            foreach (string connectionId in leavingConnectionIds)
            {
                await _hub.Groups.RemoveFromGroupAsync(connectionId, $"conv_{conv.Id}");
            }

            await BroadcastSystemMessage(conv.Id, me, $"{me_user.Username} đã rời khỏi nhóm.");
            await _hub.Clients.Group($"conv_{conv.Id}").SendAsync("MemberLeft", new GroupMemberChangedDto
            {
                GroupId = groupId,
                ConversationId = conv.Id,
                UserId = me
            });
        }

        return Ok(ApiResponse.Ok("Đã rời nhóm thành công."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task BroadcastSystemMessage(Guid conversationId, Guid senderId, string content)
    {
        Message sysMsg = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = MessageType.System,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(sysMsg);
        Conversation? conv = await _db.Conversations.FindAsync(conversationId);
        if (conv is not null) conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        User? sender = await _db.Users.FindAsync(senderId);
        await _hub.Clients.Group($"conv_{conversationId}").SendAsync("ReceiveMessage", new
        {
            Id = sysMsg.Id,
            ConversationId = conversationId,
            SenderId = senderId,
            SenderUsername = sender?.Username ?? string.Empty,
            Content = content,
            Type = MessageType.System.ToString(),
            Status = MessageStatus.Sent.ToString(),
            IsEdited = false,
            IsRevoked = false,
            IsDeleted = false,
            Attachments = Array.Empty<object>(),
            CreatedAt = sysMsg.CreatedAt,
            UpdatedAt = sysMsg.UpdatedAt
        });
    }

    private async Task<GroupDto> BuildGroupDto(Guid groupId)
    {
        ChatGroup group = await _db.ChatGroups
            .Include(g => g.Members).ThenInclude(m => m.User)
            .FirstAsync(g => g.Id == groupId);

        Guid? convId = await _db.Conversations
            .Where(c => c.ChatGroupId == groupId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();

        return new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            AvatarUrl = group.AvatarUrl,
            AllowMembersToAdd = group.AllowMembersToAdd,
            CreatedByUserId = group.CreatedByUserId,
            CreatedAt = group.CreatedAt,
            ConversationId = convId,
            Members = group.Members.Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                Username = m.User.Username,
                AvatarUrl = m.User.AvatarUrl,
                Role = m.Role,
                JoinedAt = m.JoinedAt,
                IsOnline = _presence.IsOnline(m.UserId)
            }).ToList()
        };
    }
}
