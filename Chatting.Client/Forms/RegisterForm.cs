using Chatting.Client.Services;
using Chatting.Shared.DTOs;
using System.Text.RegularExpressions;

namespace Chatting.Client.Forms;

public partial class RegisterForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;

    /// <summary>
    /// Stores the registered username upon successful registration and activation,
    /// so the LoginForm can automatically pre-fill the username for the user.
    /// </summary>
    public string? RegisteredUsername { get; private set; }

    public RegisterForm(ApiClient apiClient, SessionManager session)
    {
        _apiClient = apiClient;
        _session = session;
        InitializeComponent();
    }

    private async void btnRegister_Click(object? sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string displayName = txtDisplayName.Text.Trim();
        string email = txtEmail.Text.Trim();
        string password = txtPassword.Text;
        string confirmPassword = txtConfirmPassword.Text;

        // Validation - Username
        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Vui lòng nhập tài khoản đăng nhập (Username)!");
            txtUsername.Focus();
            return;
        }

        if (username.Length < 3 || username.Length > 50)
        {
            ShowError("Tài khoản đăng nhập phải chứa từ 3 đến 50 ký tự!");
            txtUsername.Focus();
            return;
        }

        if (username.Contains(' '))
        {
            ShowError("Tài khoản đăng nhập không được chứa khoảng trắng (dấu cách)!");
            txtUsername.Focus();
            return;
        }

        // Default display name to username if empty
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = username;
        }

        // Validation - Email
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowError("Vui lòng nhập địa chỉ Email hợp lệ để nhận mã OTP!");
            txtEmail.Focus();
            return;
        }

        // Validation - Password
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            ShowError("Mật khẩu phải chứa ít nhất 6 ký tự!");
            txtPassword.Focus();
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Mật khẩu xác nhận không khớp!");
            txtConfirmPassword.Focus();
            return;
        }

        btnRegister.Enabled = false;
        lblErrorMessage.ForeColor = Color.FromArgb(13, 110, 253);
        lblErrorMessage.Text = "Đang kiểm tra tài khoản và gửi yêu cầu đăng ký...";

        // Check username availability
        var checkRes = await _apiClient.CheckUsernameAsync(username);
        if (checkRes != null && !checkRes.Success && checkRes.Data == false)
        {
            btnRegister.Enabled = true;
            ShowError(checkRes.Message ?? "Tài khoản đăng nhập này đã được sử dụng. Vui lòng chọn tài khoản khác!");
            txtUsername.Focus();
            return;
        }

        var dto = new RegisterRequestDto
        {
            Username = username,
            DisplayName = displayName,
            Email = email,
            Password = password
        };

        var res = await _apiClient.RegisterAsync(dto);

        btnRegister.Enabled = true;

        if (res != null && res.Success)
        {
            MessageBox.Show(
                "Đăng ký tài khoản thành công!\r\nMã xác minh OTP đã được gửi đến email (hoặc console máy chủ).\r\nVui lòng nhập OTP để kích hoạt tài khoản.",
                "Đăng Ký Thành Công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Chuyển sang form xác minh OTP
            using var otpForm = new VerifyOtpForm(email, _apiClient, _session);
            Hide();
            var otpResult = otpForm.ShowDialog();
            if (otpResult == DialogResult.OK)
            {
                RegisteredUsername = username;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Show();
                lblErrorMessage.ForeColor = Color.FromArgb(220, 38, 38);
                lblErrorMessage.Text = "Tài khoản chưa được kích hoạt OTP.";
            }
        }
        else
        {
            ShowError(res?.Message ?? "Đăng ký thất bại. Vui lòng thử lại!");
        }
    }

    private void btnBackToLogin_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void ShowError(string msg)
    {
        lblErrorMessage.ForeColor = Color.FromArgb(220, 38, 38);
        lblErrorMessage.Text = msg;
    }
}
