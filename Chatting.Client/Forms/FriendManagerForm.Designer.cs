namespace Chatting.Client.Forms;

partial class FriendManagerForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        picRobotMascot = new PictureBox();
        lblHeaderTitle = new Label();
        lblHeaderSubtitle = new Label();
        pnlBodyContainer = new Panel();
        pnlCardWrapper = new Panel();
        pnlMainCard = new Panel();
        pnlTabPages = new Panel();
        pnlTabFriendsPage = new Panel();
        flpFriendsList = new FlowLayoutPanel();
        pnlFriendsEmpty = new Panel();
        lblFriendsEmptyIcon = new Label();
        lblFriendsEmptyTitle = new Label();
        lblFriendsEmptySub = new Label();
        pnlFriendsTableHeader = new Panel();
        lblColFriendName = new Label();
        lblColFriendEmail = new Label();
        lblColFriendStatus = new Label();
        lblColFriendAction = new Label();
        pnlFriendsToolbar = new Panel();
        pnlFriendSearchBox = new Panel();
        lblFriendSearchIcon = new Label();
        txtFriendSearch = new TextBox();
        cboFriendFilter = new ComboBox();
        btnAddFriendQuick = new Button();
        pnlFriendsFooter = new Panel();
        lblFriendsCount = new Label();
        pnlTabSearchPage = new Panel();
        flpSearchResults = new FlowLayoutPanel();
        pnlSearchEmpty = new Panel();
        lblSearchEmptyIcon = new Label();
        lblSearchEmptyTitle = new Label();
        lblSearchEmptySub = new Label();
        pnlSearchTableHeader = new Panel();
        lblColSearchUser = new Label();
        lblColSearchEmail = new Label();
        lblColSearchRelation = new Label();
        lblColSearchAction = new Label();
        pnlSearchToolbar = new Panel();
        lblSearchPrompt = new Label();
        pnlSearchBox = new Panel();
        lblSearchBoxIcon = new Label();
        txtSearchKeyword = new TextBox();
        btnSearch = new Button();
        pnlTabRequestsPage = new Panel();
        splitRequests = new SplitContainer();
        pnlIncomingCard = new Panel();
        flpIncomingRequests = new FlowLayoutPanel();
        pnlIncomingEmpty = new Panel();
        lblIncEmptyIcon = new Label();
        lblIncEmptyTitle = new Label();
        lblIncEmptySub = new Label();
        pnlIncomingHeader = new Panel();
        lblIncomingTitle = new Label();
        pnlOutgoingCard = new Panel();
        flpOutgoingRequests = new FlowLayoutPanel();
        pnlOutgoingEmpty = new Panel();
        lblOutEmptyIcon = new Label();
        lblOutEmptyTitle = new Label();
        lblOutEmptySub = new Label();
        pnlOutgoingHeader = new Panel();
        lblOutgoingTitle = new Label();
        pnlTopTabs = new Panel();
        btnTopTabFriends = new Button();
        btnTopTabSearch = new Button();
        btnTopTabRequests = new Button();
        pnlTopTabIndicator = new Panel();
        pnlLeftNav = new Panel();
        btnNavFriends = new Button();
        btnNavSearch = new Button();
        btnNavRequests = new Button();
        pnlLeftBottom = new Panel();
        btnNavRefresh = new Button();
        pnlMyStatus = new Panel();
        lblMyStatusDot = new Label();
        lblMyStatusText = new Label();

        pnlHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picRobotMascot).BeginInit();
        pnlBodyContainer.SuspendLayout();
        pnlCardWrapper.SuspendLayout();
        pnlMainCard.SuspendLayout();
        pnlTabPages.SuspendLayout();
        pnlTabFriendsPage.SuspendLayout();
        pnlFriendsEmpty.SuspendLayout();
        pnlFriendsTableHeader.SuspendLayout();
        pnlFriendsToolbar.SuspendLayout();
        pnlFriendSearchBox.SuspendLayout();
        pnlFriendsFooter.SuspendLayout();
        pnlTabSearchPage.SuspendLayout();
        pnlSearchEmpty.SuspendLayout();
        pnlSearchTableHeader.SuspendLayout();
        pnlSearchToolbar.SuspendLayout();
        pnlSearchBox.SuspendLayout();
        pnlTabRequestsPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitRequests).BeginInit();
        splitRequests.Panel1.SuspendLayout();
        splitRequests.Panel2.SuspendLayout();
        splitRequests.SuspendLayout();
        pnlIncomingCard.SuspendLayout();
        pnlIncomingEmpty.SuspendLayout();
        pnlIncomingHeader.SuspendLayout();
        pnlOutgoingCard.SuspendLayout();
        pnlOutgoingEmpty.SuspendLayout();
        pnlOutgoingHeader.SuspendLayout();
        pnlTopTabs.SuspendLayout();
        pnlLeftNav.SuspendLayout();
        pnlLeftBottom.SuspendLayout();
        pnlMyStatus.SuspendLayout();
        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.Controls.Add(picRobotMascot);
        pnlHeader.Controls.Add(lblHeaderTitle);
        pnlHeader.Controls.Add(lblHeaderSubtitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(1000, 78);
        pnlHeader.TabIndex = 0;

        // 
        // picRobotMascot
        // 
        picRobotMascot.BackColor = Color.Transparent;
        picRobotMascot.Location = new Point(16, 6);
        picRobotMascot.Name = "picRobotMascot";
        picRobotMascot.Size = new Size(66, 66);
        picRobotMascot.SizeMode = PictureBoxSizeMode.Zoom;
        picRobotMascot.TabIndex = 0;
        picRobotMascot.TabStop = false;

        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.BackColor = Color.Transparent;
        lblHeaderTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(90, 14);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(205, 28);
        lblHeaderTitle.TabIndex = 1;
        lblHeaderTitle.Text = "QUẢN LÝ BẠN BÈ";

        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.AutoSize = true;
        lblHeaderSubtitle.BackColor = Color.Transparent;
        lblHeaderSubtitle.Font = new Font("Segoe UI", 9F);
        lblHeaderSubtitle.ForeColor = Color.FromArgb(224, 231, 255);
        lblHeaderSubtitle.Location = new Point(92, 44);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new Size(303, 15);
        lblHeaderSubtitle.TabIndex = 2;
        lblHeaderSubtitle.Text = "Tìm kiếm người dùng, gửi và phản hồi lời mời kết bạn";

        // 
        // pnlBodyContainer
        // 
        pnlBodyContainer.BackColor = Color.FromArgb(241, 245, 249);
        pnlBodyContainer.Controls.Add(pnlCardWrapper);
        pnlBodyContainer.Controls.Add(pnlLeftNav);
        pnlBodyContainer.Dock = DockStyle.Fill;
        pnlBodyContainer.Location = new Point(0, 78);
        pnlBodyContainer.Name = "pnlBodyContainer";
        pnlBodyContainer.Size = new Size(1000, 562);
        pnlBodyContainer.TabIndex = 1;

        // 
        // pnlLeftNav
        // 
        pnlLeftNav.BackColor = Color.FromArgb(248, 250, 252);
        pnlLeftNav.Controls.Add(btnNavFriends);
        pnlLeftNav.Controls.Add(btnNavSearch);
        pnlLeftNav.Controls.Add(btnNavRequests);
        pnlLeftNav.Controls.Add(pnlLeftBottom);
        pnlLeftNav.Dock = DockStyle.Left;
        pnlLeftNav.Location = new Point(0, 0);
        pnlLeftNav.Name = "pnlLeftNav";
        pnlLeftNav.Padding = new Padding(12, 16, 12, 14);
        pnlLeftNav.Size = new Size(205, 562);
        pnlLeftNav.TabIndex = 0;

        // 
        // btnNavFriends
        // 
        btnNavFriends.Cursor = Cursors.Hand;
        btnNavFriends.Dock = DockStyle.Top;
        btnNavFriends.FlatAppearance.BorderSize = 0;
        btnNavFriends.FlatStyle = FlatStyle.Flat;
        btnNavFriends.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
        btnNavFriends.ForeColor = Color.FromArgb(37, 99, 235);
        btnNavFriends.Location = new Point(12, 16);
        btnNavFriends.Name = "btnNavFriends";
        btnNavFriends.Padding = new Padding(10, 0, 0, 0);
        btnNavFriends.Size = new Size(181, 44);
        btnNavFriends.TabIndex = 0;
        btnNavFriends.Text = "Bạn bè hiện tại";
        btnNavFriends.TextAlign = ContentAlignment.MiddleLeft;
        btnNavFriends.UseVisualStyleBackColor = true;

        // 
        // btnNavSearch
        // 
        btnNavSearch.Cursor = Cursors.Hand;
        btnNavSearch.Dock = DockStyle.Top;
        btnNavSearch.FlatAppearance.BorderSize = 0;
        btnNavSearch.FlatStyle = FlatStyle.Flat;
        btnNavSearch.Font = new Font("Segoe UI", 9.25F);
        btnNavSearch.ForeColor = Color.FromArgb(100, 116, 139);
        btnNavSearch.Location = new Point(12, 66);
        btnNavSearch.Name = "btnNavSearch";
        btnNavSearch.Padding = new Padding(10, 0, 0, 0);
        btnNavSearch.Size = new Size(181, 44);
        btnNavSearch.TabIndex = 1;
        btnNavSearch.Text = "Tìm kiếm kết bạn";
        btnNavSearch.TextAlign = ContentAlignment.MiddleLeft;
        btnNavSearch.UseVisualStyleBackColor = true;

        // 
        // btnNavRequests
        // 
        btnNavRequests.Cursor = Cursors.Hand;
        btnNavRequests.Dock = DockStyle.Top;
        btnNavRequests.FlatAppearance.BorderSize = 0;
        btnNavRequests.FlatStyle = FlatStyle.Flat;
        btnNavRequests.Font = new Font("Segoe UI", 9.25F);
        btnNavRequests.ForeColor = Color.FromArgb(100, 116, 139);
        btnNavRequests.Location = new Point(12, 116);
        btnNavRequests.Name = "btnNavRequests";
        btnNavRequests.Padding = new Padding(10, 0, 0, 0);
        btnNavRequests.Size = new Size(181, 44);
        btnNavRequests.TabIndex = 2;
        btnNavRequests.Text = "Lời mời kết bạn";
        btnNavRequests.TextAlign = ContentAlignment.MiddleLeft;
        btnNavRequests.UseVisualStyleBackColor = true;

        // 
        // pnlLeftBottom
        // 
        pnlLeftBottom.Controls.Add(btnNavRefresh);
        pnlLeftBottom.Controls.Add(pnlMyStatus);
        pnlLeftBottom.Dock = DockStyle.Bottom;
        pnlLeftBottom.Location = new Point(12, 468);
        pnlLeftBottom.Name = "pnlLeftBottom";
        pnlLeftBottom.Size = new Size(181, 80);
        pnlLeftBottom.TabIndex = 3;

        // 
        // pnlMyStatus
        // 
        pnlMyStatus.BackColor = Color.White;
        pnlMyStatus.Controls.Add(lblMyStatusDot);
        pnlMyStatus.Controls.Add(lblMyStatusText);
        pnlMyStatus.Dock = DockStyle.Top;
        pnlMyStatus.Location = new Point(0, 0);
        pnlMyStatus.Name = "pnlMyStatus";
        pnlMyStatus.Size = new Size(181, 34);
        pnlMyStatus.TabIndex = 0;

        // 
        // lblMyStatusDot
        // 
        lblMyStatusDot.AutoSize = true;
        lblMyStatusDot.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblMyStatusDot.ForeColor = Color.FromArgb(22, 163, 74);
        lblMyStatusDot.Location = new Point(10, 10);
        lblMyStatusDot.Name = "lblMyStatusDot";
        lblMyStatusDot.Size = new Size(15, 13);
        lblMyStatusDot.TabIndex = 0;
        lblMyStatusDot.Text = "●";

        // 
        // lblMyStatusText
        // 
        lblMyStatusText.AutoSize = true;
        lblMyStatusText.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblMyStatusText.ForeColor = Color.FromArgb(71, 85, 105);
        lblMyStatusText.Location = new Point(28, 9);
        lblMyStatusText.Name = "lblMyStatusText";
        lblMyStatusText.Size = new Size(61, 15);
        lblMyStatusText.TabIndex = 1;
        lblMyStatusText.Text = "Trực tuyến";

        // 
        // btnNavRefresh
        // 
        btnNavRefresh.Cursor = Cursors.Hand;
        btnNavRefresh.Dock = DockStyle.Bottom;
        btnNavRefresh.FlatAppearance.BorderSize = 0;
        btnNavRefresh.FlatStyle = FlatStyle.Flat;
        btnNavRefresh.Font = new Font("Segoe UI", 8.5F);
        btnNavRefresh.ForeColor = Color.FromArgb(100, 116, 139);
        btnNavRefresh.Location = new Point(0, 42);
        btnNavRefresh.Name = "btnNavRefresh";
        btnNavRefresh.Size = new Size(181, 38);
        btnNavRefresh.TabIndex = 1;
        btnNavRefresh.Text = "Làm mới";
        btnNavRefresh.TextAlign = ContentAlignment.MiddleLeft;
        btnNavRefresh.UseVisualStyleBackColor = true;

        // 
        // pnlCardWrapper
        // 
        pnlCardWrapper.Controls.Add(pnlMainCard);
        pnlCardWrapper.Dock = DockStyle.Fill;
        pnlCardWrapper.Location = new Point(205, 0);
        pnlCardWrapper.Name = "pnlCardWrapper";
        pnlCardWrapper.Padding = new Padding(12, 12, 16, 14);
        pnlCardWrapper.Size = new Size(795, 562);
        pnlCardWrapper.TabIndex = 1;

        // 
        // pnlMainCard
        // 
        pnlMainCard.BackColor = Color.White;
        pnlMainCard.Controls.Add(pnlTabPages);
        pnlMainCard.Controls.Add(pnlTopTabs);
        pnlMainCard.Dock = DockStyle.Fill;
        pnlMainCard.Location = new Point(12, 12);
        pnlMainCard.Name = "pnlMainCard";
        pnlMainCard.Padding = new Padding(16, 10, 16, 12);
        pnlMainCard.Size = new Size(767, 536);
        pnlMainCard.TabIndex = 0;

        // 
        // pnlTopTabs
        // 
        pnlTopTabs.Controls.Add(pnlTopTabIndicator);
        pnlTopTabs.Controls.Add(btnTopTabFriends);
        pnlTopTabs.Controls.Add(btnTopTabSearch);
        pnlTopTabs.Controls.Add(btnTopTabRequests);
        pnlTopTabs.Dock = DockStyle.Top;
        pnlTopTabs.Location = new Point(16, 10);
        pnlTopTabs.Name = "pnlTopTabs";
        pnlTopTabs.Size = new Size(735, 42);
        pnlTopTabs.TabIndex = 0;

        // 
        // btnTopTabFriends
        // 
        btnTopTabFriends.Cursor = Cursors.Hand;
        btnTopTabFriends.FlatAppearance.BorderSize = 0;
        btnTopTabFriends.FlatStyle = FlatStyle.Flat;
        btnTopTabFriends.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        btnTopTabFriends.ForeColor = Color.FromArgb(37, 99, 235);
        btnTopTabFriends.Location = new Point(0, 0);
        btnTopTabFriends.Name = "btnTopTabFriends";
        btnTopTabFriends.Size = new Size(160, 38);
        btnTopTabFriends.TabIndex = 0;
        btnTopTabFriends.Text = "Bạn bè hiện tại";
        btnTopTabFriends.UseVisualStyleBackColor = true;

        // 
        // btnTopTabSearch
        // 
        btnTopTabSearch.Cursor = Cursors.Hand;
        btnTopTabSearch.FlatAppearance.BorderSize = 0;
        btnTopTabSearch.FlatStyle = FlatStyle.Flat;
        btnTopTabSearch.Font = new Font("Segoe UI", 9.5F);
        btnTopTabSearch.ForeColor = Color.FromArgb(100, 116, 139);
        btnTopTabSearch.Location = new Point(165, 0);
        btnTopTabSearch.Name = "btnTopTabSearch";
        btnTopTabSearch.Size = new Size(160, 38);
        btnTopTabSearch.TabIndex = 1;
        btnTopTabSearch.Text = "Tìm kiếm kết bạn";
        btnTopTabSearch.UseVisualStyleBackColor = true;

        // 
        // btnTopTabRequests
        // 
        btnTopTabRequests.Cursor = Cursors.Hand;
        btnTopTabRequests.FlatAppearance.BorderSize = 0;
        btnTopTabRequests.FlatStyle = FlatStyle.Flat;
        btnTopTabRequests.Font = new Font("Segoe UI", 9.5F);
        btnTopTabRequests.ForeColor = Color.FromArgb(100, 116, 139);
        btnTopTabRequests.Location = new Point(330, 0);
        btnTopTabRequests.Name = "btnTopTabRequests";
        btnTopTabRequests.Size = new Size(160, 38);
        btnTopTabRequests.TabIndex = 2;
        btnTopTabRequests.Text = "Lời mời kết bạn";
        btnTopTabRequests.UseVisualStyleBackColor = true;

        // 
        // pnlTopTabIndicator
        // 
        pnlTopTabIndicator.BackColor = Color.FromArgb(37, 99, 235);
        pnlTopTabIndicator.Location = new Point(8, 38);
        pnlTopTabIndicator.Name = "pnlTopTabIndicator";
        pnlTopTabIndicator.Size = new Size(144, 3);
        pnlTopTabIndicator.TabIndex = 3;

        // 
        // pnlTabPages
        // 
        pnlTabPages.Controls.Add(pnlTabFriendsPage);
        pnlTabPages.Controls.Add(pnlTabSearchPage);
        pnlTabPages.Controls.Add(pnlTabRequestsPage);
        pnlTabPages.Dock = DockStyle.Fill;
        pnlTabPages.Location = new Point(16, 52);
        pnlTabPages.Name = "pnlTabPages";
        pnlTabPages.Size = new Size(735, 472);
        pnlTabPages.TabIndex = 1;

        // 
        // ── TAB 1: BẠN BÈ HIỆN TẠI ──────────────────────────────────────────
        // 
        pnlTabFriendsPage.Controls.Add(flpFriendsList);
        pnlTabFriendsPage.Controls.Add(pnlFriendsEmpty);
        pnlTabFriendsPage.Controls.Add(pnlFriendsTableHeader);
        pnlTabFriendsPage.Controls.Add(pnlFriendsToolbar);
        pnlTabFriendsPage.Controls.Add(pnlFriendsFooter);
        pnlTabFriendsPage.Dock = DockStyle.Fill;
        pnlTabFriendsPage.Location = new Point(0, 0);
        pnlTabFriendsPage.Name = "pnlTabFriendsPage";
        pnlTabFriendsPage.Size = new Size(735, 472);
        pnlTabFriendsPage.TabIndex = 0;

        // 
        // pnlFriendsToolbar
        // 
        pnlFriendsToolbar.Controls.Add(pnlFriendSearchBox);
        pnlFriendsToolbar.Controls.Add(cboFriendFilter);
        pnlFriendsToolbar.Controls.Add(btnAddFriendQuick);
        pnlFriendsToolbar.Dock = DockStyle.Top;
        pnlFriendsToolbar.Location = new Point(0, 0);
        pnlFriendsToolbar.Name = "pnlFriendsToolbar";
        pnlFriendsToolbar.Padding = new Padding(0, 6, 0, 8);
        pnlFriendsToolbar.Size = new Size(735, 50);
        pnlFriendsToolbar.TabIndex = 0;

        // 
        // pnlFriendSearchBox
        // 
        pnlFriendSearchBox.BackColor = Color.FromArgb(248, 250, 252);
        pnlFriendSearchBox.Controls.Add(lblFriendSearchIcon);
        pnlFriendSearchBox.Controls.Add(txtFriendSearch);
        pnlFriendSearchBox.Location = new Point(0, 6);
        pnlFriendSearchBox.Name = "pnlFriendSearchBox";
        pnlFriendSearchBox.Size = new Size(340, 36);
        pnlFriendSearchBox.TabIndex = 0;

        // 
        // lblFriendSearchIcon
        // 
        lblFriendSearchIcon.AutoSize = true;
        lblFriendSearchIcon.Font = new Font("Segoe MDL2 Assets", 9F);
        lblFriendSearchIcon.ForeColor = Color.FromArgb(148, 163, 184);
        lblFriendSearchIcon.Location = new Point(8, 10);
        lblFriendSearchIcon.Name = "lblFriendSearchIcon";
        lblFriendSearchIcon.Size = new Size(20, 16);
        lblFriendSearchIcon.TabIndex = 0;
        lblFriendSearchIcon.Text = "\uE721";

        // 
        // txtFriendSearch
        // 
        txtFriendSearch.BackColor = Color.FromArgb(248, 250, 252);
        txtFriendSearch.BorderStyle = BorderStyle.None;
        txtFriendSearch.Font = new Font("Segoe UI", 9.25F);
        txtFriendSearch.ForeColor = Color.FromArgb(15, 23, 42);
        txtFriendSearch.Location = new Point(32, 9);
        txtFriendSearch.Name = "txtFriendSearch";
        txtFriendSearch.PlaceholderText = "Tìm kiếm bạn bè...";
        txtFriendSearch.Size = new Size(295, 17);
        txtFriendSearch.TabIndex = 1;

        // 
        // cboFriendFilter
        // 
        cboFriendFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboFriendFilter.FlatStyle = FlatStyle.Flat;
        cboFriendFilter.Font = new Font("Segoe UI", 9F);
        cboFriendFilter.ForeColor = Color.FromArgb(15, 23, 42);
        cboFriendFilter.Items.AddRange(new object[] { "Tất cả", "● Đang trực tuyến", "○ Ngoại tuyến" });
        cboFriendFilter.Location = new Point(355, 12);
        cboFriendFilter.Name = "cboFriendFilter";
        cboFriendFilter.Size = new Size(155, 23);
        cboFriendFilter.TabIndex = 1;

        // 
        // btnAddFriendQuick
        // 
        btnAddFriendQuick.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAddFriendQuick.BackColor = Color.FromArgb(37, 99, 235);
        btnAddFriendQuick.Cursor = Cursors.Hand;
        btnAddFriendQuick.FlatAppearance.BorderSize = 0;
        btnAddFriendQuick.FlatStyle = FlatStyle.Flat;
        btnAddFriendQuick.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        btnAddFriendQuick.ForeColor = Color.White;
        btnAddFriendQuick.Location = new Point(625, 6);
        btnAddFriendQuick.Name = "btnAddFriendQuick";
        btnAddFriendQuick.Size = new Size(110, 36);
        btnAddFriendQuick.TabIndex = 2;
        btnAddFriendQuick.Text = "Thêm bạn";
        btnAddFriendQuick.UseVisualStyleBackColor = false;

        // 
        // pnlFriendsTableHeader
        // 
        pnlFriendsTableHeader.BackColor = Color.FromArgb(248, 250, 252);
        pnlFriendsTableHeader.Controls.Add(lblColFriendName);
        pnlFriendsTableHeader.Controls.Add(lblColFriendEmail);
        pnlFriendsTableHeader.Controls.Add(lblColFriendStatus);
        pnlFriendsTableHeader.Controls.Add(lblColFriendAction);
        pnlFriendsTableHeader.Dock = DockStyle.Top;
        pnlFriendsTableHeader.Location = new Point(0, 50);
        pnlFriendsTableHeader.Name = "pnlFriendsTableHeader";
        pnlFriendsTableHeader.Size = new Size(735, 34);
        pnlFriendsTableHeader.TabIndex = 1;

        // 
        // lblColFriendName
        // 
        lblColFriendName.AutoSize = true;
        lblColFriendName.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColFriendName.ForeColor = Color.FromArgb(100, 116, 139);
        lblColFriendName.Location = new Point(14, 9);
        lblColFriendName.Name = "lblColFriendName";
        lblColFriendName.Size = new Size(89, 15);
        lblColFriendName.TabIndex = 0;
        lblColFriendName.Text = "Tên người dùng";

        // 
        // lblColFriendEmail
        // 
        lblColFriendEmail.AutoSize = true;
        lblColFriendEmail.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColFriendEmail.ForeColor = Color.FromArgb(100, 116, 139);
        lblColFriendEmail.Location = new Point(255, 9);
        lblColFriendEmail.Name = "lblColFriendEmail";
        lblColFriendEmail.Size = new Size(36, 15);
        lblColFriendEmail.TabIndex = 1;
        lblColFriendEmail.Text = "Email";

        // 
        // lblColFriendStatus
        // 
        lblColFriendStatus.AutoSize = true;
        lblColFriendStatus.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColFriendStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblColFriendStatus.Location = new Point(475, 9);
        lblColFriendStatus.Name = "lblColFriendStatus";
        lblColFriendStatus.Size = new Size(60, 15);
        lblColFriendStatus.TabIndex = 2;
        lblColFriendStatus.Text = "Trạng thái";

        // 
        // lblColFriendAction
        // 
        lblColFriendAction.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblColFriendAction.AutoSize = true;
        lblColFriendAction.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColFriendAction.ForeColor = Color.FromArgb(100, 116, 139);
        lblColFriendAction.Location = new Point(650, 9);
        lblColFriendAction.Name = "lblColFriendAction";
        lblColFriendAction.Size = new Size(54, 15);
        lblColFriendAction.TabIndex = 3;
        lblColFriendAction.Text = "Thao tác";

        // 
        // flpFriendsList
        // 
        flpFriendsList.AutoScroll = true;
        flpFriendsList.BackColor = Color.White;
        flpFriendsList.Dock = DockStyle.Fill;
        flpFriendsList.FlowDirection = FlowDirection.TopDown;
        flpFriendsList.Location = new Point(0, 84);
        flpFriendsList.Name = "flpFriendsList";
        flpFriendsList.Padding = new Padding(0, 4, 4, 4);
        flpFriendsList.Size = new Size(735, 348);
        flpFriendsList.TabIndex = 2;
        flpFriendsList.WrapContents = false;

        // 
        // pnlFriendsEmpty
        // 
        pnlFriendsEmpty.BackColor = Color.White;
        pnlFriendsEmpty.Controls.Add(lblFriendsEmptyIcon);
        pnlFriendsEmpty.Controls.Add(lblFriendsEmptyTitle);
        pnlFriendsEmpty.Controls.Add(lblFriendsEmptySub);
        pnlFriendsEmpty.Dock = DockStyle.Fill;
        pnlFriendsEmpty.Location = new Point(0, 84);
        pnlFriendsEmpty.Name = "pnlFriendsEmpty";
        pnlFriendsEmpty.Size = new Size(735, 348);
        pnlFriendsEmpty.TabIndex = 3;
        pnlFriendsEmpty.Visible = false;

        // 
        // lblFriendsEmptyIcon
        // 
        lblFriendsEmptyIcon.Anchor = AnchorStyles.None;
        lblFriendsEmptyIcon.Font = new Font("Segoe MDL2 Assets", 32F);
        lblFriendsEmptyIcon.ForeColor = Color.FromArgb(203, 213, 225);
        lblFriendsEmptyIcon.Location = new Point(292, 90);
        lblFriendsEmptyIcon.Name = "lblFriendsEmptyIcon";
        lblFriendsEmptyIcon.Size = new Size(150, 55);
        lblFriendsEmptyIcon.TabIndex = 0;
        lblFriendsEmptyIcon.Text = "\uE716";
        lblFriendsEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblFriendsEmptyTitle
        // 
        lblFriendsEmptyTitle.Anchor = AnchorStyles.None;
        lblFriendsEmptyTitle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        lblFriendsEmptyTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblFriendsEmptyTitle.Location = new Point(167, 150);
        lblFriendsEmptyTitle.Name = "lblFriendsEmptyTitle";
        lblFriendsEmptyTitle.Size = new Size(400, 24);
        lblFriendsEmptyTitle.TabIndex = 1;
        lblFriendsEmptyTitle.Text = "Chưa có bạn bè nào";
        lblFriendsEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblFriendsEmptySub
        // 
        lblFriendsEmptySub.Anchor = AnchorStyles.None;
        lblFriendsEmptySub.Font = new Font("Segoe UI", 9F);
        lblFriendsEmptySub.ForeColor = Color.FromArgb(148, 163, 184);
        lblFriendsEmptySub.Location = new Point(142, 178);
        lblFriendsEmptySub.Name = "lblFriendsEmptySub";
        lblFriendsEmptySub.Size = new Size(450, 40);
        lblFriendsEmptySub.TabIndex = 2;
        lblFriendsEmptySub.Text = "Hãy tìm kiếm người dùng trong mạng LAN để gửi lời mời kết bạn.";
        lblFriendsEmptySub.TextAlign = ContentAlignment.TopCenter;

        // 
        // pnlFriendsFooter
        // 
        pnlFriendsFooter.BackColor = Color.White;
        pnlFriendsFooter.Controls.Add(lblFriendsCount);
        pnlFriendsFooter.Dock = DockStyle.Bottom;
        pnlFriendsFooter.Location = new Point(0, 432);
        pnlFriendsFooter.Name = "pnlFriendsFooter";
        pnlFriendsFooter.Size = new Size(735, 40);
        pnlFriendsFooter.TabIndex = 4;

        // 
        // lblFriendsCount
        // 
        lblFriendsCount.AutoSize = true;
        lblFriendsCount.Font = new Font("Segoe UI", 8.5F);
        lblFriendsCount.ForeColor = Color.FromArgb(100, 116, 139);
        lblFriendsCount.Location = new Point(6, 14);
        lblFriendsCount.Name = "lblFriendsCount";
        lblFriendsCount.Size = new Size(111, 15);
        lblFriendsCount.TabIndex = 0;
        lblFriendsCount.Text = "Tổng cộng: 0 bạn bè";

        // 
        // ── TAB 2: TÌM KIẾM KẾT BẠN ──────────────────────────────────────────
        // 
        pnlTabSearchPage.Controls.Add(flpSearchResults);
        pnlTabSearchPage.Controls.Add(pnlSearchEmpty);
        pnlTabSearchPage.Controls.Add(pnlSearchTableHeader);
        pnlTabSearchPage.Controls.Add(pnlSearchToolbar);
        pnlTabSearchPage.Dock = DockStyle.Fill;
        pnlTabSearchPage.Location = new Point(0, 0);
        pnlTabSearchPage.Name = "pnlTabSearchPage";
        pnlTabSearchPage.Size = new Size(735, 472);
        pnlTabSearchPage.TabIndex = 1;
        pnlTabSearchPage.Visible = false;

        // 
        // pnlSearchToolbar
        // 
        pnlSearchToolbar.Controls.Add(lblSearchPrompt);
        pnlSearchToolbar.Controls.Add(pnlSearchBox);
        pnlSearchToolbar.Controls.Add(btnSearch);
        pnlSearchToolbar.Dock = DockStyle.Top;
        pnlSearchToolbar.Location = new Point(0, 0);
        pnlSearchToolbar.Name = "pnlSearchToolbar";
        pnlSearchToolbar.Size = new Size(735, 76);
        pnlSearchToolbar.TabIndex = 0;

        // 
        // lblSearchPrompt
        // 
        lblSearchPrompt.AutoSize = true;
        lblSearchPrompt.Font = new Font("Segoe UI Semibold", 8.75F, FontStyle.Bold);
        lblSearchPrompt.ForeColor = Color.FromArgb(71, 85, 105);
        lblSearchPrompt.Location = new Point(2, 8);
        lblSearchPrompt.Name = "lblSearchPrompt";
        lblSearchPrompt.Size = new Size(335, 15);
        lblSearchPrompt.TabIndex = 0;
        lblSearchPrompt.Text = "Tìm kiếm người dùng trong mạng LAN (theo tên hoặc email):";

        // 
        // pnlSearchBox
        // 
        pnlSearchBox.BackColor = Color.FromArgb(248, 250, 252);
        pnlSearchBox.Controls.Add(lblSearchBoxIcon);
        pnlSearchBox.Controls.Add(txtSearchKeyword);
        pnlSearchBox.Location = new Point(0, 30);
        pnlSearchBox.Name = "pnlSearchBox";
        pnlSearchBox.Size = new Size(460, 36);
        pnlSearchBox.TabIndex = 1;

        // 
        // lblSearchBoxIcon
        // 
        lblSearchBoxIcon.AutoSize = true;
        lblSearchBoxIcon.Font = new Font("Segoe MDL2 Assets", 9F);
        lblSearchBoxIcon.ForeColor = Color.FromArgb(148, 163, 184);
        lblSearchBoxIcon.Location = new Point(10, 10);
        lblSearchBoxIcon.Name = "lblSearchBoxIcon";
        lblSearchBoxIcon.Size = new Size(20, 16);
        lblSearchBoxIcon.TabIndex = 0;
        lblSearchBoxIcon.Text = "\uE721";

        // 
        // txtSearchKeyword
        // 
        txtSearchKeyword.BackColor = Color.FromArgb(248, 250, 252);
        txtSearchKeyword.BorderStyle = BorderStyle.None;
        txtSearchKeyword.Font = new Font("Segoe UI", 9.25F);
        txtSearchKeyword.ForeColor = Color.FromArgb(15, 23, 42);
        txtSearchKeyword.Location = new Point(34, 9);
        txtSearchKeyword.Name = "txtSearchKeyword";
        txtSearchKeyword.PlaceholderText = "Nhập tên người dùng hoặc email...";
        txtSearchKeyword.Size = new Size(415, 17);
        txtSearchKeyword.TabIndex = 1;

        // 
        // btnSearch
        // 
        btnSearch.BackColor = Color.FromArgb(37, 99, 235);
        btnSearch.Cursor = Cursors.Hand;
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.FlatStyle = FlatStyle.Flat;
        btnSearch.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        btnSearch.ForeColor = Color.White;
        btnSearch.Location = new Point(475, 30);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(110, 36);
        btnSearch.TabIndex = 2;
        btnSearch.Text = "Tìm kiếm";
        btnSearch.UseVisualStyleBackColor = false;

        // 
        // pnlSearchTableHeader
        // 
        pnlSearchTableHeader.BackColor = Color.FromArgb(248, 250, 252);
        pnlSearchTableHeader.Controls.Add(lblColSearchUser);
        pnlSearchTableHeader.Controls.Add(lblColSearchEmail);
        pnlSearchTableHeader.Controls.Add(lblColSearchRelation);
        pnlSearchTableHeader.Controls.Add(lblColSearchAction);
        pnlSearchTableHeader.Dock = DockStyle.Top;
        pnlSearchTableHeader.Location = new Point(0, 76);
        pnlSearchTableHeader.Name = "pnlSearchTableHeader";
        pnlSearchTableHeader.Size = new Size(735, 34);
        pnlSearchTableHeader.TabIndex = 1;

        // 
        // lblColSearchUser
        // 
        lblColSearchUser.AutoSize = true;
        lblColSearchUser.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColSearchUser.ForeColor = Color.FromArgb(100, 116, 139);
        lblColSearchUser.Location = new Point(14, 9);
        lblColSearchUser.Name = "lblColSearchUser";
        lblColSearchUser.Size = new Size(70, 15);
        lblColSearchUser.TabIndex = 0;
        lblColSearchUser.Text = "Người dùng";

        // 
        // lblColSearchEmail
        // 
        lblColSearchEmail.AutoSize = true;
        lblColSearchEmail.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColSearchEmail.ForeColor = Color.FromArgb(100, 116, 139);
        lblColSearchEmail.Location = new Point(255, 9);
        lblColSearchEmail.Name = "lblColSearchEmail";
        lblColSearchEmail.Size = new Size(36, 15);
        lblColSearchEmail.TabIndex = 1;
        lblColSearchEmail.Text = "Email";

        // 
        // lblColSearchRelation
        // 
        lblColSearchRelation.AutoSize = true;
        lblColSearchRelation.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColSearchRelation.ForeColor = Color.FromArgb(100, 116, 139);
        lblColSearchRelation.Location = new Point(475, 9);
        lblColSearchRelation.Name = "lblColSearchRelation";
        lblColSearchRelation.Size = new Size(81, 15);
        lblColSearchRelation.TabIndex = 2;
        lblColSearchRelation.Text = "Mối quan hệ";

        // 
        // lblColSearchAction
        // 
        lblColSearchAction.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblColSearchAction.AutoSize = true;
        lblColSearchAction.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblColSearchAction.ForeColor = Color.FromArgb(100, 116, 139);
        lblColSearchAction.Location = new Point(650, 9);
        lblColSearchAction.Name = "lblColSearchAction";
        lblColSearchAction.Size = new Size(54, 15);
        lblColSearchAction.TabIndex = 3;
        lblColSearchAction.Text = "Thao tác";

        // 
        // flpSearchResults
        // 
        flpSearchResults.AutoScroll = true;
        flpSearchResults.BackColor = Color.White;
        flpSearchResults.Dock = DockStyle.Fill;
        flpSearchResults.FlowDirection = FlowDirection.TopDown;
        flpSearchResults.Location = new Point(0, 110);
        flpSearchResults.Name = "flpSearchResults";
        flpSearchResults.Padding = new Padding(0, 4, 4, 4);
        flpSearchResults.Size = new Size(735, 362);
        flpSearchResults.TabIndex = 2;
        flpSearchResults.WrapContents = false;

        // 
        // pnlSearchEmpty
        // 
        pnlSearchEmpty.BackColor = Color.White;
        pnlSearchEmpty.Controls.Add(lblSearchEmptyIcon);
        pnlSearchEmpty.Controls.Add(lblSearchEmptyTitle);
        pnlSearchEmpty.Controls.Add(lblSearchEmptySub);
        pnlSearchEmpty.Dock = DockStyle.Fill;
        pnlSearchEmpty.Location = new Point(0, 110);
        pnlSearchEmpty.Name = "pnlSearchEmpty";
        pnlSearchEmpty.Size = new Size(735, 362);
        pnlSearchEmpty.TabIndex = 3;

        // 
        // lblSearchEmptyIcon
        // 
        lblSearchEmptyIcon.Anchor = AnchorStyles.None;
        lblSearchEmptyIcon.Font = new Font("Segoe MDL2 Assets", 32F);
        lblSearchEmptyIcon.ForeColor = Color.FromArgb(203, 213, 225);
        lblSearchEmptyIcon.Location = new Point(292, 90);
        lblSearchEmptyIcon.Name = "lblSearchEmptyIcon";
        lblSearchEmptyIcon.Size = new Size(150, 55);
        lblSearchEmptyIcon.TabIndex = 0;
        lblSearchEmptyIcon.Text = "\uE721";
        lblSearchEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblSearchEmptyTitle
        // 
        lblSearchEmptyTitle.Anchor = AnchorStyles.None;
        lblSearchEmptyTitle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        lblSearchEmptyTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblSearchEmptyTitle.Location = new Point(167, 150);
        lblSearchEmptyTitle.Name = "lblSearchEmptyTitle";
        lblSearchEmptyTitle.Size = new Size(400, 24);
        lblSearchEmptyTitle.TabIndex = 1;
        lblSearchEmptyTitle.Text = "Tìm kiếm bạn bè trong mạng LAN";
        lblSearchEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblSearchEmptySub
        // 
        lblSearchEmptySub.Anchor = AnchorStyles.None;
        lblSearchEmptySub.Font = new Font("Segoe UI", 9F);
        lblSearchEmptySub.ForeColor = Color.FromArgb(148, 163, 184);
        lblSearchEmptySub.Location = new Point(142, 178);
        lblSearchEmptySub.Name = "lblSearchEmptySub";
        lblSearchEmptySub.Size = new Size(450, 40);
        lblSearchEmptySub.TabIndex = 2;
        lblSearchEmptySub.Text = "Nhập tên người dùng hoặc email vào ô tìm kiếm ở trên để bắt đầu.";
        lblSearchEmptySub.TextAlign = ContentAlignment.TopCenter;

        // 
        // ── TAB 3: LỜI MỜI KẾT BẠN ──────────────────────────────────────────
        // 
        pnlTabRequestsPage.Controls.Add(splitRequests);
        pnlTabRequestsPage.Dock = DockStyle.Fill;
        pnlTabRequestsPage.Location = new Point(0, 0);
        pnlTabRequestsPage.Name = "pnlTabRequestsPage";
        pnlTabRequestsPage.Size = new Size(735, 472);
        pnlTabRequestsPage.TabIndex = 2;
        pnlTabRequestsPage.Visible = false;

        // 
        // splitRequests
        // 
        splitRequests.Dock = DockStyle.Fill;
        splitRequests.Location = new Point(0, 0);
        splitRequests.Name = "splitRequests";
        splitRequests.Panel1.Controls.Add(pnlIncomingCard);
        splitRequests.Panel1.Padding = new Padding(0, 4, 8, 4);
        splitRequests.Panel2.Controls.Add(pnlOutgoingCard);
        splitRequests.Panel2.Padding = new Padding(8, 4, 0, 4);
        splitRequests.Size = new Size(735, 472);
        splitRequests.SplitterDistance = 360;
        splitRequests.TabIndex = 0;

        // 
        // pnlIncomingCard
        // 
        pnlIncomingCard.BackColor = Color.White;
        pnlIncomingCard.Controls.Add(flpIncomingRequests);
        pnlIncomingCard.Controls.Add(pnlIncomingEmpty);
        pnlIncomingCard.Controls.Add(pnlIncomingHeader);
        pnlIncomingCard.Dock = DockStyle.Fill;
        pnlIncomingCard.Location = new Point(0, 4);
        pnlIncomingCard.Name = "pnlIncomingCard";
        pnlIncomingCard.Size = new Size(352, 464);
        pnlIncomingCard.TabIndex = 0;

        // 
        // pnlIncomingHeader
        // 
        pnlIncomingHeader.BackColor = Color.FromArgb(239, 246, 255);
        pnlIncomingHeader.Controls.Add(lblIncomingTitle);
        pnlIncomingHeader.Dock = DockStyle.Top;
        pnlIncomingHeader.Location = new Point(0, 0);
        pnlIncomingHeader.Name = "pnlIncomingHeader";
        pnlIncomingHeader.Padding = new Padding(12, 10, 12, 10);
        pnlIncomingHeader.Size = new Size(352, 42);
        pnlIncomingHeader.TabIndex = 0;

        // 
        // lblIncomingTitle
        // 
        lblIncomingTitle.AutoSize = true;
        lblIncomingTitle.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
        lblIncomingTitle.ForeColor = Color.FromArgb(30, 64, 175);
        lblIncomingTitle.Location = new Point(10, 12);
        lblIncomingTitle.Name = "lblIncomingTitle";
        lblIncomingTitle.Size = new Size(130, 17);
        lblIncomingTitle.TabIndex = 0;
        lblIncomingTitle.Text = "Lời mời đã nhận";

        // 
        // flpIncomingRequests
        // 
        flpIncomingRequests.AutoScroll = true;
        flpIncomingRequests.BackColor = Color.White;
        flpIncomingRequests.Dock = DockStyle.Fill;
        flpIncomingRequests.FlowDirection = FlowDirection.TopDown;
        flpIncomingRequests.Location = new Point(0, 42);
        flpIncomingRequests.Name = "flpIncomingRequests";
        flpIncomingRequests.Padding = new Padding(0, 6, 4, 6);
        flpIncomingRequests.Size = new Size(352, 422);
        flpIncomingRequests.TabIndex = 1;
        flpIncomingRequests.WrapContents = false;

        // 
        // pnlIncomingEmpty
        // 
        pnlIncomingEmpty.BackColor = Color.White;
        pnlIncomingEmpty.Controls.Add(lblIncEmptyIcon);
        pnlIncomingEmpty.Controls.Add(lblIncEmptyTitle);
        pnlIncomingEmpty.Controls.Add(lblIncEmptySub);
        pnlIncomingEmpty.Dock = DockStyle.Fill;
        pnlIncomingEmpty.Location = new Point(0, 42);
        pnlIncomingEmpty.Name = "pnlIncomingEmpty";
        pnlIncomingEmpty.Size = new Size(352, 422);
        pnlIncomingEmpty.TabIndex = 2;
        pnlIncomingEmpty.Visible = false;

        // 
        // lblIncEmptyIcon
        // 
        lblIncEmptyIcon.Anchor = AnchorStyles.None;
        lblIncEmptyIcon.Font = new Font("Segoe MDL2 Assets", 24F);
        lblIncEmptyIcon.ForeColor = Color.FromArgb(203, 213, 225);
        lblIncEmptyIcon.Location = new Point(126, 90);
        lblIncEmptyIcon.Name = "lblIncEmptyIcon";
        lblIncEmptyIcon.Size = new Size(100, 45);
        lblIncEmptyIcon.TabIndex = 0;
        lblIncEmptyIcon.Text = "\uE77B";
        lblIncEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblIncEmptyTitle
        // 
        lblIncEmptyTitle.Anchor = AnchorStyles.None;
        lblIncEmptyTitle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        lblIncEmptyTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblIncEmptyTitle.Location = new Point(26, 140);
        lblIncEmptyTitle.Name = "lblIncEmptyTitle";
        lblIncEmptyTitle.Size = new Size(300, 20);
        lblIncEmptyTitle.TabIndex = 1;
        lblIncEmptyTitle.Text = "Không có lời mời nào";
        lblIncEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblIncEmptySub
        // 
        lblIncEmptySub.Anchor = AnchorStyles.None;
        lblIncEmptySub.Font = new Font("Segoe UI", 8.5F);
        lblIncEmptySub.ForeColor = Color.FromArgb(148, 163, 184);
        lblIncEmptySub.Location = new Point(16, 164);
        lblIncEmptySub.Name = "lblIncEmptySub";
        lblIncEmptySub.Size = new Size(320, 35);
        lblIncEmptySub.TabIndex = 2;
        lblIncEmptySub.Text = "Chưa có lời mời kết bạn nào được gửi tới bạn.";
        lblIncEmptySub.TextAlign = ContentAlignment.TopCenter;

        // 
        // pnlOutgoingCard
        // 
        pnlOutgoingCard.BackColor = Color.White;
        pnlOutgoingCard.Controls.Add(flpOutgoingRequests);
        pnlOutgoingCard.Controls.Add(pnlOutgoingEmpty);
        pnlOutgoingCard.Controls.Add(pnlOutgoingHeader);
        pnlOutgoingCard.Dock = DockStyle.Fill;
        pnlOutgoingCard.Location = new Point(8, 4);
        pnlOutgoingCard.Name = "pnlOutgoingCard";
        pnlOutgoingCard.Size = new Size(363, 464);
        pnlOutgoingCard.TabIndex = 1;

        // 
        // pnlOutgoingHeader
        // 
        pnlOutgoingHeader.BackColor = Color.FromArgb(248, 250, 252);
        pnlOutgoingHeader.Controls.Add(lblOutgoingTitle);
        pnlOutgoingHeader.Dock = DockStyle.Top;
        pnlOutgoingHeader.Location = new Point(0, 0);
        pnlOutgoingHeader.Name = "pnlOutgoingHeader";
        pnlOutgoingHeader.Padding = new Padding(12, 10, 12, 10);
        pnlOutgoingHeader.Size = new Size(363, 42);
        pnlOutgoingHeader.TabIndex = 0;

        // 
        // lblOutgoingTitle
        // 
        lblOutgoingTitle.AutoSize = true;
        lblOutgoingTitle.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
        lblOutgoingTitle.ForeColor = Color.FromArgb(51, 65, 85);
        lblOutgoingTitle.Location = new Point(10, 12);
        lblOutgoingTitle.Name = "lblOutgoingTitle";
        lblOutgoingTitle.Size = new Size(125, 17);
        lblOutgoingTitle.TabIndex = 0;
        lblOutgoingTitle.Text = "Lời mời đã gửi";

        // 
        // flpOutgoingRequests
        // 
        flpOutgoingRequests.AutoScroll = true;
        flpOutgoingRequests.BackColor = Color.White;
        flpOutgoingRequests.Dock = DockStyle.Fill;
        flpOutgoingRequests.FlowDirection = FlowDirection.TopDown;
        flpOutgoingRequests.Location = new Point(0, 42);
        flpOutgoingRequests.Name = "flpOutgoingRequests";
        flpOutgoingRequests.Padding = new Padding(0, 6, 4, 6);
        flpOutgoingRequests.Size = new Size(363, 422);
        flpOutgoingRequests.TabIndex = 1;
        flpOutgoingRequests.WrapContents = false;

        // 
        // pnlOutgoingEmpty
        // 
        pnlOutgoingEmpty.BackColor = Color.White;
        pnlOutgoingEmpty.Controls.Add(lblOutEmptyIcon);
        pnlOutgoingEmpty.Controls.Add(lblOutEmptyTitle);
        pnlOutgoingEmpty.Controls.Add(lblOutEmptySub);
        pnlOutgoingEmpty.Dock = DockStyle.Fill;
        pnlOutgoingEmpty.Location = new Point(0, 42);
        pnlOutgoingEmpty.Name = "pnlOutgoingEmpty";
        pnlOutgoingEmpty.Size = new Size(363, 422);
        pnlOutgoingEmpty.TabIndex = 2;
        pnlOutgoingEmpty.Visible = false;

        // 
        // lblOutEmptyIcon
        // 
        lblOutEmptyIcon.Anchor = AnchorStyles.None;
        lblOutEmptyIcon.Font = new Font("Segoe MDL2 Assets", 24F);
        lblOutEmptyIcon.ForeColor = Color.FromArgb(203, 213, 225);
        lblOutEmptyIcon.Location = new Point(131, 90);
        lblOutEmptyIcon.Name = "lblOutEmptyIcon";
        lblOutEmptyIcon.Size = new Size(100, 45);
        lblOutEmptyIcon.TabIndex = 0;
        lblOutEmptyIcon.Text = "\uE8FA";
        lblOutEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblOutEmptyTitle
        // 
        lblOutEmptyTitle.Anchor = AnchorStyles.None;
        lblOutEmptyTitle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        lblOutEmptyTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblOutEmptyTitle.Location = new Point(31, 140);
        lblOutEmptyTitle.Name = "lblOutEmptyTitle";
        lblOutEmptyTitle.Size = new Size(300, 20);
        lblOutEmptyTitle.TabIndex = 1;
        lblOutEmptyTitle.Text = "Không có lời mời đang chờ";
        lblOutEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblOutEmptySub
        // 
        lblOutEmptySub.Anchor = AnchorStyles.None;
        lblOutEmptySub.Font = new Font("Segoe UI", 8.5F);
        lblOutEmptySub.ForeColor = Color.FromArgb(148, 163, 184);
        lblOutEmptySub.Location = new Point(21, 164);
        lblOutEmptySub.Name = "lblOutEmptySub";
        lblOutEmptySub.Size = new Size(320, 35);
        lblOutEmptySub.TabIndex = 2;
        lblOutEmptySub.Text = "Bạn chưa gửi lời mời kết bạn nào đang chờ phản hồi.";
        lblOutEmptySub.TextAlign = ContentAlignment.TopCenter;

        // 
        // FriendManagerForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 245, 249);
        ClientSize = new Size(1000, 640);
        Controls.Add(pnlBodyContainer);
        Controls.Add(pnlHeader);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        MinimumSize = new Size(880, 560);
        Name = "FriendManagerForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Quản Lý Bạn Bè - Chatting";
        Load += FriendManagerForm_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picRobotMascot).EndInit();
        pnlBodyContainer.ResumeLayout(false);
        pnlCardWrapper.ResumeLayout(false);
        pnlMainCard.ResumeLayout(false);
        pnlTabPages.ResumeLayout(false);
        pnlTabFriendsPage.ResumeLayout(false);
        pnlFriendsEmpty.ResumeLayout(false);
        pnlFriendsTableHeader.ResumeLayout(false);
        pnlFriendsTableHeader.PerformLayout();
        pnlFriendsToolbar.ResumeLayout(false);
        pnlFriendSearchBox.ResumeLayout(false);
        pnlFriendSearchBox.PerformLayout();
        pnlFriendsFooter.ResumeLayout(false);
        pnlFriendsFooter.PerformLayout();
        pnlTabSearchPage.ResumeLayout(false);
        pnlSearchEmpty.ResumeLayout(false);
        pnlSearchTableHeader.ResumeLayout(false);
        pnlSearchTableHeader.PerformLayout();
        pnlSearchToolbar.ResumeLayout(false);
        pnlSearchToolbar.PerformLayout();
        pnlSearchBox.ResumeLayout(false);
        pnlSearchBox.PerformLayout();
        pnlTabRequestsPage.ResumeLayout(false);
        splitRequests.Panel1.ResumeLayout(false);
        splitRequests.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitRequests).EndInit();
        splitRequests.ResumeLayout(false);
        pnlIncomingCard.ResumeLayout(false);
        pnlIncomingEmpty.ResumeLayout(false);
        pnlIncomingHeader.ResumeLayout(false);
        pnlIncomingHeader.PerformLayout();
        pnlOutgoingCard.ResumeLayout(false);
        pnlOutgoingEmpty.ResumeLayout(false);
        pnlOutgoingHeader.ResumeLayout(false);
        pnlOutgoingHeader.PerformLayout();
        pnlTopTabs.ResumeLayout(false);
        pnlLeftNav.ResumeLayout(false);
        pnlLeftBottom.ResumeLayout(false);
        pnlMyStatus.ResumeLayout(false);
        pnlMyStatus.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private PictureBox picRobotMascot;
    private Label lblHeaderTitle;
    private Label lblHeaderSubtitle;

    private Panel pnlBodyContainer;
    private Panel pnlLeftNav;
    private Button btnNavFriends;
    private Button btnNavSearch;
    private Button btnNavRequests;
    private Panel pnlLeftBottom;
    private Panel pnlMyStatus;
    private Label lblMyStatusDot;
    private Label lblMyStatusText;
    private Button btnNavRefresh;

    private Panel pnlCardWrapper;
    private Panel pnlMainCard;
    private Panel pnlTopTabs;
    private Button btnTopTabFriends;
    private Button btnTopTabSearch;
    private Button btnTopTabRequests;
    private Panel pnlTopTabIndicator;
    private Panel pnlTabPages;

    // Tab 1: Friends
    private Panel pnlTabFriendsPage;
    private Panel pnlFriendsToolbar;
    private Panel pnlFriendSearchBox;
    private Label lblFriendSearchIcon;
    private TextBox txtFriendSearch;
    private ComboBox cboFriendFilter;
    private Button btnAddFriendQuick;
    private Panel pnlFriendsTableHeader;
    private Label lblColFriendName;
    private Label lblColFriendEmail;
    private Label lblColFriendStatus;
    private Label lblColFriendAction;
    private FlowLayoutPanel flpFriendsList;
    private Panel pnlFriendsEmpty;
    private Label lblFriendsEmptyIcon;
    private Label lblFriendsEmptyTitle;
    private Label lblFriendsEmptySub;
    private Panel pnlFriendsFooter;
    private Label lblFriendsCount;

    // Tab 2: Search
    private Panel pnlTabSearchPage;
    private Panel pnlSearchToolbar;
    private Label lblSearchPrompt;
    private Panel pnlSearchBox;
    private Label lblSearchBoxIcon;
    private TextBox txtSearchKeyword;
    private Button btnSearch;
    private Panel pnlSearchTableHeader;
    private Label lblColSearchUser;
    private Label lblColSearchEmail;
    private Label lblColSearchRelation;
    private Label lblColSearchAction;
    private FlowLayoutPanel flpSearchResults;
    private Panel pnlSearchEmpty;
    private Label lblSearchEmptyIcon;
    private Label lblSearchEmptyTitle;
    private Label lblSearchEmptySub;

    // Tab 3: Requests
    private Panel pnlTabRequestsPage;
    private SplitContainer splitRequests;
    private Panel pnlIncomingCard;
    private Panel pnlIncomingHeader;
    private Label lblIncomingTitle;
    private FlowLayoutPanel flpIncomingRequests;
    private Panel pnlIncomingEmpty;
    private Label lblIncEmptyIcon;
    private Label lblIncEmptyTitle;
    private Label lblIncEmptySub;

    private Panel pnlOutgoingCard;
    private Panel pnlOutgoingHeader;
    private Label lblOutgoingTitle;
    private FlowLayoutPanel flpOutgoingRequests;
    private Panel pnlOutgoingEmpty;
    private Label lblOutEmptyIcon;
    private Label lblOutEmptyTitle;
    private Label lblOutEmptySub;
}
