namespace Readier.Helpers;

public static class AppClock
{
    private static readonly TimeZoneInfo AppTimeZone = ResolveTimeZone();

    public static DateTime Now
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AppTimeZone);

    public static DateTime Today => Now.Date;

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
        }
    }
}
