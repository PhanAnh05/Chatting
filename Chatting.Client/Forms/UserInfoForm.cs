using Chatting.Shared.DTOs;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Chatting.Client.Forms;

public partial class UserInfoForm : Form
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    public UserInfoForm(string username, string? displayName, string? email, bool isOnline, string? bio = null, bool darkMode = false)
    {
        InitializeComponent();
        ClientSize = new Size(380, 530);
        lblDisplayName.AutoEllipsis = true;
        if (darkMode)
        {
            pnlBody.BackColor = Color.FromArgb(15, 23, 42);
            lblDisplayName.ForeColor = Color.FromArgb(226, 232, 240);
            lblUsername.ForeColor = Color.FromArgb(148, 163, 184);
            lblEmail.ForeColor = Color.FromArgb(203, 213, 225);
            btnClose.BackColor = Color.FromArgb(30, 41, 59);
            btnClose.ForeColor = Color.FromArgb(226, 232, 240);
        }
        btnClose.Location = new Point(25, 410);
        btnClose.Size = new Size(330, 42);

        // Window drag handler
        pnlHeader.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        };

        // Rounded form border
        Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 16, 16));

        // Rounded button
        btnClose.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnClose.Width, btnClose.Height, 10, 10));

        // Populate fields
        lblDisplayName.Text = string.IsNullOrWhiteSpace(displayName) ? username : displayName;
        lblUsername.Text = $"@{username}";
        lblEmail.Text = string.IsNullOrWhiteSpace(email) ? "Không có thông tin email" : email;
        toolTipUserInfo.ShowAlways = true;
        toolTipUserInfo.SetToolTip(lblDisplayName, lblDisplayName.Text);
        toolTipUserInfo.SetToolTip(lblUsername, lblUsername.Text);
        toolTipUserInfo.SetToolTip(lblEmail, lblEmail.Text);

        Label lblBioTitle = new()
        {
            Text = "GIỚI THIỆU",
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(25, 294),
            AutoSize = true
        };
        TextBox txtBio = new()
        {
            Text = string.IsNullOrWhiteSpace(bio) ? "Chưa có phần giới thiệu." : bio,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = darkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59),
            Location = new Point(25, 314),
            Size = new Size(330, 82),
            BackColor = darkMode ? Color.FromArgb(15, 23, 42) : Color.White,
            BorderStyle = BorderStyle.None,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            TabStop = true,
            AccessibleName = "Phần giới thiệu",
            AccessibleDescription = "Nội dung giới thiệu của người dùng. Có thể cuộn để đọc toàn bộ."
        };
        pnlBody.Controls.Add(lblBioTitle);
        pnlBody.Controls.Add(txtBio);

        if (isOnline)
        {
            lblStatus.Text = "● Đang trực tuyến (Online)";
            lblStatus.ForeColor = Color.FromArgb(22, 163, 74);
        }
        else
        {
            lblStatus.Text = "○ Ngoại tuyến (Offline)";
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        }

        // Draw circular avatar
        picAvatar.Image = GenerateAvatarBitmap(lblDisplayName.Text);
        picAvatar.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picAvatar.Width, picAvatar.Height, picAvatar.Width, picAvatar.Height));
    }

    private static Bitmap GenerateAvatarBitmap(string name)
    {
        Bitmap bmp = new(80, 80);
        using Graphics g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using Brush brush = new SolidBrush(Color.FromArgb(37, 99, 235));
        g.FillEllipse(brush, 0, 0, 79, 79);

        string initial = string.IsNullOrWhiteSpace(name) ? "?" : name[0].ToString().ToUpper();
        using Font font = new("Segoe UI", 28, FontStyle.Bold, GraphicsUnit.Pixel);
        using Brush textBrush = new SolidBrush(Color.White);

        StringFormat sf = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString(initial, font, textBrush, new RectangleF(0, 0, 80, 80), sf);
        return bmp;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
