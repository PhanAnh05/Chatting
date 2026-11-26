namespace Chatting.Client.Forms;

partial class VideoCallForm
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
        pnlControlBar = new Panel();
        lblDuration = new Label();
        btnEndCall = new Button();
        btnToggleCam = new Button();
        btnToggleMic = new Button();
        lblCallInfo = new Label();
        wvVideoCall = new Microsoft.Web.WebView2.WinForms.WebView2();
        timerCallDuration = new System.Windows.Forms.Timer(components);
        pnlControlBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)wvVideoCall).BeginInit();
        SuspendLayout();
        // 
        // pnlControlBar
        // 
        pnlControlBar.BackColor = Color.FromArgb(15, 23, 42);
        pnlControlBar.Controls.Add(lblDuration);
        pnlControlBar.Controls.Add(btnEndCall);
        pnlControlBar.Controls.Add(btnToggleCam);
        pnlControlBar.Controls.Add(btnToggleMic);
        pnlControlBar.Controls.Add(lblCallInfo);
        pnlControlBar.Dock = DockStyle.Bottom;
        pnlControlBar.Location = new Point(0, 526);
        pnlControlBar.Name = "pnlControlBar";
        pnlControlBar.Size = new Size(884, 75);
        pnlControlBar.TabIndex = 0;
        // 
        // lblDuration
        // 
        lblDuration.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lblDuration.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
        lblDuration.ForeColor = Color.FromArgb(56, 189, 248);
        lblDuration.Location = new Point(740, 26);
        lblDuration.Name = "lblDuration";
        lblDuration.Size = new Size(120, 25);
        lblDuration.TabIndex = 4;
        lblDuration.Text = "00:00";
        lblDuration.TextAlign = ContentAlignment.MiddleRight;
        // 
        // btnEndCall
        // 
        btnEndCall.Anchor = AnchorStyles.Bottom;
        btnEndCall.BackColor = Color.FromArgb(220, 38, 38);
        btnEndCall.FlatStyle = FlatStyle.Flat;
        btnEndCall.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        btnEndCall.ForeColor = Color.White;
        btnEndCall.Location = new Point(540, 18);
        btnEndCall.Name = "btnEndCall";
        btnEndCall.Size = new Size(110, 40);
        btnEndCall.TabIndex = 3;
        btnEndCall.Text = "Kết Thúc";
        btnEndCall.UseVisualStyleBackColor = false;
        btnEndCall.Click += btnEndCall_Click;
        // 
        // btnToggleCam
        // 
        btnToggleCam.Anchor = AnchorStyles.Bottom;
        btnToggleCam.BackColor = Color.FromArgb(51, 65, 85);
        btnToggleCam.FlatStyle = FlatStyle.Flat;
        btnToggleCam.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        btnToggleCam.ForeColor = Color.White;
        btnToggleCam.Location = new Point(420, 18);
        btnToggleCam.Name = "btnToggleCam";
        btnToggleCam.Size = new Size(100, 40);
        btnToggleCam.TabIndex = 2;
        btnToggleCam.Text = "Tắt Cam";
        btnToggleCam.UseVisualStyleBackColor = false;
        btnToggleCam.Click += btnToggleCam_Click;
        // 
        // btnToggleMic
        // 
        btnToggleMic.Anchor = AnchorStyles.Bottom;
        btnToggleMic.BackColor = Color.FromArgb(51, 65, 85);
        btnToggleMic.FlatStyle = FlatStyle.Flat;
        btnToggleMic.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point);
        btnToggleMic.ForeColor = Color.White;
        btnToggleMic.Location = new Point(300, 18);
        btnToggleMic.Name = "btnToggleMic";
        btnToggleMic.Size = new Size(100, 40);
        btnToggleMic.TabIndex = 1;
        btnToggleMic.Text = "Tắt Mic";
        btnToggleMic.UseVisualStyleBackColor = false;
        btnToggleMic.Click += btnToggleMic_Click;
        // 
        // lblCallInfo
        // 
        lblCallInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblCallInfo.AutoEllipsis = true;
        lblCallInfo.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        lblCallInfo.ForeColor = Color.White;
        lblCallInfo.Location = new Point(24, 25);
        lblCallInfo.Name = "lblCallInfo";
        lblCallInfo.Size = new Size(260, 25);
        lblCallInfo.TabIndex = 0;
        lblCallInfo.Text = "Đang kết nối...";
        lblCallInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // wvVideoCall
        // 
        wvVideoCall.AllowExternalDrop = true;
        wvVideoCall.CreationProperties = null;
        wvVideoCall.DefaultBackgroundColor = Color.FromArgb(17, 24, 39);
        wvVideoCall.Dock = DockStyle.Fill;
        wvVideoCall.Location = new Point(0, 0);
        wvVideoCall.Name = "wvVideoCall";
        wvVideoCall.Size = new Size(884, 526);
        wvVideoCall.TabIndex = 1;
        wvVideoCall.ZoomFactor = 1D;
        // 
        // timerCallDuration
        // 
        timerCallDuration.Interval = 1000;
        timerCallDuration.Tick += timerCallDuration_Tick;
        // 
        // VideoCallForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(17, 24, 39);
        ClientSize = new Size(884, 601);
        Controls.Add(wvVideoCall);
        Controls.Add(pnlControlBar);
        MinimumSize = new Size(640, 480);
        Name = "VideoCallForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Cuộc Gọi Video - Chatting";
        FormClosing += VideoCallForm_FormClosing;
        Load += VideoCallForm_Load;
        pnlControlBar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)wvVideoCall).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlControlBar;
    private Label lblCallInfo;
    private Button btnToggleMic;
    private Button btnToggleCam;
    private Button btnEndCall;
    private Label lblDuration;
    private Microsoft.Web.WebView2.WinForms.WebView2 wvVideoCall;
    private System.Windows.Forms.Timer timerCallDuration;
}
