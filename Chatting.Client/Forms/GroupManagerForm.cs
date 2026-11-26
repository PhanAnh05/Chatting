using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using Chatting.Shared.Enums;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Forms;

public partial class GroupManagerForm : Form
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;
    private readonly Guid? _groupId;
    private GroupDto? _existingGroup;

    public Guid? CreatedOrModifiedConversationId { get; private set; }

    private List<FriendDto> _friendsList = [];
    private readonly HashSet<Guid> _selectedFriendIds = [];
    private string _activeEditTab = "members"; // "members" or "add_friends"
    private bool _isGroupNameFocused = false;
    private readonly CheckBox _allowMemberAdd = new()
    {
        Text = "Cho phép thành viên thêm người",
        AutoSize = true,
        Anchor = AnchorStyles.Top | AnchorStyles.Left,
        Location = new Point(96, 45),
        ForeColor = Color.FromArgb(51, 65, 85),
        Font = new Font("Segoe UI", 9F),
        Visible = false
    };

    public GroupManagerForm(ApiClient apiClient, SessionManager session, Guid? groupId = null)
    {
        _apiClient = apiClient;
        _session = session;
        _groupId = groupId;

        InitializeComponent();
        pnlGroupNameRow.Height = 72;
        pnlGroupNameRow.Controls.Add(_allowMemberAdd);
        ApplyCustomStyling();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI Styling & GDI+ Setup
    // ─────────────────────────────────────────────────────────────────────────
    private void ApplyCustomStyling()
    {
        // 1. Robot mascot image loading
        Image? mascotImg = LoadRobotMascot();
        if (mascotImg != null)
        {
            picRobotMascot.Image = mascotImg;
        }
        else
        {
            picRobotMascot.Image = AiAvatarHelper.GetRobotBitmap(58, 58);
        }

        // 2. Custom header gradient painting
        pnlHeader.Paint += (s, pe) =>
        {
            using LinearGradientBrush lgb = new(
                new Point(0, 0),
                new Point(pnlHeader.Width, 0),
                Color.FromArgb(29, 78, 216),  // #1D4ED8
                Color.FromArgb(37, 99, 235)   // #2563EB
            );
            pe.Graphics.FillRectangle(lgb, 0, 0, pnlHeader.Width, pnlHeader.Height);

            // Subtle decorative circles watermark on the right
            using var waterBrush = new SolidBrush(Color.FromArgb(30, 255, 255, 255));
            pe.Graphics.FillEllipse(waterBrush, pnlHeader.Width - 130, -35, 160, 160);
            pe.Graphics.FillEllipse(waterBrush, pnlHeader.Width - 70, 25, 95, 95);
        };

        // 3. Card border and rounded corners
        pnlMainCard.Paint += (s, pe) =>
        {
            using var pen = new Pen(_session.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240), 1.2f);
            DrawRoundedRectangle(pe.Graphics, pen, new Rectangle(0, 0, pnlMainCard.Width - 1, pnlMainCard.Height - 1), 12);
        };

        // 4. Group name box border & focus effects
        txtGroupName.GotFocus += (s, e) =>
        {
            _isGroupNameFocused = true;
            pnlGroupNameBox.Invalidate();
        };
        txtGroupName.LostFocus += (s, e) =>
        {
            _isGroupNameFocused = false;
            pnlGroupNameBox.Invalidate();
        };
        pnlGroupNameBox.Paint += (s, pe) =>
        {
            Color borderColor = _isGroupNameFocused ? Color.FromArgb(37, 99, 235) : (_session.IsDarkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(147, 197, 253));
            using var pen = new Pen(borderColor, _isGroupNameFocused ? 1.8f : 1.2f);
            DrawRoundedRectangle(pe.Graphics, pen, new Rectangle(0, 0, pnlGroupNameBox.Width - 1, pnlGroupNameBox.Height - 1), 8);
        };

        // 5. Border container for friends list
        pnlListBorderContainer.Paint += (s, pe) =>
        {
            using var pen = new Pen(_session.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225), 1.2f);
            DrawRoundedRectangle(pe.Graphics, pen, new Rectangle(0, 0, pnlListBorderContainer.Width - 1, pnlListBorderContainer.Height - 1), 8);
        };

        // 6. Close button border paint
        btnClose.Paint += (s, pe) =>
        {
            using var pen = new Pen(Color.FromArgb(148, 163, 184), 1.2f);
            DrawRoundedRectangle(pe.Graphics, pen, new Rectangle(0, 0, btnClose.Width - 1, btnClose.Height - 1), 8);
        };

        // 7. Hover effects for buttons
        btnSubmit.MouseEnter += (s, e) => btnSubmit.BackColor = Color.FromArgb(37, 99, 235);
        btnSubmit.MouseLeave += (s, e) => btnSubmit.BackColor = Color.FromArgb(29, 78, 216);

        btnClose.MouseEnter += (s, e) => btnClose.BackColor = _session.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(248, 250, 252);
        btnClose.MouseLeave += (s, e) => btnClose.BackColor = _session.IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.White;

        btnLeaveGroup.MouseEnter += (s, e) => btnLeaveGroup.BackColor = _session.IsDarkMode ? Color.FromArgb(127, 29, 29) : Color.FromArgb(254, 226, 226);
        btnLeaveGroup.MouseLeave += (s, e) => btnLeaveGroup.BackColor = _session.IsDarkMode ? Color.FromArgb(69, 26, 26) : Color.FromArgb(254, 242, 242);

        btnRemoveMember.MouseEnter += (s, e) => btnRemoveMember.BackColor = _session.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
        btnRemoveMember.MouseLeave += (s, e) => btnRemoveMember.BackColor = _session.IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);

        // 8. Navigation tab clicks for Edit mode
        btnTabMembers.Click += (s, e) => SwitchEditTab("members");
        btnTabAddFriends.Click += (s, e) => SwitchEditTab("add_friends");

        // 9. Resize & Layout events
        Resize += (s, e) =>
        {
            UpdateAllRegions();
            AdjustRowWidths();
            UpdateTabIndicatorPosition();
        };

        pnlTabs.Resize += (s, e) => UpdateTabIndicatorPosition();

        ApplyTheme();

        UpdateAllRegions();
    }

    private void ApplyTheme()
    {
        bool dark = _session.IsDarkMode;
        Color page = dark ? Color.FromArgb(11, 18, 32) : Color.FromArgb(244, 247, 254);
        Color surface = dark ? Color.FromArgb(15, 23, 42) : Color.White;
        Color raised = dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(248, 250, 252);
        Color text = dark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);
        Color muted = dark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

        BackColor = page;
        pnlBodyContainer.BackColor = page;
        pnlMainCard.BackColor = surface;
        pnlMiddleHeader.BackColor = surface;
        pnlGroupNameRow.BackColor = surface;
        pnlGroupNameBox.BackColor = surface;
        pnlTabs.BackColor = surface;
        pnlListBorderContainer.BackColor = surface;
        pnlFooter.BackColor = surface;
        flpFriendsList.BackColor = surface;
        flpMembersList.BackColor = surface;
        pnlFriendsEmpty.BackColor = raised;
        txtGroupName.BackColor = dark ? raised : Color.White;
        txtGroupName.ForeColor = text;
        _allowMemberAdd.ForeColor = text;
        _allowMemberAdd.BackColor = surface;
        lblGroupName.ForeColor = muted;
        lblChooseFriends.ForeColor = text;
        lblFriendsEmptyTitle.ForeColor = text;
        lblFriendsEmptySub.ForeColor = muted;
        btnTabMembers.BackColor = surface;
        btnTabAddFriends.BackColor = surface;
        btnTabMembers.ForeColor = muted;
        btnTabAddFriends.ForeColor = muted;
        btnClose.BackColor = dark ? raised : Color.White;
        btnClose.ForeColor = text;
        btnRemoveMember.BackColor = dark ? raised : Color.FromArgb(241, 245, 249);
        btnRemoveMember.ForeColor = text;
        btnLeaveGroup.BackColor = dark ? Color.FromArgb(69, 26, 26) : Color.FromArgb(254, 242, 242);
        btnLeaveGroup.ForeColor = dark ? Color.FromArgb(254, 202, 202) : Color.FromArgb(185, 28, 28);
        pnlMainCard.Invalidate();
        pnlListBorderContainer.Invalidate();
        pnlGroupNameBox.Invalidate();
    }

    private void UpdateAllRegions()
    {
        UpdateControlRegion(pnlMainCard, 12);
        UpdateControlRegion(pnlRobotContainer, 10);
        UpdateControlRegion(pnlGroupNameBox, 8);
        UpdateControlRegion(pnlListBorderContainer, 8);
        UpdateControlRegion(btnSubmit, 8);
        UpdateControlRegion(btnClose, 8);
        UpdateControlRegion(btnLeaveGroup, 8);
        UpdateControlRegion(btnRemoveMember, 8);
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

    private static void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
    {
        using GraphicsPath path = CreateRoundedRectanglePath(rect, radius);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.DrawPath(pen, path);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lifecycle & Data Loading
    // ─────────────────────────────────────────────────────────────────────────
    private async void GroupManagerForm_Load(object? sender, EventArgs e)
    {
        UpdateAllRegions();

        IconHelper.SetButtonIcon(btnLeaveGroup, IconHelper.Close, 14, Color.FromArgb(220, 38, 38), ContentAlignment.MiddleLeft);
        IconHelper.SetButtonIcon(btnRemoveMember, IconHelper.Delete, 14, Color.FromArgb(71, 85, 105), ContentAlignment.MiddleLeft);

        bool isEditMode = _groupId.HasValue && _groupId.Value != Guid.Empty;

        if (isEditMode)
        {
            Text = "Cài Đặt & Quản Lý Nhóm - Chatting";
            lblHeaderTitle.Text = "CÀI ĐẶT & THÀNH VIÊN NHÓM";
            lblHeaderSubtitle.Text = "Cập nhật tên nhóm, thêm bạn bè hoặc phân quyền thành viên hội thoại";
            btnSubmit.Text = "  Lưu Thay Đổi";
            IconHelper.SetButtonIcon(btnSubmit, IconHelper.Check, 14, Color.White, ContentAlignment.MiddleLeft);
            btnLeaveGroup.Visible = true;
            pnlTabs.Visible = true;
            lblChooseFriends.Visible = false;

            await LoadGroupDetailsAsync(_groupId!.Value);
            await LoadFriendsAsync();
            SwitchEditTab("members");
        }
        else
        {
            Text = "Tạo & Quản lý Nhóm Chat - Chatting";
            lblHeaderTitle.Text = "QUẢN LÝ NHÓM CHAT";
            lblHeaderSubtitle.Text = "Tạo nhóm trò chuyện mới hoặc quản lý, phân quyền thành viên hội thoại";
            btnSubmit.Text = "  Tạo Nhóm Mới";
            IconHelper.SetButtonIcon(btnSubmit, IconHelper.People, 14, Color.White, ContentAlignment.MiddleLeft);
            btnLeaveGroup.Visible = false;
            btnRemoveMember.Visible = false;
            pnlTabs.Visible = false;
            _allowMemberAdd.Visible = true;
            lblChooseFriends.Visible = true;
            lblChooseFriends.Text = "Chọn bạn bè thêm vào nhóm:";
            flpFriendsList.Visible = true;
            flpFriendsList.BringToFront();

            await LoadFriendsAsync();
        }

        AdjustRowWidths();
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

        // Synchronize legacy control
        chkListFriends.Items.Clear();
        foreach (var f in _friendsList)
        {
            chkListFriends.Items.Add(new FriendItem(f.UserId, f.Username));
        }

        RenderFriendsList();
    }

    private async Task LoadGroupDetailsAsync(Guid groupId)
    {
        var res = await _apiClient.GetGroupAsync(groupId);
        if (res?.Success == true && res.Data != null)
        {
            _existingGroup = res.Data;
            txtGroupName.Text = _existingGroup.Name;
            _allowMemberAdd.Checked = _existingGroup.AllowMembersToAdd;
            bool isLeader = _existingGroup.Members.Any(m => m.UserId == _session.CurrentUser?.Id && m.Role == GroupRole.Leader);
            _allowMemberAdd.Visible = isLeader;
            txtGroupName.ReadOnly = !isLeader;
            btnTabAddFriends.Visible = isLeader || _existingGroup.AllowMembersToAdd;
            if (!btnTabAddFriends.Visible) SwitchEditTab("members");

            // Synchronize legacy control
            lstMembers.Items.Clear();
            foreach (var m in _existingGroup.Members)
            {
                var item = new ListViewItem(m.Username);
                string role = m.Role == GroupRole.Leader ? "Trưởng nhóm" : "Thành viên";
                item.SubItems.Add(role);
                item.SubItems.Add(m.IsOnline ? "Online" : "Offline");
                item.Tag = m;
                lstMembers.Items.Add(item);
            }

            RenderMembersList();
            RenderFriendsList(); // Re-render to exclude or mark current members

            int count = _existingGroup.Members.Count;
            btnTabMembers.Text = $"  Thành viên ({count})";
            IconHelper.SetButtonIcon(btnTabMembers, IconHelper.People, 14, Color.FromArgb(37, 99, 235), ContentAlignment.MiddleLeft);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Render Friends List (Custom Checked List)
    // ─────────────────────────────────────────────────────────────────────────
    private static void ClearRows(FlowLayoutPanel list)
    {
        foreach (Control row in list.Controls.Cast<Control>().ToArray())
            row.Dispose();
        list.Controls.Clear();
    }

    private async Task LoadRowAvatarAsync(PictureBox avatar, string? avatarUrl, string username, bool isOnline)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl)) return;

        try
        {
            // This method starts on the UI thread. Awaiting the shared cache returns
            // to that thread, including when the PictureBox has no handle yet.
            Image? image = await AvatarImageCache.GetAsync(_session.ServerUrl, avatarUrl);
            if (image is null || avatar.IsDisposed || IsDisposed) return;

            Image rendered = GenerateAvatarBitmap(username, isOnline, image);
            Image? old = avatar.Image;
            avatar.Image = rendered;
            old?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GroupManagerForm] Avatar error: {ex.Message}");
        }
    }

    private void RenderFriendsList()
    {
        flpFriendsList.SuspendLayout();
        ClearRows(flpFriendsList);

        // In edit mode, exclude friends who are already members
        var availableFriends = _friendsList.AsEnumerable();
        if (_existingGroup != null && _existingGroup.Members.Count > 0)
        {
            var existingMemberIds = _existingGroup.Members.Select(m => m.UserId).ToHashSet();
            availableFriends = availableFriends.Where(f => !existingMemberIds.Contains(f.UserId));
        }

        var friendsToDisplay = availableFriends.OrderByDescending(f => f.IsOnline).ThenBy(f => f.Username).ToList();

        if (friendsToDisplay.Count == 0)
        {
            pnlFriendsEmpty.Visible = true;
            pnlFriendsEmpty.BringToFront();

            if (_existingGroup != null)
            {
                lblFriendsEmptyTitle.Text = "Tất cả bạn bè đã ở trong nhóm";
                lblFriendsEmptySub.Text = "Hiện không còn bạn bè nào khác để thêm vào nhóm này.";
            }
            else
            {
                lblFriendsEmptyTitle.Text = "Chưa có bạn bè nào";
                lblFriendsEmptySub.Text = "Hãy kết bạn thêm trong mạng LAN để mời vào nhóm trò chuyện.";
            }
        }
        else
        {
            pnlFriendsEmpty.Visible = false;
            int rowWidth = Math.Max(350, flpFriendsList.ClientSize.Width - (flpFriendsList.VerticalScroll.Visible ? 6 : 2));

            foreach (var f in friendsToDisplay)
            {
                var row = CreateFriendRowItem(f, rowWidth);
                flpFriendsList.Controls.Add(row);
            }
        }

        flpFriendsList.ResumeLayout();
    }

    private Control CreateFriendRowItem(FriendDto f, int rowWidth)
    {
        bool dark = _session.IsDarkMode;
        Color rowColor = dark ? Color.FromArgb(15, 23, 42) : Color.White;
        Panel row = new()
        {
            Width = rowWidth,
            Height = 54,
            Margin = new Padding(0, 1, 0, 1),
            BackColor = rowColor,
            Cursor = Cursors.Hand
        };

        bool isChecked = _selectedFriendIds.Contains(f.UserId);

        // Subtle bottom separator line
        row.Paint += (s, pe) =>
        {
            using var pen = new Pen(dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249), 1);
            pe.Graphics.DrawLine(pen, 14, row.Height - 1, row.Width - 14, row.Height - 1);
        };

        // Smooth hover effect
        row.MouseEnter += (s, e) => row.BackColor = dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(248, 250, 252);
        row.MouseLeave += (s, e) =>
        {
            if (!row.ClientRectangle.Contains(row.PointToClient(Cursor.Position)))
                row.BackColor = rowColor;
        };

        // 1. Avatar (38x38) with status dot
        PictureBox picAvatar = new()
        {
            Size = new Size(38, 38),
            Location = new Point(14, 8),
            Image = GenerateAvatarBitmap(f.Username, f.IsOnline),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        picAvatar.Disposed += (_, _) => picAvatar.Image?.Dispose();
        _ = LoadRowAvatarAsync(picAvatar, f.AvatarUrl, f.Username, f.IsOnline);

        // 2. Name
        Label lblName = new()
        {
            Text = f.Username,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            ForeColor = dark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42),
            Location = new Point(62, 17),
            Size = new Size(180, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        // 3. Email
        string displayEmail = !string.IsNullOrWhiteSpace(f.Email) ? f.Email : $"@{f.Username.ToLowerInvariant()}";
        Label lblEmail = new()
        {
            Text = displayEmail,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = dark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139),
            Location = new Point(255, 18),
            Size = new Size(200, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        // 4. Online/Offline Status
        Label lblStatus = new()
        {
            Text = f.IsOnline ? "●  Trực tuyến" : "●  Ngoại tuyến",
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = f.IsOnline ? Color.FromArgb(22, 163, 74) : Color.FromArgb(148, 163, 184),
            Location = new Point(row.Width - 195, 17),
            Size = new Size(130, 20),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        // 5. Circular Checkbox button (22x22)
        PictureBox picCheckbox = new()
        {
            Size = new Size(22, 22),
            Location = new Point(row.Width - 46, 16),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Image = GenerateCheckboxBitmap(isChecked),
            SizeMode = PictureBoxSizeMode.CenterImage,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        // Toggle selection logic
        Action toggleChecked = () =>
        {
            bool nowChecked;
            if (_selectedFriendIds.Contains(f.UserId))
            {
                _selectedFriendIds.Remove(f.UserId);
                nowChecked = false;
            }
            else
            {
                _selectedFriendIds.Add(f.UserId);
                nowChecked = true;
            }

            Image? oldCheckbox = picCheckbox.Image;
            picCheckbox.Image = GenerateCheckboxBitmap(nowChecked);
            oldCheckbox?.Dispose();
            SyncLegacyCheckedListBox();
        };
        picCheckbox.Disposed += (_, _) => picCheckbox.Image?.Dispose();

        // Click on any element toggles the checkbox
        row.Click += (s, e) => toggleChecked();
        picAvatar.Click += (s, e) => toggleChecked();
        lblName.Click += (s, e) => toggleChecked();
        lblEmail.Click += (s, e) => toggleChecked();
        lblStatus.Click += (s, e) => toggleChecked();
        picCheckbox.Click += (s, e) => toggleChecked();

        row.Controls.Add(picAvatar);
        row.Controls.Add(lblName);
        row.Controls.Add(lblEmail);
        row.Controls.Add(lblStatus);
        row.Controls.Add(picCheckbox);

        return row;
    }

    private void SyncLegacyCheckedListBox()
    {
        for (int i = 0; i < chkListFriends.Items.Count; i++)
        {
            if (chkListFriends.Items[i] is FriendItem fi)
            {
                bool isChecked = _selectedFriendIds.Contains(fi.UserId);
                chkListFriends.SetItemChecked(i, isChecked);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Render Members List (Edit Mode)
    // ─────────────────────────────────────────────────────────────────────────
    private void RenderMembersList()
    {
        if (_existingGroup == null) return;

        flpMembersList.SuspendLayout();
        ClearRows(flpMembersList);

        int rowWidth = Math.Max(350, flpMembersList.ClientSize.Width - (flpMembersList.VerticalScroll.Visible ? 6 : 2));
        var myUserId = _session.CurrentUser?.Id;
        bool isLeader = _existingGroup.Members.Any(m => m.UserId == myUserId && m.Role == GroupRole.Leader);

        // Sort: Leader first, then Online, then Username
        var sorted = _existingGroup.Members
            .OrderByDescending(m => m.Role == GroupRole.Leader)
            .ThenByDescending(m => m.IsOnline)
            .ThenBy(m => m.Username)
            .ToList();

        foreach (var m in sorted)
        {
            var row = CreateMemberRowItem(m, rowWidth, isLeader, myUserId);
            flpMembersList.Controls.Add(row);
        }

        flpMembersList.ResumeLayout();
    }

    private Control CreateMemberRowItem(GroupMemberDto m, int rowWidth, bool isCurrentUserLeader, Guid? myUserId)
    {
        bool dark = _session.IsDarkMode;
        Color rowColor = dark ? Color.FromArgb(15, 23, 42) : Color.White;
        Panel row = new()
        {
            Width = rowWidth,
            Height = 54,
            Margin = new Padding(0, 1, 0, 1),
            BackColor = rowColor
        };

        // Separator
        row.Paint += (s, pe) =>
        {
            using var pen = new Pen(dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249), 1);
            pe.Graphics.DrawLine(pen, 14, row.Height - 1, row.Width - 14, row.Height - 1);
        };

        // Hover
        row.MouseEnter += (s, e) => row.BackColor = dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(248, 250, 252);
        row.MouseLeave += (s, e) =>
        {
            if (!row.ClientRectangle.Contains(row.PointToClient(Cursor.Position)))
                row.BackColor = rowColor;
        };

        // Avatar
        PictureBox picAvatar = new()
        {
            Size = new Size(38, 38),
            Location = new Point(14, 8),
            Image = GenerateAvatarBitmap(m.Username, m.IsOnline),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        picAvatar.Disposed += (_, _) => picAvatar.Image?.Dispose();
        _ = LoadRowAvatarAsync(picAvatar, m.AvatarUrl, m.Username, m.IsOnline);

        // Name
        Label lblName = new()
        {
            Text = m.Username,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
            ForeColor = dark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42),
            Location = new Point(62, 17),
            Size = new Size(170, 20),
            AutoEllipsis = true,
            BackColor = Color.Transparent
        };

        // Role Pill
        Panel pnlRole = new()
        {
            Size = new Size(115, 26),
            Location = new Point(245, 14),
            BackColor = m.Role == GroupRole.Leader ? (dark ? Color.FromArgb(69, 52, 23) : Color.FromArgb(254, 243, 199)) : (dark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249))
        };
        UpdateControlRegion(pnlRole, 13);

        Label lblRole = new()
        {
            Text = m.Role == GroupRole.Leader ? "Trưởng nhóm" : "Thành viên",
            Font = new Font("Segoe UI Semibold", 8.25F, FontStyle.Bold),
            ForeColor = m.Role == GroupRole.Leader ? Color.FromArgb(217, 119, 6) : (dark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139)),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };
        pnlRole.Controls.Add(lblRole);

        // Online Status
        Label lblStatus = new()
        {
            Text = m.IsOnline ? "●  Trực tuyến" : "●  Ngoại tuyến",
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            ForeColor = m.IsOnline ? Color.FromArgb(22, 163, 74) : Color.FromArgb(148, 163, 184),
            Location = new Point(375, 17),
            Size = new Size(110, 20),
            BackColor = Color.Transparent
        };

        // Actions on the right
        if (m.UserId == myUserId)
        {
            Label lblYou = new()
            {
                Text = "(Bạn)",
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(row.Width - 90, 17),
                Size = new Size(60, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            row.Controls.Add(lblYou);
        }
        else if (isCurrentUserLeader)
        {
            // Leader actions: Role switch & Remove
            Button btnToggleRole = new()
            {
                Text = m.Role == GroupRole.Leader ? " Hạ quyền" : " Phong làm trưởng nhóm",
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                BackColor = Color.FromArgb(239, 246, 255),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(m.Role == GroupRole.Leader ? 90 : 175, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(row.Width - (m.Role == GroupRole.Leader ? 165 : 250), 13),
                Cursor = Cursors.Hand
            };
            IconHelper.SetButtonIcon(btnToggleRole, IconHelper.Star, 12, Color.FromArgb(37, 99, 235), ContentAlignment.MiddleLeft);
            btnToggleRole.FlatAppearance.BorderSize = 0;
            UpdateControlRegion(btnToggleRole, 6);
            btnToggleRole.Click += async (s, e) => await ToggleMemberRoleAsync(m);

            Button btnRemove = new()
            {
                Text = " Xóa",
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                BackColor = Color.FromArgb(254, 242, 242),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(68, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(row.Width - 70, 13),
                Cursor = Cursors.Hand
            };
            IconHelper.SetButtonIcon(btnRemove, IconHelper.Delete, 12, Color.FromArgb(220, 38, 38), ContentAlignment.MiddleLeft);
            btnRemove.FlatAppearance.BorderSize = 0;
            UpdateControlRegion(btnRemove, 6);
            btnRemove.Click += async (s, e) => await RemoveMemberAsync(m);

            row.Controls.Add(btnToggleRole);
            row.Controls.Add(btnRemove);
        }

        row.Controls.Add(picAvatar);
        row.Controls.Add(lblName);
        row.Controls.Add(pnlRole);
        row.Controls.Add(lblStatus);

        return row;
    }

    private async Task ToggleMemberRoleAsync(GroupMemberDto member)
    {
        if (!_groupId.HasValue) return;

        var newRole = member.Role == GroupRole.Leader ? GroupRole.Member : GroupRole.Leader;
        string roleName = newRole == GroupRole.Leader ? "Trưởng nhóm" : "Thành viên";

        var ask = MessageBox.Show(
            $"Bạn có muốn đổi vai trò của '{member.Username}' thành {roleName}?",
            "Xác nhận đổi vai trò",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (ask == DialogResult.Yes)
        {
            var res = await _apiClient.UpdateMemberRoleAsync(_groupId.Value, new UpdateMemberRoleDto
            {
                UserId = member.UserId,
                Role = newRole
            });

            if (res?.Success == true)
            {
                await LoadGroupDetailsAsync(_groupId.Value);
            }
            else
            {
                MessageBox.Show(res?.Message ?? "Không thể cập nhật vai trò.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task RemoveMemberAsync(GroupMemberDto member)
    {
        if (!_groupId.HasValue) return;

        var ask = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa thành viên '{member.Username}' khỏi nhóm?",
            "Xác nhận xóa thành viên",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (ask == DialogResult.Yes)
        {
            var res = await _apiClient.RemoveGroupMemberAsync(_groupId.Value, member.UserId);
            if (res?.Success == true)
            {
                await LoadGroupDetailsAsync(_groupId.Value);
                await LoadFriendsAsync();
            }
            else
            {
                MessageBox.Show(res?.Message ?? "Không thể xóa thành viên khỏi nhóm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tab Switching (Edit Mode)
    // ─────────────────────────────────────────────────────────────────────────
    private void SwitchEditTab(string tabKey)
    {
        _activeEditTab = tabKey;

        Color membersColor = tabKey == "members" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
        btnTabMembers.ForeColor = membersColor;
        btnTabMembers.Font = new Font("Segoe UI Semibold", 9.5F, tabKey == "members" ? FontStyle.Bold : FontStyle.Regular);
        IconHelper.SetButtonIcon(btnTabMembers, IconHelper.People, 14, membersColor, ContentAlignment.MiddleLeft);

        Color addFriendsColor = tabKey == "add_friends" ? Color.FromArgb(37, 99, 235) : Color.FromArgb(100, 116, 139);
        btnTabAddFriends.ForeColor = addFriendsColor;
        btnTabAddFriends.Font = new Font("Segoe UI Semibold", 9.5F, tabKey == "add_friends" ? FontStyle.Bold : FontStyle.Regular);
        IconHelper.SetButtonIcon(btnTabAddFriends, IconHelper.AddUser, 14, addFriendsColor, ContentAlignment.MiddleLeft);

        UpdateTabIndicatorPosition();

        if (tabKey == "members")
        {
            flpMembersList.Visible = true;
            flpMembersList.BringToFront();
            flpFriendsList.Visible = false;
            pnlFriendsEmpty.Visible = false;
        }
        else
        {
            flpFriendsList.Visible = true;
            flpFriendsList.BringToFront();
            flpMembersList.Visible = false;
            if (flpFriendsList.Controls.Count == 0)
            {
                pnlFriendsEmpty.Visible = true;
                pnlFriendsEmpty.BringToFront();
            }
        }

        AdjustRowWidths();
    }

    private void UpdateTabIndicatorPosition()
    {
        if (!pnlTabs.Visible) return;

        Button? target = _activeEditTab switch
        {
            "members" => btnTabMembers,
            "add_friends" => btnTabAddFriends,
            _ => btnTabMembers
        };

        if (target != null && target.Width > 0)
        {
            pnlTabIndicator.SetBounds(target.Left + 10, pnlTabs.Height - 3, Math.Max(40, target.Width - 20), 3);
            pnlTabIndicator.BringToFront();
        }
    }

    private void AdjustRowWidths()
    {
        int friendsWidth = Math.Max(350, flpFriendsList.ClientSize.Width - (flpFriendsList.VerticalScroll.Visible ? 6 : 2));
        foreach (Control c in flpFriendsList.Controls)
        {
            if (c is Panel p)
                p.Width = friendsWidth;
        }

        int membersWidth = Math.Max(350, flpMembersList.ClientSize.Width - (flpMembersList.VerticalScroll.Visible ? 6 : 2));
        foreach (Control c in flpMembersList.Controls)
        {
            if (c is Panel p)
                p.Width = membersWidth;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Submit Actions (Create / Update Group)
    // ─────────────────────────────────────────────────────────────────────────
    private async void btnSubmit_Click(object? sender, EventArgs e)
    {
        string name = txtGroupName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Vui lòng nhập tên nhóm trò chuyện!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtGroupName.Focus();
            return;
        }

        btnSubmit.Enabled = false;

        try
        {
            if (_groupId.HasValue && _groupId.Value != Guid.Empty)
            {
                if (_existingGroup != null)
                {
                    bool isLeader = _existingGroup.Members.Any(m => m.UserId == _session.CurrentUser?.Id && m.Role == GroupRole.Leader);
                    UpdateGroupDto update = new();
                    if (isLeader && _existingGroup.Name != name) update.Name = name;
                    if (isLeader && _existingGroup.AllowMembersToAdd != _allowMemberAdd.Checked)
                        update.AllowMembersToAdd = _allowMemberAdd.Checked;

                    if (update.Name is not null || update.AllowMembersToAdd.HasValue)
                    {
                        var updateResult = await _apiClient.UpdateGroupAsync(_groupId.Value, update);
                        if (updateResult?.Success != true)
                        {
                            MessageBox.Show(updateResult?.Message ?? "Không thể cập nhật cài đặt nhóm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                // Add newly selected members
                if (_selectedFriendIds.Count > 0)
                {
                    var addResult = await _apiClient.AddGroupMembersAsync(_groupId.Value, new AddGroupMembersDto
                    {
                        UserIds = _selectedFriendIds.ToList()
                    });
                    if (addResult?.Success != true)
                    {
                        MessageBox.Show(addResult?.Message ?? "Không thể thêm thành viên vào nhóm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                CreatedOrModifiedConversationId = _existingGroup?.ConversationId;
                MessageBox.Show("Đã cập nhật thông tin nhóm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                // Create new group
                var res = await _apiClient.CreateGroupAsync(new CreateGroupDto
                {
                    Name = name,
                    MemberUserIds = _selectedFriendIds.ToList(),
                    AllowMembersToAdd = _allowMemberAdd.Checked
                });

                if (res?.Success == true && res.Data != null)
                {
                    CreatedOrModifiedConversationId = res.Data.ConversationId;
                    MessageBox.Show($"Tạo nhóm '{name}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show(res?.Message ?? "Không thể tạo nhóm lúc này.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSubmit.Enabled = true;
        }
    }

    private async void btnRemoveMember_Click(object? sender, EventArgs e)
    {
        if (!_groupId.HasValue || lstMembers.SelectedItems.Count == 0) return;
        var member = lstMembers.SelectedItems[0].Tag as GroupMemberDto;
        if (member == null) return;

        if (member.UserId == _session.CurrentUser?.Id)
        {
            MessageBox.Show("Để rời nhóm, vui lòng bấm nút 'Rời khỏi nhóm'!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        await RemoveMemberAsync(member);
    }

    private async void btnLeaveGroup_Click(object? sender, EventArgs e)
    {
        if (!_groupId.HasValue || _groupId.Value == Guid.Empty) return;

        var ask = MessageBox.Show(
            "Bạn có chắc chắn muốn rời khỏi nhóm trò chuyện này?",
            "Xác nhận rời nhóm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (ask == DialogResult.Yes)
        {
            btnLeaveGroup.Enabled = false;
            try
            {
                var res = await _apiClient.LeaveGroupAsync(_groupId.Value);
                if (res?.Success == true)
                {
                    MessageBox.Show("Bạn đã rời nhóm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show(res?.Message ?? "Không thể rời nhóm lúc này.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnLeaveGroup.Enabled = true;
            }
        }
    }

    private void btnClose_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GDI+ Checkbox & Avatar Graphics Generators
    // ─────────────────────────────────────────────────────────────────────────
    private static Bitmap GenerateCheckboxBitmap(bool isChecked)
    {
        Bitmap bmp = new(22, 22);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (isChecked)
        {
            // Filled blue circle #2563EB
            using var brush = new SolidBrush(Color.FromArgb(37, 99, 235));
            g.FillEllipse(brush, 0, 0, 21, 21);

            // Crisp white checkmark ✓
            using var pen = new Pen(Color.White, 2.2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            PointF p1 = new(5.8f, 10.8f);
            PointF p2 = new(9.2f, 14.5f);
            PointF p3 = new(15.5f, 7.5f);
            g.DrawLines(pen, [p1, p2, p3]);
        }
        else
        {
            // Unchecked: white background, #CBD5E1 border
            using var bgBrush = new SolidBrush(Color.White);
            g.FillEllipse(bgBrush, 1, 1, 19, 19);

            using var pen = new Pen(Color.FromArgb(203, 213, 225), 1.8f);
            g.DrawEllipse(pen, 1, 1, 19, 19);
        }

        return bmp;
    }

    private static Bitmap GenerateAvatarBitmap(string name, bool isOnline, Image? customImage = null)
    {
        int size = 38;
        Bitmap bmp = new(size, size);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (customImage != null)
        {
            using GraphicsPath clip = new();
            clip.AddEllipse(1, 1, size - 2, size - 2);
            g.SetClip(clip);
            g.DrawImage(customImage, 1, 1, size - 2, size - 2);
            g.ResetClip();
        }
        else
        {
            int hash = Math.Abs(name.GetHashCode());
            Color[] palette = [
                Color.FromArgb(37, 99, 235),   // #2563EB Blue
                Color.FromArgb(13, 148, 136),  // #0D9488 Teal
                Color.FromArgb(124, 58, 237),  // #7C3AED Purple
                Color.FromArgb(219, 39, 119),  // #DB2777 Pink
                Color.FromArgb(234, 88, 12),   // #EA580C Orange
                Color.FromArgb(5, 150, 105)    // #059669 Emerald
            ];
            Color bg = palette[hash % palette.Length];

            using (Brush brush = new SolidBrush(bg))
            {
                g.FillEllipse(brush, 1, 1, size - 2, size - 2);
            }

            string initial = string.IsNullOrWhiteSpace(name) ? "?" : name[0].ToString().ToUpper();
            using Font font = new("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
            using Brush textBrush = new SolidBrush(Color.White);
            StringFormat sf = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(initial, font, textBrush, new RectangleF(1, 1, size - 2, size - 2), sf);
        }

        // Status dot at bottom right (x=26, y=26, size=10x10)
        Color dotColor = isOnline ? Color.FromArgb(22, 163, 74) : Color.FromArgb(148, 163, 184);
        using (Brush dotBrush = new SolidBrush(dotColor))
        using (Pen borderPen = new(Color.White, 2f))
        {
            g.FillEllipse(dotBrush, 26, 26, 10, 10);
            g.DrawEllipse(borderPen, 26, 26, 10, 10);
        }

        return bmp;
    }

    private record FriendItem(Guid UserId, string Username)
    {
        public override string ToString() => Username;
    }
}
