using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace Chatting.Server.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otpCode, string purpose = "Xác minh email")
    {
        string from = _config["Email:From"] ?? "noreply@chatting.local";
        string displayName = _config["Email:DisplayName"] ?? "Chatting App";
        string host = _config["Email:SmtpHost"] ?? "localhost";
        int port = int.Parse(_config["Email:SmtpPort"] ?? "587");
        bool enableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true");
        string? user = _config["Email:Username"];
        string? pass = _config["Email:Password"];

        string body = $"""
            <html>
            <body style="font-family:Arial,sans-serif;background:#f4f4f4;padding:30px;">
              <div style="max-width:480px;margin:auto;background:#fff;border-radius:8px;padding:32px;box-shadow:0 2px 8px rgba(0,0,0,.1)">
                <h2 style="color:#5865F2;margin-top:0">{purpose} – Chatting</h2>
                <p>Xin chào <strong>{toName}</strong>,</p>
                <p>Mã OTP của bạn là:</p>
                <div style="font-size:36px;font-weight:bold;letter-spacing:12px;text-align:center;
                            background:#f0f0ff;border-radius:6px;padding:16px;margin:20px 0;color:#5865F2">
                  {otpCode}
                </div>
                <p>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này với bất kỳ ai.</p>
                <hr style="border:none;border-top:1px solid #eee"/>
                <p style="color:#888;font-size:12px">Chatting – Ứng dụng nhắn tin nội bộ LAN</p>
              </div>
            </body>
            </html>
            """;

        try
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                throw new InvalidOperationException("Chưa cấu hình Email:Username hoặc Email:Password.");

            await SendWithStartTlsOverIpv4Async(host, port, enableSsl, user, pass, from, toEmail,
                $"[Chatting] {purpose}: {otpCode}", body);
            _logger.LogInformation("[EmailService] OTP email sent to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EmailService] Failed to send OTP email to {Email}", toEmail);
            return false;
        }
    }

    private static async Task SendWithStartTlsOverIpv4Async(string host, int port, bool enableSsl,
        string username, string password, string from, string recipient, string subject, string htmlBody)
    {
        if (!enableSsl || port != 587)
            throw new InvalidOperationException("Email hiện chỉ hỗ trợ SMTP STARTTLS qua cổng 587.");

        IPAddress address = (await Dns.GetHostAddressesAsync(host))
            .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
            ?? throw new SocketException((int)SocketError.HostNotFound);

        using TcpClient client = new(AddressFamily.InterNetwork);
        await client.ConnectAsync(address, port);
        using NetworkStream network = client.GetStream();
        using StreamReader reader = new(network, Encoding.ASCII, false, 4096, leaveOpen: true);
        await using StreamWriter writer = new(network, new UTF8Encoding(false), 4096, leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };

        await ExpectAsync(reader, 220);
        await CommandAsync(writer, reader, "EHLO chatting.local", 250);
        await CommandAsync(writer, reader, "STARTTLS", 220);

        await using SslStream tls = new(network, leaveInnerStreamOpen: false);
        await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = host,
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        });

        using StreamReader tlsReader = new(tls, Encoding.ASCII, false, 4096, leaveOpen: true);
        await using StreamWriter tlsWriter = new(tls, new UTF8Encoding(false), 4096, leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };
        await CommandAsync(tlsWriter, tlsReader, "EHLO chatting.local", 250);
        await CommandAsync(tlsWriter, tlsReader, "AUTH LOGIN", 334);
        await CommandAsync(tlsWriter, tlsReader, Convert.ToBase64String(Encoding.UTF8.GetBytes(username)), 334);
        await CommandAsync(tlsWriter, tlsReader, Convert.ToBase64String(Encoding.UTF8.GetBytes(password)), 235);
        await CommandAsync(tlsWriter, tlsReader, $"MAIL FROM:<{from}>", 250);
        await CommandAsync(tlsWriter, tlsReader, $"RCPT TO:<{recipient}>", 250);
        await CommandAsync(tlsWriter, tlsReader, "DATA", 354);
        await tlsWriter.WriteLineAsync($"From: Chatting <{from}>");
        await tlsWriter.WriteLineAsync($"To: <{recipient}>");
        await tlsWriter.WriteLineAsync($"Subject: {subject}");
        await tlsWriter.WriteLineAsync("MIME-Version: 1.0");
        await tlsWriter.WriteLineAsync("Content-Type: text/html; charset=utf-8");
        await tlsWriter.WriteLineAsync();
        await tlsWriter.WriteLineAsync(htmlBody.Replace("\r\n.", "\r\n.."));
        await tlsWriter.WriteLineAsync(".");
        await ExpectAsync(tlsReader, 250);
        await CommandAsync(tlsWriter, tlsReader, "QUIT", 221);
    }

    private static async Task CommandAsync(StreamWriter writer, StreamReader reader, string command, int expectedCode)
    {
        await writer.WriteLineAsync(command);
        await ExpectAsync(reader, expectedCode);
    }

    private static async Task ExpectAsync(StreamReader reader, int expectedCode)
    {
        string? line;
        do
        {
            line = await reader.ReadLineAsync();
            if (line is null || line.Length < 3 || !int.TryParse(line[..3], out int actual) || actual != expectedCode)
                throw new SmtpException($"Máy chủ SMTP trả về không hợp lệ: {line ?? "(đã đóng kết nối)"}");
        } while (line.Length > 3 && line[3] == '-');
    }
}
