using Chatting.Shared.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chatting.Client.Services;

/// <summary>
/// Wraps HttpClient for all REST API calls. Automatically attaches JWT Bearer token
/// and handles 401 → refresh token rotation transparently.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionManager _session;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(SessionManager session)
    {
        _session = session;
        _http = new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Auth endpoints
    // ─────────────────────────────────────────────────────────────────────────
    public Task<ApiResponse?> RegisterAsync(RegisterRequestDto dto) =>
        PostAsync<ApiResponse>("api/auth/register", dto);

    public Task<ApiResponse<bool>?> CheckUsernameAsync(string username) =>
        GetAsync<ApiResponse<bool>>($"api/auth/check-username?username={Uri.EscapeDataString(username)}");

    public Task<ApiResponse?> VerifyOtpAsync(VerifyOtpRequestDto dto) =>
        PostAsync<ApiResponse>("api/auth/verify-otp", dto);

    public Task<ApiResponse?> ResendOtpAsync(ResendOtpRequestDto dto) =>
        PostAsync<ApiResponse>("api/auth/resend-otp", dto);

    public Task<ApiResponse?> RequestPasswordResetAsync(ForgotPasswordRequestDto dto) =>
        PostAsync<ApiResponse>("api/auth/forgot-password", dto);

    public Task<ApiResponse?> ResetPasswordAsync(ResetPasswordRequestDto dto) =>
        PostAsync<ApiResponse>("api/auth/reset-password", dto);

    public Task<ApiResponse<LoginResponseDto>?> LoginAsync(LoginRequestDto dto) =>
        PostAsync<ApiResponse<LoginResponseDto>>("api/auth/login", dto);

    public Task<ApiResponse<LoginResponseDto>?> RefreshAsync(RefreshTokenRequestDto dto) =>
        PostAsync<ApiResponse<LoginResponseDto>>("api/auth/refresh", dto);

    public Task<ApiResponse?> LogoutAsync(LogoutRequestDto dto) =>
        PostAuthAsync<ApiResponse>("api/auth/logout", dto);

    // ─────────────────────────────────────────────────────────────────────────
    // Ping / connectivity
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<bool> PingAsync(string serverUrl)
    {
        try
        {
            using HttpClient tmp = new() { Timeout = TimeSpan.FromSeconds(5) };
            HttpResponseMessage res = await tmp.GetAsync($"{serverUrl.TrimEnd('/')}/api/auth/ping");
            // Even 404 means server is reachable
            return res.StatusCode != HttpStatusCode.ServiceUnavailable;
        }
        catch { return false; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Friends
    // ─────────────────────────────────────────────────────────────────────────
    public Task<ApiResponse<List<FriendDto>>?> GetFriendsAsync() =>
        GetAuthAsync<ApiResponse<List<FriendDto>>>("api/friends");

    public Task<ApiResponse<List<UserSearchResultDto>>?> SearchUsersAsync(string keyword) =>
        GetAuthAsync<ApiResponse<List<UserSearchResultDto>>>($"api/friends/search?keyword={Uri.EscapeDataString(keyword)}");

    public Task<ApiResponse<List<FriendRequestDto>>?> GetIncomingRequestsAsync() =>
        GetAuthAsync<ApiResponse<List<FriendRequestDto>>>("api/friends/requests/incoming");

    public Task<ApiResponse<List<FriendRequestDto>>?> GetOutgoingRequestsAsync() =>
        GetAuthAsync<ApiResponse<List<FriendRequestDto>>>("api/friends/requests/outgoing");

    public Task<ApiResponse<FriendRequestDto>?> SendFriendRequestAsync(SendFriendRequestDto dto) =>
        PostAuthAsync<ApiResponse<FriendRequestDto>>("api/friends/requests", dto);

    public Task<ApiResponse?> AcceptFriendRequestAsync(Guid requestId) =>
        PostAuthAsync<ApiResponse>($"api/friends/requests/{requestId}/accept", null);

    public Task<ApiResponse?> RejectFriendRequestAsync(Guid requestId) =>
        PostAuthAsync<ApiResponse>($"api/friends/requests/{requestId}/reject", null);

    public Task<ApiResponse?> CancelFriendRequestAsync(Guid requestId) =>
        DeleteAuthAsync<ApiResponse>($"api/friends/requests/{requestId}");

    public Task<ApiResponse?> UnfriendAsync(Guid friendUserId) =>
        DeleteAuthAsync<ApiResponse>($"api/friends/{friendUserId}");

    // ─────────────────────────────────────────────────────────────────────────
    // Conversations & Messages
    // ─────────────────────────────────────────────────────────────────────────
    public Task<ApiResponse<List<ConversationDto>>?> GetConversationsAsync() =>
        GetAuthAsync<ApiResponse<List<ConversationDto>>>("api/conversations");

    public Task<ApiResponse<ConversationDto>?> GetConversationAsync(Guid conversationId) =>
        GetAuthAsync<ApiResponse<ConversationDto>>($"api/conversations/{conversationId}");

    public Task<ApiResponse?> SetNicknameAsync(Guid conversationId, SetNicknameRequestDto dto) =>
        PutAuthAsync<ApiResponse>($"api/conversations/{conversationId}/nickname", dto);

    public Task<ApiResponse<UserProfileDto>?> UpdateProfileAsync(UpdateProfileRequestDto dto) =>
        PutAuthAsync<ApiResponse<UserProfileDto>>("api/users/profile", dto);

    public Task<ApiResponse?> DeleteConversationAsync(Guid conversationId) =>
        DeleteAuthAsync<ApiResponse>($"api/conversations/{conversationId}");

    public Task<ApiResponse<PagedResult<MessageDto>>?> GetMessagesAsync(
        Guid conversationId, int pageIndex = 1, int pageSize = 30, string? keyword = null)
    {
        string url = $"api/conversations/{conversationId}/messages?pageIndex={pageIndex}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(keyword)) url += $"&keyword={Uri.EscapeDataString(keyword)}";
        return GetAuthAsync<ApiResponse<PagedResult<MessageDto>>>(url);
    }

    public Task<ApiResponse<MessageDto>?> SendMessageAsync(SendMessageDto dto) =>
        PostAuthAsync<ApiResponse<MessageDto>>("api/conversations/messages", dto);

    public Task<ApiResponse?> EditMessageAsync(Guid messageId, EditMessageDto dto) =>
        PutAuthAsync<ApiResponse>($"api/conversations/messages/{messageId}", dto);

    public Task<ApiResponse?> RevokeMessageAsync(Guid messageId) =>
        PostAuthAsync<ApiResponse>($"api/conversations/messages/{messageId}/revoke", null);

    public Task<ApiResponse<MessagePinChangedDto>?> SetMessagePinnedAsync(Guid messageId, bool isPinned) =>
        PutAuthAsync<ApiResponse<MessagePinChangedDto>>(
            $"api/conversations/messages/{messageId}/pin",
            new SetMessagePinDto { IsPinned = isPinned });

    public Task<ApiResponse<List<MessageDto>>?> ForwardMessageAsync(Guid messageId, IEnumerable<Guid> targetConversationIds) =>
        PostAuthAsync<ApiResponse<List<MessageDto>>>(
            $"api/conversations/messages/{messageId}/forward",
            new ForwardMessageDto { TargetConversationIds = targetConversationIds.Distinct().ToList() });

    public Task<ApiResponse<MessageReactionsChangedDto>?> ToggleMessageReactionAsync(Guid messageId, string emoji) =>
        PostAuthAsync<ApiResponse<MessageReactionsChangedDto>>(
            $"api/conversations/messages/{messageId}/reactions",
            new SetMessageReactionDto { Emoji = emoji });

    public Task<ApiResponse?> HideMessageForMeAsync(Guid messageId) =>
        DeleteAuthAsync<ApiResponse>($"api/conversations/messages/{messageId}");

    public Task<ApiResponse<SharedContentDto>?> GetSharedContentAsync(Guid conversationId) =>
        GetAuthAsync<ApiResponse<SharedContentDto>>($"api/conversations/{conversationId}/shared-content");

    // ─────────────────────────────────────────────────────────────────────────
    // Files
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<AttachmentDto>?> UploadFileAsync(
        Guid conversationId, string filePath)
    {
        SetAuthHeader();
        using MultipartFormDataContent form = new();
        byte[] bytes = await File.ReadAllBytesAsync(filePath);
        string mimeType = GetMimeType(filePath);
        ByteArrayContent fileContent = new(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        HttpResponseMessage res = await _http.PostAsync(
            $"{_session.ServerUrl}/api/files/upload?conversationId={conversationId}", form);
        return await ParseResponseAsync<ApiResponse<AttachmentDto>>(res);
    }

    public async Task<ApiResponse<AttachmentDto>?> UploadVoiceAsync(
        Guid conversationId, string filePath, int durationSeconds)
    {
        SetAuthHeader();
        using MultipartFormDataContent form = new();
        
        byte[] bytes = Array.Empty<byte>();
        for (int retry = 0; retry < 5; retry++)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ms = new MemoryStream();
                await fs.CopyToAsync(ms);
                bytes = ms.ToArray();
                break;
            }
            catch (IOException) when (retry < 4)
            {
                await Task.Delay(100);
            }
        }

        ByteArrayContent fileContent = new(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        HttpResponseMessage res = await _http.PostAsync(
            $"{_session.ServerUrl}/api/files/upload-voice?conversationId={conversationId}&durationSeconds={durationSeconds}", form);
        return await ParseResponseAsync<ApiResponse<AttachmentDto>>(res);
    }

    public async Task<byte[]?> DownloadFileAsync(Guid attachmentId)
    {
        SetAuthHeader();
        HttpResponseMessage res = await _http.GetAsync($"{_session.ServerUrl}/api/files/{attachmentId}/download");
        if (res.IsSuccessStatusCode)
            return await res.Content.ReadAsByteArrayAsync();
        return null;
    }

    public async Task<ApiResponse<string>?> UploadAvatarAsync(string filePath)
    {
        await EnsureValidTokenAsync();
        SetAuthHeader();
        using MultipartFormDataContent form = new();
        byte[] bytes = await File.ReadAllBytesAsync(filePath);
        ByteArrayContent fileContent = new(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(GetMimeType(filePath));
        form.Add(fileContent, "file", Path.GetFileName(filePath));

        HttpResponseMessage res = await _http.PostAsync($"{_session.ServerUrl}/api/files/avatar", form);
        return await ParseResponseAsync<ApiResponse<string>>(res);
    }

    public string GetFileUrl(string relativePath) =>
        $"{_session.ServerUrl}/{relativePath.TrimStart('/')}";

    // ─────────────────────────────────────────────────────────────────────────
    // Groups
    // ─────────────────────────────────────────────────────────────────────────
    public Task<ApiResponse<GroupDto>?> CreateGroupAsync(CreateGroupDto dto) =>
        PostAuthAsync<ApiResponse<GroupDto>>("api/groups", dto);

    public Task<ApiResponse<GroupDto>?> GetGroupAsync(Guid groupId) =>
        GetAuthAsync<ApiResponse<GroupDto>>($"api/groups/{groupId}");

    public Task<ApiResponse?> UpdateGroupAsync(Guid groupId, UpdateGroupDto dto) =>
        PutAuthAsync<ApiResponse>($"api/groups/{groupId}", dto);

    public Task<ApiResponse?> AddGroupMembersAsync(Guid groupId, AddGroupMembersDto dto) =>
        PostAuthAsync<ApiResponse>($"api/groups/{groupId}/members", dto);

    public Task<ApiResponse?> RemoveGroupMemberAsync(Guid groupId, Guid userId) =>
        DeleteAuthAsync<ApiResponse>($"api/groups/{groupId}/members/{userId}");

    public Task<ApiResponse?> UpdateMemberRoleAsync(Guid groupId, UpdateMemberRoleDto dto) =>
        PutAuthAsync<ApiResponse>($"api/groups/{groupId}/members/role", dto);

    public Task<ApiResponse?> LeaveGroupAsync(Guid groupId) =>
        PostAuthAsync<ApiResponse>($"api/groups/{groupId}/leave", null);

    // ─────────────────────────────────────────────────────────────────────────
    // Calls
    // ─────────────────────────────────────────────────────────────────────────
    public Task<ApiResponse<PagedResult<CallInfoDto>>?> GetCallHistoryAsync(
        Guid? conversationId = null, string? search = null, string? filter = null, int pageIndex = 1, int pageSize = 50)
    {
        string url = $"api/calls?pageIndex={pageIndex}&pageSize={pageSize}";
        if (conversationId.HasValue) url += $"&conversationId={conversationId.Value}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search.Trim())}";
        if (!string.IsNullOrWhiteSpace(filter)) url += $"&filter={Uri.EscapeDataString(filter.Trim())}";
        return GetAuthAsync<ApiResponse<PagedResult<CallInfoDto>>>(url);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AI Assistant
    // ─────────────────────────────────────────────────────────────────────────
    public Task<ApiResponse<Guid>?> GetAiConversationAsync() =>
        GetAuthAsync<ApiResponse<Guid>>("api/ai/conversation");

    public Task<ApiResponse<MessageDto>?> SendAiMessageAsync(Guid conversationId, string message) =>
        PostAuthAsync<ApiResponse<MessageDto>>("api/ai/chat",
            new { ConversationId = conversationId, Message = message });

    // ─────────────────────────────────────────────────────────────────────────
    // Internal helpers
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<T?> PostAsync<T>(string endpoint, object? body)
    {
        try
        {
            HttpResponseMessage res = await _http.PostAsJsonAsync($"{_session.ServerUrl}/{endpoint}", body, JsonOpts);
            return await ParseResponseAsync<T>(res);
        }
        catch (Exception)
        {
            return CreateErrorResponse<T>($"Không thể kết nối đến máy chủ ({_session.ServerUrl}). Vui lòng kiểm tra lại địa chỉ hoặc đảm bảo Server đã được bật!");
        }
    }

    private async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            HttpResponseMessage res = await _http.GetAsync($"{_session.ServerUrl}/{endpoint}");
            return await ParseResponseAsync<T>(res);
        }
        catch (Exception)
        {
            return CreateErrorResponse<T>($"Không thể kết nối đến máy chủ ({_session.ServerUrl}). Vui lòng kiểm tra lại địa chỉ hoặc đảm bảo Server đã được bật!");
        }
    }

    private async Task<T?> PostAuthAsync<T>(string endpoint, object? body)
    {
        try
        {
            await EnsureValidTokenAsync();
            SetAuthHeader();
            HttpResponseMessage res = await _http.PostAsJsonAsync($"{_session.ServerUrl}/{endpoint}", body, JsonOpts);
            return await ParseResponseAsync<T>(res);
        }
        catch (Exception)
        {
            return CreateErrorResponse<T>($"Không thể kết nối đến máy chủ ({_session.ServerUrl}). Vui lòng kiểm tra lại địa chỉ hoặc đảm bảo Server đã được bật!");
        }
    }

    private async Task<T?> GetAuthAsync<T>(string endpoint)
    {
        try
        {
            await EnsureValidTokenAsync();
            SetAuthHeader();
            HttpResponseMessage res = await _http.GetAsync($"{_session.ServerUrl}/{endpoint}");
            return await ParseResponseAsync<T>(res);
        }
        catch (Exception)
        {
            return CreateErrorResponse<T>($"Không thể kết nối đến máy chủ ({_session.ServerUrl}). Vui lòng kiểm tra lại địa chỉ hoặc đảm bảo Server đã được bật!");
        }
    }

    private async Task<T?> PutAuthAsync<T>(string endpoint, object? body)
    {
        try
        {
            await EnsureValidTokenAsync();
            SetAuthHeader();
            HttpResponseMessage res = await _http.PutAsJsonAsync($"{_session.ServerUrl}/{endpoint}", body, JsonOpts);
            return await ParseResponseAsync<T>(res);
        }
        catch (Exception)
        {
            return CreateErrorResponse<T>($"Không thể kết nối đến máy chủ ({_session.ServerUrl}). Vui lòng kiểm tra lại địa chỉ hoặc đảm bảo Server đã được bật!");
        }
    }

    private async Task<T?> DeleteAuthAsync<T>(string endpoint)
    {
        try
        {
            await EnsureValidTokenAsync();
            SetAuthHeader();
            HttpResponseMessage res = await _http.DeleteAsync($"{_session.ServerUrl}/{endpoint}");
            return await ParseResponseAsync<T>(res);
        }
        catch (Exception)
        {
            return CreateErrorResponse<T>($"Không thể kết nối đến máy chủ ({_session.ServerUrl}). Vui lòng kiểm tra lại địa chỉ hoặc đảm bảo Server đã được bật!");
        }
    }

    private static T? CreateErrorResponse<T>(string message)
    {
        if (typeof(T) == typeof(ApiResponse))
        {
            return (T?)(object)ApiResponse.Fail(message);
        }
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            var res = Activator.CreateInstance(typeof(T));
            typeof(T).GetProperty("Success")?.SetValue(res, false);
            typeof(T).GetProperty("Message")?.SetValue(res, message);
            return (T?)res;
        }
        return default;
    }

    private async Task<T?> ParseResponseAsync<T>(HttpResponseMessage res)
    {
        string json = await res.Content.ReadAsStringAsync();
        try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
        catch { return default; }
    }

    private void SetAuthHeader()
    {
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _session.AccessToken);
    }

    private async Task EnsureValidTokenAsync()
    {
        if (string.IsNullOrEmpty(_session.AccessToken)) return;
        // Try to parse expiry
        try
        {
            string payload = _session.AccessToken.Split('.')[1];
            // Pad Base64
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out JsonElement expEl))
            {
                long exp = expEl.GetInt64();
                DateTimeOffset expiry = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expiry > DateTimeOffset.UtcNow.AddSeconds(30))
                    return; // Token still valid
            }
        }
        catch { return; }

        // Token expired or close to expiry — refresh
        if (!string.IsNullOrEmpty(_session.RefreshToken))
        {
            ApiResponse<LoginResponseDto>? result = await PostAsync<ApiResponse<LoginResponseDto>>(
                "api/auth/refresh", new RefreshTokenRequestDto { RefreshToken = _session.RefreshToken });
            if (result?.Success == true && result.Data is not null)
                _session.UpdateAccessToken(result.Data.AccessToken, result.Data.RefreshToken);
        }
    }

    private static string GetMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
