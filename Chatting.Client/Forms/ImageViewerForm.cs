using Chatting.Client.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chatting.Client.Forms;

public partial class ImageViewerForm : Form
{
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    private readonly string _filePath;
    private readonly string _fileName;

    public ImageViewerForm(string filePath, string fileName)
    {
        InitializeComponent();
        _filePath = filePath;
        _fileName = fileName;

        lblTitle.Text = $"Xem ảnh: {fileName}";
        lblTitle.AccessibleDescription = lblTitle.Text;
        toolTipImageViewer.SetToolTip(lblTitle, lblTitle.Text);

        IconHelper.SetButtonIcon(btnOpenExternal, IconHelper.OpenFolder, 13, Color.White, ContentAlignment.MiddleLeft);
        IconHelper.SetButtonIcon(btnSave, IconHelper.Download, 13, Color.White, ContentAlignment.MiddleLeft);

        // Drag form with title bar
        pnlTitle.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        };

        pnlTitle.DoubleClick += (s, e) => btnMaxTop_Click(s, e);

        LoadImage();
    }

    private void LoadImage()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                using var stream = new MemoryStream(File.ReadAllBytes(_filePath));
                var img = Image.FromStream(stream);
                picImage.Image = new Bitmap(img);
                FileInfo fi = new(_filePath);
                lblInfo.Text = $"Kích thước: {img.Width} × {img.Height} px  •  Dung lượng: {fi.Length / 1024.0:F1} KB";
            }
            else
            {
                lblInfo.Text = "Tệp ảnh không tồn tại hoặc đã bị xóa.";
            }
        }
        catch (Exception ex)
        {
            lblInfo.Text = $"Lỗi tải ảnh: {ex.Message}";
        }
    }

    private void btnCloseTop_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void btnMaxTop_Click(object? sender, EventArgs e)
    {
        WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        btnMaxTop.Text = WindowState == FormWindowState.Maximized ? IconHelper.Restore : IconHelper.Maximize;
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        if (!File.Exists(_filePath))
        {
            MessageBox.Show("Tệp ảnh không tồn tại để lưu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using SaveFileDialog sfd = new()
        {
            FileName = _fileName,
            Filter = "Tệp hình ảnh (*.png;*.jpg;*.jpeg;*.webp;*.gif)|*.png;*.jpg;*.jpeg;*.webp;*.gif|Tất cả tệp (*.*)|*.*",
            Title = "Lưu hình ảnh về máy tính"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                File.Copy(_filePath, sfd.FileName, overwrite: true);
                MessageBox.Show("Đã lưu ảnh thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void btnOpenExternal_Click(object? sender, EventArgs e)
    {
        if (File.Exists(_filePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo(_filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở bằng ứng dụng mặc định: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ImageViewerForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }
}
