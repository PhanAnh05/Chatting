namespace Chatting.Client.Controls;

partial class ConversationListItemControl
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
        picAvatar = new PictureBox();
        pnlOnlineBadge = new Panel();
        lblTitle = new Label();
        lblAiBadge = new Label();
        lblTime = new Label();
        lblLastMessage = new Label();
        lblUnreadBadge = new Label();
        ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
        SuspendLayout();
        // 
        // picAvatar
        // 
        picAvatar.BackColor = Color.FromArgb(224, 231, 255);
        picAvatar.Location = new Point(12, 11);
        picAvatar.Name = "picAvatar";
        picAvatar.Size = new Size(48, 48);
        picAvatar.SizeMode = PictureBoxSizeMode.CenterImage;
        picAvatar.TabIndex = 0;
        picAvatar.TabStop = false;
        // 
        // pnlOnlineBadge
        // 
        pnlOnlineBadge.BackColor = Color.FromArgb(34, 197, 94);
        pnlOnlineBadge.Location = new Point(48, 47);
        pnlOnlineBadge.Name = "pnlOnlineBadge";
        pnlOnlineBadge.Size = new Size(13, 13);
        pnlOnlineBadge.TabIndex = 1;
        pnlOnlineBadge.Visible = false;
        // 
        // lblTitle
        // 
        lblTitle.AutoEllipsis = true;
        lblTitle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(68, 12);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(160, 20);
        lblTitle.TabIndex = 2;
        lblTitle.Text = "Tên Người Dùng / Nhóm";
        // 
        // lblAiBadge
        // 
        lblAiBadge.BackColor = Color.FromArgb(109, 40, 217);
        lblAiBadge.Font = new Font("Segoe UI", 6.75F, FontStyle.Bold, GraphicsUnit.Point);
        lblAiBadge.ForeColor = Color.White;
        lblAiBadge.Location = new Point(175, 14);
        lblAiBadge.Name = "lblAiBadge";
        lblAiBadge.Size = new Size(48, 16);
        lblAiBadge.TabIndex = 6;
        lblAiBadge.Text = "AI BOT";
        lblAiBadge.TextAlign = ContentAlignment.MiddleCenter;
        lblAiBadge.Visible = false;
        // 
        // lblTime
        // 
        lblTime.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
        lblTime.ForeColor = Color.FromArgb(148, 163, 184);
        lblTime.Location = new Point(230, 14);
        lblTime.Name = "lblTime";
        lblTime.Size = new Size(62, 16);
        lblTime.TabIndex = 3;
        lblTime.Text = "12:30";
        lblTime.TextAlign = ContentAlignment.MiddleRight;
        // 
        // lblLastMessage
        // 
        lblLastMessage.AutoEllipsis = true;
        lblLastMessage.Font = new Font("Segoe UI", 8.75F, FontStyle.Regular, GraphicsUnit.Point);
        lblLastMessage.ForeColor = Color.FromArgb(100, 116, 139);
        lblLastMessage.Location = new Point(68, 36);
        lblLastMessage.Name = "lblLastMessage";
        lblLastMessage.Size = new Size(185, 20);
        lblLastMessage.TabIndex = 4;
        lblLastMessage.Text = "Nội dung tin nhắn gần nhất...";
        // 
        // lblUnreadBadge
        // 
        lblUnreadBadge.BackColor = Color.FromArgb(220, 38, 38);
        lblUnreadBadge.Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
        lblUnreadBadge.ForeColor = Color.White;
        lblUnreadBadge.Location = new Point(260, 36);
        lblUnreadBadge.Name = "lblUnreadBadge";
        lblUnreadBadge.Size = new Size(26, 18);
        lblUnreadBadge.TabIndex = 5;
        lblUnreadBadge.Text = "3";
        lblUnreadBadge.TextAlign = ContentAlignment.MiddleCenter;
        lblUnreadBadge.Visible = false;
        // 
        // ConversationListItemControl
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        Controls.Add(lblUnreadBadge);
        Controls.Add(lblLastMessage);
        Controls.Add(lblTime);
        Controls.Add(lblAiBadge);
        Controls.Add(lblTitle);
        Controls.Add(pnlOnlineBadge);
        Controls.Add(picAvatar);
        Cursor = Cursors.Hand;
        Margin = new Padding(0, 0, 0, 1);
        Name = "ConversationListItemControl";
        Size = new Size(300, 70);
        ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private PictureBox picAvatar;
    private Panel pnlOnlineBadge;
    private Label lblTitle;
    private Label lblAiBadge;
    private Label lblTime;
    private Label lblLastMessage;
    private Label lblUnreadBadge;
}
