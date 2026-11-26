namespace Chatting.Client.Services;

/// <summary>Converts timestamps stored by the server in UTC to Vietnam time (UTC+7).</summary>
public static class VietnamTime
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public static DateTime FromUtc(DateTime timestamp)
    {
        // SQL Server's datetime2 does not retain DateTime.Kind. Server timestamps
        // are written with DateTime.UtcNow, so an unspecified value is UTC here.
        DateTime utc = timestamp.Kind == DateTimeKind.Utc
            ? timestamp
            : DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);
    }
}
