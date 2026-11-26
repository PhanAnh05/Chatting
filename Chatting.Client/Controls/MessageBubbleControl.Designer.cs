using Microsoft.Web.WebView2.WinForms;

namespace Chatting.Client.Controls;

partial class MessageBubbleControl
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

    #region Component Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        picSenderAvatar = new PictureBox();
        lblSenderName = new Label();
        pnlBubble = new Panel();
        lblContent = new Label();
        picImage = new PictureBox();
        pnlDocument = new Panel();
        btnDocDownload = new Button();
        btnDocOpen = new Button();
        lblDocSize = new Label();
        lblDocName = new Label();
        lblDocIcon = new Label();
        pnlVoice = new Panel();
        lblVoiceTime = new Label();
        pnlVoiceProgress = new Panel();
        pnlVoiceBar = new Panel();
        btnPlayVoice = new Button();
        pnlVideoInfo = new Panel();
        lblVideoInfoIcon = new Label();
        lblVideoInfoName = new Label();
        lblVideoInfoSize = new Label();
        btnVideoInfoOpen = new Button();
        btnVideoInfoDownload = new Button();
        lblMeta = new Label();
        ctxMenu = new ContextMenuStrip(components);
        toolTipMessageBubble = new ToolTip(components);
        menuItemCopy = new ToolStripMenuItem();
        menuItemOpen = new ToolStripMenuItem();
        menuItemDownload = new ToolStripMenuItem();
        menuItemEdit = new ToolStripMenuItem();
        menuItemRevoke = new ToolStripMenuItem();
        menuItemDelete = new ToolStripMenuItem();
        ((System.ComponentModel.ISupportInitialize)picSenderAvatar).BeginInit();
        pnlBubble.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
        pnlDocument.SuspendLayout();
        pnlVoice.SuspendLayout();
        pnlVoiceProgress.SuspendLayout();
        pnlVideoInfo.SuspendLayout();
        ctxMenu.SuspendLayout();
        SuspendLayout();
        // picSenderAvatar
        picSenderAvatar.BackColor = Color.FromArgb(224, 231, 255);
        picSenderAvatar.Location = new Point(8, 6);
        picSenderAvatar.Name = "picSenderAvatar";
        picSenderAvatar.Size = new Size(32, 32);
        picSenderAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picSenderAvatar.TabIndex = 0;
        picSenderAvatar.TabStop = false;
        // lblSenderName
        lblSenderName.AutoSize = true;
        lblSenderName.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
        lblSenderName.ForeColor = Color.FromArgb(100, 116, 139);
        lblSenderName.Location = new Point(48, 4);
        lblSenderName.Name = "lblSenderName";
        lblSenderName.Size = new Size(67, 13);
        lblSenderName.TabIndex = 1;
        lblSenderName.Text = "Nguoi dung";
        lblSenderName.Visible = false;
        // pnlBubble
        pnlBubble.BackColor = Color.FromArgb(241, 245, 249);
        pnlBubble.Controls.Add(pnlVoice);
        pnlBubble.Controls.Add(pnlDocument);
        pnlBubble.Controls.Add(pnlVideoInfo);
        pnlBubble.Controls.Add(picImage);
        pnlBubble.Controls.Add(lblContent);
        pnlBubble.Location = new Point(48, 20);
        pnlBubble.Name = "pnlBubble";
        pnlBubble.Padding = new Padding(10, 8, 10, 8);
        pnlBubble.Size = new Size(360, 120);
        pnlBubble.TabIndex = 2;
        // lblContent
        lblContent.AutoSize = true;
        lblContent.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblContent.ForeColor = Color.FromArgb(15, 23, 42);
        lblContent.Location = new Point(10, 8);
        lblContent.MaximumSize = new Size(396, 0);
        lblContent.Name = "lblContent";
        lblContent.Size = new Size(119, 17);
        lblContent.TabIndex = 0;
        lblContent.Text = "Noi dung tin nhan...";
        lblContent.UseCompatibleTextRendering = true;
        // picImage
        picImage.BackColor = Color.FromArgb(226, 232, 240);
        picImage.Cursor = Cursors.Hand;
        picImage.Location = new Point(10, 10);
        picImage.Name = "picImage";
        picImage.Size = new Size(240, 150);
        picImage.SizeMode = PictureBoxSizeMode.Zoom;
        picImage.TabIndex = 1;
        picImage.TabStop = false;
        picImage.Visible = false;
        picImage.Click += picImage_Click;
        // pnlDocument
        pnlDocument.BackColor = Color.FromArgb(248, 250, 252);
        pnlDocument.Controls.Add(btnDocDownload);
        pnlDocument.Controls.Add(btnDocOpen);
        pnlDocument.Controls.Add(lblDocSize);
        pnlDocument.Controls.Add(lblDocName);
        pnlDocument.Controls.Add(lblDocIcon);
        pnlDocument.Location = new Point(10, 10);
        pnlDocument.Name = "pnlDocument";
        pnlDocument.Size = new Size(285, 54);
        pnlDocument.TabIndex = 2;
        pnlDocument.Visible = false;
        // btnDocDownload
        btnDocDownload.BackColor = Color.FromArgb(241, 245, 249);
        btnDocDownload.Cursor = Cursors.Hand;
        btnDocDownload.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        btnDocDownload.FlatStyle = FlatStyle.Flat;
        btnDocDownload.Font = new Font("Segoe UI", 8F);
        btnDocDownload.ForeColor = Color.FromArgb(15, 23, 42);
        btnDocDownload.Location = new Point(236, 12);
        btnDocDownload.Name = "btnDocDownload";
        btnDocDownload.Size = new Size(42, 30);
        btnDocDownload.TabIndex = 4;
        btnDocDownload.Text = "Luu";
        btnDocDownload.UseVisualStyleBackColor = false;
        btnDocDownload.Click += btnDocDownload_Click;
        // btnDocOpen
        btnDocOpen.BackColor = Color.FromArgb(37, 99, 235);
        btnDocOpen.Cursor = Cursors.Hand;
        btnDocOpen.FlatAppearance.BorderSize = 0;
        btnDocOpen.FlatStyle = FlatStyle.Flat;
        btnDocOpen.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        btnDocOpen.ForeColor = Color.White;
        btnDocOpen.Location = new Point(190, 12);
        btnDocOpen.Name = "btnDocOpen";
        btnDocOpen.Size = new Size(42, 30);
        btnDocOpen.TabIndex = 3;
        btnDocOpen.Text = "Mo";
        btnDocOpen.UseVisualStyleBackColor = false;
        btnDocOpen.Click += btnDocOpen_Click;
        // lblDocSize
        lblDocSize.Font = new Font("Segoe UI", 7.5F);
        lblDocSize.ForeColor = Color.FromArgb(100, 116, 139);
        lblDocSize.Location = new Point(48, 28);
        lblDocSize.Name = "lblDocSize";
        lblDocSize.Size = new Size(136, 16);
        lblDocSize.TabIndex = 2;
        lblDocSize.Text = "0 KB";
        // lblDocName
        lblDocName.AutoEllipsis = true;
        lblDocName.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblDocName.ForeColor = Color.FromArgb(15, 23, 42);
        lblDocName.Location = new Point(48, 8);
        lblDocName.Name = "lblDocName";
        lblDocName.Size = new Size(136, 18);
        lblDocName.TabIndex = 1;
        lblDocName.Text = "tailieu.docx";
        // lblDocIcon
        lblDocIcon.Font = new Font("Segoe MDL2 Assets", 18F);
        lblDocIcon.Location = new Point(4, 6);
        lblDocIcon.Name = "lblDocIcon";
        lblDocIcon.Size = new Size(40, 40);
        lblDocIcon.TabIndex = 0;
        lblDocIcon.Text = "\uE8A5";
        lblDocIcon.TextAlign = ContentAlignment.MiddleCenter;
        // pnlVoice
        pnlVoice.BackColor = Color.FromArgb(248, 250, 252);
        pnlVoice.Controls.Add(lblVoiceTime);
        pnlVoice.Controls.Add(pnlVoiceProgress);
        pnlVoice.Controls.Add(btnPlayVoice);
        pnlVoice.Location = new Point(10, 10);
        pnlVoice.Name = "pnlVoice";
        pnlVoice.Size = new Size(250, 50);
        pnlVoice.TabIndex = 3;
        pnlVoice.Visible = false;
        // lblVoiceTime
        lblVoiceTime.Font = new Font("Segoe UI", 7.5F);
        lblVoiceTime.ForeColor = Color.FromArgb(100, 116, 139);
        lblVoiceTime.Location = new Point(50, 28);
        lblVoiceTime.Name = "lblVoiceTime";
        lblVoiceTime.Size = new Size(190, 16);
        lblVoiceTime.TabIndex = 2;
        lblVoiceTime.Text = "00:05";
        // pnlVoiceProgress
        pnlVoiceProgress.BackColor = Color.FromArgb(226, 232, 240);
        pnlVoiceProgress.Controls.Add(pnlVoiceBar);
        pnlVoiceProgress.Cursor = Cursors.Hand;
        pnlVoiceProgress.Location = new Point(50, 14);
        pnlVoiceProgress.Name = "pnlVoiceProgress";
        pnlVoiceProgress.Size = new Size(190, 8);
        pnlVoiceProgress.TabIndex = 1;
        pnlVoiceProgress.MouseClick += pnlVoiceProgress_MouseClick;
        // pnlVoiceBar
        pnlVoiceBar.BackColor = Color.FromArgb(37, 99, 235);
        pnlVoiceBar.Dock = DockStyle.Left;
        pnlVoiceBar.Location = new Point(0, 0);
        pnlVoiceBar.Name = "pnlVoiceBar";
        pnlVoiceBar.Size = new Size(0, 8);
        pnlVoiceBar.TabIndex = 0;
        // btnPlayVoice
        btnPlayVoice.BackColor = Color.FromArgb(37, 99, 235);
        btnPlayVoice.Cursor = Cursors.Hand;
        btnPlayVoice.FlatAppearance.BorderSize = 0;
        btnPlayVoice.FlatStyle = FlatStyle.Flat;
        btnPlayVoice.Font = new Font("Segoe UI Symbol", 13F, FontStyle.Bold);
        btnPlayVoice.ForeColor = Color.White;
        btnPlayVoice.Location = new Point(6, 5);
        btnPlayVoice.Name = "btnPlayVoice";
        btnPlayVoice.Size = new Size(40, 40);
        btnPlayVoice.TabIndex = 0;
        btnPlayVoice.Text = "▶";
        btnPlayVoice.AccessibleName = "Phát tin nhắn thoại";
        btnPlayVoice.AccessibleDescription = "Phát tin nhắn thoại.";
        toolTipMessageBubble.SetToolTip(btnPlayVoice, "Phát tin nhắn thoại");
        btnPlayVoice.UseVisualStyleBackColor = false;
        btnPlayVoice.Click += btnPlayVoice_Click;
        // pnlVideoInfo
        pnlVideoInfo.BackColor = Color.FromArgb(245, 243, 255);
        pnlVideoInfo.Controls.Add(btnVideoInfoDownload);
        pnlVideoInfo.Controls.Add(btnVideoInfoOpen);
        pnlVideoInfo.Controls.Add(lblVideoInfoSize);
        pnlVideoInfo.Controls.Add(lblVideoInfoName);
        pnlVideoInfo.Controls.Add(lblVideoInfoIcon);
        pnlVideoInfo.Location = new Point(10, 210);
        pnlVideoInfo.Name = "pnlVideoInfo";
        pnlVideoInfo.Size = new Size(340, 54);
        pnlVideoInfo.TabIndex = 6;
        pnlVideoInfo.Visible = false;
        // lblVideoInfoIcon
        lblVideoInfoIcon.Font = new Font("Segoe MDL2 Assets", 18F);
        lblVideoInfoIcon.ForeColor = Color.FromArgb(124, 58, 237);
        lblVideoInfoIcon.Location = new Point(4, 6);
        lblVideoInfoIcon.Name = "lblVideoInfoIcon";
        lblVideoInfoIcon.Size = new Size(40, 40);
        lblVideoInfoIcon.TabIndex = 0;
        lblVideoInfoIcon.Text = "\uE714";
        lblVideoInfoIcon.TextAlign = ContentAlignment.MiddleCenter;
        // lblVideoInfoName
        lblVideoInfoName.AutoEllipsis = true;
        lblVideoInfoName.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblVideoInfoName.ForeColor = Color.FromArgb(15, 23, 42);
        lblVideoInfoName.Location = new Point(48, 8);
        lblVideoInfoName.Name = "lblVideoInfoName";
        lblVideoInfoName.Size = new Size(176, 18);
        lblVideoInfoName.TabIndex = 1;
        lblVideoInfoName.Text = "video.mp4";
        // lblVideoInfoSize
        lblVideoInfoSize.Font = new Font("Segoe UI", 7.5F);
        lblVideoInfoSize.ForeColor = Color.FromArgb(100, 116, 139);
        lblVideoInfoSize.Location = new Point(48, 28);
        lblVideoInfoSize.Name = "lblVideoInfoSize";
        lblVideoInfoSize.Size = new Size(176, 16);
        lblVideoInfoSize.TabIndex = 2;
        lblVideoInfoSize.Text = "0 MB";
        // btnVideoInfoOpen
        btnVideoInfoOpen.BackColor = Color.FromArgb(124, 58, 237);
        btnVideoInfoOpen.Cursor = Cursors.Hand;
        btnVideoInfoOpen.FlatAppearance.BorderSize = 0;
        btnVideoInfoOpen.FlatStyle = FlatStyle.Flat;
        btnVideoInfoOpen.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        btnVideoInfoOpen.ForeColor = Color.White;
        btnVideoInfoOpen.Location = new Point(232, 12);
        btnVideoInfoOpen.Name = "btnVideoInfoOpen";
        btnVideoInfoOpen.Size = new Size(42, 30);
        btnVideoInfoOpen.TabIndex = 3;
        btnVideoInfoOpen.Text = "Mo";
        btnVideoInfoOpen.UseVisualStyleBackColor = false;
        btnVideoInfoOpen.Click += btnVideoInfoOpen_Click;
        // btnVideoInfoDownload
        btnVideoInfoDownload.BackColor = Color.FromArgb(237, 233, 254);
        btnVideoInfoDownload.Cursor = Cursors.Hand;
        btnVideoInfoDownload.FlatAppearance.BorderColor = Color.FromArgb(167, 139, 250);
        btnVideoInfoDownload.FlatStyle = FlatStyle.Flat;
        btnVideoInfoDownload.Font = new Font("Segoe UI", 8F);
        btnVideoInfoDownload.ForeColor = Color.FromArgb(109, 40, 217);
        btnVideoInfoDownload.Location = new Point(278, 12);
        btnVideoInfoDownload.Name = "btnVideoInfoDownload";
        btnVideoInfoDownload.Size = new Size(50, 30);
        btnVideoInfoDownload.TabIndex = 4;
        btnVideoInfoDownload.Text = "Lưu";
        btnVideoInfoDownload.UseVisualStyleBackColor = false;
        btnVideoInfoDownload.Click += btnVideoInfoDownload_Click;
        // lblMeta
        lblMeta.AutoSize = true;
        lblMeta.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular, GraphicsUnit.Point);
        lblMeta.ForeColor = Color.FromArgb(148, 163, 184);
        lblMeta.Location = new Point(52, 145);
        lblMeta.Name = "lblMeta";
        lblMeta.Size = new Size(67, 12);
        lblMeta.TabIndex = 3;
        lblMeta.Text = "12:35 Da gui";
        // ctxMenu
        ctxMenu.Items.AddRange(new ToolStripItem[] { menuItemCopy, menuItemOpen, menuItemDownload, menuItemEdit, menuItemRevoke, menuItemDelete });
        ctxMenu.Name = "ctxMenu";
        ctxMenu.Size = new Size(240, 144);
        menuItemCopy.Name = "menuItemCopy";
        menuItemCopy.Size = new Size(197, 22);
        menuItemCopy.Text = "Sao chép";
        menuItemCopy.Click += menuItemCopy_Click;
        menuItemOpen.Name = "menuItemOpen";
        menuItemOpen.Size = new Size(197, 22);
        menuItemOpen.Text = "Mở tệp / xem ảnh";
        menuItemOpen.Click += menuItemOpen_Click;
        menuItemDownload.Name = "menuItemDownload";
        menuItemDownload.Size = new Size(197, 22);
        menuItemDownload.Text = "Lưu về máy...";
        menuItemDownload.Click += menuItemDownload_Click;
        menuItemEdit.Name = "menuItemEdit";
        menuItemEdit.Size = new Size(197, 22);
        menuItemEdit.Text = "Chỉnh sửa tin nhắn";
        menuItemEdit.Click += menuItemEdit_Click;
        menuItemRevoke.Name = "menuItemRevoke";
        menuItemRevoke.Size = new Size(197, 22);
        menuItemRevoke.Text = "Thu hồi cho mọi người";
        menuItemRevoke.Click += menuItemRevoke_Click;
        menuItemDelete.Name = "menuItemDelete";
        menuItemDelete.Size = new Size(197, 22);
        menuItemDelete.Text = "Xóa với tôi";
        menuItemDelete.Click += menuItemDelete_Click;
        // MessageBubbleControl
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Transparent;
        ContextMenuStrip = ctxMenu;
        Controls.Add(lblMeta);
        Controls.Add(pnlBubble);
        Controls.Add(lblSenderName);
        Controls.Add(picSenderAvatar);
        Margin = new Padding(0, 2, 0, 4);
        Name = "MessageBubbleControl";
        Size = new Size(420, 165);
        ((System.ComponentModel.ISupportInitialize)picSenderAvatar).EndInit();
        pnlBubble.ResumeLayout(false);
        pnlBubble.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picImage).EndInit();
        pnlDocument.ResumeLayout(false);
        pnlVoice.ResumeLayout(false);
        pnlVoiceProgress.ResumeLayout(false);
        pnlVideoInfo.ResumeLayout(false);
        ctxMenu.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private PictureBox picSenderAvatar;
    private Label lblSenderName;
    private Panel pnlBubble;
    private Label lblContent;
    private PictureBox picImage;
    private Panel pnlDocument;
    private Label lblDocIcon;
    private Label lblDocName;
    private Label lblDocSize;
    private Button btnDocOpen;
    private Button btnDocDownload;
    private Panel pnlVoice;
    private Button btnPlayVoice;
    private Panel pnlVoiceProgress;
    private Panel pnlVoiceBar;
    private Label lblVoiceTime;
    private Microsoft.Web.WebView2.WinForms.WebView2 wv2Video = null!;
    private Panel pnlVideoInfo;
    private Label lblVideoInfoIcon;
    private Label lblVideoInfoName;
    private Label lblVideoInfoSize;
    private Button btnVideoInfoOpen;
    private Button btnVideoInfoDownload;
    private Label lblMeta;
    private ContextMenuStrip ctxMenu;
    private ToolTip toolTipMessageBubble;
    private ToolStripMenuItem menuItemCopy;
    private ToolStripMenuItem menuItemOpen;
    private ToolStripMenuItem menuItemDownload;
    private ToolStripMenuItem menuItemEdit;
    private ToolStripMenuItem menuItemRevoke;
    private ToolStripMenuItem menuItemDelete;
}
