# Chatting

Đồ án lập trình mạng: ứng dụng nhắn tin desktop Windows theo mô hình client–server, hỗ trợ chat thời gian thực, bạn bè, nhóm, gửi file/voice, gọi audio/video và hội thoại AI.

## Công nghệ và cấu trúc

- `Chatting.Client`: C# WinForms, SignalR Client, WebView2/WebRTC và NAudio.
- `Chatting.Server`: ASP.NET Core Web API, SignalR, Entity Framework Core và SQL Server.
- `Chatting.Shared`: DTO và enum dùng chung.
- `Chatting.slnx`: solution chính, target .NET 10.
- `Chatting/`: project WinForms cũ được giữ theo bản source gốc; không nằm trong solution chính.

## Chạy trên Windows

1. Cài .NET 10 SDK và SQL Server; dùng Visual Studio có hỗ trợ .NET 10 nếu mở solution bằng IDE.
2. Mở PowerShell tại thư mục repository. Đặt cấu hình cho terminal chạy server:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=ChattingDB;Trusted_Connection=True;TrustServerCertificate=True;'
$env:Jwt__Secret = [Guid]::NewGuid().ToString('N') + [Guid]::NewGuid().ToString('N')
$env:Email__From = 'your-email@example.com'
$env:Email__Username = 'your-email@example.com'
$env:Email__Password = 'YOUR_SMTP_APP_PASSWORD'
# Tùy chọn nếu dùng chat AI:
$env:AiSettings__OpenAiApiKey = 'YOUR_AI_API_KEY'
dotnet run --project Chatting.Server/Chatting.Server.csproj
```

Giữ JWT secret ổn định giữa các lần chạy nếu muốn duy trì token đăng nhập. Thay thông tin SQL Server và SMTP bằng cấu hình của bạn. Mặc định server tự áp dụng EF migrations khi khởi động; tài khoản database cần đủ quyền tạo/cập nhật database. Cấu hình AI còn phụ thuộc endpoint, provider và model trong `Chatting.Server/appsettings.json`.

3. Mở terminal thứ hai để chạy client:

```powershell
dotnet run --project Chatting.Client/Chatting.Client.csproj
```

Client mặc định dùng `http://127.0.0.1:5000`. Khi chạy trên máy khác trong LAN, đổi `ServerUrl` trong `Chatting.Client/appsettings.client.json` thành IP LAN của máy chủ và cấu hình firewall phù hợp. SignalR Hub nằm tại `/hubs/chat`.

## Phạm vi bản đưa lên GitHub

Đây là bản source Chatting desktop cũ, chưa chuyển sang Flutter. Source, assets và migrations được giữ lại. Khóa API, mật khẩu SMTP, JWT secret, phiên đăng nhập đã lưu, file upload cá nhân và các thư mục build/publish không được đưa lên.

Luồng gọi dùng SignalR để chuyển SDP/ICE, WebRTC để truyền media. Bản gốc có STUN; gọi qua các mạng khác nhau cần bổ sung TURN khi không thể kết nối trực tiếp. Cấu hình HTTP/LAN hiện tại cần được điều chỉnh trước khi triển khai công khai.
