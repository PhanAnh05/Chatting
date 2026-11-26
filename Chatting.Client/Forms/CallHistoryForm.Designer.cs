namespace Chatting.Client.Forms;

partial class CallHistoryForm
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
        SuspendLayout();
        // 
        // CallHistoryForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(880, 620);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(680, 480);
        Name = "CallHistoryForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Lịch sử cuộc gọi — Chatting";
        ResumeLayout(false);
    }

    #endregion
}
