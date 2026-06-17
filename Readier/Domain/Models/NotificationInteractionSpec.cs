namespace Readier.Models;

public enum NotificationActionType
{
    OpenApp,
    Snooze,
    Dismiss
}

public sealed record NotificationActionSpec(NotificationActionType Type, string Label, int? Minutes = null);

public static class NotificationInteractionSpec
{
    public static IReadOnlyList<int> LeaveSoonOptions { get; } = new[] { 0, 5, 10, 15, 20, 30 };

    public static IReadOnlyList<int> SnoozePresetOptions { get; } = new[] { 5, 10, 15, 20, 30 };

    public static IReadOnlyList<NotificationActionSpec> DefaultActions { get; } = new[]
    {
        new NotificationActionSpec(NotificationActionType.OpenApp, "앱 열기"),
        new NotificationActionSpec(NotificationActionType.Snooze, "10분 미루기", 10),
        new NotificationActionSpec(NotificationActionType.Dismiss, "닫기")
    };
}
