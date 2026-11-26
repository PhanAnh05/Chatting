namespace Chatting.Server.Services;

public class FileStorageService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public string RootPath =>
        Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                     "uploads");

    public bool IsAllowedExtension(string fileName)
    {
        string allowed = _config["FileStorage:AllowedExtensions"]
            ?? ".jpg,.jpeg,.png,.gif,.webp,.mp4,.mp3,.wav,.ogg,.pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.txt,.zip,.rar,.7z";
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return allowed.Split(',').Contains(ext);
    }

    public long MaxFileSizeBytes =>
        long.Parse(_config["FileStorage:MaxFileSizeBytes"] ?? "52428800");

    public async Task<(string StoredFileName, string RelativePath)> SaveFileAsync(
        Stream fileStream, string originalFileName, string subFolder = "files")
    {
        string ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        string storedName = $"{Guid.NewGuid():N}{ext}";
        string folderPath = Path.Combine(RootPath, subFolder);
        Directory.CreateDirectory(folderPath);
        string fullPath = Path.Combine(folderPath, storedName);

        using FileStream fs = new(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fs);

        return (storedName, $"uploads/{subFolder}/{storedName}");
    }

    public string GetAbsolutePath(string relativePath)
        => Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                        relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    public void DeleteFile(string relativePath)
    {
        string abs = GetAbsolutePath(relativePath);
        if (File.Exists(abs)) File.Delete(abs);
    }
}
