namespace Chatting.Server.Models;

public class Friendship
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
}
