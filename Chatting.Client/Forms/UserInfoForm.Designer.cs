namespace Chatting.Client.Forms;

partial class UserInfoForm
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
        pnlHeader = new Panel();
        toolTipUserInfo = new ToolTip(components);
        lblHeaderTitle = new Label();
        btnCloseTop = new Button();
        pnlBody = new Panel();
        btnClose = new Button();
        lblStatus = new Label();
        lblEmail = new Label();
        lblEmailTitle = new Label();
        lblUsername = new Label();
        lblUsernameTitle = new Label();
        lblDisplayName = new Label();
        picAvatar = new PictureBox();
        pnlHeader.SuspendLayout();
        pnlBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(18, 24, 38);
        pnlHeader.Controls.Add(lblHeaderTitle);
        pnlHeader.Controls.Add(btnCloseTop);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(380, 50);
        pnlHeader.TabIndex = 0;
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(18, 14);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(150, 19);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "Thông Tin Người Dùng";
        // 
        // btnCloseTop
        // 
        btnCloseTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCloseTop.BackColor = Color.Transparent;
        btnCloseTop.FlatAppearance.BorderSize = 0;
        btnCloseTop.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 68, 68);
        btnCloseTop.FlatStyle = FlatStyle.Flat;
        btnCloseTop.Cursor = Cursors.Hand;
        btnCloseTop.Font = new Font("Segoe MDL2 Assets", 9F);
        btnCloseTop.ForeColor = Color.FromArgb(148, 163, 184);
        btnCloseTop.Location = new Point(340, 8);
        btnCloseTop.Name = "btnCloseTop";
        btnCloseTop.Size = new Size(32, 32);
        btnCloseTop.TabIndex = 1;
        btnCloseTop.Text = "\uE711";
        btnCloseTop.AccessibleName = "Đóng thông tin người dùng";
        btnCloseTop.AccessibleDescription = "Đóng cửa sổ thông tin người dùng.";
        toolTipUserInfo.SetToolTip(btnCloseTop, "Đóng");
        btnCloseTop.UseVisualStyleBackColor = false;
        btnCloseTop.Click += (s, e) => Close();
        // 
        // pnlBody
        // 
        pnlBody.BackColor = Color.White;
        pnlBody.Controls.Add(btnClose);
        pnlBody.Controls.Add(lblStatus);
        pnlBody.Controls.Add(lblEmail);
        pnlBody.Controls.Add(lblEmailTitle);
        pnlBody.Controls.Add(lblUsername);
        pnlBody.Controls.Add(lblUsernameTitle);
        pnlBody.Controls.Add(lblDisplayName);
        pnlBody.Controls.Add(picAvatar);
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Location = new Point(0, 50);
        pnlBody.Name = "pnlBody";
        pnlBody.Padding = new Padding(25, 20, 25, 20);
        pnlBody.Size = new Size(380, 390);
        pnlBody.TabIndex = 1;
        // 
        // btnClose
        // 
        btnClose.BackColor = Color.FromArgb(37, 99, 235);
        btnClose.Cursor = Cursors.Hand;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        btnClose.ForeColor = Color.White;
        btnClose.Location = new Point(25, 325);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(330, 42);
        btnClose.TabIndex = 7;
        btnClose.Text = "Đóng";
        btnClose.UseVisualStyleBackColor = false;
        btnClose.Click += (s, e) => Close();
        // 
        // lblStatus
        // 
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(22, 163, 74);
        lblStatus.Location = new Point(25, 145);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(330, 22);
        lblStatus.TabIndex = 6;
        lblStatus.Text = "● Đang trực tuyến";
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblEmail
        // 
        lblEmail.AutoEllipsis = true;
        lblEmail.Font = new Font("Segoe UI", 9.5F);
        lblEmail.ForeColor = Color.FromArgb(30, 41, 59);
        lblEmail.Location = new Point(25, 260);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(330, 25);
        lblEmail.TabIndex = 5;
        lblEmail.Text = "email@example.com";
        // 
        // lblEmailTitle
        // 
        lblEmailTitle.AutoSize = true;
        lblEmailTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblEmailTitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblEmailTitle.Location = new Point(25, 240);
        lblEmailTitle.Name = "lblEmailTitle";
        lblEmailTitle.Size = new Size(86, 13);
        lblEmailTitle.TabIndex = 4;
        lblEmailTitle.Text = "ĐỊA CHỈ EMAIL";
        // 
        // lblUsername
        // 
        lblUsername.AutoEllipsis = true;
        lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblUsername.ForeColor = Color.FromArgb(37, 99, 235);
        lblUsername.Location = new Point(25, 200);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(330, 25);
        lblUsername.TabIndex = 3;
        lblUsername.Text = "@username";
        // 
        // lblUsernameTitle
        // 
        lblUsernameTitle.AutoSize = true;
        lblUsernameTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblUsernameTitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblUsernameTitle.Location = new Point(25, 180);
        lblUsernameTitle.Name = "lblUsernameTitle";
        lblUsernameTitle.Size = new Size(134, 13);
        lblUsernameTitle.TabIndex = 2;
        lblUsernameTitle.Text = "TÀI KHOẢN ĐĂNG NHẬP";
        // 
        // lblDisplayName
        // 
        lblDisplayName.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblDisplayName.ForeColor = Color.FromArgb(15, 23, 42);
        lblDisplayName.Location = new Point(25, 115);
        lblDisplayName.Name = "lblDisplayName";
        lblDisplayName.Size = new Size(330, 28);
        lblDisplayName.TabIndex = 1;
        lblDisplayName.Text = "Tên Hiển Thị";
        lblDisplayName.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // picAvatar
        // 
        picAvatar.BackColor = Color.FromArgb(59, 130, 246);
        picAvatar.Location = new Point(150, 20);
        picAvatar.Name = "picAvatar";
        picAvatar.Size = new Size(80, 80);
        picAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picAvatar.TabIndex = 0;
        picAvatar.TabStop = false;
        // 
        // UserInfoForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(380, 440);
        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.None;
        Name = "UserInfoForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thông Tin Người Dùng";
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Label lblHeaderTitle;
    private ToolTip toolTipUserInfo;
    private Button btnCloseTop;
    private Panel pnlBody;
    private PictureBox picAvatar;
    private Label lblDisplayName;
    private Label lblStatus;
    private Label lblUsernameTitle;
    private Label lblUsername;
    private Label lblEmailTitle;
    private Label lblEmail;
    private Button btnClose;
}
