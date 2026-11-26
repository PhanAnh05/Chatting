using Chatting.Client.Forms;
using Chatting.Client.Services;
using Chatting.Shared.DTOs;

namespace Chatting.Client;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 1. Khởi tạo các Service Singleton dùng chung
        SessionManager session = new();
        ApiClient apiClient = new(session);
        SignalRService signalR = new(session);

        // 2. Vòng lặp phiên làm việc (Hỗ trợ Đăng xuất -> Quay lại Đăng nhập)
        while (true)
        {
            // Kiểm tra tính năng Ghi nhớ đăng nhập
            var (rememberMe, savedEmail, savedRefreshToken) = session.GetSavedCredentials();
            bool autoLoginSuccess = false;

            if (rememberMe && !string.IsNullOrEmpty(savedRefreshToken))
            {
                try
                {
                    var refreshRes = apiClient.RefreshAsync(new RefreshTokenRequestDto
                    {
                        RefreshToken = savedRefreshToken
                    }).GetAwaiter().GetResult();

                    if (refreshRes?.Success == true && refreshRes.Data != null)
                    {
                        session.SetSession(
                            refreshRes.Data.AccessToken,
                            refreshRes.Data.RefreshToken,
                            refreshRes.Data.User);

                        // Cập nhật refresh token mới xoay vòng
                        session.SaveRememberMe(savedEmail, refreshRes.Data.RefreshToken);
                        autoLoginSuccess = true;
                    }
                }
                catch
                {
                    // Nếu lỗi mạng hoặc token hết hạn, hiển thị lại LoginForm
                    autoLoginSuccess = false;
                }
            }

            if (!autoLoginSuccess)
            {
                using LoginForm loginForm = new(apiClient, session);
                DialogResult loginResult = loginForm.ShowDialog();

                if (loginResult != DialogResult.OK)
                {
                    // Người dùng bấm thoát hoặc đóng cửa sổ đăng nhập
                    break;
                }
            }

            // 3. Chạy giao diện chính Messenger
            using MainMessengerForm mainForm = new(apiClient, session, signalR);
            Application.Run(mainForm);

            // Khi MainMessengerForm đóng:
            // Nếu DialogResult == DialogResult.Abort -> Người dùng bấm Đăng xuất -> Quay lại vòng lặp đăng nhập
            // Ngược lại -> Người dùng bấm nút X tắt ứng dụng -> Thoát hoàn toàn
            if (mainForm.DialogResult != DialogResult.Abort)
            {
                break;
            }
        }
    }
}