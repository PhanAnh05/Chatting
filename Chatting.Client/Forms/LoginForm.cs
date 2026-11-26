using Chatting.Client.Services;
using Chatting.Shared.DTOs;

namespace Chatting.Client.Forms;

public partial class LoginForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;

    public LoginForm(ApiClient apiClient, SessionManager session)
    {
        _apiClient = apiClient;
        _session = session;
        InitializeComponent();
    }

    private void LoginForm_Load(object? sender, EventArgs e)
    {
        IconHelper.SetButtonIcon(btnServerConfig, IconHelper.Settings, 12, Color.White, ContentAlignment.MiddleLeft);
        UpdateServerDisplay();

        // Load saved credentials
        var (rememberMe, savedEmail, _) = _session.GetSavedCredentials();

        if (rememberMe && !string.IsNullOrEmpty(savedEmail))
        {
            txtEmail.Text = savedEmail;
            chkRememberMe.Checked = true;
            txtPassword.Focus();
        }
        else
        {
            txtEmail.Focus();
        }
    }

    private void UpdateServerDisplay()
    {
        lblServerHint.Text = $"Máy chủ: {_session.ServerUrl}";
    }

    private void btnServerConfig_Click(object? sender, EventArgs e)
    {
        using var configForm = new ServerConfigForm(_session, _apiClient);
        if (configForm.ShowDialog() == DialogResult.OK)
        {
            UpdateServerDisplay();
            lblErrorMessage.Text = string.Empty;
        }
    }

    private async void btnLogin_Click(object? sender, EventArgs e)
    {
        string username = txtEmail.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Vui lòng nhập tài khoản đăng nhập (Username) hoặc Email!");
            txtEmail.Focus();
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowError("Vui lòng nhập mật khẩu!");
            txtPassword.Focus();
            return;
        }

        btnLogin.Enabled = false;
        lblErrorMessage.ForeColor = Color.FromArgb(13, 110, 253);
        lblErrorMessage.Text = "Đang xác thực thông tin đăng nhập...";

        var dto = new LoginRequestDto
        {
            Username = username,
            Password = password,
            RememberMe = chkRememberMe.Checked
        };

        var res = await _apiClient.LoginAsync(dto);

        btnLogin.Enabled = true;

        if (res != null && res.Success && res.Data != null)
        {
            var data = res.Data;
            _session.SetSession(data.AccessToken, data.RefreshToken, data.User);

            if (chkRememberMe.Checked)
            {
                _session.SaveRememberMe(username, data.RefreshToken);
            }
            else
            {
                _session.ClearRememberMe();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        else if (res?.ErrorCode == "UNVERIFIED_EMAIL")
        {
            lblErrorMessage.Text = string.Empty;
            var ask = MessageBox.Show(
                "Tài khoản của bạn chưa được xác minh mã OTP Email!\r\nBạn có muốn nhập mã OTP để kích hoạt ngay bây giờ không?",
                "Tài Khoản Chưa Xác Minh",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ask == DialogResult.Yes)
            {
                using var otpForm = new VerifyOtpForm(username, _apiClient, _session);
                if (otpForm.ShowDialog() == DialogResult.OK)
                {
                    lblErrorMessage.ForeColor = Color.FromArgb(22, 163, 74);
                    lblErrorMessage.Text = "Kích hoạt thành công! Hãy nhập lại mật khẩu để đăng nhập.";
                }
            }
        }
        else
        {
            ShowError(res?.Message ?? "Đăng nhập thất bại. Kiểm tra lại tài khoản hoặc mật khẩu!");
        }
    }

    private void btnGoToRegister_Click(object? sender, EventArgs e)
    {
        using var regForm = new RegisterForm(_apiClient, _session);
        Hide();
        var result = regForm.ShowDialog();
        Show();

        if (result == DialogResult.OK)
        {
            if (!string.IsNullOrEmpty(regForm.RegisteredUsername))
            {
                txtEmail.Text = regForm.RegisteredUsername;
                txtPassword.Clear();
                txtPassword.Focus();
            }
            lblErrorMessage.ForeColor = Color.FromArgb(22, 163, 74);
            lblErrorMessage.Text = "Đăng ký thành công! Hãy nhập mật khẩu để đăng nhập.";
        }
    }

    private void btnForgotPassword_Click(object? sender, EventArgs e)
    {
        using var resetForm = new ResetPasswordForm(_apiClient, txtEmail.Text);
        if (resetForm.ShowDialog(this) == DialogResult.OK)
        {
            txtPassword.Clear();
            txtPassword.Focus();
            lblErrorMessage.ForeColor = Color.FromArgb(22, 163, 74);
            lblErrorMessage.Text = "Mật khẩu đã được đặt lại. Hãy đăng nhập.";
        }
    }

    /// <summary>
    /// Set the account textbox programmatically (e.g., after registration).
    /// </summary>
    public void SetLoginAccount(string account)
    {
        txtEmail.Text = account;
    }

    public void SetEmail(string email) => SetLoginAccount(email);

    private void ShowError(string msg)
    {
        lblErrorMessage.ForeColor = Color.FromArgb(220, 38, 38);
        lblErrorMessage.Text = msg;
    }
}
