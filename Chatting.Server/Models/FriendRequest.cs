using Chatting.Shared.Enums;

namespace Chatting.Server.Models;

public class FriendRequest
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
