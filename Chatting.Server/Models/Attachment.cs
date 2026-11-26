namespace Chatting.Server.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public Guid? MessageId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Message? Message { get; set; }
    public User UploadedByUser { get; set; } = null!;
}
