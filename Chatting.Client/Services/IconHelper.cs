using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Chatting.Client.Services;

/// <summary>
/// Centralized icon system for LAN Messenger.
/// Renders crisp vector-based icons using Segoe MDL2 Assets.
/// Adheres strictly to the dark navy theme:
/// - Background normal: #1E293B
/// - Background hover: #263B5B
/// - Background active: #2563EB
/// - Icon default: #CBD5E1
/// - Icon hover: #60A5FA
/// - Active icon: #FFFFFF
/// - Status online: #22C55E
/// - Danger / delete: #EF4444
/// - AI accent: #8B5CF6
/// </summary>
public static class IconHelper
{
    // ─── Segoe MDL2 Assets glyph code-points ─────────────────────────────

    // Navigation & Header
    public const string Search = "\uE721";          // Search
    public const string Phone = "\uE717";           // Phone / Voice call
    public const string PhoneOff = "\uE778";        // Phone hangup / End call
    public const string PhoneMissed = "\uE119";     // Missed call
    public const string Video = "\uE714";           // Video camera / Video call
    public const string VideoOff = "\uE25B";        // Video camera off
    public const string Attach = "\uE723";          // Paperclip / Attach
    public const string More = "\uE712";            // Ellipsis / More options
    public const string MoreHorizontal = "\uE10C";  // Three dots horizontal

    // Left Navigation Strip
    public const string Chat = "\uE8BD";           // Message / Chat bubble
    public const string ChatBubbles = "\uE8F2";    // Multiple chat bubbles
    public const string Files = "\uE8A5";          // Files / Documents
    public const string Settings = "\uE713";       // Settings / Gear
    public const string Palette = "\uE790";        // Color palette / Theme

    // Account & User Management
    public const string Contact = "\uE77B";        // User / Contact
    public const string UserInfo = "\uE779";       // User profile
    public const string People = "\uE716";         // Group / Users
    public const string AddUser = "\uE8FA";        // Add friend
    public const string AddGroup = "\uE902";       // Create group
    public const string Logout = "\uE8AC";         // Sign out / Exit

    // Message Input Bar & Actions
    public const string Emoji = "\uE899";          // Smiley / Emoji
    public const string Mic = "\uE720";            // Microphone
    public const string MicOff = "\uF781";         // Microphone mute
    public const string StopRecord = "\uE71A";     // Stop square
    public const string Send = "\uE724";           // Send arrow

    // Context Menu & Message Actions
    public const string Copy = "\uE8C8";           // Copy
    public const string Edit = "\uE70F";           // Pencil / Edit
    public const string Reply = "\uE97A";          // Reply arrow
    public const string Forward = "\uE72A";        // Forward arrow
    public const string Delete = "\uE74D";         // Trash / Delete
    public const string Pin = "\uE718";            // Pin
    public const string Unpin = "\uE77A";          // Unpin
    public const string Star = "\uE734";           // Star favorite
    public const string StarFilled = "\uE735";     // Star filled
    public const string Refresh = "\uE72C";        // Refresh
    public const string Block = "\uF140";          // Block contact
    public const string Info = "\uE946";           // Info circle
    public const string Download = "\uE896";       // Download arrow
    public const string Open = "\uE8E5";           // Open in new / Launch
    public const string View = "\uE7B3";           // Eye / View
    public const string Lock = "\uE72E";           // Padlock
    public const string BellOff = "\uE7ED";        // Notification off
    public const string Close = "\uE711";          // Close X
    public const string Check = "\uE73E";          // Checkmark
    public const string Add = "\uE710";            // Plus sign

    // Media & Playback
    public const string Image = "\uE8B9";          // Picture / Image
    public const string Camera = "\uE722";         // Camera
    public const string Play = "\uE768";           // Play triangle
    public const string Pause = "\uE769";          // Pause
    public const string Document = "\uE8A5";       // Document / FileText
    public const string File = "\uE8A5";           // File
    public const string Folder = "\uE8B7";         // Folder
    public const string OpenFolder = "\uED25";     // Open folder / launch
    public const string Link = "\uE71B";           // Web link
    public const string MapPin = "\uE707";         // Location pin

    // Directional
    public const string ArrowUp = "\uE74A";        // Arrow up
    public const string ArrowDown = "\uE74B";      // Arrow down
    public const string ArrowLeft = "\uE72B";      // Arrow left
    public const string ArrowRight = "\uE72A";     // Arrow right
    public const string ChevronUp = "\uE70E";      // Chevron up
    public const string ChevronDown = "\uE70D";    // Chevron down

    // Window Management
    public const string Minimize = "\uE921";       // Minimize
    public const string Maximize = "\uE922";       // Maximize
    public const string Restore = "\uE923";        // Restore

    // AI & Chatbot
    public const string Bot = "\uE99A";            // Robot / Chatbot
    public const string Sparkle = "\uE9CE";        // Sparkles / AI Assistant
    public const string Lightbulb = "\uEA80";      // Lightbulb / Suggestions
    public const string GraduationCap = "\uE7BE";  // Learning / Education
    public const string Heart = "\uEB51";          // Heart / Favorite

    // ─── Theme Colors ────────────────────────────────────────────────────

    public static readonly Color IconDefault = Color.FromArgb(203, 213, 225);       // #CBD5E1
    public static readonly Color IconHover = Color.FromArgb(96, 165, 250);          // #60A5FA
    public static readonly Color IconActive = Color.FromArgb(255, 255, 255);        // #FFFFFF
    public static readonly Color BtnBgNormal = Color.FromArgb(30, 41, 59);          // #1E293B
    public static readonly Color BtnBgHover = Color.FromArgb(38, 59, 91);           // #263B5B
    public static readonly Color BtnBgActive = Color.FromArgb(37, 99, 235);         // #2563EB
    public static readonly Color StatusOnline = Color.FromArgb(34, 197, 94);        // #22C55E
    public static readonly Color StatusOffline = Color.FromArgb(100, 116, 139);     // #64748B
    public static readonly Color StatusDanger = Color.FromArgb(239, 68, 68);        // #EF4444
    public static readonly Color AiAccent = Color.FromArgb(139, 92, 246);           // #8B5CF6
    public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);

    // Light theme fallbacks
    public static readonly Color IconLight = Color.FromArgb(71, 85, 105);
    public static readonly Color BtnBgLight = Color.FromArgb(241, 245, 249);
    public static readonly Color BtnBgLightHover = Color.FromArgb(226, 232, 240);

    // ─── Rendering Methods ───────────────────────────────────────────────

    /// <summary>
    /// Render an MDL2 glyph to a square bitmap icon.
    /// </summary>
    public static Bitmap RenderIcon(string glyph, Color color, int size = 28, float fontSize = 18F)
    {
        var bmp = new Bitmap(size, size);
        bmp.SetResolution(96, 96);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        using var font = new Font("Segoe MDL2 Assets", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        TextRenderer.DrawText(g, glyph, font, new Rectangle(0, 0, size, size), color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        return bmp;
    }

    /// <summary>
    /// Sets an MDL2 icon image on a button, safely disposing previous image and clearing text.
    /// </summary>
    public static void SetButtonIcon(Button button, string glyph, Color color,
        int iconSize = 28, float fontSize = 18F, bool clearText = true)
    {
        var icon = RenderIcon(glyph, color, iconSize, fontSize);
        var previous = button.Image;
        button.Image = icon;
        previous?.Dispose();
        if (clearText)
        {
            button.Text = string.Empty;
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }
    }

    /// <summary>
    /// Sets an MDL2 icon with a specified pixel size and color on a button, preserving button text and aligning to left/center.
    /// </summary>
    public static void SetButtonIcon(Button button, string glyph, int iconSize, Color color, ContentAlignment align = ContentAlignment.MiddleLeft)
    {
        float fontSize = Math.Max(8F, iconSize * 0.72F);
        var icon = RenderIcon(glyph, color, iconSize, fontSize);
        var previous = button.Image;
        button.Image = icon;
        previous?.Dispose();
        button.ImageAlign = align;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
    }

    /// <summary>
    /// Configures a navigation button with icon above text label.
    /// </summary>
    public static void SetNavButtonIcon(Button button, string glyph, string label, Color color,
        int iconSize = 24, float fontSize = 18F)
    {
        SetButtonIcon(button, glyph, color, iconSize, fontSize, clearText: false);
        button.Text = label;
        button.ForeColor = color;
        button.TextImageRelation = TextImageRelation.ImageAboveText;
        button.ImageAlign = ContentAlignment.MiddleCenter;
        button.TextAlign = ContentAlignment.BottomCenter;
        button.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        button.Padding = new Padding(0, 3, 0, 2);
        button.UseVisualStyleBackColor = false;
    }

    /// <summary>
    /// Sets an MDL2 glyph as text directly on a label.
    /// </summary>
    public static void SetLabelIcon(Label label, string glyph, Color color, float fontSize = 14F)
    {
        label.Font = new Font("Segoe MDL2 Assets", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        label.Text = glyph;
        label.ForeColor = color;
    }

    /// <summary>
    /// Sets a 16x16 MDL2 icon on a ToolStripMenuItem.
    /// </summary>
    public static void SetMenuItemIcon(ToolStripMenuItem item, string glyph, Color color, int iconSize = 16, float fontSize = 13F)
    {
        var prev = item.Image;
        item.Image = RenderIcon(glyph, color, iconSize, fontSize);
        prev?.Dispose();
    }

    /// <summary>
    /// Applies consistent toolbar button styling with colors per the reference image:
    /// - Normal: bg #1E293B, icon #CBD5E1
    /// - Hover: bg #263B5B, icon #60A5FA
    /// - Active: bg #2563EB, icon #FFFFFF
    /// - Calling: bg #22C55E, icon #FFFFFF
    /// </summary>
    public static void ApplyToolbarButtonStyle(Button button, string glyph, bool isDark,
        bool isActive = false, bool isCalling = false, int size = 28, float fontSize = 18F)
    {
        Color bg;
        Color fg;

        if (isCalling)
        {
            bg = StatusOnline;
            fg = IconActive;
        }
        else if (isActive)
        {
            bg = BtnBgActive;
            fg = IconActive;
        }
        else if (isDark)
        {
            bg = BtnBgNormal;
            fg = IconDefault;
        }
        else
        {
            bg = BtnBgLight;
            fg = IconLight;
        }

        button.BackColor = bg;
        button.FlatAppearance.BorderSize = 0;
        button.FlatStyle = FlatStyle.Flat;
        button.Cursor = Cursors.Hand;
        button.FlatAppearance.MouseOverBackColor = isCalling ? Color.FromArgb(16, 185, 129)
            : isActive ? Color.FromArgb(29, 78, 216)
            : isDark ? BtnBgHover : BtnBgLightHover;

        SetButtonIcon(button, glyph, fg, size, fontSize);
    }

    /// <summary>
    /// Wire interactive hover effect so the icon itself glows #60A5FA on hover when in normal state.
    /// </summary>
    public static void WireHoverIcon(Button button, string glyph, bool isDark, int size = 28, float fontSize = 18F)
    {
        button.MouseEnter += (s, e) =>
        {
            if (button.BackColor != BtnBgActive && button.BackColor != StatusOnline)
            {
                SetButtonIcon(button, glyph, IconHover, size, fontSize);
            }
        };
        button.MouseLeave += (s, e) =>
        {
            if (button.BackColor != BtnBgActive && button.BackColor != StatusOnline)
            {
                Color defaultColor = isDark ? IconDefault : IconLight;
                SetButtonIcon(button, glyph, defaultColor, size, fontSize);
            }
        };
    }

    /// <summary>
    /// Return file-type glyph code for MDL2.
    /// </summary>
    public static string GetFileTypeGlyph(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" => Image,
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".flv" or ".webm" => Video,
            ".mp3" or ".wav" or ".ogg" or ".flac" or ".aac" or ".wma" or ".m4a" => "\uE8D6", // Audio
            ".doc" or ".docx" or ".odt" or ".rtf" or ".txt" => Document,
            ".xls" or ".xlsx" or ".csv" or ".ods" => Document,
            ".pdf" => Document,
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => Folder,
            _ => Document
        };
    }

    /// <summary>
    /// Return file-type color for crisp visual distinction.
    /// </summary>
    public static Color GetFileTypeColor(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => Color.FromArgb(168, 85, 247), // Purple
            ".mp4" or ".avi" or ".mkv" or ".mov" => Color.FromArgb(59, 130, 246),                        // Blue
            ".mp3" or ".wav" or ".ogg" or ".m4a" => Color.FromArgb(236, 72, 153),                        // Pink
            ".doc" or ".docx" => Color.FromArgb(37, 99, 235),                                            // Word Blue
            ".xls" or ".xlsx" or ".csv" => Color.FromArgb(34, 197, 94),                                  // Excel Green
            ".pdf" => Color.FromArgb(239, 68, 68),                                                        // PDF Red
            ".zip" or ".rar" or ".7z" => Color.FromArgb(245, 158, 11),                                   // Orange
            _ => Color.FromArgb(148, 163, 184)
        };
    }

    /// <summary>
    /// Generates an avatar bitmap with user/group icon when no image is available.
    /// </summary>
    public static Bitmap GenerateAvatarBitmap(string name, bool isGroup, int size = 48)
    {
        Bitmap bmp = new(size, size);
        bmp.SetResolution(96, 96);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Color bg = isGroup ? Color.FromArgb(37, 99, 235) : Color.FromArgb(79, 70, 229);
        using (Brush brush = new SolidBrush(bg))
        {
            g.FillEllipse(brush, 0, 0, size - 1, size - 1);
        }

        string glyph = isGroup ? People : Contact;
        using var font = new Font("Segoe MDL2 Assets", size * 0.45F, FontStyle.Regular, GraphicsUnit.Pixel);
        TextRenderer.DrawText(g, glyph, font, new Rectangle(0, 0, size, size), Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        return bmp;
    }
}
