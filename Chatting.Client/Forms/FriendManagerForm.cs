using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Forms;

public partial class FriendManagerForm : Form
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;

    public Guid? SelectedConversationIdToOpen { get; private set; }

    private List<FriendDto> _friendsList = [];
    private List<FriendRequestDto> _incomingRequests = [];
    private List<FriendRequestDto> _outgoingRequests = [];
    private string _activeTabKey = "friends"; // "friends", "search", "requests"

    public FriendManagerForm(ApiClient apiClient, SessionManager session)
    {
        _apiClient = apiClient;
        _session = session;

        InitializeComponent();
        ApplyCustomStyling();
    }

    private void ApplyCustomStyling()
    {
        // 1. Robot mascot image loading
        Image? mascotImg = LoadRobotMascot();
        if (mascotImg != null)
        {
            picRobotMascot.Image = mascotImg;
        }

        // 2. Custom header gradient painting
        pnlHeader.Paint += (s, pe) =>
        {
            using LinearGradientBrush lgb = new(
                new Point(0, 0),
                new Point(pnlHeader.Width, 0),
                Color.FromArgb(29, 78, 216),  // #1D4ED8
                Color.FromArgb(59, 130, 246)  // #3B82F6
            );
            pe.Graphics.FillRectangle(lgb, 0, 0, pnlHeader.Width, pnlHeader.Height);

            // Subtle decorative circles watermark on the right
            using var waterBrush = new SolidBrush(Color.FromArgb(20, 255, 255, 255));
            pe.Graphics.FillEllipse(waterBrush, pnlHeader.Width - 120, -30, 140, 140);
            pe.Graphics.FillEllipse(waterBrush, pnlHeader.Width - 65, 20, 85, 85);
        };

        // 3. Round card and toolbars
        UpdateControlRegion(pnlMainCard, 14);
        UpdateControlRegion(btnNavFriends, 10);
        UpdateControlRegion(btnNavSearch, 10);
        UpdateControlRegion(btnNavRequests, 10);
        UpdateControlRegion(btnNavRefresh, 10);
        UpdateControlRegion(pnlMyStatus, 8);
        UpdateControlRegion(pnlFriendSearchBox, 8);
        UpdateControlRegion(btnAddFriendQuick, 8);
        UpdateControlRegion(pnlSearchBox, 8);
        UpdateControlRegion(btnSearch, 8);
        UpdateControlRegion(pnlIncomingHeader, 8);
        UpdateControlRegion(pnlOutgoingHeader, 8);

        // Current user status
        lblMyStatusText.Text = _session.CurrentUser?.Username ?? "Trực tuyến";

        IconHelper.SetButtonIcon(btnNavRefresh, IconHelper.Refresh, 14, Color.FromArgb(100, 116, 139), ContentAlignment.MiddleLeft);
        IconHelper.SetButtonIcon(btnSearch, IconHelper.Search, 14, Color.White, ContentAlignment.MiddleLeft);
        IconHelper.SetButtonIcon(btnAddFriendQuick, IconHelper.AddUser, 14, Color.White, ContentAlignment.MiddleLeft);

        // Navigation pill events
        btnNavFriends.Click += (s, e) => SwitchTab("friends");
        btnNavSearch.Click += (s, e) => SwitchTab("search");
        btnNavRequests.Click += (s, e) => SwitchTab("requests");
        btnNavRefresh.Click += async (s, e) => await LoadAllDataAsync();

        // Top horizontal tabs events
        btnTopTabFriends.Click += (s, e) => SwitchTab("friends");
        btnTopTabSearch.Click += (s, e) => SwitchTab("search");
        btnTopTabRequests.Click += (s, e) => SwitchTab("requests");

        // Action buttons & search
        btnAddFriendQuick.Click += (s, e) =>
        {
            SwitchTab("search");
            txtSearchKeyword.Focus();
        };

        cboFriendFilter.SelectedIndex = 0;
        cboFriendFilter.SelectedIndexChanged += (s, e) => ApplyFriendsFilter();
        txtFriendSearch.TextChanged += (s, e) => ApplyFriendsFilter();

        btnSearch.Click += (s, e) => ExecuteSearch();
        txtSearchKeyword.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ExecuteSearch();
            }
        };

        // Handle responsive resizing
        Resize += (s, e) =>
        {
            UpdateTabIndicatorPosition();
            AdjustTableRowsWidth();
        };

        pnlTopTabs.Resize += (s, e) => UpdateTabIndicatorPosition();
        pnlSearchToolbar.SizeChanged += (s, e) => LayoutSearchToolbar();
        pnlSearchTableHeader.SizeChanged += (s, e) => LayoutSearchTable();
        flpSearchResults.SizeChanged += (s, e) =>
        {
            LayoutSearchToolbar();
            LayoutSearchTable();
            AdjustTableRowsWidth();
        };
        LayoutSearchToolbar();
    }

    private static Image? LoadRobotMascot()
    {
        try
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(appDir, "Assets", "robot_mascot.png");
            if (File.Exists(path))
            {
                using var stream = new MemoryStream(File.ReadAllBytes(path));
                return Image.FromStream(stream);
            }
        }
        catch { }
        return null;
    }

    private static void UpdateControlRegion(Control ctrl, int radius)
    {
        if (ctrl.Width > 0 && ctrl.Height > 0)
        {
            ctrl.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, ctrl.Width, ctrl.Height, radius, radius));
        }
    }

    private async void FriendManagerForm_Load(object? sender, EventArgs e)
    {
        SwitchTab("friends");
        await LoadAllDataAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab Navigation
    // ─────────────────────────────────────────────────────────────────────────
    private void SwitchTab(string tabKey)
    {
        _activeTabKey = tabKey;

        // 1. Update left sidebar pills
        UpdateNavPill(btnNavFriends, tabKey == "friends", IconHelper.People);
        UpdateNavPill(btnNavSearch, tabKey == "search", IconHelper.Search);
        UpdateNavPill(btnNavRequests, tabKey == "requests", IconHelper.AddUser);

        // 2. Update top horizontal tabs
        Color friendsColor = tabKey == "friends" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
        btnTopTabFriends.ForeColor = friendsColor;
        btnTopTabFriends.Font = new Font("Segoe UI Semibold", 9.5F, tabKey == "friends" ? FontStyle.Bold : FontStyle.Regular);
        IconHelper.SetButtonIcon(btnTopTabFriends, IconHelper.People, 14, friendsColor, ContentAlignment.MiddleLeft);

        Color searchColor = tabKey == "search" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
        btnTopTabSearch.ForeColor = searchColor;
        btnTopTabSearch.Font = new Font("Segoe UI Semibold", 9.5F, tabKey == "search" ? FontStyle.Bold : FontStyle.Regular);
        IconHelper.SetButtonIcon(btnTopTabSearch, IconHelper.Search, 14, searchColor, ContentAlignment.MiddleLeft);

        Color reqColor = tabKey == "requests" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
        btnTopTabRequests.ForeColor = reqColor;
        btnTopTabRequests.Font = new Font("Segoe UI Semibold", 9.5F, tabKey == "requests" ? FontStyle.Bold : FontStyle.Regular);
        IconHelper.SetButtonIcon(btnTopTabRequests, IconHelper.AddUser, 14, reqColor, ContentAlignment.MiddleLeft);

        UpdateTabIndicatorPosition();

        // 3. Switch visible page
        pnlTabFriendsPage.Visible = tabKey == "friends";
        pnlTabSearchPage.Visible = tabKey == "search";
        pnlTabRequestsPage.Visible = tabKey == "requests";

        if (tabKey == "friends") pnlTabFriendsPage.BringToFront();
        else if (tabKey == "search") pnlTabSearchPage.BringToFront();
        else if (tabKey == "requests") pnlTabRequestsPage.BringToFront();

        AdjustTableRowsWidth();
    }

    private static void UpdateNavPill(Button btn, bool active, string glyph)
    {
        btn.BackColor = active ? Color.FromArgb(224, 237, 255) : Color.Transparent;
        Color iconColor = active ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
        btn.ForeColor = iconColor;
        btn.Font = new Font("Segoe UI Semibold", 9.25F, active ? FontStyle.Bold : FontStyle.Regular);
        IconHelper.SetButtonIcon(btn, glyph, 15, iconColor, ContentAlignment.MiddleLeft);
    }

    private void UpdateTabIndicatorPosition()
    {
        Button? target = _activeTabKey switch
        {
            "friends" => btnTopTabFriends,
            "search" => btnTopTabSearch,
            "requests" => btnTopTabRequests,
            _ => btnTopTabFriends
        };

        if (target != null)
        {
            pnlTopTabIndicator.SetBounds(target.Left + 10, pnlTopTabs.Height - 3, Math.Max(40, target.Width - 20), 3);
            pnlTopTabIndicator.BringToFront();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Data Loading & Syncing
    // ─────────────────────────────────────────────────────────────────────────
    private async Task LoadAllDataAsync()
    {
        btnNavRefresh.Enabled = false;
        btnNavRefresh.Text = "  Đang tải...";

        try
        {
            await Task.WhenAll(LoadFriendsAsync(), LoadRequestsAsync());
        }
        finally
        {
            btnNavRefresh.Enabled = true;
            btnNavRefresh.Text = "  Làm mới";
        }
    }

    private async Task LoadFriendsAsync()
    {
        var res = await _apiClient.GetFriendsAsync();
        if (res?.Success == true && res.Data != null)
        {
            _friendsList = res.Data;
        }
        else
        {
            _friendsList = [];
        }

        ApplyFriendsFilter();
    }

    private async Task LoadRequestsAsync()
    {
        var incTask = _apiClient.GetIncomingRequestsAsync();
        var outTask = _apiClient.GetOutgoingRequestsAsync();
        await Task.WhenAll(incTask, outTask);

        _incomingRequests = incTask.Result?.Success == true && incTask.Result.Data != null ? incTask.Result.Data : [];
        _outgoingRequests = outTask.Result?.Success == true && outTask.Result.Data != null ? outTask.Result.Data : [];

        RenderIncomingRequests();
        RenderOutgoingRequests();

        // Update badge count
        int totalRequests = _incomingRequests.Count + _outgoingRequests.Count;
        string reqText = totalRequests > 0 ? $"Lời mời ({totalRequests})" : "Lời mời kết bạn";
        btnNavRequests.Text = $"  {reqText}";
        btnTopTabRequests.Text = $"  {reqText}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 1: Render & Filter Friends
    // ─────────────────────────────────────────────────────────────────────────
    private void ApplyFriendsFilter()
    {
        string kw = txtFriendSearch.Text.Trim();
        int filterMode = cboFriendFilter.SelectedIndex; // 0: All, 1: Online, 2: Offline

        var query = _friendsList.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(kw))
        {
            query = query.Where(f =>
                f.Username.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(f.Email) && f.Email.Contains(kw, StringComparison.OrdinalIgnoreCase)));
        }

        if (filterMode == 1) // Online only
        {
            query = query.Where(f => f.IsOnline);
        }
        else if (filterMode == 2) // Offline only
        {
            query = query.Where(f => !f.IsOnline);
        }

        var filtered = query.OrderByDescending(f => f.IsOnline).ThenBy(f => f.Username).ToList();

        flpFriendsList.SuspendLayout();
        flpFriendsList.Controls.Clear();

        if (filtered.Count == 0)
        {
            pnlFriendsEmpty.Visible = true;
            pnlFriendsEmpty.BringToFront();

            if (string.IsNullOrWhiteSpace(kw) && filterMode == 0)
            {
                lblFriendsEmptyTitle.Text = "Chưa có bạn bè nào";
                lblFriendsEmptySub.Text = "Hãy tìm kiếm người dùng trong mạng LAN để gửi lời mời kết bạn.";
            }
            else
            {
                lblFriendsEmptyTitle.Text = "Không tìm thấy bạn bè phù hợp";
                lblFriendsEmptySub.Text = "Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc trạng thái.";
            }
        }
        else
        {
            pnlFriendsEmpty.Visible = false;

            int rowWidth = Math.Max(400, flpFriendsList.ClientSize.Width - 14);

            foreach (var f in filtered)
            {
                var row = CreateFriendRowCard(f, rowWidth);
                flpFriendsList.Controls.Add(row);
            }
        }

        flpFriendsList.ResumeLayout();

        // Footer counts
        int totalFriends = _friendsList.Count;
        int onlineFriends = _friendsList.Count(x => x.IsOnline);
        lblFriendsCount.Text = $"Tổng cộng: {totalFriends} bạn bè  •  {onlineFriends} đang online";

        // Tab badge
        btnNavFriends.Text = totalFriends > 0 ? $"  Bạn bè ({totalFriends})" : "  Bạn bè hiện tại";
        btnTopTabFriends.Text = totalFriends > 0 ? $"  Bạn bè ({totalFriends})" : "  Bạn bè hiện tại";
    }

    private Control CreateFriendRowCard(FriendDto f, int rowWidth)
    {
        Panel row = new()
        {
            Width = rowWidth,
            Height = 54,
            Margin = new Padding(0, 1, 0, 1),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };

        // Bottom separator
        row.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(241, 245, 249), 1);
            pe.Graphics.DrawLine(pen, 14, row.Height - 1, row.Width - 14, row.Height - 1);
        };

        // Hover effect
        row.MouseEnter += (s, e) => row.BackColor = Color.FromArgb(248, 250, 252);
        row.MouseLeave += (s, e) =>
        {
            if (!row.ClientRectangle.Contains(row.PointToClient(Cursor.Position)))
                row.BackColor = Color.White;
        };

        // 1. Avatar with status badge
        PictureBox picAvatar = new()
        {
            Size = new Size(36, 36),
            Location = new Point(14, 9),
            Image = GenerateAvatarBitmap(f.Username, f.IsOnline),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        _ = LoadAvatarIntoAsync(picAvatar, f.AvatarUrl);

        // 2. Name
        Label lblName = new()
        {
            Text = f.Username,
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(58, 17),
            Size = new Size(185, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        // 3. Email
        string displayEmail = !string.IsNullOrWhiteSpace(f.Email) ? f.Email : $"@{f.Username.ToLowerInvariant()}";
        Label lblEmail = new()
        {
            Text = displayEmail,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(255, 18),
            Size = new Size(205, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        // 4. Status Pill
        Panel pnlStatus = new()
        {
            Size = new Size(110, 26),
            Location = new Point(475, 14),
            BackColor = f.IsOnline ? Color.FromArgb(220, 252, 231) : Color.FromArgb(241, 245, 249)
        };
        UpdateControlRegion(pnlStatus, 13);

        Label lblStatus = new()
        {
            Text = f.IsOnline ? "●  Đang online" : "○  Ngoại tuyến",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = f.IsOnline ? Color.FromArgb(22, 163, 74) : Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        pnlStatus.Controls.Add(lblStatus);

        // 5. Action Buttons
        Button btnChat = new()
        {
            Text = "  Nhắn tin",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            BackColor = Color.FromArgb(239, 246, 255),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(92, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(row.Width - 142, 13),
            Cursor = Cursors.Hand
        };
        IconHelper.SetButtonIcon(btnChat, IconHelper.Chat, 12, Color.FromArgb(37, 99, 235), ContentAlignment.MiddleLeft);
        btnChat.FlatAppearance.BorderSize = 0;
        UpdateControlRegion(btnChat, 6);
        btnChat.Click += (s, e) => ChatWithFriend(f);

        Button btnDelete = new()
        {
            Text = "",
            BackColor = Color.FromArgb(248, 250, 252),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(32, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(row.Width - 44, 13),
            Cursor = Cursors.Hand
        };
        IconHelper.SetButtonIcon(btnDelete, IconHelper.Delete, 13, Color.FromArgb(239, 68, 68), ContentAlignment.MiddleCenter);
        btnDelete.AccessibleName = "Hủy kết bạn";
        btnDelete.AccessibleDescription = $"Xóa {f.Username} khỏi danh sách bạn bè.";
        btnDelete.FlatAppearance.BorderSize = 0;
        UpdateControlRegion(btnDelete, 6);
        btnDelete.Click += (s, e) => Unfriend(f);

        ToolTip tt = new();
        tt.SetToolTip(btnDelete, "Hủy kết bạn");
        tt.SetToolTip(btnChat, $"Mở trò chuyện với {f.Username}");

        // Double click anywhere opens chat
        row.DoubleClick += (s, e) => ChatWithFriend(f);
        lblName.DoubleClick += (s, e) => ChatWithFriend(f);
        lblEmail.DoubleClick += (s, e) => ChatWithFriend(f);
        picAvatar.DoubleClick += (s, e) => ChatWithFriend(f);

        row.Controls.Add(picAvatar);
        row.Controls.Add(lblName);
        row.Controls.Add(lblEmail);
        row.Controls.Add(pnlStatus);
        row.Controls.Add(btnChat);
        row.Controls.Add(btnDelete);

        return row;
    }

    private void ChatWithFriend(FriendDto f)
    {
        if (f.DirectConversationId.HasValue)
        {
            SelectedConversationIdToOpen = f.DirectConversationId.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show("Đang tải dữ liệu hội thoại, vui lòng thử lại sau giây lát!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void Unfriend(FriendDto f)
    {
        var ask = MessageBox.Show(
            $"Bạn có chắc chắn muốn hủy kết bạn với {f.Username}?",
            "Xác nhận hủy kết bạn",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (ask == DialogResult.Yes)
        {
            var res = await _apiClient.UnfriendAsync(f.UserId);
            if (res?.Success == true)
            {
                await LoadFriendsAsync();
            }
            else
            {
                MessageBox.Show(res?.Message ?? "Không thể hủy kết bạn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 2: User Search
    // ─────────────────────────────────────────────────────────────────────────
    private async void ExecuteSearch()
    {
        string kw = txtSearchKeyword.Text.Trim();
        if (string.IsNullOrEmpty(kw)) return;

        btnSearch.Enabled = false;
        btnSearch.Text = "⏳ Đang tìm...";
        flpSearchResults.SuspendLayout();
        flpSearchResults.Controls.Clear();

        try
        {
            var res = await _apiClient.SearchUsersAsync(kw);

            if (res?.Success == true && res.Data != null && res.Data.Count > 0)
            {
                pnlSearchEmpty.Visible = false;

                int rowWidth = GetSearchResultRowWidth();

                foreach (var u in res.Data)
                {
                    var row = CreateSearchResultRowCard(u, rowWidth);
                    flpSearchResults.Controls.Add(row);
                }
                LayoutSearchTable();
            }
            else
            {
                pnlSearchEmpty.Visible = true;
                pnlSearchEmpty.BringToFront();
                lblSearchEmptyTitle.Text = "Không tìm thấy người dùng";
                lblSearchEmptySub.Text = $"Không tìm thấy người dùng nào phù hợp với từ khóa '{kw}'.";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            flpSearchResults.ResumeLayout();
            btnSearch.Enabled = true;
            btnSearch.Text = "  Tìm kiếm";
        }
    }

    private Control CreateSearchResultRowCard(UserSearchResultDto u, int rowWidth)
    {
        Panel row = new()
        {
            Width = rowWidth,
            Height = 54,
            Margin = new Padding(0, 1, 0, 1),
            BackColor = Color.White
        };

        row.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(241, 245, 249), 1);
            pe.Graphics.DrawLine(pen, 14, row.Height - 1, row.Width - 14, row.Height - 1);
        };

        row.MouseEnter += (s, e) => row.BackColor = Color.FromArgb(248, 250, 252);
        row.MouseLeave += (s, e) =>
        {
            if (!row.ClientRectangle.Contains(row.PointToClient(Cursor.Position)))
                row.BackColor = Color.White;
        };

        // Avatar
        PictureBox picAvatar = new()
        {
            Size = new Size(36, 36),
            Location = new Point(14, 9),
            Image = GenerateAvatarBitmap(u.Username, u.IsOnline),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        _ = LoadAvatarIntoAsync(picAvatar, u.AvatarUrl);

        // Name
        Label lblName = new()
        {
            Name = "searchUserName",
            Text = u.Username,
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(58, 17),
            Size = new Size(185, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        // Email
        string displayEmail = !string.IsNullOrWhiteSpace(u.Email) ? u.Email : $"@{u.Username.ToLowerInvariant()}";
        Label lblEmail = new()
        {
            Name = "searchUserEmail",
            Text = displayEmail,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(255, 18),
            Size = new Size(205, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        // Relationship Pill
        (string relText, Color relBg, Color relFg) = u.IsFriend
            ? ("Đã là bạn bè", Color.FromArgb(220, 252, 231), Color.FromArgb(22, 163, 74))
            : u.HasSentRequest
                ? ("Đã gửi lời mời", Color.FromArgb(254, 243, 199), Color.FromArgb(217, 119, 6))
                : u.HasReceivedRequest
                    ? ("Đã nhận lời mời", Color.FromArgb(219, 234, 254), Color.FromArgb(37, 99, 235))
                    : ("Chưa kết bạn", Color.FromArgb(241, 245, 249), Color.FromArgb(100, 116, 139));

        Panel pnlRelation = new()
        {
            Name = "searchUserRelation",
            Size = new Size(115, 26),
            Location = new Point(475, 14),
            BackColor = relBg
        };
        UpdateControlRegion(pnlRelation, 13);

        Label lblRel = new()
        {
            Text = relText,
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = relFg,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        pnlRelation.Controls.Add(lblRel);

        // Action Button
        Button btnAction = new()
        {
            Name = "searchUserAction",
            Size = new Size(95, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            Location = new Point(row.Width - 110, 13),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnAction.FlatAppearance.BorderSize = 0;
        UpdateControlRegion(btnAction, 6);

        if (u.IsFriend)
        {
            btnAction.Text = "  Nhắn tin";
            btnAction.BackColor = Color.FromArgb(239, 246, 255);
            btnAction.ForeColor = Color.FromArgb(37, 99, 235);
            IconHelper.SetButtonIcon(btnAction, IconHelper.Chat, 12, Color.FromArgb(37, 99, 235), ContentAlignment.MiddleLeft);
            btnAction.Click += (s, e) =>
            {
                var f = _friendsList.FirstOrDefault(x => x.UserId == u.Id);
                if (f != null) ChatWithFriend(f);
            };
        }
        else if (u.HasSentRequest)
        {
            btnAction.Text = "  Đã gửi";
            btnAction.BackColor = Color.FromArgb(226, 232, 240);
            btnAction.ForeColor = Color.FromArgb(51, 65, 85);
            IconHelper.SetButtonIcon(btnAction, IconHelper.Check, 12, Color.FromArgb(51, 65, 85), ContentAlignment.MiddleLeft);
            btnAction.Cursor = Cursors.Default;
        }
        else if (u.HasReceivedRequest)
        {
            btnAction.Text = "  Đồng ý";
            btnAction.BackColor = Color.FromArgb(220, 252, 231);
            btnAction.ForeColor = Color.FromArgb(22, 163, 74);
            IconHelper.SetButtonIcon(btnAction, IconHelper.Check, 12, Color.FromArgb(22, 163, 74), ContentAlignment.MiddleLeft);
            btnAction.Click += async (s, e) =>
            {
                if (u.PendingRequestId.HasValue)
                {
                    btnAction.Enabled = false;
                    var res = await _apiClient.AcceptFriendRequestAsync(u.PendingRequestId.Value);
                    if (res?.Success == true)
                    {
                        MessageBox.Show($"Đã chấp nhận kết bạn với {u.Username}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAllDataAsync();
                        ExecuteSearch();
                    }
                }
            };
        }
        else
        {
            btnAction.Text = "  Kết bạn";
            btnAction.BackColor = Color.FromArgb(37, 99, 235);
            btnAction.ForeColor = Color.White;
            IconHelper.SetButtonIcon(btnAction, IconHelper.AddUser, 12, Color.White, ContentAlignment.MiddleLeft);
            btnAction.Click += async (s, e) =>
            {
                if (btnAction.Text.Contains("Đã gửi")) return;
                btnAction.Enabled = false;
                btnAction.Text = "  Đang gửi...";
                var res = await _apiClient.SendFriendRequestAsync(new SendFriendRequestDto { ReceiverId = u.Id });
                if (res?.Success == true)
                {
                    btnAction.Text = "  Đã gửi";
                    btnAction.BackColor = Color.FromArgb(226, 232, 240);
                    btnAction.ForeColor = Color.FromArgb(51, 65, 85);
                    IconHelper.SetButtonIcon(btnAction, IconHelper.Check, 12, Color.FromArgb(51, 65, 85), ContentAlignment.MiddleLeft);
                    btnAction.Cursor = Cursors.Default;
                    btnAction.Enabled = true;
                    lblRel.Text = "Đã gửi lời mời";
                    pnlRelation.BackColor = Color.FromArgb(254, 243, 199);
                    lblRel.ForeColor = Color.FromArgb(217, 119, 6);
                    await LoadRequestsAsync();
                }
                else
                {
                    btnAction.Enabled = true;
                    btnAction.Text = "  Kết bạn";
                    IconHelper.SetButtonIcon(btnAction, IconHelper.AddUser, 12, Color.White, ContentAlignment.MiddleLeft);
                    MessageBox.Show(res?.Message ?? "Không thể gửi lời mời kết bạn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
        }

        row.Controls.Add(picAvatar);
        row.Controls.Add(lblName);
        row.Controls.Add(lblEmail);
        row.Controls.Add(pnlRelation);
        row.Controls.Add(btnAction);

        return row;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab 3: Friend Requests (Incoming & Outgoing)
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderIncomingRequests()
    {
        flpIncomingRequests.SuspendLayout();
        flpIncomingRequests.Controls.Clear();

        lblIncomingTitle.Text = $"Lời mời đã nhận ({_incomingRequests.Count})";

        if (_incomingRequests.Count == 0)
        {
            pnlIncomingEmpty.Visible = true;
            pnlIncomingEmpty.BringToFront();
        }
        else
        {
            pnlIncomingEmpty.Visible = false;

            int cardWidth = Math.Max(260, flpIncomingRequests.ClientSize.Width - 12);

            foreach (var req in _incomingRequests)
            {
                var card = CreateIncomingRequestCard(req, cardWidth);
                flpIncomingRequests.Controls.Add(card);
            }
        }

        flpIncomingRequests.ResumeLayout();
    }

    private Control CreateIncomingRequestCard(FriendRequestDto req, int cardWidth)
    {
        Panel card = new()
        {
            Width = cardWidth,
            Height = 68,
            Margin = new Padding(0, 3, 0, 3),
            BackColor = Color.White
        };
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 8, 8));

        card.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        PictureBox pic = new()
        {
            Size = new Size(36, 36),
            Location = new Point(10, 16),
            Image = GenerateAvatarBitmap(req.SenderUsername, false, false),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        _ = LoadAvatarIntoAsync(pic, req.SenderAvatarUrl);

        Label lblName = new()
        {
            Text = req.SenderUsername,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(54, 14),
            Size = new Size(card.Width - 215, 18),
            AutoEllipsis = true
        };

        Label lblTime = new()
        {
            Text = $"Gửi lúc: {VietnamTime.FromUtc(req.CreatedAt):dd/MM/yyyy HH:mm}",
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(54, 34),
            Size = new Size(card.Width - 215, 18),
            AutoEllipsis = true
        };

        Button btnAccept = new()
        {
            Text = " Đồng ý",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(37, 99, 235),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(76, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(card.Width - 156, 20),
            Cursor = Cursors.Hand
        };
        IconHelper.SetButtonIcon(btnAccept, IconHelper.Check, 12, Color.White, ContentAlignment.MiddleLeft);
        btnAccept.FlatAppearance.BorderSize = 0;
        UpdateControlRegion(btnAccept, 6);

        Button btnReject = new()
        {
            Text = " Từ chối",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            BackColor = Color.FromArgb(241, 245, 249),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(76, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(card.Width - 76, 20),
            Cursor = Cursors.Hand
        };
        IconHelper.SetButtonIcon(btnReject, IconHelper.Close, 11, Color.FromArgb(100, 116, 139), ContentAlignment.MiddleLeft);
        btnReject.FlatAppearance.BorderSize = 0;
        UpdateControlRegion(btnReject, 6);

        btnAccept.Click += async (s, e) =>
        {
            btnAccept.Enabled = false;
            btnReject.Enabled = false;
            var res = await _apiClient.AcceptFriendRequestAsync(req.Id);
            if (res?.Success == true)
            {
                await LoadAllDataAsync();
            }
            else
            {
                btnAccept.Enabled = true;
                btnReject.Enabled = true;
                MessageBox.Show(res?.Message ?? "Không thể chấp nhận lời mời.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        btnReject.Click += async (s, e) =>
        {
            btnAccept.Enabled = false;
            btnReject.Enabled = false;
            var res = await _apiClient.RejectFriendRequestAsync(req.Id);
            if (res?.Success == true)
            {
                await LoadRequestsAsync();
            }
            else
            {
                btnAccept.Enabled = true;
                btnReject.Enabled = true;
                MessageBox.Show(res?.Message ?? "Không thể từ chối lời mời.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        card.Controls.Add(pic);
        card.Controls.Add(lblName);
        card.Controls.Add(lblTime);
        card.Controls.Add(btnAccept);
        card.Controls.Add(btnReject);

        return card;
    }

    private void RenderOutgoingRequests()
    {
        flpOutgoingRequests.SuspendLayout();
        flpOutgoingRequests.Controls.Clear();

        lblOutgoingTitle.Text = $"Lời mời đã gửi ({_outgoingRequests.Count})";

        if (_outgoingRequests.Count == 0)
        {
            pnlOutgoingEmpty.Visible = true;
            pnlOutgoingEmpty.BringToFront();
        }
        else
        {
            pnlOutgoingEmpty.Visible = false;

            int cardWidth = Math.Max(260, flpOutgoingRequests.ClientSize.Width - 12);

            foreach (var req in _outgoingRequests)
            {
                var card = CreateOutgoingRequestCard(req, cardWidth);
                flpOutgoingRequests.Controls.Add(card);
            }
        }

        flpOutgoingRequests.ResumeLayout();
    }

    private Control CreateOutgoingRequestCard(FriendRequestDto req, int cardWidth)
    {
        Panel card = new()
        {
            Width = cardWidth,
            Height = 68,
            Margin = new Padding(0, 3, 0, 3),
            BackColor = Color.White
        };
        card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 8, 8));

        card.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240), 1);
            pe.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        PictureBox pic = new()
        {
            Size = new Size(36, 36),
            Location = new Point(10, 16),
            Image = GenerateAvatarBitmap(req.ReceiverUsername, false, false),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        _ = LoadAvatarIntoAsync(pic, req.ReceiverAvatarUrl);

        Label lblName = new()
        {
            Text = req.ReceiverUsername,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(54, 14),
            Size = new Size(card.Width - 145, 18),
            AutoEllipsis = true
        };

        Label lblTime = new()
        {
            Text = $"Đã gửi: {VietnamTime.FromUtc(req.CreatedAt):dd/MM/yyyy HH:mm}",
            Font = new Font("Segoe UI", 7.5F),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(54, 34),
            Size = new Size(card.Width - 145, 18),
            AutoEllipsis = true
        };

        Button btnCancel = new()
        {
            Text = " Thu hồi",
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 38, 38),
            BackColor = Color.FromArgb(254, 242, 242),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(80, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(card.Width - 90, 20),
            Cursor = Cursors.Hand
        };
        IconHelper.SetButtonIcon(btnCancel, IconHelper.Close, 11, Color.FromArgb(220, 38, 38), ContentAlignment.MiddleLeft);
        btnCancel.FlatAppearance.BorderSize = 0;
        UpdateControlRegion(btnCancel, 6);

        btnCancel.Click += async (s, e) =>
        {
            btnCancel.Enabled = false;
            var res = await _apiClient.CancelFriendRequestAsync(req.Id);
            if (res?.Success == true)
            {
                await LoadRequestsAsync();
            }
            else
            {
                btnCancel.Enabled = true;
                MessageBox.Show(res?.Message ?? "Không thể thu hồi lời mời.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        card.Controls.Add(pic);
        card.Controls.Add(lblName);
        card.Controls.Add(lblTime);
        card.Controls.Add(btnCancel);

        return card;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: Dynamic Width & Avatar
    // ─────────────────────────────────────────────────────────────────────────
    private void AdjustTableRowsWidth()
    {
        int friendsWidth = Math.Max(400, flpFriendsList.ClientSize.Width - 14);
        foreach (Control c in flpFriendsList.Controls)
        {
            if (c is Panel p)
                p.Width = friendsWidth;
        }

        int searchWidth = GetSearchResultRowWidth();
        foreach (Control c in flpSearchResults.Controls)
        {
            if (c is Panel p)
                p.Width = searchWidth;
        }
        LayoutSearchTable();

        int incWidth = Math.Max(260, flpIncomingRequests.ClientSize.Width - 12);
        foreach (Control c in flpIncomingRequests.Controls)
        {
            if (c is Panel p)
                p.Width = incWidth;
        }

        int outWidth = Math.Max(260, flpOutgoingRequests.ClientSize.Width - 12);
        foreach (Control c in flpOutgoingRequests.Controls)
        {
            if (c is Panel p)
                p.Width = outWidth;
        }
    }

    private int GetSearchResultRowWidth()
    {
        int scrollWidth = flpSearchResults.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0;
        return Math.Max(300, flpSearchResults.ClientSize.Width - flpSearchResults.Padding.Horizontal - scrollWidth - 2);
    }

    private static int GetSearchRelationLeft(int rowWidth)
    {
        int actionLeft = rowWidth - 110;
        int maxRelationLeft = Math.Max(330, actionLeft - 135);
        int minRelationLeft = Math.Min(475, maxRelationLeft);
        return Math.Clamp((int)(rowWidth * 0.66), minRelationLeft, maxRelationLeft);
    }

    private void LayoutSearchToolbar()
    {
        if (pnlSearchToolbar.ClientSize.Width <= 0) return;
        const int buttonWidth = 130;
        const int gap = 14;
        int inputWidth = Math.Max(240, pnlSearchToolbar.ClientSize.Width - buttonWidth - gap - 4);
        pnlSearchBox.SetBounds(0, 30, inputWidth, 36);
        txtSearchKeyword.Width = Math.Max(120, inputWidth - txtSearchKeyword.Left - 10);
        btnSearch.SetBounds(inputWidth + gap, 30, buttonWidth, 36);
    }

    private void LayoutSearchTable()
    {
        int rowWidth = GetSearchResultRowWidth();
        int rowOffset = flpSearchResults.Padding.Left;
        int actionLeft = rowWidth - 110;
        int relationLeft = GetSearchRelationLeft(rowWidth);
        int emailLeft = 255;
        int emailWidth = Math.Max(70, relationLeft - emailLeft - 18);
        int nameWidth = Math.Max(120, emailLeft - 70);

        lblColSearchUser.Location = new Point(rowOffset + 14, 9);
        lblColSearchEmail.Location = new Point(rowOffset + emailLeft, 9);
        lblColSearchRelation.Location = new Point(rowOffset + relationLeft + Math.Max(0, (115 - lblColSearchRelation.Width) / 2), 9);
        lblColSearchAction.Location = new Point(rowOffset + actionLeft + Math.Max(0, (95 - lblColSearchAction.Width) / 2), 9);

        foreach (Panel row in flpSearchResults.Controls.OfType<Panel>())
        {
            if (row.Controls["searchUserName"] is Label name)
                name.SetBounds(58, 17, nameWidth, 20);
            if (row.Controls["searchUserEmail"] is Label email)
                email.SetBounds(emailLeft, 18, emailWidth, 20);
            if (row.Controls["searchUserRelation"] is Panel relation)
                relation.Location = new Point(relationLeft, 14);
            if (row.Controls["searchUserAction"] is Button action)
                action.Location = new Point(actionLeft, 13);
        }
    }

    private static Bitmap GenerateAvatarBitmap(string name, bool isOnline, bool drawBadge = true)
    {
        Bitmap bmp = new(36, 36);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int hash = Math.Abs(name.GetHashCode());
        Color[] palette = [
            Color.FromArgb(37, 99, 235),
            Color.FromArgb(147, 51, 234),
            Color.FromArgb(219, 39, 119),
            Color.FromArgb(13, 148, 136),
            Color.FromArgb(234, 88, 12),
            Color.FromArgb(5, 150, 105)
        ];
        Color bg = palette[hash % palette.Length];

        using (Brush brush = new SolidBrush(bg))
        {
            g.FillEllipse(brush, 1, 1, 34, 34);
        }

        string initial = string.IsNullOrWhiteSpace(name) ? "?" : name[0].ToString().ToUpper();
        using (Font font = new("Segoe UI", 12, FontStyle.Bold, GraphicsUnit.Pixel))
        using (Brush textBrush = new SolidBrush(Color.White))
        {
            StringFormat sf = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(initial, font, textBrush, new RectangleF(1, 1, 34, 34), sf);
        }

        if (drawBadge)
        {
            Color dotColor = isOnline ? Color.FromArgb(34, 197, 94) : Color.FromArgb(148, 163, 184);
            using Brush dotBrush = new SolidBrush(dotColor);
            using Pen borderPen = new(Color.White, 2);

            g.FillEllipse(dotBrush, 24, 24, 10, 10);
            g.DrawEllipse(borderPen, 24, 24, 10, 10);
        }

        return bmp;
    }

    private async Task LoadAvatarIntoAsync(PictureBox pictureBox, string? avatarUrl)
    {
        Image? source = await AvatarImageCache.GetAsync(_session.ServerUrl, avatarUrl);
        if (source is null || IsDisposed || pictureBox.IsDisposed)
            return;

        Bitmap rendered = AvatarImageCache.CreateCircularFit(source, pictureBox.Size, Color.White, Color.FromArgb(226, 232, 240));
        if (IsDisposed || pictureBox.IsDisposed)
        {
            rendered.Dispose();
            return;
        }

        Image? old = pictureBox.Image;
        pictureBox.Image = rendered;
        old?.Dispose();
    }
}
