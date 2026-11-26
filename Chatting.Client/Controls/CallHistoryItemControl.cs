using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Controls;

public class CallHistoryItemControl : UserControl
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    public CallInfoDto Call { get; private set; }
    public Guid CurrentUserId { get; private set; }
    public bool IsSelected { get; private set; }

    public event Action<CallInfoDto>? ItemClicked;
    public event Action<CallInfoDto>? VoiceCallbackClicked;
    public event Action<CallInfoDto>? VideoCallbackClicked;
    public event Action<Guid>? OpenChatClicked;
    private readonly string _serverUrl;
    private readonly ToolTip _toolTip = new();

    private readonly PictureBox picAvatar = new();
    private readonly Label lblName = new();
    private readonly Label lblCallTypeBadge = new();
    private readonly Label lblStatusIcon = new();
    private readonly Label lblStatusText = new();
    private readonly Label lblTime = new();
    private readonly Label lblDuration = new();
    private readonly Button btnVoiceCall = new();
    private readonly Button btnVideoCall = new();
    private readonly ContextMenuStrip ctxMenu = new();

    private readonly Color _bgNormal = Color.White;
    private readonly Color _bgHover = Color.FromArgb(248, 250, 252);
    private readonly Color _bgSelected = Color.FromArgb(239, 246, 255);
    private readonly Color _borderSelected = Color.FromArgb(147, 197, 253);

    public CallHistoryItemControl(CallInfoDto call, Guid currentUserId, string serverUrl)
    {
        Call = call;
        CurrentUserId = currentUserId;
        _serverUrl = serverUrl;

        InitializeComponentManual();
        UpdateData();
        Disposed += (_, _) => _toolTip.Dispose();
    }

    private void InitializeComponentManual()
    {
        Height = 72;
        Width = 700;
        BackColor = _bgNormal;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;

        // Avatar
        picAvatar.Size = new Size(44, 44);
        picAvatar.Location = new Point(14, 14);
        picAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picAvatar.BackColor = Color.FromArgb(59, 130, 246);

        // Name
        lblName.AutoSize = false;
        lblName.Location = new Point(68, 14);
        lblName.Size = new Size(240, 22);
        lblName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblName.ForeColor = Color.FromArgb(15, 23, 42);
        lblName.AutoEllipsis = true;

        // Call type badge (Thoại / Video)
        lblCallTypeBadge.AutoSize = true;
        lblCallTypeBadge.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        lblCallTypeBadge.Padding = new Padding(5, 2, 5, 2);

        // Status icon + text (↙ Cuộc gọi đến • Đã kết nối)
        lblStatusIcon.AutoSize = true;
        lblStatusIcon.Font = new Font("Segoe UI", 9.5F);
        lblStatusIcon.Location = new Point(68, 38);

        lblStatusText.AutoSize = true;
        lblStatusText.Font = new Font("Segoe UI", 8.5F);
        lblStatusText.Location = new Point(88, 38);

        // Duration
        lblDuration.AutoSize = true;
        lblDuration.Font = new Font("Segoe UI", 8.5F);
        lblDuration.ForeColor = Color.FromArgb(100, 116, 139);

        // Time
        lblTime.AutoSize = false;
        lblTime.Size = new Size(160, 20);
        lblTime.Font = new Font("Segoe UI", 8.5F);
        lblTime.ForeColor = Color.FromArgb(148, 163, 184);
        lblTime.TextAlign = ContentAlignment.TopRight;

        // Callback Voice button
        btnVoiceCall.Size = new Size(84, 34);
        btnVoiceCall.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnVoiceCall.Text = " Thoại";
        IconHelper.SetButtonIcon(btnVoiceCall, IconHelper.Phone, Color.FromArgb(29, 78, 216), 18, 12F, clearText: false);
        btnVoiceCall.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnVoiceCall.AccessibleName = "Gọi lại bằng cuộc gọi thoại";
        btnVoiceCall.AccessibleDescription = "Bắt đầu cuộc gọi thoại với người này.";
        btnVoiceCall.FlatStyle = FlatStyle.Flat;
        btnVoiceCall.FlatAppearance.BorderSize = 1;
        btnVoiceCall.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        btnVoiceCall.BackColor = Color.FromArgb(239, 246, 255);
        btnVoiceCall.ForeColor = Color.FromArgb(29, 78, 216);
        btnVoiceCall.Cursor = Cursors.Hand;
        btnVoiceCall.Click += (s, e) => VoiceCallbackClicked?.Invoke(Call);
        _toolTip.SetToolTip(btnVoiceCall, "Gọi lại bằng cuộc gọi thoại");

        // Callback Video button
        btnVideoCall.Size = new Size(84, 34);
        btnVideoCall.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnVideoCall.Text = " Video";
        IconHelper.SetButtonIcon(btnVideoCall, IconHelper.Video, Color.FromArgb(15, 23, 42), 18, 12F, clearText: false);
        btnVideoCall.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnVideoCall.AccessibleName = "Gọi lại bằng cuộc gọi video";
        btnVideoCall.AccessibleDescription = "Bắt đầu cuộc gọi video với người này.";
        btnVideoCall.FlatStyle = FlatStyle.Flat;
        btnVideoCall.FlatAppearance.BorderSize = 1;
        btnVideoCall.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        btnVideoCall.BackColor = Color.FromArgb(248, 250, 252);
        btnVideoCall.ForeColor = Color.FromArgb(15, 23, 42);
        btnVideoCall.Cursor = Cursors.Hand;
        btnVideoCall.Click += (s, e) => VideoCallbackClicked?.Invoke(Call);
        _toolTip.SetToolTip(btnVideoCall, "Gọi lại bằng cuộc gọi video");

        // Context Menu
        var mnuVoice = new ToolStripMenuItem("Gọi lại bằng cuộc gọi thoại", null, (s, e) => VoiceCallbackClicked?.Invoke(Call));
        IconHelper.SetMenuItemIcon(mnuVoice, IconHelper.Phone, Color.FromArgb(71, 85, 105), 16);

        var mnuVideo = new ToolStripMenuItem("Gọi lại bằng cuộc gọi video", null, (s, e) => VideoCallbackClicked?.Invoke(Call));
        IconHelper.SetMenuItemIcon(mnuVideo, IconHelper.Video, Color.FromArgb(71, 85, 105), 16);

        var mnuChat = new ToolStripMenuItem("Mở cuộc trò chuyện", null, (s, e) => OpenChatClicked?.Invoke(Call.ConversationId));
        IconHelper.SetMenuItemIcon(mnuChat, IconHelper.Chat, Color.FromArgb(71, 85, 105), 16);

        var mnuCopy = new ToolStripMenuItem("Sao chép tên tài khoản", null, (s, e) =>
        {
            string targetUser = GetContactUsername();
            if (!string.IsNullOrEmpty(targetUser))
                Clipboard.SetText(targetUser);
        });
        IconHelper.SetMenuItemIcon(mnuCopy, IconHelper.Copy, Color.FromArgb(71, 85, 105), 16);

        ctxMenu.Items.AddRange(new ToolStripItem[] { mnuVoice, mnuVideo, new ToolStripSeparator(), mnuChat, mnuCopy });
        ContextMenuStrip = ctxMenu;

        Controls.Add(picAvatar);
        Controls.Add(lblName);
        Controls.Add(lblCallTypeBadge);
        Controls.Add(lblStatusIcon);
        Controls.Add(lblStatusText);
        Controls.Add(lblDuration);
        Controls.Add(lblTime);
        Controls.Add(btnVoiceCall);
        Controls.Add(btnVideoCall);

        WireEvents(this);
    }

    private void WireEvents(Control ctrl)
    {
        if (ctrl != btnVoiceCall && ctrl != btnVideoCall)
        {
            ctrl.MouseEnter += (s, e) => { if (!IsSelected) BackColor = _bgHover; };
            ctrl.MouseLeave += (s, e) => { if (!IsSelected) BackColor = _bgNormal; };
            ctrl.Click += (s, e) =>
            {
                ItemClicked?.Invoke(Call);
            };
        }

        foreach (Control c in ctrl.Controls)
            WireEvents(c);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        BackColor = selected ? _bgSelected : _bgNormal;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Bottom separator
        using var penSep = new Pen(Color.FromArgb(241, 245, 249), 1);
        e.Graphics.DrawLine(penSep, 14, Height - 1, Width - 14, Height - 1);

        // Selection border
        if (IsSelected)
        {
            using var penSel = new Pen(_borderSelected, 2);
            e.Graphics.DrawRectangle(penSel, 1, 1, Width - 3, Height - 3);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        AdjustLayout();
    }

    private void AdjustLayout()
    {
        int w = Width;

        // Position action buttons on the far right
        btnVideoCall.Location = new Point(w - 14 - btnVideoCall.Width, (Height - btnVideoCall.Height) / 2);
        btnVoiceCall.Location = new Point(btnVideoCall.Left - 8 - btnVoiceCall.Width, (Height - btnVoiceCall.Height) / 2);

        // Time positioned to the left of the buttons
        int timeRight = btnVoiceCall.Left - 16;
        lblTime.Location = new Point(Math.Max(300, timeRight - lblTime.Width), 14);

        // Name and badges
        int maxNameW = Math.Max(120, lblTime.Left - 80);
        lblName.Width = Math.Min(240, maxNameW);
        lblCallTypeBadge.Location = new Point(lblName.Right + 8, 14);

        // Status row
        lblStatusIcon.Location = new Point(68, 40);
        lblStatusText.Location = new Point(lblStatusIcon.Right + 4, 40);
        lblDuration.Location = new Point(lblStatusText.Right + 6, 40);

        // Update rounded regions
        if (btnVoiceCall.Width > 0 && btnVoiceCall.Height > 0)
        {
            var oldR = btnVoiceCall.Region;
            btnVoiceCall.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnVoiceCall.Width + 1, btnVoiceCall.Height + 1, 8, 8));
            oldR?.Dispose();
        }

        if (btnVideoCall.Width > 0 && btnVideoCall.Height > 0)
        {
            var oldR = btnVideoCall.Region;
            btnVideoCall.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnVideoCall.Width + 1, btnVideoCall.Height + 1, 8, 8));
            oldR?.Dispose();
        }

        if (picAvatar.Width > 0 && picAvatar.Height > 0)
        {
            var oldR = picAvatar.Region;
            picAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picAvatar.Width, picAvatar.Height, picAvatar.Width, picAvatar.Height));
            oldR?.Dispose();
        }
    }

    private string GetContactUsername()
    {
        bool isMeCaller = Call.CallerId == CurrentUserId;
        return isMeCaller ? (Call.ReceiverUsername ?? "Người dùng") : Call.CallerUsername;
    }

    private string GetContactDisplayName()
    {
        bool isMeCaller = Call.CallerId == CurrentUserId;
        if (isMeCaller)
        {
            return !string.IsNullOrWhiteSpace(Call.ReceiverDisplayName)
                ? Call.ReceiverDisplayName
                : (!string.IsNullOrWhiteSpace(Call.ReceiverUsername) ? Call.ReceiverUsername : "Người dùng");
        }
        else
        {
            return !string.IsNullOrWhiteSpace(Call.CallerDisplayName)
                ? Call.CallerDisplayName
                : Call.CallerUsername;
        }
    }

    public void UpdateData()
    {
        bool isMeCaller = Call.CallerId == CurrentUserId;
        string contactName = GetContactDisplayName();
        lblName.Text = contactName;

        // Avatar
        picAvatar.Image = GenerateAvatarImage(contactName);
        string? avatarUrl = isMeCaller ? Call.ReceiverAvatarUrl : Call.CallerAvatarUrl;
        _ = LoadAvatarAsync(avatarUrl);

        // Call Type Badge
        if (Call.CallType == CallType.Video)
        {
            lblCallTypeBadge.Text = "VIDEO";
            lblCallTypeBadge.BackColor = Color.FromArgb(243, 232, 255);
            lblCallTypeBadge.ForeColor = Color.FromArgb(126, 34, 206);
        }
        else
        {
            lblCallTypeBadge.Text = "THOẠI";
            lblCallTypeBadge.BackColor = Color.FromArgb(224, 231, 255);
            lblCallTypeBadge.ForeColor = Color.FromArgb(37, 99, 235);
        }

        // Direction & Status
        if (isMeCaller)
        {
            // Outgoing
            IconHelper.SetLabelIcon(lblStatusIcon, IconHelper.ArrowUp, Color.FromArgb(37, 99, 235), 11F);

            if (Call.Status == CallStatus.Ended || Call.Status == CallStatus.Ongoing)
            {
                lblStatusText.Text = "Cuộc gọi đi • Đã kết nối";
                lblStatusText.ForeColor = Color.FromArgb(22, 163, 74);
            }
            else if (Call.Status == CallStatus.Rejected)
            {
                lblStatusText.Text = "Cuộc gọi đi • Bị từ chối";
                lblStatusText.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else if (Call.Status == CallStatus.Missed)
            {
                lblStatusText.Text = "Cuộc gọi đi • Không trả lời";
                lblStatusText.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else
            {
                lblStatusText.Text = "Cuộc gọi đi";
                lblStatusText.ForeColor = Color.FromArgb(100, 116, 139);
            }
        }
        else
        {
            // Incoming
            if (Call.Status == CallStatus.Missed)
            {
                IconHelper.SetLabelIcon(lblStatusIcon, IconHelper.PhoneMissed, Color.FromArgb(239, 68, 68), 12F);
                lblStatusText.Text = "Cuộc gọi nhỡ";
                lblStatusText.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatusText.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            }
            else if (Call.Status == CallStatus.Rejected)
            {
                IconHelper.SetLabelIcon(lblStatusIcon, IconHelper.Block, Color.FromArgb(220, 38, 38), 12F);
                lblStatusText.Text = "Đã từ chối";
                lblStatusText.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else if (Call.Status == CallStatus.Ended || Call.Status == CallStatus.Ongoing)
            {
                IconHelper.SetLabelIcon(lblStatusIcon, IconHelper.ArrowDown, Color.FromArgb(22, 163, 74), 11F);
                lblStatusText.Text = "Cuộc gọi đến • Đã kết nối";
                lblStatusText.ForeColor = Color.FromArgb(22, 163, 74);
            }
            else
            {
                IconHelper.SetLabelIcon(lblStatusIcon, IconHelper.ArrowDown, Color.FromArgb(100, 116, 139), 11F);
                lblStatusText.Text = "Cuộc gọi đến";
                lblStatusText.ForeColor = Color.FromArgb(100, 116, 139);
            }
        }

        // Duration
        if (Call.DurationSeconds.HasValue && Call.DurationSeconds > 0)
        {
            var ts = TimeSpan.FromSeconds(Call.DurationSeconds.Value);
            string durStr = ts.Hours > 0
                ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            lblDuration.Text = $"• Thời lượng: {durStr}";
            lblDuration.Visible = true;
        }
        else
        {
            lblDuration.Visible = false;
        }

        // Timestamp
        var localTime = VietnamTime.FromUtc(Call.StartedAt);
        if (localTime.Date == DateTime.Today)
            lblTime.Text = $"Hôm nay {localTime:HH:mm}";
        else if (localTime.Date == DateTime.Today.AddDays(-1))
            lblTime.Text = $"Hôm qua {localTime:HH:mm}";
        else
            lblTime.Text = localTime.ToString("dd/MM/yyyy HH:mm");

        AdjustLayout();
    }

    private async Task LoadAvatarAsync(string? avatarUrl)
    {
        Image? source = await AvatarImageCache.GetAsync(_serverUrl, avatarUrl);
        if (source is null || IsDisposed || picAvatar.IsDisposed)
            return;

        Bitmap rendered = AvatarImageCache.CreateCircularFit(source, picAvatar.Size, Color.White, Color.FromArgb(226, 232, 240));
        if (IsDisposed || picAvatar.IsDisposed)
        {
            rendered.Dispose();
            return;
        }

        Image? old = picAvatar.Image;
        picAvatar.Image = rendered;
        old?.Dispose();
    }

    private static Image GenerateAvatarImage(string name)
    {
        var bmp = new Bitmap(44, 44);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Deterministic color
        int hash = Math.Abs(name.GetHashCode());
        Color[] colors =
        {
            Color.FromArgb(37, 99, 235),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(217, 119, 6),
            Color.FromArgb(139, 92, 246),
            Color.FromArgb(236, 72, 153),
            Color.FromArgb(14, 165, 233)
        };
        Color bg = colors[hash % colors.Length];

        using var brush = new SolidBrush(bg);
        g.FillEllipse(brush, 0, 0, 44, 44);

        string initial = !string.IsNullOrWhiteSpace(name) ? name.Trim()[0].ToString().ToUpperInvariant() : "?";
        using var font = new Font("Segoe UI", 14F, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        var size = g.MeasureString(initial, font);
        g.DrawString(initial, font, textBrush, (44 - size.Width) / 2, (44 - size.Height) / 2);

        return bmp;
    }
}
