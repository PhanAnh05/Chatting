using Chatting.Shared.DTOs;
using Chatting.Client.Services;

namespace Chatting.Client.Forms;

public sealed class EditProfileForm : Form
{
    private readonly TextBox _displayName = new();
    private readonly TextBox _username = new();
    private readonly TextBox _bio = new();
    private readonly PictureBox _avatarPreview = new();
    private readonly Label _avatarHint = new();
    private readonly ToolTip _toolTip = new();
    private readonly bool _darkMode;

    public UpdateProfileRequestDto? ProfileChanges { get; private set; }
    public string? SelectedAvatarPath { get; private set; }

    public EditProfileForm(UserSessionDto user, string serverUrl, bool darkMode = false)
    {
        _darkMode = darkMode;
        Text = "Chỉnh sửa hồ sơ";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 580);
        BackColor = darkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
        Font = new Font("Segoe UI", 9.5F);

        Panel header = new() { Dock = DockStyle.Top, Height = 76, BackColor = darkMode ? Color.FromArgb(8, 13, 25) : Color.FromArgb(15, 23, 42) };
        Label title = new() { Text = "Hồ sơ của tôi", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(26, 14), AutoSize = true };
        Label subtitle = new() { Text = "Chỉ tên hiển thị và phần giới thiệu có thể chỉnh sửa", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(203, 213, 225), Location = new Point(28, 45), AutoSize = true };
        header.Controls.AddRange([title, subtitle]);
        Controls.Add(header);

        _avatarPreview.Location = new Point(28, 94);
        _avatarPreview.Size = new Size(76, 76);
        _avatarPreview.SizeMode = PictureBoxSizeMode.Zoom;
        _avatarPreview.BackColor = darkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240);
        _avatarPreview.Image = CreateAvatarPlaceholder(user.DisplayName);
        Controls.Add(_avatarPreview);

        Button chooseAvatar = new()
        {
            Text = "  Đổi ảnh đại diện",
            Location = new Point(120, 112),
            Size = new Size(170, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = darkMode ? Color.FromArgb(30, 41, 59) : Color.White,
            ForeColor = darkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(51, 65, 85),
            Cursor = Cursors.Hand
        };
        IconHelper.SetButtonIcon(chooseAvatar, IconHelper.Camera, 14, darkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(51, 65, 85), ContentAlignment.MiddleLeft);
        chooseAvatar.FlatAppearance.BorderColor = darkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(203, 213, 225);
        chooseAvatar.Click += (_, _) => ChooseAvatar();
        Controls.Add(chooseAvatar);
        _avatarHint = new()
        {
            Text = "JPG, PNG, WEBP hoặc GIF · tối đa 10 MB",
            Location = new Point(120, 153),
            Size = new Size(310, 24),
            AutoSize = false,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = darkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139)
        };
        Controls.Add(_avatarHint);
        Shown += async (_, _) => await LoadCurrentAvatarAsync(serverUrl, user.AvatarUrl);
        _toolTip.ShowAlways = true;
        _toolTip.SetToolTip(_avatarHint, _avatarHint.Text);
        Disposed += (_, _) =>
        {
            _avatarPreview.Image?.Dispose();
            _toolTip.Dispose();
        };

        AddField("TÊN HIỂN THỊ", _displayName, 188, user.DisplayName);
        _displayName.MaxLength = 100;
        AddField("TÊN TÀI KHOẢN · KHÔNG THỂ THAY ĐỔI", _username, 268, user.Username);
        _username.ReadOnly = true;
        _username.TabStop = false;
        _username.Cursor = Cursors.No;
        _username.BackColor = darkMode ? Color.FromArgb(23, 32, 51) : Color.FromArgb(241, 245, 249);
        _username.ForeColor = darkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
        Label bioLabel = new() { Text = "GIỚI THIỆU", Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = darkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139), Location = new Point(28, 348), AutoSize = true };
        _bio.Location = new Point(28, 370);
        _bio.Size = new Size(424, 82);
        _bio.Multiline = true;
        _bio.MaxLength = 500;
        _bio.ScrollBars = ScrollBars.Vertical;
        _bio.Text = user.Bio ?? string.Empty;
        _bio.BorderStyle = BorderStyle.FixedSingle;
        _bio.BackColor = darkMode ? Color.FromArgb(30, 41, 59) : Color.White;
        _bio.ForeColor = darkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);
        Controls.AddRange([bioLabel, _bio]);

        Label note = new() { Text = "Giới thiệu tối đa 500 ký tự.", Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(28, 458), AutoSize = true };
        Button cancel = new() { Text = "Hủy", DialogResult = DialogResult.Cancel, Size = new Size(96, 40), Location = new Point(244, 514), FlatStyle = FlatStyle.Flat, BackColor = darkMode ? Color.FromArgb(30, 41, 59) : Color.White, ForeColor = darkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(51, 65, 85) };
        Button save = new() { Text = "Lưu hồ sơ", Size = new Size(120, 40), Location = new Point(352, 514), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White };
        save.FlatAppearance.BorderSize = 0;
        cancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        save.Click += (_, _) =>
        {
            ProfileChanges = new UpdateProfileRequestDto
            {
                DisplayName = _displayName.Text.Trim(),
                Bio = _bio.Text.Trim()
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.AddRange([note, cancel, save]);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private void AddField(string labelText, TextBox input, int top, string? value)
    {
        Label label = new() { Text = labelText, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = _darkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139), Location = new Point(28, top), AutoSize = true };
        input.Location = new Point(28, top + 22);
        input.Size = new Size(424, 30);
        input.Text = value ?? string.Empty;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.BackColor = _darkMode ? Color.FromArgb(30, 41, 59) : Color.White;
        input.ForeColor = _darkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(15, 23, 42);
        Controls.AddRange([label, input]);
    }

    private void ChooseAvatar()
    {
        using OpenFileDialog dialog = new()
        {
            Title = "Chọn ảnh đại diện",
            Filter = "Ảnh đại diện|*.jpg;*.jpeg;*.png;*.webp;*.gif",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            if (new FileInfo(dialog.FileName).Length > 10 * 1024 * 1024)
            {
                MessageBox.Show(this, "Ảnh đại diện không được vượt quá 10 MB.", "Ảnh đại diện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using Image source = Image.FromFile(dialog.FileName);
            SetAvatarPreview(AvatarImageCache.CreateCircularFit(source, _avatarPreview.Size,
                _darkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240),
                _darkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(203, 213, 225)));
            SelectedAvatarPath = dialog.FileName;
            _avatarHint.Text = Path.GetFileName(dialog.FileName);
            _toolTip.SetToolTip(_avatarHint, _avatarHint.Text);
        }
        catch (Exception)
        {
            MessageBox.Show(this, "Tệp này không đọc được như một ảnh hợp lệ.", "Ảnh đại diện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task LoadCurrentAvatarAsync(string serverUrl, string? avatarUrl)
    {
        Image? image = await AvatarImageCache.GetAsync(serverUrl, avatarUrl);
        if (image is null || IsDisposed || !string.IsNullOrWhiteSpace(SelectedAvatarPath)) return;
        SetAvatarPreview(AvatarImageCache.CreateCircularFit(image, _avatarPreview.Size,
            _darkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(226, 232, 240),
            _darkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(203, 213, 225)));
    }

    private void SetAvatarPreview(Image image)
    {
        Image? old = _avatarPreview.Image;
        _avatarPreview.Image = image;
        old?.Dispose();
    }

    private Bitmap CreateAvatarPlaceholder(string? displayName)
    {
        Bitmap image = new(_avatarPreview.Width, _avatarPreview.Height);
        using Graphics graphics = Graphics.FromImage(image);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using SolidBrush background = new(Color.FromArgb(37, 99, 235));
        graphics.FillEllipse(background, 0, 0, image.Width - 1, image.Height - 1);
        string initial = string.IsNullOrWhiteSpace(displayName) ? "?" : displayName.Trim()[0].ToString().ToUpperInvariant();
        using Font font = new("Segoe UI", 27F, FontStyle.Bold);
        TextRenderer.DrawText(graphics, initial, font, new Rectangle(0, 0, image.Width, image.Height), Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        return image;
    }
}
