using Chatting.Client.Services;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace Chatting.Client.Controls;

/// <summary>Draws normal message text mixed with image-backed [:emoji_n:] tokens.</summary>
public sealed class EmojiTextView : Control
{
    private static readonly Regex TokenRegex = new(@"\[:emoji_(?<index>\d{1,2}):\]", RegexOptions.Compiled);
    private readonly List<(string? Text, int? Emoji)> _parts = [];
    private int _maxWidth = 300;
    private const int EmojiSize = 24;

    public EmojiTextView()
    {
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 9.5F);
        ForeColor = Color.FromArgb(15, 23, 42);
        // This custom WinForms control cannot use Color.Transparent.
        // MessageBubbleControl keeps this color in sync with its bubble.
        BackColor = Color.FromArgb(241, 245, 249);
        TabStop = false;
    }

    public void SetMessage(string content, int maxWidth)
    {
        _maxWidth = Math.Max(80, maxWidth);
        _parts.Clear();
        int start = 0;
        foreach (Match match in TokenRegex.Matches(content))
        {
            if (match.Index > start) _parts.Add((content[start..match.Index], null));
            if (int.TryParse(match.Groups["index"].Value, out int index) && index is >= 0 and < EmojiSprite.Count)
                _parts.Add((null, index));
            else
                _parts.Add((match.Value, null));
            start = match.Index + match.Length;
        }
        if (start < content.Length || _parts.Count == 0) _parts.Add((content[start..], null));
        Size = MeasureContent();
        Invalidate();
    }

    private Size MeasureContent()
    {
        using Bitmap bitmap = new(1, 1);
        using Graphics graphics = Graphics.FromImage(bitmap);
        int x = 0, y = 0, lineHeight = Math.Max(Font.Height + 3, EmojiSize), widest = 0;
        foreach (var part in _parts)
        {
            if (part.Emoji is int)
            {
                if (x > 0 && x + EmojiSize > _maxWidth) { widest = Math.Max(widest, x); x = 0; y += lineHeight; }
                x += EmojiSize;
                continue;
            }
            foreach (string fragment in SplitText(part.Text ?? string.Empty))
            {
                Size size = TextRenderer.MeasureText(graphics, fragment, Font, Size.Empty, TextFormatFlags.NoPadding);
                if (fragment == "\n") { widest = Math.Max(widest, x); x = 0; y += lineHeight; continue; }
                if (x > 0 && x + size.Width > _maxWidth) { widest = Math.Max(widest, x); x = 0; y += lineHeight; }
                x += size.Width;
            }
        }
        return new Size(Math.Max(1, Math.Min(_maxWidth, Math.Max(widest, x))), y + lineHeight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        int x = 0, y = 0, lineHeight = Math.Max(Font.Height + 3, EmojiSize);
        foreach (var part in _parts)
        {
            if (part.Emoji is int emoji)
            {
                if (x > 0 && x + EmojiSize > Width) { x = 0; y += lineHeight; }
                EmojiSprite.Draw(e.Graphics, emoji, new Rectangle(x, y + (lineHeight - EmojiSize) / 2, EmojiSize, EmojiSize));
                x += EmojiSize;
                continue;
            }
            foreach (string fragment in SplitText(part.Text ?? string.Empty))
            {
                if (fragment == "\n") { x = 0; y += lineHeight; continue; }
                Size size = TextRenderer.MeasureText(e.Graphics, fragment, Font, Size.Empty, TextFormatFlags.NoPadding);
                if (x > 0 && x + size.Width > Width) { x = 0; y += lineHeight; }
                TextRenderer.DrawText(e.Graphics, fragment, Font, new Point(x, y + (lineHeight - Font.Height) / 2), ForeColor, TextFormatFlags.NoPadding);
                x += size.Width;
            }
        }
    }

    private static IEnumerable<string> SplitText(string value)
    {
        foreach (Match match in Regex.Matches(value, @"\n|\s+|[^\s]+")) yield return match.Value;
    }
}
