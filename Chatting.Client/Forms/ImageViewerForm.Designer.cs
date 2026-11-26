namespace Chatting.Client.Forms;

partial class ImageViewerForm
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
        pnlTitle = new Panel();
        toolTipImageViewer = new ToolTip(components);
        lblTitle = new Label();
        btnCloseTop = new Button();
        btnMaxTop = new Button();
        pnlBottomBar = new Panel();
        lblInfo = new Label();
        btnOpenExternal = new Button();
        btnSave = new Button();
        picImage = new PictureBox();
        pnlTitle.SuspendLayout();
        pnlBottomBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picImage).BeginInit();
        SuspendLayout();
        // 
        // pnlTitle
        // 
        pnlTitle.BackColor = Color.FromArgb(15, 23, 42);
        pnlTitle.Controls.Add(lblTitle);
        pnlTitle.Controls.Add(btnMaxTop);
        pnlTitle.Controls.Add(btnCloseTop);
        pnlTitle.Dock = DockStyle.Top;
        pnlTitle.Location = new Point(0, 0);
        pnlTitle.Name = "pnlTitle";
        pnlTitle.Size = new Size(800, 40);
        pnlTitle.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblTitle.AutoEllipsis = true;
        lblTitle.AutoSize = false;
        lblTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(14, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(660, 40);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Xem ảnh đính kèm";
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnMaxTop
        // 
        btnMaxTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMaxTop.FlatAppearance.BorderSize = 0;
        btnMaxTop.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnMaxTop.FlatStyle = FlatStyle.Flat;
        btnMaxTop.Cursor = Cursors.Hand;
        btnMaxTop.Font = new Font("Segoe MDL2 Assets", 10F);
        btnMaxTop.ForeColor = Color.FromArgb(148, 163, 184);
        btnMaxTop.Location = new Point(716, 0);
        btnMaxTop.Name = "btnMaxTop";
        btnMaxTop.Size = new Size(42, 40);
        btnMaxTop.TabIndex = 2;
        btnMaxTop.Text = "\uE922";
        btnMaxTop.AccessibleName = "Phóng to hoặc khôi phục cửa sổ";
        btnMaxTop.AccessibleDescription = "Thay đổi kích thước cửa sổ xem ảnh.";
        toolTipImageViewer.SetToolTip(btnMaxTop, "Phóng to / Khôi phục");
        btnMaxTop.UseVisualStyleBackColor = true;
        btnMaxTop.Click += btnMaxTop_Click;
        // 
        // btnCloseTop
        // 
        btnCloseTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCloseTop.FlatAppearance.BorderSize = 0;
        btnCloseTop.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 68, 68);
        btnCloseTop.FlatStyle = FlatStyle.Flat;
        btnCloseTop.Cursor = Cursors.Hand;
        btnCloseTop.Font = new Font("Segoe MDL2 Assets", 10F);
        btnCloseTop.ForeColor = Color.FromArgb(148, 163, 184);
        btnCloseTop.Location = new Point(758, 0);
        btnCloseTop.Name = "btnCloseTop";
        btnCloseTop.Size = new Size(42, 40);
        btnCloseTop.TabIndex = 1;
        btnCloseTop.Text = "\uE711";
        btnCloseTop.AccessibleName = "Đóng trình xem ảnh";
        btnCloseTop.AccessibleDescription = "Đóng cửa sổ xem ảnh.";
        toolTipImageViewer.SetToolTip(btnCloseTop, "Đóng");
        btnCloseTop.UseVisualStyleBackColor = true;
        btnCloseTop.Click += btnCloseTop_Click;
        // 
        // pnlBottomBar
        // 
        pnlBottomBar.BackColor = Color.FromArgb(15, 23, 42);
        pnlBottomBar.Controls.Add(lblInfo);
        pnlBottomBar.Controls.Add(btnOpenExternal);
        pnlBottomBar.Controls.Add(btnSave);
        pnlBottomBar.Dock = DockStyle.Bottom;
        pnlBottomBar.Location = new Point(0, 550);
        pnlBottomBar.Name = "pnlBottomBar";
        pnlBottomBar.Padding = new Padding(12, 6, 12, 6);
        pnlBottomBar.Size = new Size(800, 50);
        pnlBottomBar.TabIndex = 1;
        // 
        // lblInfo
        // 
        lblInfo.AutoSize = true;
        lblInfo.Font = new Font("Segoe UI", 8.5F);
        lblInfo.ForeColor = Color.FromArgb(148, 163, 184);
        lblInfo.Location = new Point(14, 18);
        lblInfo.Name = "lblInfo";
        lblInfo.Size = new Size(110, 15);
        lblInfo.TabIndex = 2;
        lblInfo.Text = "Kích thước: 0 x 0 px";
        // 
        // btnOpenExternal
        // 
        btnOpenExternal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpenExternal.BackColor = Color.FromArgb(30, 41, 59);
        btnOpenExternal.Cursor = Cursors.Hand;
        btnOpenExternal.FlatAppearance.BorderSize = 0;
        btnOpenExternal.FlatStyle = FlatStyle.Flat;
        btnOpenExternal.Font = new Font("Segoe UI", 8.5F);
        btnOpenExternal.ForeColor = Color.White;
        btnOpenExternal.Location = new Point(540, 10);
        btnOpenExternal.Name = "btnOpenExternal";
        btnOpenExternal.Size = new Size(115, 30);
        btnOpenExternal.TabIndex = 1;
        btnOpenExternal.Text = "Mở bên ngoài";
        btnOpenExternal.UseVisualStyleBackColor = false;
        btnOpenExternal.Click += btnOpenExternal_Click;
        // 
        // btnSave
        // 
        btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSave.BackColor = Color.FromArgb(37, 99, 235);
        btnSave.Cursor = Cursors.Hand;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(665, 10);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(120, 30);
        btnSave.TabIndex = 0;
        btnSave.Text = "Lưu về máy";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += btnSave_Click;
        // 
        // picImage
        // 
        picImage.BackColor = Color.FromArgb(10, 15, 29);
        picImage.Dock = DockStyle.Fill;
        picImage.Location = new Point(0, 40);
        picImage.Name = "picImage";
        picImage.Size = new Size(800, 510);
        picImage.SizeMode = PictureBoxSizeMode.Zoom;
        picImage.TabIndex = 2;
        picImage.TabStop = false;
        // 
        // ImageViewerForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(10, 15, 29);
        ClientSize = new Size(800, 600);
        Controls.Add(picImage);
        Controls.Add(pnlBottomBar);
        Controls.Add(pnlTitle);
        FormBorderStyle = FormBorderStyle.None;
        KeyPreview = true;
        MinimumSize = new Size(400, 300);
        Name = "ImageViewerForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Xem ảnh";
        KeyDown += ImageViewerForm_KeyDown;
        pnlTitle.ResumeLayout(false);
        pnlTitle.PerformLayout();
        pnlBottomBar.ResumeLayout(false);
        pnlBottomBar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picImage).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlTitle;
    private Label lblTitle;
    private ToolTip toolTipImageViewer;
    private Button btnCloseTop;
    private Button btnMaxTop;
    private Panel pnlBottomBar;
    private Label lblInfo;
    private Button btnOpenExternal;
    private Button btnSave;
    private PictureBox picImage;
}
