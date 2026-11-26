using Chatting.Server.Data;
using Chatting.Server.Hubs;
using Chatting.Server.Models;
using Chatting.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly IHubContext<ChatHub> _hub;

    public UsersController(ChatDbContext db, IHubContext<ChatHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    private Guid Me => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")!.Value);

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
    {
        User? user = await _db.Users.FindAsync(Me);
        if (user is null) return Unauthorized();

        string displayName = (dto.DisplayName ?? string.Empty).Trim();
        if (displayName.Length > 100) return BadRequest(ApiResponse<UserProfileDto>.Fail("Tên hiển thị tối đa 100 ký tự."));
        string bio = (dto.Bio ?? string.Empty).Trim();
        if (bio.Length > 500) return BadRequest(ApiResponse<UserProfileDto>.Fail("Phần giới thiệu tối đa 500 ký tự."));
        user.DisplayName = displayName;
        user.Bio = bio;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        UserProfileDto profile = ToDto(user);
        await _hub.Clients.All.SendAsync("UserProfileUpdated", profile);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Đã cập nhật hồ sơ."));
    }

    internal static UserProfileDto ToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName,
        AvatarUrl = user.AvatarUrl,
        Bio = user.Bio
    };
}
