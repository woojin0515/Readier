namespace Readier.Models;

public class UserPreferences
{
    public bool NotificationsEnabled { get; set; } = true;

    public PreparationProfile PreparationProfile { get; set; } = new();
}
