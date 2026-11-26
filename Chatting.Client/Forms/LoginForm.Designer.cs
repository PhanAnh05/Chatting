namespace Chatting.Client.Forms;

partial class LoginForm
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
        btnServerConfig = new Button();
        lblSubtitle = new Label();
        lblLogo = new Label();
        pnlBody = new Panel();
        lblServerHint = new Label();
        btnGoToRegister = new Button();
        btnForgotPassword = new Button();
        lblOr = new Label();
        btnLogin = new Button();
        lblErrorMessage = new Label();
        chkRememberMe = new CheckBox();
        txtPassword = new TextBox();
        lblPassword = new Label();
        txtEmail = new TextBox();
        lblEmail = new Label();
        pnlHeader.SuspendLayout();
        pnlBody.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(30, 41, 59);
        pnlHeader.Controls.Add(btnServerConfig);
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblLogo);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(434, 100);
        pnlHeader.TabIndex = 0;
        // 
        // btnServerConfig
        // 
        btnServerConfig.BackColor = Color.FromArgb(51, 65, 85);
        btnServerConfig.FlatAppearance.BorderSize = 0;
        btnServerConfig.FlatStyle = FlatStyle.Flat;
        btnServerConfig.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        btnServerConfig.ForeColor = Color.White;
        btnServerConfig.Location = new Point(326, 16);
        btnServerConfig.Name = "btnServerConfig";
        btnServerConfig.Size = new Size(88, 28);
        btnServerConfig.TabIndex = 2;
        btnServerConfig.Text = "  Máy chủ";
        btnServerConfig.UseVisualStyleBackColor = false;
        btnServerConfig.Click += btnServerConfig_Click;
        // 
        // lblSubtitle
        // 
        lblSubtitle.AutoSize = true;
        lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblSubtitle.Location = new Point(25, 58);
        lblSubtitle.Name = "lblSubtitle";
        lblSubtitle.Size = new Size(244, 15);
        lblSubtitle.TabIndex = 1;
        lblSubtitle.Text = "Ứng dụng Nhắn tin & Gọi điện trong Mạng LAN";
        // 
        // lblLogo
        // 
        lblLogo.AutoSize = true;
        lblLogo.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
        lblLogo.ForeColor = Color.White;
        lblLogo.Location = new Point(23, 20);
        lblLogo.Name = "lblLogo";
        lblLogo.Size = new Size(140, 32);
        lblLogo.TabIndex = 0;
        lblLogo.Text = "CHATTING";
        // 
        // pnlBody
        // 
        pnlBody.BackColor = Color.White;
        pnlBody.Controls.Add(lblServerHint);
        pnlBody.Controls.Add(btnGoToRegister);
        pnlBody.Controls.Add(btnForgotPassword);
        pnlBody.Controls.Add(lblOr);
        pnlBody.Controls.Add(btnLogin);
        pnlBody.Controls.Add(lblErrorMessage);
        pnlBody.Controls.Add(chkRememberMe);
        pnlBody.Controls.Add(txtPassword);
        pnlBody.Controls.Add(lblPassword);
        pnlBody.Controls.Add(txtEmail);
        pnlBody.Controls.Add(lblEmail);
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Location = new Point(0, 100);
        pnlBody.Name = "pnlBody";
        pnlBody.Padding = new Padding(30);
        pnlBody.Size = new Size(434, 431);
        pnlBody.TabIndex = 1;
        // 
        // lblServerHint
        // 
        lblServerHint.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        lblServerHint.ForeColor = Color.FromArgb(100, 116, 139);
        lblServerHint.Location = new Point(30, 395);
        lblServerHint.Name = "lblServerHint";
        lblServerHint.Size = new Size(374, 20);
        lblServerHint.TabIndex = 9;
        lblServerHint.Text = "Máy chủ: http://127.0.0.1:5000";
        lblServerHint.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // btnGoToRegister
        // 
        btnGoToRegister.BackColor = Color.Transparent;
        btnGoToRegister.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnGoToRegister.FlatStyle = FlatStyle.Flat;
        btnGoToRegister.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnGoToRegister.ForeColor = Color.FromArgb(13, 110, 253);
        btnGoToRegister.Location = new Point(30, 345);
        btnGoToRegister.Name = "btnGoToRegister";
        btnGoToRegister.Size = new Size(374, 38);
        btnGoToRegister.TabIndex = 8;
        btnGoToRegister.Text = "Tạo tài khoản mới (Đăng ký)";
        btnGoToRegister.UseVisualStyleBackColor = false;
        btnGoToRegister.Click += btnGoToRegister_Click;
        // 
        // btnForgotPassword
        // 
        btnForgotPassword.BackColor = Color.Transparent;
        btnForgotPassword.FlatAppearance.BorderSize = 0;
        btnForgotPassword.FlatStyle = FlatStyle.Flat;
        btnForgotPassword.Font = new Font("Segoe UI", 8.5F, FontStyle.Underline, GraphicsUnit.Point);
        btnForgotPassword.ForeColor = Color.FromArgb(37, 99, 235);
        btnForgotPassword.Location = new Point(244, 176);
        btnForgotPassword.Name = "btnForgotPassword";
        btnForgotPassword.Size = new Size(160, 26);
        btnForgotPassword.TabIndex = 10;
        btnForgotPassword.Text = "Quên mật khẩu?";
        btnForgotPassword.UseVisualStyleBackColor = false;
        btnForgotPassword.Click += btnForgotPassword_Click;
        // 
        // lblOr
        // 
        lblOr.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        lblOr.ForeColor = Color.FromArgb(148, 163, 184);
        lblOr.Location = new Point(30, 318);
        lblOr.Name = "lblOr";
        lblOr.Size = new Size(374, 18);
        lblOr.TabIndex = 7;
        lblOr.Text = "────────────────  HOẶC  ────────────────";
        lblOr.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // btnLogin
        // 
        btnLogin.BackColor = Color.FromArgb(13, 110, 253);
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        btnLogin.ForeColor = Color.White;
        btnLogin.Location = new Point(30, 260);
        btnLogin.Name = "btnLogin";
        btnLogin.Size = new Size(374, 44);
        btnLogin.TabIndex = 6;
        btnLogin.Text = "ĐĂNG NHẬP";
        btnLogin.UseVisualStyleBackColor = false;
        btnLogin.Click += btnLogin_Click;
        // 
        // lblErrorMessage
        // 
        lblErrorMessage.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        lblErrorMessage.ForeColor = Color.FromArgb(220, 38, 38);
        lblErrorMessage.Location = new Point(30, 212);
        lblErrorMessage.Name = "lblErrorMessage";
        lblErrorMessage.Size = new Size(374, 38);
        lblErrorMessage.TabIndex = 5;
        lblErrorMessage.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // chkRememberMe
        // 
        chkRememberMe.AutoSize = true;
        chkRememberMe.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        chkRememberMe.ForeColor = Color.FromArgb(71, 85, 105);
        chkRememberMe.Location = new Point(30, 180);
        chkRememberMe.Name = "chkRememberMe";
        chkRememberMe.Size = new Size(197, 19);
        chkRememberMe.TabIndex = 4;
        chkRememberMe.Text = "Ghi nhớ đăng nhập trên máy này";
        chkRememberMe.UseVisualStyleBackColor = true;
        // 
        // txtPassword
        // 
        txtPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtPassword.Location = new Point(30, 138);
        txtPassword.Name = "txtPassword";
        txtPassword.PasswordChar = '●';
        txtPassword.Size = new Size(374, 25);
        txtPassword.TabIndex = 3;
        // 
        // lblPassword
        // 
        lblPassword.AutoSize = true;
        lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblPassword.ForeColor = Color.FromArgb(30, 41, 59);
        lblPassword.Location = new Point(28, 116);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(62, 15);
        lblPassword.TabIndex = 2;
        lblPassword.Text = "Mật khẩu:";
        // 
        // txtEmail
        // 
        txtEmail.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtEmail.Location = new Point(30, 68);
        txtEmail.Name = "txtEmail";
        txtEmail.PlaceholderText = "Nhập tài khoản hoặc email...";
        txtEmail.Size = new Size(374, 25);
        txtEmail.TabIndex = 1;
        // 
        // lblEmail
        // 
        lblEmail.AutoSize = true;
        lblEmail.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblEmail.ForeColor = Color.FromArgb(30, 41, 59);
        lblEmail.Location = new Point(28, 46);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(185, 15);
        lblEmail.TabIndex = 0;
        lblEmail.Text = "Tài khoản đăng nhập (Username):";
        // 
        // LoginForm
        // 
        AcceptButton = btnLogin;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(434, 531);
        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Đăng Nhập - Chatting LAN Messenger";
        Load += LoginForm_Load;
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Label lblLogo;
    private Label lblSubtitle;
    private Button btnServerConfig;
    private Panel pnlBody;
    private Label lblEmail;
    private TextBox txtEmail;
    private Label lblPassword;
    private TextBox txtPassword;
    private CheckBox chkRememberMe;
    private Label lblErrorMessage;
    private Button btnLogin;
    private Label lblOr;
    private Button btnGoToRegister;
    private Label lblServerHint;
    private Button btnForgotPassword;
}
