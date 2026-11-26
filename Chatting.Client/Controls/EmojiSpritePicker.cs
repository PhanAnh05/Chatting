using Chatting.Client.Services;
using System.Drawing.Drawing2D;

namespace Chatting.Client.Controls;

/// <summary>Pixel-precise 9 × 7 picker; avoids font glyphs and button image clipping.</summary>
public sealed class EmojiSpritePicker : Control
{
    private const int CellWidth = 48;
    private const int CellHeight = 44;
    private int _hoverIndex = -1;

    public event Action<int>? EmojiSelected;

    public EmojiSpritePicker()
    {
        Size = new Size(EmojiSprite.Columns * CellWidth, EmojiSprite.Rows * CellHeight);
        BackColor = Color.White;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        for (int index = 0; index < EmojiSprite.Count; index++)
        {
            int row = index / EmojiSprite.Columns;
            int col = index % EmojiSprite.Columns;
            Rectangle cell = new(col * CellWidth, row * CellHeight, CellWidth, CellHeight);
            if (index == _hoverIndex)
            {
                using Brush hover = new SolidBrush(Color.FromArgb(224, 231, 255));
                e.Graphics.FillRectangle(hover, cell);
            }

            // EmojiSprite has already cut this into an isolated, square 64 × 64 tile.
            // Render it square to avoid stretching the face in the picker.
            const int emojiSize = 36;
            EmojiSprite.Draw(
                e.Graphics,
                index,
                new Rectangle(
                    cell.X + (CellWidth - emojiSize) / 2,
                    cell.Y + (CellHeight - emojiSize) / 2,
                    emojiSize,
                    emojiSize));
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int index = GetIndex(e.Location);
        if (index != _hoverIndex) { _hoverIndex = index; Invalidate(); }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoverIndex >= 0) { _hoverIndex = -1; Invalidate(); }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        int index = GetIndex(e.Location);
        if (index >= 0) EmojiSelected?.Invoke(index);
    }

    private static int GetIndex(Point point)
    {
        int col = point.X / CellWidth;
        int row = point.Y / CellHeight;
        int index = row * EmojiSprite.Columns + col;
        return col is >= 0 and < EmojiSprite.Columns && row is >= 0 and < EmojiSprite.Rows && index < EmojiSprite.Count ? index : -1;
    }
}
