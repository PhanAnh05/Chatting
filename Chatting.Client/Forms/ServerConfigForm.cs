using Chatting.Client.Services;

namespace Chatting.Client.Forms;

public partial class ServerConfigForm : Form
{
    private readonly SessionManager _session;
    private readonly ApiClient _apiClient;

    public ServerConfigForm(SessionManager session, ApiClient apiClient)
    {
        _session = session;
        _apiClient = apiClient;
        InitializeComponent();
    }

    private void ServerConfigForm_Load(object? sender, EventArgs e)
    {
        txtServerUrl.Text = _session.ServerUrl;
        lblStatus.Text = "Trạng thái: Chưa kiểm tra kết nối";
        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
    }

    private async void btnPing_Click(object? sender, EventArgs e)
    {
        string url = txtServerUrl.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("Vui lòng nhập địa chỉ URL máy chủ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnPing.Enabled = false;
        lblStatus.Text = "Đang kiểm tra kết nối tới Server...";
        lblStatus.ForeColor = Color.FromArgb(13, 110, 253);

        bool reachable = await _apiClient.PingAsync(url);

        btnPing.Enabled = true;
        if (reachable)
        {
            lblStatus.Text = "Kết nối thành công! Server đang hoạt động tốt.";
            lblStatus.ForeColor = Color.FromArgb(22, 163, 74);
        }
        else
        {
            lblStatus.Text = "Không thể kết nối tới Server. Hãy kiểm tra IP hoặc Firewall!";
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
        }
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        string url = txtServerUrl.Text.Trim();
        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("Địa chỉ máy chủ không được để trống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "http://" + url;
        }

        _session.SetServerUrl(url);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
