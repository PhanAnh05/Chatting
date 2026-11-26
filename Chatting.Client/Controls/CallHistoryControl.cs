using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Controls;

public class CallHistoryControl : UserControl
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;

    public event Action<Guid, Guid?, CallType>? OnCallbackRequested;
    public event Action<Guid>? OnOpenConversationRequested;

    private List<CallInfoDto> _allCalls = new();
    private CallInfoDto? _selectedCall;
    private string _currentFilter = "all"; // all, incoming, outgoing, missed

    // UI Controls
    private readonly Panel pnlHeader = new();
    private readonly Label lblTitle = new();
    private readonly Label lblSubtitle = new();
    private readonly Button btnRefresh = new();

    private readonly Panel pnlFilterBar = new();
    private readonly Panel pnlSearchWrapper = new();
    private readonly Label lblSearchIcon = new();
    private readonly TextBox txtSearch = new();

    private readonly Button btnFilterAll = new();
    private readonly Button btnFilterIncoming = new();
    private readonly Button btnFilterOutgoing = new();
    private readonly Button btnFilterMissed = new();

    private readonly FlowLayoutPanel flpCalls = new();
    private readonly Panel pnlEmptyState = new();
    private readonly Panel pnlEmptyCenter = new();
    private readonly Label lblEmptyIcon = new();
    private readonly Label lblEmptyTitle = new();
    private readonly Label lblEmptySubtitle = new();

    // Detail Panel
    private readonly Panel pnlDetail = new();
    private readonly PictureBox picDetailAvatar = new();
    private readonly Label lblDetailName = new();
    private readonly Label lblDetailStatus = new();
    private readonly Label lblDetailDuration = new();
    private readonly Label lblDetailTime = new();
    private readonly Button btnDetailVoiceCall = new();
    private readonly Button btnDetailVideoCall = new();
    private readonly Button btnDetailChat = new();

    public CallHistoryControl(ApiClient apiClient, SessionManager session)
    {
        _apiClient = apiClient;
        _session = session;

        InitializeComponentManual();
    }

    private void InitializeComponentManual()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        DoubleBuffered = true;

        // ── 1. Top Header ────────────────────────────────────────────────────────
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 68;
        pnlHeader.BackColor = Color.White;
        pnlHeader.Padding = new Padding(24, 14, 24, 10);

        lblTitle.Text = "Lịch sử cuộc gọi";
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(24, 12);
        lblTitle.AutoSize = true;

        lblSubtitle.Text = "Xem và quản lý các cuộc gọi của bạn";
        lblSubtitle.Font = new Font("Segoe UI", 8.5F);
        lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        lblSubtitle.Location = new Point(24, 38);
        lblSubtitle.AutoSize = true;

        btnRefresh.Text = "Làm mới";
        IconHelper.SetButtonIcon(btnRefresh, IconHelper.Refresh, Color.FromArgb(15, 23, 42), 18, 12F, clearText: false);
        btnRefresh.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnRefresh.Size = new Size(100, 34);
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.Location = new Point(Width - 124, 16);
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.FlatAppearance.BorderSize = 1;
        btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        btnRefresh.BackColor = Color.FromArgb(248, 250, 252);
        btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
        btnRefresh.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.Click += async (s, e) => await LoadHistoryAsync();

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(btnRefresh);

        // Header bottom line
        pnlHeader.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        };

        // ── 2. Filter & Search Bar ───────────────────────────────────────────────
        pnlFilterBar.Dock = DockStyle.Top;
        pnlFilterBar.Height = 56;
        pnlFilterBar.BackColor = Color.FromArgb(248, 250, 252);
        pnlFilterBar.Padding = new Padding(24, 10, 24, 10);

        // Search box wrapper
        pnlSearchWrapper.Size = new Size(280, 36);
        pnlSearchWrapper.Location = new Point(24, 10);
        pnlSearchWrapper.BackColor = Color.White;
        pnlSearchWrapper.Padding = new Padding(8, 6, 8, 6);

        IconHelper.SetLabelIcon(lblSearchIcon, IconHelper.Search, Color.FromArgb(148, 163, 184), 11F);
        lblSearchIcon.Location = new Point(6, 8);
        lblSearchIcon.Size = new Size(22, 20);

        txtSearch.BorderStyle = BorderStyle.None;
        txtSearch.Font = new Font("Segoe UI", 9.5F);
        txtSearch.ForeColor = Color.FromArgb(15, 23, 42);
        txtSearch.Location = new Point(32, 8);
        txtSearch.Size = new Size(238, 20);
        txtSearch.PlaceholderText = "Tìm kiếm theo tên hoặc tài khoản...";
        txtSearch.TextChanged += (s, e) => ApplyLocalFilter();

        pnlSearchWrapper.Controls.Add(lblSearchIcon);
        pnlSearchWrapper.Controls.Add(txtSearch);

        // Filter pill buttons
        ConfigureFilterButton(btnFilterAll, "Tất cả", "all", 76);
        ConfigureFilterButton(btnFilterIncoming, "  Cuộc gọi đến", "incoming", 126);
        ConfigureFilterButton(btnFilterOutgoing, "  Cuộc gọi đi", "outgoing", 116);
        ConfigureFilterButton(btnFilterMissed, "  Cuộc gọi nhỡ", "missed", 126);
        LayoutFilterBar();

        pnlFilterBar.Controls.Add(pnlSearchWrapper);
        pnlFilterBar.Controls.Add(btnFilterAll);
        pnlFilterBar.Controls.Add(btnFilterIncoming);
        pnlFilterBar.Controls.Add(btnFilterOutgoing);
        pnlFilterBar.Controls.Add(btnFilterMissed);

        pnlFilterBar.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawLine(pen, 0, pnlFilterBar.Height - 1, pnlFilterBar.Width, pnlFilterBar.Height - 1);
        };

        // ── 3. Detail Info Bar (Bottom) ──────────────────────────────────────────
        pnlDetail.Dock = DockStyle.Bottom;
        pnlDetail.Height = 72;
        pnlDetail.BackColor = Color.FromArgb(248, 250, 252);
        pnlDetail.Padding = new Padding(24, 10, 24, 10);
        pnlDetail.Visible = false;

        picDetailAvatar.Size = new Size(44, 44);
        picDetailAvatar.Location = new Point(24, 14);
        picDetailAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picDetailAvatar.BackColor = Color.FromArgb(59, 130, 246);

        lblDetailName.Location = new Point(78, 14);
        lblDetailName.Size = new Size(260, 22);
        lblDetailName.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblDetailName.ForeColor = Color.FromArgb(15, 23, 42);
        lblDetailName.AutoEllipsis = true;

        lblDetailStatus.Location = new Point(78, 38);
        lblDetailStatus.Size = new Size(400, 20);
        lblDetailStatus.Font = new Font("Segoe UI", 8.5F);
        lblDetailStatus.ForeColor = Color.FromArgb(100, 116, 139);

        // Buttons on right of detail
        btnDetailChat.Size = new Size(110, 38);
        btnDetailChat.Text = "Nhắn tin";
        IconHelper.SetButtonIcon(btnDetailChat, IconHelper.Chat, Color.FromArgb(51, 65, 85), 18, 12F, clearText: false);
        btnDetailChat.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnDetailChat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnDetailChat.FlatStyle = FlatStyle.Flat;
        btnDetailChat.FlatAppearance.BorderSize = 1;
        btnDetailChat.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnDetailChat.BackColor = Color.White;
        btnDetailChat.ForeColor = Color.FromArgb(51, 65, 85);
        btnDetailChat.Cursor = Cursors.Hand;
        btnDetailChat.Click += (s, e) =>
        {
            if (_selectedCall != null)
                OnOpenConversationRequested?.Invoke(_selectedCall.ConversationId);
        };

        btnDetailVoiceCall.Size = new Size(125, 38);
        btnDetailVoiceCall.Text = "Gọi lại thoại";
        IconHelper.SetButtonIcon(btnDetailVoiceCall, IconHelper.Phone, Color.FromArgb(29, 78, 216), 18, 12F, clearText: false);
        btnDetailVoiceCall.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnDetailVoiceCall.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnDetailVoiceCall.FlatStyle = FlatStyle.Flat;
        btnDetailVoiceCall.FlatAppearance.BorderSize = 1;
        btnDetailVoiceCall.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        btnDetailVoiceCall.BackColor = Color.FromArgb(239, 246, 255);
        btnDetailVoiceCall.ForeColor = Color.FromArgb(29, 78, 216);
        btnDetailVoiceCall.Cursor = Cursors.Hand;
        btnDetailVoiceCall.Click += (s, e) =>
        {
            if (_selectedCall != null)
                TriggerCall(_selectedCall, CallType.Audio);
        };

        btnDetailVideoCall.Size = new Size(125, 38);
        btnDetailVideoCall.Text = "Gọi lại video";
        IconHelper.SetButtonIcon(btnDetailVideoCall, IconHelper.Video, Color.White, 18, 12F, clearText: false);
        btnDetailVideoCall.TextImageRelation = TextImageRelation.ImageBeforeText;
        btnDetailVideoCall.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnDetailVideoCall.FlatStyle = FlatStyle.Flat;
        btnDetailVideoCall.FlatAppearance.BorderSize = 0;
        btnDetailVideoCall.BackColor = Color.FromArgb(37, 99, 235);
        btnDetailVideoCall.ForeColor = Color.White;
        btnDetailVideoCall.Cursor = Cursors.Hand;
        btnDetailVideoCall.Click += (s, e) =>
        {
            if (_selectedCall != null)
                TriggerCall(_selectedCall, CallType.Video);
        };

        pnlDetail.Controls.Add(picDetailAvatar);
        pnlDetail.Controls.Add(lblDetailName);
        pnlDetail.Controls.Add(lblDetailStatus);
        pnlDetail.Controls.Add(btnDetailChat);
        pnlDetail.Controls.Add(btnDetailVoiceCall);
        pnlDetail.Controls.Add(btnDetailVideoCall);

        pnlDetail.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawLine(pen, 0, 0, pnlDetail.Width, 0);
        };

        // ── 4. Empty State Panel ─────────────────────────────────────────────────
        pnlEmptyState.Dock = DockStyle.Fill;
        pnlEmptyState.BackColor = Color.White;
        pnlEmptyState.Visible = false;

        pnlEmptyCenter.Size = new Size(540, 160);
        pnlEmptyCenter.BackColor = Color.Transparent;

        IconHelper.SetLabelIcon(lblEmptyIcon, IconHelper.Phone, Color.FromArgb(203, 213, 225), 42F);
        lblEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;
        lblEmptyIcon.Dock = DockStyle.Top;
        lblEmptyIcon.Height = 65;

        lblEmptyTitle.Text = "Chưa có cuộc gọi nào";
        lblEmptyTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblEmptyTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;
        lblEmptyTitle.Dock = DockStyle.Top;
        lblEmptyTitle.Height = 32;

        lblEmptySubtitle.Text = "Các cuộc gọi thoại và video của bạn sẽ hiển thị tại đây sau khi bạn gọi hoặc nhận cuộc gọi.";
        lblEmptySubtitle.Font = new Font("Segoe UI", 9.5F);
        lblEmptySubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblEmptySubtitle.TextAlign = ContentAlignment.MiddleCenter;
        lblEmptySubtitle.Dock = DockStyle.Top;
        lblEmptySubtitle.Height = 34;

        pnlEmptyCenter.Controls.Add(lblEmptySubtitle);
        pnlEmptyCenter.Controls.Add(lblEmptyTitle);
        pnlEmptyCenter.Controls.Add(lblEmptyIcon);

        pnlEmptyState.Controls.Add(pnlEmptyCenter);
        pnlEmptyState.Resize += (s, e) => CenterEmptyState();

        // ── 5. Calls List (Fill) ─────────────────────────────────────────────────
        flpCalls.Dock = DockStyle.Fill;
        flpCalls.AutoScroll = true;
        flpCalls.FlowDirection = FlowDirection.TopDown;
        flpCalls.WrapContents = false;
        flpCalls.BackColor = Color.White;
        flpCalls.Padding = new Padding(16, 8, 16, 16);

        // Assemble controls
        Controls.Add(flpCalls);
        Controls.Add(pnlEmptyState);
        Controls.Add(pnlDetail);
        Controls.Add(pnlFilterBar);
        Controls.Add(pnlHeader);

        UpdateFilterButtonStyles();
    }

    private void ConfigureFilterButton(Button btn, string text, string filterKey, int width)
    {
        btn.Text = text;
        btn.Tag = filterKey;
        btn.Size = new Size(width, 36);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
        btn.Click += (s, e) =>
        {
            _currentFilter = filterKey;
            UpdateFilterButtonStyles();
            ApplyLocalFilter();
        };
    }

    private void UpdateFilterButtonStyles()
    {
        Button[] buttons = { btnFilterAll, btnFilterIncoming, btnFilterOutgoing, btnFilterMissed };
        foreach (var b in buttons)
        {
            bool active = (b.Tag as string) == _currentFilter;
            Color iconColor = active ? Color.White : Color.FromArgb(71, 85, 105);
            if (active)
            {
                b.BackColor = Color.FromArgb(37, 99, 235);
                b.ForeColor = Color.White;
            }
            else
            {
                b.BackColor = Color.FromArgb(241, 245, 249);
                b.ForeColor = Color.FromArgb(71, 85, 105);
            }

            string? glyph = (b.Tag as string) switch
            {
                "incoming" => IconHelper.ArrowDown,
                "outgoing" => IconHelper.ArrowUp,
                "missed" => IconHelper.PhoneMissed,
                _ => null
            };
            if (!string.IsNullOrEmpty(glyph))
            {
                IconHelper.SetButtonIcon(b, glyph, 12, iconColor, ContentAlignment.MiddleLeft);
            }

            if (b.Width > 0 && b.Height > 0)
            {
                var oldR = b.Region;
                b.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, b.Width + 1, b.Height + 1, 8, 8));
                oldR?.Dispose();
            }
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        AdjustLayout();
    }

    private void LayoutFilterBar()
    {
        int x = 24;
        pnlSearchWrapper.Location = new Point(x, 10);
        x += pnlSearchWrapper.Width + 12;

        Button[] buttons = { btnFilterAll, btnFilterIncoming, btnFilterOutgoing, btnFilterMissed };
        foreach (var b in buttons)
        {
            b.Location = new Point(x, 10);
            x += b.Width + 8;
        }
    }

    private void CenterEmptyState()
    {
        pnlEmptyCenter.Location = new Point(
            Math.Max(10, (pnlEmptyState.ClientSize.Width - pnlEmptyCenter.Width) / 2),
            Math.Max(20, (pnlEmptyState.ClientSize.Height - pnlEmptyCenter.Height) / 2));
    }

    private void AdjustLayout()
    {
        btnRefresh.Location = new Point(Width - 124, 16);
        LayoutFilterBar();
        CenterEmptyState();

        // Round search wrapper
        if (pnlSearchWrapper.Width > 0 && pnlSearchWrapper.Height > 0)
        {
            var oldR = pnlSearchWrapper.Region;
            pnlSearchWrapper.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlSearchWrapper.Width + 1, pnlSearchWrapper.Height + 1, 8, 8));
            oldR?.Dispose();
        }

        // Layout detail buttons on right
        btnDetailVideoCall.Location = new Point(pnlDetail.Width - 24 - btnDetailVideoCall.Width, (pnlDetail.Height - btnDetailVideoCall.Height) / 2);
        btnDetailVoiceCall.Location = new Point(btnDetailVideoCall.Left - 8 - btnDetailVoiceCall.Width, (pnlDetail.Height - btnDetailVoiceCall.Height) / 2);
        btnDetailChat.Location = new Point(btnDetailVoiceCall.Left - 8 - btnDetailChat.Width, (pnlDetail.Height - btnDetailChat.Height) / 2);

        // Adjust items width in flpCalls
        int itemW = Math.Max(300, flpCalls.ClientSize.Width - 36);
        foreach (Control c in flpCalls.Controls)
        {
            if (c is CallHistoryItemControl item)
                item.Width = itemW;
        }

        // Detail buttons rounded
        RoundControl(btnDetailVideoCall, 8);
        RoundControl(btnDetailVoiceCall, 8);
        RoundControl(btnDetailChat, 8);
        RoundControl(btnRefresh, 8);
        if (picDetailAvatar.Width > 0)
        {
            var oldR = picDetailAvatar.Region;
            picDetailAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picDetailAvatar.Width, picDetailAvatar.Height, picDetailAvatar.Width, picDetailAvatar.Height));
            oldR?.Dispose();
        }
    }

    private void RoundControl(Control c, int radius)
    {
        if (c.Width <= 0 || c.Height <= 0) return;
        var oldR = c.Region;
        c.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, c.Width + 1, c.Height + 1, radius, radius));
        oldR?.Dispose();
    }

    /// <summary>
    /// Loads call history asynchronously from server and renders items.
    /// Does NOT initiate any calls.
    /// </summary>
    public async Task LoadHistoryAsync()
    {
        btnRefresh.Enabled = false;
        btnRefresh.Text = "Đang tải...";
        IconHelper.SetButtonIcon(btnRefresh, IconHelper.Refresh, Color.FromArgb(100, 116, 139), 18, 12F, clearText: false);

        try
        {
            var res = await _apiClient.GetCallHistoryAsync(pageSize: 100);
            if (res?.Success == true && res.Data?.Items != null)
            {
                _allCalls = res.Data.Items;
                ApplyLocalFilter();
            }
            else
            {
                _allCalls = new();
                ApplyLocalFilter();
            }
        }
        catch (Exception)
        {
            _allCalls = new();
            ApplyLocalFilter();
        }
        finally
        {
            btnRefresh.Enabled = true;
            btnRefresh.Text = "Làm mới";
            IconHelper.SetButtonIcon(btnRefresh, IconHelper.Refresh, Color.FromArgb(15, 23, 42), 18, 12F, clearText: false);
        }
    }

    private void ApplyLocalFilter()
    {
        Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;
        string query = txtSearch.Text.Trim().ToLowerInvariant();

        var filtered = _allCalls.Where(c =>
        {
            // Search text
            if (!string.IsNullOrWhiteSpace(query))
            {
                bool matchCaller = (c.CallerUsername?.ToLowerInvariant().Contains(query) ?? false) ||
                                   (c.CallerDisplayName?.ToLowerInvariant().Contains(query) ?? false);
                bool matchReceiver = (c.ReceiverUsername?.ToLowerInvariant().Contains(query) ?? false) ||
                                     (c.ReceiverDisplayName?.ToLowerInvariant().Contains(query) ?? false);
                if (!matchCaller && !matchReceiver) return false;
            }

            // Filter tab
            if (_currentFilter == "incoming")
            {
                return c.ReceiverId == myId;
            }
            else if (_currentFilter == "outgoing")
            {
                return c.CallerId == myId;
            }
            else if (_currentFilter == "missed")
            {
                return (c.ReceiverId == myId && c.Status == CallStatus.Missed) || c.Status == CallStatus.Missed;
            }

            return true;
        }).OrderByDescending(c => c.StartedAt).ToList();

        RenderList(filtered);
    }

    private void RenderList(List<CallInfoDto> calls)
    {
        flpCalls.SuspendLayout();
        flpCalls.Controls.Clear();

        Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;

        if (calls.Count == 0)
        {
            pnlEmptyState.Visible = true;
            flpCalls.Visible = false;
            pnlDetail.Visible = false;
            CenterEmptyState();
            flpCalls.ResumeLayout();
            return;
        }

        pnlEmptyState.Visible = false;
        flpCalls.Visible = true;

        int itemW = Math.Max(300, flpCalls.ClientSize.Width - 36);

        foreach (var call in calls)
        {
            var item = new CallHistoryItemControl(call, myId, _session.ServerUrl)
            {
                Width = itemW
            };

            item.ItemClicked += OnItemSelect;
            item.VoiceCallbackClicked += (c) => TriggerCall(c, CallType.Audio);
            item.VideoCallbackClicked += (c) => TriggerCall(c, CallType.Video);
            item.OpenChatClicked += (convId) => OnOpenConversationRequested?.Invoke(convId);

            flpCalls.Controls.Add(item);
        }

        flpCalls.ResumeLayout();

        // If previously selected call still in list, keep selection
        if (_selectedCall != null)
        {
            var match = calls.FirstOrDefault(c => c.CallId == _selectedCall.CallId);
            if (match != null)
                ShowDetail(match);
            else
                pnlDetail.Visible = false;
        }
        else
        {
            pnlDetail.Visible = false;
        }
    }

    private void OnItemSelect(CallInfoDto call)
    {
        _selectedCall = call;

        foreach (Control c in flpCalls.Controls)
        {
            if (c is CallHistoryItemControl item)
                item.SetSelected(item.Call.CallId == call.CallId);
        }

        ShowDetail(call);
    }

    private void ShowDetail(CallInfoDto call)
    {
        _selectedCall = call;
        Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;
        bool isMeCaller = call.CallerId == myId;

        string contactName = isMeCaller
            ? (!string.IsNullOrWhiteSpace(call.ReceiverDisplayName) ? call.ReceiverDisplayName : (call.ReceiverUsername ?? "Người dùng"))
            : (!string.IsNullOrWhiteSpace(call.CallerDisplayName) ? call.CallerDisplayName : call.CallerUsername);

        lblDetailName.Text = contactName;
        picDetailAvatar.Image = GenerateAvatarImage(contactName);
        string? avatarUrl = isMeCaller ? call.ReceiverAvatarUrl : call.CallerAvatarUrl;
        _ = LoadDetailAvatarAsync(avatarUrl);

        string dirStr = isMeCaller ? "Cuộc gọi đi" : "Cuộc gọi đến";
        string typeStr = call.CallType == CallType.Video ? "Video" : "Thoại";
        string statusStr = call.Status switch
        {
            CallStatus.Ended => "Đã kết nối và kết thúc bình thường",
            CallStatus.Ongoing => "Đang diễn ra",
            CallStatus.Missed => "Cuộc gọi nhỡ",
            CallStatus.Rejected => "Đã bị từ chối",
            _ => "Không thành công"
        };

        string durStr = "";
        if (call.DurationSeconds.HasValue && call.DurationSeconds > 0)
        {
            var ts = TimeSpan.FromSeconds(call.DurationSeconds.Value);
            durStr = $" • Thời lượng: {(ts.Hours > 0 ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}" : $"{ts.Minutes:D2}:{ts.Seconds:D2}")}";
        }

        lblDetailStatus.Text = $"{dirStr} ({typeStr}) • {statusStr}{durStr} • {VietnamTime.FromUtc(call.StartedAt):dd/MM/yyyy HH:mm}";

        pnlDetail.Visible = true;
        AdjustLayout();
    }

    private async Task LoadDetailAvatarAsync(string? avatarUrl)
    {
        Image? source = await AvatarImageCache.GetAsync(_session.ServerUrl, avatarUrl);
        if (source is null || IsDisposed || picDetailAvatar.IsDisposed)
            return;

        Bitmap rendered = AvatarImageCache.CreateCircularFit(source, picDetailAvatar.Size, Color.White, Color.FromArgb(226, 232, 240));
        if (IsDisposed || picDetailAvatar.IsDisposed)
        {
            rendered.Dispose();
            return;
        }

        Image? old = picDetailAvatar.Image;
        picDetailAvatar.Image = rendered;
        old?.Dispose();
    }

    private void TriggerCall(CallInfoDto call, CallType type)
    {
        Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;
        Guid? targetContactId = (call.CallerId == myId) ? call.ReceiverId : call.CallerId;

        OnCallbackRequested?.Invoke(call.ConversationId, targetContactId, type);
    }

    private static Image GenerateAvatarImage(string name)
    {
        var bmp = new Bitmap(44, 44);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

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
