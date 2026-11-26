namespace Chatting.Server.Models;

public class OtpCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public string Purpose { get; set; } = "REGISTER";
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public int ResendCount { get; set; }
    public int MaxResends { get; set; } = 3;
    public DateTime? LastSentAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
