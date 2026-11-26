using Chatting.Shared.Enums;

namespace Chatting.Server.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public bool IsEdited { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPinned { get; set; }
    public Guid? PinnedByUserId { get; set; }
    public DateTime? PinnedAt { get; set; }
    public string? ForwardedFromUsername { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Conversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public Message? ReplyToMessage { get; set; }
    public ICollection<Message> Replies { get; set; } = new List<Message>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
