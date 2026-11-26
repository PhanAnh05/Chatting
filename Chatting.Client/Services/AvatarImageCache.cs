using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace Chatting.Client.Services;

/// <summary>Caches downloaded avatars and renders them inside a circle without cropping.</summary>
public static class AvatarImageCache
{
    private static readonly ConcurrentDictionary<string, Image> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public static async Task<Image?> GetAsync(string serverUrl, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) return null;
        string url = avatarUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? avatarUrl : $"{serverUrl.TrimEnd('/')}/{avatarUrl.TrimStart('/')}";
        if (Cache.TryGetValue(url, out Image? cached)) return cached;
        try
        {
            byte[] bytes = await Http.GetByteArrayAsync(url);
            using MemoryStream ms = new(bytes);
            using Image loaded = Image.FromStream(ms);
            Image copy = new Bitmap(loaded);
            return Cache.GetOrAdd(url, copy);
        }
        catch { return null; }
    }

    public static Bitmap CreateCircularFit(Image image, Size size, Color background, Color border)
    {
        int width = Math.Max(1, size.Width), height = Math.Max(1, size.Height);
        Bitmap output = new(width, height);
        using Graphics g = Graphics.FromImage(output);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        using GraphicsPath clip = new();
        clip.AddEllipse(1, 1, width - 3, height - 3);
        g.SetClip(clip);
        g.Clear(background);
        float scale = Math.Min((width - 4f) / image.Width, (height - 4f) / image.Height);
        float drawWidth = image.Width * scale, drawHeight = image.Height * scale;
        g.DrawImage(image, (width - drawWidth) / 2f, (height - drawHeight) / 2f, drawWidth, drawHeight);
        g.ResetClip();
        using Pen pen = new(border, 1.2f);
        g.DrawEllipse(pen, 1, 1, width - 3, height - 3);
        return output;
    }
}
