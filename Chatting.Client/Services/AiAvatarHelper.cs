using System.Drawing;
using System.Drawing.Drawing2D;

namespace Chatting.Client.Services;

/// <summary>
/// Helper to generate pure vector GDI+ Hologram Cyber Glow Sparkle avatars for Chatting AI.
/// Zero external image dependencies, zero white edges, perfectly crisp at any resolution.
/// </summary>
public static class AiAvatarHelper
{
    private static readonly Dictionary<int, Bitmap> _cache = new();
    private static readonly object _lock = new();

    public static Bitmap GetRobotBitmap(int w, int h) => GetHologramStarAvatar(Math.Max(w, h));

    public static Bitmap GetHologramStarAvatar(int size)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(size, out var cached))
                return (Bitmap)cached.Clone();

            Bitmap bmp = new(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var rect = new Rectangle(1, 1, size - 2, size - 2);

                // 1. Hologram gradient background: Violet (#7C3AED) -> Hot Pink (#EC4899)
                Color c1 = Color.FromArgb(124, 58, 237);
                Color c2 = Color.FromArgb(236, 72, 153);
                using (var bgBrush = new LinearGradientBrush(rect, c1, c2, 135f))
                {
                    g.FillEllipse(bgBrush, rect);
                }

                // 2. Translucent reflective border ring
                using (var borderPen = new Pen(Color.FromArgb(180, 255, 255, 255), Math.Max(1.2f, size * 0.035f)))
                {
                    g.DrawEllipse(borderPen, rect);
                }

                // 3. Central 4-pointed sparkle star
                float cx = size / 2f;
                float cy = size / 2f;
                float radius = size * 0.24f;
                DrawAiStar(g, cx, cy, radius, Color.White);

                // 4. Sparkle dust dots
                using (var dustBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                {
                    float dSize = Math.Max(1.5f, size * 0.045f);
                    g.FillEllipse(dustBrush, cx - size * 0.20f, cy - size * 0.20f, dSize, dSize);
                    g.FillEllipse(dustBrush, cx + size * 0.18f, cy + size * 0.18f, dSize, dSize);
                }

                // 5. Secondary mini sparkle star (top-right)
                float sRadius = radius * 0.38f;
                DrawAiStar(g, cx + size * 0.21f, cy - size * 0.19f, sRadius, Color.FromArgb(240, 255, 255, 255));
            }

            _cache[size] = (Bitmap)bmp.Clone();
            return bmp;
        }
    }

    /// <summary>
    /// Vector 4-point sparkle star with smooth inward curves.
    /// </summary>
    public static void DrawAiStar(Graphics g, float cx, float cy, float radius, Color color)
    {
        using var path = new GraphicsPath();
        float inner = radius * 0.28f; // Soft curvy curvature

        PointF[] points = new PointF[]
        {
            new(cx, cy - radius),
            new(cx + inner, cy - inner),
            new(cx + radius, cy),
            new(cx + inner, cy + inner),
            new(cx, cy + radius),
            new(cx - inner, cy + inner),
            new(cx - radius, cy),
            new(cx - inner, cy - inner)
        };

        path.AddPolygon(points);
        using var brush = new SolidBrush(color);
        g.FillPath(brush, path);
    }
}
