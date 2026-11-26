using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Chatting.Server.Models;
using Microsoft.IdentityModel.Tokens;

namespace Chatting.Server.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        string secret = _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        string issuer = _config["Jwt:Issuer"] ?? "ChattingServer";
        string audience = _config["Jwt:Audience"] ?? "ChattingClient";
        int expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secret));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically random 64-byte Refresh Token (URL-safe Base64).
    /// Returns (plainText, hash) — store only the hash.
    /// </summary>
    public (string PlainText, string Hash) GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        string plainText = Convert.ToBase64String(bytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        string hash = PasswordHasher.Sha256(plainText);
        return (plainText, hash);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        string secret = _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        string issuer = _config["Jwt:Issuer"] ?? "ChattingServer";
        string audience = _config["Jwt:Audience"] ?? "ChattingClient";

        TokenValidationParameters parameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = false, // We check expiry manually for refresh flow
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out SecurityToken secToken);

            if (secToken is JwtSecurityToken jwt &&
                jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            {
                return principal;
            }
        }
        catch { /* invalid token */ }
        return null;
    }

    public Guid? GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        string? sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out Guid id) ? id : null;
    }
}
