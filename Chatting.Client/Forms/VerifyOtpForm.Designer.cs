namespace Chatting.Client.Forms;

partial class VerifyOtpForm
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
        lblHeaderSubtitle = new Label();
        lblHeaderTitle = new Label();
        pnlBody = new Panel();
        btnBackToLogin = new Button();
        lblCountdown = new Label();
        btnResendOtp = new Button();
        btnVerify = new Button();
        lblStatusMessage = new Label();
        txtOtpCode = new TextBox();
        lblOtpPrompt = new Label();
        lblEmailNotice = new Label();
        timerCountdown = new System.Windows.Forms.Timer(components);
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
        pnlHeader.Size = new Size(440, 85);
        pnlHeader.TabIndex = 0;
        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.AutoSize = true;
        lblHeaderSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblHeaderSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
        lblHeaderSubtitle.Location = new Point(25, 48);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new Size(297, 15);
        lblHeaderSubtitle.TabIndex = 1;
        lblHeaderSubtitle.Text = "Nhập mã OTP 6 chữ số vừa được gửi đến hòm thư của bạn";
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(23, 16);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(239, 30);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "XÁC MINH TÀI KHOẢN";
        // 
        // pnlBody
        // 
        pnlBody.BackColor = Color.White;
        pnlBody.Controls.Add(btnBackToLogin);
        pnlBody.Controls.Add(lblCountdown);
        pnlBody.Controls.Add(btnResendOtp);
        pnlBody.Controls.Add(btnVerify);
        pnlBody.Controls.Add(lblStatusMessage);
        pnlBody.Controls.Add(txtOtpCode);
        pnlBody.Controls.Add(lblOtpPrompt);
        pnlBody.Controls.Add(lblEmailNotice);
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Location = new Point(0, 85);
        pnlBody.Name = "pnlBody";
        pnlBody.Padding = new Padding(30);
        pnlBody.Size = new Size(440, 355);
        pnlBody.TabIndex = 1;
        // 
        // btnBackToLogin
        // 
        btnBackToLogin.BackColor = Color.Transparent;
        btnBackToLogin.FlatAppearance.BorderSize = 0;
        btnBackToLogin.FlatStyle = FlatStyle.Flat;
        btnBackToLogin.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnBackToLogin.ForeColor = Color.FromArgb(100, 116, 139);
        btnBackToLogin.Location = new Point(30, 298);
        btnBackToLogin.Name = "btnBackToLogin";
        btnBackToLogin.Size = new Size(380, 32);
        btnBackToLogin.TabIndex = 7;
        btnBackToLogin.Text = "← Quay lại trang Đăng nhập";
        btnBackToLogin.UseVisualStyleBackColor = false;
        btnBackToLogin.Click += btnBackToLogin_Click;
        // 
        // lblCountdown
        // 
        lblCountdown.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic, GraphicsUnit.Point);
        lblCountdown.ForeColor = Color.FromArgb(100, 116, 139);
        lblCountdown.Location = new Point(210, 248);
        lblCountdown.Name = "lblCountdown";
        lblCountdown.Size = new Size(200, 23);
        lblCountdown.TabIndex = 6;
        lblCountdown.Text = "Gửi lại sau: 60s";
        lblCountdown.TextAlign = ContentAlignment.MiddleRight;
        // 
        // btnResendOtp
        // 
        btnResendOtp.BackColor = Color.FromArgb(241, 245, 249);
        btnResendOtp.Enabled = false;
        btnResendOtp.FlatStyle = FlatStyle.System;
        btnResendOtp.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        btnResendOtp.Location = new Point(30, 244);
        btnResendOtp.Name = "btnResendOtp";
        btnResendOtp.Size = new Size(160, 30);
        btnResendOtp.TabIndex = 5;
        btnResendOtp.Text = "Gửi lại mã OTP";
        btnResendOtp.UseVisualStyleBackColor = false;
        btnResendOtp.Click += btnResendOtp_Click;
        // 
        // btnVerify
        // 
        btnVerify.BackColor = Color.FromArgb(13, 110, 253);
        btnVerify.FlatStyle = FlatStyle.Flat;
        btnVerify.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        btnVerify.ForeColor = Color.White;
        btnVerify.Location = new Point(30, 185);
        btnVerify.Name = "btnVerify";
        btnVerify.Size = new Size(380, 44);
        btnVerify.TabIndex = 4;
        btnVerify.Text = "XÁC MINH NGAY";
        btnVerify.UseVisualStyleBackColor = false;
        btnVerify.Click += btnVerify_Click;
        // 
        // lblStatusMessage
        // 
        lblStatusMessage.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
        lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38);
        lblStatusMessage.Location = new Point(30, 145);
        lblStatusMessage.Name = "lblStatusMessage";
        lblStatusMessage.Size = new Size(380, 32);
        lblStatusMessage.TabIndex = 3;
        lblStatusMessage.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // txtOtpCode
        // 
        txtOtpCode.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point);
        txtOtpCode.ForeColor = Color.FromArgb(15, 23, 42);
        txtOtpCode.Location = new Point(70, 92);
        txtOtpCode.MaxLength = 6;
        txtOtpCode.Name = "txtOtpCode";
        txtOtpCode.PlaceholderText = "••••••";
        txtOtpCode.Size = new Size(300, 43);
        txtOtpCode.TabIndex = 2;
        txtOtpCode.TextAlign = HorizontalAlignment.Center;
        // 
        // lblOtpPrompt
        // 
        lblOtpPrompt.AutoSize = true;
        lblOtpPrompt.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblOtpPrompt.ForeColor = Color.FromArgb(30, 41, 59);
        lblOtpPrompt.Location = new Point(148, 65);
        lblOtpPrompt.Name = "lblOtpPrompt";
        lblOtpPrompt.Size = new Size(144, 17);
        lblOtpPrompt.TabIndex = 1;
        lblOtpPrompt.Text = "Nhập mã OTP (6 số):";
        // 
        // lblEmailNotice
        // 
        lblEmailNotice.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblEmailNotice.ForeColor = Color.FromArgb(13, 110, 253);
        lblEmailNotice.Location = new Point(30, 15);
        lblEmailNotice.Name = "lblEmailNotice";
        lblEmailNotice.Size = new Size(380, 40);
        lblEmailNotice.TabIndex = 0;
        lblEmailNotice.Text = "Mã xác minh đã gửi đến email:\r\nuser@example.com";
        lblEmailNotice.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // timerCountdown
        // 
        timerCountdown.Interval = 1000;
        timerCountdown.Tick += timerCountdown_Tick;
        // 
        // VerifyOtpForm
        // 
        AcceptButton = btnVerify;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(440, 440);
        Controls.Add(pnlBody);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "VerifyOtpForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Xác Minh Tài Khoản - Chatting";
        Load += VerifyOtpForm_Load;
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
    private Label lblEmailNotice;
    private Label lblOtpPrompt;
    private TextBox txtOtpCode;
    private Label lblStatusMessage;
    private Button btnVerify;
    private Button btnResendOtp;
    private Label lblCountdown;
    private Button btnBackToLogin;
    private System.Windows.Forms.Timer timerCountdown;
}
