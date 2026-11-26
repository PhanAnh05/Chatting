namespace Chatting.Client.Forms;

partial class RegisterForm
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
        lblHeaderSubtitle = new Label();
        lblHeaderTitle = new Label();
        pnlBody = new Panel();
        btnBackToLogin = new Button();
        btnRegister = new Button();
        lblErrorMessage = new Label();
        txtConfirmPassword = new TextBox();
        lblConfirmPassword = new Label();
        txtPassword = new TextBox();
        lblPassword = new Label();
        txtEmail = new TextBox();
        lblEmail = new Label();
        txtDisplayName = new TextBox();
        lblDisplayName = new Label();
        txtUsername = new TextBox();
        lblUsername = new Label();
        pnlHeader.SuspendLayout();
        pnlBody.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(30, 41, 59);
        pnlHeader.Controls.Add(lblHeaderSubtitle);
        pnlHeader.Controls.Add(lblHeaderTitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(434, 85);
        pnlHeader.TabIndex = 0;
        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.AutoSize = true;
        lblHeaderSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblHeaderSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblHeaderSubtitle.Location = new Point(25, 48);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new Size(292, 15);
        lblHeaderSubtitle.TabIndex = 1;
        lblHeaderSubtitle.Text = "Đăng ký tài khoản để kết nối bạn bè trong mạng LAN";
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(23, 16);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(244, 30);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "TẠO TÀI KHOẢN MỚI";
        // 
        // pnlBody
        // 
        pnlBody.BackColor = Color.White;
        pnlBody.Controls.Add(btnBackToLogin);
        pnlBody.Controls.Add(btnRegister);
        pnlBody.Controls.Add(lblErrorMessage);
        pnlBody.Controls.Add(txtConfirmPassword);
        pnlBody.Controls.Add(lblConfirmPassword);
        pnlBody.Controls.Add(txtPassword);
        pnlBody.Controls.Add(lblPassword);
        pnlBody.Controls.Add(txtEmail);
        pnlBody.Controls.Add(lblEmail);
        pnlBody.Controls.Add(txtDisplayName);
        pnlBody.Controls.Add(lblDisplayName);
        pnlBody.Controls.Add(txtUsername);
        pnlBody.Controls.Add(lblUsername);
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Location = new Point(0, 85);
        pnlBody.Name = "pnlBody";
        pnlBody.Padding = new Padding(30);
        pnlBody.Size = new Size(434, 535);
        pnlBody.TabIndex = 1;
        // 
        // lblUsername
        // 
        lblUsername.AutoSize = true;
        lblUsername.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblUsername.ForeColor = Color.FromArgb(30, 41, 59);
        lblUsername.Location = new Point(28, 20);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(185, 15);
        lblUsername.TabIndex = 0;
        lblUsername.Text = "Tài khoản đăng nhập (Username):";
        // 
        // txtUsername
        // 
        txtUsername.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtUsername.Location = new Point(30, 40);
        txtUsername.Name = "txtUsername";
        txtUsername.PlaceholderText = "Ví dụ: nguyenvana hoặc john99";
        txtUsername.Size = new Size(374, 25);
        txtUsername.TabIndex = 1;
        // 
        // lblDisplayName
        // 
        lblDisplayName.AutoSize = true;
        lblDisplayName.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblDisplayName.ForeColor = Color.FromArgb(30, 41, 59);
        lblDisplayName.Location = new Point(28, 77);
        lblDisplayName.Name = "lblDisplayName";
        lblDisplayName.Size = new Size(139, 15);
        lblDisplayName.TabIndex = 2;
        lblDisplayName.Text = "Tên hiển thị (Họ và tên):";
        // 
        // txtDisplayName
        // 
        txtDisplayName.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtDisplayName.Location = new Point(30, 97);
        txtDisplayName.Name = "txtDisplayName";
        txtDisplayName.PlaceholderText = "Ví dụ: Nguyễn Văn A";
        txtDisplayName.Size = new Size(374, 25);
        txtDisplayName.TabIndex = 3;
        // 
        // lblEmail
        // 
        lblEmail.AutoSize = true;
        lblEmail.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblEmail.ForeColor = Color.FromArgb(30, 41, 59);
        lblEmail.Location = new Point(28, 134);
        lblEmail.Name = "lblEmail";
        lblEmail.Size = new Size(160, 15);
        lblEmail.TabIndex = 4;
        lblEmail.Text = "Địa chỉ Email (để nhận OTP):";
        // 
        // txtEmail
        // 
        txtEmail.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtEmail.Location = new Point(30, 154);
        txtEmail.Name = "txtEmail";
        txtEmail.PlaceholderText = "name@company.com";
        txtEmail.Size = new Size(374, 25);
        txtEmail.TabIndex = 5;
        // 
        // lblPassword
        // 
        lblPassword.AutoSize = true;
        lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblPassword.ForeColor = Color.FromArgb(30, 41, 59);
        lblPassword.Location = new Point(28, 191);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(62, 15);
        lblPassword.TabIndex = 6;
        lblPassword.Text = "Mật khẩu:";
        // 
        // txtPassword
        // 
        txtPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtPassword.Location = new Point(30, 211);
        txtPassword.Name = "txtPassword";
        txtPassword.PasswordChar = '●';
        txtPassword.Size = new Size(374, 25);
        txtPassword.TabIndex = 7;
        // 
        // lblConfirmPassword
        // 
        lblConfirmPassword.AutoSize = true;
        lblConfirmPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblConfirmPassword.ForeColor = Color.FromArgb(30, 41, 59);
        lblConfirmPassword.Location = new Point(28, 248);
        lblConfirmPassword.Name = "lblConfirmPassword";
        lblConfirmPassword.Size = new Size(119, 15);
        lblConfirmPassword.TabIndex = 8;
        lblConfirmPassword.Text = "Xác nhận mật khẩu:";
        // 
        // txtConfirmPassword
        // 
        txtConfirmPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtConfirmPassword.Location = new Point(30, 268);
        txtConfirmPassword.Name = "txtConfirmPassword";
        txtConfirmPassword.PasswordChar = '●';
        txtConfirmPassword.Size = new Size(374, 25);
        txtConfirmPassword.TabIndex = 9;
        // 
        // lblErrorMessage
        // 
        lblErrorMessage.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        lblErrorMessage.ForeColor = Color.FromArgb(220, 38, 38);
        lblErrorMessage.Location = new Point(30, 305);
        lblErrorMessage.Name = "lblErrorMessage";
        lblErrorMessage.Size = new Size(374, 38);
        lblErrorMessage.TabIndex = 10;
        lblErrorMessage.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // btnRegister
        // 
        btnRegister.BackColor = Color.FromArgb(22, 163, 74);
        btnRegister.FlatStyle = FlatStyle.Flat;
        btnRegister.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        btnRegister.ForeColor = Color.White;
        btnRegister.Location = new Point(30, 350);
        btnRegister.Name = "btnRegister";
        btnRegister.Size = new Size(374, 44);
        btnRegister.TabIndex = 11;
        btnRegister.Text = "ĐĂNG KÝ NGAY";
        btnRegister.UseVisualStyleBackColor = false;
        btnRegister.Click += btnRegister_Click;
        // 
        // btnBackToLogin
        // 
        btnBackToLogin.BackColor = Color.Transparent;
        btnBackToLogin.FlatAppearance.BorderSize = 0;
        btnBackToLogin.FlatStyle = FlatStyle.Flat;
        btnBackToLogin.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnBackToLogin.ForeColor = Color.FromArgb(13, 110, 253);
        btnBackToLogin.Location = new Point(30, 404);
        btnBackToLogin.Name = "btnBackToLogin";
        btnBackToLogin.Size = new Size(374, 32);
        btnBackToLogin.TabIndex = 12;
        btnBackToLogin.Text = "Đã có tài khoản? Quay lại Đăng nhập";
        btnBackToLogin.UseVisualStyleBackColor = false;
        btnBackToLogin.Click += btnBackToLogin_Click;
        // 
        // RegisterForm
        // 
        AcceptButton = btnRegister;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(434, 550);
        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "RegisterForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Đăng Ký Tài Khoản - Chatting";
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Label lblHeaderTitle;
    private Label lblHeaderSubtitle;
    private Panel pnlBody;
    private Label lblUsername;
    private TextBox txtUsername;
    private Label lblDisplayName;
    private TextBox txtDisplayName;
    private Label lblEmail;
    private TextBox txtEmail;
    private Label lblPassword;
    private TextBox txtPassword;
    private Label lblConfirmPassword;
    private TextBox txtConfirmPassword;
    private Label lblErrorMessage;
    private Button btnRegister;
    private Button btnBackToLogin;
}
