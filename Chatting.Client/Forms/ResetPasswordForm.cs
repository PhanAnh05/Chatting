using Chatting.Client.Services;
using Chatting.Shared.DTOs;

namespace Chatting.Client.Forms;

/// <summary>OTP-based password recovery for users who cannot sign in.</summary>
public class ResetPasswordForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly TextBox _email = new() { PlaceholderText = "Email đã đăng ký", Width = 350 };
    private readonly TextBox _otp = new() { PlaceholderText = "Mã OTP gồm 6 chữ số", Width = 350, MaxLength = 6 };
    private readonly TextBox _password = new() { PlaceholderText = "Mật khẩu mới (ít nhất 6 ký tự)", Width = 350, UseSystemPasswordChar = true };
    private readonly TextBox _confirmPassword = new() { PlaceholderText = "Nhập lại mật khẩu mới", Width = 350, UseSystemPasswordChar = true };
    private readonly Label _status = new() { Width = 350, Height = 42, TextAlign = ContentAlignment.MiddleCenter };
    private readonly Button _sendCode = new() { Text = "Gửi mã OTP", Width = 168, Height = 36 };
    private readonly Button _reset = new() { Text = "Đặt lại mật khẩu", Width = 168, Height = 36, Enabled = false };

    public ResetPasswordForm(ApiClient apiClient, string? email = null)
    {
        _apiClient = apiClient;
        Text = "Quên mật khẩu - Chatting";
        ClientSize = new Size(410, 375);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        Label title = new() { Text = "ĐẶT LẠI MẬT KHẨU", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), AutoSize = true, Location = new Point(28, 22) };
        Label hint = new() { Text = "Chúng tôi sẽ gửi mã OTP có hiệu lực 5 phút đến email của bạn.", Location = new Point(30, 56), Size = new Size(350, 36), ForeColor = Color.FromArgb(71, 85, 105) };
        _email.Location = new Point(30, 100);
        _otp.Location = new Point(30, 140);
        _password.Location = new Point(30, 180);
        _confirmPassword.Location = new Point(30, 220);
        _sendCode.Location = new Point(30, 278);
        _reset.Location = new Point(212, 278);
        _status.Location = new Point(30, 322);
        _sendCode.Click += SendCodeAsync;
        _reset.Click += ResetPasswordAsync;
        Controls.AddRange([title, hint, _email, _otp, _password, _confirmPassword, _sendCode, _reset, _status]);
        _email.Text = email?.Trim() ?? string.Empty;
        AcceptButton = _reset;
    }

    private async void SendCodeAsync(object? sender, EventArgs e)
    {
        string email = _email.Text.Trim();
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _)) { ShowStatus("Vui lòng nhập email hợp lệ.", true); return; }
        _sendCode.Enabled = false;
        ShowStatus("Đang gửi mã OTP...", false);
        var result = await _apiClient.RequestPasswordResetAsync(new ForgotPasswordRequestDto { Email = email });
        _sendCode.Enabled = true;
        if (result?.Success == true)
        {
            _reset.Enabled = true;
            ShowStatus("Đã gửi mã. Kiểm tra email của bạn.", false, true);
            _otp.Focus();
        }
        else ShowStatus(result?.Message ?? "Không thể gửi mã OTP.", true);
    }

    private async void ResetPasswordAsync(object? sender, EventArgs e)
    {
        if (_otp.Text.Trim().Length != 6) { ShowStatus("Vui lòng nhập đủ 6 chữ số OTP.", true); return; }
        if (_password.Text.Length < 6) { ShowStatus("Mật khẩu mới phải có ít nhất 6 ký tự.", true); return; }
        if (_password.Text != _confirmPassword.Text) { ShowStatus("Mật khẩu xác nhận không khớp.", true); return; }
        _reset.Enabled = false;
        ShowStatus("Đang đặt lại mật khẩu...", false);
        var result = await _apiClient.ResetPasswordAsync(new ResetPasswordRequestDto { Email = _email.Text.Trim(), OtpCode = _otp.Text.Trim(), NewPassword = _password.Text });
        if (result?.Success == true)
        {
            MessageBox.Show("Đặt lại mật khẩu thành công. Hãy đăng nhập bằng mật khẩu mới.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
            return;
        }
        _reset.Enabled = true;
        ShowStatus(result?.Message ?? "Không thể đặt lại mật khẩu.", true);
    }

    private void ShowStatus(string text, bool isError, bool isSuccess = false)
    {
        _status.Text = text;
        _status.ForeColor = isError ? Color.FromArgb(220, 38, 38) : isSuccess ? Color.FromArgb(22, 163, 74) : Color.FromArgb(37, 99, 235);
    }
}
