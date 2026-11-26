namespace Chatting.Client.Forms;

partial class GroupManagerForm
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
        pnlRobotContainer = new Panel();
        picRobotMascot = new PictureBox();
        lblHeaderTitle = new Label();
        lblHeaderSubtitle = new Label();
        pnlBodyContainer = new Panel();
        pnlMainCard = new Panel();
        pnlListBorderContainer = new Panel();
        flpFriendsList = new FlowLayoutPanel();
        flpMembersList = new FlowLayoutPanel();
        pnlFriendsEmpty = new Panel();
        lblFriendsEmptyIcon = new Label();
        lblFriendsEmptyTitle = new Label();
        lblFriendsEmptySub = new Label();
        pnlMiddleHeader = new Panel();
        lblChooseFriends = new Label();
        pnlTabs = new Panel();
        btnTabMembers = new Button();
        btnTabAddFriends = new Button();
        pnlTabIndicator = new Panel();
        pnlGroupNameRow = new Panel();
        pnlGroupNameBox = new Panel();
        txtGroupName = new TextBox();
        lblGroupName = new Label();
        pnlFooter = new Panel();
        btnLeaveGroup = new Button();
        btnRemoveMember = new Button();
        btnSubmit = new Button();
        btnClose = new Button();
        
        // Legacy controls for full backward compatibility
        grpFriends = new GroupBox();
        chkListFriends = new CheckedListBox();
        grpMembers = new GroupBox();
        lstMembers = new ListView();
        colMemberName = new ColumnHeader();
        colMemberRole = new ColumnHeader();
        colMemberStatus = new ColumnHeader();

        pnlHeader.SuspendLayout();
        pnlRobotContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picRobotMascot).BeginInit();
        pnlBodyContainer.SuspendLayout();
        pnlMainCard.SuspendLayout();
        pnlListBorderContainer.SuspendLayout();
        pnlFriendsEmpty.SuspendLayout();
        pnlMiddleHeader.SuspendLayout();
        pnlTabs.SuspendLayout();
        pnlGroupNameRow.SuspendLayout();
        pnlGroupNameBox.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.Controls.Add(pnlRobotContainer);
        pnlHeader.Controls.Add(lblHeaderTitle);
        pnlHeader.Controls.Add(lblHeaderSubtitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(820, 92);
        pnlHeader.TabIndex = 0;

        // 
        // pnlRobotContainer
        // 
        pnlRobotContainer.BackColor = Color.White;
        pnlRobotContainer.Controls.Add(picRobotMascot);
        pnlRobotContainer.Location = new Point(18, 14);
        pnlRobotContainer.Name = "pnlRobotContainer";
        pnlRobotContainer.Padding = new Padding(3);
        pnlRobotContainer.Size = new Size(64, 64);
        pnlRobotContainer.TabIndex = 0;

        // 
        // picRobotMascot
        // 
        picRobotMascot.BackColor = Color.Transparent;
        picRobotMascot.Dock = DockStyle.Fill;
        picRobotMascot.Location = new Point(3, 3);
        picRobotMascot.Name = "picRobotMascot";
        picRobotMascot.Size = new Size(58, 58);
        picRobotMascot.SizeMode = PictureBoxSizeMode.Zoom;
        picRobotMascot.TabIndex = 0;
        picRobotMascot.TabStop = false;

        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.BackColor = Color.Transparent;
        lblHeaderTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(96, 18);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(270, 30);
        lblHeaderTitle.TabIndex = 1;
        lblHeaderTitle.Text = "QUẢN LÝ NHÓM CHAT";

        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.AutoSize = true;
        lblHeaderSubtitle.BackColor = Color.Transparent;
        lblHeaderSubtitle.Font = new Font("Segoe UI", 9.5F);
        lblHeaderSubtitle.ForeColor = Color.FromArgb(219, 234, 254);
        lblHeaderSubtitle.Location = new Point(98, 52);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new Size(428, 17);
        lblHeaderSubtitle.TabIndex = 2;
        lblHeaderSubtitle.Text = "Tạo nhóm trò chuyện mới hoặc quản lý, phân quyền thành viên hội thoại";

        // 
        // pnlBodyContainer
        // 
        pnlBodyContainer.BackColor = Color.FromArgb(244, 247, 254);
        pnlBodyContainer.Controls.Add(pnlMainCard);
        pnlBodyContainer.Dock = DockStyle.Fill;
        pnlBodyContainer.Location = new Point(0, 92);
        pnlBodyContainer.Name = "pnlBodyContainer";
        pnlBodyContainer.Padding = new Padding(20, 14, 20, 8);
        pnlBodyContainer.Size = new Size(820, 464);
        pnlBodyContainer.TabIndex = 1;

        // 
        // pnlMainCard
        // 
        pnlMainCard.BackColor = Color.White;
        pnlMainCard.Controls.Add(pnlListBorderContainer);
        pnlMainCard.Controls.Add(pnlMiddleHeader);
        pnlMainCard.Controls.Add(pnlGroupNameRow);
        pnlMainCard.Dock = DockStyle.Fill;
        pnlMainCard.Location = new Point(20, 14);
        pnlMainCard.Name = "pnlMainCard";
        pnlMainCard.Padding = new Padding(22, 18, 22, 18);
        pnlMainCard.Size = new Size(780, 442);
        pnlMainCard.TabIndex = 0;

        // 
        // pnlGroupNameRow
        // 
        pnlGroupNameRow.Controls.Add(pnlGroupNameBox);
        pnlGroupNameRow.Controls.Add(lblGroupName);
        pnlGroupNameRow.Dock = DockStyle.Top;
        pnlGroupNameRow.Location = new Point(22, 18);
        pnlGroupNameRow.Name = "pnlGroupNameRow";
        pnlGroupNameRow.Size = new Size(736, 44);
        pnlGroupNameRow.TabIndex = 0;

        // 
        // lblGroupName
        // 
        lblGroupName.AutoSize = true;
        lblGroupName.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblGroupName.ForeColor = Color.FromArgb(30, 41, 59);
        lblGroupName.Location = new Point(0, 9);
        lblGroupName.Name = "lblGroupName";
        lblGroupName.Size = new Size(81, 19);
        lblGroupName.TabIndex = 0;
        lblGroupName.Text = "Tên nhóm:";

        // 
        // pnlGroupNameBox
        // 
        pnlGroupNameBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlGroupNameBox.BackColor = Color.White;
        pnlGroupNameBox.Controls.Add(txtGroupName);
        pnlGroupNameBox.Location = new Point(96, 0);
        pnlGroupNameBox.Name = "pnlGroupNameBox";
        pnlGroupNameBox.Size = new Size(640, 38);
        pnlGroupNameBox.TabIndex = 1;

        // 
        // txtGroupName
        // 
        txtGroupName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtGroupName.BackColor = Color.White;
        txtGroupName.BorderStyle = BorderStyle.None;
        txtGroupName.Font = new Font("Segoe UI", 10.5F);
        txtGroupName.ForeColor = Color.FromArgb(15, 23, 42);
        txtGroupName.Location = new Point(12, 9);
        txtGroupName.Name = "txtGroupName";
        txtGroupName.PlaceholderText = "Nhập tên nhóm trò chuyện...";
        txtGroupName.Size = new Size(616, 19);
        txtGroupName.TabIndex = 0;

        // 
        // pnlMiddleHeader
        // 
        pnlMiddleHeader.Controls.Add(lblChooseFriends);
        pnlMiddleHeader.Controls.Add(pnlTabs);
        pnlMiddleHeader.Dock = DockStyle.Top;
        pnlMiddleHeader.Location = new Point(22, 62);
        pnlMiddleHeader.Name = "pnlMiddleHeader";
        pnlMiddleHeader.Size = new Size(736, 42);
        pnlMiddleHeader.TabIndex = 1;

        // 
        // lblChooseFriends
        // 
        lblChooseFriends.AutoSize = true;
        lblChooseFriends.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblChooseFriends.ForeColor = Color.FromArgb(51, 65, 85);
        lblChooseFriends.Location = new Point(0, 14);
        lblChooseFriends.Name = "lblChooseFriends";
        lblChooseFriends.Size = new Size(207, 19);
        lblChooseFriends.TabIndex = 0;
        lblChooseFriends.Text = "Chọn bạn bè thêm vào nhóm:";

        // 
        // pnlTabs
        // 
        pnlTabs.Controls.Add(pnlTabIndicator);
        pnlTabs.Controls.Add(btnTabMembers);
        pnlTabs.Controls.Add(btnTabAddFriends);
        pnlTabs.Dock = DockStyle.Fill;
        pnlTabs.Location = new Point(0, 0);
        pnlTabs.Name = "pnlTabs";
        pnlTabs.Size = new Size(736, 42);
        pnlTabs.TabIndex = 1;
        pnlTabs.Visible = false;

        // 
        // btnTabMembers
        // 
        btnTabMembers.Cursor = Cursors.Hand;
        btnTabMembers.FlatAppearance.BorderSize = 0;
        btnTabMembers.FlatStyle = FlatStyle.Flat;
        btnTabMembers.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        btnTabMembers.ForeColor = Color.FromArgb(37, 99, 235);
        btnTabMembers.Location = new Point(0, 2);
        btnTabMembers.Name = "btnTabMembers";
        btnTabMembers.Size = new Size(185, 36);
        btnTabMembers.TabIndex = 0;
        btnTabMembers.Text = "Thành viên hiện tại";
        btnTabMembers.UseVisualStyleBackColor = true;

        // 
        // btnTabAddFriends
        // 
        btnTabAddFriends.Cursor = Cursors.Hand;
        btnTabAddFriends.FlatAppearance.BorderSize = 0;
        btnTabAddFriends.FlatStyle = FlatStyle.Flat;
        btnTabAddFriends.Font = new Font("Segoe UI", 9.5F);
        btnTabAddFriends.ForeColor = Color.FromArgb(100, 116, 139);
        btnTabAddFriends.Location = new Point(190, 2);
        btnTabAddFriends.Name = "btnTabAddFriends";
        btnTabAddFriends.Size = new Size(185, 36);
        btnTabAddFriends.TabIndex = 1;
        btnTabAddFriends.Text = "Thêm bạn bè";
        btnTabAddFriends.UseVisualStyleBackColor = true;

        // 
        // pnlTabIndicator
        // 
        pnlTabIndicator.BackColor = Color.FromArgb(37, 99, 235);
        pnlTabIndicator.Location = new Point(10, 38);
        pnlTabIndicator.Name = "pnlTabIndicator";
        pnlTabIndicator.Size = new Size(165, 3);
        pnlTabIndicator.TabIndex = 2;

        // 
        // pnlListBorderContainer
        // 
        pnlListBorderContainer.BackColor = Color.White;
        pnlListBorderContainer.Controls.Add(flpFriendsList);
        pnlListBorderContainer.Controls.Add(flpMembersList);
        pnlListBorderContainer.Controls.Add(pnlFriendsEmpty);
        pnlListBorderContainer.Dock = DockStyle.Fill;
        pnlListBorderContainer.Location = new Point(22, 104);
        pnlListBorderContainer.Name = "pnlListBorderContainer";
        pnlListBorderContainer.Padding = new Padding(2);
        pnlListBorderContainer.Size = new Size(736, 320);
        pnlListBorderContainer.TabIndex = 2;

        // 
        // flpFriendsList
        // 
        flpFriendsList.AutoScroll = true;
        flpFriendsList.BackColor = Color.White;
        flpFriendsList.Dock = DockStyle.Fill;
        flpFriendsList.FlowDirection = FlowDirection.TopDown;
        flpFriendsList.Location = new Point(2, 2);
        flpFriendsList.Name = "flpFriendsList";
        flpFriendsList.Padding = new Padding(0, 2, 4, 2);
        flpFriendsList.Size = new Size(732, 316);
        flpFriendsList.TabIndex = 0;
        flpFriendsList.WrapContents = false;

        // 
        // flpMembersList
        // 
        flpMembersList.AutoScroll = true;
        flpMembersList.BackColor = Color.White;
        flpMembersList.Dock = DockStyle.Fill;
        flpMembersList.FlowDirection = FlowDirection.TopDown;
        flpMembersList.Location = new Point(2, 2);
        flpMembersList.Name = "flpMembersList";
        flpMembersList.Padding = new Padding(0, 2, 4, 2);
        flpMembersList.Size = new Size(732, 316);
        flpMembersList.TabIndex = 1;
        flpMembersList.Visible = false;
        flpMembersList.WrapContents = false;

        // 
        // pnlFriendsEmpty
        // 
        pnlFriendsEmpty.BackColor = Color.White;
        pnlFriendsEmpty.Controls.Add(lblFriendsEmptyIcon);
        pnlFriendsEmpty.Controls.Add(lblFriendsEmptyTitle);
        pnlFriendsEmpty.Controls.Add(lblFriendsEmptySub);
        pnlFriendsEmpty.Dock = DockStyle.Fill;
        pnlFriendsEmpty.Location = new Point(2, 2);
        pnlFriendsEmpty.Name = "pnlFriendsEmpty";
        pnlFriendsEmpty.Size = new Size(732, 316);
        pnlFriendsEmpty.TabIndex = 2;
        pnlFriendsEmpty.Visible = false;

        // 
        // lblFriendsEmptyIcon
        // 
        lblFriendsEmptyIcon.Anchor = AnchorStyles.None;
        lblFriendsEmptyIcon.Font = new Font("Segoe MDL2 Assets", 30F);
        lblFriendsEmptyIcon.ForeColor = Color.FromArgb(203, 213, 225);
        lblFriendsEmptyIcon.Location = new Point(291, 75);
        lblFriendsEmptyIcon.Name = "lblFriendsEmptyIcon";
        lblFriendsEmptyIcon.Size = new Size(150, 50);
        lblFriendsEmptyIcon.TabIndex = 0;
        lblFriendsEmptyIcon.Text = "\uE716";
        lblFriendsEmptyIcon.TextAlign = ContentAlignment.MiddleCenter;

        // 
        // lblFriendsEmptyTitle
        // 
        lblFriendsEmptyTitle.Anchor = AnchorStyles.None;
        lblFriendsEmptyTitle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        lblFriendsEmptyTitle.ForeColor = Color.FromArgb(71, 85, 105);
        lblFriendsEmptyTitle.Location = new Point(166, 130);
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
        lblFriendsEmptySub.Location = new Point(141, 158);
        lblFriendsEmptySub.Name = "lblFriendsEmptySub";
        lblFriendsEmptySub.Size = new Size(450, 40);
        lblFriendsEmptySub.TabIndex = 2;
        lblFriendsEmptySub.Text = "Hãy kết bạn thêm để thêm bạn bè vào nhóm trò chuyện.";
        lblFriendsEmptySub.TextAlign = ContentAlignment.TopCenter;

        // 
        // pnlFooter
        // 
        pnlFooter.BackColor = Color.FromArgb(244, 247, 254);
        pnlFooter.Controls.Add(btnLeaveGroup);
        pnlFooter.Controls.Add(btnRemoveMember);
        pnlFooter.Controls.Add(btnSubmit);
        pnlFooter.Controls.Add(btnClose);
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Location = new Point(0, 556);
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Padding = new Padding(20, 10, 20, 14);
        pnlFooter.Size = new Size(820, 64);
        pnlFooter.TabIndex = 2;

        // 
        // btnLeaveGroup
        // 
        btnLeaveGroup.BackColor = Color.FromArgb(254, 242, 242);
        btnLeaveGroup.Cursor = Cursors.Hand;
        btnLeaveGroup.FlatAppearance.BorderSize = 0;
        btnLeaveGroup.FlatStyle = FlatStyle.Flat;
        btnLeaveGroup.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        btnLeaveGroup.ForeColor = Color.FromArgb(220, 38, 38);
        btnLeaveGroup.Location = new Point(20, 12);
        btnLeaveGroup.Name = "btnLeaveGroup";
        btnLeaveGroup.Size = new Size(130, 40);
        btnLeaveGroup.TabIndex = 3;
        btnLeaveGroup.Text = "Rời nhóm";
        btnLeaveGroup.UseVisualStyleBackColor = false;
        btnLeaveGroup.Visible = false;
        btnLeaveGroup.Click += btnLeaveGroup_Click;

        // 
        // btnRemoveMember
        // 
        btnRemoveMember.BackColor = Color.FromArgb(241, 245, 249);
        btnRemoveMember.Cursor = Cursors.Hand;
        btnRemoveMember.FlatAppearance.BorderSize = 0;
        btnRemoveMember.FlatStyle = FlatStyle.Flat;
        btnRemoveMember.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        btnRemoveMember.ForeColor = Color.FromArgb(71, 85, 105);
        btnRemoveMember.Location = new Point(160, 12);
        btnRemoveMember.Name = "btnRemoveMember";
        btnRemoveMember.Size = new Size(130, 40);
        btnRemoveMember.TabIndex = 2;
        btnRemoveMember.Text = "Xóa thành viên";
        btnRemoveMember.UseVisualStyleBackColor = false;
        btnRemoveMember.Visible = false;
        btnRemoveMember.Click += btnRemoveMember_Click;

        // 
        // btnSubmit
        // 
        btnSubmit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSubmit.BackColor = Color.FromArgb(29, 78, 216);
        btnSubmit.Cursor = Cursors.Hand;
        btnSubmit.FlatAppearance.BorderSize = 0;
        btnSubmit.FlatStyle = FlatStyle.Flat;
        btnSubmit.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        btnSubmit.ForeColor = Color.White;
        btnSubmit.Location = new Point(515, 12);
        btnSubmit.Name = "btnSubmit";
        btnSubmit.Size = new Size(160, 40);
        btnSubmit.TabIndex = 1;
        btnSubmit.Text = "Tạo Nhóm Mới";
        btnSubmit.UseVisualStyleBackColor = false;
        btnSubmit.Click += btnSubmit_Click;

        // 
        // btnClose
        // 
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnClose.BackColor = Color.White;
        btnClose.Cursor = Cursors.Hand;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        btnClose.ForeColor = Color.FromArgb(30, 41, 59);
        btnClose.Location = new Point(685, 12);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(115, 40);
        btnClose.TabIndex = 0;
        btnClose.Text = "Hủy / Đóng";
        btnClose.UseVisualStyleBackColor = false;
        btnClose.Click += btnClose_Click;

        // 
        // GroupManagerForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(244, 247, 254);
        ClientSize = new Size(820, 620);
        Controls.Add(pnlBodyContainer);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        MinimumSize = new Size(720, 540);
        Name = "GroupManagerForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Tạo & Quản lý Nhóm Chat - Chatting";
        Load += GroupManagerForm_Load;

        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlRobotContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picRobotMascot).EndInit();
        pnlBodyContainer.ResumeLayout(false);
        pnlMainCard.ResumeLayout(false);
        pnlListBorderContainer.ResumeLayout(false);
        pnlFriendsEmpty.ResumeLayout(false);
        pnlMiddleHeader.ResumeLayout(false);
        pnlMiddleHeader.PerformLayout();
        pnlTabs.ResumeLayout(false);
        pnlGroupNameRow.ResumeLayout(false);
        pnlGroupNameRow.PerformLayout();
        pnlGroupNameBox.ResumeLayout(false);
        pnlGroupNameBox.PerformLayout();
        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Panel pnlRobotContainer;
    private PictureBox picRobotMascot;
    private Label lblHeaderTitle;
    private Label lblHeaderSubtitle;

    private Panel pnlBodyContainer;
    private Panel pnlMainCard;
    private Panel pnlGroupNameRow;
    private Label lblGroupName;
    private Panel pnlGroupNameBox;
    private TextBox txtGroupName;

    private Panel pnlMiddleHeader;
    private Label lblChooseFriends;
    private Panel pnlTabs;
    private Button btnTabMembers;
    private Button btnTabAddFriends;
    private Panel pnlTabIndicator;

    private Panel pnlListBorderContainer;
    private FlowLayoutPanel flpFriendsList;
    private FlowLayoutPanel flpMembersList;
    private Panel pnlFriendsEmpty;
    private Label lblFriendsEmptyIcon;
    private Label lblFriendsEmptyTitle;
    private Label lblFriendsEmptySub;

    private Panel pnlFooter;
    private Button btnLeaveGroup;
    private Button btnRemoveMember;
    private Button btnSubmit;
    private Button btnClose;

    // Legacy controls preserved for compatibility
    private GroupBox grpFriends;
    private CheckedListBox chkListFriends;
    private GroupBox grpMembers;
    private ListView lstMembers;
    private ColumnHeader colMemberName;
    private ColumnHeader colMemberRole;
    private ColumnHeader colMemberStatus;
}
