namespace Chatting.Client.Forms;

partial class MainMessengerForm
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
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMessengerForm));
        pnlTitleBar = new Panel();
        btnCloseWindow = new Button();
        btnMaximize = new Button();
        btnMinimize = new Button();
        lblAppTitle = new Label();
        lblTitleLogo = new Label();
        pnlNavStrip = new Panel();
        lblLanIndicator = new Label();
        btnNavSettings = new Button();
        btnNavFiles = new Button();
        btnNavCalls = new Button();
        btnNavChat = new Button();
        pnlSidebar = new Panel();
        flpConversations = new FlowLayoutPanel();
        pnlSearch = new Panel();
        txtSearchConversations = new TextBox();
        pnlUserHeader = new Panel();
        btnLogout = new Button();
        btnNewGroup = new Button();
        btnOpenFriends = new Button();
        lblMyStatus = new Label();
        lblMyUsername = new Label();
        picMyAvatar = new PictureBox();
        pnlSidebarBorder = new Panel();
        pnlMainContainer = new Panel();
        pnlChatArea = new Panel();
        pnlToast = new Panel();
        lblToastMessage = new Label();
        pnlMoreMenu = new Panel();
        itemRefresh = new Panel();
        lblRefreshSub = new Label();
        lblRefreshTitle = new Label();
        lblRefreshIcon = new Label();
        itemBlock = new Panel();
        lblBlockSub = new Label();
        lblBlockTitle = new Label();
        lblBlockIcon = new Label();
        itemDelete = new Panel();
        lblDeleteSub = new Label();
        lblDeleteTitle = new Label();
        lblDeleteIcon = new Label();
        itemSearch = new Panel();
        lblSearchSub = new Label();
        lblSearchTitle = new Label();
        lblSearchIcon = new Label();
        itemUserInfo = new Panel();
        lblUserInfoSub = new Label();
        lblUserInfoTitle = new Label();
        lblUserInfoIcon = new Label();
        flpMessages = new FlowLayoutPanel();
        pnlEmptyState = new Panel();
        lblEmptyState = new Label();
        lblEmptyStateHint = new Label();
        lblTypingIndicator = new Label();
        pnlBlockedBanner = new Panel();
        lblBlockedNotice = new Label();
        btnBlockedUnblock = new Button();
        pnlMessageSearch = new Panel();
        pnlSearchInputContainer = new Panel();
        btnSearchClear = new Button();
        btnCloseSearch = new Button();
        btnSearchNext = new Button();
        btnSearchPrev = new Button();
        lblSearchMatchCount = new Label();
        txtSearchMessageKeyword = new TextBox();
        lblSearchIconHeader = new Label();
        pnlCallHistory = new Panel();
        pnlInputBar = new Panel();
        pnlInputContainer = new Panel();
        btnSendMessage = new Button();
        txtMessageInput = new TextBox();
        btnRecordVoice = new Button();
        btnAttachFile = new Button();
        btnEmoji = new Button();
        pnlChatHeader = new Panel();
        pnlHeaderButtons = new Panel();
        btnMoreMenu = new Button();
        btnAttachFileHeader = new Button();
        btnGroupInfo = new Button();
        btnVideoCall = new Button();
        btnVoiceCall = new Button();
        btnSearch = new Button();
        lblChatStatus = new Label();
        lblChatTitle = new Label();
        picChatAvatar = new PictureBox();
        openFileDialog = new OpenFileDialog();
        notifyIconTray = new NotifyIcon(components);
        ctxTrayMenu = new ContextMenuStrip(components);
        menuTrayOpen = new ToolStripMenuItem();
        menuTrayLogout = new ToolStripMenuItem();
        menuTrayExit = new ToolStripMenuItem();
        toolTipMain = new ToolTip(components);
        picAiRobotBubble = new PictureBox();
        tmrAiFloat = new System.Windows.Forms.Timer(components);
        pnlTitleBar.SuspendLayout();
        pnlNavStrip.SuspendLayout();
        pnlSidebar.SuspendLayout();
        pnlSearch.SuspendLayout();
        pnlUserHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picMyAvatar).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picAiRobotBubble).BeginInit();
        pnlMainContainer.SuspendLayout();
        pnlCallHistory.SuspendLayout();
        pnlChatArea.SuspendLayout();
        pnlToast.SuspendLayout();
        pnlMoreMenu.SuspendLayout();
        itemRefresh.SuspendLayout();
        itemBlock.SuspendLayout();
        itemDelete.SuspendLayout();
        itemSearch.SuspendLayout();
        itemUserInfo.SuspendLayout();
        pnlEmptyState.SuspendLayout();
        pnlBlockedBanner.SuspendLayout();
        pnlMessageSearch.SuspendLayout();
        pnlSearchInputContainer.SuspendLayout();
        pnlInputBar.SuspendLayout();
        pnlChatHeader.SuspendLayout();
        pnlHeaderButtons.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picChatAvatar).BeginInit();
        ctxTrayMenu.SuspendLayout();
        SuspendLayout();
        // 
        // pnlTitleBar
        // 
        pnlTitleBar.BackColor = Color.FromArgb(13, 21, 39);
        pnlTitleBar.Controls.Add(btnCloseWindow);
        pnlTitleBar.Controls.Add(btnMaximize);
        pnlTitleBar.Controls.Add(btnMinimize);
        pnlTitleBar.Controls.Add(lblAppTitle);
        pnlTitleBar.Controls.Add(lblTitleLogo);
        pnlTitleBar.Dock = DockStyle.Top;
        pnlTitleBar.Location = new Point(0, 0);
        pnlTitleBar.Name = "pnlTitleBar";
        pnlTitleBar.Size = new Size(1250, 36);
        pnlTitleBar.TabIndex = 3;
        // 
        // btnCloseWindow
        // 
        btnCloseWindow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCloseWindow.BackColor = Color.Transparent;
        btnCloseWindow.FlatAppearance.BorderSize = 0;
        btnCloseWindow.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 68, 68);
        btnCloseWindow.FlatStyle = FlatStyle.Flat;
        btnCloseWindow.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnCloseWindow.ForeColor = Color.FromArgb(148, 163, 184);
        btnCloseWindow.Location = new Point(1205, 0);
        btnCloseWindow.Name = "btnCloseWindow";
        btnCloseWindow.Size = new Size(45, 36);
        btnCloseWindow.TabIndex = 4;
        btnCloseWindow.Text = "\uE711";
        btnCloseWindow.UseVisualStyleBackColor = false;
        btnCloseWindow.Click += btnCloseWindow_Click;
        // 
        // btnMaximize
        // 
        btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMaximize.BackColor = Color.Transparent;
        btnMaximize.FlatAppearance.BorderSize = 0;
        btnMaximize.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnMaximize.FlatStyle = FlatStyle.Flat;
        btnMaximize.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnMaximize.ForeColor = Color.FromArgb(148, 163, 184);
        btnMaximize.Location = new Point(1160, 0);
        btnMaximize.Name = "btnMaximize";
        btnMaximize.Size = new Size(45, 36);
        btnMaximize.TabIndex = 3;
        btnMaximize.Text = "\uE922";
        btnMaximize.UseVisualStyleBackColor = false;
        btnMaximize.Click += btnMaximize_Click;
        // 
        // btnMinimize
        // 
        btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMinimize.BackColor = Color.Transparent;
        btnMinimize.FlatAppearance.BorderSize = 0;
        btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnMinimize.FlatStyle = FlatStyle.Flat;
        btnMinimize.Font = new Font("Segoe MDL2 Assets", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnMinimize.ForeColor = Color.FromArgb(148, 163, 184);
        btnMinimize.Location = new Point(1115, 0);
        btnMinimize.Name = "btnMinimize";
        btnMinimize.Size = new Size(45, 36);
        btnMinimize.TabIndex = 2;
        btnMinimize.Text = "\uE921";
        btnMinimize.UseVisualStyleBackColor = false;
        btnMinimize.Click += btnMinimize_Click;
        // 
        // lblAppTitle
        // 
        lblAppTitle.AutoSize = true;
        lblAppTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblAppTitle.ForeColor = Color.FromArgb(241, 245, 249);
        lblAppTitle.Location = new Point(44, 9);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Size = new Size(183, 17);
        lblAppTitle.TabIndex = 1;
        lblAppTitle.Text = "Chatting — LAN Messenger";
        // 
        // lblTitleLogo
        // 
        lblTitleLogo.AutoSize = true;
        lblTitleLogo.Font = new Font("Segoe MDL2 Assets", 12F, FontStyle.Regular, GraphicsUnit.Point);
        lblTitleLogo.ForeColor = Color.FromArgb(59, 130, 246);
        lblTitleLogo.Location = new Point(14, 9);
        lblTitleLogo.Name = "lblTitleLogo";
        lblTitleLogo.Size = new Size(24, 21);
        lblTitleLogo.TabIndex = 0;
        lblTitleLogo.Text = "\uE8BD";
        // 
        // pnlNavStrip
        // 
        pnlNavStrip.BackColor = Color.FromArgb(11, 18, 32);
        pnlNavStrip.Controls.Add(picAiRobotBubble);
        pnlNavStrip.Controls.Add(lblLanIndicator);
        pnlNavStrip.Controls.Add(btnNavSettings);
        pnlNavStrip.Controls.Add(btnNavFiles);
        pnlNavStrip.Controls.Add(btnNavCalls);
        pnlNavStrip.Controls.Add(btnNavChat);
        pnlNavStrip.Dock = DockStyle.Left;
        pnlNavStrip.Location = new Point(0, 36);
        pnlNavStrip.Name = "pnlNavStrip";
        pnlNavStrip.Size = new Size(68, 764);
        pnlNavStrip.TabIndex = 4;
        // 
        // lblLanIndicator
        // 
        lblLanIndicator.Dock = DockStyle.Bottom;
        lblLanIndicator.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point);
        lblLanIndicator.ForeColor = Color.FromArgb(34, 197, 94);
        lblLanIndicator.Location = new Point(0, 720);
        lblLanIndicator.Name = "lblLanIndicator";
        lblLanIndicator.Size = new Size(68, 44);
        lblLanIndicator.TabIndex = 4;
        lblLanIndicator.Text = "● LAN\r\nOnline";
        lblLanIndicator.TextAlign = ContentAlignment.MiddleCenter;
        toolTipMain.SetToolTip(lblLanIndicator, "Đang kết nối trong mạng LAN");
        // 
        // btnNavSettings
        // 
        btnNavSettings.Cursor = Cursors.Hand;
        btnNavSettings.FlatAppearance.BorderSize = 0;
        btnNavSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnNavSettings.FlatStyle = FlatStyle.Flat;
        btnNavSettings.Font = new Font("Segoe UI", 8F);
        btnNavSettings.ForeColor = Color.FromArgb(148, 163, 184);
        btnNavSettings.Location = new Point(6, 215);
        btnNavSettings.Name = "btnNavSettings";
        btnNavSettings.Size = new Size(56, 56);
        btnNavSettings.TabIndex = 3;
        btnNavSettings.Text = "Cài đặt";
        toolTipMain.SetToolTip(btnNavSettings, "Cài đặt & Máy chủ");
        btnNavSettings.UseVisualStyleBackColor = true;
        btnNavSettings.Click += btnNavSettings_Click;
        // 
        // btnNavFiles
        // 
        btnNavFiles.Cursor = Cursors.Hand;
        btnNavFiles.FlatAppearance.BorderSize = 0;
        btnNavFiles.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnNavFiles.FlatStyle = FlatStyle.Flat;
        btnNavFiles.Font = new Font("Segoe UI", 8F);
        btnNavFiles.ForeColor = Color.FromArgb(148, 163, 184);
        btnNavFiles.Location = new Point(6, 150);
        btnNavFiles.Name = "btnNavFiles";
        btnNavFiles.Size = new Size(56, 56);
        btnNavFiles.TabIndex = 2;
        btnNavFiles.Text = "Tệp tin";
        toolTipMain.SetToolTip(btnNavFiles, "Gửi & Quản lý tệp tin");
        btnNavFiles.UseVisualStyleBackColor = true;
        btnNavFiles.Click += btnNavFiles_Click;
        // 
        // btnNavCalls
        // 
        btnNavCalls.Cursor = Cursors.Hand;
        btnNavCalls.FlatAppearance.BorderSize = 0;
        btnNavCalls.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnNavCalls.FlatStyle = FlatStyle.Flat;
        btnNavCalls.Font = new Font("Segoe UI", 8F);
        btnNavCalls.ForeColor = Color.FromArgb(148, 163, 184);
        btnNavCalls.Location = new Point(6, 85);
        btnNavCalls.Name = "btnNavCalls";
        btnNavCalls.Size = new Size(56, 56);
        btnNavCalls.TabIndex = 1;
        btnNavCalls.Text = "Cuộc gọi";
        toolTipMain.SetToolTip(btnNavCalls, "Cuộc gọi mạng LAN");
        btnNavCalls.UseVisualStyleBackColor = true;
        btnNavCalls.Click += btnNavCalls_Click;
        // 
        // btnNavChat
        // 
        btnNavChat.BackColor = Color.FromArgb(37, 99, 235);
        btnNavChat.Cursor = Cursors.Hand;
        btnNavChat.FlatAppearance.BorderSize = 0;
        btnNavChat.FlatStyle = FlatStyle.Flat;
        btnNavChat.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        btnNavChat.ForeColor = Color.White;
        btnNavChat.Location = new Point(6, 20);
        btnNavChat.Name = "btnNavChat";
        btnNavChat.Size = new Size(56, 56);
        btnNavChat.TabIndex = 0;
        btnNavChat.Text = "Chat";
        toolTipMain.SetToolTip(btnNavChat, "Đoạn chat");
        btnNavChat.UseVisualStyleBackColor = false;
        btnNavChat.Click += btnNavChat_Click;
        // 
        // pnlSidebar
        // 
        pnlSidebar.BackColor = Color.FromArgb(17, 28, 51);
        pnlSidebar.Controls.Add(flpConversations);
        pnlSidebar.Controls.Add(pnlSearch);
        pnlSidebar.Controls.Add(pnlUserHeader);
        pnlSidebar.Dock = DockStyle.Left;
        pnlSidebar.Location = new Point(68, 36);
        pnlSidebar.Name = "pnlSidebar";
        pnlSidebar.Size = new Size(270, 764);
        pnlSidebar.TabIndex = 0;
        // 
        // flpConversations
        // 
        flpConversations.AutoScroll = true;
        flpConversations.BackColor = Color.FromArgb(17, 28, 51);
        flpConversations.Dock = DockStyle.Fill;
        flpConversations.FlowDirection = FlowDirection.TopDown;
        flpConversations.Location = new Point(0, 130);
        flpConversations.Name = "flpConversations";
        flpConversations.Padding = new Padding(8, 4, 8, 8);
        flpConversations.Size = new Size(270, 634);
        flpConversations.TabIndex = 2;
        flpConversations.WrapContents = false;
        // 
        // pnlSearch
        // 
        pnlSearch.BackColor = Color.FromArgb(17, 28, 51);
        pnlSearch.Controls.Add(txtSearchConversations);
        pnlSearch.Dock = DockStyle.Top;
        pnlSearch.Location = new Point(0, 80);
        pnlSearch.Name = "pnlSearch";
        pnlSearch.Padding = new Padding(12, 8, 12, 8);
        pnlSearch.Size = new Size(270, 50);
        pnlSearch.TabIndex = 1;
        // 
        // txtSearchConversations
        // 
        txtSearchConversations.BackColor = Color.FromArgb(30, 43, 69);
        txtSearchConversations.BorderStyle = BorderStyle.None;
        txtSearchConversations.Dock = DockStyle.Fill;
        txtSearchConversations.Font = new Font("Segoe UI", 9.5F);
        txtSearchConversations.ForeColor = Color.FromArgb(226, 232, 240);
        txtSearchConversations.Location = new Point(12, 8);
        txtSearchConversations.Name = "txtSearchConversations";
        txtSearchConversations.PlaceholderText = "Tìm kiếm hội thoại...";
        txtSearchConversations.Size = new Size(246, 17);
        txtSearchConversations.TabIndex = 0;
        txtSearchConversations.TextChanged += txtSearchConversations_TextChanged;
        // 
        // pnlUserHeader
        // 
        pnlUserHeader.BackColor = Color.FromArgb(14, 23, 42);
        pnlUserHeader.Controls.Add(btnLogout);
        pnlUserHeader.Controls.Add(btnNewGroup);
        pnlUserHeader.Controls.Add(btnOpenFriends);
        pnlUserHeader.Controls.Add(lblMyStatus);
        pnlUserHeader.Controls.Add(lblMyUsername);
        pnlUserHeader.Controls.Add(picMyAvatar);
        pnlUserHeader.Dock = DockStyle.Top;
        pnlUserHeader.Location = new Point(0, 0);
        pnlUserHeader.Name = "pnlUserHeader";
        pnlUserHeader.Padding = new Padding(12, 0, 8, 0);
        pnlUserHeader.Size = new Size(270, 80);
        pnlUserHeader.TabIndex = 0;
        // 
        // btnLogout
        // 
        btnLogout.BackColor = Color.FromArgb(30, 43, 69);
        btnLogout.Cursor = Cursors.Hand;
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(185, 28, 28);
        btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);
        btnLogout.FlatStyle = FlatStyle.Flat;
        btnLogout.Font = new Font("Segoe MDL2 Assets", 11F);
        btnLogout.ForeColor = Color.FromArgb(248, 113, 113);
        btnLogout.Location = new Point(230, 24);
        btnLogout.Name = "btnLogout";
        btnLogout.Size = new Size(32, 34);
        btnLogout.TabIndex = 5;
        btnLogout.Text = "\uE8AC";
        toolTipMain.SetToolTip(btnLogout, "Đăng xuất tài khoản");
        btnLogout.UseVisualStyleBackColor = false;
        btnLogout.Click += btnLogout_Click;
        // 
        // btnNewGroup
        // 
        btnNewGroup.BackColor = Color.FromArgb(30, 43, 69);
        btnNewGroup.Cursor = Cursors.Hand;
        btnNewGroup.FlatAppearance.BorderSize = 0;
        btnNewGroup.FlatAppearance.MouseDownBackColor = Color.FromArgb(5, 150, 105);
        btnNewGroup.FlatAppearance.MouseOverBackColor = Color.FromArgb(16, 185, 129);
        btnNewGroup.FlatStyle = FlatStyle.Flat;
        btnNewGroup.Font = new Font("Segoe MDL2 Assets", 11F);
        btnNewGroup.ForeColor = Color.FromArgb(148, 163, 184);
        btnNewGroup.Location = new Point(192, 24);
        btnNewGroup.Name = "btnNewGroup";
        btnNewGroup.Size = new Size(32, 34);
        btnNewGroup.TabIndex = 4;
        btnNewGroup.Text = "\uE902";
        toolTipMain.SetToolTip(btnNewGroup, "Tạo nhóm chat mới");
        btnNewGroup.UseVisualStyleBackColor = false;
        btnNewGroup.Click += btnNewGroup_Click;
        // 
        // btnOpenFriends
        // 
        btnOpenFriends.BackColor = Color.FromArgb(30, 43, 69);
        btnOpenFriends.Cursor = Cursors.Hand;
        btnOpenFriends.FlatAppearance.BorderSize = 0;
        btnOpenFriends.FlatAppearance.MouseDownBackColor = Color.FromArgb(37, 99, 235);
        btnOpenFriends.FlatAppearance.MouseOverBackColor = Color.FromArgb(59, 130, 246);
        btnOpenFriends.FlatStyle = FlatStyle.Flat;
        btnOpenFriends.Font = new Font("Segoe MDL2 Assets", 11F);
        btnOpenFriends.ForeColor = Color.FromArgb(148, 163, 184);
        btnOpenFriends.Location = new Point(154, 24);
        btnOpenFriends.Name = "btnOpenFriends";
        btnOpenFriends.Size = new Size(32, 34);
        btnOpenFriends.TabIndex = 3;
        btnOpenFriends.Text = "\uE77B";
        toolTipMain.SetToolTip(btnOpenFriends, "Quản lý bạn bè");
        btnOpenFriends.UseVisualStyleBackColor = false;
        btnOpenFriends.Click += btnOpenFriends_Click;
        // 
        // lblMyStatus
        // 
        lblMyStatus.AutoSize = true;
        lblMyStatus.Font = new Font("Segoe UI", 7.5F);
        lblMyStatus.ForeColor = Color.FromArgb(34, 197, 94);
        lblMyStatus.Location = new Point(56, 42);
        lblMyStatus.Name = "lblMyStatus";
        lblMyStatus.Size = new Size(93, 12);
        lblMyStatus.TabIndex = 2;
        lblMyStatus.Text = "● Đang trực tuyến";
        // 
        // lblMyUsername
        // 
        lblMyUsername.AutoEllipsis = true;
        lblMyUsername.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        lblMyUsername.ForeColor = Color.FromArgb(248, 250, 252);
        lblMyUsername.Location = new Point(56, 22);
        lblMyUsername.Name = "lblMyUsername";
        lblMyUsername.Size = new Size(95, 18);
        lblMyUsername.TabIndex = 1;
        lblMyUsername.Text = "Username";
        // 
        // picMyAvatar
        // 
        picMyAvatar.BackColor = Color.FromArgb(147, 51, 234);
        picMyAvatar.Cursor = Cursors.Hand;
        picMyAvatar.Location = new Point(12, 20);
        picMyAvatar.Name = "picMyAvatar";
        picMyAvatar.Size = new Size(38, 38);
        picMyAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picMyAvatar.TabIndex = 0;
        picMyAvatar.TabStop = false;
        toolTipMain.SetToolTip(picMyAvatar, "Nhấn để đổi ảnh đại diện");
        // 
        // pnlSidebarBorder
        // 
        pnlSidebarBorder.BackColor = Color.FromArgb(30, 43, 69);
        pnlSidebarBorder.Dock = DockStyle.Left;
        pnlSidebarBorder.Location = new Point(338, 36);
        pnlSidebarBorder.Name = "pnlSidebarBorder";
        pnlSidebarBorder.Size = new Size(1, 764);
        pnlSidebarBorder.TabIndex = 2;
        // 
        // pnlMainContainer
        // 
        pnlMainContainer.BackColor = Color.White;
        pnlMainContainer.Controls.Add(pnlCallHistory);
        pnlMainContainer.Controls.Add(pnlChatArea);
        pnlMainContainer.Dock = DockStyle.Fill;
        pnlMainContainer.Location = new Point(339, 36);
        pnlMainContainer.Name = "pnlMainContainer";
        pnlMainContainer.Size = new Size(911, 764);
        pnlMainContainer.TabIndex = 1;
        // 
        // pnlCallHistory
        // 
        pnlCallHistory.BackColor = Color.White;
        pnlCallHistory.Dock = DockStyle.Fill;
        pnlCallHistory.Location = new Point(0, 0);
        pnlCallHistory.Name = "pnlCallHistory";
        pnlCallHistory.Size = new Size(911, 764);
        pnlCallHistory.TabIndex = 0;
        pnlCallHistory.Visible = false;
        // 
        // pnlChatArea
        // 
        pnlChatArea.BackColor = Color.White;
        pnlChatArea.Controls.Add(pnlToast);
        pnlChatArea.Controls.Add(pnlMoreMenu);
        pnlChatArea.Controls.Add(flpMessages);
        pnlChatArea.Controls.Add(pnlEmptyState);
        pnlChatArea.Controls.Add(lblTypingIndicator);
        pnlChatArea.Controls.Add(pnlBlockedBanner);
        pnlChatArea.Controls.Add(pnlMessageSearch);
        pnlChatArea.Controls.Add(pnlInputBar);
        pnlChatArea.Controls.Add(pnlChatHeader);
        pnlChatArea.Dock = DockStyle.Fill;
        pnlChatArea.Location = new Point(0, 0);
        pnlChatArea.Name = "pnlChatArea";
        pnlChatArea.Size = new Size(911, 764);
        pnlChatArea.TabIndex = 1;
        // 
        // pnlToast
        // 
        pnlToast.Anchor = AnchorStyles.Top;
        pnlToast.BackColor = Color.FromArgb(15, 23, 42);
        pnlToast.Controls.Add(lblToastMessage);
        pnlToast.Location = new Point(310, 75);
        pnlToast.Name = "pnlToast";
        pnlToast.Size = new Size(290, 36);
        pnlToast.TabIndex = 8;
        pnlToast.Visible = false;
        // 
        // lblToastMessage
        // 
        lblToastMessage.Dock = DockStyle.Fill;
        lblToastMessage.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        lblToastMessage.ForeColor = Color.White;
        lblToastMessage.Location = new Point(0, 0);
        lblToastMessage.Name = "lblToastMessage";
        lblToastMessage.Size = new Size(290, 36);
        lblToastMessage.TabIndex = 0;
        lblToastMessage.Text = "Đã làm mới cuộc trò chuyện";
        lblToastMessage.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // pnlMoreMenu
        // 
        pnlMoreMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pnlMoreMenu.BackColor = Color.White;
        pnlMoreMenu.Controls.Add(itemRefresh);
        pnlMoreMenu.Controls.Add(itemBlock);
        pnlMoreMenu.Controls.Add(itemDelete);
        pnlMoreMenu.Controls.Add(itemSearch);
        pnlMoreMenu.Controls.Add(itemUserInfo);
        pnlMoreMenu.Location = new Point(660, 68);
        pnlMoreMenu.Name = "pnlMoreMenu";
        pnlMoreMenu.Padding = new Padding(4);
        pnlMoreMenu.Size = new Size(235, 230);
        pnlMoreMenu.TabIndex = 7;
        pnlMoreMenu.Visible = false;
        // 
        // itemRefresh
        // 
        itemRefresh.Controls.Add(lblRefreshSub);
        itemRefresh.Controls.Add(lblRefreshTitle);
        itemRefresh.Controls.Add(lblRefreshIcon);
        itemRefresh.Cursor = Cursors.Hand;
        itemRefresh.Dock = DockStyle.Top;
        itemRefresh.Location = new Point(4, 180);
        itemRefresh.Name = "itemRefresh";
        itemRefresh.Size = new Size(227, 44);
        itemRefresh.TabIndex = 4;
        // 
        // lblRefreshSub
        // 
        lblRefreshSub.AutoSize = true;
        lblRefreshSub.Font = new Font("Segoe UI", 7.5F);
        lblRefreshSub.ForeColor = Color.FromArgb(100, 116, 139);
        lblRefreshSub.Location = new Point(36, 23);
        lblRefreshSub.Name = "lblRefreshSub";
        lblRefreshSub.Size = new Size(89, 12);
        lblRefreshSub.TabIndex = 2;
        lblRefreshSub.Text = "Tải lại nội dung chat";
        // 
        // lblRefreshTitle
        // 
        lblRefreshTitle.AutoSize = true;
        lblRefreshTitle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblRefreshTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblRefreshTitle.Location = new Point(36, 6);
        lblRefreshTitle.Name = "lblRefreshTitle";
        lblRefreshTitle.Size = new Size(137, 15);
        lblRefreshTitle.TabIndex = 1;
        lblRefreshTitle.Text = "Làm mới cuộc trò chuyện";
        // 
        // lblRefreshIcon
        // 
        lblRefreshIcon.AutoSize = true;
        lblRefreshIcon.Font = new Font("Segoe MDL2 Assets", 11F);
        lblRefreshIcon.Location = new Point(8, 12);
        lblRefreshIcon.Name = "lblRefreshIcon";
        lblRefreshIcon.Size = new Size(23, 20);
        lblRefreshIcon.TabIndex = 0;
        lblRefreshIcon.Text = "\uE72C";
        // 
        // itemBlock
        // 
        itemBlock.Controls.Add(lblBlockSub);
        itemBlock.Controls.Add(lblBlockTitle);
        itemBlock.Controls.Add(lblBlockIcon);
        itemBlock.Cursor = Cursors.Hand;
        itemBlock.Dock = DockStyle.Top;
        itemBlock.Location = new Point(4, 136);
        itemBlock.Name = "itemBlock";
        itemBlock.Size = new Size(227, 44);
        itemBlock.TabIndex = 3;
        // 
        // lblBlockSub
        // 
        lblBlockSub.AutoSize = true;
        lblBlockSub.Font = new Font("Segoe UI", 7.5F);
        lblBlockSub.ForeColor = Color.FromArgb(100, 116, 139);
        lblBlockSub.Location = new Point(36, 23);
        lblBlockSub.Name = "lblBlockSub";
        lblBlockSub.Size = new Size(153, 12);
        lblBlockSub.TabIndex = 2;
        lblBlockSub.Text = "Ngừng nhận tin nhắn từ người này";
        // 
        // lblBlockTitle
        // 
        lblBlockTitle.AutoSize = true;
        lblBlockTitle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblBlockTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblBlockTitle.Location = new Point(36, 6);
        lblBlockTitle.Name = "lblBlockTitle";
        lblBlockTitle.Size = new Size(99, 15);
        lblBlockTitle.TabIndex = 1;
        lblBlockTitle.Text = "Chặn người dùng";
        // 
        // lblBlockIcon
        // 
        lblBlockIcon.AutoSize = true;
        lblBlockIcon.Font = new Font("Segoe MDL2 Assets", 11F);
        lblBlockIcon.Location = new Point(8, 12);
        lblBlockIcon.Name = "lblBlockIcon";
        lblBlockIcon.Size = new Size(23, 20);
        lblBlockIcon.TabIndex = 0;
        lblBlockIcon.Text = "\uF140";
        // 
        // itemDelete
        // 
        itemDelete.Controls.Add(lblDeleteSub);
        itemDelete.Controls.Add(lblDeleteTitle);
        itemDelete.Controls.Add(lblDeleteIcon);
        itemDelete.Cursor = Cursors.Hand;
        itemDelete.Dock = DockStyle.Top;
        itemDelete.Location = new Point(4, 92);
        itemDelete.Name = "itemDelete";
        itemDelete.Size = new Size(227, 44);
        itemDelete.TabIndex = 2;
        // 
        // lblDeleteSub
        // 
        lblDeleteSub.AutoSize = true;
        lblDeleteSub.Font = new Font("Segoe UI", 7.5F);
        lblDeleteSub.ForeColor = Color.FromArgb(100, 116, 139);
        lblDeleteSub.Location = new Point(36, 23);
        lblDeleteSub.Name = "lblDeleteSub";
        lblDeleteSub.Size = new Size(68, 12);
        lblDeleteSub.TabIndex = 2;
        lblDeleteSub.Text = "Xóa lịch sử chat";
        // 
        // lblDeleteTitle
        // 
        lblDeleteTitle.AutoSize = true;
        lblDeleteTitle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblDeleteTitle.ForeColor = Color.FromArgb(220, 38, 38);
        lblDeleteTitle.Location = new Point(36, 6);
        lblDeleteTitle.Name = "lblDeleteTitle";
        lblDeleteTitle.Size = new Size(111, 15);
        lblDeleteTitle.TabIndex = 1;
        lblDeleteTitle.Text = "Xóa cuộc trò chuyện";
        // 
        // lblDeleteIcon
        // 
        lblDeleteIcon.AutoSize = true;
        lblDeleteIcon.Font = new Font("Segoe MDL2 Assets", 11F);
        lblDeleteIcon.Location = new Point(8, 12);
        lblDeleteIcon.Name = "lblDeleteIcon";
        lblDeleteIcon.Size = new Size(23, 20);
        lblDeleteIcon.TabIndex = 0;
        lblDeleteIcon.Text = "\uE74D";
        // 
        // itemSearch
        // 
        itemSearch.Controls.Add(lblSearchSub);
        itemSearch.Controls.Add(lblSearchTitle);
        itemSearch.Controls.Add(lblSearchIcon);
        itemSearch.Cursor = Cursors.Hand;
        itemSearch.Dock = DockStyle.Top;
        itemSearch.Location = new Point(4, 48);
        itemSearch.Name = "itemSearch";
        itemSearch.Size = new Size(227, 44);
        itemSearch.TabIndex = 1;
        // 
        // lblSearchSub
        // 
        lblSearchSub.AutoSize = true;
        lblSearchSub.Font = new Font("Segoe UI", 7.5F);
        lblSearchSub.ForeColor = Color.FromArgb(100, 116, 139);
        lblSearchSub.Location = new Point(36, 23);
        lblSearchSub.Name = "lblSearchSub";
        lblSearchSub.Size = new Size(160, 12);
        lblSearchSub.TabIndex = 2;
        lblSearchSub.Text = "Tìm nội dung trong cuộc trò chuyện";
        // 
        // lblSearchTitle
        // 
        lblSearchTitle.AutoSize = true;
        lblSearchTitle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblSearchTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblSearchTitle.Location = new Point(36, 6);
        lblSearchTitle.Name = "lblSearchTitle";
        lblSearchTitle.Size = new Size(100, 15);
        lblSearchTitle.TabIndex = 1;
        lblSearchTitle.Text = "Tìm kiếm tin nhắn";
        // 
        // lblSearchIcon
        // 
        lblSearchIcon.AutoSize = true;
        lblSearchIcon.Font = new Font("Segoe MDL2 Assets", 11F);
        lblSearchIcon.Location = new Point(8, 12);
        lblSearchIcon.Name = "lblSearchIcon";
        lblSearchIcon.Size = new Size(23, 20);
        lblSearchIcon.TabIndex = 0;
        lblSearchIcon.Text = "\uE721";
        // 
        // itemUserInfo
        // 
        itemUserInfo.Controls.Add(lblUserInfoSub);
        itemUserInfo.Controls.Add(lblUserInfoTitle);
        itemUserInfo.Controls.Add(lblUserInfoIcon);
        itemUserInfo.Cursor = Cursors.Hand;
        itemUserInfo.Dock = DockStyle.Top;
        itemUserInfo.Location = new Point(4, 4);
        itemUserInfo.Name = "itemUserInfo";
        itemUserInfo.Size = new Size(227, 44);
        itemUserInfo.TabIndex = 0;
        // 
        // lblUserInfoSub
        // 
        lblUserInfoSub.AutoSize = true;
        lblUserInfoSub.Font = new Font("Segoe UI", 7.5F);
        lblUserInfoSub.ForeColor = Color.FromArgb(100, 116, 139);
        lblUserInfoSub.Location = new Point(36, 23);
        lblUserInfoSub.Name = "lblUserInfoSub";
        lblUserInfoSub.Size = new Size(95, 12);
        lblUserInfoSub.TabIndex = 2;
        lblUserInfoSub.Text = "Xem thông tin liên hệ";
        // 
        // lblUserInfoTitle
        // 
        lblUserInfoTitle.AutoSize = true;
        lblUserInfoTitle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
        lblUserInfoTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblUserInfoTitle.Location = new Point(36, 6);
        lblUserInfoTitle.Name = "lblUserInfoTitle";
        lblUserInfoTitle.Size = new Size(122, 15);
        lblUserInfoTitle.TabIndex = 1;
        lblUserInfoTitle.Text = "Thông tin người dùng";
        // 
        // lblUserInfoIcon
        // 
        lblUserInfoIcon.AutoSize = true;
        lblUserInfoIcon.Font = new Font("Segoe MDL2 Assets", 11F);
        lblUserInfoIcon.Location = new Point(8, 12);
        lblUserInfoIcon.Name = "lblUserInfoIcon";
        lblUserInfoIcon.Size = new Size(23, 20);
        lblUserInfoIcon.TabIndex = 0;
        lblUserInfoIcon.Text = "\uE77B";
        // 
        // flpMessages
        // 
        flpMessages.AutoScroll = true;
        flpMessages.BackColor = Color.White;
        flpMessages.Dock = DockStyle.Fill;
        flpMessages.FlowDirection = FlowDirection.TopDown;
        flpMessages.Location = new Point(0, 68);
        flpMessages.Name = "flpMessages";
        flpMessages.Padding = new Padding(12, 10, 12, 10);
        flpMessages.Size = new Size(911, 608);
        flpMessages.TabIndex = 2;
        flpMessages.Visible = false;
        flpMessages.WrapContents = false;
        // 
        // pnlEmptyState
        // 
        pnlEmptyState.BackColor = Color.White;
        pnlEmptyState.Controls.Add(lblEmptyState);
        pnlEmptyState.Controls.Add(lblEmptyStateHint);
        pnlEmptyState.Dock = DockStyle.Fill;
        pnlEmptyState.Location = new Point(0, 68);
        pnlEmptyState.Name = "pnlEmptyState";
        pnlEmptyState.Size = new Size(911, 608);
        pnlEmptyState.TabIndex = 5;
        // 
        // lblEmptyState
        // 
        lblEmptyState.Font = new Font("Segoe MDL2 Assets", 48F);
        lblEmptyState.ForeColor = Color.FromArgb(203, 213, 225);
        lblEmptyState.Location = new Point(0, 180);
        lblEmptyState.Name = "lblEmptyState";
        lblEmptyState.Size = new Size(911, 100);
        lblEmptyState.TabIndex = 0;
        lblEmptyState.Text = "\uE8BD";
        lblEmptyState.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblEmptyStateHint
        // 
        lblEmptyStateHint.Font = new Font("Segoe UI", 13F);
        lblEmptyStateHint.ForeColor = Color.FromArgb(148, 163, 184);
        lblEmptyStateHint.Location = new Point(0, 285);
        lblEmptyStateHint.Name = "lblEmptyStateHint";
        lblEmptyStateHint.Size = new Size(911, 40);
        lblEmptyStateHint.TabIndex = 1;
        lblEmptyStateHint.Text = "Chọn một cuộc trò chuyện để bắt đầu nhắn tin";
        lblEmptyStateHint.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblTypingIndicator
        // 
        lblTypingIndicator.BackColor = Color.White;
        lblTypingIndicator.Dock = DockStyle.Bottom;
        lblTypingIndicator.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblTypingIndicator.ForeColor = Color.FromArgb(100, 116, 139);
        lblTypingIndicator.Location = new Point(0, 676);
        lblTypingIndicator.Name = "lblTypingIndicator";
        lblTypingIndicator.Padding = new Padding(20, 0, 0, 4);
        lblTypingIndicator.Size = new Size(911, 20);
        lblTypingIndicator.TabIndex = 3;
        lblTypingIndicator.Text = "Ai đó đang soạn tin nhắn...";
        lblTypingIndicator.Visible = false;
        // 
        // pnlBlockedBanner
        // 
        pnlBlockedBanner.BackColor = Color.FromArgb(254, 242, 242);
        pnlBlockedBanner.Controls.Add(btnBlockedUnblock);
        pnlBlockedBanner.Controls.Add(lblBlockedNotice);
        pnlBlockedBanner.Dock = DockStyle.Bottom;
        pnlBlockedBanner.Location = new Point(0, 696);
        pnlBlockedBanner.Name = "pnlBlockedBanner";
        pnlBlockedBanner.Size = new Size(911, 0);
        pnlBlockedBanner.TabIndex = 9;
        pnlBlockedBanner.Visible = false;
        // 
        // lblBlockedNotice
        // 
        lblBlockedNotice.AutoSize = true;
        lblBlockedNotice.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblBlockedNotice.ForeColor = Color.FromArgb(185, 28, 28);
        lblBlockedNotice.Location = new Point(20, 9);
        lblBlockedNotice.Name = "lblBlockedNotice";
        lblBlockedNotice.Size = new Size(257, 15);
        lblBlockedNotice.TabIndex = 0;
        lblBlockedNotice.Text = "Bạn đã chặn người dùng này.";
        // 
        // btnBlockedUnblock
        // 
        btnBlockedUnblock.BackColor = Color.Transparent;
        btnBlockedUnblock.Cursor = Cursors.Hand;
        btnBlockedUnblock.FlatAppearance.BorderSize = 0;
        btnBlockedUnblock.FlatStyle = FlatStyle.Flat;
        btnBlockedUnblock.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Underline);
        btnBlockedUnblock.ForeColor = Color.FromArgb(37, 99, 235);
        btnBlockedUnblock.Location = new Point(285, 5);
        btnBlockedUnblock.Name = "btnBlockedUnblock";
        btnBlockedUnblock.Size = new Size(80, 24);
        btnBlockedUnblock.TabIndex = 1;
        btnBlockedUnblock.Text = "Bỏ chặn";
        btnBlockedUnblock.UseVisualStyleBackColor = false;
        btnBlockedUnblock.Click += btnBlockedUnblock_Click;
        // 
        // pnlMessageSearch
        // 
        pnlMessageSearch.BackColor = Color.FromArgb(248, 250, 252);
        pnlMessageSearch.Controls.Add(btnCloseSearch);
        pnlMessageSearch.Controls.Add(btnSearchNext);
        pnlMessageSearch.Controls.Add(btnSearchPrev);
        pnlMessageSearch.Controls.Add(lblSearchMatchCount);
        pnlMessageSearch.Controls.Add(pnlSearchInputContainer);
        pnlMessageSearch.Dock = DockStyle.Top;
        pnlMessageSearch.Location = new Point(0, 68);
        pnlMessageSearch.Name = "pnlMessageSearch";
        pnlMessageSearch.Padding = new Padding(12, 5, 12, 5);
        pnlMessageSearch.Size = new Size(911, 42);
        pnlMessageSearch.TabIndex = 6;
        pnlMessageSearch.Visible = false;
        // 
        // pnlSearchInputContainer
        // 
        pnlSearchInputContainer.BackColor = Color.White;
        pnlSearchInputContainer.Controls.Add(btnSearchClear);
        pnlSearchInputContainer.Controls.Add(txtSearchMessageKeyword);
        pnlSearchInputContainer.Controls.Add(lblSearchIconHeader);
        pnlSearchInputContainer.Location = new Point(14, 5);
        pnlSearchInputContainer.Name = "pnlSearchInputContainer";
        pnlSearchInputContainer.Size = new Size(430, 32);
        pnlSearchInputContainer.TabIndex = 0;
        // 
        // lblSearchIconHeader
        // 
        lblSearchIconHeader.Font = new Font("Segoe MDL2 Assets", 9F);
        lblSearchIconHeader.ForeColor = Color.FromArgb(148, 163, 184);
        lblSearchIconHeader.Location = new Point(8, 7);
        lblSearchIconHeader.Name = "lblSearchIconHeader";
        lblSearchIconHeader.Size = new Size(18, 18);
        lblSearchIconHeader.TabIndex = 0;
        lblSearchIconHeader.Text = "\uE721";
        lblSearchIconHeader.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // txtSearchMessageKeyword
        // 
        txtSearchMessageKeyword.BackColor = Color.White;
        txtSearchMessageKeyword.BorderStyle = BorderStyle.None;
        txtSearchMessageKeyword.Font = new Font("Segoe UI", 9F);
        txtSearchMessageKeyword.Location = new Point(32, 8);
        txtSearchMessageKeyword.Name = "txtSearchMessageKeyword";
        txtSearchMessageKeyword.PlaceholderText = "Nhập từ khóa tìm kiếm trong cuộc trò chuyện...";
        txtSearchMessageKeyword.Size = new Size(365, 16);
        txtSearchMessageKeyword.TabIndex = 1;
        txtSearchMessageKeyword.TextChanged += txtSearchMessageKeyword_TextChanged;
        txtSearchMessageKeyword.KeyDown += txtSearchMessageKeyword_KeyDown;
        // 
        // btnSearchClear
        // 
        btnSearchClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSearchClear.BackColor = Color.Transparent;
        btnSearchClear.Cursor = Cursors.Hand;
        btnSearchClear.FlatAppearance.BorderSize = 0;
        btnSearchClear.FlatStyle = FlatStyle.Flat;
        btnSearchClear.Font = new Font("Segoe MDL2 Assets", 8F, FontStyle.Bold);
        btnSearchClear.ForeColor = Color.FromArgb(71, 85, 105);
        btnSearchClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
        btnSearchClear.Location = new Point(398, 3);
        btnSearchClear.Name = "btnSearchClear";
        btnSearchClear.Size = new Size(26, 26);
        btnSearchClear.TabIndex = 2;
        btnSearchClear.Text = "\uE711";
        btnSearchClear.AccessibleName = "Xóa từ khóa tìm kiếm";
        btnSearchClear.AccessibleDescription = "Xóa nội dung trong ô tìm kiếm tin nhắn.";
        toolTipMain.SetToolTip(btnSearchClear, "Xóa từ khóa");
        btnSearchClear.UseVisualStyleBackColor = false;
        btnSearchClear.Visible = false;
        btnSearchClear.Click += btnSearchClear_Click;
        // 
        // btnCloseSearch
        // 
        btnCloseSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCloseSearch.BackColor = Color.Transparent;
        btnCloseSearch.Cursor = Cursors.Hand;
        btnCloseSearch.FlatAppearance.BorderSize = 0;
        btnCloseSearch.FlatStyle = FlatStyle.Flat;
        btnCloseSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
        btnCloseSearch.Font = new Font("Segoe MDL2 Assets", 8.5F, FontStyle.Bold);
        btnCloseSearch.ForeColor = Color.FromArgb(100, 116, 139);
        btnCloseSearch.Location = new Point(872, 6);
        btnCloseSearch.Name = "btnCloseSearch";
        btnCloseSearch.Size = new Size(28, 28);
        btnCloseSearch.TabIndex = 5;
        btnCloseSearch.Text = "\uE711";
        btnCloseSearch.AccessibleName = "Đóng tìm kiếm tin nhắn";
        btnCloseSearch.AccessibleDescription = "Đóng thanh tìm kiếm tin nhắn.";
        toolTipMain.SetToolTip(btnCloseSearch, "Đóng tìm kiếm (Esc)");
        btnCloseSearch.UseVisualStyleBackColor = false;
        btnCloseSearch.Click += btnCloseSearch_Click;
        // 
        // btnSearchNext
        // 
        btnSearchNext.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSearchNext.BackColor = Color.FromArgb(241, 245, 249);
        btnSearchNext.Cursor = Cursors.Hand;
        btnSearchNext.FlatAppearance.BorderSize = 0;
        btnSearchNext.FlatStyle = FlatStyle.Flat;
        btnSearchNext.Font = new Font("Segoe MDL2 Assets", 10F, FontStyle.Bold);
        btnSearchNext.ForeColor = Color.FromArgb(37, 99, 235);
        btnSearchNext.FlatAppearance.MouseOverBackColor = Color.FromArgb(219, 234, 254);
        btnSearchNext.Location = new Point(841, 6);
        btnSearchNext.Name = "btnSearchNext";
        btnSearchNext.Size = new Size(28, 28);
        btnSearchNext.TabIndex = 4;
        btnSearchNext.Text = "\uE74B";
        btnSearchNext.AccessibleName = "Tin nhắn tiếp theo";
        btnSearchNext.AccessibleDescription = "Chuyển đến kết quả tìm kiếm tiếp theo.";
        toolTipMain.SetToolTip(btnSearchNext, "Tin tiếp theo");
        btnSearchNext.UseVisualStyleBackColor = false;
        btnSearchNext.Click += btnSearchNext_Click;
        // 
        // btnSearchPrev
        // 
        btnSearchPrev.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSearchPrev.BackColor = Color.FromArgb(241, 245, 249);
        btnSearchPrev.Cursor = Cursors.Hand;
        btnSearchPrev.FlatAppearance.BorderSize = 0;
        btnSearchPrev.FlatStyle = FlatStyle.Flat;
        btnSearchPrev.Font = new Font("Segoe MDL2 Assets", 10F, FontStyle.Bold);
        btnSearchPrev.ForeColor = Color.FromArgb(37, 99, 235);
        btnSearchPrev.FlatAppearance.MouseOverBackColor = Color.FromArgb(219, 234, 254);
        btnSearchPrev.Location = new Point(810, 6);
        btnSearchPrev.Name = "btnSearchPrev";
        btnSearchPrev.Size = new Size(28, 28);
        btnSearchPrev.TabIndex = 3;
        btnSearchPrev.Text = "\uE74A";
        btnSearchPrev.AccessibleName = "Tin nhắn trước đó";
        btnSearchPrev.AccessibleDescription = "Chuyển đến kết quả tìm kiếm trước đó.";
        toolTipMain.SetToolTip(btnSearchPrev, "Tin trước đó");
        btnSearchPrev.UseVisualStyleBackColor = false;
        btnSearchPrev.Click += btnSearchPrev_Click;
        // 
        // lblSearchMatchCount
        // 
        lblSearchMatchCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblSearchMatchCount.Font = new Font("Segoe UI", 8.5F);
        lblSearchMatchCount.ForeColor = Color.FromArgb(100, 116, 139);
        lblSearchMatchCount.Location = new Point(640, 12);
        lblSearchMatchCount.Name = "lblSearchMatchCount";
        lblSearchMatchCount.Size = new Size(160, 18);
        lblSearchMatchCount.TabIndex = 2;
        lblSearchMatchCount.Text = "0 kết quả";
        lblSearchMatchCount.TextAlign = ContentAlignment.MiddleRight;
        // 
        // pnlInputBar
        // 
        pnlInputBar.BackColor = Color.White;
        pnlInputBar.Controls.Add(btnSendMessage);
        pnlInputBar.Controls.Add(pnlInputContainer);
        pnlInputBar.Controls.Add(btnRecordVoice);
        pnlInputBar.Controls.Add(btnAttachFile);
        pnlInputBar.Controls.Add(btnEmoji);
        pnlInputBar.Dock = DockStyle.Bottom;
        pnlInputBar.Location = new Point(0, 696);
        pnlInputBar.Name = "pnlInputBar";
        pnlInputBar.Padding = new Padding(14, 10, 14, 10);
        pnlInputBar.Size = new Size(911, 68);
        pnlInputBar.TabIndex = 1;
        // 
        // pnlInputContainer
        // 
        pnlInputContainer.BackColor = Color.FromArgb(241, 245, 249);
        pnlInputContainer.Controls.Add(txtMessageInput);
        pnlInputContainer.Location = new Point(148, 12);
        pnlInputContainer.Name = "pnlInputContainer";
        pnlInputContainer.Padding = new Padding(12, 10, 12, 10);
        pnlInputContainer.Size = new Size(690, 44);
        pnlInputContainer.TabIndex = 3;
        // 
        // txtMessageInput
        // 
        txtMessageInput.BackColor = Color.FromArgb(241, 245, 249);
        txtMessageInput.BorderStyle = BorderStyle.None;
        txtMessageInput.Dock = DockStyle.Fill;
        txtMessageInput.Font = new Font("Segoe UI", 10F);
        txtMessageInput.ForeColor = Color.FromArgb(15, 23, 42);
        txtMessageInput.Multiline = true;
        txtMessageInput.Name = "txtMessageInput";
        txtMessageInput.PlaceholderText = "Nhập tin nhắn...";
        txtMessageInput.ScrollBars = ScrollBars.None;
        txtMessageInput.Size = new Size(666, 24);
        txtMessageInput.TabIndex = 0;
        txtMessageInput.TextChanged += txtMessageInput_TextChanged;
        txtMessageInput.KeyDown += txtMessageInput_KeyDown;
        // 
        // btnSendMessage
        // 
        btnSendMessage.BackColor = Color.FromArgb(203, 213, 225);
        btnSendMessage.Cursor = Cursors.Default;
        btnSendMessage.Enabled = false;
        btnSendMessage.FlatAppearance.BorderSize = 0;
        btnSendMessage.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);
        btnSendMessage.FlatAppearance.MouseOverBackColor = Color.FromArgb(59, 130, 246);
        btnSendMessage.FlatStyle = FlatStyle.Flat;
        btnSendMessage.Font = new Font("Segoe MDL2 Assets", 13F);
        btnSendMessage.ForeColor = Color.White;
        btnSendMessage.Location = new Point(852, 12);
        btnSendMessage.Name = "btnSendMessage";
        btnSendMessage.Size = new Size(46, 44);
        btnSendMessage.TabIndex = 4;
        btnSendMessage.Text = "\uE724";
        toolTipMain.SetToolTip(btnSendMessage, "Gửi tin nhắn (Enter)");
        btnSendMessage.UseVisualStyleBackColor = false;
        btnSendMessage.Click += btnSendMessage_Click;
        // 
        // btnRecordVoice
        // 
        btnRecordVoice.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnRecordVoice.BackColor = Color.FromArgb(30, 41, 59);
        btnRecordVoice.Cursor = Cursors.Hand;
        btnRecordVoice.FlatAppearance.BorderSize = 0;
        btnRecordVoice.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnRecordVoice.FlatStyle = FlatStyle.Flat;
        btnRecordVoice.Font = new Font("Segoe MDL2 Assets", 13F);
        btnRecordVoice.ForeColor = Color.FromArgb(203, 213, 225);
        btnRecordVoice.Location = new Point(98, 12);
        btnRecordVoice.Name = "btnRecordVoice";
        btnRecordVoice.Size = new Size(40, 44);
        btnRecordVoice.TabIndex = 2;
        btnRecordVoice.Text = "\uE720";
        toolTipMain.SetToolTip(btnRecordVoice, "Ghi âm tin nhắn thoại");
        btnRecordVoice.UseVisualStyleBackColor = false;
        btnRecordVoice.Click += btnRecordVoice_Click;
        // 
        // btnAttachFile
        // 
        btnAttachFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnAttachFile.BackColor = Color.FromArgb(30, 41, 59);
        btnAttachFile.Cursor = Cursors.Hand;
        btnAttachFile.FlatAppearance.BorderSize = 0;
        btnAttachFile.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnAttachFile.FlatStyle = FlatStyle.Flat;
        btnAttachFile.Font = new Font("Segoe MDL2 Assets", 13F);
        btnAttachFile.ForeColor = Color.FromArgb(203, 213, 225);
        btnAttachFile.Location = new Point(54, 12);
        btnAttachFile.Name = "btnAttachFile";
        btnAttachFile.Size = new Size(40, 44);
        btnAttachFile.TabIndex = 1;
        btnAttachFile.Text = "\uE723";
        toolTipMain.SetToolTip(btnAttachFile, "Đính kèm file / hình ảnh (Ctrl+O)");
        btnAttachFile.UseVisualStyleBackColor = false;
        btnAttachFile.Click += btnAttachFile_Click;
        // 
        // btnEmoji
        // 
        btnEmoji.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnEmoji.BackColor = Color.FromArgb(30, 41, 59);
        btnEmoji.Cursor = Cursors.Hand;
        btnEmoji.FlatAppearance.BorderSize = 0;
        btnEmoji.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnEmoji.FlatStyle = FlatStyle.Flat;
        btnEmoji.Font = new Font("Segoe MDL2 Assets", 13F);
        btnEmoji.ForeColor = Color.FromArgb(203, 213, 225);
        btnEmoji.Location = new Point(12, 12);
        btnEmoji.Name = "btnEmoji";
        btnEmoji.Size = new Size(40, 44);
        btnEmoji.TabIndex = 0;
        btnEmoji.Text = "\uE899";
        toolTipMain.SetToolTip(btnEmoji, "Chèn biểu tượng cảm xúc (Emoji)");
        btnEmoji.UseVisualStyleBackColor = false;
        toolTipMain.SetToolTip(btnEmoji, "Chèn biểu tượng cảm xúc (Emoji)");
        btnEmoji.UseVisualStyleBackColor = false;
        // 
        // pnlChatHeader
        // 
        pnlChatHeader.BackColor = Color.White;
        pnlChatHeader.Controls.Add(pnlHeaderButtons);
        pnlChatHeader.Controls.Add(lblChatStatus);
        pnlChatHeader.Controls.Add(lblChatTitle);
        pnlChatHeader.Controls.Add(picChatAvatar);
        pnlChatHeader.Dock = DockStyle.Top;
        pnlChatHeader.Location = new Point(0, 0);
        pnlChatHeader.Name = "pnlChatHeader";
        pnlChatHeader.Size = new Size(911, 68);
        pnlChatHeader.TabIndex = 0;
        // 
        // pnlHeaderButtons
        // 
        pnlHeaderButtons.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pnlHeaderButtons.BackColor = Color.White;
        pnlHeaderButtons.Controls.Add(btnMoreMenu);
        pnlHeaderButtons.Controls.Add(btnAttachFileHeader);
        pnlHeaderButtons.Controls.Add(btnGroupInfo);
        pnlHeaderButtons.Controls.Add(btnVideoCall);
        pnlHeaderButtons.Controls.Add(btnVoiceCall);
        pnlHeaderButtons.Controls.Add(btnSearch);
        pnlHeaderButtons.Location = new Point(640, 10);
        pnlHeaderButtons.Name = "pnlHeaderButtons";
        pnlHeaderButtons.Size = new Size(260, 48);
        pnlHeaderButtons.TabIndex = 3;
        // 
        // btnMoreMenu
        // 
        btnMoreMenu.BackColor = Color.FromArgb(30, 41, 59);
        btnMoreMenu.Cursor = Cursors.Hand;
        btnMoreMenu.FlatAppearance.BorderSize = 0;
        btnMoreMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnMoreMenu.FlatStyle = FlatStyle.Flat;
        btnMoreMenu.Font = new Font("Segoe MDL2 Assets", 12F, FontStyle.Bold);
        btnMoreMenu.ForeColor = Color.FromArgb(203, 213, 225);
        btnMoreMenu.Location = new Point(216, 4);
        btnMoreMenu.Name = "btnMoreMenu";
        btnMoreMenu.Size = new Size(38, 40);
        btnMoreMenu.TabIndex = 5;
        btnMoreMenu.Text = "\uE10C";
        toolTipMain.SetToolTip(btnMoreMenu, "Tùy chọn cuộc trò chuyện");
        btnMoreMenu.UseVisualStyleBackColor = false;
        btnMoreMenu.Click += btnMoreMenu_Click;
        // 
        // btnAttachFileHeader
        // 
        btnAttachFileHeader.BackColor = Color.FromArgb(30, 41, 59);
        btnAttachFileHeader.Cursor = Cursors.Hand;
        btnAttachFileHeader.FlatAppearance.BorderSize = 0;
        btnAttachFileHeader.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnAttachFileHeader.FlatStyle = FlatStyle.Flat;
        btnAttachFileHeader.Font = new Font("Segoe MDL2 Assets", 12F);
        btnAttachFileHeader.ForeColor = Color.FromArgb(203, 213, 225);
        btnAttachFileHeader.Location = new Point(172, 4);
        btnAttachFileHeader.Name = "btnAttachFileHeader";
        btnAttachFileHeader.Size = new Size(38, 40);
        btnAttachFileHeader.TabIndex = 4;
        btnAttachFileHeader.Text = "\uE8A5";
        toolTipMain.SetToolTip(btnAttachFileHeader, "Nội dung đã chia sẻ (Ảnh, tệp tin & liên kết)");
        btnAttachFileHeader.UseVisualStyleBackColor = false;
        btnAttachFileHeader.Click += btnAttachFileHeader_Click;
        // 
        // btnGroupInfo
        // 
        btnGroupInfo.BackColor = Color.FromArgb(30, 41, 59);
        btnGroupInfo.Cursor = Cursors.Hand;
        btnGroupInfo.FlatAppearance.BorderSize = 0;
        btnGroupInfo.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnGroupInfo.FlatStyle = FlatStyle.Flat;
        btnGroupInfo.Font = new Font("Segoe MDL2 Assets", 12F);
        btnGroupInfo.ForeColor = Color.FromArgb(203, 213, 225);
        btnGroupInfo.Location = new Point(128, 4);
        btnGroupInfo.Name = "btnGroupInfo";
        btnGroupInfo.Size = new Size(38, 40);
        btnGroupInfo.TabIndex = 3;
        btnGroupInfo.Text = "\uE716";
        toolTipMain.SetToolTip(btnGroupInfo, "Quản lý nhóm");
        btnGroupInfo.UseVisualStyleBackColor = false;
        btnGroupInfo.Visible = false;
        btnGroupInfo.Click += btnGroupInfo_Click;
        // 
        // btnVideoCall
        // 
        btnVideoCall.BackColor = Color.FromArgb(30, 41, 59);
        btnVideoCall.Cursor = Cursors.Hand;
        btnVideoCall.FlatAppearance.BorderSize = 0;
        btnVideoCall.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnVideoCall.FlatStyle = FlatStyle.Flat;
        btnVideoCall.Font = new Font("Segoe MDL2 Assets", 12F);
        btnVideoCall.ForeColor = Color.FromArgb(203, 213, 225);
        btnVideoCall.Location = new Point(88, 4);
        btnVideoCall.Name = "btnVideoCall";
        btnVideoCall.Size = new Size(38, 40);
        btnVideoCall.TabIndex = 2;
        btnVideoCall.Text = "\uE714";
        toolTipMain.SetToolTip(btnVideoCall, "Gọi video WebRTC LAN");
        btnVideoCall.UseVisualStyleBackColor = false;
        btnVideoCall.Click += btnVideoCall_Click;
        // 
        // btnVoiceCall
        // 
        btnVoiceCall.BackColor = Color.FromArgb(30, 41, 59);
        btnVoiceCall.Cursor = Cursors.Hand;
        btnVoiceCall.FlatAppearance.BorderSize = 0;
        btnVoiceCall.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnVoiceCall.FlatStyle = FlatStyle.Flat;
        btnVoiceCall.Font = new Font("Segoe MDL2 Assets", 12F);
        btnVoiceCall.ForeColor = Color.FromArgb(203, 213, 225);
        btnVoiceCall.Location = new Point(46, 4);
        btnVoiceCall.Name = "btnVoiceCall";
        btnVoiceCall.Size = new Size(38, 40);
        btnVoiceCall.TabIndex = 1;
        btnVoiceCall.Text = "\uE717";
        toolTipMain.SetToolTip(btnVoiceCall, "Gọi thoại qua mạng LAN");
        btnVoiceCall.UseVisualStyleBackColor = false;
        btnVoiceCall.Click += btnVoiceCall_Click;
        // 
        // btnSearch
        // 
        btnSearch.BackColor = Color.FromArgb(30, 41, 59);
        btnSearch.Cursor = Cursors.Hand;
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 59, 91);
        btnSearch.FlatStyle = FlatStyle.Flat;
        btnSearch.Font = new Font("Segoe MDL2 Assets", 12F);
        btnSearch.ForeColor = Color.FromArgb(203, 213, 225);
        btnSearch.Location = new Point(4, 4);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(38, 40);
        btnSearch.TabIndex = 0;
        btnSearch.Text = "\uE721";
        toolTipMain.SetToolTip(btnSearch, "Tìm kiếm tin nhắn");
        btnSearch.UseVisualStyleBackColor = false;
        btnSearch.Click += btnSearch_Click;
        toolTipMain.SetToolTip(btnSearch, "Tìm kiếm tin nhắn");
        btnSearch.UseVisualStyleBackColor = false;
        btnSearch.Click += btnSearch_Click;
        // 
        // lblChatStatus
        // 
        lblChatStatus.AutoSize = true;
        lblChatStatus.Font = new Font("Segoe UI", 8.5F);
        lblChatStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblChatStatus.Location = new Point(66, 38);
        lblChatStatus.Name = "lblChatStatus";
        lblChatStatus.Size = new Size(72, 15);
        lblChatStatus.TabIndex = 2;
        lblChatStatus.Text = "Ngoại tuyến";
        // 
        // lblChatTitle
        // 
        lblChatTitle.AutoEllipsis = true;
        lblChatTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblChatTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblChatTitle.Location = new Point(66, 14);
        lblChatTitle.Name = "lblChatTitle";
        lblChatTitle.Size = new Size(500, 22);
        lblChatTitle.TabIndex = 1;
        lblChatTitle.Text = "Chọn cuộc trò chuyện";
        // 
        // picChatAvatar
        // 
        picChatAvatar.BackColor = Color.FromArgb(59, 130, 246);
        picChatAvatar.Location = new Point(14, 12);
        picChatAvatar.Name = "picChatAvatar";
        picChatAvatar.Size = new Size(44, 44);
        picChatAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picChatAvatar.TabIndex = 0;
        picChatAvatar.TabStop = false;
        // 
        // menuTrayOpen
        // 
        menuTrayOpen.Name = "menuTrayOpen";
        menuTrayOpen.Size = new Size(168, 22);
        menuTrayOpen.Text = "Mở Chatting";
        menuTrayOpen.Click += menuTrayOpen_Click;
        // 
        // menuTrayLogout
        // 
        menuTrayLogout.Name = "menuTrayLogout";
        menuTrayLogout.Size = new Size(168, 22);
        menuTrayLogout.Text = "Đăng xuất";
        menuTrayLogout.Click += btnLogout_Click;
        // 
        // menuTrayExit
        // 
        menuTrayExit.Name = "menuTrayExit";
        menuTrayExit.Size = new Size(168, 22);
        menuTrayExit.Text = "Thoát ứng dụng";
        menuTrayExit.Click += menuTrayExit_Click;
        // 
        // ctxTrayMenu
        // 
        ctxTrayMenu.Items.AddRange(new ToolStripItem[] { menuTrayOpen, menuTrayLogout, menuTrayExit });
        ctxTrayMenu.Name = "ctxTrayMenu";
        ctxTrayMenu.Size = new Size(169, 70);
        // 
        // toolTipMain
        // 
        toolTipMain.AutoPopDelay = 5000;
        toolTipMain.BackColor = Color.FromArgb(15, 23, 42);
        toolTipMain.ForeColor = Color.White;
        toolTipMain.InitialDelay = 300;
        toolTipMain.ReshowDelay = 100;
        // 
        // picAiRobotBubble
        // 
        picAiRobotBubble.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        picAiRobotBubble.BackColor = Color.Transparent;
        picAiRobotBubble.Cursor = Cursors.Hand;
        picAiRobotBubble.Location = new Point(6, 655);
        picAiRobotBubble.Name = "picAiRobotBubble";
        picAiRobotBubble.Size = new Size(56, 56);
        picAiRobotBubble.TabIndex = 99;
        picAiRobotBubble.TabStop = false;
        toolTipMain.SetToolTip(picAiRobotBubble, "Chatting AI — Trợ lý ảo Hologram");
        picAiRobotBubble.Click += picAiRobotBubble_Click;
        picAiRobotBubble.Paint += picAiRobotBubble_Paint;
        picAiRobotBubble.MouseEnter += picAiRobotBubble_MouseEnter;
        picAiRobotBubble.MouseLeave += picAiRobotBubble_MouseLeave;
        // 
        // tmrAiFloat
        // 
        tmrAiFloat.Interval = 50;
        tmrAiFloat.Tick += tmrAiFloat_Tick;
        // 
        // MainMessengerForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(13, 21, 39);
        ClientSize = new Size(1250, 800);
        Controls.Add(pnlMainContainer);
        Controls.Add(pnlSidebarBorder);
        Controls.Add(pnlSidebar);
        Controls.Add(pnlNavStrip);
        Controls.Add(pnlTitleBar);
        FormBorderStyle = FormBorderStyle.None;
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(980, 640);
        Name = "MainMessengerForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Chatting — LAN Messenger";
        FormClosing += MainMessengerForm_FormClosing;
        Load += MainMessengerForm_Load;
        pnlTitleBar.ResumeLayout(false);
        pnlTitleBar.PerformLayout();
        pnlNavStrip.ResumeLayout(false);
        pnlSidebar.ResumeLayout(false);
        pnlSearch.ResumeLayout(false);
        pnlSearch.PerformLayout();
        pnlUserHeader.ResumeLayout(false);
        pnlUserHeader.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picMyAvatar).EndInit();
        pnlMainContainer.ResumeLayout(false);
        pnlCallHistory.ResumeLayout(false);
        pnlChatArea.ResumeLayout(false);
        pnlToast.ResumeLayout(false);
        pnlMoreMenu.ResumeLayout(false);
        itemRefresh.ResumeLayout(false);
        itemRefresh.PerformLayout();
        itemBlock.ResumeLayout(false);
        itemBlock.PerformLayout();
        itemDelete.ResumeLayout(false);
        itemDelete.PerformLayout();
        itemSearch.ResumeLayout(false);
        itemSearch.PerformLayout();
        itemUserInfo.ResumeLayout(false);
        itemUserInfo.PerformLayout();
        pnlEmptyState.ResumeLayout(false);
        pnlBlockedBanner.ResumeLayout(false);
        pnlBlockedBanner.PerformLayout();
        pnlSearchInputContainer.ResumeLayout(false);
        pnlSearchInputContainer.PerformLayout();
        pnlMessageSearch.ResumeLayout(false);
        pnlMessageSearch.PerformLayout();
        pnlInputBar.ResumeLayout(false);
        pnlInputBar.PerformLayout();
        pnlChatHeader.ResumeLayout(false);
        pnlChatHeader.PerformLayout();
        pnlHeaderButtons.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picChatAvatar).EndInit();
        ((System.ComponentModel.ISupportInitialize)picAiRobotBubble).EndInit();
        ctxTrayMenu.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlTitleBar;
    private Label lblTitleLogo;
    private Label lblAppTitle;
    private Button btnMinimize;
    private Button btnMaximize;
    private Button btnCloseWindow;
    private Panel pnlNavStrip;
    private Button btnNavChat;
    private Button btnNavCalls;
    private Button btnNavFiles;
    private Button btnNavSettings;
    private Label lblLanIndicator;
    private Panel pnlSidebar;
    private Panel pnlSidebarBorder;
    private Panel pnlUserHeader;
    private PictureBox picMyAvatar;
    private Label lblMyUsername;
    private Label lblMyStatus;
    private Button btnOpenFriends;
    private Button btnNewGroup;
    private Button btnLogout;
    private Panel pnlSearch;
    private TextBox txtSearchConversations;
    private FlowLayoutPanel flpConversations;
    private Panel pnlChatArea;
    private Panel pnlChatHeader;
    private Panel pnlHeaderButtons;
    private PictureBox picChatAvatar;
    private Label lblChatTitle;
    private Label lblChatStatus;
    private Button btnSearch;
    private Button btnVoiceCall;
    private Button btnVideoCall;
    private Button btnGroupInfo;
    private Button btnAttachFileHeader;
    private Button btnMoreMenu;
    private Panel pnlMoreMenu;
    private Panel itemUserInfo;
    private Label lblUserInfoIcon;
    private Label lblUserInfoTitle;
    private Label lblUserInfoSub;
    private Panel itemSearch;
    private Label lblSearchIcon;
    private Label lblSearchTitle;
    private Label lblSearchSub;
    private Panel itemDelete;
    private Label lblDeleteIcon;
    private Label lblDeleteTitle;
    private Label lblDeleteSub;
    private Panel itemBlock;
    private Label lblBlockIcon;
    private Label lblBlockTitle;
    private Label lblBlockSub;
    private Panel itemRefresh;
    private Label lblRefreshIcon;
    private Label lblRefreshTitle;
    private Label lblRefreshSub;
    private Panel pnlMessageSearch;
    private Panel pnlSearchInputContainer;
    private Button btnSearchClear;
    private Label lblSearchIconHeader;
    private TextBox txtSearchMessageKeyword;
    private Label lblSearchMatchCount;
    private Button btnSearchPrev;
    private Button btnSearchNext;
    private Button btnCloseSearch;
    private Panel pnlBlockedBanner;
    private Label lblBlockedNotice;
    private Button btnBlockedUnblock;
    private Panel pnlToast;
    private Label lblToastMessage;
    private FlowLayoutPanel flpMessages;
    private Panel pnlEmptyState;
    private Label lblEmptyState;
    private Label lblEmptyStateHint;
    private Label lblTypingIndicator;
    private Panel pnlMainContainer;
    private Panel pnlCallHistory;
    private Panel pnlInputBar;
    private Panel pnlInputContainer;
    private Button btnEmoji;
    private Button btnAttachFile;
    private Button btnRecordVoice;
    private TextBox txtMessageInput;
    private Button btnSendMessage;
    private OpenFileDialog openFileDialog;
    private NotifyIcon notifyIconTray;
    private ContextMenuStrip ctxTrayMenu;
    private ToolStripMenuItem menuTrayOpen;
    private ToolStripMenuItem menuTrayLogout;
    private ToolStripMenuItem menuTrayExit;
    private ToolTip toolTipMain;
    private PictureBox picAiRobotBubble;
    private System.Windows.Forms.Timer tmrAiFloat;
}
