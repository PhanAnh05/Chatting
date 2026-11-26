using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Controls;

public partial class ConversationListItemControl : UserControl
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    public ConversationDto? Conversation { get; private set; }
    public bool IsSelected { get; private set; }

    public event Action<ConversationDto>? ItemClicked;
    public event Action<ConversationDto>? AvatarClicked;
    private int _avatarLoadVersion;
    private bool _avatarInitialized;
    private string? _avatarUrl;
    private string? _avatarTitle;
    private ConversationType _avatarType;
    private bool _avatarIsAi;

    private static readonly Font TitleBoldFont = new("Segoe UI", 9.75F, FontStyle.Bold);
    private static readonly Font TitleRegularFont = new("Segoe UI", 9.75F, FontStyle.Regular);

    private bool _isAi = false;
    private Color _normalBg = Color.FromArgb(18, 24, 38);
    private Color _hoverBg = Color.FromArgb(30, 41, 59);
    private Color _selectedBg = Color.White;
    private bool _darkTheme = true;

    public ConversationListItemControl()
    {
        InitializeComponent();
        WireHoverAndClick(this);

        // Round corners
        pnlOnlineBadge.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlOnlineBadge.Width, pnlOnlineBadge.Height, pnlOnlineBadge.Width, pnlOnlineBadge.Height));
        picAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picAvatar.Width, picAvatar.Height, picAvatar.Width, picAvatar.Height));
        picAvatar.Click += (s, e) => { if (Conversation != null) AvatarClicked?.Invoke(Conversation); };
        Disposed += (_, _) => picAvatar.Image?.Dispose();
    }

    private void ApplyCardShape()
    {
        Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 14, 14));
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyCardShape();
        AdjustChildLayout();
    }

    private void AdjustChildLayout()
    {
        int w = Width;
        lblTime.Location = new Point(Math.Max(120, w - lblTime.Width - 10), 14);
        lblUnreadBadge.Location = new Point(Math.Max(120, w - lblUnreadBadge.Width - 10), 36);

        if (_isAi)
        {
            int titleW = TextRenderer.MeasureText(lblTitle.Text, lblTitle.Font).Width;
            lblTitle.Width = Math.Min(titleW + 4, Math.Max(50, w - 68 - lblAiBadge.Width - lblTime.Width - 20));
            lblAiBadge.Location = new Point(lblTitle.Left + lblTitle.Width + 4, lblTitle.Top + 2);
        }
        else
        {
            lblTitle.Width = Math.Max(50, w - 68 - lblTime.Width - 14);
        }

        lblLastMessage.Width = Math.Max(50, w - 68 - (lblUnreadBadge.Visible ? lblUnreadBadge.Width + 14 : 14));
    }

    private void WireHoverAndClick(Control ctrl)
    {
        ctrl.Click += (s, e) =>
        {
            if (Conversation != null)
                ItemClicked?.Invoke(Conversation);
        };

        ctrl.MouseEnter += (s, e) =>
        {
            if (!IsSelected)
            {
                BackColor = _hoverBg;
                ApplyColors(false, true);
            }
        };

        ctrl.MouseLeave += (s, e) =>
        {
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                if (!IsSelected)
                {
                    BackColor = _normalBg;
                    ApplyColors(false, false);
                }
            }
        };

        foreach (Control child in ctrl.Controls)
        {
            WireHoverAndClick(child);
        }
    }

    public void SetData(ConversationDto conversation, bool isSelected, Guid currentUserId, string serverUrl)
    {
        Conversation = conversation;
        IsSelected = isSelected;
        BackColor = isSelected ? _selectedBg : _normalBg;
        if (Region is null) ApplyCardShape();

        // Title
        lblTitle.Text = conversation.Title;

        // Last message & time
        if (conversation.LastMessage != null)
        {
            var msg = conversation.LastMessage;
            string sender = msg.Type != MessageType.System && msg.SenderId == currentUserId ? "Bạn: " : "";
            string content = msg.Type switch
            {
                MessageType.Image => "[Hình ảnh]",
                MessageType.Video => "[Video]",
                MessageType.Voice => "[Tin nhắn thoại]",
                MessageType.Document => "[Tệp đính kèm]",
                MessageType.System => $"[Hệ thống: {msg.Content}]",
                _ => msg.Content ?? ""
            };

            if (msg.IsRevoked)
                content = "[Tin nhắn đã bị thu hồi]";

            lblLastMessage.Text = $"{sender}{content}";

            DateTime localCreatedAt = VietnamTime.FromUtc(msg.CreatedAt);
            if (localCreatedAt.Date == DateTime.Today)
                lblTime.Text = localCreatedAt.ToString("HH:mm");
            else
                lblTime.Text = localCreatedAt.ToString("dd/MM");
        }
        else
        {
            lblLastMessage.Text = "Chưa có tin nhắn nào";
            lblTime.Text = string.Empty;
        }

        // Unread badge
        if (conversation.UnreadCount > 0)
        {
            lblUnreadBadge.Text = conversation.UnreadCount > 99 ? "99+" : conversation.UnreadCount.ToString();
            lblUnreadBadge.Visible = true;
            lblUnreadBadge.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblUnreadBadge.Width, lblUnreadBadge.Height, 10, 10));
        }
        else
        {
            lblUnreadBadge.Visible = false;
        }

        // Detect AI conversation
        bool isAi = (conversation.Title != null && (conversation.Title.Contains("AI", StringComparison.OrdinalIgnoreCase) || conversation.Title.Contains("Chatting AI")))
                    || conversation.Members.Any(m => m.UserId == Guid.Parse("00000000-0000-0000-0000-000000000001"));
        _isAi = isAi;

        bool avatarChanged = !_avatarInitialized ||
            _avatarUrl != conversation.AvatarUrl ||
            _avatarTitle != conversation.Title ||
            _avatarType != conversation.Type ||
            _avatarIsAi != isAi;
        if (avatarChanged)
        {
            _avatarInitialized = true;
            _avatarUrl = conversation.AvatarUrl;
            _avatarTitle = conversation.Title;
            _avatarType = conversation.Type;
            _avatarIsAi = isAi;
            ++_avatarLoadVersion; // Ignore an older download after the avatar changes.
        }

        if (isAi)
        {
            lblTitle.Text = "Chatting AI";
            lblAiBadge.Visible = true;
            lblAiBadge.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, lblAiBadge.Width, lblAiBadge.Height, 6, 6));

            int titleW = TextRenderer.MeasureText(lblTitle.Text, lblTitle.Font).Width;
            lblTitle.Width = titleW + 4;
            lblAiBadge.Location = new Point(lblTitle.Left + lblTitle.Width + 4, lblTitle.Top + 2);
            lblAiBadge.BringToFront();

            if (avatarChanged)
                ReplaceAvatar(AiAvatarHelper.GetHologramStarAvatar(picAvatar.Width));
            pnlOnlineBadge.Visible = false;
        }
        else
        {
            lblTitle.Text = conversation.Title;
            lblAiBadge.Visible = false;

            // Online status (direct chat with friend)
            bool isOnline = false;
            if (conversation.Type == ConversationType.Direct)
            {
                var otherMember = conversation.Members.FirstOrDefault(m => m.UserId != currentUserId);
                if (otherMember != null && otherMember.IsOnline)
                    isOnline = true;
            }
            pnlOnlineBadge.Visible = isOnline;

            if (avatarChanged)
            {
                ReplaceAvatar(GenerateAvatarBitmap(conversation.Title ?? "?", conversation.Type == ConversationType.Group));
                _ = LoadAvatarAsync(conversation.AvatarUrl, serverUrl, _avatarLoadVersion);
            }
        }

        ApplyColors(isSelected, false);
        AdjustChildLayout();
    }

    public void ApplyTheme(bool dark)
    {
        _darkTheme = dark;
        _normalBg = dark ? Color.FromArgb(18, 24, 38) : Color.White;
        _hoverBg = dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
        _selectedBg = dark ? Color.FromArgb(30, 58, 95) : Color.FromArgb(219, 234, 254);
        BackColor = IsSelected ? _selectedBg : _normalBg;
        if (Conversation is not null)
            ApplyColors(IsSelected, false);
    }

    private void ReplaceAvatar(Image image)
    {
        Image? old = picAvatar.Image;
        picAvatar.Image = image;
        old?.Dispose();
    }

    private async Task LoadAvatarAsync(string? avatarUrl, string serverUrl, int version)
    {
        Image? image = await AvatarImageCache.GetAsync(serverUrl, avatarUrl);
        if (image is null || version != _avatarLoadVersion || IsDisposed) return;
        Bitmap rendered = AvatarImageCache.CreateCircularFit(image, picAvatar.Size, Color.FromArgb(30, 41, 59), Color.FromArgb(148, 163, 184));
        ReplaceAvatar(rendered);
    }

    private void ApplyColors(bool isSelected, bool isHover)
    {
        if (_isAi)
        {
            lblTitle.ForeColor = Color.FromArgb(167, 139, 250); // #A78BFA
            lblTitle.Font = TitleBoldFont;
            lblLastMessage.ForeColor = isSelected && !_darkTheme ? Color.FromArgb(71, 85, 105) : (isHover ? Color.FromArgb(203, 213, 225) : Color.FromArgb(148, 163, 184));
            lblTime.ForeColor = Color.FromArgb(100, 116, 139);
        }
        else if (isSelected)
        {
            lblTitle.ForeColor = _darkTheme ? Color.White : Color.FromArgb(15, 23, 42);
            lblLastMessage.ForeColor = _darkTheme ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);
            lblTime.ForeColor = _darkTheme ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
            lblTitle.Font = TitleBoldFont;
        }
        else
        {
            lblTitle.ForeColor = _darkTheme
                ? (isHover ? Color.White : Color.FromArgb(241, 245, 249))
                : Color.FromArgb(15, 23, 42);
            lblLastMessage.ForeColor = _darkTheme
                ? (isHover ? Color.FromArgb(203, 213, 225) : Color.FromArgb(148, 163, 184))
                : Color.FromArgb(100, 116, 139);
            lblTime.ForeColor = _darkTheme ? Color.FromArgb(100, 116, 139) : Color.FromArgb(148, 163, 184);
            lblTitle.Font = (Conversation?.UnreadCount ?? 0) > 0 ? TitleBoldFont : TitleRegularFont;
        }
    }

    private static Bitmap GenerateAvatarBitmap(string name, bool isGroup)
    {
        return IconHelper.GenerateAvatarBitmap(name, isGroup, 48);
    }
}
