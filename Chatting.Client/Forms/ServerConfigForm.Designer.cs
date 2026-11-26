namespace Chatting.Client.Forms;

partial class ServerConfigForm
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
        lblTip = new Label();
        lblStatus = new Label();
        btnPing = new Button();
        txtServerUrl = new TextBox();
        lblServerUrl = new Label();
        pnlFooter = new Panel();
        btnCancel = new Button();
        btnSave = new Button();
        pnlHeader.SuspendLayout();
        pnlBody.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(13, 110, 253);
        pnlHeader.Controls.Add(lblHeaderSubtitle);
        pnlHeader.Controls.Add(lblHeaderTitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(504, 70);
        pnlHeader.TabIndex = 0;
        // 
        // lblHeaderSubtitle
        // 
        lblHeaderSubtitle.AutoSize = true;
        lblHeaderSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        lblHeaderSubtitle.ForeColor = Color.FromArgb(224, 238, 255);
        lblHeaderSubtitle.Location = new Point(18, 40);
        lblHeaderSubtitle.Name = "lblHeaderSubtitle";
        lblHeaderSubtitle.Size = new Size(335, 15);
        lblHeaderSubtitle.TabIndex = 1;
        lblHeaderSubtitle.Text = "Nhập địa chỉ IP máy chủ Chatting đang lắng nghe trong mạng LAN";
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.AutoSize = true;
        lblHeaderTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
        lblHeaderTitle.ForeColor = Color.White;
        lblHeaderTitle.Location = new Point(16, 14);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(244, 21);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "CẤU HÌNH MÁY CHỦ MẠNG LAN";
        // 
        // pnlBody
        // 
        pnlBody.BackColor = Color.White;
        pnlBody.Controls.Add(lblTip);
        pnlBody.Controls.Add(lblStatus);
        pnlBody.Controls.Add(btnPing);
        pnlBody.Controls.Add(txtServerUrl);
        pnlBody.Controls.Add(lblServerUrl);
        pnlBody.Dock = DockStyle.Fill;
        pnlBody.Location = new Point(0, 70);
        pnlBody.Name = "pnlBody";
        pnlBody.Padding = new Padding(20);
        pnlBody.Size = new Size(504, 161);
        pnlBody.TabIndex = 1;
        // 
        // lblTip
        // 
        lblTip.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic, GraphicsUnit.Point);
        lblTip.ForeColor = Color.DimGray;
        lblTip.Location = new Point(20, 115);
        lblTip.Name = "lblTip";
        lblTip.Size = new Size(464, 35);
        lblTip.TabIndex = 4;
        lblTip.Text = "Gợi ý: Nếu chạy giữa 2 máy tính trong mạng Wi-Fi/LAN, hãy nhập http://<IP_MAY_CHU>:5000 (Ví dụ: http://192.168.1.15:5000).";
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblStatus.Location = new Point(20, 85);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(204, 15);
        lblStatus.TabIndex = 3;
        lblStatus.Text = "Trạng thái: Chưa kiểm tra kết nối";
        // 
        // btnPing
        // 
        btnPing.BackColor = Color.FromArgb(241, 245, 249);
        btnPing.FlatStyle = FlatStyle.System;
        btnPing.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnPing.Location = new Point(364, 42);
        btnPing.Name = "btnPing";
        btnPing.Size = new Size(120, 29);
        btnPing.TabIndex = 2;
        btnPing.Text = "Kiểm tra (Ping)";
        btnPing.UseVisualStyleBackColor = false;
        btnPing.Click += btnPing_Click;
        // 
        // txtServerUrl
        // 
        txtServerUrl.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        txtServerUrl.Location = new Point(20, 44);
        txtServerUrl.Name = "txtServerUrl";
        txtServerUrl.PlaceholderText = "http://192.168.1.x:5000";
        txtServerUrl.Size = new Size(335, 25);
        txtServerUrl.TabIndex = 1;
        // 
        // lblServerUrl
        // 
        lblServerUrl.AutoSize = true;
        lblServerUrl.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        lblServerUrl.ForeColor = Color.FromArgb(30, 41, 59);
        lblServerUrl.Location = new Point(18, 20);
        lblServerUrl.Name = "lblServerUrl";
        lblServerUrl.Size = new Size(130, 15);
        lblServerUrl.TabIndex = 0;
        lblServerUrl.Text = "Địa chỉ Server (URL):";
        // 
        // pnlFooter
        // 
        pnlFooter.BackColor = Color.FromArgb(248, 250, 252);
        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Controls.Add(btnSave);
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Location = new Point(0, 231);
        pnlFooter.Name = "pnlFooter";
        pnlFooter.Size = new Size(504, 55);
        pnlFooter.TabIndex = 2;
        // 
        // btnCancel
        // 
        btnCancel.BackColor = Color.White;
        btnCancel.FlatStyle = FlatStyle.System;
        btnCancel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnCancel.Location = new Point(399, 12);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(85, 31);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Đóng";
        btnCancel.UseVisualStyleBackColor = false;
        btnCancel.Click += btnCancel_Click;
        // 
        // btnSave
        // 
        btnSave.BackColor = Color.FromArgb(13, 110, 253);
        btnSave.FlatStyle = FlatStyle.System;
        btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(274, 12);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(115, 31);
        btnSave.TabIndex = 0;
        btnSave.Text = "Lưu Cấu Hình";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += btnSave_Click;
        // 
        // ServerConfigForm
        // 
        AcceptButton = btnSave;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(504, 286);
        Controls.Add(pnlBody);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ServerConfigForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Cấu hình Máy chủ Chatting";
        Load += ServerConfigForm_Load;
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        pnlFooter.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Label lblHeaderTitle;
    private Label lblHeaderSubtitle;
    private Panel pnlBody;
    private Label lblServerUrl;
    private TextBox txtServerUrl;
    private Button btnPing;
    private Label lblStatus;
    private Label lblTip;
    private Panel pnlFooter;
    private Button btnSave;
    private Button btnCancel;
}
