using Chatting.Server.Models;
using Chatting.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Chatting.Server.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<ChatGroup> ChatGroups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMember> ConversationMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageHiddenForUser> MessageHiddenForUsers { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Call> Calls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Users ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).ValueGeneratedOnAdd();
            e.Property(u => u.Username).HasMaxLength(60).IsRequired();
            e.Property(u => u.DisplayName).HasMaxLength(100);
            e.Property(u => u.Bio).HasMaxLength(500);
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Status).HasConversion<int>();

            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ── OtpCodes ───────────────────────────────────────────────────────────
        modelBuilder.Entity<OtpCode>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).ValueGeneratedOnAdd();
            e.Property(o => o.CodeHash).IsRequired();
            e.Property(o => o.Purpose).HasMaxLength(50);

            e.HasOne(o => o.User)
             .WithMany(u => u.OtpCodes)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── RefreshTokens ──────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).ValueGeneratedOnAdd();
            e.Property(r => r.TokenHash).IsRequired();
            e.Property(r => r.CreatedByIp).HasMaxLength(50);

            e.HasIndex(r => r.TokenHash).IsUnique();

            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── FriendRequests ─────────────────────────────────────────────────────
        modelBuilder.Entity<FriendRequest>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();
            e.Property(f => f.Status).HasConversion<int>();

            e.HasIndex(f => new { f.SenderId, f.ReceiverId, f.Status });

            e.HasOne(f => f.Sender)
             .WithMany(u => u.SentFriendRequests)
             .HasForeignKey(f => f.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(f => f.Receiver)
             .WithMany(u => u.ReceivedFriendRequests)
             .HasForeignKey(f => f.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Friendships ────────────────────────────────────────────────────────
        modelBuilder.Entity<Friendship>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();

            e.HasIndex(f => new { f.User1Id, f.User2Id }).IsUnique();

            e.HasOne(f => f.User1)
             .WithMany(u => u.Friendships1)
             .HasForeignKey(f => f.User1Id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(f => f.User2)
             .WithMany(u => u.Friendships2)
             .HasForeignKey(f => f.User2Id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ChatGroups ─────────────────────────────────────────────────────────
        modelBuilder.Entity<ChatGroup>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Id).ValueGeneratedOnAdd();
            e.Property(g => g.Name).HasMaxLength(100).IsRequired();

            e.HasOne(g => g.CreatedByUser)
             .WithMany()
             .HasForeignKey(g => g.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── GroupMembers ───────────────────────────────────────────────────────
        modelBuilder.Entity<GroupMember>(e =>
        {
            e.HasKey(gm => gm.Id);
            e.Property(gm => gm.Id).ValueGeneratedOnAdd();
            e.Property(gm => gm.Role).HasConversion<int>();

            e.HasIndex(gm => new { gm.ChatGroupId, gm.UserId }).IsUnique();

            e.HasOne(gm => gm.ChatGroup)
             .WithMany(g => g.Members)
             .HasForeignKey(gm => gm.ChatGroupId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(gm => gm.User)
             .WithMany(u => u.GroupMemberships)
             .HasForeignKey(gm => gm.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Conversations ──────────────────────────────────────────────────────
        modelBuilder.Entity<Conversation>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedOnAdd();
            e.Property(c => c.Type).HasConversion<int>();
            e.Property(c => c.Title).HasMaxLength(100);

            e.HasIndex(c => c.UpdatedAt);

            // 1-1 with ChatGroup, Cascade from Conversation side
            e.HasOne(c => c.ChatGroup)
             .WithOne(g => g.Conversation)
             .HasForeignKey<Conversation>(c => c.ChatGroupId)
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired(false);
        });

        // ── ConversationMembers ────────────────────────────────────────────────
        modelBuilder.Entity<ConversationMember>(e =>
        {
            e.HasKey(cm => cm.Id);
            e.Property(cm => cm.Id).ValueGeneratedOnAdd();
            e.Property(cm => cm.Nickname).HasMaxLength(100);

            e.HasIndex(cm => new { cm.ConversationId, cm.UserId }).IsUnique();

            e.HasOne(cm => cm.Conversation)
             .WithMany(c => c.Members)
             .HasForeignKey(cm => cm.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cm => cm.User)
             .WithMany(u => u.ConversationMemberships)
             .HasForeignKey(cm => cm.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Messages ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.Property(m => m.Content).HasMaxLength(4000);
            e.Property(m => m.Type).HasConversion<int>();
            e.Property(m => m.Status).HasConversion<int>();
            e.Property(m => m.ForwardedFromUsername).HasMaxLength(100);

            e.HasIndex(m => new { m.ConversationId, m.CreatedAt });

            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing FK for replies
            e.HasOne(m => m.ReplyToMessage)
             .WithMany(m => m.Replies)
             .HasForeignKey(m => m.ReplyToMessageId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);
        });

        modelBuilder.Entity<MessageHiddenForUser>(e =>
        {
            e.HasKey(h => new { h.MessageId, h.UserId });

            e.HasIndex(h => h.UserId);

            e.HasOne<Message>()
             .WithMany()
             .HasForeignKey(h => h.MessageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<User>()
             .WithMany()
             .HasForeignKey(h => h.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MessageReaction>(e =>
        {
            e.HasKey(r => new { r.MessageId, r.UserId, r.Emoji });
            e.Property(r => r.Emoji).HasMaxLength(16).IsRequired();
            e.HasIndex(r => r.UserId);

            e.HasOne<Message>()
             .WithMany()
             .HasForeignKey(r => r.MessageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<User>()
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Attachments ────────────────────────────────────────────────────────
        modelBuilder.Entity<Attachment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).ValueGeneratedOnAdd();
            e.Property(a => a.OriginalFileName).HasMaxLength(260);
            e.Property(a => a.StoredFileName).HasMaxLength(260);
            e.Property(a => a.ContentType).HasMaxLength(128);
            e.Property(a => a.RelativePath).HasMaxLength(500);

            e.HasOne(a => a.Message)
             .WithMany(m => m.Attachments)
             .HasForeignKey(a => a.MessageId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);

            e.HasOne(a => a.UploadedByUser)
             .WithMany()
             .HasForeignKey(a => a.UploadedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Calls ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Call>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedOnAdd();
            e.Property(c => c.CallType).HasConversion<int>();
            e.Property(c => c.Status).HasConversion<int>();

            e.HasOne(c => c.Conversation)
             .WithMany(cv => cv.Calls)
             .HasForeignKey(c => c.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Caller)
             .WithMany(u => u.InitiatedCalls)
             .HasForeignKey(c => c.CallerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Receiver)
             .WithMany(u => u.ReceivedCalls)
             .HasForeignKey(c => c.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);
        });
    }
}
