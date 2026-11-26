using Chatting.Server.Data;
using Chatting.Server.Models;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Chatting.Server.Services;

/// <summary>
/// Handles all AI assistant interactions: seeding the AI bot user, creating/resolving
/// the AI conversation for each human user, and calling the OpenAI API.
/// </summary>
public class AiAssistantService
{
    // Fixed deterministic Guid for the AI system user
    public static readonly Guid FixedAiUserId = new("00000000-0000-0000-0000-000000000001");
    public const string AiUsername = "chatting_ai";
    public const string AiDisplayName = "Chatting AI ✨";
    public const string AiEmail = "ai@chatting.system";

    private readonly ChatDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AiAssistantService> _logger;
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public AiAssistantService(ChatDbContext db, IConfiguration config, ILogger<AiAssistantService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Seed AI user in database if not exists
    // ─────────────────────────────────────────────────────────────────────────
    public async Task EnsureAiUserExistsAsync()
    {
        bool exists = await _db.Users.AnyAsync(u => u.Id == FixedAiUserId);
        if (exists) return;

        _db.Users.Add(new User
        {
            Id = FixedAiUserId,
            Username = AiUsername,
            DisplayName = AiDisplayName,
            Email = AiEmail,
            PasswordHash = "AI_SYSTEM_NO_LOGIN",
            IsEmailVerified = true,
            IsActive = true,
            Status = UserStatus.Online,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        _logger.LogInformation("AI system user seeded with Id={Id}", FixedAiUserId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Get or create a private AI conversation for the given user
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<Guid> GetOrCreateAiConversationAsync(Guid userId)
    {
        await EnsureAiUserExistsAsync();

        // Find existing direct conversation between userId and AI
        var userConvIds = _db.ConversationMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.ConversationId);

        var aiConvIds = _db.ConversationMembers
            .Where(cm => cm.UserId == FixedAiUserId)
            .Select(cm => cm.ConversationId);

        Guid? existingId = await _db.Conversations
            .Where(c => c.Type == ConversationType.Direct
                     && userConvIds.Contains(c.Id)
                     && aiConvIds.Contains(c.Id))
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();

        if (existingId.HasValue)
            return existingId.Value;

        // Create new conversation
        Conversation conv = new()
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.Direct,
            Title = AiDisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Conversations.Add(conv);

        _db.ConversationMembers.Add(new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        });
        _db.ConversationMembers.Add(new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = conv.Id,
            UserId = FixedAiUserId,
            JoinedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Created AI conversation {ConvId} for user {UserId}", conv.Id, userId);
        return conv.Id;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────────
    // Save user message to database and return DTO immediately
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<MessageDto?> SaveUserMessageAsync(Guid userId, Guid conversationId, string userMessage)
    {
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (!isMember) return null;

        User? sender = await _db.Users.FindAsync(userId);
        if (sender is null) return null;

        Message userMsg = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = userId,
            Content = userMessage,
            Type = MessageType.Text,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Messages.Add(userMsg);

        Conversation? conv = await _db.Conversations.FindAsync(conversationId);
        if (conv is not null) conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new MessageDto
        {
            Id = userMsg.Id,
            ConversationId = conversationId,
            SenderId = userId,
            SenderUsername = sender.Username,
            SenderAvatarUrl = sender.AvatarUrl,
            Content = userMessage,
            Type = MessageType.Text,
            Status = MessageStatus.Sent,
            CreatedAt = userMsg.CreatedAt,
            UpdatedAt = userMsg.UpdatedAt
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generate AI reply using conversation context, save to DB, and return DTO
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<MessageDto?> GenerateAiReplyAsync(Guid userId, Guid conversationId)
    {
        bool isMember = await _db.ConversationMembers
            .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);
        if (!isMember) return null;

        // Fetch last 10 messages for context
        List<Message> context = await _db.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId
                     && !m.IsDeleted && !m.IsRevoked
                     && !_db.MessageHiddenForUsers.Any(h => h.MessageId == m.Id && h.UserId == userId)
                     && m.Type == MessageType.Text)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .ToListAsync();
        context.Reverse();

        // Call AI API
        string aiReplyText = await CallAiApiAsync(userId, context);

        // Save AI reply
        Message aiMsg = new()
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = FixedAiUserId,
            Content = aiReplyText,
            Type = MessageType.Text,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow.AddMilliseconds(5),
            UpdatedAt = DateTime.UtcNow.AddMilliseconds(5)
        };
        _db.Messages.Add(aiMsg);

        Conversation? conv = await _db.Conversations.FindAsync(conversationId);
        if (conv is not null) conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new MessageDto
        {
            Id = aiMsg.Id,
            ConversationId = conversationId,
            SenderId = FixedAiUserId,
            SenderUsername = AiUsername,
            Content = aiReplyText,
            Type = MessageType.Text,
            Status = MessageStatus.Sent,
            CreatedAt = aiMsg.CreatedAt,
            UpdatedAt = aiMsg.UpdatedAt
        };
    }

    // Convenience wrapper for backwards compatibility
    public async Task<(MessageDto UserMessageDto, MessageDto AiMessageDto)?> SendToAiAsync(
        Guid userId, Guid conversationId, string userMessage)
    {
        var userDto = await SaveUserMessageAsync(userId, conversationId, userMessage);
        if (userDto is null) return null;

        var aiDto = await GenerateAiReplyAsync(userId, conversationId);
        if (aiDto is null) return null;

        return (userDto, aiDto);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Build AI API request (Supports OpenAI and Google Gemini)
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<string> CallAiApiAsync(Guid userId, List<Message> contextMessages)
    {
        string apiKey = _config["AiSettings:OpenAiApiKey"] ?? string.Empty;
        string model = _config["AiSettings:Model"] ?? "gpt-4o-mini";
        string endpoint = _config["AiSettings:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        string systemPrompt = _config["AiSettings:SystemPrompt"] ?? "Bạn là Chatting AI ✨, trợ lý ảo thông minh.";
        string provider = _config["AiSettings:Provider"] ?? "OpenAI";

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("PASTE_YOUR"))
        {
            return "⚠️ Chatting AI chưa được cấu hình API Key. Vui lòng cập nhật `AiSettings:OpenAiApiKey` trong appsettings.json của server.";
        }

        // Auto-detect Google Gemini API key (starts with AIzaSy)
        bool isGemini = provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase) ||
                        apiKey.StartsWith("AIzaSy", StringComparison.OrdinalIgnoreCase);

        if (isGemini)
        {
            if (endpoint.Contains("api.openai.com") || string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";
            }
            if (model.Contains("gpt"))
            {
                model = "gemini-1.5-flash";
            }
        }

        try
        {
            var messages = new JsonArray
            {
                new JsonObject { ["role"] = "system", ["content"] = systemPrompt }
            };

            foreach (Message m in contextMessages)
            {
                string role = m.SenderId == userId ? "user" : "assistant";
                messages.Add(new JsonObject { ["role"] = role, ["content"] = m.Content ?? "" });
            }

            var body = new JsonObject
            {
                ["model"] = model,
                ["messages"] = messages,
                ["max_tokens"] = 800,
                ["temperature"] = 0.7
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _http.SendAsync(request);
            string raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI API error {Status}: {Body}", response.StatusCode, raw);
                try
                {
                    using JsonDocument errDoc = JsonDocument.Parse(raw);
                    if (errDoc.RootElement.TryGetProperty("error", out var errObj))
                    {
                        string? errMsg = errObj.TryGetProperty("message", out var m) ? m.GetString() : null;
                        string? code = errObj.TryGetProperty("code", out var c) ? c.GetString() : null;

                        if (code == "credit_balance_exhausted" || code == "insufficient_quota" || (errMsg != null && errMsg.Contains("credits remaining")))
                        {
                            return "⚠️ Tài khoản OpenAI của bạn đã hết hạn mức credit (credit_balance_exhausted). Hãy nạp tiền tại https://platform.openai.com/settings/organization/billing/ hoặc chuyển sang dùng Google Gemini API (miễn phí).";
                        }
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return "⚠️ API Key không hợp lệ hoặc đã bị vô hiệu hóa (401 Unauthorized). Vui lòng kiểm tra lại key.";
                        }
                        if (!string.IsNullOrWhiteSpace(errMsg))
                        {
                            return $"⚠️ Nhà cung cấp AI báo lỗi: {errMsg}";
                        }
                    }
                }
                catch { }

                return $"❌ Trợ lý AI tạm thời không khả dụng (Mã lỗi: {(int)response.StatusCode}). Vui lòng kiểm tra tài khoản AI.";
            }

            using JsonDocument doc = JsonDocument.Parse(raw);
            string? reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return reply?.Trim() ?? "❓ Không nhận được phản hồi từ AI.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling AI API");
            return $"❌ Lỗi kết nối tới trợ lý AI: {ex.Message}";
        }
    }
}
