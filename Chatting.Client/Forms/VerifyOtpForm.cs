using Chatting.Client.Services;
using Chatting.Shared.DTOs;

namespace Chatting.Client.Forms;

public partial class VerifyOtpForm : Form
{
    private readonly string _email;
    private readonly ApiClient _apiClient;
    private readonly SessionManager _session;
    private int _secondsRemaining = 60;

    public VerifyOtpForm(string email, ApiClient apiClient, SessionManager session)
    {
        _email = email;
        _apiClient = apiClient;
        _session = session;
        InitializeComponent();
    }

    private void VerifyOtpForm_Load(object? sender, EventArgs e)
    {
        lblEmailNotice.Text = $"Mã OTP đã gửi đến email:\r\n{_email}";
        lblStatusMessage.Text = string.Empty;
        txtOtpCode.Focus();

        StartResendCountdown(60);
    }

    private void StartResendCountdown(int seconds)
    {
        _secondsRemaining = seconds;
        btnResendOtp.Enabled = false;
        lblCountdown.Text = $"Gửi lại sau: {_secondsRemaining}s";
        timerCountdown.Start();
    }

    private void timerCountdown_Tick(object? sender, EventArgs e)
    {
        _secondsRemaining--;
        if (_secondsRemaining > 0)
        {
            lblCountdown.Text = $"Gửi lại sau: {_secondsRemaining}s";
        }
        else
        {
            timerCountdown.Stop();
            btnResendOtp.Enabled = true;
            lblCountdown.Text = "Có thể gửi lại mã";
        }
    }

    private async void btnResendOtp_Click(object? sender, EventArgs e)
    {
        btnResendOtp.Enabled = false;
        lblStatusMessage.ForeColor = Color.FromArgb(13, 110, 253);
        lblStatusMessage.Text = "Đang gửi lại mã OTP mới...";

        var res = await _apiClient.ResendOtpAsync(new ResendOtpRequestDto { Email = _email });

        if (res != null && res.Success)
        {
            lblStatusMessage.ForeColor = Color.FromArgb(22, 163, 74);
            lblStatusMessage.Text = "Đã gửi mã OTP mới! Vui lòng kiểm tra email hoặc console server.";
            StartResendCountdown(60);
        }
        else
        {
            lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatusMessage.Text = res?.Message ?? "Không thể gửi lại mã OTP lúc này.";
            btnResendOtp.Enabled = true;
        }
    }

    private async void btnVerify_Click(object? sender, EventArgs e)
    {
        string otp = txtOtpCode.Text.Trim();
        if (otp.Length != 6)
        {
            lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatusMessage.Text = "Vui lòng nhập đủ 6 chữ số mã OTP!";
            txtOtpCode.Focus();
            return;
        }

        btnVerify.Enabled = false;
        lblStatusMessage.ForeColor = Color.FromArgb(13, 110, 253);
        lblStatusMessage.Text = "Đang kiểm tra mã OTP...";

        var res = await _apiClient.VerifyOtpAsync(new VerifyOtpRequestDto
        {
            Email = _email,
            OtpCode = otp
        });

        btnVerify.Enabled = true;

        if (res != null && res.Success)
        {
            MessageBox.Show(
                "Xác minh tài khoản thành công!\r\nBạn có thể đăng nhập vào ứng dụng ngay bây giờ.",
                "Thành Công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            lblStatusMessage.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatusMessage.Text = res?.Message ?? "Mã OTP không chính xác hoặc đã hết hạn!";
            txtOtpCode.SelectAll();
            txtOtpCode.Focus();
        }
    }

    private void btnBackToLogin_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
