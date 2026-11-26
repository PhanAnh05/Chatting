using Chatting.Client.Forms;
using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Controls;

public partial class MessageBubbleControl : UserControl
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    public MessageDto? MessageData { get; private set; }
    public bool IsMyMessage { get; private set; }

    public event Action<MessageDto>? OnEditRequested;
    public event Action<MessageDto>? OnRevokeRequested;
    public event Action<MessageDto>? OnHideForMeRequested;
    public event Action<MessageDto, string>? OnReactionRequested;
    public event Action<MessageDto>? OnPinRequested;
    public event Action<MessageDto>? OnForwardRequested;
    public event Action<AttachmentDto>? OnDownloadRequested;

    private readonly ToolStripMenuItem _menuItemPin = new() { Name = "menuItemPin", Size = new Size(220, 22) };
    private readonly ToolStripMenuItem _menuItemForward = new() { Name = "menuItemForward", Size = new Size(220, 22), Text = "Chuyển tiếp..." };

    private static readonly (string Emoji, string Label)[] ReactionChoices =
    [
        ("👍", "Thích"), ("❤️", "Yêu thích"), ("😂", "Haha"),
        ("😮", "Wow"), ("😢", "Buồn"), ("🎉", "Chúc mừng")
    ];
    private static readonly Font ReactionCaptionFont = new("Segoe UI", 8F, FontStyle.Regular);
    private static readonly Font ReactionEmojiFont = new("Segoe UI Emoji", 11F, FontStyle.Regular);
    private ToolStripControlHost? _reactionPickerHost;
    private readonly FlowLayoutPanel _reactionPanel = new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false,
        BackColor = Color.Transparent,
        Margin = Padding.Empty,
        Padding = Padding.Empty,
        Visible = false
    };

    private string _serverUrl = string.Empty;
    private Guid _currentUserId;
    private bool _isGroupChat;
    private bool _darkTheme;
    private ApiClient? _apiClient;
    private AttachmentDto? _primaryAttachment;
    private string? _cachedFilePath;
    private EmojiTextView? _emojiTextView;

    // Search match highlight state
    private System.Windows.Forms.Timer? _colorFadeTimer;
    private Color _fadeStartColor;
    private Color _fadeTargetColor;
    private int _fadeStep;
    private const int MaxFadeSteps = 8;
    private bool _isSearchMatch;
    private bool _isSearchActiveMatch;
    private string? _searchQuery;

    public MessageBubbleControl()
    {
        InitializeComponent();
        Panel reactionMenuPanel = new()
        {
            BackColor = SystemColors.Menu,
            Size = new Size(210, 48),
            Margin = Padding.Empty
        };
        Label reactionCaption = new()
        {
            AutoSize = false,
            Text = "Thả cảm xúc",
            Font = ReactionCaptionFont,
            ForeColor = SystemColors.MenuText,
            Location = new Point(7, 1),
            Size = new Size(196, 16)
        };
        FlowLayoutPanel reactionButtons = new()
        {
            AutoSize = false,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = SystemColors.Menu,
            Location = new Point(4, 19),
            Size = new Size(202, 27),
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        reactionMenuPanel.Controls.Add(reactionCaption);
        reactionMenuPanel.Controls.Add(reactionButtons);
        foreach ((string emoji, string label) in ReactionChoices)
        {
            Button reactionButton = new()
            {
                Text = emoji,
                AccessibleName = label,
                AccessibleDescription = $"Thả cảm xúc {label}",
                Font = ReactionEmojiFont,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(31, 26),
                Margin = new Padding(1, 0, 1, 0),
                Padding = Padding.Empty,
                Cursor = Cursors.Hand,
                Tag = emoji,
                TabStop = false
            };
            reactionButton.FlatAppearance.BorderSize = 0;
            reactionButton.Click += ReactionPickerButton_Click;
            reactionButtons.Controls.Add(reactionButton);
        }
        _reactionPickerHost = new ToolStripControlHost(reactionMenuPanel)
        {
            AutoSize = false,
            Size = reactionMenuPanel.Size,
            Margin = new Padding(2)
        };
        ctxMenu.Items.Insert(0, _reactionPickerHost);
        ctxMenu.Items.Insert(1, new ToolStripSeparator());
        ctxMenu.Items.Insert(5, new ToolStripSeparator());
        ctxMenu.Items.Insert(5, _menuItemForward);
        ctxMenu.Items.Insert(5, _menuItemPin);
        _menuItemPin.Click += (_, _) =>
        {
            if (MessageData is not null) OnPinRequested?.Invoke(MessageData);
        };
        _menuItemForward.Click += (_, _) =>
        {
            if (MessageData is not null) OnForwardRequested?.Invoke(MessageData);
        };
        Controls.Add(_reactionPanel);
        _reactionPanel.BringToFront();

        pnlBubble.BackColorChanged += (_, _) =>
        {
            if (_emojiTextView?.Visible == true)
            {
                _emojiTextView.BackColor = pnlBubble.BackColor;
            }
        };

        // Hook Voice Player global events
        VoicePlayerManager.Instance.OnProgress += VoicePlayer_OnProgress;
        VoicePlayerManager.Instance.OnStateChanged += VoicePlayer_OnStateChanged;
        VoicePlayerManager.Instance.OnStopped += VoicePlayer_OnStopped;
        UpdateVoicePlaybackButton(isPlaying: false);

        pnlBubble.Paint += PnlBubble_Paint;
        lblContent.Paint += LblContent_Paint;

        Disposed += (s, e) =>
        {
            VoicePlayerManager.Instance.OnProgress -= VoicePlayer_OnProgress;
            VoicePlayerManager.Instance.OnStateChanged -= VoicePlayer_OnStateChanged;
            VoicePlayerManager.Instance.OnStopped -= VoicePlayer_OnStopped;
            _colorFadeTimer?.Stop();
            _colorFadeTimer?.Dispose();
            _colorFadeTimer = null;
            picImage.Image?.Dispose();
            picSenderAvatar.Image?.Dispose();
        };
    }

    private void VoicePlayer_OnProgress(Guid attId, float progress, TimeSpan current, TimeSpan total)
    {
        if (_primaryAttachment?.Id == attId && IsHandleCreated)
        {
            try
            {
                BeginInvoke(() =>
                {
                    int barWidth = (int)(pnlVoiceProgress.Width * progress);
                    pnlVoiceBar.Width = Math.Clamp(barWidth, 0, pnlVoiceProgress.Width);
                    lblVoiceTime.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";
                });
            }
            catch { }
        }
    }

    private void VoicePlayer_OnStateChanged(Guid attId, bool isPlaying)
    {
        if (_primaryAttachment?.Id == attId && IsHandleCreated)
        {
            try
            {
                BeginInvoke(() =>
                {
                    UpdateVoicePlaybackButton(isPlaying);
                });
            }
            catch { }
        }
    }

    private void VoicePlayer_OnStopped(Guid attId)
    {
        if (_primaryAttachment?.Id == attId && IsHandleCreated)
        {
            try
            {
                BeginInvoke(() =>
                {
                    UpdateVoicePlaybackButton(isPlaying: false);
                    pnlVoiceBar.Width = 0;
                    int dur = _primaryAttachment.DurationSeconds ?? 0;
                    lblVoiceTime.Text = $"{dur / 60:D2}:{dur % 60:D2}";
                });
            }
            catch { }
        }
    }

    private void UpdateVoicePlaybackButton(bool isPlaying)
    {
        string action = isPlaying ? "Tạm dừng" : "Phát";
        IconHelper.SetButtonIcon(btnPlayVoice, isPlaying ? IconHelper.Pause : IconHelper.Play, Color.White, 20, 14F);
        btnPlayVoice.AccessibleName = $"{action} tin nhắn thoại";
        btnPlayVoice.AccessibleDescription = $"{action} tin nhắn thoại.";
        toolTipMessageBubble.SetToolTip(btnPlayVoice, $"{action} tin nhắn thoại");
    }

    public void SetData(MessageDto msg, Guid currentUserId, bool isGroupChat, string serverUrl, ApiClient? apiClient = null)
    {
        _colorFadeTimer?.Stop();
        _colorFadeTimer?.Dispose();
        _colorFadeTimer = null;
        _isSearchMatch = false;
        _isSearchActiveMatch = false;
        _searchQuery = null;

        MessageData = msg;
        _currentUserId = currentUserId;
        _isGroupChat = isGroupChat;
        IsMyMessage = msg.SenderId == currentUserId;
        _serverUrl = serverUrl;
        _apiClient = apiClient;
        _primaryAttachment = msg.Attachments.FirstOrDefault();

        // Context menu items visibility
        bool isSystemMessage = msg.Type == MessageType.System;
        bool canManageMessage = !isSystemMessage && !msg.IsRevoked && !msg.IsDeleted;
        ContextMenuStrip = isSystemMessage || msg.IsRevoked || msg.IsDeleted ? null : ctxMenu;
        menuItemEdit.Visible = !isSystemMessage && IsMyMessage && msg.Type == MessageType.Text && !msg.IsRevoked && !msg.IsDeleted;
        menuItemRevoke.Visible = !isSystemMessage && IsMyMessage && !msg.IsRevoked && !msg.IsDeleted;
        menuItemDelete.Visible = !isSystemMessage && !msg.IsRevoked && !msg.IsDeleted;
        if (_reactionPickerHost is not null)
            _reactionPickerHost.Visible = !isSystemMessage && !msg.IsRevoked && !msg.IsDeleted;
        menuItemCopy.Visible = !isSystemMessage && !string.IsNullOrEmpty(msg.Content);
        menuItemOpen.Visible = !isSystemMessage && _primaryAttachment != null && !msg.IsRevoked && !msg.IsDeleted;
        menuItemDownload.Visible = !isSystemMessage && _primaryAttachment != null && !msg.IsRevoked && !msg.IsDeleted;
        _menuItemPin.Visible = canManageMessage;
        _menuItemPin.Text = msg.IsPinned ? "Bỏ ghim tin nhắn" : "Ghim tin nhắn";
        IconHelper.SetMenuItemIcon(_menuItemPin, msg.IsPinned ? IconHelper.Unpin : IconHelper.Pin, Color.FromArgb(245, 158, 11));
        _menuItemForward.Visible = canManageMessage;
        _menuItemForward.Text = "Chuyển tiếp";
        IconHelper.SetMenuItemIcon(_menuItemForward, IconHelper.Forward, Color.FromArgb(96, 165, 250));
        IconHelper.SetMenuItemIcon(menuItemCopy, IconHelper.Copy, Color.FromArgb(96, 165, 250));
        IconHelper.SetMenuItemIcon(menuItemOpen, IconHelper.Open, Color.FromArgb(96, 165, 250));
        IconHelper.SetMenuItemIcon(menuItemDownload, IconHelper.Download, Color.FromArgb(96, 165, 250));
        IconHelper.SetMenuItemIcon(menuItemEdit, IconHelper.Edit, Color.FromArgb(96, 165, 250));
        IconHelper.SetMenuItemIcon(menuItemRevoke, IconHelper.Reply, Color.FromArgb(239, 68, 68));
        IconHelper.SetMenuItemIcon(menuItemDelete, IconHelper.Delete, Color.FromArgb(239, 68, 68));

        // Sender Name in group chat
        if (!isSystemMessage && !string.IsNullOrWhiteSpace(msg.ForwardedFromUsername))
        {
            string forwardedFrom = $"↪ Chuyển tiếp từ {msg.ForwardedFromUsername}";
            lblSenderName.Text = isGroupChat && !IsMyMessage
                ? $"{msg.SenderUsername} · {forwardedFrom}"
                : forwardedFrom;
            lblSenderName.Visible = true;
        }
        else if (isGroupChat && !IsMyMessage && !isSystemMessage)
        {
            lblSenderName.Text = msg.SenderUsername;
            lblSenderName.Visible = true;
        }
        else
        {
            lblSenderName.Visible = false;
        }

        // Avatar
        picSenderAvatar.Visible = !isSystemMessage;
        bool isAiSender = msg.SenderUsername == "chatting_ai" ||
                          msg.SenderId == Guid.Parse("00000000-0000-0000-0000-000000000001");
        if (!isSystemMessage)
        {
            Image? oldAvatar = picSenderAvatar.Image;
            if (isAiSender)
            {
                picSenderAvatar.Image = AiAvatarHelper.GetRobotBitmap(picSenderAvatar.Width, picSenderAvatar.Height);
            }
            else
            {
                picSenderAvatar.Image = GenerateAvatarBitmap(IsMyMessage ? "Bạn" : msg.SenderUsername, IsMyMessage);
            }
            oldAvatar?.Dispose();
            if (!isAiSender) _ = LoadSenderAvatarAsync(msg.SenderAvatarUrl);
            picSenderAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picSenderAvatar.Width, picSenderAvatar.Height, picSenderAvatar.Width, picSenderAvatar.Height));
        }

        // Rounded buttons inside panels
        btnDocOpen.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnDocOpen.Width, btnDocOpen.Height, 6, 6));
        btnDocDownload.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnDocDownload.Width, btnDocDownload.Height, 6, 6));
        btnPlayVoice.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnPlayVoice.Width, btnPlayVoice.Height, btnPlayVoice.Width, btnPlayVoice.Height));
        pnlVoiceProgress.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlVoiceProgress.Width, pnlVoiceProgress.Height, 6, 6));
        pnlDocument.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlDocument.Width, pnlDocument.Height, 10, 10));

        // Content & Attachments Display
        if (msg.IsRevoked)
        {
            lblContent.Visible = true;
            lblContent.Text = "Tin nhắn này đã bị thu hồi";
            lblContent.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            picImage.Visible = false;
            pnlDocument.Visible = false;
            pnlVoice.Visible = false;
            if (wv2Video is not null) wv2Video.Visible = false;
            pnlVideoInfo.Visible = false;
        }
        else if (msg.IsDeleted)
        {
            lblContent.Visible = true;
            lblContent.Text = "Tin nhắn đã bị xóa";
            lblContent.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            picImage.Visible = false;
            pnlDocument.Visible = false;
            pnlVoice.Visible = false;
            if (wv2Video is not null) wv2Video.Visible = false;
            pnlVideoInfo.Visible = false;
        }
        else
        {
            lblContent.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            if (msg.Type == MessageType.Image && _primaryAttachment != null)
            {
                // ── Image Attachment ───────────────────────────────────────────
                picImage.Visible = true;
                pnlDocument.Visible = false;
                pnlVoice.Visible = false;
                if (wv2Video is not null) wv2Video.Visible = false;
                pnlVideoInfo.Visible = false;

                // Only show text if different from filename
                bool hasText = !string.IsNullOrWhiteSpace(msg.Content) && msg.Content != _primaryAttachment.OriginalFileName;
                lblContent.Visible = hasText;
                lblContent.Text = hasText ? msg.Content : string.Empty;

                LoadImageThumbnailAsync();
            }
            else if (msg.Type == MessageType.Voice && _primaryAttachment != null)
            {
                // ── Voice Attachment ───────────────────────────────────────────
                picImage.Visible = false;
                pnlDocument.Visible = false;
                pnlVoice.Visible = true;
                if (wv2Video is not null) wv2Video.Visible = false;
                pnlVideoInfo.Visible = false;
                lblContent.Visible = false;

                int dur = _primaryAttachment.DurationSeconds ?? 0;
                lblVoiceTime.Text = $"{dur / 60:D2}:{dur % 60:D2}";
                UpdateVoicePlaybackButton(VoicePlayerManager.Instance.CurrentAttachmentId == _primaryAttachment.Id && VoicePlayerManager.Instance.IsPlaying);
            }
            else if ((msg.Type == MessageType.Video || IsVideoAttachment(_primaryAttachment)) && _primaryAttachment != null)
            {
                // ── Video Attachment (inline WebView2 player) ────────────────────────
                picImage.Visible = false;
                pnlDocument.Visible = false;
                pnlVoice.Visible = false;
                EnsureVideoPlayerControl().Visible = true;
                pnlVideoInfo.Visible = true;

                bool hasText = !string.IsNullOrWhiteSpace(msg.Content) && msg.Content != _primaryAttachment.OriginalFileName;
                lblContent.Visible = hasText;
                lblContent.Text = hasText ? msg.Content : string.Empty;

                IconHelper.SetLabelIcon(lblVideoInfoIcon, IconHelper.Video, Color.FromArgb(59, 130, 246), 18F);
                lblVideoInfoName.Text = _primaryAttachment.OriginalFileName;
                lblVideoInfoSize.Text = FormatFileSize(_primaryAttachment.FileSize);
                LayoutVideoInfoRow();

                // Apply rounded regions to the video info buttons
                btnVideoInfoOpen.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnVideoInfoOpen.Width, btnVideoInfoOpen.Height, 6, 6));
                btnVideoInfoDownload.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnVideoInfoDownload.Width, btnVideoInfoDownload.Height, 6, 6));
                pnlVideoInfo.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlVideoInfo.Width, pnlVideoInfo.Height, 10, 10));

                _ = InitVideoPlayerAsync(_primaryAttachment);
            }
            else if (_primaryAttachment != null)
            {
                // ── Document / Other File Attachment ───────────────────────────
                picImage.Visible = false;
                pnlDocument.Visible = true;
                pnlVoice.Visible = false;
                if (wv2Video is not null) wv2Video.Visible = false;
                pnlVideoInfo.Visible = false;

                bool hasText = !string.IsNullOrWhiteSpace(msg.Content) && msg.Content != _primaryAttachment.OriginalFileName;
                lblContent.Visible = hasText;
                lblContent.Text = hasText ? msg.Content : string.Empty;

                lblDocName.Text = _primaryAttachment.OriginalFileName;
                lblDocSize.Text = FormatFileSize(_primaryAttachment.FileSize);
                IconHelper.SetLabelIcon(lblDocIcon, IconHelper.GetFileTypeGlyph(_primaryAttachment.OriginalFileName), IconHelper.GetFileTypeColor(_primaryAttachment.OriginalFileName), 20F);
            }
            else
            {
                // ── Text Only ──────────────────────────────────────────────────
                picImage.Visible = false;
                pnlDocument.Visible = false;
                pnlVoice.Visible = false;
                if (wv2Video is not null) wv2Video.Visible = false;
                pnlVideoInfo.Visible = false;
                SetTextContent(msg.Content ?? string.Empty);
            }
        }

        // Timestamp & Status (double checkmark for read)
        string timeStr = VietnamTime.FromUtc(msg.CreatedAt).ToString("HH:mm");
        if (msg.IsEdited) timeStr += " • Đã sửa";

        if (isSystemMessage)
        {
            lblMeta.Text = timeStr;
            lblMeta.ForeColor = Color.FromArgb(148, 163, 184);
        }
        else if (IsMyMessage)
        {
            string checkmark = msg.Status switch
            {
                MessageStatus.Seen => " ✓✓",
                MessageStatus.Delivered => " ✓✓",
                _ => " ✓"
            };
            lblMeta.Text = $"{timeStr}{checkmark}";
            lblMeta.ForeColor = msg.Status == MessageStatus.Seen ? Color.FromArgb(37, 99, 235) : Color.FromArgb(148, 163, 184);
        }
        else
        {
            lblMeta.Text = timeStr;
            lblMeta.ForeColor = Color.FromArgb(148, 163, 184);
        }
        if (msg.IsPinned)
        {
            lblMeta.Text = "[Ghim] " + lblMeta.Text;
            lblMeta.ForeColor = _darkTheme ? Color.FromArgb(251, 191, 36) : Color.FromArgb(180, 83, 9);
        }

        RenderReactionControls();
        AdjustBubbleLayout();
    }

    public void ApplyTheme(bool dark)
    {
        _darkTheme = dark;
        if (MessageData is not null)
            SetData(MessageData, _currentUserId, _isGroupChat, _serverUrl, _apiClient);
    }

    private void SetTextContent(string content)
    {
        bool hasEmojiToken = content.Contains("[:emoji_", StringComparison.Ordinal);
        lblContent.Visible = !hasEmojiToken;
        lblContent.Text = hasEmojiToken ? string.Empty : content;
        if (!hasEmojiToken)
        {
            if (_emojiTextView is not null) _emojiTextView.Visible = false;
            return;
        }

        _emojiTextView ??= new EmojiTextView { Visible = false };
        if (!pnlBubble.Controls.Contains(_emojiTextView)) pnlBubble.Controls.Add(_emojiTextView);
        _emojiTextView.ForeColor = _darkTheme ? Color.FromArgb(248, 250, 252) : Color.FromArgb(15, 23, 42);
        _emojiTextView.BackColor = pnlBubble.BackColor;
        _emojiTextView.SetMessage(content, 396);
        _emojiTextView.Visible = true;
        _emojiTextView.BringToFront();
    }

    private async Task LoadSenderAvatarAsync(string? avatarUrl)
    {
        Image? image = await AvatarImageCache.GetAsync(_serverUrl, avatarUrl);
        if (image is null || IsDisposed) return;
        Bitmap rendered = AvatarImageCache.CreateCircularFit(image, picSenderAvatar.Size, Color.FromArgb(30, 41, 59), Color.FromArgb(148, 163, 184));
        Image? old = picSenderAvatar.Image;
        picSenderAvatar.Image = rendered;
        if (old is not null) old.Dispose();
        picSenderAvatar.Cursor = Cursors.Hand;
        picSenderAvatar.Click -= SenderAvatar_Click;
        picSenderAvatar.Click += SenderAvatar_Click;
    }

    private void SenderAvatar_Click(object? sender, EventArgs e)
    {
        if (picSenderAvatar.Image is not null)
            new AvatarPreviewForm(picSenderAvatar.Image, MessageData?.SenderUsername ?? "Ảnh đại diện").ShowDialog(FindForm());
    }

    private async void LoadImageThumbnailAsync()
    {
        if (_primaryAttachment == null) return;

        // Check if already in cache
        if (FileCacheService.Instance.IsCached(_primaryAttachment, out string cachedPath))
        {
            DisplayThumbnail(cachedPath);
            return;
        }

        if (_apiClient == null) return;

        // Async download from server
        string? downloadedPath = await FileCacheService.Instance.EnsureCachedAsync(_primaryAttachment, _serverUrl, _apiClient);
        if (!string.IsNullOrEmpty(downloadedPath) && File.Exists(downloadedPath))
        {
            if (IsHandleCreated)
            {
                BeginInvoke(() => DisplayThumbnail(downloadedPath));
            }
        }
    }

    private void DisplayThumbnail(string localPath)
    {
        _cachedFilePath = localPath;
        try
        {
            using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var tempImg = Image.FromStream(fileStream);

            // Compute thumbnail dimensions preserving aspect ratio (max 260x170)
            int maxW = 260;
            int maxH = 170;
            float ratio = Math.Min((float)maxW / tempImg.Width, (float)maxH / tempImg.Height);
            int thumbW = Math.Max(80, (int)(tempImg.Width * ratio));
            int thumbH = Math.Max(60, (int)(tempImg.Height * ratio));

            Bitmap bmp = new(tempImg, new Size(thumbW, thumbH));
            picImage.Image?.Dispose();
            picImage.Image = bmp;
            picImage.Size = new Size(thumbW, thumbH);
            picImage.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, thumbW, thumbH, 12, 12));

            AdjustBubbleLayout();
        }
        catch
        {
            picImage.BackColor = Color.FromArgb(241, 245, 249);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (MessageData != null)
        {
            LayoutVideoInfoRow();
            AdjustBubbleLayout();
        }
    }

    /// <summary>
    /// Keeps the video action buttons within the file-information row at every
    /// DPI setting. The original fixed coordinates could push the Save button
    /// beyond the rounded panel's right edge when Windows scaled the control.
    /// </summary>
    private void LayoutVideoInfoRow()
    {
        const int sidePadding = 8;
        const int buttonGap = 6;
        const int openButtonWidth = 46;
        const int downloadButtonWidth = 58;

        int buttonY = Math.Max(0, (pnlVideoInfo.ClientSize.Height - btnVideoInfoDownload.Height) / 2);
        int downloadX = Math.Max(sidePadding, pnlVideoInfo.ClientSize.Width - sidePadding - downloadButtonWidth);
        int openX = Math.Max(sidePadding, downloadX - buttonGap - openButtonWidth);

        btnVideoInfoDownload.Size = new Size(downloadButtonWidth, btnVideoInfoDownload.Height);
        btnVideoInfoDownload.Location = new Point(downloadX, buttonY);
        btnVideoInfoOpen.Size = new Size(openButtonWidth, btnVideoInfoOpen.Height);
        btnVideoInfoOpen.Location = new Point(openX, buttonY);

        int textWidth = Math.Max(40, openX - lblVideoInfoName.Left - sidePadding);
        lblVideoInfoName.Width = textWidth;
        lblVideoInfoSize.Width = textWidth;
    }

    private void AdjustBubbleLayout()
    {
        int availableWidth = Width;
        int maxBubbleWidth = 420;

        int contentWidth = 90;
        int contentHeight = 20;

        int currentY = 8;

        if (picImage.Visible)
        {
            picImage.Location = new Point(10, currentY);
            currentY += picImage.Height + 8;
            contentWidth = Math.Max(contentWidth, picImage.Width + 20);
        }
        else if (pnlDocument.Visible)
        {
            pnlDocument.Location = new Point(10, currentY);
            currentY += pnlDocument.Height + 8;
            contentWidth = Math.Max(contentWidth, pnlDocument.Width + 20);
        }
        else if (pnlVoice.Visible)
        {
            pnlVoice.Location = new Point(10, currentY);
            currentY += pnlVoice.Height + 8;
            contentWidth = Math.Max(contentWidth, pnlVoice.Width + 20);
        }
        else if (wv2Video?.Visible == true)
        {
            // Video player on top, file-info row directly below
            wv2Video.Location = new Point(10, currentY);
            currentY += wv2Video.Height + 4;
            pnlVideoInfo.Location = new Point(10, currentY);
            currentY += pnlVideoInfo.Height + 8;
            // The info panel can be wider than the WebView at high-DPI scales.
            // Size the containing bubble to whichever child is widest so its
            // right-aligned action buttons are never clipped.
            contentWidth = Math.Max(contentWidth, Math.Max(wv2Video.Width, pnlVideoInfo.Width) + 20);
        }

        if (_emojiTextView?.Visible == true)
        {
            _emojiTextView.Location = new Point(10, currentY);
            currentY += _emojiTextView.Height + 6;
            contentWidth = Math.Max(contentWidth, _emojiTextView.Width + 24);
        }
        else if (lblContent.Visible && !string.IsNullOrEmpty(lblContent.Text))
        {
            lblContent.Location = new Point(10, currentY);
            // Measure with the Label itself so wrapping and height use the same
            // renderer (UseCompatibleTextRendering) as the visible text.
            int textWidth = maxBubbleWidth - 24;
            lblContent.MaximumSize = new Size(textWidth, 0);
            Size textSize = lblContent.GetPreferredSize(new Size(textWidth, 0));
            lblContent.Size = textSize;
            currentY += lblContent.Height + 6;
            contentWidth = Math.Max(contentWidth, lblContent.Width + 24);
        }

        contentHeight = currentY + 4;
        pnlBubble.Size = new Size(contentWidth, contentHeight);

        // Apply rounded corner to bubble
        pnlBubble.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlBubble.Width, pnlBubble.Height, 16, 16));

        if (MessageData?.Type == MessageType.System)
        {
            pnlBubble.BackColor = _darkTheme ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240);
            lblContent.ForeColor = _darkTheme ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);
            lblContent.TextAlign = ContentAlignment.MiddleCenter;
            int bubbleX = Math.Max(0, (availableWidth - contentWidth) / 2);
            pnlBubble.Location = new Point(bubbleX, 8);
            lblMeta.Location = new Point(Math.Max(0, (availableWidth - lblMeta.Width) / 2), pnlBubble.Bottom + 4);
            Height = pnlBubble.Bottom + 26;
            return;
        }
        else if (_isSearchActiveMatch)
        {
            pnlBubble.BackColor = Color.FromArgb(254, 215, 170); // #fed7aa
        }
        else if (_isSearchMatch)
        {
            pnlBubble.BackColor = Color.FromArgb(255, 237, 213); // #ffedd5
        }
        else if (IsMyMessage)
        {
            pnlBubble.BackColor = _darkTheme ? Color.FromArgb(30, 64, 112) : Color.FromArgb(216, 236, 255);
        }
        else
        {
            pnlBubble.BackColor = _darkTheme ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
        }

        if (IsMyMessage)
        {
            lblContent.ForeColor = _darkTheme ? Color.FromArgb(248, 250, 252) : Color.FromArgb(15, 23, 42);

            int avatarX = availableWidth - picSenderAvatar.Width - 10;
            int bubbleX = avatarX - contentWidth - 8;
            int bubbleY = lblSenderName.Visible ? 24 : 8;

            if (lblSenderName.Visible)
                lblSenderName.Location = new Point(Math.Max(0, bubbleX + contentWidth - lblSenderName.Width), 4);
            picSenderAvatar.Location = new Point(avatarX, bubbleY);
            pnlBubble.Location = new Point(bubbleX, bubbleY);
            lblMeta.Location = new Point(bubbleX + contentWidth - lblMeta.Width, bubbleY + contentHeight + 4);
        }
        else
        {
            lblContent.ForeColor = _darkTheme ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);

            int bubbleX = 48;
            int bubbleY = lblSenderName.Visible ? 24 : 8;

            picSenderAvatar.Location = new Point(8, bubbleY);
            pnlBubble.Location = new Point(bubbleX, bubbleY);
            lblMeta.Location = new Point(bubbleX + 4, bubbleY + contentHeight + 4);
        }

        if (_reactionPanel.Visible)
        {
            int reactionX = IsMyMessage
                ? Math.Max(0, pnlBubble.Right - _reactionPanel.Width)
                : pnlBubble.Left + 2;
            _reactionPanel.Location = new Point(reactionX, pnlBubble.Bottom + 1);
            _reactionPanel.BringToFront();
            lblMeta.Location = new Point(
                IsMyMessage ? Math.Max(0, pnlBubble.Right - lblMeta.Width) : pnlBubble.Left + 4,
                _reactionPanel.Bottom + 1);
            Height = Math.Max(pnlBubble.Bottom + 26, lblMeta.Bottom + 6);
        }
        else
        {
            Height = pnlBubble.Bottom + 26;
        }
    }

    private void RenderReactionControls()
    {
        foreach (Control oldControl in _reactionPanel.Controls.Cast<Control>().ToArray())
        {
            _reactionPanel.Controls.Remove(oldControl);
            oldControl.Dispose();
        }

        if (MessageData is null || MessageData.Type == MessageType.System || MessageData.IsRevoked || MessageData.IsDeleted)
        {
            _reactionPanel.Visible = false;
            return;
        }

        foreach (MessageReactionSummaryDto reaction in MessageData.Reactions.Where(r => r.Count > 0))
        {
            Button chip = new()
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 23,
                Text = $"{reaction.Emoji} {reaction.Count}",
                ForeColor = _darkTheme ? Color.FromArgb(226, 232, 240) : Color.FromArgb(51, 65, 85),
                BackColor = reaction.ReactedByMe
                    ? (_darkTheme ? Color.FromArgb(30, 64, 112) : Color.FromArgb(219, 234, 254))
                    : (_darkTheme ? Color.FromArgb(30, 41, 59) : Color.FromArgb(248, 250, 252)),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(2, 0, 2, 0),
                Padding = new Padding(5, 0, 5, 0),
                Tag = reaction.Emoji,
                Cursor = Cursors.Hand
            };
            chip.FlatAppearance.BorderColor = _darkTheme ? Color.FromArgb(71, 85, 105) : Color.FromArgb(203, 213, 225);
            chip.FlatAppearance.BorderSize = 1;
            chip.Click += ReactionChip_Click;
            _reactionPanel.Controls.Add(chip);
        }

        _reactionPanel.Visible = _reactionPanel.Controls.Count > 0;
        _reactionPanel.PerformLayout();
    }

    private void ReactionPickerButton_Click(object? sender, EventArgs e)
    {
        if (sender is Control { Tag: string emoji } && MessageData is not null)
        {
            OnReactionRequested?.Invoke(MessageData, emoji);
            ctxMenu.Close();
        }
    }

    private void ReactionChip_Click(object? sender, EventArgs e)
    {
        if (sender is Control { Tag: string emoji } && MessageData is not null)
            OnReactionRequested?.Invoke(MessageData, emoji);
    }

    // ── Voice Message Interactions ───────────────────────────────────────────
    private async void btnPlayVoice_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment == null || _apiClient == null) return;

        Guid attId = _primaryAttachment.Id;

        if (VoicePlayerManager.Instance.CurrentAttachmentId == attId && VoicePlayerManager.Instance.IsPlaying)
        {
            VoicePlayerManager.Instance.Pause(attId);
            return;
        }

        btnPlayVoice.Enabled = false;
        try
        {
            string? localWav = await FileCacheService.Instance.EnsureCachedAsync(_primaryAttachment, _serverUrl, _apiClient);
            if (!string.IsNullOrEmpty(localWav) && File.Exists(localWav))
            {
                VoicePlayerManager.Instance.Play(attId, localWav);
            }
            else
            {
                MessageBox.Show("Không thể tải tin nhắn thoại từ máy chủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi phát âm thanh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnPlayVoice.Enabled = true;
        }
    }

    private void pnlVoiceProgress_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_primaryAttachment != null && VoicePlayerManager.Instance.CurrentAttachmentId == _primaryAttachment.Id)
        {
            float percent = (float)e.X / pnlVoiceProgress.Width;
            VoicePlayerManager.Instance.Seek(_primaryAttachment.Id, percent);
        }
    }

    // ── Image Interactions ───────────────────────────────────────────────────
    private async void picImage_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment == null) return;

        string? path = _cachedFilePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            if (_apiClient != null)
                path = await FileCacheService.Instance.EnsureCachedAsync(_primaryAttachment, _serverUrl, _apiClient);
        }

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            using var viewer = new ImageViewerForm(path, _primaryAttachment.OriginalFileName);
            viewer.ShowDialog(FindForm());
        }
        else
        {
            MessageBox.Show("Đang tải ảnh về máy, vui lòng thử lại sau giây lát...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // ── Document Interactions ────────────────────────────────────────────────
    private async void btnDocOpen_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment == null || _apiClient == null) return;

        btnDocOpen.Enabled = false;
        try
        {
            string? localPath = await FileCacheService.Instance.EnsureCachedAsync(_primaryAttachment, _serverUrl, _apiClient);
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                if (!FileCacheService.Instance.OpenFileWithDefaultApp(localPath, out string? err))
                {
                    MessageBox.Show(err, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Không thể tải tệp tin từ máy chủ.", "Lỗi tải tệp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            btnDocOpen.Enabled = true;
        }
    }

    private void btnDocDownload_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment != null)
        {
            OnDownloadRequested?.Invoke(_primaryAttachment);
        }
    }

    // ── Context Menu Actions ─────────────────────────────────────────────────
    private void menuItemCopy_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(lblContent.Text))
            Clipboard.SetText(lblContent.Text);
    }

    private void menuItemOpen_Click(object? sender, EventArgs e)
    {
        if (MessageData?.Type == MessageType.Image)
            picImage_Click(sender, e);
        else if (wv2Video?.Visible == true)
            btnVideoInfoOpen_Click(sender, e);
        else if (pnlDocument.Visible)
            btnDocOpen_Click(sender, e);
        else if (pnlVoice.Visible)
            btnPlayVoice_Click(sender, e);
    }

    private void menuItemDownload_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment != null)
            OnDownloadRequested?.Invoke(_primaryAttachment);
    }

    private void menuItemEdit_Click(object? sender, EventArgs e)
    {
        if (MessageData != null)
            OnEditRequested?.Invoke(MessageData);
    }

    private void menuItemRevoke_Click(object? sender, EventArgs e)
    {
        if (MessageData != null)
            OnRevokeRequested?.Invoke(MessageData);
    }

    private void menuItemDelete_Click(object? sender, EventArgs e)
    {
        if (MessageData != null)
            OnHideForMeRequested?.Invoke(MessageData);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static bool IsVideoAttachment(AttachmentDto? att)
    {
        if (att == null) return false;
        if (att.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return true;
        string ext = Path.GetExtension(att.OriginalFileName).ToLowerInvariant();
        return ext is ".mp4" or ".webm" or ".mov" or ".avi" or ".mkv" or ".wmv" or ".flv";
    }

    // ── Video Player (WebView2) ─────────────────────────────────────────────────
    private bool _webViewReady = false;
    private static readonly object WebViewEnvironmentLock = new();
    private static Task<CoreWebView2Environment>? _webViewEnvironmentTask;

    private Microsoft.Web.WebView2.WinForms.WebView2 EnsureVideoPlayerControl()
    {
        if (wv2Video is not null) return wv2Video;

        wv2Video = new Microsoft.Web.WebView2.WinForms.WebView2
        {
            DefaultBackgroundColor = Color.FromArgb(15, 23, 42),
            Location = new Point(10, 10),
            Name = "wv2Video",
            Size = new Size(340, 192),
            TabIndex = 5,
            Visible = false
        };
        pnlBubble.Controls.Add(wv2Video);
        return wv2Video;
    }

    /// <summary>
    /// Initialises the WebView2 environment (once) and loads the HTML5 video player
    /// page with the local video file served as a blob URL.
    /// </summary>
    private async Task InitVideoPlayerAsync(AttachmentDto att)
    {
        try
        {
            var video = EnsureVideoPlayerControl();
            // Ensure WebView2 environment is initialised only once
            if (!_webViewReady)
            {
                CoreWebView2Environment env = await GetWebViewEnvironmentAsync();
                await video.EnsureCoreWebView2Async(env);
                video.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                video.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webViewReady = true;
            }

            // Download/cache the video file locally. A message may arrive immediately
            // after upload, so retry once for a short-lived LAN/server race.
            string? localPath = await EnsureVideoCachedAsync(att);

            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                // Show a nice error page instead of crashing
                video.NavigateToString(BuildErrorHtml());
                return;
            }

            // Map the local file so WebView2 can load it via virtual host
            string hostName = "localvideo.chat";
            string videoDir  = Path.GetDirectoryName(localPath)!;
            video.CoreWebView2.SetVirtualHostNameToFolderMapping(
                hostName, videoDir, CoreWebView2HostResourceAccessKind.Allow);

            string videoFileName = Path.GetFileName(localPath);
            string videoUrl  = $"https://{hostName}/{videoFileName}";
            string playerHtml = BuildVideoPlayerHtml(videoUrl);
            video.NavigateToString(playerHtml);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VideoPlayer] init error: {ex.Message}");
        }
    }

    /// <summary>
    /// All video bubbles in one client share one WebView2 environment. The profile
    /// is scoped to this process so two locally launched clients (two accounts)
    /// do not lock each other out of WebView2 initialisation.
    /// </summary>
    private static Task<CoreWebView2Environment> GetWebViewEnvironmentAsync()
    {
        lock (WebViewEnvironmentLock)
        {
            if (_webViewEnvironmentTask == null || _webViewEnvironmentTask.IsFaulted)
            {
                string processProfile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ChattingApp", "WebView2", Process.GetCurrentProcess().Id.ToString());
                _webViewEnvironmentTask = CoreWebView2Environment.CreateAsync(null, processProfile);
            }

            return _webViewEnvironmentTask;
        }
    }

    private async Task<string?> EnsureVideoCachedAsync(AttachmentDto att)
    {
        if (_apiClient == null)
            return null;

        string? localPath = await FileCacheService.Instance.EnsureCachedAsync(att, _serverUrl, _apiClient);
        if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            return localPath;

        await Task.Delay(350);
        return await FileCacheService.Instance.EnsureCachedAsync(att, _serverUrl, _apiClient);
    }

    /// <summary>Generates the self-contained HTML page for the inline video player.</summary>
    private static string BuildVideoPlayerHtml(string videoUrl)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width,initial-scale=1">
            <style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body {
                background: #0f141c;
                display: flex; align-items: center; justify-content: center;
                width: 100vw; height: 100vh; overflow: hidden;
              }
              .player-wrap {
                position: relative;
                width: 100%; height: 100%;
                background: #0f141c;
                border-radius: 10px;
                overflow: hidden;
              }
              video {
                width: 100%; height: calc(100% - 44px);
                object-fit: contain;
                display: block;
                background: #0f141c;
              }
              /* ── Controls bar ── */
              .controls {
                position: absolute;
                bottom: 0; left: 0; right: 0;
                height: 44px;
                background: linear-gradient(transparent, rgba(0,0,0,0.75));
                display: flex;
                align-items: center;
                padding: 0 8px;
                gap: 6px;
                color: #fff;
                font-family: -apple-system, 'Segoe UI', sans-serif;
                font-size: 12px;
              }
              .btn {
                background: none;
                border: none;
                color: #fff;
                cursor: pointer;
                padding: 4px;
                border-radius: 4px;
                display: flex; align-items: center; justify-content: center;
                font-size: 14px;
                flex-shrink: 0;
                transition: background 0.15s;
              }
              .btn:hover { background: rgba(255,255,255,0.15); }
              .btn svg { width: 16px; height: 16px; fill: #fff; }
              /* ── Purple seek bar ── */
              .seek-wrap {
                flex: 1;
                height: 4px;
                background: rgba(255,255,255,0.25);
                border-radius: 2px;
                cursor: pointer;
                position: relative;
              }
              .seek-fill {
                height: 100%;
                width: 0%;
                background: #7c3aed;
                border-radius: 2px;
                pointer-events: none;
              }
              .seek-thumb {
                position: absolute;
                top: 50%; right: auto;
                width: 10px; height: 10px;
                background: #7c3aed;
                border-radius: 50%;
                transform: translate(-50%, -50%);
                left: 0%;
                pointer-events: none;
              }
              .time { font-size: 11px; white-space: nowrap; user-select: none; flex-shrink: 0; }
              .spacer { flex: 0 0 2px; }
            </style>
            </head>
            <body>
            <div class="player-wrap">
              <video id="v" preload="metadata" src="{{videoUrl}}"></video>
              <div class="controls" id="ctrl">
                <!-- Play/Pause -->
                <button class="btn" id="btnPlay" title="Play/Pause" onclick="togglePlay()">
                  <svg id="icPlay" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
                  <svg id="icPause" viewBox="0 0 24 24" style="display:none"><path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/></svg>
                </button>
                <!-- Time -->
                <span class="time" id="timeDisp">00:00 / 00:00</span>
                <!-- Seek bar -->
                <div class="seek-wrap" id="seek" onmousedown="seekStart(event)">
                  <div class="seek-fill" id="seekFill"></div>
                  <div class="seek-thumb" id="seekThumb"></div>
                </div>
                <!-- Mute -->
                <button class="btn" id="btnMute" title="Mute" onclick="toggleMute()">
                  <svg id="icVol" viewBox="0 0 24 24"><path d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z"/></svg>
                  <svg id="icMute" viewBox="0 0 24 24" style="display:none"><path d="M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4L9.91 6.09 12 8.18V4z"/></svg>
                </button>
                <!-- Settings (no-op visual) -->
                <button class="btn" title="Settings" onclick="">
                  <svg viewBox="0 0 24 24"><path d="M19.14 12.94c.04-.3.06-.61.06-.94s-.02-.64-.07-.94l2.03-1.58a.49.49 0 0 0 .12-.61l-1.92-3.32a.49.49 0 0 0-.59-.22l-2.39.96a6.93 6.93 0 0 0-1.62-.94l-.36-2.54a.484.484 0 0 0-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87a.48.48 0 0 0 .12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58a.49.49 0 0 0-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32a.48.48 0 0 0-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z"/></svg>
                </button>
                <!-- PiP -->
                <button class="btn" title="Picture-in-Picture" onclick="togglePip()">
                  <svg viewBox="0 0 24 24"><path d="M19 7h-8v6h8V7zm2-4H3c-1.1 0-2 .9-2 2v14c0 1.1.9 1.98 2 1.98h18c1.1 0 2-.88 2-1.98V5c0-1.1-.9-2-2-2zm0 16.01H3V4.98h18v14.03z"/></svg>
                </button>
                <!-- Fullscreen -->
                <button class="btn" title="Fullscreen" onclick="toggleFS()">
                  <svg viewBox="0 0 24 24"><path d="M7 14H5v5h5v-2H7v-3zm-2-4h2V7h3V5H5v5zm12 7h-3v2h5v-5h-2v3zM14 5v2h3v3h2V5h-5z"/></svg>
                </button>
              </div>
            </div>
            <script>
              const v = document.getElementById('v');
              const seekFill  = document.getElementById('seekFill');
              const seekThumb = document.getElementById('seekThumb');
              const timeDisp  = document.getElementById('timeDisp');

              function fmt(s) {
                const m = Math.floor(s / 60);
                const sec = Math.floor(s % 60);
                return String(m).padStart(2,'0') + ':' + String(sec).padStart(2,'0');
              }

              v.addEventListener('timeupdate', () => {
                if (!v.duration) return;
                const pct = (v.currentTime / v.duration) * 100;
                seekFill.style.width  = pct + '%';
                seekThumb.style.left  = pct + '%';
                timeDisp.textContent  = fmt(v.currentTime) + ' / ' + fmt(v.duration);
              });

              v.addEventListener('play',  () => { icPlay.style.display='none';  icPause.style.display=''; });
              v.addEventListener('pause', () => { icPlay.style.display='';      icPause.style.display='none'; });
              v.addEventListener('volumechange', () => {
                icVol.style.display  = v.muted ? 'none' : '';
                icMute.style.display = v.muted ? ''     : 'none';
              });
              v.addEventListener('loadedmetadata', () => {
                timeDisp.textContent = '00:00 / ' + fmt(v.duration);
              });

              function togglePlay() { v.paused ? v.play() : v.pause(); }
              function toggleMute() { v.muted = !v.muted; }
              async function togglePip() {
                try {
                  if (document.pictureInPictureElement) await document.exitPictureInPicture();
                  else await v.requestPictureInPicture();
                } catch(_) {}
              }
              function toggleFS() {
                if (!document.fullscreenElement) document.documentElement.requestFullscreen();
                else document.exitFullscreen();
              }

              let seeking = false;
              function seekTo(e) {
                const rect = document.getElementById('seek').getBoundingClientRect();
                const pct = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
                v.currentTime = pct * v.duration;
              }
              function seekStart(e) {
                seeking = true;
                seekTo(e);
                window.addEventListener('mousemove', seekMove);
                window.addEventListener('mouseup',   seekEnd);
              }
              function seekMove(e) { if (seeking) seekTo(e); }
              function seekEnd()   { seeking = false; window.removeEventListener('mousemove', seekMove); window.removeEventListener('mouseup', seekEnd); }
            </script>
            </body>
            </html>
            """.Replace("{{videoUrl}}", videoUrl);
    }

    private static string BuildErrorHtml() => """
        <!DOCTYPE html><html><body style="background:#0f141c;color:#94a3b8;
        display:flex;align-items:center;justify-content:center;height:100vh;
        font-family:Segoe UI,sans-serif;font-size:13px;">
        <div style="text-align:center">
          <div style="font-size:32px;margin-bottom:8px">&#127909;</div>
          <div>Không thể tải video</div>
        </div></body></html>
        """;

    // ── Video Info Row Interactions ─────────────────────────────────────────────
    private async void btnVideoInfoOpen_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment == null || _apiClient == null) return;

        try
        {
            string? localPath = await FileCacheService.Instance.EnsureCachedAsync(_primaryAttachment, _serverUrl, _apiClient);
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                if (!FileCacheService.Instance.OpenFileWithDefaultApp(localPath, out string? err))
                    MessageBox.Show(err, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Không thể tải video từ máy chủ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi mở video: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnVideoInfoDownload_Click(object? sender, EventArgs e)
    {
        if (_primaryAttachment != null)
            OnDownloadRequested?.Invoke(_primaryAttachment);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private static string GetFileIconEmoji(string fileName)
    {
        return IconHelper.GetFileTypeGlyph(fileName);
    }

    private static Bitmap GenerateAvatarBitmap(string name, bool isMine = false)
    {
        return IconHelper.GenerateAvatarBitmap(name, false, 32);
    }

    /// <summary>
    /// Highlights or resets the search match state on this message bubble.
    /// </summary>
    public void SetSearchHighlight(bool isMatch, bool isActiveMatch, string? query)
    {
        _isSearchMatch = isMatch;
        _isSearchActiveMatch = isActiveMatch;
        _searchQuery = isMatch ? query : null;

        Color targetColor;
        if (isActiveMatch)
        {
            targetColor = Color.FromArgb(254, 215, 170); // #fed7aa
        }
        else if (isMatch)
        {
            targetColor = Color.FromArgb(255, 237, 213); // #ffedd5
        }
        else
        {
            targetColor = IsMyMessage ? Color.FromArgb(216, 236, 255) : Color.FromArgb(241, 245, 249);
        }

        AnimateBubbleColor(targetColor);
        pnlBubble.Invalidate();
        lblContent.Invalidate();
    }

    private void AnimateBubbleColor(Color targetColor)
    {
        _colorFadeTimer?.Stop();
        _colorFadeTimer?.Dispose();
        _colorFadeTimer = null;

        if (pnlBubble.BackColor == targetColor) return;

        _fadeStartColor = pnlBubble.BackColor;
        _fadeTargetColor = targetColor;
        _fadeStep = 0;

        _colorFadeTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _colorFadeTimer.Tick += (s, e) =>
        {
            _fadeStep++;
            float t = (float)_fadeStep / MaxFadeSteps;
            float ease = 1f - (1f - t) * (1f - t);
            int r = (int)(_fadeStartColor.R + (_fadeTargetColor.R - _fadeStartColor.R) * ease);
            int g = (int)(_fadeStartColor.G + (_fadeTargetColor.G - _fadeStartColor.G) * ease);
            int b = (int)(_fadeStartColor.B + (_fadeTargetColor.B - _fadeStartColor.B) * ease);

            pnlBubble.BackColor = Color.FromArgb(
                Math.Clamp(r, 0, 255),
                Math.Clamp(g, 0, 255),
                Math.Clamp(b, 0, 255));

            if (_fadeStep >= MaxFadeSteps)
            {
                pnlBubble.BackColor = _fadeTargetColor;
                _colorFadeTimer.Stop();
                _colorFadeTimer.Dispose();
                _colorFadeTimer = null;
            }
        };
        _colorFadeTimer.Start();
    }

    private void PnlBubble_Paint(object? sender, PaintEventArgs e)
    {
        if (_isSearchActiveMatch)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(253, 186, 116), 1.75f); // #fdba74
            using var path = CreateRoundedRectanglePath(new Rectangle(1, 1, pnlBubble.Width - 3, pnlBubble.Height - 3), 14);
            e.Graphics.DrawPath(pen, path);
        }
    }

    private void LblContent_Paint(object? sender, PaintEventArgs e)
    {
        if (!_isSearchMatch || string.IsNullOrEmpty(_searchQuery) || string.IsNullOrEmpty(lblContent.Text))
            return;

        string text = lblContent.Text;
        string query = _searchQuery;

        var ranges = new List<CharacterRange>();
        int start = 0;
        while (start < text.Length)
        {
            int idx = text.IndexOf(query, start, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) break;
            ranges.Add(new CharacterRange(idx, query.Length));
            start = idx + query.Length;
            if (ranges.Count >= 32) break;
        }

        if (ranges.Count == 0) return;

        try
        {
            using var sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces)
            {
                Trimming = StringTrimming.Word
            };
            sf.SetMeasurableCharacterRanges(ranges.ToArray());

            var layoutRect = new RectangleF(0, 0, lblContent.Width, lblContent.Height);
            Region[] regions = e.Graphics.MeasureCharacterRanges(text, lblContent.Font, layoutRect, sf);

            Color highlightBg = _isSearchActiveMatch
                ? Color.FromArgb(253, 186, 116)  // #fdba74
                : Color.FromArgb(254, 240, 138); // #fef08a

            using var brush = new SolidBrush(highlightBg);
            foreach (var region in regions)
            {
                RectangleF bounds = region.GetBounds(e.Graphics);
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    var padRect = new RectangleF(bounds.X - 1, bounds.Y, bounds.Width + 2, bounds.Height);
                    e.Graphics.FillRectangle(brush, padRect);
                }
            }

            using var textBrush = new SolidBrush(lblContent.ForeColor);
            e.Graphics.DrawString(text, lblContent.Font, textBrush, layoutRect, sf);
        }
        catch { }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        GraphicsPath path = new();
        int d = radius * 2;
        if (d > rect.Width) d = rect.Width;
        if (d > rect.Height) d = rect.Height;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
