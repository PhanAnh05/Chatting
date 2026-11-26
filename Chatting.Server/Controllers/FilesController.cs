using Chatting.Server.Data;
using Chatting.Server.Models;
using Chatting.Server.Services;
using Chatting.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly FileStorageService _fileStorage;
    private readonly IHubContext<Hubs.ChatHub> _hub;

    public FilesController(ChatDbContext db, FileStorageService fileStorage, IHubContext<Hubs.ChatHub> hub)
    {
        _db = db;
        _fileStorage = fileStorage;
        _hub = hub;
    }

    private Guid Me =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")!.Value);

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/files/upload?conversationId={id}
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> Upload(
        [FromQuery] Guid conversationId,
        IFormFile file)
    {
        Guid me = Me;

        // Verify membership
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == me);
        if (!isMember)
            return Forbid();

        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<AttachmentDto>.Fail("Không có file nào được gửi lên."));

        if (file.Length > _fileStorage.MaxFileSizeBytes)
            return BadRequest(ApiResponse<AttachmentDto>.Fail($"File vượt quá giới hạn {_fileStorage.MaxFileSizeBytes / 1024 / 1024} MB."));

        if (!_fileStorage.IsAllowedExtension(file.FileName))
            return BadRequest(ApiResponse<AttachmentDto>.Fail("Loại file không được phép."));

        // Determine subfolder by content type
        string subfolder = GetSubfolder(file.ContentType);

        using Stream stream = file.OpenReadStream();
        (string storedName, string relativePath) = await _fileStorage.SaveFileAsync(stream, file.FileName, subfolder);

        Attachment attachment = new()
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = me,
            OriginalFileName = file.FileName,
            StoredFileName = storedName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            RelativePath = relativePath,
            CreatedAt = DateTime.UtcNow
        };
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<AttachmentDto>.Ok(new AttachmentDto
        {
            Id = attachment.Id,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            RelativePath = attachment.RelativePath
        }, "Upload thành công."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/files/upload-voice?conversationId={id}
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("upload-voice")]
    [RequestSizeLimit(10_485_760)] // 10 MB for voice
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> UploadVoice(
        [FromQuery] Guid conversationId,
        [FromQuery] int durationSeconds,
        IFormFile file)
    {
        Guid me = Me;

        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == me);
        if (!isMember) return Forbid();

        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<AttachmentDto>.Fail("Không có file âm thanh."));

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".wav" && ext != ".mp3" && ext != ".ogg")
            return BadRequest(ApiResponse<AttachmentDto>.Fail("Chỉ chấp nhận định dạng .wav, .mp3, .ogg cho giọng nói."));

        using Stream stream = file.OpenReadStream();
        (string storedName, string relativePath) = await _fileStorage.SaveFileAsync(stream, file.FileName, "voice");

        Attachment attachment = new()
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = me,
            OriginalFileName = file.FileName,
            StoredFileName = storedName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            RelativePath = relativePath,
            DurationSeconds = durationSeconds,
            CreatedAt = DateTime.UtcNow
        };
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<AttachmentDto>.Ok(new AttachmentDto
        {
            Id = attachment.Id,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            RelativePath = attachment.RelativePath,
            DurationSeconds = attachment.DurationSeconds
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/files/{attachmentId}/download
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(Guid attachmentId)
    {
        Guid me = Me;

        Attachment? att = await _db.Attachments
            .Include(a => a.Message)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (att is null)
            return NotFound(ApiResponse.Fail("File không tồn tại."));

        // Security: must be member of the conversation the message belongs to
        if (att.MessageId.HasValue)
        {
            var msg = att.Message ?? await _db.Messages.FindAsync(att.MessageId.Value);
            if (msg is not null)
            {
                bool isMember = await _db.ConversationMembers
                    .AnyAsync(cm => cm.ConversationId == msg.ConversationId && cm.UserId == me);
                if (!isMember) return Forbid();
            }
            else if (att.UploadedByUserId != me)
            {
                return Forbid();
            }
        }
        else if (att.UploadedByUserId != me)
        {
            return Forbid();
        }

        string absPath = _fileStorage.GetAbsolutePath(att.RelativePath);
        if (!System.IO.File.Exists(absPath))
            return NotFound(ApiResponse.Fail("File không tìm thấy trên server."));

        return PhysicalFile(absPath, string.IsNullOrEmpty(att.ContentType) ? "application/octet-stream" : att.ContentType, att.OriginalFileName, enableRangeProcessing: true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/files/{attachmentId}/raw (for inline viewing/streaming)
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("{attachmentId:guid}/raw")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRaw(Guid attachmentId)
    {
        Attachment? att = await _db.Attachments.FindAsync(attachmentId);
        if (att is null)
            return NotFound();

        string absPath = _fileStorage.GetAbsolutePath(att.RelativePath);
        if (!System.IO.File.Exists(absPath))
            return NotFound();

        return PhysicalFile(absPath, string.IsNullOrEmpty(att.ContentType) ? "application/octet-stream" : att.ContentType, enableRangeProcessing: true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/files/avatar
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("avatar")]
    [RequestSizeLimit(10_485_760)] // 10 MB
    public async Task<ActionResult<ApiResponse<string>>> UploadAvatar(IFormFile file)
    {
        Guid me = Me;
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("Vui lòng chọn ảnh đại diện."));

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp" and not ".gif")
            return BadRequest(ApiResponse<string>.Fail("Chỉ chấp nhận file ảnh."));

        using Stream stream = file.OpenReadStream();
        (string storedName, _) = await _fileStorage.SaveFileAsync(stream, file.FileName, "avatars");
        string avatarUrl = $"api/files/avatars/{storedName}";

        var user = await _db.Users.FindAsync(me);
        if (user != null)
        {
            user.AvatarUrl = avatarUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("UserProfileUpdated", UsersController.ToDto(user));
        }

        return Ok(ApiResponse<string>.Ok(avatarUrl, "Cập nhật avatar thành công."));
    }

    [HttpGet("avatars/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetAvatar(string fileName)
    {
        string safeFileName = Path.GetFileName(fileName);
        string path = _fileStorage.GetAbsolutePath($"uploads/avatars/{safeFileName}");
        if (!System.IO.File.Exists(path)) return NotFound();
        string contentType = new FileExtensionContentTypeProvider().TryGetContentType(path, out string? type)
            ? type : "application/octet-stream";
        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static string GetSubfolder(string contentType)
    {
        if (contentType.StartsWith("image/")) return "images";
        if (contentType.StartsWith("video/")) return "videos";
        if (contentType.StartsWith("audio/")) return "audio";
        return "files";
    }
}
