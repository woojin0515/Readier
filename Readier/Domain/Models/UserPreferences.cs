namespace Readier.Models;

public class UserPreferences
{
    public bool NotificationsEnabled { get; set; } = true;

    public NotificationPreferences Notification { get; set; } = new();

    public PreparationProfile PreparationProfile { get; set; } = new();
}

public class NotificationPreferences
{
    public int LeaveSoonReminderMinutes { get; set; } = 10;

    public int SnoozePresetMinutes { get; set; } = 10;

    public bool UseCalmReminderCopy { get; set; } = true;
}
