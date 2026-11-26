using Chatting.Shared.DTOs;
using System.Text.Json;

namespace Chatting.Client.Services;

/// <summary>
/// Singleton session state manager. Persists refresh token and user info
/// to appsettings.client.json for "Remember Me" support.
/// </summary>
public class SessionManager
{
    private static readonly string ConfigPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.client.json");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    // ── In-memory session state ───────────────────────────────────────────────
    public string ServerUrl { get; private set; } = "http://127.0.0.1:5000";
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public UserSessionDto? CurrentUser { get; private set; }
    public bool IsDarkMode { get; private set; }
    public bool IsLoggedIn => CurrentUser is not null && !string.IsNullOrEmpty(AccessToken);

    // ─────────────────────────────────────────────────────────────────────────
    // Constructor: load persisted config
    // ─────────────────────────────────────────────────────────────────────────
    public SessionManager()
    {
        LoadFromDisk();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void SetServerUrl(string url)
    {
        ServerUrl = url.TrimEnd('/');
        SaveConfig(c => c.ServerUrl = ServerUrl);
    }

    public void SetDarkMode(bool enabled)
    {
        IsDarkMode = enabled;
        SaveConfig(c => c.IsDarkMode = enabled);
    }

    public void SetSession(string accessToken, string refreshToken, UserSessionDto user)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        CurrentUser = user;
    }

    public void UpdateAccessToken(string newAccessToken, string newRefreshToken)
    {
        AccessToken = newAccessToken;
        RefreshToken = newRefreshToken;
    }

    public void SaveRememberMe(string email, string refreshToken)
    {
        SaveConfig(c =>
        {
            c.RememberMe = true;
            c.SavedEmail = email;
            c.SavedRefreshToken = refreshToken;
        });
    }

    public void ClearRememberMe()
    {
        SaveConfig(c =>
        {
            c.RememberMe = false;
            c.SavedEmail = string.Empty;
            c.SavedRefreshToken = string.Empty;
        });
    }

    public void ClearSession()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        CurrentUser = null;
    }

    public (bool RememberMe, string SavedEmail, string SavedRefreshToken) GetSavedCredentials()
    {
        ClientConfig cfg = ReadConfig();
        return (cfg.RememberMe, cfg.SavedEmail, cfg.SavedRefreshToken);
    }

    public bool IsUserBlocked(Guid userId)
    {
        var cfg = ReadConfig();
        return cfg.BlockedUserIds.Contains(userId);
    }

    public void BlockUser(Guid userId)
    {
        SaveConfig(c =>
        {
            if (!c.BlockedUserIds.Contains(userId))
                c.BlockedUserIds.Add(userId);
        });
    }

    public void UnblockUser(Guid userId)
    {
        SaveConfig(c =>
        {
            c.BlockedUserIds.Remove(userId);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void LoadFromDisk()
    {
        ClientConfig cfg = ReadConfig();
        ServerUrl = string.IsNullOrWhiteSpace(cfg.ServerUrl) ? "http://127.0.0.1:5000" : cfg.ServerUrl.TrimEnd('/');
        IsDarkMode = cfg.IsDarkMode;
    }

    private ClientConfig ReadConfig()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<ClientConfig>(json, JsonOpts) ?? new ClientConfig();
            }
        }
        catch { /* ignore */ }
        return new ClientConfig();
    }

    private void SaveConfig(Action<ClientConfig> modify)
    {
        ClientConfig cfg = ReadConfig();
        modify(cfg);
        try { File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, JsonOpts)); }
        catch { /* ignore write errors in portable deploy */ }
    }

    private class ClientConfig
    {
        public string ServerUrl { get; set; } = "http://127.0.0.1:5000";
        public bool RememberMe { get; set; }
        public string SavedEmail { get; set; } = string.Empty;
        public string SavedRefreshToken { get; set; } = string.Empty;
        public List<Guid> BlockedUserIds { get; set; } = new();
        public bool IsDarkMode { get; set; }
    }
}
