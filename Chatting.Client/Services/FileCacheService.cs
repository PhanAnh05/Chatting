using Chatting.Shared.DTOs;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Chatting.Client.Services;

/// <summary>
/// Quản lý bộ nhớ đệm (Cache) tệp đính kèm và mở tệp an toàn trên máy khách.
/// </summary>
public class FileCacheService
{
    private static FileCacheService? _instance;
    public static FileCacheService Instance => _instance ??= new FileCacheService();
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> CacheLocks = new();

    public string CacheDirectory { get; }

    public FileCacheService()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        CacheDirectory = Path.Combine(localAppData, "ChattingLAN", "Cache");
        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
    }

    /// <summary>
    /// Tạo tên tệp cache an toàn theo AttachmentId và tên gốc.
    /// </summary>
    public string GetLocalCachePath(AttachmentDto att)
    {
        string ext = Path.GetExtension(att.OriginalFileName);
        string cleanBase = Path.GetFileNameWithoutExtension(att.OriginalFileName);
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            cleanBase = cleanBase.Replace(c, '_');
        }
        string fileName = $"{att.Id:N}_{cleanBase}{ext}";
        return Path.Combine(CacheDirectory, fileName);
    }

    /// <summary>
    /// Kiểm tra tệp đã tồn tại trong cache và kích thước hợp lệ chưa.
    /// </summary>
    public bool IsCached(AttachmentDto att, out string cachedPath)
    {
        cachedPath = GetLocalCachePath(att);
        if (File.Exists(cachedPath))
        {
            FileInfo fi = new(cachedPath);
            if (att.FileSize <= 0 || fi.Length == att.FileSize)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Đảm bảo tệp đã được tải về máy và lưu trong cache cục bộ.
    /// </summary>
    public async Task<string?> EnsureCachedAsync(AttachmentDto att, string serverUrl, ApiClient apiClient)
    {
        string localPath = GetLocalCachePath(att);
        SemaphoreSlim cacheLock = CacheLocks.GetOrAdd(att.Id, _ => new SemaphoreSlim(1, 1));
        await cacheLock.WaitAsync();
        try
        {
            if (IsCached(att, out _))
                return localPath;

            // Remove an invalid file left by an interrupted earlier download before retrying.
            if (File.Exists(localPath))
                File.Delete(localPath);

            // 1. Tải qua ApiClient với xác thực JWT
            byte[]? data = await apiClient.DownloadFileAsync(att.Id);

            // 2. Nếu thất bại, thử tải qua URL raw trực tiếp trên server LAN
            if (data == null || data.Length == 0)
            {
                using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(30) };
                string rawUrl = $"{serverUrl.TrimEnd('/')}/api/files/{att.Id}/raw";
                var res = await http.GetAsync(rawUrl);
                if (res.IsSuccessStatusCode)
                {
                    data = await res.Content.ReadAsByteArrayAsync();
                }
                else if (!string.IsNullOrEmpty(att.RelativePath))
                {
                    string staticUrl = $"{serverUrl.TrimEnd('/')}/{att.RelativePath.TrimStart('/')}";
                    var resStatic = await http.GetAsync(staticUrl);
                    if (resStatic.IsSuccessStatusCode)
                    {
                        data = await resStatic.Content.ReadAsByteArrayAsync();
                    }
                }
            }

            if (data != null && data.Length > 0 && (att.FileSize <= 0 || data.LongLength == att.FileSize))
            {
                return await WriteCacheAtomicallyAsync(localPath, data) ? localPath : null;
            }
        }
        catch
        {
            // Bỏ qua lỗi và trả về null nếu không kết nối được
        }

        finally
        {
            cacheLock.Release();
        }

        return null;
    }

    /// <summary>
    /// Stores a file the local user has just uploaded. The write is atomic so a media
    /// preview can never read a partly written cache file.
    /// </summary>
    public async Task<bool> CacheLocalFileAsync(AttachmentDto att, string sourcePath)
    {
        if (!File.Exists(sourcePath))
            return false;

        FileInfo source = new(sourcePath);
        if (att.FileSize > 0 && source.Length != att.FileSize)
            return false;

        string localPath = GetLocalCachePath(att);
        SemaphoreSlim cacheLock = CacheLocks.GetOrAdd(att.Id, _ => new SemaphoreSlim(1, 1));
        await cacheLock.WaitAsync();
        try
        {
            if (IsCached(att, out _))
                return true;

            byte[] data = await File.ReadAllBytesAsync(sourcePath);
            return await WriteCacheAtomicallyAsync(localPath, data);
        }
        catch
        {
            return false;
        }
        finally
        {
            cacheLock.Release();
        }
    }

    private static async Task<bool> WriteCacheAtomicallyAsync(string localPath, byte[] data)
    {
        string tempPath = $"{localPath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllBytesAsync(tempPath, data);
            File.Move(tempPath, localPath, overwrite: true);
            return true;
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Kiểm tra định dạng tệp có nguy cơ thực thi gây hại không.
    /// </summary>
    public static bool IsDangerousExtension(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        string[] dangerous = { ".exe", ".bat", ".cmd", ".msi", ".vbs", ".ps1", ".scr", ".jar", ".reg", ".hta", ".com" };
        return dangerous.Contains(ext);
    }

    /// <summary>
    /// Mở tệp bằng ứng dụng mặc định của hệ điều hành.
    /// </summary>
    public bool OpenFileWithDefaultApp(string filePath, out string? errorMessage)
    {
        errorMessage = null;

        if (!File.Exists(filePath))
        {
            errorMessage = "Tệp tin không tồn tại trên máy tính.";
            return false;
        }

        if (IsDangerousExtension(filePath))
        {
            errorMessage = "Vì lý do bảo mật, hệ thống không tự động mở tệp chương trình hoặc tập lệnh này. Vui lòng lưu tệp và tự mở nếu tin cậy.";
            return false;
        }

        try
        {
            ProcessStartInfo psi = new(filePath)
            {
                UseShellExecute = true
            };
            Process.Start(psi);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Không thể mở tệp bằng ứng dụng mặc định: {ex.Message}";
            return false;
        }
    }
}
