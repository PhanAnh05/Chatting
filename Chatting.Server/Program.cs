using System.Net;
using System.Text;
using Chatting.Server.Data;
using Chatting.Server.Hubs;
using Chatting.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ─── Kestrel: Listen on all interfaces ────────────────────────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5000);
});

// ─── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ─── Application Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<AiAssistantService>();
builder.Services.AddSingleton<PresenceTracker>();

// ─── JWT Authentication ───────────────────────────────────────────────────────
string jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChattingServer";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ChattingClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    // Allow JWT token from query string for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            PathString path = context.HttpContext.Request.Path;
            string? accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/chat")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ─── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─── SignalR ──────────────────────────────────────────────────────────────────
builder.Services.AddSignalR(opts =>
{
    opts.EnableDetailedErrors = true;
    opts.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});

// ─── CORS (allow all for LAN dev) ─────────────────────────────────────────────
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("LanPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─── Static Files for uploaded content ────────────────────────────────────────
builder.Services.AddDirectoryBrowser();

WebApplication app = builder.Build();

// ─── Auto-apply migrations on startup ────────────────────────────────────────
using (IServiceScope scope = app.Services.CreateScope())
{
    ChatDbContext db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".webp"] = "image/webp";
provider.Mappings[".ogg"] = "audio/ogg";
provider.Mappings[".wav"] = "audio/wav";
provider.Mappings[".mp3"] = "audio/mpeg";
provider.Mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
provider.Mappings[".doc"] = "application/msword";
provider.Mappings[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
provider.Mappings[".xls"] = "application/vnd.ms-excel";
provider.Mappings[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
provider.Mappings[".ppt"] = "application/vnd.ms-powerpoint";
provider.Mappings[".pdf"] = "application/pdf";
provider.Mappings[".txt"] = "text/plain";
provider.Mappings[".zip"] = "application/zip";
provider.Mappings[".rar"] = "application/x-rar-compressed";
provider.Mappings[".7z"] = "application/x-7z-compressed";

app.UseCors("LanPolicy");
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔═════════════════════════════════════════════╗");
Console.WriteLine("║     CHATTING SERVER — http://0.0.0.0:5000═══╝");
Console.ResetColor();

app.Run();
