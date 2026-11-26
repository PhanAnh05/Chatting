using System.Drawing.Drawing2D;

namespace Chatting.Client.Services;

/// <summary>Maps the 9 × 7 emoji sheet to stable message tokens emoji_0 … emoji_62.</summary>
public static class EmojiSprite
{
    public const int Columns = 9;
    public const int Rows = 7;
    public const int Count = Columns * Rows;
    private const float ReferenceWidth = 780f;
    private const float ReferenceHeight = 520f;
    private const float CropSize = 64f;
    private const float FirstCropX = 44f;
    private const float FirstCropY = 16.5f;
    private const float ColumnStep = 78.25f;
    private const float RowStep = 70.5f;
    private static readonly Lazy<Bitmap?[]> Icons = new(CreateIcons);

    public static bool IsToken(string value) =>
        value.StartsWith("[:emoji_", StringComparison.Ordinal) &&
        value.EndsWith(":]", StringComparison.Ordinal) &&
        int.TryParse(value[8..^2], out int index) && index is >= 0 and < Count;

    public static string Token(int index) => $"[:emoji_{index}:]";

    public static bool TryGetIndex(string token, out int index)
    {
        index = -1;
        return IsToken(token) && int.TryParse(token[8..^2], out index);
    }

    public static Rectangle GetSourceRectangle(int index)
    {
        if (index is < 0 or >= Count) return Rectangle.Empty;
        int row = index / Columns;
        int col = index % Columns;
        return Rectangle.Round(new RectangleF(
            FirstCropX + col * ColumnStep,
            FirstCropY + row * RowStep,
            CropSize,
            CropSize));
    }

    public static Bitmap? CreateIcon(int index, int size)
    {
        if (size <= 0 || index is < 0 or >= Count || Icons.Value[index] is not Bitmap source) return null;
        Bitmap icon = new(size, size);
        using Graphics g = Graphics.FromImage(icon);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawImage(source, new Rectangle(0, 0, size, size));
        return icon;
    }

    public static void Draw(Graphics graphics, int index, Rectangle destination)
    {
        if (index is >= 0 and < Count && Icons.Value[index] is Bitmap icon)
            graphics.DrawImage(icon, destination);
    }

    /// <summary>
    /// Equivalent to drawing the requested source region into a 64 × 64 HTML canvas.
    /// Each output bitmap is independent, so scaling cannot reveal a neighbouring icon.
    /// </summary>
    private static Bitmap?[] CreateIcons()
    {
        var icons = new Bitmap?[Count];
        try
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "emojis-sprite.jpg");
            if (!File.Exists(path)) return icons;

            using var sprite = new Bitmap(path);
            float scaleX = sprite.Width / ReferenceWidth;
            float scaleY = sprite.Height / ReferenceHeight;
            for (int index = 0; index < Count; index++)
            {
                int row = index / Columns;
                int col = index % Columns;
                float cropX = (FirstCropX + col * ColumnStep) * scaleX;
                float cropY = (FirstCropY + row * RowStep) * scaleY;
                float cropWidth = CropSize * scaleX;
                float cropHeight = CropSize * scaleY;

                var icon = new Bitmap((int)CropSize, (int)CropSize);
                using Graphics g = Graphics.FromImage(icon);
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(
                    sprite,
                    [
                        new PointF(0, 0),
                        new PointF(CropSize, 0),
                        new PointF(0, CropSize)
                    ],
                    new RectangleF(cropX, cropY, cropWidth, cropHeight),
                    GraphicsUnit.Pixel);
                icons[index] = icon;
            }
        }
        catch
        {
            foreach (Bitmap? icon in icons) icon?.Dispose();
            return new Bitmap?[Count];
        }

        return icons;
    }
}
