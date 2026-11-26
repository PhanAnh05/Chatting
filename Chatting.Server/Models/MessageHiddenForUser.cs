namespace Chatting.Server.Models;

public class MessageHiddenForUser
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime HiddenAt { get; set; }
}
