using Chatting.Client.Controls;
using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Forms;

public partial class MainMessengerForm : Form, IMessageFilter
{
    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;
    private readonly SignalRService _signalR;
    private readonly AudioRecorderService _audioService;

    private List<ConversationDto> _conversations = new();
    private readonly Dictionary<Guid, ConversationListItemControl> _conversationItems = new();
    private ConversationDto? _activeConversation;
    private bool _isRecordingVoice = false;
    private System.Windows.Forms.Timer? _typingTimer;
    private System.Windows.Forms.Timer? _toastTimer;
    private Form? _activeCallWindow = null;
    private readonly CallHistoryControl _callHistoryControl;
    private readonly UcSharedContent _ucSharedContent;

    // AI Assistant state
    private Guid _aiConversationId = Guid.Empty;
    private double _aiFloatTick = 0;
    private int _aiBaseY = 0;
    private bool _isAiBubbleHovered = false;
    private bool _isAiChatActive = false;

    // Search matches within active conversation
    private List<MessageBubbleControl> _searchMatches = new();
    private int _searchMatchIndex = -1;
    private int _messageLoadVersion;

    // Win32 APIs for custom titlebar dragging and rounded corners
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    public MainMessengerForm(ApiClient apiClient, SessionManager session, SignalRService signalR)
    {
        _apiClient = apiClient;
        _session = session;
        _signalR = signalR;
        _audioService = new AudioRecorderService();

        InitializeComponent();
        Application.AddMessageFilter(this);

        _callHistoryControl = new CallHistoryControl(_apiClient, _session)
        {
            Dock = DockStyle.Fill
        };
        _callHistoryControl.OnCallbackRequested += async (convId, receiverId, type) =>
        {
            await StartCallWithUserAsync(convId, receiverId, type);
        };
        _callHistoryControl.OnOpenConversationRequested += async (convId) =>
        {
            ShowChatView();
            var conv = _conversations.FirstOrDefault(c => c.Id == convId);
            if (conv != null)
            {
                SelectConversation(conv);
            }
            else
            {
                var res = await _apiClient.GetConversationAsync(convId);
                if (res?.Success == true && res.Data != null)
                {
                    await LoadConversationsAsync();
                    var target = _conversations.FirstOrDefault(c => c.Id == convId) ?? res.Data;
                    SelectConversation(target);
                }
            }
        };
        pnlCallHistory.Controls.Add(_callHistoryControl);

        _ucSharedContent = new UcSharedContent(_apiClient, _session)
        {
            Dock = DockStyle.Right,
            Width = 350,
            Visible = false
        };
        _ucSharedContent.OnCloseRequested += () => _ = ToggleSharedContentAsync(false);
        pnlChatArea.Controls.Add(_ucSharedContent);

        // Precedence: pnlChatHeader at top, pnlMessageSearch directly below header, _ucSharedContent on right, pnlInputBar at bottom
        pnlInputBar.SendToBack();
        _ucSharedContent.SendToBack();
        pnlMessageSearch.SendToBack();
        pnlChatHeader.SendToBack();
        flpMessages.BringToFront();
        pnlEmptyState.BringToFront();
        pnlToast.BringToFront();
        pnlMoreMenu.BringToFront();

        ApplyTheme();

        // Search Input Container styling & events
        pnlSearchInputContainer.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            bool isFocused = txtSearchMessageKeyword.Focused;
            Color borderColor = isFocused ? Color.FromArgb(96, 165, 250)
                : (_session.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225));
            using var pen = new Pen(borderColor, isFocused ? 1.5f : 1f);
            using var path = CreateRoundedRectanglePath(new Rectangle(0, 0, pnlSearchInputContainer.Width - 1, pnlSearchInputContainer.Height - 1), 6);
            e.Graphics.DrawPath(pen, path);
        };

        txtSearchMessageKeyword.GotFocus += (s, e) => pnlSearchInputContainer.Invalidate();
        txtSearchMessageKeyword.LostFocus += (s, e) => pnlSearchInputContainer.Invalidate();

        btnSearchClear.MouseEnter += (s, e) => btnSearchClear.ForeColor = Color.FromArgb(71, 85, 105);
        btnSearchClear.MouseLeave += (s, e) => btnSearchClear.ForeColor = Color.FromArgb(148, 163, 184);

        // Ensure robot bubble stays on top of all form controls
        picAiRobotBubble.BringToFront();
    }

    public bool PreFilterMessage(ref Message m)
    {
        // 0x0201: WM_LBUTTONDOWN, 0x0204: WM_RBUTTONDOWN, 0x00A1: WM_NCLBUTTONDOWN
        if (m.Msg == 0x0201 || m.Msg == 0x0204 || m.Msg == 0x00A1)
        {
            if (pnlMoreMenu.Visible)
            {
                Point screenPoint = Cursor.Position;
                Point menuClient = pnlMoreMenu.PointToClient(screenPoint);
                Point btnClient = btnMoreMenu.PointToClient(screenPoint);
                if (!pnlMoreMenu.ClientRectangle.Contains(menuClient) &&
                    !btnMoreMenu.ClientRectangle.Contains(btnClient))
                {
                    HideMoreMenu();
                }
            }
        }
        return false;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Application.RemoveMessageFilter(this);
        _signalR.OnMessageEdited -= HandleMessageEdited;
        _signalR.OnMessagePinUpdated -= HandleMessagePinUpdated;
        _signalR.OnMessageRevoked -= HandleMessageRevoked;
        _signalR.OnMessageHiddenForMe -= HandleMessageHiddenForMe;
        _signalR.OnMessageReactionsUpdated -= HandleMessageReactionsUpdated;
        _signalR.OnGroupUpdated -= HandleGroupUpdated;
        _signalR.OnMembersAdded -= HandleMembersAdded;
        _signalR.OnMemberRemoved -= HandleMemberRemoved;
        _signalR.OnYouWereRemovedFromGroup -= HandleYouWereRemovedFromGroup;
        _signalR.OnMemberLeft -= HandleMemberLeft;
        _signalR.OnNewConversationCreated -= HandleNewConversationCreated;
        base.OnFormClosed(e);
    }

    private async void MainMessengerForm_Load(object? sender, EventArgs e)
    {
        // Custom window drag on titlebar
        pnlTitleBar.MouseDown += TitleBar_MouseDown;
        lblAppTitle.MouseDown += TitleBar_MouseDown;
        lblTitleLogo.MouseDown += TitleBar_MouseDown;

        // Double click titlebar to toggle maximize
        pnlTitleBar.DoubleClick += (s, ev) => btnMaximize_Click(s, ev);
        lblAppTitle.DoubleClick += (s, ev) => btnMaximize_Click(s, ev);

        // Setup User Info
        if (_session.CurrentUser != null)
        {
            lblMyUsername.Text = _session.CurrentUser.DisplayName ?? _session.CurrentUser.Username;
            picMyAvatar.Image = GenerateAvatarBitmap(_session.CurrentUser.Username);
            _ = LoadAvatarIntoAsync(picMyAvatar, _session.CurrentUser.AvatarUrl);
        }

        notifyIconTray.Icon = SystemIcons.Application;
        picMyAvatar.Click += picMyAvatar_Click;
        picMyAvatar.MouseUp += picMyAvatar_MouseUp;
        lblMyUsername.Cursor = Cursors.Hand;
        lblMyUsername.Click += async (s, e) => await EditMyProfileAsync();
        toolTipMain.ShowAlways = true;
        toolTipMain.SetToolTip(picMyAvatar, "Đổi hoặc xem ảnh đại diện");
        btnMinimize.AccessibleName = "Thu nhỏ cửa sổ";
        btnMaximize.AccessibleName = "Phóng to hoặc khôi phục cửa sổ";
        btnCloseWindow.AccessibleName = "Đóng Chatting";
        toolTipMain.SetToolTip(btnMinimize, "Thu nhỏ");
        toolTipMain.SetToolTip(btnMaximize, "Phóng to / Khôi phục");
        toolTipMain.SetToolTip(btnCloseWindow, "Đóng Chatting");
        (Button Button, string Name)[] iconButtons =
        [
            (btnMinimize, "Thu nhỏ cửa sổ"),
            (btnMaximize, "Phóng to hoặc khôi phục cửa sổ"),
            (btnCloseWindow, "Đóng Chatting"),
            (btnLogout, "Đăng xuất"),
            (btnNewGroup, "Tạo nhóm chat mới"),
            (btnOpenFriends, "Quản lý bạn bè"),
            (btnNavChat, "Đoạn chat"),
            (btnNavCalls, "Lịch sử cuộc gọi"),
            (btnNavFiles, "Nội dung đã chia sẻ"),
            (btnNavSettings, "Cài đặt máy chủ"),
            (btnSearch, "Tìm kiếm tin nhắn"),
            (btnVoiceCall, "Gọi thoại"),
            (btnVideoCall, "Gọi video"),
            (btnGroupInfo, "Quản lý nhóm"),
            (btnAttachFileHeader, "Nội dung đã chia sẻ"),
            (btnMoreMenu, "Tùy chọn cuộc trò chuyện"),
            (btnSendMessage, "Gửi tin nhắn"),
            (btnRecordVoice, "Ghi âm tin nhắn thoại"),
            (btnAttachFile, "Đính kèm tệp hoặc hình ảnh"),
            (btnEmoji, "Chèn biểu tượng cảm xúc"),
            (btnSearchClear, "Xóa từ khóa tìm kiếm"),
            (btnSearchPrev, "Tin nhắn trước đó"),
            (btnSearchNext, "Tin nhắn tiếp theo"),
            (btnCloseSearch, "Đóng tìm kiếm tin nhắn")
        ];
        foreach ((Button button, string name) in iconButtons)
        {
            button.AccessibleName = name;
            string? description = toolTipMain.GetToolTip(button);
            if (!string.IsNullOrWhiteSpace(description))
                button.AccessibleDescription = description;
        }
        picChatAvatar.Cursor = Cursors.Hand;
        picChatAvatar.Click += (s, e) => ShowActiveAvatarPreview();
        picChatAvatar.MouseUp += (s, e) => { if (e.Button == MouseButtons.Right) _ = EditNicknamesAsync(); };
        lblChatTitle.Cursor = Cursors.Hand;
        lblChatTitle.Click += async (s, e) => await EditNicknamesAsync();

        // Wire emoji button
        btnEmoji.Click += btnEmoji_Click;

        // Wire More Menu items hover and clicks
        WireMoreMenu();

        pnlChatArea.Resize += (s, ev) =>
        {
            if (pnlMoreMenu.Visible)
            {
                pnlMoreMenu.Location = new Point(Math.Max(0, pnlChatArea.Width - pnlMoreMenu.Width - 14), pnlChatHeader.Height + 4);
            }
        };

        pnlInputBar.Resize += (s, ev) => LayoutInputBar();
        pnlInputBar.Layout += (s, ev) => LayoutInputBar();

        flpConversations.Resize += (s, ev) =>
        {
            int w = flpConversations.ClientSize.Width - flpConversations.Padding.Horizontal;
            if (w > 180)
            {
                foreach (Control c in flpConversations.Controls)
                {
                    if (c is ConversationListItemControl item)
                        item.Width = w;
                }
            }
        };

        flpMessages.Resize += (s, ev) =>
        {
            int w = flpMessages.ClientSize.Width - 25;
            if (w > 200)
            {
                foreach (Control c in flpMessages.Controls)
                {
                    if (c is MessageBubbleControl bubble && bubble.MessageData != null)
                    {
                        bubble.Width = w;
                    }
                }
            }
        };

        // Apply UI polish: rounded corners, borders, etc.
        ApplyUiPolish();

        // Setup SignalR handlers
        _signalR.OnReceiveMessage += HandleReceiveMessage;
        _signalR.OnUserTyping += HandleUserTyping;
        _signalR.OnMessagesRead += HandleMessagesRead;
        _signalR.OnMessageEdited += HandleMessageEdited;
        _signalR.OnMessagePinUpdated += HandleMessagePinUpdated;
        _signalR.OnMessageRevoked += HandleMessageRevoked;
        _signalR.OnMessageHiddenForMe += HandleMessageHiddenForMe;
        _signalR.OnMessageReactionsUpdated += HandleMessageReactionsUpdated;
        _signalR.OnGroupUpdated += HandleGroupUpdated;
        _signalR.OnMembersAdded += HandleMembersAdded;
        _signalR.OnMemberRemoved += HandleMemberRemoved;
        _signalR.OnYouWereRemovedFromGroup += HandleYouWereRemovedFromGroup;
        _signalR.OnMemberLeft += HandleMemberLeft;
        _signalR.OnIncomingCall += HandleIncomingCall;
        _signalR.OnCallStarted += HandleCallStarted;
        _signalR.OnCallEnded += (dto) =>
        {
            SoundNotificationService.Instance.StopRingtone();
            if (pnlCallHistory.Visible)
            {
                BeginInvoke(async () => await _callHistoryControl.LoadHistoryAsync());
            }
        };
        _signalR.OnNewConversationCreated += HandleNewConversationCreated;
        _signalR.OnFriendRequestAccepted += (FriendRequestDto req) => { BeginInvoke(async () => await LoadConversationsAsync()); };
        _signalR.OnFriendStatusChanged += (string raw) => { BeginInvoke(async () => await LoadConversationsAsync()); };
        _signalR.OnUserProfileUpdated += HandleUserProfileUpdated;
        _signalR.OnNicknameUpdated += HandleNicknameUpdated;

        try
        {
            await _signalR.ConnectAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể kết nối SignalR: {ex.Message}\r\nCác chức năng thời gian thực có thể bị chậm.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        await LoadConversationsAsync();

        // Initialize floating AI robot bubble
        InitAiRobotBubble();

        // Show empty state initially
        ShowEmptyState(true);
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(Handle, 0xA1, 0x2, 0);
        }
    }

    private void btnMinimize_Click(object? sender, EventArgs e)
    {
        WindowState = FormWindowState.Minimized;
    }

    private void btnMaximize_Click(object? sender, EventArgs e)
    {
        WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        IconHelper.SetButtonIcon(btnMaximize, WindowState == FormWindowState.Maximized ? IconHelper.Restore : IconHelper.Maximize, _session.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139), 18, 11F);
    }

    private void btnCloseWindow_Click(object? sender, EventArgs e)
    {
        Close();
    }

    /// <summary>Apply rounded corners and visual polish.</summary>
    private void ApplyUiPolish()
    {
        // Round avatars
        picMyAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picMyAvatar.Width, picMyAvatar.Height, picMyAvatar.Width, picMyAvatar.Height));
        picChatAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picChatAvatar.Width, picChatAvatar.Height, picChatAvatar.Width, picChatAvatar.Height));

        // Round navigation chat button
        btnNavChat.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnNavChat.Width, btnNavChat.Height, 14, 14));

        // Layout message input bar dynamically
        LayoutInputBar();

        // Round small icon buttons in header
        foreach (var btn in new[] { btnVoiceCall, btnVideoCall, btnGroupInfo, btnSearch, btnAttachFileHeader, btnMoreMenu })
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 10, 10));

        // Round sidebar action buttons
        foreach (var btn in new[] { btnOpenFriends, btnNewGroup, btnLogout })
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 10, 10));

        // Round more menu
        pnlMoreMenu.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlMoreMenu.Width, pnlMoreMenu.Height, 12, 12));

        // Round toast banner
        pnlToast.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlToast.Width, pnlToast.Height, 16, 16));

        // Header bottom separator
        pnlChatHeader.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawLine(pen, 0, pnlChatHeader.Height - 1, pnlChatHeader.Width, pnlChatHeader.Height - 1);
        };

        // Input bar top separator
        pnlInputBar.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawLine(pen, 0, 0, pnlInputBar.Width, 0);
        };

        // Search box: rounded border rectangle behind textbox
        pnlSearch.Paint += (s, pe) =>
        {
            var rect = new Rectangle(10, 6, pnlSearch.Width - 20, pnlSearch.Height - 12);
            using var path = new GraphicsPath();
            int r = 10;
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseAllFigures();
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(30, 43, 69));
            pe.Graphics.FillPath(brush, path);
        };

        // Click outside closes more menu
        flpMessages.Click += (s, e) => HideMoreMenu();
        pnlChatHeader.Click += (s, e) => HideMoreMenu();
        pnlInputBar.Click += (s, e) => HideMoreMenu();
        pnlSidebar.Click += (s, e) => HideMoreMenu();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Three Dots Menu (⋯) Wire & Actions
    // ─────────────────────────────────────────────────────────────────────────
    private void WireMoreMenu()
    {
        // Setup item hover effects and clicks
        SetupMenuItem(itemUserInfo, () => ShowUserInfo());
        SetupMenuItem(itemSearch, () => ShowMessageSearch());
        SetupMenuItem(itemDelete, () => ConfirmDeleteConversation());
        SetupMenuItem(itemBlock, () => ToggleBlockUser());
        SetupMenuItem(itemRefresh, async () => await RefreshActiveConversationAsync());
    }

    private void SetupMenuItem(Panel itemPanel, Action action)
    {
        Color hoverBg = _session.IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
        Color normalBg = _session.IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;

        itemPanel.MouseEnter += (s, e) => itemPanel.BackColor = hoverBg;
        itemPanel.MouseLeave += (s, e) =>
        {
            if (!itemPanel.ClientRectangle.Contains(itemPanel.PointToClient(MousePosition)))
                itemPanel.BackColor = normalBg;
        };
        itemPanel.Click += (s, e) =>
        {
            HideMoreMenu();
            action();
        };

        foreach (Control c in itemPanel.Controls)
        {
            c.MouseEnter += (s, e) => itemPanel.BackColor = hoverBg;
            c.MouseLeave += (s, e) =>
            {
                if (!itemPanel.ClientRectangle.Contains(itemPanel.PointToClient(MousePosition)))
                    itemPanel.BackColor = normalBg;
            };
            c.Click += (s, e) =>
            {
                HideMoreMenu();
                action();
            };
        }
    }

    private void btnMoreMenu_Click(object? sender, EventArgs e)
    {
        if (pnlMoreMenu.Visible)
        {
            HideMoreMenu();
        }
        else
        {
            ShowMoreMenu();
        }
    }

    private void ShowMoreMenu()
    {
        if (_activeConversation == null) return;

        // Update block label based on current state
        bool isDirect = _activeConversation.Type == ConversationType.Direct;
        itemBlock.Visible = isDirect;

        if (isDirect)
        {
            var other = _activeConversation.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser?.Id);
            bool isBlocked = other != null && _session.IsUserBlocked(other.UserId);

            lblBlockTitle.Text = isBlocked ? "Bỏ chặn người dùng" : "Chặn người dùng";
            lblBlockSub.Text = isBlocked ? "Cho phép nhận tin nhắn lại" : "Ngừng nhận tin nhắn từ người này";
            IconHelper.SetLabelIcon(lblBlockIcon, isBlocked ? IconHelper.Check : IconHelper.Block, isBlocked ? IconHelper.StatusOnline : IconHelper.StatusDanger, 14F);
            lblBlockTitle.ForeColor = isBlocked ? Color.FromArgb(22, 163, 74) : Color.FromArgb(15, 23, 42);
        }

        // Position directly below btnMoreMenu
        int x = pnlChatArea.Width - pnlMoreMenu.Width - 14;
        int y = pnlChatHeader.Height + 4;
        pnlMoreMenu.Location = new Point(x, y);
        pnlMoreMenu.Visible = true;
        pnlMoreMenu.BringToFront();
    }

    private void HideMoreMenu()
    {
        pnlMoreMenu.Visible = false;
    }

    // 1. Thông tin người dùng
    private void ShowUserInfo()
    {
        if (_activeConversation == null) return;

        if (_activeConversation.Type == ConversationType.Direct)
        {
            var other = _activeConversation.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser?.Id);
            if (other != null)
            {
                using var dlg = new UserInfoForm(other.Username,
                    string.IsNullOrWhiteSpace(other.Nickname) ? other.ProfileDisplayName : other.Nickname,
                    null, other.IsOnline, other.Bio, _session.IsDarkMode);
                dlg.ShowDialog(this);
            }
        }
        else
        {
            using var groupForm = new GroupManagerForm(_apiClient, _session, _activeConversation.ChatGroupId ?? Guid.Empty);
            groupForm.ShowDialog(this);
        }
    }

    // 2. Tìm kiếm tin nhắn
    private void btnSearch_Click(object? sender, EventArgs e)
    {
        ShowMessageSearch();
    }

    private void ShowMessageSearch()
    {
        if (_activeConversation == null) return;
        pnlMessageSearch.Visible = true;
        pnlMessageSearch.SendToBack();
        pnlChatHeader.SendToBack();
        IconHelper.ApplyToolbarButtonStyle(btnSearch, IconHelper.Search, _session.IsDarkMode, isActive: true);
        pnlChatArea.PerformLayout();
        txtSearchMessageKeyword.Focus();
        txtSearchMessageKeyword.SelectAll();
    }

    private void btnCloseSearch_Click(object? sender, EventArgs e)
    {
        pnlMessageSearch.Visible = false;
        txtSearchMessageKeyword.Clear();
        btnSearchClear.Visible = false;
        ClearSearchHighlights();
        _searchMatches.Clear();
        _searchMatchIndex = -1;
        IconHelper.ApplyToolbarButtonStyle(btnSearch, IconHelper.Search, _session.IsDarkMode, isActive: false);
        pnlChatArea.PerformLayout();
    }

    private void btnSearchClear_Click(object? sender, EventArgs e)
    {
        txtSearchMessageKeyword.Clear();
        btnSearchClear.Visible = false;
        ClearSearchHighlights();
        _searchMatches.Clear();
        _searchMatchIndex = -1;
        lblSearchMatchCount.Text = "0 kết quả";
        txtSearchMessageKeyword.Focus();
    }

    private void ClearSearchHighlights()
    {
        foreach (Control c in flpMessages.Controls)
        {
            if (c is MessageBubbleControl bubble)
            {
                bubble.SetSearchHighlight(false, false, null);
            }
        }
    }

    private void txtSearchMessageKeyword_TextChanged(object? sender, EventArgs e)
    {
        string kw = txtSearchMessageKeyword.Text.Trim();
        btnSearchClear.Visible = !string.IsNullOrEmpty(txtSearchMessageKeyword.Text);

        ClearSearchHighlights();
        _searchMatches.Clear();
        _searchMatchIndex = -1;

        if (string.IsNullOrEmpty(kw))
        {
            lblSearchMatchCount.Text = "0 kết quả";
            return;
        }

        foreach (Control c in flpMessages.Controls)
        {
            if (c is MessageBubbleControl bubble && bubble.MessageData != null)
            {
                string content = bubble.MessageData.Content ?? "";
                if (bubble.MessageData.Type != MessageType.System &&
                    content.Contains(kw, StringComparison.OrdinalIgnoreCase))
                {
                    _searchMatches.Add(bubble);
                }
            }
        }

        if (_searchMatches.Count == 0)
        {
            lblSearchMatchCount.Text = "0 kết quả";
            return;
        }

        // Highlight all matching messages
        foreach (var bubble in _searchMatches)
        {
            bubble.SetSearchHighlight(true, false, kw);
        }

        // Active match starts at the latest match
        _searchMatchIndex = _searchMatches.Count - 1;
        _searchMatches[_searchMatchIndex].SetSearchHighlight(true, true, kw);
        lblSearchMatchCount.Text = $"{_searchMatchIndex + 1}/{_searchMatches.Count} kết quả";

        ScrollMessageToCenter(_searchMatches[_searchMatchIndex], smooth: true);
    }

    private void btnSearchPrev_Click(object? sender, EventArgs e)
    {
        if (_searchMatches.Count == 0) return;

        string kw = txtSearchMessageKeyword.Text.Trim();

        // Demote previous active match
        if (_searchMatchIndex >= 0 && _searchMatchIndex < _searchMatches.Count)
        {
            _searchMatches[_searchMatchIndex].SetSearchHighlight(true, false, kw);
        }

        _searchMatchIndex--;
        if (_searchMatchIndex < 0) _searchMatchIndex = _searchMatches.Count - 1;

        // Promote new active match
        _searchMatches[_searchMatchIndex].SetSearchHighlight(true, true, kw);
        lblSearchMatchCount.Text = $"{_searchMatchIndex + 1}/{_searchMatches.Count} kết quả";

        ScrollMessageToCenter(_searchMatches[_searchMatchIndex], smooth: true);
    }

    private void btnSearchNext_Click(object? sender, EventArgs e)
    {
        if (_searchMatches.Count == 0) return;

        string kw = txtSearchMessageKeyword.Text.Trim();

        // Demote previous active match
        if (_searchMatchIndex >= 0 && _searchMatchIndex < _searchMatches.Count)
        {
            _searchMatches[_searchMatchIndex].SetSearchHighlight(true, false, kw);
        }

        _searchMatchIndex++;
        if (_searchMatchIndex >= _searchMatches.Count) _searchMatchIndex = 0;

        // Promote new active match
        _searchMatches[_searchMatchIndex].SetSearchHighlight(true, true, kw);
        lblSearchMatchCount.Text = $"{_searchMatchIndex + 1}/{_searchMatches.Count} kết quả";

        ScrollMessageToCenter(_searchMatches[_searchMatchIndex], smooth: true);
    }

    private System.Windows.Forms.Timer? _smoothScrollTimer;
    private int _scrollStartY;
    private int _scrollTargetY;
    private int _scrollStepCount;
    private int _scrollCurrentStep;

    private void ScrollMessageToCenter(MessageBubbleControl bubble, bool smooth = true)
    {
        if (bubble == null || !flpMessages.Controls.Contains(bubble)) return;

        int currentScrollY = Math.Abs(flpMessages.AutoScrollPosition.Y);
        int bubbleCenterInViewport = bubble.Top + (bubble.Height / 2);
        int viewportCenter = flpMessages.ClientSize.Height / 2;

        int deltaY = bubbleCenterInViewport - viewportCenter;
        int targetScrollY = Math.Max(0, currentScrollY + deltaY);

        if (!smooth)
        {
            _smoothScrollTimer?.Stop();
            _smoothScrollTimer?.Dispose();
            _smoothScrollTimer = null;
            flpMessages.AutoScrollPosition = new Point(0, targetScrollY);
            return;
        }

        _smoothScrollTimer?.Stop();
        _smoothScrollTimer?.Dispose();

        _scrollStartY = currentScrollY;
        _scrollTargetY = targetScrollY;
        _scrollStepCount = 10;
        _scrollCurrentStep = 0;

        _smoothScrollTimer = new System.Windows.Forms.Timer { Interval = 15 };
        _smoothScrollTimer.Tick += (s, e) =>
        {
            _scrollCurrentStep++;
            float t = (float)_scrollCurrentStep / _scrollStepCount;
            float ease = 1f - (float)Math.Pow(1f - t, 3);
            int currentY = (int)(_scrollStartY + (_scrollTargetY - _scrollStartY) * ease);

            flpMessages.AutoScrollPosition = new Point(0, Math.Max(0, currentY));

            if (_scrollCurrentStep >= _scrollStepCount)
            {
                _smoothScrollTimer.Stop();
                _smoothScrollTimer.Dispose();
                _smoothScrollTimer = null;
                flpMessages.AutoScrollPosition = new Point(0, Math.Max(0, _scrollTargetY));
            }
        };
        _smoothScrollTimer.Start();
    }

    private void txtSearchMessageKeyword_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            btnSearchNext_Click(sender, e);
        }
        else if (e.KeyCode == Keys.Escape)
        {
            btnCloseSearch_Click(sender, e);
        }
    }

    // 3. Xóa cuộc trò chuyện
    private async void ConfirmDeleteConversation()
    {
        if (_activeConversation == null) return;

        var ask = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa cuộc trò chuyện này khỏi danh sách của bạn không?\r\n\r\n" +
            "Lưu ý quan trọng: Thao tác này chỉ xóa và ẩn lịch sử trò chuyện trên máy của bạn, " +
            "hoàn toàn KHÔNG xóa tin nhắn ở phía người còn lại.",
            "Xác Nhận Xóa Cuộc Trò Chuyện",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (ask == DialogResult.Yes)
        {
            Guid convId = _activeConversation.Id;
            try
            {
                var res = await _apiClient.DeleteConversationAsync(convId);
                if (res != null && !res.Success)
                {
                    ShowToast(res.Message ?? "Không thể xóa cuộc trò chuyện.", ToastType.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeleteConversation] Error: {ex.Message}");
            }

            _conversations.RemoveAll(c => c.Id == convId);
            _activeConversation = null;
            ClearMessageBubbles();
            ShowEmptyState(true);
            RenderConversationList(txtSearchConversations.Text.Trim());
            ShowToast("Đã xóa cuộc trò chuyện khỏi danh sách", ToastType.Info);
        }
    }

    // 4. Chặn người dùng
    private void ToggleBlockUser()
    {
        if (_activeConversation == null || _activeConversation.Type != ConversationType.Direct) return;

        var other = _activeConversation.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser?.Id);
        if (other == null) return;

        bool isBlocked = _session.IsUserBlocked(other.UserId);

        if (isBlocked)
        {
            // Unblock
            _session.UnblockUser(other.UserId);
            UpdateBlockStateUi();
            ShowToast($"Đã bỏ chặn {other.Username}", ToastType.Success);
        }
        else
        {
            // Block confirmation
            var ask = MessageBox.Show(
                $"Bạn có chắc chắn muốn chặn người dùng '{other.Username}' không?\r\n\r\n" +
                "Sau khi chặn, bạn sẽ không nhận tin nhắn và cuộc gọi từ người này.",
                "Xác Nhận Chặn Người Dùng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ask == DialogResult.Yes)
            {
                _session.BlockUser(other.UserId);
                UpdateBlockStateUi();
                ShowToast($"Đã chặn {other.Username}", ToastType.Warning);
            }
        }
    }

    private void btnBlockedUnblock_Click(object? sender, EventArgs e)
    {
        ToggleBlockUser();
    }

    private void UpdateBlockStateUi()
    {
        if (_activeConversation == null || _activeConversation.Type != ConversationType.Direct)
        {
            pnlBlockedBanner.Visible = false;
            pnlBlockedBanner.Height = 0;
            pnlInputBar.Enabled = true;
            return;
        }

        var other = _activeConversation.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser?.Id);
        bool isBlocked = other != null && _session.IsUserBlocked(other.UserId);

        if (isBlocked)
        {
            pnlBlockedBanner.Height = 36;
            pnlBlockedBanner.Visible = true;
            lblBlockedNotice.Text = $"Bạn đã chặn {other?.Username}. Bạn sẽ không nhận tin nhắn từ người này.";
            pnlInputBar.Enabled = false;
            txtMessageInput.PlaceholderText = "Tài khoản này đang bị chặn. Nhấn 'Bỏ chặn' để tiếp tục nhắn tin.";
        }
        else
        {
            pnlBlockedBanner.Visible = false;
            pnlBlockedBanner.Height = 0;
            pnlInputBar.Enabled = true;
            txtMessageInput.PlaceholderText = "Nhập tin nhắn...";
            UpdateSendButtonState();
        }
    }

    // 5. Làm mới cuộc trò chuyện (CRITICAL REQUIREMENT)
    // Tải lại nội dung phía client hiện tại, hoàn toàn không ảnh hưởng đến người B
    public async Task RefreshActiveConversationAsync()
    {
        if (_activeConversation == null) return;

        try
        {
            ShowToast("Đang tải lại cuộc trò chuyện...", ToastType.Info);

            // Fetch fresh messages from server DB for this conversation ONLY
            await LoadMessagesAsync(_activeConversation.Id);

            // Update conversation item in sidebar
            var conv = _conversations.FirstOrDefault(c => c.Id == _activeConversation.Id);
            if (conv != null)
            {
                conv.UnreadCount = 0;
                RenderConversationList(txtSearchConversations.Text.Trim());
            }

            ShowToast("Đã làm mới cuộc trò chuyện", ToastType.Success);
        }
        catch (Exception ex)
        {
            ShowToast($"Lỗi làm mới: {ex.Message}", ToastType.Error);
        }
    }

    // Toast Notification Banner
    public enum ToastType { Success, Info, Warning, Error }

    public void ShowToast(string message, ToastType type = ToastType.Success)
    {
        lblToastMessage.Text = message;
        pnlToast.BackColor = type switch
        {
            ToastType.Success => Color.FromArgb(22, 163, 74),
            ToastType.Warning => Color.FromArgb(217, 119, 6),
            ToastType.Error   => Color.FromArgb(220, 38, 38),
            _                 => Color.FromArgb(15, 23, 42)
        };

        pnlToast.Location = new Point((pnlChatArea.Width - pnlToast.Width) / 2, 75);
        pnlToast.Visible = true;
        pnlToast.BringToFront();

        _toastTimer?.Stop();
        _toastTimer = new System.Windows.Forms.Timer { Interval = 2500 };
        _toastTimer.Tick += (s, e) =>
        {
            _toastTimer?.Stop();
            pnlToast.Visible = false;
        };
        _toastTimer.Start();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Navigation Strip Actions
    // ─────────────────────────────────────────────────────────────────────────
    private async void btnNavCalls_Click(object? sender, EventArgs e)
    {
        ShowCallHistoryView();
        await _callHistoryControl.LoadHistoryAsync();
    }

    private void btnNavChat_Click(object? sender, EventArgs e)
    {
        ShowChatView();
    }

    private void ShowCallHistoryView()
    {
        pnlCallHistory.Visible = true;
        pnlCallHistory.BringToFront();
        pnlChatArea.Visible = false;

        Color inactiveFg = _session.IsDarkMode ? IconHelper.IconDefault : IconHelper.IconLight;
        btnNavCalls.BackColor = IconHelper.BtnBgActive;
        btnNavCalls.ForeColor = Color.White;
        IconHelper.SetButtonIcon(btnNavCalls, IconHelper.Phone, Color.White, 24, 18F, clearText: false);

        btnNavChat.BackColor = Color.Transparent;
        btnNavChat.ForeColor = inactiveFg;
        IconHelper.SetButtonIcon(btnNavChat, IconHelper.Chat, inactiveFg, 24, 18F, clearText: false);
    }

    private void ShowChatView()
    {
        pnlCallHistory.Visible = false;
        pnlChatArea.Visible = true;
        pnlChatArea.BringToFront();

        Color inactiveFg = _session.IsDarkMode ? IconHelper.IconDefault : IconHelper.IconLight;
        btnNavChat.BackColor = IconHelper.BtnBgActive;
        btnNavChat.ForeColor = Color.White;
        IconHelper.SetButtonIcon(btnNavChat, IconHelper.Chat, Color.White, 24, 18F, clearText: false);

        btnNavCalls.BackColor = Color.Transparent;
        btnNavCalls.ForeColor = inactiveFg;
        IconHelper.SetButtonIcon(btnNavCalls, IconHelper.Phone, inactiveFg, 24, 18F, clearText: false);
    }

    private async void btnNavFiles_Click(object? sender, EventArgs e)
    {
        if (_activeConversation != null)
        {
            await ToggleSharedContentAsync();
        }
        else
        {
            MessageBox.Show("Vui lòng chọn một cuộc trò chuyện để xem nội dung đã chia sẻ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnNavSettings_Click(object? sender, EventArgs e)
    {
        ContextMenuStrip menu = new();
        var itemTheme = menu.Items.Add(_session.IsDarkMode ? "Chuyển sang giao diện sáng" : "Chuyển sang giao diện tối", null, (_, _) =>
        {
            _session.SetDarkMode(!_session.IsDarkMode);
            ApplyTheme();
        });
        IconHelper.SetMenuItemIcon((ToolStripMenuItem)itemTheme, IconHelper.Palette, IconHelper.BtnBgActive);
        menu.Items.Add(new ToolStripSeparator());
        var itemProfile = menu.Items.Add("Chỉnh sửa hồ sơ...", null, async (_, _) => await EditMyProfileAsync());
        IconHelper.SetMenuItemIcon((ToolStripMenuItem)itemProfile, IconHelper.UserInfo, IconHelper.BtnBgActive);
        var itemServer = menu.Items.Add("Cấu hình máy chủ...", null, (_, _) =>
        {
            using var configForm = new ServerConfigForm(_session, _apiClient);
            configForm.ShowDialog(this);
        });
        IconHelper.SetMenuItemIcon((ToolStripMenuItem)itemServer, IconHelper.Settings, IconHelper.BtnBgActive);
        menu.Show(btnNavSettings, new Point(btnNavSettings.Width, 0));
    }

    private void ApplyTheme()
    {
        bool dark = _session.IsDarkMode;
        Color text = dark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);
        Color muted = dark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
        Color page = dark ? Color.FromArgb(11, 18, 32) : Color.FromArgb(248, 250, 252);
        Color surface = dark ? Color.FromArgb(15, 23, 42) : Color.White;
        Color raised = dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);

        BackColor = page;
        pnlTitleBar.BackColor = dark ? Color.FromArgb(8, 13, 25) : Color.FromArgb(248, 250, 252);
        lblAppTitle.ForeColor = text;
        pnlNavStrip.BackColor = dark ? Color.FromArgb(11, 18, 32) : Color.FromArgb(226, 232, 240);
        pnlSidebar.BackColor = dark ? Color.FromArgb(17, 28, 51) : Color.FromArgb(241, 245, 249);
        pnlSidebarBorder.BackColor = dark ? Color.FromArgb(30, 43, 69) : Color.FromArgb(203, 213, 225);
        flpConversations.BackColor = pnlSidebar.BackColor;
        pnlSearch.BackColor = pnlSidebar.BackColor;
        txtSearchConversations.BackColor = dark ? Color.FromArgb(30, 43, 69) : Color.White;
        txtSearchConversations.ForeColor = text;
        pnlUserHeader.BackColor = dark ? Color.FromArgb(14, 23, 42) : Color.FromArgb(248, 250, 252);
        lblMyUsername.ForeColor = text;
        lblMyStatus.ForeColor = muted;
        btnOpenFriends.BackColor = raised;
        btnNewGroup.BackColor = raised;
        btnLogout.BackColor = raised;
        btnOpenFriends.ForeColor = text;
        btnNewGroup.ForeColor = text;
        btnLogout.ForeColor = dark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);
        pnlMainContainer.BackColor = page;
        pnlChatArea.BackColor = page;
        pnlChatHeader.BackColor = surface;
        pnlHeaderButtons.BackColor = surface;
        lblChatTitle.ForeColor = text;
        lblChatStatus.ForeColor = _activeConversation is null ? muted : lblChatStatus.ForeColor;
        lblTypingIndicator.BackColor = page;
        lblTypingIndicator.ForeColor = muted;
        flpMessages.BackColor = page;
        pnlInputBar.BackColor = surface;
        pnlInputContainer.BackColor = raised;
        txtMessageInput.BackColor = raised;
        txtMessageInput.ForeColor = text;
        pnlMessageSearch.BackColor = surface;
        pnlSearchInputContainer.BackColor = raised;
        txtSearchMessageKeyword.BackColor = raised;
        txtSearchMessageKeyword.ForeColor = text;
        lblSearchIconHeader.ForeColor = muted;
        lblSearchMatchCount.ForeColor = muted;
        btnSearchPrev.BackColor = raised;
        btnSearchNext.BackColor = raised;
        btnSearchPrev.ForeColor = text;
        btnSearchNext.ForeColor = text;
        btnSearchClear.ForeColor = muted;
        btnCloseSearch.ForeColor = muted;
        pnlEmptyState.BackColor = page;
        lblEmptyState.ForeColor = text;
        lblEmptyStateHint.ForeColor = muted;
        pnlBlockedBanner.BackColor = dark ? Color.FromArgb(69, 26, 26) : Color.FromArgb(254, 242, 242);
        lblBlockedNotice.ForeColor = dark ? Color.FromArgb(254, 202, 202) : Color.FromArgb(153, 27, 27);
        pnlMoreMenu.BackColor = surface;
        foreach (Panel item in new[] { itemUserInfo, itemSearch, itemDelete, itemBlock, itemRefresh })
            item.BackColor = surface;
        foreach (Label label in new[] { lblUserInfoTitle, lblSearchTitle, lblDeleteTitle, lblBlockTitle, lblRefreshTitle })
            label.ForeColor = text;
        foreach (Label label in new[] { lblUserInfoSub, lblSearchSub, lblDeleteSub, lblBlockSub, lblRefreshSub })
            label.ForeColor = muted;
        pnlCallHistory.BackColor = page;

        Color iconSecondary = dark ? IconHelper.IconDefault : IconHelper.IconLight;
        ConfigureNavButton(btnNavChat, IconHelper.Chat, "Chat", iconSecondary);
        ConfigureNavButton(btnNavCalls, IconHelper.Phone, "Gọi", iconSecondary);
        ConfigureNavButton(btnNavFiles, IconHelper.Files, "Tệp", iconSecondary);
        ConfigureNavButton(btnNavSettings, IconHelper.Settings, "Cài đặt", iconSecondary);
        bool callsActive = pnlCallHistory.Visible;
        btnNavChat.BackColor = callsActive ? Color.Transparent : IconHelper.BtnBgActive;
        btnNavChat.ForeColor = callsActive ? iconSecondary : Color.White;
        IconHelper.SetButtonIcon(btnNavChat, IconHelper.Chat, callsActive ? iconSecondary : Color.White, 24, 18F, clearText: false);
        btnNavCalls.BackColor = callsActive ? IconHelper.BtnBgActive : Color.Transparent;
        btnNavCalls.ForeColor = callsActive ? Color.White : iconSecondary;
        IconHelper.SetButtonIcon(btnNavCalls, IconHelper.Phone, callsActive ? Color.White : iconSecondary, 24, 18F, clearText: false);
        btnNavFiles.BackColor = Color.Transparent;
        btnNavSettings.BackColor = Color.Transparent;
        Color hoverSurface = dark ? IconHelper.BtnBgHover : IconHelper.BtnBgLightHover;
        foreach (Button button in new[] { btnNavCalls, btnNavFiles, btnNavSettings })
            button.FlatAppearance.MouseOverBackColor = hoverSurface;

        bool searchActive = pnlMessageSearch.Visible;
        bool sharedActive = _ucSharedContent.Visible;

        IconHelper.ApplyToolbarButtonStyle(btnVoiceCall, IconHelper.Phone, dark);
        IconHelper.ApplyToolbarButtonStyle(btnVideoCall, IconHelper.Video, dark);
        IconHelper.ApplyToolbarButtonStyle(btnGroupInfo, IconHelper.People, dark);
        IconHelper.ApplyToolbarButtonStyle(btnSearch, IconHelper.Search, dark, isActive: searchActive);
        IconHelper.ApplyToolbarButtonStyle(btnAttachFileHeader, IconHelper.Attach, dark, isActive: sharedActive);
        IconHelper.ApplyToolbarButtonStyle(btnMoreMenu, IconHelper.MoreHorizontal, dark);

        IconHelper.ApplyToolbarButtonStyle(btnAttachFile, IconHelper.Attach, dark);
        IconHelper.ApplyToolbarButtonStyle(btnEmoji, IconHelper.Emoji, dark);
        if (_isRecordingVoice)
        {
            IconHelper.SetButtonIcon(btnRecordVoice, IconHelper.StopRecord, Color.White, 22, 16F);
            btnRecordVoice.BackColor = IconHelper.StatusDanger;
        }
        else
        {
            IconHelper.ApplyToolbarButtonStyle(btnRecordVoice, IconHelper.Mic, dark);
        }

        UpdateSendButtonState();

        IconHelper.ApplyToolbarButtonStyle(btnOpenFriends, IconHelper.Contact, dark);
        IconHelper.ApplyToolbarButtonStyle(btnNewGroup, IconHelper.AddGroup, dark);
        IconHelper.ApplyToolbarButtonStyle(btnLogout, IconHelper.Logout, dark);
        btnLogout.ForeColor = dark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);
        btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);

        // More menu icons
        IconHelper.SetLabelIcon(lblUserInfoIcon, IconHelper.Contact, text, 14F);
        IconHelper.SetLabelIcon(lblSearchIcon, IconHelper.Search, text, 14F);
        IconHelper.SetLabelIcon(lblDeleteIcon, IconHelper.Delete, IconHelper.StatusDanger, 14F);
        IconHelper.SetLabelIcon(lblRefreshIcon, IconHelper.Refresh, text, 14F);

        // Empty state & title logo
        IconHelper.SetLabelIcon(lblEmptyState, IconHelper.Chat, dark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225), 48F);
        IconHelper.SetLabelIcon(lblTitleLogo, IconHelper.Chat, Color.FromArgb(96, 165, 250), 14F);

        // Window buttons
        IconHelper.SetButtonIcon(btnMinimize, IconHelper.Minimize, muted, 18, 11F);
        IconHelper.SetButtonIcon(btnMaximize, WindowState == FormWindowState.Maximized ? IconHelper.Restore : IconHelper.Maximize, muted, 18, 11F);
        IconHelper.SetButtonIcon(btnCloseWindow, IconHelper.Close, muted, 18, 11F);

        // Search message panel icons
        IconHelper.SetLabelIcon(lblSearchIconHeader, IconHelper.Search, muted, 12F);
        IconHelper.SetButtonIcon(btnSearchClear, IconHelper.Close, muted, 16, 11F);
        IconHelper.SetButtonIcon(btnSearchPrev, IconHelper.ArrowUp, text, 20, 13F);
        IconHelper.SetButtonIcon(btnSearchNext, IconHelper.ArrowDown, text, 20, 13F);
        IconHelper.SetButtonIcon(btnCloseSearch, IconHelper.Close, muted, 20, 13F);

        // Tray Menu
        IconHelper.SetMenuItemIcon(menuTrayOpen, IconHelper.Chat, IconHelper.BtnBgActive);
        IconHelper.SetMenuItemIcon(menuTrayLogout, IconHelper.Logout, IconHelper.StatusDanger);
        IconHelper.SetMenuItemIcon(menuTrayExit, IconHelper.Close, IconHelper.StatusDanger);

        foreach (ConversationListItemControl item in _conversationItems.Values)
            item.ApplyTheme(dark);
        foreach (MessageBubbleControl bubble in flpMessages.Controls.OfType<MessageBubbleControl>())
            bubble.ApplyTheme(dark);
    }

    private static void ConfigureNavButton(Button button, string glyph, string label, Color color)
    {
        SetIcon(button, glyph, color);
        button.Text = label;
        button.ForeColor = color;
        button.TextImageRelation = TextImageRelation.ImageAboveText;
        button.ImageAlign = ContentAlignment.MiddleCenter;
        button.TextAlign = ContentAlignment.BottomCenter;
        button.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        button.Padding = new Padding(0, 3, 0, 2);
        button.UseVisualStyleBackColor = false;
    }

    private static void SetIcon(Button button, string glyph, Color color)
    {
        Bitmap icon = new(28, 28);
        using (Graphics g = Graphics.FromImage(icon))
        using (Font font = new("Segoe MDL2 Assets", 20F, FontStyle.Regular, GraphicsUnit.Pixel))
            TextRenderer.DrawText(g, glyph, font, new Rectangle(0, 0, 28, 28), color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        Image? previous = button.Image;
        button.Image = icon;
        previous?.Dispose();
        if (button.TextImageRelation != TextImageRelation.ImageAboveText)
        {
            button.Text = string.Empty;
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Conversations Management
    // ─────────────────────────────────────────────────────────────────────────
    public async Task LoadConversationsAsync()
    {
        var res = await _apiClient.GetConversationsAsync();
        if (res?.Success == true && res.Data != null)
        {
            _conversations = res.Data;
            RenderConversationList(txtSearchConversations.Text.Trim());
        }
    }

    private void ShowEmptyState(bool show)
    {
        pnlEmptyState.Visible = show;
        flpMessages.Visible = !show;
        pnlInputBar.Enabled = !show;
        btnVoiceCall.Enabled = !show;
        btnVideoCall.Enabled = !show;
        btnSearch.Enabled = !show;
        btnMoreMenu.Enabled = !show;
        btnAttachFileHeader.Enabled = !show;

        if (show)
        {
            if (_ucSharedContent.Visible)
            {
                _ = ToggleSharedContentAsync(false);
            }
            lblChatTitle.Text = "Chatting — LAN Messenger";
            lblChatStatus.Text = string.Empty;
            picChatAvatar.Image = null;
            picChatAvatar.BackColor = Color.FromArgb(199, 210, 254);
            pnlMessageSearch.Visible = false;
            pnlBlockedBanner.Visible = false;
        }
    }

    private void RenderConversationList(string? filterKeyword = null)
    {
        flpConversations.SuspendLayout();
        try
        {
            var knownIds = _conversations.Select(c => c.Id).ToHashSet();
            foreach (var staleId in _conversationItems.Keys.Where(id => !knownIds.Contains(id)).ToArray())
            {
                var staleItem = _conversationItems[staleId];
                flpConversations.Controls.Remove(staleItem);
                staleItem.Dispose();
                _conversationItems.Remove(staleId);
            }

            var visible = string.IsNullOrWhiteSpace(filterKeyword)
                ? _conversations
                : _conversations.Where(c => c.Title.Contains(filterKeyword, StringComparison.OrdinalIgnoreCase)).ToList();
            var visibleIds = visible.Select(c => c.Id).ToHashSet();
            foreach (Control item in flpConversations.Controls.Cast<Control>().ToArray())
            {
                if (item is ConversationListItemControl conversationItem &&
                    conversationItem.Conversation is { } conversation && !visibleIds.Contains(conversation.Id))
                    flpConversations.Controls.Remove(item);
            }

            Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;
            int itemWidth = flpConversations.ClientSize.Width - flpConversations.Padding.Horizontal;
            if (itemWidth < 180) itemWidth = 254;

            for (int i = 0; i < visible.Count; i++)
            {
                var conv = visible[i];
                if (!_conversationItems.TryGetValue(conv.Id, out var itemCtrl))
                {
                    itemCtrl = new ConversationListItemControl { Margin = new Padding(0, 3, 0, 3) };
                    itemCtrl.ItemClicked += SelectConversation;
                    itemCtrl.AvatarClicked += PreviewConversationAvatar;
                    _conversationItems.Add(conv.Id, itemCtrl);
                }

                itemCtrl.Width = itemWidth;
                itemCtrl.ApplyTheme(_session.IsDarkMode);
                itemCtrl.SetData(conv, _activeConversation?.Id == conv.Id, myId, _session.ServerUrl);
                if (itemCtrl.Parent != flpConversations)
                    flpConversations.Controls.Add(itemCtrl);
                if (flpConversations.Controls.GetChildIndex(itemCtrl) != i)
                    flpConversations.Controls.SetChildIndex(itemCtrl, i);
            }
        }
        finally
        {
            flpConversations.ResumeLayout();
        }
    }

    private async void SelectConversation(ConversationDto conv)
    {
        if (pnlCallHistory.Visible)
        {
            ShowChatView();
        }
        if (pnlMessageSearch.Visible)
        {
            btnCloseSearch_Click(this, EventArgs.Empty);
        }
        VoicePlayerManager.Instance.Stop();
        _activeConversation = conv;
        lblChatTitle.Text = conv.Title;
        picChatAvatar.Image = GenerateAvatarBitmap(conv.Title, conv.Type == ConversationType.Group);
        _ = LoadAvatarIntoAsync(picChatAvatar, conv.AvatarUrl);
        picChatAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picChatAvatar.Width, picChatAvatar.Height, picChatAvatar.Width, picChatAvatar.Height));

        btnGroupInfo.Visible = conv.Type == ConversationType.Group;

        // Update online status text
        bool isAiConv = _aiConversationId != Guid.Empty && conv.Id == _aiConversationId;
        _isAiChatActive = isAiConv;
        picAiRobotBubble.Invalidate();

        if (isAiConv)
        {
            lblChatTitle.Text = "Chatting AI";
            picChatAvatar.Image = AiAvatarHelper.GetHologramStarAvatar(picChatAvatar.Width);
            lblChatStatus.Text = "Trợ lý AI • Luôn trực tuyến";
            lblChatStatus.ForeColor = Color.FromArgb(167, 139, 250);
            // Disable call buttons for AI conversation
            btnVoiceCall.Enabled = false;
            btnVideoCall.Enabled = false;
        }
        else if (conv.Type == ConversationType.Direct)
        {
            btnVoiceCall.Enabled = true;
            btnVideoCall.Enabled = true;
            var other = conv.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser?.Id);
            bool online = other?.IsOnline == true;
            lblChatStatus.Text = online ? "● Đang trực tuyến" : "○ Ngoại tuyến";
            lblChatStatus.ForeColor = online ? Color.FromArgb(22, 163, 74) : Color.FromArgb(100, 116, 139);
        }
        else
        {
            btnVoiceCall.Enabled = true;
            btnVideoCall.Enabled = true;
            lblChatStatus.Text = $"{conv.Members.Count} thành viên";
            lblChatStatus.ForeColor = Color.FromArgb(100, 116, 139);
        }

        // Check if user is blocked (skip for AI)
        if (!isAiConv)
            UpdateBlockStateUi();
        else
        {
            pnlBlockedBanner.Visible = false;
            pnlBlockedBanner.Height = 0;
            pnlInputBar.Enabled = true;
            txtMessageInput.PlaceholderText = "Nhập câu hỏi cho Chatting AI...";
        }

        // Show chat area, hide empty state
        ShowEmptyState(false);

        // Highlight selected
        RenderConversationList(txtSearchConversations.Text.Trim());

        // These independent requests can run together; loading messages no longer
        // waits for the read acknowledgement to make a network round trip.
        Task markReadTask = _signalR.MarkConversationReadAsync(conv.Id);
        conv.UnreadCount = 0;

        // Ensure SignalR group is joined
        _ = _signalR.JoinConversationAsync(conv.Id);

        // Load Messages
        await LoadMessagesAsync(conv.Id);
        await markReadTask;
        if (_activeConversation?.Id != conv.Id) return;

        // If shared content panel is open, reload for this conversation
        if (_ucSharedContent.Visible)
        {
            await _ucSharedContent.LoadContentAsync(conv);
        }
    }

    private async Task LoadMessagesAsync(Guid convId)
    {
        // Keep the panel responsive while the server request is in flight.
        ClearMessageBubbles();
        int loadVersion = _messageLoadVersion;
        var res = await _apiClient.GetMessagesAsync(convId, 1, 50);
        if (loadVersion != _messageLoadVersion || _activeConversation?.Id != convId) return;

        flpMessages.SuspendLayout();
        try
        {
            if (res?.Success == true && res.Data != null)
            {
                Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;
                bool isGroup = _activeConversation.Type == ConversationType.Group;

                // PagedResult returns latest messages descending, reverse to display ascending
                var msgs = res.Data.Items.OrderBy(m => m.CreatedAt).ToList();

                foreach (var msg in msgs)
                    AppendMessageBubble(msg, myId, isGroup);
            }
        }
        finally
        {
            flpMessages.ResumeLayout();
        }

        ScrollMessagesToBottom();
        if (pnlMessageSearch.Visible && !string.IsNullOrWhiteSpace(txtSearchMessageKeyword.Text))
            txtSearchMessageKeyword_TextChanged(txtSearchMessageKeyword, EventArgs.Empty);
    }

    private void ClearMessageBubbles()
    {
        ++_messageLoadVersion;
        _smoothScrollTimer?.Stop();
        _smoothScrollTimer?.Dispose();
        _smoothScrollTimer = null;
        _searchMatches.Clear();
        _searchMatchIndex = -1;
        var oldBubbles = flpMessages.Controls.Cast<Control>().ToArray();
        flpMessages.Controls.Clear();
        foreach (var bubble in oldBubbles)
            bubble.Dispose();
    }

    private void AppendMessageBubble(MessageDto msg, Guid myId, bool isGroup)
    {
        // Avoid adding duplicate message bubble
        foreach (Control c in flpMessages.Controls)
        {
            if (c is MessageBubbleControl mb && mb.MessageData?.Id == msg.Id)
                return;
        }

        // Messages retain the account username on the server. Resolve the local
        // conversation nickname immediately before rendering so prior messages
        // update as well when a nickname changes.
        if (_activeConversation?.Id == msg.ConversationId)
        {
            ConversationMemberDto? sender = _activeConversation.Members.FirstOrDefault(m => m.UserId == msg.SenderId);
            if (sender is not null && !string.IsNullOrWhiteSpace(sender.Nickname))
                msg.SenderUsername = sender.Nickname;
        }

        var bubble = new MessageBubbleControl();
        bubble.Width = flpMessages.ClientSize.Width - 25;
        bubble.SetData(msg, myId, isGroup, _session.ServerUrl, _apiClient);
        bubble.ApplyTheme(_session.IsDarkMode);

        bubble.OnEditRequested += HandleEditMessage;
        bubble.OnRevokeRequested += HandleRevokeMessage;
        bubble.OnHideForMeRequested += HandleHideMessageForMe;
        bubble.OnReactionRequested += HandleMessageReactionRequested;
        bubble.OnPinRequested += HandlePinMessageRequested;
        bubble.OnForwardRequested += HandleForwardMessageRequested;
        bubble.OnDownloadRequested += HandleDownloadAttachment;

        flpMessages.Controls.Add(bubble);
    }

    private void ScrollMessagesToBottom()
    {
        if (flpMessages.Controls.Count > 0)
        {
            flpMessages.ScrollControlIntoView(flpMessages.Controls[flpMessages.Controls.Count - 1]);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SignalR Incoming Event Handlers
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleReceiveMessage(MessageDto msg)
    {
        if (InvokeRequired) { Invoke(() => HandleReceiveMessage(msg)); return; }

        Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;

        // Ignore messages from blocked users
        if (_session.IsUserBlocked(msg.SenderId))
            return;

        // Play chime and show balloon if from other user
        if (msg.SenderId != myId)
        {
            SoundNotificationService.Instance.PlayMessageSound();

            if (WindowState == FormWindowState.Minimized || !ContainsFocus)
            {
                notifyIconTray.BalloonTipTitle = $"{msg.SenderUsername}";
                notifyIconTray.BalloonTipText = msg.Content ?? "[Tệp đính kèm]";
                notifyIconTray.ShowBalloonTip(3000);
            }
        }

        // If in active conversation
        if (_activeConversation != null && msg.ConversationId == _activeConversation.Id)
        {
            bool isGroup = _activeConversation.Type == ConversationType.Group;

            AppendMessageBubble(msg, myId, isGroup);
            ScrollMessagesToBottom();

            // Mark read if it's incoming
            if (msg.SenderId != myId)
            {
                _ = _signalR.MarkConversationReadAsync(_activeConversation.Id);
            }

            // Real-time update to shared content panel if open
            if (_ucSharedContent.Visible)
            {
                _ucSharedContent.OnNewMessage(msg);
            }
        }

        // Update list sidebar
        var conv = _conversations.FirstOrDefault(c => c.Id == msg.ConversationId);
        if (conv != null)
        {
            conv.LastMessage = msg;
            if (_activeConversation?.Id != msg.ConversationId && msg.SenderId != myId)
            {
                conv.UnreadCount++;
            }
            RenderConversationList(txtSearchConversations.Text.Trim());
        }
        else
        {
            _ = LoadConversationsAsync();
        }
    }

    private void HandleMessageEdited(MessageEditedDto update)
    {
        RunMainUiThread(() =>
        {
            if (_activeConversation?.Id == update.ConversationId)
            {
                MessageBubbleControl? bubble = flpMessages.Controls.OfType<MessageBubbleControl>()
                    .FirstOrDefault(b => b.MessageData?.Id == update.MessageId);
                if (bubble?.MessageData is MessageDto message)
                {
                    message.Content = update.NewContent;
                    message.IsEdited = true;
                    message.Status = MessageStatus.Edited;
                    message.UpdatedAt = update.EditedAt;
                    bubble.SetData(message, _session.CurrentUser?.Id ?? Guid.Empty,
                        _activeConversation.Type == ConversationType.Group, _session.ServerUrl, _apiClient);
                }
            }
            _ = LoadConversationsAsync();
        });
    }

    private void HandleMessageRevoked(MessageStateChangedDto update)
    {
        RunMainUiThread(() =>
        {
            ApplyMessageStateUpdate(update, revoked: true);
            _ = LoadConversationsAsync();
        });
    }

    private void HandleMessageHiddenForMe(MessageStateChangedDto update)
    {
        RunMainUiThread(() =>
        {
            if (_activeConversation?.Id == update.ConversationId)
            {
                MessageBubbleControl? bubble = flpMessages.Controls.OfType<MessageBubbleControl>()
                    .FirstOrDefault(b => b.MessageData?.Id == update.MessageId);
                if (bubble is not null)
                {
                    flpMessages.Controls.Remove(bubble);
                    bubble.Dispose();
                }
            }
            _ = LoadConversationsAsync();
        });
    }

    private async void HandleMessageReactionRequested(MessageDto message, string emoji)
    {
        var result = await _apiClient.ToggleMessageReactionAsync(message.Id, emoji);
        if (result?.Success == true && result.Data is not null)
        {
            HandleMessageReactionsUpdated(result.Data);
        }
        else
        {
            ShowToast(result?.Message ?? "Không thể thả cảm xúc.", ToastType.Error);
        }
    }

    private void HandleMessageReactionsUpdated(MessageReactionsChangedDto update)
    {
        RunMainUiThread(() =>
        {
            if (_activeConversation?.Id != update.ConversationId) return;

            MessageBubbleControl? bubble = flpMessages.Controls.OfType<MessageBubbleControl>()
                .FirstOrDefault(b => b.MessageData?.Id == update.MessageId);
            if (bubble?.MessageData is not MessageDto message) return;

            Guid myId = _session.CurrentUser?.Id ?? Guid.Empty;
            message.Reactions = update.Reactions.Select(reaction => new MessageReactionSummaryDto
            {
                Emoji = reaction.Emoji,
                Count = reaction.Count,
                ReactedByMe = reaction.ReactedUserIds.Contains(myId),
                ReactedUserIds = reaction.ReactedUserIds
            }).ToList();
            bubble.SetData(message, _session.CurrentUser?.Id ?? Guid.Empty,
                _activeConversation.Type == ConversationType.Group, _session.ServerUrl, _apiClient);
        });
    }

    private async void HandlePinMessageRequested(MessageDto message)
    {
        bool pin = !message.IsPinned;
        var result = await _apiClient.SetMessagePinnedAsync(message.Id, pin);
        if (result?.Success == true && result.Data is not null)
        {
            HandleMessagePinUpdated(result.Data);
            ShowToast(pin ? "Đã ghim tin nhắn." : "Đã bỏ ghim tin nhắn.", ToastType.Success);
        }
        else
        {
            ShowToast(result?.Message ?? "Không thể cập nhật ghim tin nhắn.", ToastType.Error);
        }
    }

    private void HandleMessagePinUpdated(MessagePinChangedDto update)
    {
        RunMainUiThread(() =>
        {
            if (_activeConversation?.Id != update.ConversationId) return;
            MessageBubbleControl? bubble = flpMessages.Controls.OfType<MessageBubbleControl>()
                .FirstOrDefault(item => item.MessageData?.Id == update.MessageId);
            if (bubble?.MessageData is not MessageDto message) return;

            message.IsPinned = update.IsPinned;
            message.PinnedByUserId = update.PinnedByUserId;
            message.PinnedAt = update.PinnedAt;
            bubble.SetData(message, _session.CurrentUser?.Id ?? Guid.Empty,
                _activeConversation.Type == ConversationType.Group, _session.ServerUrl, _apiClient);
        });
    }

    private void HandleForwardMessageRequested(MessageDto message)
    {
        List<ConversationDto> targets = _conversations
            .Where(conversation => conversation.Id != message.ConversationId)
            .ToList();
        if (targets.Count == 0)
        {
            ShowToast("Không có cuộc trò chuyện khác để chuyển tiếp.", ToastType.Warning);
            return;
        }

        using ForwardMessageForm dialog = new(targets, _session.IsDarkMode);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _ = ForwardMessageToConversationsAsync(message, dialog.SelectedConversationIds);
    }

    private async Task ForwardMessageToConversationsAsync(MessageDto message, List<Guid> targetConversationIds)
    {
        var result = await _apiClient.ForwardMessageAsync(message.Id, targetConversationIds);
        if (result?.Success == true && result.Data is not null)
        {
            ShowToast(result.Message ?? $"Đã chuyển tiếp đến {result.Data.Count} cuộc trò chuyện.", ToastType.Success);
            await LoadConversationsAsync();
        }
        else
        {
            ShowToast(result?.Message ?? "Không thể chuyển tiếp tin nhắn.", ToastType.Error);
        }
    }

    private void ApplyMessageStateUpdate(MessageStateChangedDto update, bool revoked)
    {
        if (_activeConversation?.Id != update.ConversationId) return;
        MessageBubbleControl? bubble = flpMessages.Controls.OfType<MessageBubbleControl>()
            .FirstOrDefault(b => b.MessageData?.Id == update.MessageId);
        if (bubble?.MessageData is not MessageDto message) return;

        message.IsRevoked = revoked;
        message.IsDeleted = !revoked;
        message.Content = null;
        message.Status = revoked ? MessageStatus.Revoked : MessageStatus.Deleted;
        message.UpdatedAt = update.ChangedAt;
        if (revoked)
        {
            message.IsPinned = false;
            message.PinnedByUserId = null;
            message.PinnedAt = null;
        }
        bubble.SetData(message, _session.CurrentUser?.Id ?? Guid.Empty,
            _activeConversation.Type == ConversationType.Group, _session.ServerUrl, _apiClient);
    }

    private void HandleGroupUpdated(string raw)
    {
        RunMainUiThread(() => _ = RefreshAfterGroupUpdateAsync());
    }

    private void HandleMembersAdded(GroupMembersAddedDto update)
    {
        RunMainUiThread(() => _ = RefreshGroupConversationAsync(update.ConversationId));
    }

    private void HandleMemberRemoved(GroupMemberRemovedDto update)
    {
        RunMainUiThread(() =>
        {
            if (update.RemovedUserId == _session.CurrentUser?.Id)
                _ = HandleYouWereRemovedFromGroupAsync(update);
            else
                _ = RefreshGroupConversationAsync(update.ConversationId);
        });
    }

    private void HandleMemberLeft(GroupMemberChangedDto update)
    {
        RunMainUiThread(() => _ = RefreshGroupConversationAsync(update.ConversationId));
    }

    private void HandleYouWereRemovedFromGroup(GroupMemberRemovedDto update)
    {
        RunMainUiThread(() => _ = HandleYouWereRemovedFromGroupAsync(update));
    }

    private async Task HandleYouWereRemovedFromGroupAsync(GroupMemberRemovedDto update)
    {
        if (update.RemovedUserId != _session.CurrentUser?.Id) return;

        bool wasActive = _activeConversation?.Id == update.ConversationId;
        if (wasActive)
        {
            _activeConversation = null;
            ClearMessageBubbles();
            ShowEmptyState(true);
        }

        await LoadConversationsAsync();
        if (wasActive) RenderConversationList(txtSearchConversations.Text.Trim());

        MessageBox.Show(
            $"Bạn đã bị xóa khỏi nhóm '{update.GroupName}'. Bạn không còn xem được tin nhắn trong nhóm này.",
            "Bạn đã bị xóa khỏi nhóm",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async Task RefreshGroupConversationAsync(Guid conversationId)
    {
        bool wasActive = _activeConversation?.Id == conversationId;
        await LoadConversationsAsync();
        if (!wasActive) return;

        ConversationDto? refreshed = _conversations.FirstOrDefault(c => c.Id == conversationId);
        if (refreshed is not null) SelectConversation(refreshed);
    }

    private async Task RefreshAfterGroupUpdateAsync()
    {
        Guid? activeId = _activeConversation?.Id;
        await LoadConversationsAsync();
        if (activeId.HasValue)
        {
            ConversationDto? refreshed = _conversations.FirstOrDefault(c => c.Id == activeId.Value);
            if (refreshed is not null) SelectConversation(refreshed);
        }
    }

    private void RunMainUiThread(Action action)
    {
        if (IsDisposed || Disposing) return;
        if (!InvokeRequired)
        {
            action();
            return;
        }

        if (!IsHandleCreated) return;
        try
        {
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed && !Disposing) action();
            }));
        }
        catch (InvalidOperationException) { }
    }

    private void HandleUserTyping(Guid convId, Guid userId, bool isTyping)
    {
        if (InvokeRequired) { Invoke(() => HandleUserTyping(convId, userId, isTyping)); return; }

        if (_activeConversation?.Id == convId && userId != _session.CurrentUser?.Id)
        {
            if (isTyping && !_session.IsUserBlocked(userId))
            {
                var member = _activeConversation.Members.FirstOrDefault(m => m.UserId == userId);
                lblTypingIndicator.Text = $"{member?.Username ?? "Ai đó"} đang soạn tin nhắn...";
                lblTypingIndicator.Visible = true;
            }
            else
            {
                lblTypingIndicator.Visible = false;
            }
        }
    }

    private void HandleMessagesRead(Guid convId, Guid readByUserId, DateTime readAt)
    {
        if (InvokeRequired) { Invoke(() => HandleMessagesRead(convId, readByUserId, readAt)); return; }

        if (_activeConversation?.Id == convId && readByUserId != _session.CurrentUser?.Id)
        {
            foreach (Control ctrl in flpMessages.Controls)
            {
                if (ctrl is MessageBubbleControl bubble && bubble.IsMyMessage && bubble.MessageData != null)
                {
                    bubble.MessageData.Status = MessageStatus.Seen;
                    bubble.SetData(bubble.MessageData, _session.CurrentUser!.Id, false, _session.ServerUrl);
                }
            }
        }
    }

    private void HandleIncomingCall(CallInfoDto callInfo)
    {
        if (InvokeRequired) { Invoke(() => HandleIncomingCall(callInfo)); return; }

        if (_session.IsUserBlocked(callInfo.CallerId))
        {
            _ = _signalR.RejectCallAsync(callInfo.CallId);
            return;
        }

        // Nếu người dùng đang trong cuộc gọi khác, tự động từ chối máy bận
        if (_activeCallWindow != null && !_activeCallWindow.IsDisposed)
        {
            _ = _signalR.RejectCallAsync(callInfo.CallId);
            return;
        }

        string callTypeStr = callInfo.CallType == CallType.Video ? "Video" : "Thoại";
        SoundNotificationService.Instance.StartRingtone();

        var ask = MessageBox.Show(
            $"{callInfo.CallerUsername} đang gọi {callTypeStr} cho bạn qua mạng LAN!\r\nBạn có muốn nhận cuộc gọi không?",
            "Cuộc Gọi Đến",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        SoundNotificationService.Instance.StopRingtone();

        if (ask == DialogResult.Yes)
        {
            Form callForm = callInfo.ReceiverId is null
                ? new GroupCallForm(callInfo, _signalR, _session)
                : new VideoCallForm(callInfo, isInitiator: false, _signalR, _session);
            _activeCallWindow = callForm;
            callForm.FormClosed += (s, e) => { if (_activeCallWindow == callForm) _activeCallWindow = null; };
            callForm.Show();
            _ = _signalR.AcceptCallAsync(callInfo.CallId);
        }
        else
        {
            _ = _signalR.RejectCallAsync(callInfo.CallId);
        }
    }

    private void HandleCallStarted(CallInfoDto callInfo)
    {
        if (InvokeRequired) { Invoke(() => HandleCallStarted(callInfo)); return; }

        // Đảm bảo không mở trùng lặp nhiều cửa sổ cho cùng một cuộc gọi
        if (_activeCallWindow != null && !_activeCallWindow.IsDisposed)
        {
            _activeCallWindow.BringToFront();
            return;
        }

        Form callForm = callInfo.ReceiverId is null
            ? new GroupCallForm(callInfo, _signalR, _session)
            : new VideoCallForm(callInfo, isInitiator: true, _signalR, _session);
        _activeCallWindow = callForm;
        callForm.FormClosed += (s, e) => { if (_activeCallWindow == callForm) _activeCallWindow = null; };
        callForm.Show();
    }

    private void HandleNewConversationCreated(Guid convId)
    {
        RunMainUiThread(() => _ = LoadConversationsAsync());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // User Actions: Send, Voice, Attach
    // ─────────────────────────────────────────────────────────────────────────
    private async void btnSendMessage_Click(object? sender, EventArgs e)
    {
        if (_activeConversation == null) return;
        string text = txtMessageInput.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        txtMessageInput.Clear();
        UpdateSendButtonState();
        AdjustInputBarHeight();
        _ = _signalR.SendTypingAsync(_activeConversation.Id, false);

        // If this is the AI conversation, route through the AI API
        if (_aiConversationId != Guid.Empty && _activeConversation.Id == _aiConversationId)
        {
            btnSendMessage.Enabled = false;
            ShowToast("Chatting AI đang trả lời...", ToastType.Info);
            try
            {
                var res = await _apiClient.SendAiMessageAsync(_aiConversationId, text);
                if (res?.Success == true && res.Data != null)
                {
                    HandleReceiveMessage(res.Data);
                }
                else if (res?.Success != true)
                {
                    ShowToast(res?.Message ?? "AI không phản hồi. Thử lại sau.", ToastType.Error);
                }
            }
            catch (Exception ex)
            {
                ShowToast($"Lỗi AI: {ex.Message}", ToastType.Error);
            }
            finally
            {
                btnSendMessage.Enabled = true;
            }
            return;
        }

        var dto = new SendMessageDto
        {
            ConversationId = _activeConversation.Id,
            Content = text,
            Type = MessageType.Text
        };

        await _signalR.SendMessageAsync(dto);
    }

    private void txtMessageInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.SuppressKeyPress = true;
            btnSendMessage_Click(sender, e);
        }
    }

    // Sprite-backed emoji picker. Tokens are stored in messages so no client
    // depends on a particular emoji font being installed.
    private void btnEmoji_Click(object? sender, EventArgs e)
    {
        using Form emojiForm = new()
        {
            ClientSize = new Size(432, 324),
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            BackColor = Color.White,
            TopMost = true
        };

        var btnPos = btnEmoji.PointToScreen(Point.Empty);
        int x = Math.Min(btnPos.X, Screen.FromControl(this).WorkingArea.Right - emojiForm.Width - 8);
        emojiForm.Location = new Point(Math.Max(8, x), btnPos.Y - emojiForm.Height - 4);
        var picker = new EmojiSpritePicker { Dock = DockStyle.Fill };
        picker.EmojiSelected += index =>
        {
            int sel = txtMessageInput.SelectionStart;
            string token = EmojiSprite.Token(index);
            txtMessageInput.Text = txtMessageInput.Text.Insert(sel, token);
            txtMessageInput.SelectionStart = sel + token.Length;
            txtMessageInput.Focus();
            emojiForm.Close();
        };

        emojiForm.Controls.Add(picker);
        emojiForm.Deactivate += (s, ev) => emojiForm.Close();
        emojiForm.ShowDialog(this);
    }

    private void txtMessageInput_TextChanged(object? sender, EventArgs e)
    {
        UpdateSendButtonState();
        AdjustInputBarHeight();

        if (_activeConversation == null) return;

        _ = _signalR.SendTypingAsync(_activeConversation.Id, true);

        _typingTimer?.Stop();
        _typingTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _typingTimer.Tick += (s, args) =>
        {
            _typingTimer?.Stop();
            if (_activeConversation != null)
                _ = _signalR.SendTypingAsync(_activeConversation.Id, false);
        };
        _typingTimer.Start();
    }

    private void LayoutInputBar()
    {
        if (pnlInputBar == null || txtMessageInput == null || btnSendMessage == null || pnlInputContainer == null) return;

        int padX = 14;
        int gap = 8;
        int barH = pnlInputBar.ClientSize.Height;

        // 1. Icon buttons on the left
        int btnSize = 38;
        int btnTop = Math.Max(0, (barH - btnSize) / 2);

        btnEmoji.SetBounds(padX, btnTop, btnSize, btnSize);
        btnAttachFile.SetBounds(btnEmoji.Right + gap, btnTop, btnSize, btnSize);
        btnRecordVoice.SetBounds(btnAttachFile.Right + gap, btnTop, btnSize, btnSize);

        // 2. Send button on the far right
        int sendW = 46;
        int sendH = 42;
        int sendTop = Math.Max(0, (barH - sendH) / 2);
        int sendLeft = Math.Max(0, pnlInputBar.ClientSize.Width - padX - sendW);
        btnSendMessage.SetBounds(sendLeft, sendTop, sendW, sendH);

        // 3. Input container fills between voice button and send button
        int inputLeft = btnRecordVoice.Right + 12;
        int inputRight = btnSendMessage.Left - 12;
        int inputW = Math.Max(80, inputRight - inputLeft);
        int inputH = Math.Max(42, barH - 18);
        int inputTop = Math.Max(0, (barH - inputH) / 2);

        pnlInputContainer.SetBounds(inputLeft, inputTop, inputW, inputH);

        // Update rounded regions safely
        UpdateControlRegion(pnlInputContainer, 10);
        UpdateControlRegion(btnSendMessage, 10);
        UpdateControlRegion(btnEmoji, 10);
        UpdateControlRegion(btnAttachFile, 10);
        UpdateControlRegion(btnRecordVoice, 10);
    }

    private static void UpdateControlRegion(Control c, int radius)
    {
        if (c.Width <= 0 || c.Height <= 0) return;
        var oldR = c.Region;
        c.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, c.Width + 1, c.Height + 1, radius, radius));
        oldR?.Dispose();
    }

    private void UpdateSendButtonState()
    {
        bool hasText = !string.IsNullOrWhiteSpace(txtMessageInput.Text);
        btnSendMessage.Enabled = hasText;
        if (hasText)
        {
            btnSendMessage.BackColor = IconHelper.BtnBgActive;
            btnSendMessage.Cursor = Cursors.Hand;
            IconHelper.SetButtonIcon(btnSendMessage, IconHelper.Send, Color.White, 24, 16F);
        }
        else
        {
            btnSendMessage.BackColor = _session.IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(203, 213, 225);
            btnSendMessage.Cursor = Cursors.Default;
            IconHelper.SetButtonIcon(btnSendMessage, IconHelper.Send, _session.IsDarkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(148, 163, 184), 24, 16F);
        }
    }

    private void AdjustInputBarHeight()
    {
        if (string.IsNullOrEmpty(txtMessageInput.Text))
        {
            if (pnlInputBar.Height != 68)
            {
                pnlInputBar.Height = 68;
                ScrollMessagesToBottom();
            }
            return;
        }

        int lineCount = txtMessageInput.GetLineFromCharIndex(txtMessageInput.TextLength) + 1;
        int targetHeight = 68;
        if (lineCount > 1)
        {
            targetHeight = Math.Min(120, 68 + (lineCount - 1) * 18);
        }

        if (pnlInputBar.Height != targetHeight)
        {
            pnlInputBar.Height = targetHeight;
            ScrollMessagesToBottom();
        }
    }

    private async void btnAttachFileHeader_Click(object? sender, EventArgs e)
    {
        if (_activeConversation == null)
        {
            MessageBox.Show("Vui lòng chọn một cuộc trò chuyện trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await ToggleSharedContentAsync();
    }

    private async Task ToggleSharedContentAsync(bool? forceShow = null)
    {
        bool targetState = forceShow ?? !_ucSharedContent.Visible;

        if (targetState && _activeConversation == null)
        {
            return;
        }

        _ucSharedContent.Visible = targetState;
        IconHelper.ApplyToolbarButtonStyle(btnAttachFileHeader, IconHelper.Attach, _session.IsDarkMode, isActive: targetState);

        // Visual highlight for file icon in header
        btnAttachFileHeader.BackColor = targetState ? Color.FromArgb(224, 231, 255) : Color.FromArgb(241, 245, 249);
        btnAttachFileHeader.ForeColor = targetState ? Color.FromArgb(37, 99, 235) : Color.FromArgb(71, 85, 105);

        if (targetState && _activeConversation != null)
        {
            await _ucSharedContent.LoadContentAsync(_activeConversation);
        }

        // Adjust bubbles width to fit new chat area width
        int w = flpMessages.ClientSize.Width - 25;
        if (w > 100)
        {
            foreach (Control c in flpMessages.Controls)
            {
                if (c is MessageBubbleControl mb && mb.MessageData != null)
                    mb.Width = w;
            }
        }
    }

    private async void btnAttachFile_Click(object? sender, EventArgs e)
    {
        if (_activeConversation == null)
        {
            MessageBox.Show("Vui lòng chọn một cuộc trò chuyện trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using OpenFileDialog ofd = new()
        {
            Title = "Chọn hình ảnh hoặc tệp tài liệu đính kèm",
            Filter = "Tất cả tệp được hỗ trợ|*.jpg;*.jpeg;*.png;*.gif;*.webp;*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt;*.zip;*.rar;*.7z;*.mp4;*.mp3;*.wav|" +
                     "Hình ảnh (*.jpg, *.png, *.webp, *.gif)|*.jpg;*.jpeg;*.png;*.gif;*.webp|" +
                     "Tài liệu (*.pdf, *.docx, *.xlsx, *.pptx, *.txt)|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt|" +
                     "Tất cả tệp (*.*)|*.*"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string filePath = ofd.FileName;
            if (!File.Exists(filePath)) return;

            FileInfo fi = new(filePath);
            if (fi.Length > 52428800) // 50MB
            {
                MessageBox.Show("Tệp tin vượt quá dung lượng tối đa 50MB cho phép trong mạng LAN.", "Cảnh báo dung lượng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnAttachFile.Enabled = false;
            ShowToast("Đang gửi tệp đính kèm lên máy chủ LAN...", ToastType.Info);

            try
            {
                var uploadRes = await _apiClient.UploadFileAsync(_activeConversation.Id, filePath);

                if (uploadRes?.Success == true && uploadRes.Data != null)
                {
                    var att = uploadRes.Data;

                    // Keep the exact uploaded bytes in the sender cache before the
                    // SignalR echo creates its inline media preview.
                    await FileCacheService.Instance.CacheLocalFileAsync(att, filePath);

                    string ext = Path.GetExtension(filePath).ToLower();
                    MessageType msgType = MessageType.Document;
                    if (ext is ".jpg" or ".png" or ".jpeg" or ".gif" or ".webp")
                        msgType = MessageType.Image;
                    else if (ext is ".mp4" or ".mov" or ".avi")
                        msgType = MessageType.Video;

                    var dto = new SendMessageDto
                    {
                        ConversationId = _activeConversation.Id,
                        Content = Path.GetFileName(filePath),
                        Type = msgType,
                        AttachmentIds = [att.Id]
                    };

                    await _signalR.SendMessageAsync(dto);
                    ShowToast("Đã gửi tệp đính kèm thành công", ToastType.Success);
                }
                else
                {
                    MessageBox.Show(uploadRes?.Message ?? "Tải file lên máy chủ thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối khi gửi tệp: {ex.Message}", "Lỗi truyền tệp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAttachFile.Enabled = true;
            }
        }
    }

    private async void btnRecordVoice_Click(object? sender, EventArgs e)
    {
        if (_activeConversation == null)
        {
            MessageBox.Show("Vui lòng chọn một cuộc trò chuyện trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!_isRecordingVoice)
        {
            if (NAudio.Wave.WaveInEvent.DeviceCount == 0)
            {
                MessageBox.Show("Không tìm thấy thiết bị Microphone (Micro) kết nối trên máy tính của bạn.\r\nVui lòng cắm hoặc kích hoạt Micro để ghi âm.", "Không có Micro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _audioService.StartRecording();
                _isRecordingVoice = true;
                IconHelper.SetButtonIcon(btnRecordVoice, IconHelper.StopRecord, Color.White, 22, 16F);
                btnRecordVoice.BackColor = IconHelper.StatusDanger;
                btnRecordVoice.ForeColor = Color.White;
                txtMessageInput.PlaceholderText = "Đang ghi âm giọng nói... Nhấn nút đỏ để dừng và gửi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi truy cập micro: {ex.Message}", "Lỗi thiết bị", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            _isRecordingVoice = false;
            IconHelper.ApplyToolbarButtonStyle(btnRecordVoice, IconHelper.Mic, _session.IsDarkMode);
            txtMessageInput.PlaceholderText = "Nhập tin nhắn...";

            var (wavFile, duration) = _audioService.StopRecording();
            if (string.IsNullOrEmpty(wavFile) || !File.Exists(wavFile)) return;

            if (duration < 1)
            {
                ShowToast("Đoạn ghi âm quá ngắn (dưới 1 giây)", ToastType.Warning);
                _audioService.Cleanup();
                return;
            }

            ShowToast($"Đang tải tin nhắn thoại ({duration}s) lên máy chủ...", ToastType.Info);
            try
            {
                var uploadRes = await _apiClient.UploadVoiceAsync(_activeConversation.Id, wavFile, duration);
                if (uploadRes?.Success == true && uploadRes.Data != null)
                {
                    // Pre-cache on sender machine
                    try
                    {
                        string localCache = FileCacheService.Instance.GetLocalCachePath(uploadRes.Data);
                        if (!File.Exists(localCache))
                        {
                            File.Copy(wavFile, localCache, true);
                        }
                    }
                    catch { }

                    var dto = new SendMessageDto
                    {
                        ConversationId = _activeConversation.Id,
                        Content = "Tin nhắn thoại",
                        Type = MessageType.Voice,
                        AttachmentIds = [uploadRes.Data.Id]
                    };
                    await _signalR.SendMessageAsync(dto);
                    ShowToast("Đã gửi tin nhắn thoại thành công", ToastType.Success);
                }
                else
                {
                    MessageBox.Show(uploadRes?.Message ?? "Không thể gửi tin nhắn thoại lên máy chủ.", "Lỗi gửi thoại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi truyền tin nhắn thoại: {ex.Message}", "Lỗi mạng LAN", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _audioService.Cleanup();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Calls & Group Actions
    // ─────────────────────────────────────────────────────────────────────────
    private async void btnVoiceCall_Click(object? sender, EventArgs e)
    {
        await InitiateCallAsync(CallType.Audio);
    }

    private async void btnVideoCall_Click(object? sender, EventArgs e)
    {
        await InitiateCallAsync(CallType.Video);
    }

    public async Task StartCallWithUserAsync(Guid conversationId, Guid? receiverId, CallType type)
    {
        if (_activeCallWindow != null && !_activeCallWindow.IsDisposed)
        {
            MessageBox.Show("Bạn đang trong một cuộc gọi khác. Vui lòng kết thúc cuộc gọi hiện tại trước khi bắt đầu cuộc gọi mới.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _activeCallWindow.BringToFront();
            return;
        }

        if (receiverId.HasValue && _session.IsUserBlocked(receiverId.Value))
        {
            MessageBox.Show("Bạn đã chặn người dùng này. Bỏ chặn để thực hiện cuộc gọi.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var req = new StartCallRequestDto
        {
            ConversationId = conversationId,
            ReceiverId = receiverId,
            CallType = type
        };

        await _signalR.StartCallAsync(req);
    }

    private async Task InitiateCallAsync(CallType type)
    {
        if (_activeConversation == null) return;

        Guid? receiverId = null;
        if (_activeConversation.Type == ConversationType.Direct)
        {
            var other = _activeConversation.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser?.Id);
            receiverId = other?.UserId;
        }

        await StartCallWithUserAsync(_activeConversation.Id, receiverId, type);
    }

    private async void btnGroupInfo_Click(object? sender, EventArgs e)
    {
        if (_activeConversation?.ChatGroupId == null) return;

        Guid conversationId = _activeConversation.Id;
        using var groupForm = new GroupManagerForm(_apiClient, _session, _activeConversation.ChatGroupId.Value);
        if (groupForm.ShowDialog() == DialogResult.OK)
        {
            await LoadConversationsAsync();
            ConversationDto? refreshed = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (refreshed is not null) SelectConversation(refreshed);
        }
    }

    private async void btnNewGroup_Click(object? sender, EventArgs e)
    {
        using var groupForm = new GroupManagerForm(_apiClient, _session);
        if (groupForm.ShowDialog() == DialogResult.OK)
        {
            await LoadConversationsAsync();
            if (groupForm.CreatedOrModifiedConversationId is Guid conversationId)
            {
                ConversationDto? created = _conversations.FirstOrDefault(c => c.Id == conversationId);
                if (created is not null) SelectConversation(created);
            }
        }
    }

    private async void btnOpenFriends_Click(object? sender, EventArgs e)
    {
        using var friendForm = new FriendManagerForm(_apiClient, _session);
        if (friendForm.ShowDialog() == DialogResult.OK && friendForm.SelectedConversationIdToOpen.HasValue)
        {
            Guid targetId = friendForm.SelectedConversationIdToOpen.Value;
            await LoadConversationsAsync();
            var target = _conversations.FirstOrDefault(c => c.Id == targetId);
            if (target == null)
            {
                var convRes = await _apiClient.GetConversationAsync(targetId);
                if (convRes?.Success == true && convRes.Data != null)
                {
                    target = convRes.Data;
                    _conversations.Insert(0, target);
                    RenderConversationList(txtSearchConversations.Text.Trim());
                }
            }
            if (target != null)
                SelectConversation(target);
        }
        else
        {
            _ = LoadConversationsAsync();
        }
    }

    private void btnLogout_Click(object? sender, EventArgs e)
    {
        var ask = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất khỏi tài khoản?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ask == DialogResult.Yes)
        {
            _session.ClearSession();
            _session.ClearRememberMe();
            DialogResult = DialogResult.Abort;
            Close();
        }
    }

    private void txtSearchConversations_TextChanged(object? sender, EventArgs e)
    {
        RenderConversationList(txtSearchConversations.Text.Trim());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Message Bubble Context Actions
    // ─────────────────────────────────────────────────────────────────────────
    private async void HandleEditMessage(MessageDto msg)
    {
        string? newContent = ShowPrompt("Nhập nội dung mới cho tin nhắn:", "Chỉnh sửa tin nhắn", msg.Content ?? "");

        if (!string.IsNullOrWhiteSpace(newContent) && newContent != msg.Content)
        {
            var res = await _apiClient.EditMessageAsync(msg.Id, new EditMessageDto { Content = newContent });
            if (res?.Success == true)
            {
                HandleMessageEdited(new MessageEditedDto
                {
                    MessageId = msg.Id,
                    ConversationId = msg.ConversationId,
                    NewContent = newContent,
                    EditedAt = DateTime.UtcNow
                });
            }
            else
            {
                MessageBox.Show(res?.Message ?? "Không thể chỉnh sửa tin nhắn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private static string? ShowPrompt(string prompt, string title, string defaultValue)
    {
        using Form promptForm = new()
        {
            Width = 450,
            Height = 170,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        Label lbl = new() { Left = 20, Top = 15, Text = prompt, AutoSize = true };
        TextBox txt = new() { Left = 20, Top = 40, Width = 390, Text = defaultValue };
        Button btnOk = new() { Text = "Xác nhận", Left = 230, Width = 85, Top = 80, DialogResult = DialogResult.OK };
        Button btnCancel = new() { Text = "Hủy", Left = 325, Width = 85, Top = 80, DialogResult = DialogResult.Cancel };

        promptForm.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
        promptForm.AcceptButton = btnOk;
        promptForm.CancelButton = btnCancel;

        return promptForm.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
    }

    private async void HandleRevokeMessage(MessageDto msg)
    {
        var ask = MessageBox.Show("Bạn có chắc chắn muốn thu hồi tin nhắn này cho tất cả mọi người?", "Thu hồi tin nhắn", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (ask == DialogResult.Yes)
        {
            var res = await _apiClient.RevokeMessageAsync(msg.Id);
            if (res?.Success == true)
            {
                HandleMessageRevoked(new MessageStateChangedDto
                {
                    MessageId = msg.Id,
                    ConversationId = msg.ConversationId,
                    ChangedAt = DateTime.UtcNow
                });
            }
            else
            {
                MessageBox.Show(res?.Message ?? "Không thể thu hồi tin nhắn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void HandleHideMessageForMe(MessageDto msg)
    {
        DialogResult ask = MessageBox.Show(
            "Xóa tin nhắn này ở phía bạn? Những người khác vẫn sẽ thấy tin nhắn.",
            "Xóa tin nhắn",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (ask != DialogResult.Yes) return;

        var result = await _apiClient.HideMessageForMeAsync(msg.Id);
        if (result?.Success == true)
        {
            HandleMessageHiddenForMe(new MessageStateChangedDto
            {
                MessageId = msg.Id,
                ConversationId = msg.ConversationId,
                ChangedAt = DateTime.UtcNow
            });
        }
        else
        {
            MessageBox.Show(result?.Message ?? "Không thể xóa tin nhắn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void HandleDownloadAttachment(AttachmentDto att)
    {
        string ext = Path.GetExtension(att.OriginalFileName);
        using SaveFileDialog sfd = new()
        {
            FileName = att.OriginalFileName,
            Filter = $"Tệp (*{ext})|*{ext}|Tất cả tệp (*.*)|*.*",
            Title = "Lưu tệp tin đính kèm về máy tính"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            ShowToast("Đang chuẩn bị lưu tệp tin...", ToastType.Info);
            try
            {
                string? localPath = await FileCacheService.Instance.EnsureCachedAsync(att, _session.ServerUrl, _apiClient);
                if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
                {
                    File.Copy(localPath, sfd.FileName, overwrite: true);
                    ShowToast("Đã lưu tệp tin thành công!", ToastType.Success);
                }
                else
                {
                    MessageBox.Show("Không thể tải tệp tin từ máy chủ lúc này.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu tệp: {ex.Message}", "Lỗi lưu tệp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private static Bitmap GenerateAvatarBitmap(string name, bool isGroup = false)
    {
        return IconHelper.GenerateAvatarBitmap(name, isGroup, 40);
    }

    private async Task LoadAvatarIntoAsync(PictureBox pictureBox, string? avatarUrl)
    {
        Image? source = await AvatarImageCache.GetAsync(_session.ServerUrl, avatarUrl);
        if (source is null || IsDisposed || pictureBox.IsDisposed) return;
        Bitmap rendered = AvatarImageCache.CreateCircularFit(source, pictureBox.Size, Color.FromArgb(30, 41, 59), Color.FromArgb(148, 163, 184));
        Image? old = pictureBox.Image;
        pictureBox.Image = rendered;
        old?.Dispose();
    }

    private void PreviewConversationAvatar(ConversationDto conversation)
    {
        _ = PreviewAvatarAsync(conversation.AvatarUrl, conversation.Title);
    }

    private void ShowActiveAvatarPreview()
    {
        if (_activeConversation is not null) _ = PreviewAvatarAsync(_activeConversation.AvatarUrl, _activeConversation.Title);
    }

    private async Task PreviewAvatarAsync(string? avatarUrl, string title)
    {
        Image? image = await AvatarImageCache.GetAsync(_session.ServerUrl, avatarUrl);
        if (image is not null && !IsDisposed) new AvatarPreviewForm(image, title).ShowDialog(this);
    }

    private void picMyAvatar_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;
        ContextMenuStrip menu = new();
        menu.Items.Add("Xem ảnh đại diện kích thước đầy đủ", null, async (s, a) =>
        {
            if (_session.CurrentUser is not null) await PreviewAvatarAsync(_session.CurrentUser.AvatarUrl, "Ảnh đại diện của tôi");
        });
        menu.Items.Add("Đổi ảnh đại diện...", null, picMyAvatar_Click);
        menu.Items.Add("Chỉnh sửa hồ sơ...", null, async (s, a) => await EditMyProfileAsync());
        menu.Show(picMyAvatar, e.Location);
    }

    private async Task EditMyProfileAsync()
    {
        if (_session.CurrentUser is null) return;
        using EditProfileForm form = new(_session.CurrentUser, _session.ServerUrl, _session.IsDarkMode);
        if (form.ShowDialog(this) != DialogResult.OK || form.ProfileChanges is null) return;

        if (!string.IsNullOrWhiteSpace(form.SelectedAvatarPath))
        {
            var avatarResult = await _apiClient.UploadAvatarAsync(form.SelectedAvatarPath);
            if (avatarResult?.Success != true || string.IsNullOrWhiteSpace(avatarResult.Data))
            {
                MessageBox.Show(avatarResult?.Message ?? "Không thể cập nhật ảnh đại diện.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        var result = await _apiClient.UpdateProfileAsync(form.ProfileChanges);
        if (result?.Success != true || result.Data is null)
        {
            MessageBox.Show(result?.Message ?? "Không thể cập nhật hồ sơ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        ApplyMyProfile(result.Data);
    }

    private async Task EditNicknamesAsync()
    {
        if (_activeConversation is null || _session.CurrentUser is null) return;
        ConversationMemberDto? mine = _activeConversation.Members.FirstOrDefault(m => m.UserId == _session.CurrentUser.Id);
        ConversationMemberDto? other = _activeConversation.Members.FirstOrDefault(m => m.UserId != _session.CurrentUser.Id);
        if (mine is null) return;
        string? myNickname = ShowPrompt("Biệt danh của bạn trong cuộc trò chuyện (để trống để xóa):", "Đặt biệt danh", mine.Nickname ?? string.Empty);
        if (myNickname is null) return;
        string? otherNickname = other is null ? null : ShowPrompt($"Biệt danh cho {other.Username} (để trống để xóa):", "Đặt biệt danh", other.Nickname ?? string.Empty);
        if (other is not null && otherNickname is null) return;
        var mineResult = await _apiClient.SetNicknameAsync(_activeConversation.Id, new SetNicknameRequestDto { TargetUserId = mine.UserId, Nickname = myNickname });
        if (mineResult?.Success != true) { MessageBox.Show(mineResult?.Message ?? "Không thể cập nhật biệt danh.", "Lỗi"); return; }
        if (other is not null)
            await _apiClient.SetNicknameAsync(_activeConversation.Id, new SetNicknameRequestDto { TargetUserId = other.UserId, Nickname = otherNickname });
        await LoadConversationsAsync();
        if (_activeConversation is not null)
        {
            var refreshed = _conversations.FirstOrDefault(c => c.Id == _activeConversation.Id);
            if (refreshed is not null) SelectConversation(refreshed);
        }
    }

    private async void HandleUserProfileUpdated(UserProfileDto profile)
    {
        if (InvokeRequired) { BeginInvoke(() => HandleUserProfileUpdated(profile)); return; }
        if (_session.CurrentUser?.Id == profile.Id) ApplyMyProfile(profile);
        await LoadConversationsAsync();
        if (_activeConversation is not null && _activeConversation.Members.Any(m => m.UserId == profile.Id))
        {
            ConversationDto? refreshed = _conversations.FirstOrDefault(c => c.Id == _activeConversation.Id);
            if (refreshed is not null) SelectConversation(refreshed);
        }
    }

    private void ApplyMyProfile(UserProfileDto profile)
    {
        if (_session.CurrentUser is null) return;
        _session.CurrentUser.Username = profile.Username;
        _session.CurrentUser.DisplayName = profile.DisplayName;
        _session.CurrentUser.AvatarUrl = profile.AvatarUrl;
        _session.CurrentUser.Bio = profile.Bio;
        lblMyUsername.Text = profile.DisplayName;
        _ = LoadAvatarIntoAsync(picMyAvatar, profile.AvatarUrl);
    }

    private async void HandleNicknameUpdated(Guid conversationId, Guid targetUserId, string? nickname, MessageDto systemMessage)
    {
        if (InvokeRequired) { BeginInvoke(() => HandleNicknameUpdated(conversationId, targetUserId, nickname, systemMessage)); return; }
        await LoadConversationsAsync();
        if (_activeConversation?.Id == conversationId)
        {
            ConversationDto? refreshed = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (refreshed is not null) SelectConversation(refreshed);
        }
    }

    private async void picMyAvatar_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog ofd = new()
        {
            Title = "Chọn ảnh đại diện mới",
            Filter = "Tệp hình ảnh|*.jpg;*.jpeg;*.png;*.webp;*.gif"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            var res = await _apiClient.UploadAvatarAsync(ofd.FileName);
            if (res?.Success == true && !string.IsNullOrEmpty(res.Data))
            {
                if (_session.CurrentUser != null)
                    _session.CurrentUser.AvatarUrl = res.Data;

                await LoadAvatarIntoAsync(picMyAvatar, res.Data);

                MessageBox.Show("Cập nhật ảnh đại diện thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(res?.Message ?? "Không thể cập nhật ảnh đại diện lúc này.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void menuTrayOpen_Click(object? sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void menuTrayExit_Click(object? sender, EventArgs e)
    {
        notifyIconTray.Visible = false;
        Application.Exit();
    }

    private async void MainMessengerForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        tmrAiFloat.Stop();
        VoicePlayerManager.Instance.Stop();
        notifyIconTray.Visible = false;
        await _signalR.DisconnectAsync();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            if (pnlMoreMenu.Visible)
            {
                HideMoreMenu();
                return true;
            }
            if (pnlMessageSearch.Visible)
            {
                btnCloseSearch_Click(this, EventArgs.Empty);
                return true;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AI Hologram Floating Bubble (NavStrip)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Initializes the glowing hologram AI bubble in the left nav menu above LAN status.</summary>
    private void InitAiRobotBubble()
    {
        picAiRobotBubble.Image = null;
        picAiRobotBubble.Size = new Size(56, 56);
        picAiRobotBubble.BackColor = Color.Transparent;
        picAiRobotBubble.Region = null; // Don't clip so outer neon glow aura renders smoothly

        // Ensure child of pnlNavStrip
        if (picAiRobotBubble.Parent != pnlNavStrip)
        {
            Controls.Remove(picAiRobotBubble);
            pnlNavStrip.Controls.Add(picAiRobotBubble);
        }

        // Center horizontally in pnlNavStrip, right above lblLanIndicator
        int left = Math.Max(0, (pnlNavStrip.Width - picAiRobotBubble.Width) / 2);
        _aiBaseY = Math.Max(0, pnlNavStrip.Height - lblLanIndicator.Height - picAiRobotBubble.Height - 10);
        picAiRobotBubble.Location = new Point(left, _aiBaseY);
        picAiRobotBubble.BringToFront();

        tmrAiFloat.Start();

        pnlNavStrip.Resize += (s, e) =>
        {
            _aiBaseY = Math.Max(0, pnlNavStrip.Height - lblLanIndicator.Height - picAiRobotBubble.Height - 10);
            picAiRobotBubble.Left = Math.Max(0, (pnlNavStrip.Width - picAiRobotBubble.Width) / 2);
            picAiRobotBubble.Top = _aiBaseY;
        };
    }

    /// <summary>Gentle vertical floating animation using sine wave.</summary>
    private void tmrAiFloat_Tick(object? sender, EventArgs e)
    {
        _aiFloatTick += 0.08;
        int offset = (int)(Math.Sin(_aiFloatTick) * 3);
        picAiRobotBubble.Top = _aiBaseY + offset;
    }

    private void picAiRobotBubble_MouseEnter(object? sender, EventArgs e)
    {
        _isAiBubbleHovered = true;
        picAiRobotBubble.Invalidate();
    }

    private void picAiRobotBubble_MouseLeave(object? sender, EventArgs e)
    {
        _isAiBubbleHovered = false;
        picAiRobotBubble.Invalidate();
    }

    /// <summary>Pure vector GDI+ Cyber Glow Hologram rendering for AI Bubble.</summary>
    private void picAiRobotBubble_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int w = picAiRobotBubble.Width;
        int h = picAiRobotBubble.Height;
        // Lùi vào một chút để không bị mất góc khi vẽ hiệu ứng phát sáng
        var rect = new Rectangle(5, 5, w - 10, h - 10);

        // 1. Vẽ quầng sáng Neon mờ (Glow Aura) bao quanh
        int glowAlpha = _isAiBubbleHovered ? 80 : 40;
        using (var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, 6, 182, 212)))
        {
            g.FillEllipse(glowBrush, 1, 1, w - 2, h - 2);
        }

        // 2. Vẽ nền tròn chuyển màu Gradient Hologram 3D (Tím Cyberpunk -> Xanh ngọc)
        Color c1 = _isAiChatActive ? Color.FromArgb(168, 85, 247) : (_isAiBubbleHovered ? Color.FromArgb(99, 102, 241) : Color.FromArgb(124, 58, 237));
        Color c2 = _isAiChatActive ? Color.FromArgb(236, 72, 153) : (_isAiBubbleHovered ? Color.FromArgb(6, 182, 212) : Color.FromArgb(14, 165, 233));

        using (var bgBrush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, c1, c2, 135f))
        {
            g.FillEllipse(bgBrush, rect);
        }

        // 3. Vẽ viền mảnh phản chiếu ánh sáng (Border)
        Color borderColor = _isAiBubbleHovered ? Color.FromArgb(250, 255, 255, 255) : Color.FromArgb(140, 255, 255, 255);
        using (var borderPen = new Pen(borderColor, 1.8f))
        {
            g.DrawEllipse(borderPen, rect);
        }

        // 4. Vẽ ngôi sao AI 4 cánh ở chính giữa (Sparkle Icon)
        DrawAiStar(g, w / 2f, h / 2f, 12, Color.White);

        // 5. Vẽ vài hạt sáng, chấm bụi sao li ti xung quanh để tăng hiệu ứng ma thuật (Cyberpunk)
        using (var starBrush = new SolidBrush(Color.FromArgb(210, 255, 255, 255)))
        {
            g.FillEllipse(starBrush, w / 2 - 10, h / 2 - 10, 2, 2);
            g.FillEllipse(starBrush, w / 2 + 9, h / 2 + 8, 2, 2);
        }
        // Ngôi sao phụ nhỏ góc trên bên phải
        DrawAiStar(g, w / 2f + 10, h / 2f - 9, 4.5f, Color.FromArgb(230, 255, 255, 255));
    }

    /// <summary>Hàm vẽ vector ngôi sao 4 cánh mềm mại, sắc nét</summary>
    private static void DrawAiStar(Graphics g, float cx, float cy, float radius, Color color)
    {
        AiAvatarHelper.DrawAiStar(g, cx, cy, radius, color);
    }

    /// <summary>Click handler: navigate to or create the AI conversation.</summary>
    private async void picAiRobotBubble_Click(object? sender, EventArgs e)
    {
        _isAiChatActive = true;
        picAiRobotBubble.Invalidate();
        try
        {
            // Show chat view
            ShowChatView();

            // Get or create AI conversation from server
            if (_aiConversationId == Guid.Empty)
            {
                var res = await _apiClient.GetAiConversationAsync();
                if (res?.Success == true && res.Data != Guid.Empty)
                {
                    _aiConversationId = res.Data;
                }
                else
                {
                    ShowToast("Không thể kết nối Chatting AI. Kiểm tra server.", ToastType.Error);
                    return;
                }
            }

            // Refresh conversation list (AI conv may not be in list yet)
            await LoadConversationsAsync();

            // Select the AI conversation
            var aiConv = _conversations.FirstOrDefault(c => c.Id == _aiConversationId);
            if (aiConv != null)
            {
                SelectConversation(aiConv);
            }
            else
            {
                // Fetch directly from server
                var convRes = await _apiClient.GetConversationAsync(_aiConversationId);
                if (convRes?.Success == true && convRes.Data != null)
                {
                    await LoadConversationsAsync();
                    var found = _conversations.FirstOrDefault(c => c.Id == _aiConversationId);
                    SelectConversation(found ?? convRes.Data);
                }
                else
                {
                    ShowToast("Không thể mở cuộc trò chuyện AI.", ToastType.Error);
                }
            }
        }
        catch (Exception ex)
        {
            ShowToast($"Lỗi mở AI chat: {ex.Message}", ToastType.Error);
        }
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

/// <summary>Extension to draw rounded rectangles (GDI+ helper).</summary>
internal static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, float x, float y, float width, float height, float radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
        path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
        path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseAllFigures();
        g.FillPath(brush, path);
    }
}
