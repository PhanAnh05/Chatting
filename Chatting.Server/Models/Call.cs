using Chatting.Shared.Enums;

namespace Chatting.Server.Models;

public class Call
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid CallerId { get; set; }
    public Guid? ReceiverId { get; set; }
    public CallType CallType { get; set; }
    public CallStatus Status { get; set; } = CallStatus.Ringing;
    public DateTime StartedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }

    // Navigation
    public Conversation Conversation { get; set; } = null!;
    public User Caller { get; set; } = null!;
    public User? Receiver { get; set; }
}
