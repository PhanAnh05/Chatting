using System.Security.Cryptography;
using Chatting.Server.Data;
using Chatting.Server.Models;
using Chatting.Server.Services;
using Chatting.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chatting.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly EmailService _emailService;
    private readonly JwtTokenService _jwtService;
    private readonly IConfiguration _config;

    public AuthController(
        ChatDbContext db,
        PasswordHasher hasher,
        EmailService emailService,
        JwtTokenService jwtService,
        IConfiguration config)
    {
        _db = db;
        _hasher = hasher;
        _emailService = emailService;
        _jwtService = jwtService;
        _config = config;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/auth/ping
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(ApiResponse.Ok("Pong"));

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/auth/check-username?username=...
    // ─────────────────────────────────────────────────────────────────────────
    [HttpGet("check-username")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> CheckUsername([FromQuery] string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest(ApiResponse<bool>.Fail("Vui lòng cung cấp tài khoản để kiểm tra."));

        string u = username.Trim().ToLowerInvariant();
        if (u.Length < 3 || u.Length > 50)
            return Ok(ApiResponse<bool>.Ok(false, "Tài khoản phải có từ 3 đến 50 ký tự."));

        if (u.Contains(' '))
            return Ok(ApiResponse<bool>.Ok(false, "Tài khoản không được chứa khoảng trắng (dấu cách)."));

        bool exists = await _db.Users.AnyAsync(user => user.Username.ToLower() == u);
        if (exists)
            return Ok(ApiResponse<bool>.Ok(false, "Tài khoản đăng nhập này đã được sử dụng."));

        return Ok(ApiResponse<bool>.Ok(true, "Tài khoản hợp lệ và có thể đăng ký."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/register
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(ApiResponse.Fail("Tài khoản đăng nhập, email và mật khẩu không được để trống."));
        }

        string username = dto.Username.Trim();
        string displayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? username : dto.DisplayName.Trim();
        string email = dto.Email.Trim().ToLowerInvariant();

        // Check username rules
        if (username.Length < 3 || username.Length > 50)
            return BadRequest(ApiResponse.Fail("Tài khoản đăng nhập phải từ 3 đến 50 ký tự."));

        if (username.Contains(' '))
            return BadRequest(ApiResponse.Fail("Tài khoản đăng nhập không được chứa khoảng trắng."));

        // Check email format
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
            return BadRequest(ApiResponse.Fail("Định dạng email không hợp lệ."));

        // Check duplicate Username
        bool usernameExists = await _db.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
        if (usernameExists)
            return Conflict(ApiResponse.Fail("Tài khoản đăng nhập này đã được sử dụng. Vui lòng chọn tài khoản khác.", "USERNAME_EXISTS"));

        // Check duplicate Email
        bool emailExists = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailExists)
            return Conflict(ApiResponse.Fail("Email này đã được đăng ký.", "EMAIL_EXISTS"));

        // Create user (not verified yet)
        User user = new()
        {
            Id = Guid.NewGuid(),
            Username = username,
            DisplayName = displayName,
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password),
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);

        // Generate & store OTP
        string otpPlain = GenerateOtp();
        OtpCode otpRecord = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = PasswordHasher.Sha256(otpPlain),
            Purpose = "REGISTER",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            LastSentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _db.OtpCodes.Add(otpRecord);

        await _db.SaveChangesAsync();

        // Send email (fire-and-forget style; OTP always logs to console)
        if (!await _emailService.SendOtpEmailAsync(email, user.DisplayName ?? user.Username, otpPlain))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse.Fail("Không thể gửi email OTP. Vui lòng thử lại sau."));

        return Ok(ApiResponse.Ok("Đăng ký thành công! Kiểm tra email để lấy mã OTP."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/verify-otp
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();

        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return NotFound(ApiResponse.Fail("Email không tồn tại."));

        if (user.IsEmailVerified)
            return Ok(ApiResponse.Ok("Email đã được xác minh trước đó."));

        OtpCode? otp = await _db.OtpCodes
            .Where(o => o.UserId == user.Id && o.Purpose == "REGISTER" && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null)
            return BadRequest(ApiResponse.Fail("Không tìm thấy mã OTP. Hãy gửi lại.", "OTP_NOT_FOUND"));

        if (otp.AttemptCount >= otp.MaxAttempts)
            return BadRequest(ApiResponse.Fail("Mã OTP đã bị khóa do nhập sai quá nhiều lần. Hãy gửi lại mã mới.", "OTP_LOCKED"));

        if (DateTime.UtcNow > otp.ExpiresAt)
            return BadRequest(ApiResponse.Fail("Mã OTP đã hết hạn. Hãy gửi lại.", "OTP_EXPIRED"));

        string inputHash = PasswordHasher.Sha256(dto.OtpCode.Trim());

        if (!string.Equals(inputHash, otp.CodeHash, StringComparison.OrdinalIgnoreCase))
        {
            otp.AttemptCount++;
            await _db.SaveChangesAsync();
            int remaining = otp.MaxAttempts - otp.AttemptCount;
            return BadRequest(ApiResponse.Fail($"Mã OTP sai. Còn {remaining} lần thử.", "OTP_WRONG"));
        }

        // Correct
        otp.IsUsed = true;
        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Xác minh email thành công! Bạn có thể đăng nhập."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/resend-otp
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("resend-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResendOtp([FromBody] ResendOtpRequestDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();

        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return NotFound(ApiResponse.Fail("Email không tồn tại."));

        if (user.IsEmailVerified)
            return BadRequest(ApiResponse.Fail("Email này đã được xác minh rồi."));

        OtpCode? lastOtp = await _db.OtpCodes
            .Where(o => o.UserId == user.Id && o.Purpose == "REGISTER" && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastOtp is not null)
        {
            if (lastOtp.ResendCount >= lastOtp.MaxResends)
                return BadRequest(ApiResponse.Fail("Đã gửi lại OTP quá nhiều lần (tối đa 3 lần). Vui lòng liên hệ quản trị viên.", "RESEND_LIMIT"));

            if (lastOtp.LastSentAt.HasValue &&
                (DateTime.UtcNow - lastOtp.LastSentAt.Value).TotalSeconds < 60)
            {
                int remaining = 60 - (int)(DateTime.UtcNow - lastOtp.LastSentAt.Value).TotalSeconds;
                return BadRequest(ApiResponse.Fail($"Vui lòng chờ {remaining} giây trước khi gửi lại.", "RESEND_COOLDOWN"));
            }

            lastOtp.IsUsed = true; // invalidate old OTP
        }

        // Create new OTP
        string otpPlain = GenerateOtp();
        OtpCode newOtp = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CodeHash = PasswordHasher.Sha256(otpPlain),
            Purpose = "REGISTER",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ResendCount = (lastOtp?.ResendCount ?? 0) + 1,
            LastSentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        _db.OtpCodes.Add(newOtp);
        await _db.SaveChangesAsync();

        if (!await _emailService.SendOtpEmailAsync(email, user.Username, otpPlain))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse.Fail("Không thể gửi email OTP. Vui lòng thử lại sau."));

        return Ok(ApiResponse.Ok("Đã gửi lại mã OTP."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/forgot-password — Send a one-time password reset code
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
            return BadRequest(ApiResponse.Fail("Vui lòng nhập email hợp lệ."));

        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive && u.IsEmailVerified);
        // Do not disclose whether an account exists.
        if (user is null)
            return Ok(ApiResponse.Ok("Nếu email thuộc một tài khoản đã xác minh, mã đặt lại mật khẩu đã được gửi."));

        OtpCode? lastOtp = await _db.OtpCodes
            .Where(o => o.UserId == user.Id && o.Purpose == "RESET_PASSWORD" && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastOtp?.LastSentAt is DateTime lastSent && (DateTime.UtcNow - lastSent).TotalSeconds < 60)
            return BadRequest(ApiResponse.Fail("Vui lòng chờ 60 giây trước khi yêu cầu mã mới.", "RESET_COOLDOWN"));
        if (lastOtp is not null && lastOtp.ResendCount >= lastOtp.MaxResends)
            return BadRequest(ApiResponse.Fail("Đã yêu cầu mã quá nhiều lần. Vui lòng thử lại sau.", "RESET_LIMIT"));
        if (lastOtp is not null) lastOtp.IsUsed = true;

        string otpPlain = GenerateOtp();
        _db.OtpCodes.Add(new OtpCode
        {
            Id = Guid.NewGuid(), UserId = user.Id, CodeHash = PasswordHasher.Sha256(otpPlain),
            Purpose = "RESET_PASSWORD", ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            ResendCount = (lastOtp?.ResendCount ?? 0) + 1,
            LastSentAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        if (!await _emailService.SendOtpEmailAsync(email, user.DisplayName ?? user.Username, otpPlain, "Đặt lại mật khẩu"))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse.Fail("Không thể gửi email OTP. Vui lòng thử lại sau."));
        return Ok(ApiResponse.Ok("Nếu email thuộc một tài khoản đã xác minh, mã đặt lại mật khẩu đã được gửi."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/reset-password — Validate OTP and set a new password
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();
        if (dto.NewPassword.Length < 6)
            return BadRequest(ApiResponse.Fail("Mật khẩu mới phải có ít nhất 6 ký tự."));

        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user is null) return BadRequest(ApiResponse.Fail("Mã đặt lại không hợp lệ."));

        OtpCode? otp = await _db.OtpCodes
            .Where(o => o.UserId == user.Id && o.Purpose == "RESET_PASSWORD" && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt).FirstOrDefaultAsync();
        if (otp is null || DateTime.UtcNow > otp.ExpiresAt)
            return BadRequest(ApiResponse.Fail("Mã đặt lại không hợp lệ hoặc đã hết hạn.", "OTP_EXPIRED"));
        if (otp.AttemptCount >= otp.MaxAttempts)
            return BadRequest(ApiResponse.Fail("Mã đã bị khóa do nhập sai quá nhiều lần.", "OTP_LOCKED"));
        if (!string.Equals(PasswordHasher.Sha256(dto.OtpCode.Trim()), otp.CodeHash, StringComparison.OrdinalIgnoreCase))
        {
            otp.AttemptCount++;
            await _db.SaveChangesAsync();
            return BadRequest(ApiResponse.Fail("Mã OTP không đúng.", "OTP_WRONG"));
        }

        otp.IsUsed = true;
        user.PasswordHash = _hasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        List<RefreshToken> activeTokens = await _db.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ToListAsync();
        foreach (RefreshToken token in activeTokens) { token.IsRevoked = true; token.RevokedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/login
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto dto)
    {
        string account = (dto.Username ?? dto.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(account))
            return BadRequest(ApiResponse<LoginResponseDto>.Fail("Vui lòng nhập tài khoản đăng nhập."));

        string accountLower = account.ToLowerInvariant();

        // Support authentication by Username OR Email (cho phép đăng nhập bằng Tài khoản hoặc Email)
        User? user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower() == accountLower || u.Email.ToLower() == accountLower);

        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Tài khoản hoặc mật khẩu không đúng."));

        if (!user.IsEmailVerified)
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Email chưa được xác minh.", "UNVERIFIED_EMAIL"));

        if (!user.IsActive)
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Tài khoản đã bị vô hiệu hóa.", "ACCOUNT_DISABLED"));

        string accessToken = _jwtService.GenerateAccessToken(user);
        (string refreshPlain, string refreshHash) = _jwtService.GenerateRefreshToken();

        int refreshDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");
        RefreshToken refreshRecord = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _db.RefreshTokens.Add(refreshRecord);

        user.Status = Chatting.Shared.Enums.UserStatus.Online;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshPlain,
            User = new UserSessionDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName ?? user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio
            }
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/refresh
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        string hash = PasswordHasher.Sha256(dto.RefreshToken);

        RefreshToken? stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Refresh token không hợp lệ hoặc đã hết hạn.", "INVALID_REFRESH_TOKEN"));

        if (!stored.User.IsActive)
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail("Tài khoản đã bị vô hiệu hóa.", "ACCOUNT_DISABLED"));

        // Rotate refresh token
        stored.IsRevoked = true;
        stored.RevokedAt = DateTime.UtcNow;

        string newAccess = _jwtService.GenerateAccessToken(stored.User);
        (string newRefreshPlain, string newRefreshHash) = _jwtService.GenerateRefreshToken();

        int refreshDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");
        RefreshToken newRefresh = new()
        {
            Id = Guid.NewGuid(),
            UserId = stored.UserId,
            TokenHash = newRefreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        };
        _db.RefreshTokens.Add(newRefresh);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            AccessToken = newAccess,
            RefreshToken = newRefreshPlain,
            User = new UserSessionDto
            {
                Id = stored.User.Id,
                Username = stored.User.Username,
                DisplayName = stored.User.DisplayName ?? stored.User.Username,
                Email = stored.User.Email,
                AvatarUrl = stored.User.AvatarUrl,
                Bio = stored.User.Bio
            }
        }));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/auth/logout
    // ─────────────────────────────────────────────────────────────────────────
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutRequestDto dto)
    {
        string hash = PasswordHasher.Sha256(dto.RefreshToken);
        RefreshToken? stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);
        if (stored is not null && !stored.IsRevoked)
        {
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;

            // Set user offline
            User? user = await _db.Users.FindAsync(stored.UserId);
            if (user is not null)
            {
                user.Status = Chatting.Shared.Enums.UserStatus.Offline;
                user.LastSeenAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse.Ok("Đăng xuất thành công."));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(100_000, 999_999).ToString();
    }
}
